# # main.py – PHIÊN BẢN HOÀN CHỈNH (sau khi nâng cấp: query rewriting cho ngữ cảnh + cải thiện retrieval cho details + chỉnh prompt)
# from fastapi import FastAPI, HTTPException
# from fastapi.middleware.cors import CORSMiddleware
# from pydantic import BaseModel
# from typing import List, Optional, Dict, Any
# import uvicorn
# import os
# from pathlib import Path
# import uuid
# import json
# from functools import lru_cache
# from langchain_huggingface import HuggingFaceEmbeddings
# from langchain_community.vectorstores import FAISS
# from langchain_core.prompts import ChatPromptTemplate
# from langchain_core.documents import Document
# from langchain_core.runnables import RunnableParallel
# from langchain_core.output_parsers import StrOutputParser
# from langchain_core.messages import HumanMessage, AIMessage
# from langchain_community.chat_message_histories import ChatMessageHistory
# from langchain_openai import ChatOpenAI, OpenAIEmbeddings
# # ==========================================================
# # APP + CORS
# # ==========================================================
# app = FastAPI(title="Shop Cầu Lông AI – Linh Nhớ Lâu, Siêu Thân Thiện")
# app.add_middleware(
#     CORSMiddleware,
#     allow_origins=["http://localhost:3000"],
#     allow_credentials=True,
#     allow_methods=["*"],
#     allow_headers=["*"],
# )
# # ==========================================================
# # CONFIG
# # ==========================================================
# DEEPSEEK_API_KEY = os.getenv("DEEPSEEK_API_KEY", "sk-72fc18a871824e58a910a499f281512c")
# DB_FOLDER = Path("faiss_shopcaulong")
# DB_FOLDER.mkdir(exist_ok=True)
# HISTORY_FILE = Path("chat_history.json") # Lưu lịch sử chat vĩnh viễn
# vectorstore: FAISS | None = None
# qa_chain = None
# qa_chain_fallback = None
# store: Dict[str, ChatMessageHistory] = {}
# product_doc_ids = {}
# # ==========================================================
# # LƯU & LOAD LỊCH SỬ CHAT
# # ==========================================================
# def load_history():
#     global store
#     if HISTORY_FILE.exists():
#         try:
#             data = json.loads(HISTORY_FILE.read_text(encoding="utf-8"))
#             for session_id, messages in data.items():
#                 history = ChatMessageHistory()
#                 for msg in messages:
#                     if msg["type"] == "human":
#                         history.add_message(HumanMessage(content=msg["content"]))
#                     else:
#                         history.add_message(AIMessage(content=msg["content"]))
#                 store[session_id] = history
#             print(f"Đã khôi phục lịch sử của {len(store)} phiên chat")
#         except Exception as e:
#             print("Lỗi load lịch sử chat:", e)
# def save_history():
#     data = {}
#     for session_id, history in store.items():
#         data[session_id] = [
#             {"type": "human" if isinstance(m, HumanMessage) else "ai", "content": m.content}
#             for m in history.messages
#         ]
#     try:
#         HISTORY_FILE.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
#     except Exception as e:
#         print("Lỗi lưu lịch sử chat:", e)
# def get_session_history(session_id: str) -> ChatMessageHistory:
#     if session_id not in store:
#         store[session_id] = ChatMessageHistory()
#     return store[session_id]
# # ==========================================================
# # EMBEDDING & LLM
# # ==========================================================
# @lru_cache()
# def get_embedding():
#     openai_key = os.getenv("OPENAI_API_KEY")
#     if openai_key:
#         try:
#             print("🔹 Đang sử dụng OpenAI Embeddings (text-embedding-3-small)...")
#             return OpenAIEmbeddings(
#                 model="text-embedding-3-small",
#                 api_key=openai_key,
#                 max_retries=2,
#             )
#         except Exception as e:
#             print("Lỗi OpenAI Embedding, chuyển qua HuggingFace:", e)
#     print("🔹 Fallback sang HuggingFace Embedding (multilingual-e5-small)")
#     return HuggingFaceEmbeddings(
#         model_name="intfloat/multilingual-e5-small",
#         model_kwargs={"device": "cpu"},
#         encode_kwargs={"normalize_embeddings": True},
#     )
# @lru_cache()
# def get_llm():
#     print("🔹 Khởi tạo DeepSeek LLM...")
#     return ChatOpenAI(
#         model="deepseek-chat",
#         temperature=0.3,
#         max_tokens=600,
#         api_key=DEEPSEEK_API_KEY,
#         base_url="https://api.deepseek.com",
#         timeout=20,
#         max_retries=2,
#     )
# # ==========================================================
# # DATA MODELS
# # ==========================================================
# class SizeVariant(BaseModel):
#     size: str
#     stock: int
# class ColorVariant(BaseModel):
#     color: str
#     imageUrls: List[str] = []
#     sizes: List[SizeVariant]
# class Product(BaseModel):
#     id: int
#     name: str
#     description: str = ""
#     price: float
#     discountPrice: Optional[float] = None
#     stock: int
#     brandName: str
#     categoryName: str
#     isFeatured: bool = False
#     details: List[Dict[str, Any]] = []
#     colorVariants: List[ColorVariant] = []
# # ==========================================================
# def create_product_documents(product: Product) -> List[Document]:
#     price = product.discountPrice or product.price
#     product_id = product.id
#     product_url = f"http://localhost:3000/product/{product_id}"
#     docs = []
#     # ── 1. Chunk tổng quan (giữ nguyên nhưng tối ưu hơn chút)
#     info_text = f"{product.name} | {product.brandName} | {product.categoryName}"
#     info_text += f" | Giá: {price:,.0f}đ"
#     if product.discountPrice:
#         info_text += f" (giảm còn {product.discountPrice:,.0f}đ)"
#     if product.description:
#         info_text += f"\nMô tả ngắn: {product.description.strip()}"
#     info_text += f"\nChi tiết đầy đủ: {product_url}"
#     docs.append(Document(
#         page_content=info_text.strip(),
#         metadata={
#             "product_id": product_id,
#             "type": "overview",
#             "url": product_url
#         }
#     ))
#     # ── 2. Chunk biến thể màu + size (giữ nguyên logic)
#     for cv in product.colorVariants or []:
#         color = (cv.color or "không màu").strip()
#         available_sizes = [s.size for s in (cv.sizes or []) if s.stock > 0]
#         if available_sizes:
#             sizes_str = ", ".join(available_sizes)
#             variant_text = (
#                 f"{product.name} màu {color} còn size {sizes_str} "
#                 f"giá {price:,.0f}đ. Xem chi tiết: {product_url}"
#             )
#             docs.append(Document(
#                 page_content=variant_text,
#                 metadata={
#                     "product_id": product_id,
#                     "type": "variant",
#                     "color": color,
#                     "url": product_url
#                 }
#             ))
#     # ── 3. Chunk chi tiết kỹ thuật (cải thiện: thêm keyword để dễ retrieve hơn)
#     for idx, detail in enumerate(product.details or [], 1):
#         text = detail.get("Text")
#         image = detail.get("ImageUrl")
#         sort_order = detail.get("SortOrder", idx)
#         if not text or not isinstance(text, str) or len(text.strip()) < 10:
#             continue # bỏ qua block rỗng hoặc quá ngắn
#         # Tạo nội dung chunk chi tiết với keyword rõ ràng
#         detail_content = f"Thông số kỹ thuật và chi tiết {product.name} (block {sort_order}):\n"
#         detail_content += text.strip()
#         # Nếu có ảnh thì thêm thông tin tham khảo (không bắt buộc)
#         if image and isinstance(image, str):
#             detail_content += f"\n[Hình minh họa: {image}]"
#         detail_content += f"\nXem đầy đủ sản phẩm: {product_url}"
#         docs.append(Document(
#             page_content=detail_content.strip(),
#             metadata={
#                 "product_id": product_id,
#                 "type": "detail",
#                 "block_order": sort_order,
#                 "has_image": bool(image),
#                 "url": product_url
#             }
#         ))
#     return docs
# # ==========================================================
# # XÓA CHUNKS CỦA SẢN PHẨM
# # ==========================================================
# def delete_product_chunks(product_id: int) -> int:
#     if not vectorstore or vectorstore.index.ntotal == 0:
#         return 0
#     if product_id in product_doc_ids:
#         ids_to_delete = product_doc_ids[product_id]
#         vectorstore.delete(ids_to_delete)
#         del product_doc_ids[product_id]
#         return len(ids_to_delete)
#     # Fallback
#     ids_to_delete = []
#     for i in range(vectorstore.index.ntotal):
#         doc_id = vectorstore.index_to_docstore_id[i]
#         doc = vectorstore.docstore.search(doc_id)
#         if isinstance(doc, Document) and doc.metadata.get("product_id") == product_id:
#             ids_to_delete.append(doc_id)
#     if ids_to_delete:
#         vectorstore.delete(ids_to_delete)
#     return len(ids_to_delete)
# # ==========================================================
# # PROMPTS (định nghĩa trước khi build chain)
# # ==========================================================
# template_with_products = """
# Bạn là Linh – nhân viên tư vấn của Shop Cầu Lông Pro.
# DỮ LIỆU SẢN PHẨM TRONG KHO:
# {context}
# LỊCH SỬ HỘI THOẠI GẦN ĐÂY:
# {history}
# CÂU HỎI CỦA KHÁCH:
# {question}
# QUY TẮC TRẢ LỜI:
# 1. Chỉ dùng thông tin từ "DỮ LIỆU SẢN PHẨM TRONG KHO"
# 2. Chỉ dùng đúng link xuất hiện trong dữ liệu: [xem chi tiết](URL)
# 3. Gợi ý tối đa 3 sản phẩm phù hợp nhất
# 4. Gộp màu/size của cùng sản phẩm
# 5. Giọng điệu thân thiện, tự nhiên, hạn chế emoji
# 6. Nếu hỏi về thông số kỹ thuật, sử dụng chi tiết từ các block "Thông số kỹ thuật và chi tiết" trong dữ liệu
# 7. Có thể đưa thêm 1-2 câu hỏi gợi ý để khách hỏi tiếp
# TRẢ LỜI:
# """
# template_fallback = """
# Bạn là Linh – một cô gái trẻ trung, thân thiện, nhiệt tình, làm trợ lý cho Shop Cầu Lông Pro.
# Shop chuyên bán vợt, giày, cước, cầu, phụ kiện cầu lông chính hãng (Yonex, Victor, Lining...), nhưng Linh rất vui khi trò chuyện về bất kỳ chủ đề nào với khách hàng – từ thể thao khác, sản phẩm ngoài lề, đến chuyện đời thường, tình cảm, công việc...
# LỊCH SỬ TRÒ CHUYỆN GẦN ĐÂY (đọc kỹ để hiểu ngữ cảnh và tone hiện tại):
# {history}
# CÂU HỎI HOẶC TIN NHẮN MỚI NHẤT CỦA KHÁCH:
# {question}
# TÌNH HUỐNG HIỆN TẠI: Không tìm thấy thông tin sản phẩm phù hợp trực tiếp từ kho cầu lông.
# CÁCH TRẢ LỜI TỐT NHẤT:
# - Đọc lịch sử để nắm tone cuộc trò chuyện (thân mật, nghiêm túc, vui vẻ, đang hỏi mua hay chỉ hỏi chơi...).
# - Trả lời **tự nhiên như đang chat với bạn**, không cần bắt đầu bằng "Chào bạn" mỗi lần nếu lịch sử đã thân thiết.
# - Trả lời **hữu ích, chân thực** dựa trên kiến thức chung. Nếu là sản phẩm môn khác (giày bóng đá, vợt tennis...), cứ gợi ý thoải mái dựa trên thông tin phổ biến, không cần bịa giá/link.
# - Nếu nhắc đến shop, chỉ nói nhẹ nhàng khi thật sự liên quan (ví dụ: "Shop mình chuyên cầu lông nên không có cái này, nhưng nếu bạn cần... thì Linh tư vấn liền nhé!"). Đừng nhắc lặp lại nếu lịch sử đã nói rồi.
# - Từ "bóng" → mặc định hiểu là **cầu** (shuttlecock) nếu đang nói về cầu lông, chỉ hiểu là bóng đá khi ngữ cảnh rõ ràng (sân cỏ, sút bóng, giày đá bóng...).
# - Giữ giọng điệu vui vẻ, gần gũi, không dùng emoji trừ khi lịch sử có dùng nhiều.
# - Kết thúc bằng câu hỏi hoặc gợi ý tự nhiên để tiếp tục cuộc trò chuyện (không ép buộc, tùy theo flow).
# Hãy trả lời sao cho khách cảm thấy Linh đang thực sự lắng nghe và quan tâm đến họ nhé!
# TRẢ LỜI:
# """
# template_rewrite = """
# Dựa trên lịch sử hội thoại gần đây:
# {history}

