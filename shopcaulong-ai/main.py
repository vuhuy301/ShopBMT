# main.py – PHIÊN BẢN HOÀN CHỈNH CUỐI CÙNG (Shop Cầu Lông AI Linh)
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
import uvicorn
import os
from pathlib import Path
import uuid
import json
from functools import lru_cache

from langchain_huggingface import HuggingFaceEmbeddings
from langchain_community.vectorstores import FAISS
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.documents import Document
from langchain_core.runnables import RunnableParallel
from langchain_core.output_parsers import StrOutputParser
from langchain_core.messages import HumanMessage, AIMessage
from langchain_community.chat_message_histories import ChatMessageHistory
from langchain_openai import ChatOpenAI

# ==========================================================
# APP + CORS
# ==========================================================
app = FastAPI(title="Shop Cầu Lông AI – Linh Nhớ Lâu, Siêu Thân Thiện")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================================================
# CONFIG
# ==========================================================
DEEPSEEK_API_KEY = os.getenv("DEEPSEEK_API_KEY", "sk-72fc18a871824e58a910a499f281512c")
DB_FOLDER = Path("faiss_shopcaulong")
DB_FOLDER.mkdir(exist_ok=True)
HISTORY_FILE = Path("chat_history.json")  # Lưu lịch sử chat vĩnh viễn

vectorstore: FAISS | None = None
qa_chain = None

# Lưu tất cả lịch sử chat: user_123, user_456, hoặc uuid cho khách vãng lai
store: Dict[str, ChatMessageHistory] = {}

# ==========================================================
# LƯU & LOAD LỊCH SỬ CHAT
# ==========================================================
def load_history():
    global store
    if HISTORY_FILE.exists():
        try:
            data = json.loads(HISTORY_FILE.read_text(encoding="utf-8"))
            for session_id, messages in data.items():
                history = ChatMessageHistory()
                for msg in messages:
                    if msg["type"] == "human":
                        history.add_message(HumanMessage(content=msg["content"]))
                    else:
                        history.add_message(AIMessage(content=msg["content"]))
                store[session_id] = history
            print(f"Đã khôi phục lịch sử của {len(store)} phiên chat")
        except Exception as e:
            print("Lỗi load lịch sử chat:", e)

def save_history():
    data = {}
    for session_id, history in store.items():
        data[session_id] = [
            {"type": "human" if isinstance(m, HumanMessage) else "ai", "content": m.content}
            for m in history.messages
        ]
    try:
        HISTORY_FILE.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    except Exception as e:
        print("Lỗi lưu lịch sử chat:", e)

def get_session_history(session_id: str) -> ChatMessageHistory:
    if session_id not in store:
        store[session_id] = ChatMessageHistory()
    return store[session_id]

# ==========================================================
# EMBEDDING & LLM
# ==========================================================
@lru_cache()
def get_embedding():
    print("Khởi tạo embedding multilingual-e5-small...")
    return HuggingFaceEmbeddings(
        model_name="intfloat/multilingual-e5-small",
        model_kwargs={"device": "cpu"},
        encode_kwargs={"normalize_embeddings": True},
    )

@lru_cache()
def get_llm():
    print("Khởi tạo DeepSeek LLM...")
    return ChatOpenAI(
        model="deepseek-chat",
        temperature=0.3,
        max_tokens=600,
        api_key=DEEPSEEK_API_KEY,
        base_url="https://api.deepseek.com",
        timeout=20,
        max_retries=2,
    )

# ==========================================================
# DATA MODELS
# ==========================================================
class SizeVariant(BaseModel):
    size: str
    stock: int

class ColorVariant(BaseModel):
    color: str
    imageUrls: List[str] = []
    sizes: List[SizeVariant]

class Product(BaseModel):
    id: int
    name: str
    description: str = ""
    price: float
    discountPrice: Optional[float] = None
    stock: int
    brandName: str
    categoryName: str
    isFeatured: bool = False
    details: List[Dict[str, Any]] = []
    colorVariants: List[ColorVariant] = []

