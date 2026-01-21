# main.py – PHIÊN BẢN HOÀN CHỈNH (sau khi fix lỗi startup + thứ tự prompt)
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
from langchain_openai import ChatOpenAI, OpenAIEmbeddings

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
qa_chain_fallback = None

store: Dict[str, ChatMessageHistory] = {}
product_doc_ids = {}

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
    openai_key = os.getenv("OPENAI_API_KEY")
    if openai_key:
        try:
            print("🔹 Đang sử dụng OpenAI Embeddings (text-embedding-3-small)...")
            return OpenAIEmbeddings(
                model="text-embedding-3-small",
                api_key=openai_key,
                max_retries=2,
            )
        except Exception as e:
            print("Lỗi OpenAI Embedding, chuyển qua HuggingFace:", e)
    print("🔹 Fallback sang HuggingFace Embedding (multilingual-e5-small)")
    return HuggingFaceEmbeddings(
        model_name="intfloat/multilingual-e5-small",
        model_kwargs={"device": "cpu"},
        encode_kwargs={"normalize_embeddings": True},
    )

@lru_cache()
def get_llm():
    print("🔹 Khởi tạo DeepSeek LLM...")
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
# TẠO DOCUMENTS TỪ PRODUCT
# ==========================================================
def create_product_documents(product: Product) -> List[Document]:
    price = product.discountPrice or product.price
    product_id = product.id
    product_url = f"http://localhost:3000/product/{product_id}"
    docs = []

    # Chunk tổng quan
    info_text = f"{product.name} {product.brandName} giá {price:,.0f}đ"
    if product.description:
        info_text += f". {product.description}"
    info_text += f" Xem chi tiết: {product_url}"
    docs.append(Document(
        page_content=info_text,
        metadata={
            "product_id": product_id,
            "type": "full_info",
            "url": product_url
        }
    ))

    # Chunk biến thể màu/size
    for cv in product.colorVariants or []:
        color = (cv.color or "không màu").strip()
        available_sizes = [s.size for s in (cv.sizes or []) if s.stock > 0]
        if available_sizes:
            sizes_str = ", ".join(available_sizes)
            variant_text = f"{product.name} màu {color} có size {sizes_str} giá {price:,.0f}đ còn hàng. Xem chi tiết: {product_url}"
            docs.append(Document(
                page_content=variant_text,
                metadata={
                    "product_id": product_id,
                    "type": "variant",
                    "color": color,
                    "url": product_url
                }
            ))
    return docs

# ==========================================================
# XÓA CHUNKS CỦA SẢN PHẨM
# ==========================================================
def delete_product_chunks(product_id: int) -> int:
    if not vectorstore or vectorstore.index.ntotal == 0:
        return 0
    if product_id in product_doc_ids:
        ids_to_delete = product_doc_ids[product_id]
        vectorstore.delete(ids_to_delete)
        del product_doc_ids[product_id]
        return len(ids_to_delete)

    # Fallback
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
# PROMPTS (định nghĩa trước khi build chain)
# ==========================================================
template_with_products = """
Bạn là Linh – nhân viên tư vấn của Shop Cầu Lông Pro.
DỮ LIỆU SẢN PHẨM TRONG KHO:
{context}
LỊCH SỬ HỘI THOẠI GẦN ĐÂY:
{history}
CÂU HỎI CỦA KHÁCH:
{question}
QUY TẮC TRẢ LỜI:
1. Chỉ dùng thông tin từ "DỮ LIỆU SẢN PHẨM TRONG KHO"
2. Bắt buộc dùng đúng link có trong dữ liệu: <a href="..." target="_blank">xem chi tiết</a>
3. Gợi ý tối đa 3 sản phẩm phù hợp nhất
4. Gộp màu/size của cùng sản phẩm
5. Giọng điệu thân thiện, tự nhiên, hạn chế emoji
6. Có thể đưa thêm 1-2 câu hỏi gợi ý để khách hỏi tiếp
TRẢ LỜI:
"""