# Và câu hỏi hiện tại của khách: {question}

# Hãy viết lại câu hỏi thành một câu hỏi độc lập, đầy đủ thông tin, không cần tham chiếu đến lịch sử (như "này", "trên", "đó"). Bao gồm tên sản phẩm cụ thể nếu lịch sử đề cập.

# Ví dụ:
# - Lịch sử: Khách hỏi về vợt Yonex Astrox 99, bạn gợi ý nó.
# - Câu hỏi: "Thông số kỹ thuật của vợt trên là gì?"
# - Viết lại: "Thông số kỹ thuật của vợt Yonex Astrox 99 là gì?"

# Nếu câu hỏi đã độc lập và không cần ngữ cảnh, giữ nguyên.
# Chỉ trả về câu hỏi đã viết lại, không giải thích.
# """
# prompt_with_products = ChatPromptTemplate.from_template(template_with_products)
# prompt_fallback = ChatPromptTemplate.from_template(template_fallback)
# prompt_rewrite = ChatPromptTemplate.from_template(template_rewrite)
# # ==========================================================
# # BUILD QA CHAIN
# # ==========================================================
# def build_qa_chain():
#     global qa_chain, qa_chain_fallback
#     if not vectorstore or vectorstore.index.ntotal == 0:
#         qa_chain = None
#         qa_chain_fallback = None
#         return
#     # Retriever cơ bản với threshold thấp hơn để lấy nhiều detail hơn (từ 0.35 xuống 0.3)
#     retriever = vectorstore.as_retriever(
#         search_type="similarity_score_threshold",
#         search_kwargs={"k": 15, "score_threshold": 0.3}  # Tăng k=15, hạ threshold để dễ lấy detail
#     )
#     # Chain chính (có sản phẩm)
#     qa_chain = (
#         RunnableParallel({
#             "context": lambda x: "\n".join([d.page_content for d in retriever.invoke(x["question"])]),
#             "question": lambda x: x["question"],
#             "history": lambda x: x.get("history", "Chưa có lịch sử"),
#         })
#         | prompt_with_products
#         | get_llm()
#         | StrOutputParser()
#     )
#     # Chain fallback
#     qa_chain_fallback = (
#         RunnableParallel({
#             "question": lambda x: x["question"],
#             "history": lambda x: x.get("history", "Chưa có lịch sử"),
#         })
#         | prompt_fallback
#         | get_llm()
#         | StrOutputParser()
#     )
# # ==========================================================
# # ROUTES
# # ==========================================================
# @app.get("/")
# async def root():
#     total = vectorstore.index.ntotal if vectorstore else 0
#     return {
#         "message": "Linh đang online và nhớ hết khách rồi nè!",
#         "chunks": total,
#         "active_sessions": len(store)
#     }
# @app.post("/debug_chunks")
# async def debug_chunks_post(
#     limit: int = 50,
#     product_id: Optional[int] = None,
#     include_metadata: bool = True
# ):
#     """
#     Xem tất cả chunk hiện có trong FAISS (dùng POST)
#     - limit: số lượng chunk tối đa trả về (mặc định 50)
#     - product_id: lọc theo sản phẩm (nếu có)
#     - include_metadata: có hiện metadata không
#     """
#     if not vectorstore or vectorstore.index.ntotal == 0:
#         return {
#             "total_chunks_in_db": 0,
#             "returned_chunks": 0,
#             "chunks": [],
#             "message": "Chưa có dữ liệu nào được chunk"
#         }
   