# ==========================================================
# CHUNKING
# ==========================================================
# THAY THẾ HÀM create_product_documents bằng cái này
def create_product_documents(product: dict) -> List[Document]:
    price = product.get("discountPrice") or product["price"]
    name = product["name"]
    brand = product["brandName"]
    description = product.get("description", "")
    category = product.get("categoryName", "")
    featured = "hot" if product.get("isFeatured", False) else ""
    
    docs = []
    
    # CHUNK 1: THÔNG TIN TỔNG QUÁT (1 DUY NHẤT)
    full_info = f"{name} {brand} {category} {featured} giá {price:,.0f}đ. {description}"
    docs.append(
        Document(
            page_content=full_info,
            metadata={
                "product_id": product["id"], 
                "type": "full_info",
                "name": name,
                "brand": brand,
                "price": price,
                "discount_price": product.get("discountPrice"),
                "description": description
            }
        )
    )
    
    # CHUNK 2: TỪNG MÀU (GỘP TẤT CẢ SIZE)
    for cv in product.get("colorVariants", []):
        color = cv.get("color", "không màu")
        available_sizes = [sz["size"] for sz in cv.get("sizes", []) if sz["stock"] > 0]
        if available_sizes:
            sizes_text = ", ".join(available_sizes)
            color_variant = f"{name} màu {color} có size {sizes_text} giá {price:,.0f}đ còn hàng"
            docs.append(
                Document(
                    page_content=color_variant,
                    metadata={
                        "product_id": product["id"],
                        "type": "color_variant",
                        "color": color,
                        "available_sizes": available_sizes,
                        "price": price
                    }
                )
            )
    
    return docs

def delete_product_chunks(product_id: int) -> int:
    if not vectorstore or vectorstore.index.ntotal == 0:
        return 0
    ids_to_delete = []
    for i in range(vectorstore.index.ntotal):
        doc_id = vectorstore.index_to_docstore_id[i]
        doc = vectorstore.docstore.search(doc_id)
        if isinstance(doc, Document) and doc.metadata.get("product_id") == product_id:
            ids_to_delete.append(doc_id)
    if ids_to_delete:
        vectorstore.delete(ids_to_delete)
    return len(ids_to_delete)

# ==========================================================
# QA CHAIN
# ==========================================================
def build_qa_chain():
    global qa_chain
    if not vectorstore or vectorstore.index.ntotal == 0:
        qa_chain = None
        return
    retriever = vectorstore.as_retriever(search_kwargs={"k": 10, "fetch_k": 40})
    template = """
        Bạn là Linh – nhân viên bán hàng siêu dễ thương, nhanh nhẹn của Shop Cầu Lông Pro.

        Lịch sử trò chuyện:
        {history}

        Dữ liệu sản phẩm hiện có:
        {context}

        Khách hỏi:
        {question}

        QUY TẮC BẮT BUỘC:
        - Chỉ trả lời dựa trên dữ liệu trên, không bịa đặt
        - **GỘP MÀU/SIZE CỦA CÙNG MỘT SẢN PHẨM – KHÔNG LẶP**
        - Ví dụ: "Yonex 88D Pro giá 3.4tr có màu Đỏ (3U, 4U), Xanh (3U) còn hàng"
        - Gợi ý tối đa 3 sản phẩm phù hợp nhất
        - Nếu chưa rõ → hỏi lại nhẹ nhàng
        - Ngắn gọn, thân thiện, emoji vừa phải
        """

    prompt = ChatPromptTemplate.from_template(template)
    qa_chain = (
        RunnableParallel({
            "context": lambda x: "\n".join([d.page_content for d in retriever.invoke(x["question"])]),
            "question": lambda x: x["question"],
            "history": lambda x: x.get("history", "Chưa có lịch sử"),
        })
        | prompt
        | get_llm()
        | StrOutputParser()
    )

# ==========================================================
# ROUTES
# ==========================================================
@app.get("/")
async def root():
    total = vectorstore.index.ntotal if vectorstore else 0
    return {
        "message": "Linh đang online và nhớ hết khách rồi nè!",
        "chunks": total,
        "active_sessions": len(store)
    }
# THÊM NGAY SAU ROUTE /reindex_all HOẶC CUỐI FILE TRƯỚC STARTUP

@app.post("/debug_chunks")
async def debug_chunks_post(
    limit: int = 50,
    product_id: Optional[int] = None,
    include_metadata: bool = True
):
    """
    Xem tất cả chunk hiện có trong FAISS (dùng POST)
    - limit: số lượng chunk tối đa trả về (mặc định 50)
    - product_id: lọc theo sản phẩm (nếu có)
    - include_metadata: có hiện metadata không
    """
    if not vectorstore or vectorstore.index.ntotal == 0:
        return {
            "total_chunks_in_db": 0,
            "returned_chunks": 0,
            "chunks": [],
            "message": "Chưa có dữ liệu nào được chunk"
        }

    total = vectorstore.index.ntotal
    results = []

    for i in range(min(limit, total)):
        doc_id = vectorstore.index_to_docstore_id[i]
        doc = vectorstore.docstore.search(doc_id)
        if isinstance(doc, Document):
            item = {
                "index": i,
                "content": doc.page_content
            }
            if include_metadata:
                item["metadata"] = doc.metadata

            # Lọc theo product_id nếu có
            if product_id is not None:
                if doc.metadata.get("product_id") != product_id:
                    continue

            results.append(item)

    return {
        "total_chunks_in_db": total,
        "returned_chunks": len(results),
        "chunks": results,
        "message": f"Đã chunk {total} mẩu dữ liệu từ sản phẩm"
    }