# template_fallback = """
# Bạn là Linh – trợ lý AI thông minh của Shop Cầu Lông Pro.
# LỊCH SỬ HỘI THOẠI GẦN ĐÂY:
# {history}
# CÂU HỎI CỦA KHÁCH:
# {question}
# TÌNH HUỐNG: Không tìm thấy sản phẩm phù hợp trong kho dữ liệu.
# QUY TẮC TRẢ LỜI:
# 1. Bắt đầu bằng thông báo: "⚠️ *Linh chưa tìm thấy sản phẩm này trong gian hàng hiện tại.*"
# 2. SAU ĐÓ vẫn cố gắng trả lời câu hỏi dựa trên kiến thức chung về cầu lông
# 3. Nếu câu hỏi không liên quan đến cầu lông, vẫn trả lời nhưng thêm: "💡 *Thông tin này mang tính tham khảo, có thể không chính xác 100%*"
# 4. Giữ giọng điệu thân thiện, hữu ích
# 5. Gợi ý khách liên hệ shop hoặc mô tả rõ hơn nhu cầu
# VÍ DỤ:
# - Hỏi về sản phẩm không có: "⚠️ *Linh chưa tìm thấy vợt Yonex Astrox 99 Pro trong kho.* Tuy nhiên, dòng Astrox thường phù hợp với lối chơi tấn công mạnh..."
# - Hỏi ngoài lề: "Chào bạn! Về câu hỏi này thì... 💡 *Thông tin mang tính tham khảo, có thể không chính xác 100%*"
# TRẢ LỜI:
# """
template_fallback = """
Bạn là Linh – một cô gái trẻ trung, thân thiện, nhiệt tình, làm trợ lý cho Shop Cầu Lông Pro. 
Shop chuyên bán vợt, giày, cước, cầu, phụ kiện cầu lông chính hãng (Yonex, Victor, Lining...), nhưng Linh rất vui khi trò chuyện về bất kỳ chủ đề nào với khách hàng – từ thể thao khác, sản phẩm ngoài lề, đến chuyện đời thường, tình cảm, công việc...

LỊCH SỬ TRÒ CHUYỆN GẦN ĐÂY (đọc kỹ để hiểu ngữ cảnh và tone hiện tại):
{history}

CÂU HỎI HOẶC TIN NHẮN MỚI NHẤT CỦA KHÁCH:
{question}

TÌNH HUỐNG HIỆN TẠI: Không tìm thấy thông tin sản phẩm phù hợp trực tiếp từ kho cầu lông.

CÁCH TRẢ LỜI TỐT NHẤT:
- Đọc lịch sử để nắm tone cuộc trò chuyện (thân mật, nghiêm túc, vui vẻ, đang hỏi mua hay chỉ hỏi chơi...).
- Trả lời **tự nhiên như đang chat với bạn**, không cần bắt đầu bằng "Chào bạn" mỗi lần nếu lịch sử đã thân thiết.
- Trả lời **hữu ích, chân thực** dựa trên kiến thức chung. Nếu là sản phẩm môn khác (giày bóng đá, vợt tennis...), cứ gợi ý thoải mái dựa trên thông tin phổ biến, không cần bịa giá/link.
- Nếu nhắc đến shop, chỉ nói nhẹ nhàng khi thật sự liên quan (ví dụ: "Shop mình chuyên cầu lông nên không có cái này, nhưng nếu bạn cần... thì Linh tư vấn liền nhé!"). Đừng nhắc lặp lại nếu lịch sử đã nói rồi.
- Từ "bóng" → mặc định hiểu là **cầu** (shuttlecock) nếu đang nói về cầu lông, chỉ hiểu là bóng đá khi ngữ cảnh rõ ràng (sân cỏ, sút bóng, giày đá bóng...).
- Giữ giọng điệu vui vẻ, gần gũi, không dùng emoji trừ khi lịch sử có dùng nhiều.
- Kết thúc bằng câu hỏi hoặc gợi ý tự nhiên để tiếp tục cuộc trò chuyện (không ép buộc, tùy theo flow).

Hãy trả lời sao cho khách cảm thấy Linh đang thực sự lắng nghe và quan tâm đến họ nhé!
TRẢ LỜI:
"""

prompt_with_products = ChatPromptTemplate.from_template(template_with_products)
prompt_fallback = ChatPromptTemplate.from_template(template_fallback)