#     total = vectorstore.index.ntotal
#     results = []
   
#     for i in range(min(limit, total)):
#         doc_id = vectorstore.index_to_docstore_id[i]
#         doc = vectorstore.docstore.search(doc_id)
       
#         if isinstance(doc, Document):
#             item = {
#                 "index": i,
#                 "content": doc.page_content
#             }
#             if include_metadata:
#                 item["metadata"] = doc.metadata
           
#             # Lọc theo product_id nếu có
#             if product_id is not None:
#                 if doc.metadata.get("product_id") != product_id:
#                     continue
           
#             results.append(item)
   
#     return {
#         "total_chunks_in_db": total,
#         "returned_chunks": len(results),
#         "chunks": results,
#         "message": f"Đã chunk {total} mẩu dữ liệu từ sản phẩm"
#     }
# @app.post("/add_product")
# async def add_product(product: Product):
#     global vectorstore
   
#     delete_product_chunks(product.id)
#     docs = create_product_documents(product)
   
#     if vectorstore is None:
#         vectorstore = FAISS.from_documents(docs, get_embedding())
#     else:
#         doc_ids = vectorstore.add_documents(docs)
#         product_doc_ids[product.id] = doc_ids
   
#     vectorstore.save_local(str(DB_FOLDER))
#     build_qa_chain()
   