@app.post("/add_product")
async def add_product(product: Product):
    global vectorstore
    delete_product_chunks(product.id)
    docs = create_product_documents(product.dict())
    if vectorstore is None:
        vectorstore = FAISS.from_documents(docs, get_embedding())
    else:
        vectorstore.add_documents(docs)
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    return {"status": "OK", "message": f"Đã thêm {product.name}"}

@app.post("/update_product")
async def update_product(product: Product):
    global vectorstore
    print(f"[AI] Nhận yêu cầu update sản phẩm ID: {product.id} - {product.name}")  # ← THÊM DÒNG NÀY

    delete_product_chunks(product.id)
    docs = create_product_documents(product.dict())
    if vectorstore is None:
        vectorstore = FAISS.from_documents(docs, get_embedding())
    else:
        vectorstore.add_documents(docs)
    
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    
    print(f"[AI] Đã chunk {len(docs)} mẩu cho sản phẩm {product.name}")  # ← THÊM DÒNG NÀY
    
    return {"status": "OK", "message": f"Đã cập nhật {product.name}"}

@app.post("/delete_product")
async def delete_product(product_id: int):
    deleted = delete_product_chunks(product_id)
    if deleted == 0:
        raise HTTPException(404, "Không tìm thấy sản phẩm")
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    return {"status": "OK", "deleted_chunks": deleted}

@app.post("/reindex_all")
async def rebuild(products: List[Product]):
    global vectorstore
    all_docs = [doc for p in products for doc in create_product_documents(p.dict())]
    vectorstore = FAISS.from_documents(all_docs, get_embedding())
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    return {"status": "OK", "chunks": len(all_docs)}

# ==================== CHAT PAYLOAD ====================
class ChatPayload(BaseModel):
    question: str
    session_id: Optional[str] = None   # cho khách vãng lai
    user_id: Optional[str] = None     # cho người đã đăng nhập

@app.post("/chat")
async def chat(payload: ChatPayload):
    if not qa_chain:
        raise HTTPException(500, "Chưa có dữ liệu sản phẩm!")

    # Frontend đã gửi sẵn user_id hoặc session_id → tin tưởng luôn!
    if payload.user_id:
        session_id = f"user_{payload.user_id}"
    elif payload.session_id:
        session_id = payload.session_id
    else:
        session_id = str(uuid.uuid4())

    history = get_session_history(session_id)
    history_str = "\n".join(
        f"{'Khách' if isinstance(m, HumanMessage) else 'Linh'}: {m.content}"
        for m in history.messages[-10:]
    ) or "Chưa có lịch sử"

    answer = qa_chain.invoke({"question": payload.question, "history": history_str})

    history.add_message(HumanMessage(content=payload.question))
    history.add_message(AIMessage(content=answer))
    save_history()

    resp = {"answer": answer.strip()}
    if not payload.user_id and not payload.session_id:
        resp["session_id"] = session_id
    return resp

@app.get("/my_chat_history")
async def get_my_chat_history(user_id: str = None, session_id: str = None):
    # Frontend gửi user_id (đã đăng nhập) hoặc session_id (khách vãng lai)
    if user_id:
        sid = f"user_{user_id}"
    elif session_id:
        sid = session_id
    else:
        raise HTTPException(400, "Thiếu user_id hoặc session_id")

    history = get_session_history(sid)
    messages = []
    for msg in history.messages:
        role = "user" if isinstance(msg, HumanMessage) else "ai"
        messages.append({"role": role, "content": msg.content})

    return {"messages": messages}

# ==========================================================
# STARTUP & SHUTDOWN
# ==========================================================
@app.on_event("startup")
async def startup():
    global vectorstore
    # Load FAISS
    if (DB_FOLDER / "index.faiss").exists():
        try:
            vectorstore = FAISS.load_local(
                str(DB_FOLDER), get_embedding(), allow_dangerous_deserialization=True
            )
            print(f"Load FAISS thành công: {vectorstore.index.ntotal} chunks")
        except Exception as e:
            print("Load FAISS lỗi:", e)

    if vectorstore is None:
        vectorstore = FAISS.from_texts(["Shop đang khởi động..."], get_embedding())
        vectorstore.save_local(str(DB_FOLDER))

    load_history()
    build_qa_chain()
    print("Linh đã sẵn sàng – nhớ hết khách cũ, siêu chuyên nghiệp!")

@app.on_event("shutdown")
async def shutdown():
    save_history()
    print("Đã lưu toàn bộ lịch sử chat trước khi tắt server!")

# ==========================================================
# RUN
# ==========================================================
if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)