# ==========================================================
# BUILD QA CHAIN
# ==========================================================
def build_qa_chain():
    global qa_chain, qa_chain_fallback
    if not vectorstore or vectorstore.index.ntotal == 0:
        qa_chain = None
        qa_chain_fallback = None
        return

    # Retriever cơ bản với threshold
    retriever = vectorstore.as_retriever(
        search_type="similarity_score_threshold",
        search_kwargs={"k": 10, "score_threshold": 0.35}
    )

    # Chain chính (có sản phẩm)
    qa_chain = (
        RunnableParallel({
            "context": lambda x: "\n".join([d.page_content for d in retriever.invoke(x["question"])]),
            "question": lambda x: x["question"],
            "history": lambda x: x.get("history", "Chưa có lịch sử"),
        })
        | prompt_with_products
        | get_llm()
        | StrOutputParser()
    )

    # Chain fallback
    qa_chain_fallback = (
        RunnableParallel({
            "question": lambda x: x["question"],
            "history": lambda x: x.get("history", "Chưa có lịch sử"),
        })
        | prompt_fallback
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
    docs = create_product_documents(product)
    
    if vectorstore is None:
        vectorstore = FAISS.from_documents(docs, get_embedding())
    else:
        doc_ids = vectorstore.add_documents(docs)
        product_doc_ids[product.id] = doc_ids
    
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    
    return {"status": "OK", "message": f"Đã thêm {product.name}"}

@app.post("/update_product")
async def update_product(product: Product):
    global vectorstore
    
    print(f"[AI] Nhận yêu cầu update sản phẩm ID: {product.id} - {product.name}")
    
    delete_product_chunks(product.id)
    docs = create_product_documents(product)
    
    if vectorstore is None:
        vectorstore = FAISS.from_documents(docs, get_embedding())
    else:
        doc_ids = vectorstore.add_documents(docs)
        product_doc_ids[product.id] = doc_ids
    
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    
    print(f"[AI] Đã chunk {len(docs)} mẩu cho sản phẩm {product.name}")
    
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
    global vectorstore, product_doc_ids
    
    # Reset mapping mới
    product_doc_ids = {}
    
    # Gom toàn bộ tài liệu
    all_docs = []
    for p in products:
        docs = create_product_documents(p)
        all_docs.extend(docs)
        product_doc_ids[p.id] = []  # tạm tạo danh sách rỗng
    
    # Build FAISS từ đầu
    vectorstore = FAISS.from_documents(all_docs, get_embedding())
    
    # Sau khi build FAISS → doc_ids được sinh theo thứ tự
    current_index = 0
    for p in products:
        docs_count = len(create_product_documents(p))
        ids = []
        for i in range(docs_count):
            ids.append(vectorstore.index_to_docstore_id[current_index + i])
        product_doc_ids[p.id] = ids
        current_index += docs_count
    
    # Lưu database
    vectorstore.save_local(str(DB_FOLDER))
    build_qa_chain()
    
    return {"status": "OK", "chunks": len(all_docs)}

# ==================== CHAT PAYLOAD ====================
class ChatPayload(BaseModel):
    question: str
    session_id: Optional[str] = None
    user_id: Optional[str] = None

@app.post("/chat")
async def chat(payload: ChatPayload):
    if not qa_chain:
        raise HTTPException(500, "Chưa có dữ liệu sản phẩm!")

    # Xác định session_id
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

    # Query Classification
    classify_prompt = ChatPromptTemplate.from_template("""
        Phân loại câu hỏi: "{question}"
        - Nếu liên quan đến sản phẩm cầu lông (vợt, giày, cước, tư vấn mua hàng, so sánh sản phẩm): trả "relevant"
        - Nếu không liên quan (hỏi tình yêu, thời tiết, chủ đề khác): trả "irrelevant"
        Chỉ trả từ "relevant" hoặc "irrelevant", không giải thích.
    """)
    classify_chain = classify_prompt | get_llm() | StrOutputParser()
    classification = classify_chain.invoke({"question": payload.question}).strip().lower()

    if classification == "irrelevant":
        print("Query không liên quan → direct fallback")
        answer = qa_chain_fallback.invoke({"question": payload.question, "history": history_str})
    else:
        # Retrieve + kiểm tra context
        retriever = vectorstore.as_retriever(search_kwargs={"k": 10})
        try:
            retrieved_docs = vectorstore.as_retriever(
                search_type="similarity_score_threshold",
                search_kwargs={"k": 10, "score_threshold": 0.35}
            ).invoke(payload.question)
        except:
            retrieved_docs = retriever.invoke(payload.question)

        context_text = "\n".join([d.page_content for d in retrieved_docs])

        product_keywords = ["vợt", "giày", "cước", "yonex", "victor", "lining", "còn hàng"]
        has_products = len(retrieved_docs) > 0 and any(kw.lower() in context_text.lower() for kw in product_keywords)

        print(f"Số docs: {len(retrieved_docs)} | Có sản phẩm phù hợp: {has_products}")

        if has_products:
            answer = qa_chain.invoke({"question": payload.question, "history": history_str})
        else:
            answer = qa_chain_fallback.invoke({"question": payload.question, "history": history_str})

    # Lưu lịch sử
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

if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)