#     return {"status": "OK", "message": f"Đã thêm {product.name}"}
# @app.post("/update_product")
# async def update_product(product: Product):
#     global vectorstore
   
#     print(f"[AI] Nhận yêu cầu update sản phẩm ID: {product.id} - {product.name}")
   
#     delete_product_chunks(product.id)
#     docs = create_product_documents(product)
   
#     if vectorstore is None:
#         vectorstore = FAISS.from_documents(docs, get_embedding())
#     else:
#         doc_ids = vectorstore.add_documents(docs)
#         product_doc_ids[product.id] = doc_ids
   
#     vectorstore.save_local(str(DB_FOLDER))
#     build_qa_chain()
   
#     print(f"[AI] Đã chunk {len(docs)} mẩu cho sản phẩm {product.name}")
   
#     return {"status": "OK", "message": f"Đã cập nhật {product.name}"}
# @app.post("/delete_product")
# async def delete_product(product_id: int):
#     deleted = delete_product_chunks(product_id)
   
#     if deleted == 0:
#         raise HTTPException(404, "Không tìm thấy sản phẩm")
   
#     vectorstore.save_local(str(DB_FOLDER))
#     build_qa_chain()
   
#     return {"status": "OK", "deleted_chunks": deleted}
# @app.post("/reindex_all")
# async def rebuild(products: List[Product]):
#     global vectorstore, product_doc_ids
   
#     # Reset mapping mới
#     product_doc_ids = {}
   
#     # Gom toàn bộ tài liệu
#     all_docs = []
#     for p in products:
#         docs = create_product_documents(p)
#         all_docs.extend(docs)
#         product_doc_ids[p.id] = [] # tạm tạo danh sách rỗng
   
#     # Build FAISS từ đầu
#     vectorstore = FAISS.from_documents(all_docs, get_embedding())
   
#     # Sau khi build FAISS → doc_ids được sinh theo thứ tự
#     current_index = 0
#     for p in products:
#         docs_count = len(create_product_documents(p))
#         ids = []
#         for i in range(docs_count):
#             ids.append(vectorstore.index_to_docstore_id[current_index + i])
#         product_doc_ids[p.id] = ids
#         current_index += docs_count
   
#     # Lưu database
#     vectorstore.save_local(str(DB_FOLDER))
#     build_qa_chain()
   
#     return {"status": "OK", "chunks": len(all_docs)}
# # ==================== CHAT PAYLOAD ====================
# class ChatPayload(BaseModel):
#     question: str
#     session_id: Optional[str] = None
#     user_id: Optional[str] = None
# @app.post("/chat")
# async def chat(payload: ChatPayload):
#     if not qa_chain:
#         raise HTTPException(500, "Chưa có dữ liệu sản phẩm!")
#     # Xác định session_id
#     if payload.user_id:
#         session_id = f"user_{payload.user_id}"
#     elif payload.session_id:
#         session_id = payload.session_id
#     else:
#         session_id = str(uuid.uuid4())
#     history = get_session_history(session_id)
#     history_str = "\n".join(
#         f"{'Khách' if isinstance(m, HumanMessage) else 'Linh'}: {m.content}"
#         for m in history.messages[-10:]
#     ) or "Chưa có lịch sử"
#     # Query Classification
#     classify_prompt = ChatPromptTemplate.from_template("""
#         Phân loại câu hỏi: "{question}"
#         - Nếu liên quan đến sản phẩm cầu lông (vợt, giày, cước, tư vấn mua hàng, so sánh sản phẩm, thông số kỹ thuật): trả "relevant"
#         - Nếu không liên quan (hỏi tình yêu, thời tiết, chủ đề khác): trả "irrelevant"
#         Chỉ trả từ "relevant" hoặc "irrelevant", không giải thích.
#     """)
#     classify_chain = classify_prompt | get_llm() | StrOutputParser()
#     classification = classify_chain.invoke({"question": payload.question}).strip().lower()
#     if classification == "irrelevant":
#         print("Query không liên quan → direct fallback")
#         answer = qa_chain_fallback.invoke({"question": payload.question, "history": history_str})
#     else:
#         # Rewrite query để xử lý ngữ cảnh (như "vợt trên")
#         rewrite_chain = prompt_rewrite | get_llm() | StrOutputParser()
#         standalone_question = rewrite_chain.invoke({"question": payload.question, "history": history_str}).strip()
#         print(f"Query gốc: {payload.question} → Standalone: {standalone_question}")
#         # Retrieve + kiểm tra context
#         retriever = vectorstore.as_retriever(search_kwargs={"k": 15})
#         try:
#             retrieved_docs = vectorstore.as_retriever(
#                 search_type="similarity_score_threshold",
#                 search_kwargs={"k": 15, "score_threshold": 0.3}
#             ).invoke(standalone_question)
#         except:
#             retrieved_docs = retriever.invoke(standalone_question)
#         context_text = "\n".join([d.page_content for d in retrieved_docs])
#         product_keywords = ["vợt", "giày", "cước", "yonex", "victor", "lining", "còn hàng", "thông số", "chi tiết"]
#         has_products = len(retrieved_docs) > 0 and any(kw.lower() in context_text.lower() for kw in product_keywords)
#         print(f"Số docs: {len(retrieved_docs)} | Có sản phẩm phù hợp: {has_products}")
#         if has_products:
#             answer = qa_chain.invoke({"question": payload.question, "history": history_str, "context": context_text})  # Sử dụng question gốc cho prompt, nhưng context từ standalone
#         else:
#             answer = qa_chain_fallback.invoke({"question": payload.question, "history": history_str})
#     # Lưu lịch sử
#     history.add_message(HumanMessage(content=payload.question))
#     history.add_message(AIMessage(content=answer))
#     save_history()
#     resp = {"answer": answer.strip()}
#     if not payload.user_id and not payload.session_id:
#         resp["session_id"] = session_id
#     return resp
# @app.get("/my_chat_history")
# async def get_my_chat_history(user_id: str = None, session_id: str = None):
#     # Frontend gửi user_id (đã đăng nhập) hoặc session_id (khách vãng lai)
#     if user_id:
#         sid = f"user_{user_id}"
#     elif session_id:
#         sid = session_id
#     else:
#         raise HTTPException(400, "Thiếu user_id hoặc session_id")
   
#     history = get_session_history(sid)
#     messages = []
#     for msg in history.messages:
#         role = "user" if isinstance(msg, HumanMessage) else "ai"
#         messages.append({"role": role, "content": msg.content})
   
#     return {"messages": messages}

# main.py – PHIÊN BẢN TỐI ƯU HOÀN CHỈNH (copy và chạy luôn)
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
app = FastAPI(title="Shop Cầu Lông AI – Retrieval Siêu Chính Xác")
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
HISTORY_FILE = Path("chat_history.json")

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
# HÀM TẠO CHUNKS TỐI ƯU HƠN
# ==========================================================
def create_product_documents(product: Product) -> List[Document]:
    price = product.discountPrice or product.price
    product_id = product.id
    product_url = f"http://localhost:3000/product/{product_id}"
    docs = []
    
    # ── 1. Chunk tổng quan với keywords tìm kiếm
    overview_keywords = [
        product.name,
        product.brandName,
        product.categoryName,
        "vợt cầu lông" if "vợt" in product.name.lower() else "",
        "giày cầu lông" if "giày" in product.name.lower() else "",
    ]
    
    info_text = f"{product.name} | {product.brandName} | {product.categoryName}"
    info_text += f" | Giá: {price:,.0f}đ"
    if product.discountPrice:
        info_text += f" (giảm còn {product.discountPrice:,.0f}đ)"
    if product.description:
        info_text += f"\nMô tả: {product.description.strip()}"
    info_text += f"\nKeywords: {', '.join(filter(None, overview_keywords))}"
    info_text += f"\nXem chi tiết: {product_url}"
    
    docs.append(Document(
        page_content=info_text.strip(),
        metadata={
            "product_id": product_id,
            "type": "overview",
            "url": product_url
        }
    ))
    
    # ── 2. Chunk biến thể màu + size
    for cv in product.colorVariants or []:
        color = (cv.color or "không màu").strip()
        available_sizes = [s.size for s in (cv.sizes or []) if s.stock > 0]
        if available_sizes:
            sizes_str = ", ".join(available_sizes)
            variant_text = (
                f"{product.name} màu {color} còn size {sizes_str} "
                f"giá {price:,.0f}đ. Xem chi tiết: {product_url}"
            )
            docs.append(Document(
                page_content=variant_text,
                metadata={
                    "product_id": product_id,
                    "type": "variant",
                    "color": color,
                    "url": product_url
                }
            ))
    
    # 3.Tạo nhiều chunks cho details với keywords phong phú
    all_details_text = []
    
    for idx, detail in enumerate(product.details or [], 1):
        text = detail.get("Text")
        if not text or not isinstance(text, str) or len(text.strip()) < 10:
            continue
        all_details_text.append(text.strip())
    
    # Gộp toàn bộ details thành 1 văn bản lớn
    full_details = "\n\n".join(all_details_text)
    
    if full_details:
        # A. Chunk TỔNG HỢP toàn bộ details (cho câu hỏi tổng quát)
        detail_summary = f"""
CHI TIẾT ĐẦY ĐỦ VỀ {product.name}

{full_details}

Xem sản phẩm: {product_url}
        """.strip()
        
        docs.append(Document(
            page_content=detail_summary,
            metadata={
                "product_id": product_id,
                "type": "detail_full",
                "url": product_url
            }
        ))
        
        # B. Chunk THÔNG SỐ KỸ THUẬT (trích xuất từ details)
        tech_keywords = [
            "thông số", "trọng lượng", "độ cứng", "điểm cân bằng", 
            "chu vi", "chiều dài", "mức căng", "điểm swing"," Vật liệu trục","Chiều dài tổng thể"
            "Vật liệu khung", "công nghệ", "weight", "balance", "stiffness","Chiều dài cán vợt","Mức căng dây"
        ]
        
        tech_sections = []
        for line in full_details.split('\n'):
            if any(kw in line.lower() for kw in tech_keywords):
                tech_sections.append(line.strip())
        
        if tech_sections:
            tech_content = f"""
THÔNG SỐ KỸ THUẬT CHI TIẾT - {product.name}

{chr(10).join(tech_sections)}

Keywords: thông số kỹ thuật, specifications, độ cứng, trọng lượng, cân nặng, điểm cân bằng, kích thước
Xem đầy đủ: {product_url}
            """.strip()
            
            docs.append(Document(
                page_content=tech_content,
                metadata={
                    "product_id": product_id,
                    "type": "detail_specs",
                    "url": product_url
                }
            ))
        
        # C. Chunk CÔNG NGHỆ (nếu có)
        tech_blocks = []
        for text in all_details_text:
            if "công nghệ" in text.lower() or "technology" in text.lower():
                tech_blocks.append(text)
        
        if tech_blocks:
            tech_content = f"""
CÔNG NGHỆ ÁP DỤNG TRÊN {product.name}

{chr(10).join(tech_blocks)}

Keywords: công nghệ, technology, innovation, tính năng
Xem chi tiết: {product_url}
            """.strip()
            
            docs.append(Document(
                page_content=tech_content,
                metadata={
                    "product_id": product_id,
                    "type": "detail_tech",
                    "url": product_url
                }
            ))
        
        # D. Chunk ĐỐI TƯỢNG SỬ DỤNG (nếu có)
        target_blocks = []
        for text in all_details_text:
            if any(kw in text.lower() for kw in ["phù hợp", "đối tượng", "người chơi", "trình độ"]):
                target_blocks.append(text)
        
        if target_blocks:
            target_content = f"""
ĐỐI TƯỢNG PHÙ HỢP VỚI {product.name}

{chr(10).join(target_blocks)}

Keywords: phù hợp, đối tượng, người chơi, trình độ, level
Xem thêm: {product_url}
            """.strip()
            
            docs.append(Document(
                page_content=target_content,
                metadata={
                    "product_id": product_id,
                    "type": "detail_target",
                    "url": product_url
                }
            ))
    
    return docs

# ==========================================================
# XÓA CHUNKS
# ==========================================================
def delete_product_chunks(product_id: int) -> int:
    if not vectorstore or vectorstore.index.ntotal == 0:
        return 0
    if product_id in product_doc_ids:
        ids_to_delete = product_doc_ids[product_id]
        vectorstore.delete(ids_to_delete)
        del product_doc_ids[product_id]
        return len(ids_to_delete)
    
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
# QUERY EXPANSION - Mở rộng query với từ đồng nghĩa
# ==========================================================
def expand_query(original_query: str) -> str:
    """
    Mở rộng query với các từ đồng nghĩa và liên quan
    """
    expansions = {
        "thông số": ["thông số kỹ thuật", "specifications", "specs", "độ cứng", "trọng lượng", "cân nặng", "điểm cân bằng"],
        "công nghệ": ["technology", "tính năng", "innovation", "cải tiến"],
        "phù hợp": ["đối tượng", "người chơi", "trình độ", "suitable for"],
        "giá": ["giá bán", "price", "giá tiền", "bao nhiêu tiền"],
        "màu": ["màu sắc", "color", "phối màu"],
        "size": ["kích thước", "kích cỡ", "số đo"],
    }
    
    query_lower = original_query.lower()
    expanded_terms = [original_query]
    
    for keyword, synonyms in expansions.items():
        if keyword in query_lower:
            expanded_terms.extend(synonyms)
    
    return " ".join(expanded_terms)

# ==========================================================
# PROMPTS
# ==========================================================
template_with_products = """
Bạn là Linh – nhân viên tư vấn của Shop Cầu Lông Pro.

DỮ LIỆU SẢN PHẨM TRONG KHO:
{context}

LỊCH SỬ HỘI THOẠI GẦN ĐÂY:
{history}

CÂU HỎI CỦA KHÁCH:
{question}

NGUYÊN TẮC BẮT BUỘC:
1. Chỉ trả lời dựa trên DỮ LIỆU SẢN PHẨM TRONG KHO bên trên.
2. KHÔNG sử dụng kiến thức bên ngoài dữ liệu đã cung cấp.
3. Nếu dữ liệu KHÔNG đủ để trả lời:
   - Phải nói rõ: "Hiện shop chưa có đủ thông tin cho nội dung này".
   - KHÔNG suy đoán, KHÔNG bịa.
4. Nếu dữ liệu chỉ trả lời được một phần:
   - Chỉ trả lời phần có dữ liệu.
   - Phần còn thiếu phải nói rõ là chưa có thông tin.
5. Khi trích link, Chỉ dùng đúng link xuất hiện trong dữ liệu: [xem chi tiết](URL)
6. Ưu tiên dùng các cụm:
   - "Dựa trên dữ liệu shop đang có..."
   - "Theo thông tin hiện tại của shop..."
   - "Trong hệ thống của shop..."

CÁCH TRẢ LỜI:
- Giọng điệu thân thiện, tự nhiên, chuyên nghiệp
- Gợi ý tối đa 3 sản phẩm phù hợp nhất (nếu có)
- Gộp màu/size của cùng một sản phẩm
- Với thông số kỹ thuật: trích đúng từ phần chi tiết
- Có thể kết thúc bằng 1 câu hỏi gợi ý nhẹ (không ép mua)

VÍ DỤ:
Câu hỏi: "Vợt này có phù hợp người mới không?"
Trả lời:
"Dựa trên dữ liệu shop đang có, mẫu vợt này có trọng lượng nhẹ và thân vợt không quá cứng,
phù hợp cho người mới chơi hoặc chơi phong trào."

Câu hỏi: "Vợt này đánh có sướng không?"
Trả lời:
"Hiện shop chưa có đủ dữ liệu để đánh giá cảm giác đánh thực tế của sản phẩm này.
Nếu bạn cho Linh biết trình độ chơi, mình sẽ tư vấn chính xác hơn nhé."

TRẢ LỜI:

"""

template_fallback = """
Bạn là Linh – trợ lý thân thiện của Shop Cầu Lông Pro.

LỊCH SỬ TRÒ CHUYỆN GẦN ĐÂY:
{history}

CÂU HỎI HOẶC TIN NHẮN MỚI NHẤT:
{question}

TÌNH HUỐNG:
Không tìm thấy dữ liệu sản phẩm phù hợp trong hệ thống shop.

NGUYÊN TẮC:
1. Đây là trả lời mang tính trò chuyện và tham khảo, KHÔNG dựa trên dữ liệu shop.
2. KHÔNG khẳng định tuyệt đối, KHÔNG nói như thông tin chính thức của shop.
3. Nếu chia sẻ ý kiến hoặc kinh nghiệm, cần nói rõ tính tham khảo.

CÁCH TRẢ LỜI:
- Trò chuyện tự nhiên như đang chat với bạn
- Có thể chia sẻ kiến thức chung hoặc kinh nghiệm phổ biến
- Dùng các cụm:
  - "Mình chia sẻ ở góc độ tham khảo nhé..."
  - "Theo kinh nghiệm chung thì..."
  - "Ý kiến cá nhân của mình là..."
- Không spam emoji
- Kết thúc bằng câu hỏi hoặc gợi ý nhẹ để tiếp tục cuộc trò chuyện

VÍ DỤ:
Câu hỏi: "Dây mảnh có đánh mạnh hơn không?"
Trả lời:
"Mình chia sẻ ở góc độ tham khảo thôi nha 😊
Dây mảnh thường cho cảm giác cầu tốt hơn, nhưng đổi lại sẽ dễ đứt hơn so với dây dày."

TRẢ LỜI:

"""

template_rewrite = """
Dựa trên lịch sử hội thoại:
{history}

Và câu hỏi hiện tại của khách:
{question}

Hãy viết lại câu hỏi thành MỘT câu hỏi độc lập, đầy đủ ngữ cảnh, KHÔNG dùng các từ mơ hồ như:
"cái này", "vợt trên", "sản phẩm đó", "nó", "loại kia"...

QUY TẮC:
- Nếu lịch sử có nhắc tên sản phẩm → phải đưa tên sản phẩm vào câu hỏi mới
- Có thể bổ sung từ đồng nghĩa để dễ truy xuất dữ liệu
  (ví dụ: thông số = specs = trọng lượng = độ cứng)
- Nếu câu hỏi đã đủ rõ và độc lập → giữ nguyên

VÍ DỤ:
Lịch sử: Khách đang nói về vợt Yonex Astrox 99  
Câu hỏi: "Thông số kỹ thuật của vợt trên là gì?"  
Viết lại: "Thông số kỹ thuật của vợt Yonex Astrox 99 là gì?"

Chỉ trả về câu hỏi đã viết lại, KHÔNG giải thích thêm.

"""

prompt_with_products = ChatPromptTemplate.from_template(template_with_products)
prompt_fallback = ChatPromptTemplate.from_template(template_fallback)
prompt_rewrite = ChatPromptTemplate.from_template(template_rewrite)

# ==========================================================
# BUILD QA CHAIN
# ==========================================================
def build_qa_chain():
    global qa_chain, qa_chain_fallback
    if not vectorstore or vectorstore.index.ntotal == 0:
        qa_chain = None
        qa_chain_fallback = None
        return
    
    # Retriever với MMR để tăng đa dạng kết quả
    retriever = vectorstore.as_retriever(
        search_type="mmr",
        search_kwargs={
            "k": 20,
            "fetch_k": 50,
            "lambda_mult": 0.7
        }
    )
    
    # Chain chính
    qa_chain = (
        RunnableParallel({
            "context": lambda x: "\n\n".join([d.page_content for d in retriever.invoke(x["question"])]),
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
        "message": "Linh đang online với retrieval siêu chính xác!",
        "chunks": total,
        "active_sessions": len(store)
    }

@app.post("/debug_chunks")
async def debug_chunks_post(
    limit: int = 50,
    product_id: Optional[int] = None,
    include_metadata: bool = True
):
    if not vectorstore or vectorstore.index.ntotal == 0:
        return {
            "total_chunks_in_db": 0,
            "returned_chunks": 0,
            "chunks": [],
            "message": "Chưa có dữ liệu"
        }
    
    total = vectorstore.index.ntotal
    results = []
    
    for i in range(min(limit, total)):
        doc_id = vectorstore.index_to_docstore_id[i]
        doc = vectorstore.docstore.search(doc_id)
        
        if isinstance(doc, Document):
            item = {"index": i, "content": doc.page_content}
            if include_metadata:
                item["metadata"] = doc.metadata
            
            if product_id is not None:
                if doc.metadata.get("product_id") != product_id:
                    continue
            
            results.append(item)
    
    return {
        "total_chunks_in_db": total,
        "returned_chunks": len(results),
        "chunks": results
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
    
    return {"status": "OK", "message": f"Đã thêm {product.name}", "chunks": len(docs)}

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
    
    return {"status": "OK", "message": f"Đã cập nhật {product.name}", "chunks": len(docs)}

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
    
    product_doc_ids = {}
    all_docs = []
    
    for p in products:
        docs = create_product_documents(p)
        all_docs.extend(docs)
        product_doc_ids[p.id] = []
    
    vectorstore = FAISS.from_documents(all_docs, get_embedding())
    
    current_index = 0
    for p in products:
        docs_count = len(create_product_documents(p))
        ids = [vectorstore.index_to_docstore_id[current_index + i] for i in range(docs_count)]
        product_doc_ids[p.id] = ids
        current_index += docs_count
    
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
        - Nếu liên quan đến sản phẩm cầu lông (vợt, giày, cước, tư vấn mua, thông số kỹ thuật): "relevant"
        - Nếu không liên quan: "irrelevant"
        Chỉ trả "relevant" hoặc "irrelevant", không giải thích.
    """)
    classify_chain = classify_prompt | get_llm() | StrOutputParser()
    classification = classify_chain.invoke({"question": payload.question}).strip().lower()
    
    if classification == "irrelevant":
        print("Query không liên quan → fallback")
        answer = qa_chain_fallback.invoke({"question": payload.question, "history": history_str})
    else:
        # Rewrite query + Expand query
        rewrite_chain = prompt_rewrite | get_llm() | StrOutputParser()
        standalone_question = rewrite_chain.invoke({
            "question": payload.question, 
            "history": history_str
        }).strip()
        
        # Mở rộng query với từ đồng nghĩa
        expanded_query = expand_query(standalone_question)
        
        print(f"Query gốc: {payload.question}")
        print(f"Standalone: {standalone_question}")
        print(f"Expanded: {expanded_query}")
        
        # Retrieve với expanded query
        retriever = vectorstore.as_retriever(
            search_type="mmr",
            search_kwargs={"k": 20, "fetch_k": 50, "lambda_mult": 0.7}
        )
        
        try:
            retrieved_docs = retriever.invoke(expanded_query)
        except:
            retrieved_docs = retriever.invoke(standalone_question)
        
        context_text = "\n\n".join([d.page_content for d in retrieved_docs])
        
        # Kiểm tra xem có thông tin sản phẩm không
        product_keywords = ["vợt", "giày", "cước", "yonex", "victor", "lining", "thông số", "công nghệ"]
        has_products = len(retrieved_docs) > 0 and any(kw.lower() in context_text.lower() for kw in product_keywords)
        
        print(f"Số docs: {len(retrieved_docs)} | Có sản phẩm: {has_products}")
        
        if has_products:
            answer = qa_chain.invoke({
                "question": payload.question, 
                "history": history_str, 
                "context": context_text
            })
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
