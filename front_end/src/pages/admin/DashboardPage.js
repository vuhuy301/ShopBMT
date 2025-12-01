// // DashboardPage.js
// import React, { useState, useEffect } from "react";
// import axios from "axios";
// import {
//   getAllProducts,
//   createProduct,
//   updateProduct,
//   deleteProduct,
//   getAllBrands,
//   getAllCategories,
// } from "../../services/admin/productAdminService";

// // LẤY BASE URL ẢNH TỪ .env (bắt buộc có dòng này trong file .env)
// // REACT_APP_IMAGE_BASE_URL=https://localhost:7002
// const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL || "https://localhost:7002";

// // COMPONENT PREVIEW ẢNH - ĐÃ SỬA HOÀN TOÀN LỖI + HỖ TRỢ ẢNH CŨ
// const ImagePreview = ({ file, onRemove }) => {
//   const [preview, setPreview] = useState(null);

//   useEffect(() => {
//     setPreview(null);
//     if (!file) return;

//     // Nếu là string → là ảnh cũ từ server
//     if (typeof file === "string") {
//       const fullUrl = file.startsWith("http") ? file : IMAGE_BASE + file;
//       setPreview(fullUrl);
//       return;
//     }

//     // Nếu là File mới upload
//     if (file instanceof File || file instanceof Blob) {
//       const reader = new FileReader();
//       reader.onloadend = () => setPreview(reader.result);
//       reader.onerror = () => console.error("Lỗi đọc file:", file.name);
//       reader.readAsDataURL(file);
//       return () => reader.abort();
//     }
//   }, [file]);

//   if (!preview) return null;

//   return (
//     <div style={{ position: "relative", display: "inline-block", margin: "8px" }}>
//       <img
//         src={preview}
//         alt="preview"
//         style={{
//           width: "120px",
//           height: "120px",
//           objectFit: "cover",
//           borderRadius: "8px",
//           border: "2px solid #ddd",
//         }}
//       />
//       <button
//         type="button"
//         onClick={onRemove}
//         style={{
//           position: "absolute",
//           top: "-8px",
//           right: "-8px",
//           background: "red",
//           color: "white",
//           border: "none",
//           borderRadius: "50%",
//           width: "28px",
//           height: "28px",
//           fontSize: "18px",
//           cursor: "pointer",
//         }}
//       >
//         ×
//       </button>
//     </div>
//   );
// };

// const DashboardPage = () => {
//   const [products, setProducts] = useState([]);
//   const [brands, setBrands] = useState([]);
//   const [categories, setCategories] = useState([]);
//   const [loading, setLoading] = useState(true);
//   const [showModal, setShowModal] = useState(false);
//   const [isEdit, setIsEdit] = useState(false);
//   const [currentId, setCurrentId] = useState(null);

//   // Form state
//   const [name, setName] = useState("");
//   const [description, setDescription] = useState("");
//   const [price, setPrice] = useState("");
//   const [discountPrice, setDiscountPrice] = useState("");
//   const [brandId, setBrandId] = useState("");
//   const [categoryId, setCategoryId] = useState("");
//   const [isFeatured, setIsFeatured] = useState(false);

//   // Ảnh chính
//   const [existingImages, setExistingImages] = useState([]); // string[]
//   const [imageFiles, setImageFiles] = useState([]);         // File[]

//   // Chi tiết mô tả
//   const [existingDetails, setExistingDetails] = useState([]);
//   const [newDetails, setNewDetails] = useState([]);

//   // Biến thể màu (chỉ còn Size + Stock)
//   const [existingColorVariants, setExistingColorVariants] = useState([]);
//   const [newColorVariants, setNewColorVariants] = useState([]);

//   // Load dữ liệu chung
//   useEffect(() => {
//     const load = async () => {
//       try {
//         setLoading(true);
//         const [prods, brds, cats] = await Promise.all([
//           getAllProducts(),
//           getAllBrands(),
//           getAllCategories(),
//         ]);
//         setProducts(prods || []);
//         setBrands(brds || []);
//         setCategories(cats || []);
//       } catch (err) {
//         alert("Lỗi tải dữ liệu: " + err.message);
//       } finally {
//         setLoading(false);
//       }
//     };
//     load();
//   }, []);

//   const resetForm = () => {
//     setName("");
//     setDescription("");
//     setPrice("");
//     setDiscountPrice("");
//     setBrandId("");
//     setCategoryId("");
//     setIsFeatured(false);
//     setExistingImages([]);
//     setImageFiles([]);
//     setExistingDetails([]);
//     setNewDetails([]);
//     setExistingColorVariants([]);
//     setNewColorVariants([]);
//     setIsEdit(false);
//     setCurrentId(null);
//   };

//   const openAdd = () => {
//     resetForm();
//     setNewDetails([{ text: "", imageFile: null }]);
//     setNewColorVariants([{ color: "", imageFiles: [], sizes: [{ size: "", stock: 0 }] }]);
//     setShowModal(true);
//   };

//   const openEdit = async (p) => {
//     try {
//       setLoading(true);
//       resetForm();
//       setIsEdit(true);
//       setCurrentId(p.id);

//       const { data } = await axios.get(`https://localhost:7002/api/Products/${p.id}`);

//       setName(data.name || "");
//       setDescription(data.description || "");
//       setPrice(data.price?.toString() || "");
//       setDiscountPrice(data.discountPrice?.toString() || "");
//       setBrandId(data.brandId?.toString() || "");
//       setCategoryId(data.categoryId?.toString() || "");
//       setIsFeatured(data.isFeatured || false);

//       // Ảnh chính cũ (lưu dạng /images/products/xxx.jpg)
//       setExistingImages(data.images || []);

//       // Chi tiết mô tả cũ
//       setExistingDetails(
//         (data.details || []).map((d) => ({
//           text: d.text || "",
//           imageUrl: d.imageUrl || null,
//         }))
//       );

//       // Biến thể màu cũ
//       setExistingColorVariants(
//         (data.colorVariants || []).map((cv) => ({
//           color: cv.color || "",
//           imageUrls: cv.imageUrls || [],
//           sizes: (cv.sizes || []).map((s) => ({
//             size: s.size || "",
//             stock: s.stock || 0,
//           })),
//         }))
//       );

//       // Khởi tạo phần thêm mới
//       setNewDetails([{ text: "", imageFile: null }]);
//       setNewColorVariants([{ color: "", imageFiles: [], sizes: [{ size: "", stock: 0 }] }]);

//       setShowModal(true);
//     } catch (err) {
//       alert("Lỗi tải chi tiết: " + (err.response?.data?.message || err.message));
//     } finally {
//       setLoading(false);
//     }
//   };

//   const handleSubmit = async (e) => {
//     e.preventDefault();
//     const formData = new FormData();

//     formData.append("Name", name.trim());
//     if (description.trim()) formData.append("Description", description.trim());
//     formData.append("Price", price);
//     if (discountPrice) formData.append("DiscountPrice", discountPrice);
//     formData.append("BrandId", brandId);
//     formData.append("CategoryId", categoryId);
//     formData.append("IsFeatured", isFeatured.toString());

//     // Ảnh chính
//     imageFiles.forEach((f) => formData.append("ImageFiles", f));
//     existingImages.forEach((url) => formData.append("ExistingImages", url));

//     // Chi tiết mô tả mới
//     newDetails.forEach((d, i) => {
//       if (d.text.trim()) formData.append(`Details[${i}].Text`, d.text.trim());
//       if (d.imageFile) formData.append(`Details[${i}].ImageFile`, d.imageFile);
//     });

//     // Chi tiết cũ (khi edit)
//     if (isEdit) {
//       existingDetails.forEach((d, i) => {
//         if (d.text.trim()) formData.append(`ExistingDetails[${i}].Text`, d.text.trim());
//         if (d.imageUrl) formData.append(`ExistingDetails[${i}].ImageUrl`, d.imageUrl);
//       });
//     }

//     // Biến thể màu mới
//     newColorVariants.forEach((v, i) => {
//       if (v.color.trim()) {
//         formData.append(`ColorVariants[${i}].Color`, v.color.trim());
//         v.imageFiles.forEach((f) => formData.append(`ColorVariants[${i}].ImageFiles`, f));
//         v.sizes.forEach((s, j) => {
//           if (s.size.trim()) {
//             formData.append(`ColorVariants[${i}].Sizes[${j}].Size`, s.size.trim());
//             formData.append(`ColorVariants[${i}].Sizes[${j}].Stock`, s.stock.toString());
//           }
//         });
//       }
//     });

//     // Biến thể màu cũ (khi edit)
//     if (isEdit) {
//       existingColorVariants.forEach((v, i) => {
//         if (v.color.trim()) {
//           formData.append(`ExistingColorVariants[${i}].Color`, v.color.trim());
//           v.imageUrls.forEach((url) => formData.append(`ExistingColorVariants[${i}].ImageUrls`, url));
//           v.sizes.forEach((s, j) => {
//             if (s.size.trim()) {
//               formData.append(`ExistingColorVariants[${i}].Sizes[${j}].Size`, s.size.trim());
//               formData.append(`ExistingColorVariants[${i}].Sizes[${j}].Stock`, s.stock.toString());
//             }
//           });
//         }
//       });
//     }

//     try {
//       if (isEdit) {
//         await updateProduct(currentId, formData);
//         alert("Cập nhật thành công!");
//       } else {
//         await createProduct(formData);
//         alert("Thêm sản phẩm thành công!");
//       }
//       setShowModal(false);
//       const prods = await getAllProducts();
//       setProducts(prods);
//     } catch (err) {
//       console.error(err);
//       alert("Lỗi: " + (err.response?.data?.message || err.message));
//     }
//   };

//   const handleDelete = async (id) => {
//     if (!window.confirm("Xóa sản phẩm này thật chứ?")) return;
//     try {
//       await deleteProduct(id);
//       setProducts((prev) => prev.filter((p) => p.id !== id));
//     } catch (err) {
//       alert("Xóa lỗi: " + err.message);
//     }
//   };

//   return (
//     <div style={{ padding: "20px", fontFamily: "Segoe UI, sans-serif" }}>
//       <h1>Quản Lý Sản Phẩm</h1>
//       <button
//         onClick={openAdd}
//         style={{
//           padding: "14px 28px",
//           background: "#28a745",
//           color: "#fff",
//           border: "none",
//           borderRadius: "8px",
//           fontSize: "16px",
//           cursor: "pointer",
//           marginBottom: "20px",
//         }}
//       >
//         + Thêm sản phẩm mới
//       </button>

//       {loading ? (
//         <p>Đang tải...</p>
//       ) : (
//         <div>
//           <h3>Danh sách sản phẩm ({products.length})</h3>
//           {products.map((p) => (
//             <div
//               key={p.id}
//               style={{
//                 border: "1px solid #eee",
//                 padding: "15px",
//                 margin: "12px 0",
//                 borderRadius: "10px",
//                 background: "#f9f9f9",
//               }}
//             >
//               <strong>{p.name}</strong> ({p.brandName} - {p.categoryName})
//               <div style={{ fontSize: "14px", color: "#555" }}>
//                 Giá: {Number(p.price).toLocaleString("vi-VN")}₫
//                 {p.discountPrice && (
//                   <span style={{ color: "red", marginLeft: "10px" }}>
//                     → {Number(p.discountPrice).toLocaleString("vi-VN")}₫
//                   </span>
//                 )}
//                 <br />
//                 Tồn kho: {p.stock}{" "}
//                 {p.isFeatured && <span style={{ color: "orange" }}> [Nổi bật]</span>}
//               </div>
//               <div style={{ marginTop: "10px" }}>
//                 <button
//                   onClick={() => openEdit(p)}
//                   style={{
//                     marginRight: "10px",
//                     padding: "8px 16px",
//                     background: "#007bff",
//                     color: "#fff",
//                     border: "none",
//                     borderRadius: "6px",
//                   }}
//                 >
//                   Sửa
//                 </button>
//                 <button
//                   onClick={() => handleDelete(p.id)}
//                   style={{
//                     padding: "8px 16px",
//                     background: "#dc3545",
//                     color: "#fff",
//                     border: "none",
//                     borderRadius: "6px",
//                   }}
//                 >
//                   Xóa
//                 </button>
//               </div>
//             </div>
//           ))}
//         </div>
//       )}

//       {/* MODAL */}
//       {showModal && (
//         <div
//           style={{
//             position: "fixed",
//             inset: 0,
//             background: "rgba(0,0,0,0.75)",
//             display: "flex",
//             justifyContent: "center",
//             alignItems: "center",
//             zIndex: 999,
//           }}
//         >
//           <div
//             style={{
//               background: "#fff",
//               padding: "30px",
//               borderRadius: "12px",
//               width: "1100px",
//               maxHeight: "90vh",
//               overflowY: "auto",
//             }}
//           >
//             <h2 style={{ margin: 0 }}>
//               {isEdit ? "Sửa sản phẩm" : "Thêm sản phẩm mới"}
//             </h2>
//             <form onSubmit={handleSubmit} style={{ marginTop: "20px" }}>
//               <input
//                 placeholder="Tên sản phẩm *"
//                 value={name}
//                 onChange={(e) => setName(e.target.value)}
//                 required
//                 style={{ width: "100%", padding: "12px", margin: "10px 0", borderRadius: "6px", border: "1px solid #ccc" }}
//               />
//               <textarea
//                 placeholder="Mô tả"
//                 value={description}
//                 onChange={(e) => setDescription(e.target.value)}
//                 style={{ width: "100%", padding: "12px", height: "100px", borderRadius: "6px", border: "1px solid #ccc", margin: "10px 0" }}
//               />
//               <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
//                 <input type="number" placeholder="Giá gốc *" value={price} onChange={(e) => setPrice(e.target.value)} required />
//                 <input type="number" placeholder="Giá giảm" value={discountPrice} onChange={(e) => setDiscountPrice(e.target.value)} />
//                 <select value={brandId} onChange={(e) => setBrandId(e.target.value)} required>
//                   <option value="">Chọn thương hiệu</option>
//                   {brands.map((b) => (
//                     <option key={b.id} value={b.id}>{b.name}</option>
//                   ))}
//                 </select>
//                 <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
//                   <option value="">Chọn danh mục</option>
//                   {categories.map((c) => (
//                     <option key={c.id} value={c.id}>{c.name}</option>
//                   ))}
//                 </select>
//               </div>
//               <label style={{ display: "block", margin: "15px 0" }}>
//                 <input type="checkbox" checked={isFeatured} onChange={(e) => setIsFeatured(e.target.checked)} /> Sản phẩm nổi bật
//               </label>

//               <hr style={{ margin: "30px 0" }} />

//               {/* ẢNH CHÍNH */}
//               {isEdit && existingImages.length > 0 && (
//                 <>
//                   <h4>Ảnh chính cũ (có thể xóa)</h4>
//                   <div style={{ display: "flex", flexWrap: "wrap", marginBottom: "15px" }}>
//                     {existingImages.map((url, i) => (
//                       <ImagePreview
//                         key={i}
//                         file={url}
//                         onRemove={() => setExistingImages(prev => prev.filter((_, idx) => idx !== i))}
//                       />
//                     ))}
//                   </div>
//                 </>
//               )}

//               <h4>Ảnh chính mới</h4>
//               <input
//                 type="file"
//                 multiple
//                 accept="image/*"
//                 onChange={(e) => setImageFiles([...imageFiles, ...Array.from(e.target.files)])}
//               />
//               <div style={{ display: "flex", flexWrap: "wrap", marginTop: "10px" }}>
//                 {imageFiles.map((f, i) => (
//                   <ImagePreview
//                     key={i}
//                     file={f}
//                     onRemove={() => setImageFiles(prev => prev.filter((_, idx) => idx !== i))}
//                   />
//                 ))}
//               </div>

//               <hr style={{ margin: "30px 0" }} />

//               {/* CHI TIẾT MÔ TẢ CŨ */}
//               {isEdit && existingDetails.length > 0 && (
//                 <>
//                   <h4>Chi tiết mô tả cũ</h4>
//                   {existingDetails.map((d, i) => (
//                     <div key={i} style={{ border: "1px dashed #ccc", padding: "15px", margin: "15px 0", borderRadius: "8px" }}>
//                       <textarea
//                         placeholder="Nội dung"
//                         value={d.text}
//                         onChange={(e) => {
//                           const nd = [...existingDetails];
//                           nd[i].text = e.target.value;
//                           setExistingDetails(nd);
//                         }}
//                         style={{ width: "100%", height: "100px", padding: "10px" }}
//                       />
//                       {d.imageUrl && (
//                         <div style={{ marginTop: "10px" }}>
//                           <ImagePreview
//                             file={d.imageUrl}
//                             onRemove={() => {
//                               const nd = [...existingDetails];
//                               nd[i].imageUrl = null;
//                               setExistingDetails(nd);
//                             }}
//                           />
//                         </div>
//                       )}
//                     </div>
//                   ))}
//                 </>
//               )}

//               <h4>{isEdit ? "Thêm chi tiết mô tả mới" : "Chi tiết mô tả"}</h4>
//               {newDetails.map((d, i) => (
//                 <div key={i} style={{ border: "1px dashed #aaa", padding: "15px", margin: "15px 0", borderRadius: "8px" }}>
//                   <textarea
//                     placeholder="Nội dung"
//                     value={d.text || ""}
//                     onChange={(e) => {
//                       const nd = [...newDetails];
//                       nd[i].text = e.target.value;
//                       setNewDetails(nd);
//                     }}
//                     style={{ width: "100%", height: "100px", padding: "10px" }}
//                   />
//                   <input
//                     type="file"
//                     accept="image/*"
//                     onChange={(e) => {
//                       if (e.target.files[0]) {
//                         const nd = [...newDetails];
//                         nd[i].imageFile = e.target.files[0];
//                         setNewDetails(nd);
//                       }
//                     }}
//                   />
//                   {d.imageFile && (
//                     <div style={{ marginTop: "10px" }}>
//                       <ImagePreview
//                         file={d.imageFile}
//                         onRemove={() => {
//                           const nd = [...newDetails];
//                           nd[i].imageFile = null;
//                           setNewDetails(nd);
//                         }}
//                       />
//                     </div>
//                   )}
//                   <button
//                     type="button"
//                     onClick={() => setNewDetails(prev => prev.filter((_, idx) => idx !== i))}
//                     style={{ marginTop: "10px", background: "red", color: "white", padding: "6px 12px", border: "none" }}
//                   >
//                     Xóa mục này
//                   </button>
//                 </div>
//               ))}
//               <button
//                 type="button"
//                 onClick={() => setNewDetails(prev => [...prev, { text: "", imageFile: null }])}
//                 style={{ background: "#007bff", color: "white", padding: "10px 20px", border: "none", borderRadius: "8px" }}
//               >
//                 + Thêm mục mô tả mới
//               </button>

//               <hr style={{ margin: "30px 0" }} />

//               {/* BIẾN THỂ MÀU CŨ */}
//               {isEdit && existingColorVariants.length > 0 && (
//                 <>
//                   <h4>Biến thể màu cũ</h4>
//                   {existingColorVariants.map((variant, i) => (
//                     <div key={i} style={{ border: "1px dashed #ccc", padding: "20px", margin: "15px 0", borderRadius: "8px" }}>
//                       <input
//                         placeholder="Tên màu"
//                         value={variant.color}
//                         onChange={(e) => {
//                           const v = [...existingColorVariants];
//                           v[i].color = e.target.value;
//                           setExistingColorVariants(v);
//                         }}
//                         style={{ width: "100%", padding: "10px", marginBottom: "10px" }}
//                       />
//                       <div style={{ display: "flex", flexWrap: "wrap" }}>
//                         {variant.imageUrls.map((url, idx) => (
//                           <ImagePreview
//                             key={idx}
//                             file={url}
//                             onRemove={() => {
//                               const v = [...existingColorVariants];
//                               v[i].imageUrls = v[i].imageUrls.filter((_, idxx) => idxx !== idx);
//                               setExistingColorVariants(v);
//                             }}
//                           />
//                         ))}
//                       </div>
//                       <h5 style={{ margin: "15px 0 10px" }}>Kích thước & tồn kho</h5>
//                       {variant.sizes.map((size, idx) => (
//                         <div key={idx} style={{ display: "flex", gap: "10px", marginBottom: "10px", alignItems: "center" }}>
//                           <input
//                             placeholder="Size"
//                             value={size.size}
//                             onChange={(e) => {
//                               const v = [...existingColorVariants];
//                               v[i].sizes[idx].size = e.target.value;
//                               setExistingColorVariants(v);
//                             }}
//                           />
//                           <input
//                             type="number"
//                             placeholder="Tồn kho"
//                             value={size.stock}
//                             onChange={(e) => {
//                               const v = [...existingColorVariants];
//                               v[i].sizes[idx].stock = parseInt(e.target.value) || 0;
//                               setExistingColorVariants(v);
//                             }}
//                           />
//                           <button
//                             type="button"
//                             onClick={() => {
//                               const v = [...existingColorVariants];
//                               v[i].sizes = v[i].sizes.filter((_, idxx) => idxx !== idx);
//                               setExistingColorVariants(v);
//                             }}
//                             style={{ background: "red", color: "white", padding: "8px 12px", border: "none" }}
//                           >
//                             Xóa
//                           </button>
//                         </div>
//                       ))}
//                       <button
//                         type="button"
//                         onClick={() => {
//                           const v = [...existingColorVariants];
//                           v[i].sizes.push({ size: "", stock: 0 });
//                           setExistingColorVariants(v);
//                         }}
//                         style={{ background: "#28a745", color: "white", padding: "8px 16px", border: "none" }}
//                       >
//                         + Thêm size
//                       </button>
//                     </div>
//                   ))}
//                 </>
//               )}

//               {/* BIẾN THỂ MÀU MỚI */}
//               <h4>{isEdit ? "Thêm biến thể màu mới" : "Biến thể màu"}</h4>
//               {newColorVariants.map((variant, i) => (
//                 <div key={i} style={{ border: "1px dashed #aaa", padding: "20px", margin: "20px 0", borderRadius: "10px" }}>
//                   <input
//                     placeholder="Tên màu (VD: Đỏ, Xanh...)"
//                     value={variant.color}
//                     onChange={(e) => {
//                       const v = [...newColorVariants];
//                       v[i].color = e.target.value;
//                       setNewColorVariants(v);
//                     }}
//                     style={{ width: "100%", padding: "10px", marginBottom: "10px" }}
//                   />
//                   <input
//                     type="file"
//                     multiple
//                     accept="image/*"
//                     onChange={(e) => {
//                       const files = Array.from(e.target.files);
//                       const v = [...newColorVariants];
//                       v[i].imageFiles = [...(v[i].imageFiles || []), ...files];
//                       setNewColorVariants(v);
//                     }}
//                   />
//                   <div style={{ display: "flex", flexWrap: "wrap", marginTop: "10px" }}>
//                     {(variant.imageFiles || []).map((f, idx) => (
//                       <ImagePreview
//                         key={idx}
//                         file={f}
//                         onRemove={() => {
//                           const v = [...newColorVariants];
//                           v[i].imageFiles = v[i].imageFiles.filter((_, idxx) => idxx !== idx);
//                           setNewColorVariants(v);
//                         }}
//                       />
//                     ))}
//                   </div>

//                   <h5 style={{ margin: "15px 0 10px" }}>Kích thước & tồn kho</h5>
//                   {variant.sizes.map((size, idx) => (
//                     <div key={idx} style={{ display: "flex", gap: "10px", marginBottom: "10px", alignItems: "center" }}>
//                       <input
//                         placeholder="Size (VD: M, L)"
//                         value={size.size}
//                         onChange={(e) => {
//                           const v = [...newColorVariants];
//                           v[i].sizes[idx].size = e.target.value;
//                           setNewColorVariants(v);
//                         }}
//                       />
//                       <input
//                         type="number"
//                         placeholder="Tồn kho"
//                         value={size.stock}
//                         onChange={(e) => {
//                           const v = [...newColorVariants];
//                           v[i].sizes[idx].stock = parseInt(e.target.value) || 0;
//                           setNewColorVariants(v);
//                         }}
//                       />
//                       <button
//                         type="button"
//                         onClick={() => {
//                           const v = [...newColorVariants];
//                           v[i].sizes = v[i].sizes.filter((_, idxx) => idxx !== idx);
//                           setNewColorVariants(v);
//                         }}
//                         style={{ background: "red", color: "white", padding: "8px 12px", border: "none" }}
//                       >
//                         Xóa
//                       </button>
//                     </div>
//                   ))}
//                   <button
//                     type="button"
//                     onClick={() => {
//                       const v = [...newColorVariants];
//                       v[i].sizes.push({ size: "", stock: 0 });
//                       setNewColorVariants(v);
//                     }}
//                     style={{ background: "#28a745", color: "white", padding: "8px 16px", border: "none", marginTop: "10px" }}
//                   >
//                     + Thêm size
//                   </button>

//                   <button
//                     type="button"
//                     onClick={() => setNewColorVariants(prev => prev.filter((_, idx) => idx !== i))}
//                     style={{ background: "red", color: "white", padding: "10px 20px", border: "none", marginLeft: "20px" }}
//                   >
//                     Xóa màu này
//                   </button>
//                 </div>
//               ))}

//               <button
//                 type="button"
//                 onClick={() => setNewColorVariants(prev => [...prev, { color: "", imageFiles: [], sizes: [{ size: "", stock: 0 }] }])}
//                 style={{
//                   background: "#007bff",
//                   color: "white",
//                   padding: "12px 24px",
//                   border: "none",
//                   borderRadius: "8px",
//                   fontSize: "16px",
//                   margin: "20px 0",
//                 }}
//               >
//                 + Thêm biến thể màu mới
//               </button>

//               <div style={{ textAlign: "right", marginTop: "40px" }}>
//                 <button
//                   type="button"
//                   onClick={() => setShowModal(false)}
//                   style={{
//                     marginRight: "20px",
//                     padding: "12px 24px",
//                     background: "#6c757d",
//                     color: "#fff",
//                     border: "none",
//                     borderRadius: "8px",
//                   }}
//                 >
//                   Hủy
//                 </button>
//                 <button
//                   type="submit"
//                   style={{
//                     padding: "14px 40px",
//                     background: "#28a745",
//                     color: "#fff",
//                     border: "none",
//                     borderRadius: "8px",
//                     fontSize: "16px",
//                   }}
//                 >
//                   {isEdit ? "CẬP NHẬT" : "THÊM MỚI"}
//                 </button>
//               </div>
//             </form>
//           </div>
//         </div>
//       )}
//     </div>
//   );
// };

// export default DashboardPage;
// DashboardPage.js - PHIÊN BẢN HOÀN HẢO - STOCK KHÔNG BAO GIỜ BỊ RESET
import React, { useState, useEffect } from "react";
import axios from "axios";
import {
  getAllProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  getAllBrands,
  getAllCategories,
} from "../../services/admin/productAdminService";

// Base URL ảnh từ .env (bắt buộc có dòng này trong .env)
// REACT_APP_IMAGE_BASE_URL=https://localhost:7002
const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL || "https://localhost:7002";

// Component preview ảnh (hỗ trợ cả ảnh cũ từ server và ảnh mới upload)
const ImagePreview = ({ file, onRemove }) => {
  const [preview, setPreview] = useState(null);

  useEffect(() => {
    setPreview(null);
    if (!file) return;

    if (typeof file === "string") {
      const fullUrl = file.startsWith("http") ? file : IMAGE_BASE + file;
      setPreview(fullUrl);
      return;
    }

    if (file instanceof File || file instanceof Blob) {
      const reader = new FileReader();
      reader.onloadend = () => setPreview(reader.result);
      reader.onerror = () => console.error("Lỗi đọc file:", file.name);
      reader.readAsDataURL(file);
      return () => reader.abort();
    }
  }, [file]);

  if (!preview) return null;

  return (
    <div style={{ position: "relative", display: "inline-block", margin: "8px" }}>
      <img
        src={preview}
        alt="preview"
        style={{
          width: "120px",
          height: "120px",
          objectFit: "cover",
          borderRadius: "8px",
          border: "2px solid #ddd",
        }}
      />
      <button
        type="button"
        onClick={onRemove}
        style={{
          position: "absolute",
          top: "-8px",
          right: "-8px",
          background: "red",
          color: "white",
          border: "none",
          borderRadius: "50%",
          width: "28px",
          height: "28px",
          fontSize: "18px",
          cursor: "pointer",
        }}
      >
        ×
      </button>
    </div>
  );
};

const DashboardPage = () => {
  const [products, setProducts] = useState([]);
  const [brands, setBrands] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [currentId, setCurrentId] = useState(null);

  // Thông tin cơ bản
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [discountPrice, setDiscountPrice] = useState("");
  const [brandId, setBrandId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [isFeatured, setIsFeatured] = useState(false);

  // Ảnh chính
  const [existingImages, setExistingImages] = useState([]); // string[]
  const [imageFiles, setImageFiles] = useState([]);         // File[]

  // Chi tiết mô tả (gộp cũ + mới)
  const [details, setDetails] = useState([]);

  // Biến thể màu - GỘP CŨ + MỚI THÀNH 1 MẢNG DUY NHẤT → KHÔNG MẤT STOCK
  const [colorVariants, setColorVariants] = useState([]);

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const [prods, brds, cats] = await Promise.all([
          getAllProducts(),
          getAllBrands(),
          getAllCategories(),
        ]);
        setProducts(prods || []);
        setBrands(brds || []);
        setCategories(cats || []);
      } catch (err) {
        alert("Lỗi tải dữ liệu: " + err.message);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const resetForm = () => {
    setName(""); setDescription(""); setPrice(""); setDiscountPrice("");
    setBrandId(""); setCategoryId(""); setIsFeatured(false);
    setExistingImages([]); setImageFiles([]);
    setDetails([]);
    setColorVariants([]);
    setIsEdit(false); setCurrentId(null);
  };

  const openAdd = () => {
    resetForm();
    setDetails([{ text: "", imageFile: null, imageUrl: null }]);
    setColorVariants([{ color: "", imageFiles: [], imageUrls: [], sizes: [{ size: "", stock: 0 }] }]);
    setShowModal(true);
  };

  const openEdit = async (p) => {
    try {
      setLoading(true);
      resetForm();
      setIsEdit(true);
      setCurrentId(p.id);

      const { data } = await axios.get(`https://localhost:7002/api/Products/${p.id}`);

      setName(data.name || "");
      setDescription(data.description || "");
      setPrice(data.price?.toString() || "");
      setDiscountPrice(data.discountPrice?.toString() || "");
      setBrandId(data.brandId?.toString() || "");
      setCategoryId(data.categoryId?.toString() || "");
      setIsFeatured(data.isFeatured || false);
      setExistingImages(data.images || []);

      // Chi tiết mô tả cũ
      setDetails((data.details || []).map(d => ({
        text: d.text || "",
        imageUrl: d.imageUrl || null,
        imageFile: null,
      })));

      // Biến thể màu cũ → gộp luôn vào mảng chung
      const loadedVariants = (data.colorVariants || []).map(cv => ({
      id: cv.id,                                      // THÊM DÒNG NÀY
      color: cv.color || "",
      imageUrls: cv.imageUrls || [],
      imageFiles: [],
      sizes: (cv.sizes || []).map(s => ({
        id: s.id,                                     // THÊM DÒNG NÀY
        size: s.size || "",
        stock: s.stock || 0,
      })),
    }));

    setColorVariants(loadedVariants.length > 0 
      ? loadedVariants 
      : [{ id: 0, color: "", imageFiles: [], imageUrls: [], sizes: [{ id: 0, size: "", stock: 0 }] }]
    );

      setShowModal(true);
    } catch (err) {
      alert("Lỗi tải chi tiết: " + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const formData = new FormData();

    formData.append("Name", name.trim());
    if (description.trim()) formData.append("Description", description.trim());
    formData.append("Price", price);
    if (discountPrice) formData.append("DiscountPrice", discountPrice);
    formData.append("BrandId", brandId);
    formData.append("CategoryId", categoryId);
    formData.append("IsFeatured", isFeatured.toString());

    // Ảnh chính
    imageFiles.forEach(f => formData.append("ImageFiles", f));
    existingImages.forEach(url => formData.append("ExistingImages", url));

    // Chi tiết mô tả (cũ + mới)
    details.forEach((d, i) => {
      if (d.text?.trim()) {
        formData.append(`Details[${i}].Text`, d.text.trim());
        if (d.imageFile) formData.append(`Details[${i}].ImageFile`, d.imageFile);
        if (d.imageUrl) formData.append(`Details[${i}].ImageUrl`, d.imageUrl);
      }
    });

    // TẤT CẢ BIẾN THỂ MÀU (CŨ + MỚI) GỬI CHUNG QUA ColorVariants
    colorVariants.forEach((variant, i) => {
      if (!variant.color.trim()) return;
      formData.append(`ColorVariants[${i}].Id`, variant.id || 0);
      formData.append(`ColorVariants[${i}].Color`, variant.color.trim());

      // Ảnh mới
      (variant.imageFiles || []).forEach(f => formData.append(`ColorVariants[${i}].ImageFiles`, f));
      // Ảnh cũ
      (variant.imageUrls || []).forEach(url => formData.append(`ColorVariants[${i}].ImageUrls`, url));

      // Sizes
      variant.sizes.forEach((s, j) => {
        if (s.size.trim()) {
          formData.append(`ColorVariants[${i}].Sizes[${j}].Id`, s.id || 0);
          formData.append(`ColorVariants[${i}].Sizes[${j}].Size`, s.size.trim());
          formData.append(`ColorVariants[${i}].Sizes[${j}].Stock`, s.stock.toString());
        }
      });
    });

    try {
      if (isEdit) {
        await updateProduct(currentId, formData);
        alert("Cập nhật sản phẩm thành công!");
      } else {
        await createProduct(formData);
        alert("Thêm sản phẩm thành công!");
      }
      setShowModal(false);
      const prods = await getAllProducts();
      setProducts(prods);
    } catch (err) {
      console.error(err);
      alert("Lỗi: " + (err.response?.data?.message || err.message));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa sản phẩm này thật chứ?")) return;
    try {
      await deleteProduct(id);
      setProducts(prev => prev.filter(p => p.id !== id));
    } catch (err) {
      alert("Xóa lỗi: " + err.message);
    }
  };

  return (
    <div style={{ padding: "20px", fontFamily: "Segoe UI, sans-serif" }}>
      <h1>Quản Lý Sản Phẩm</h1>
      <button
        onClick={openAdd}
        style={{
          padding: "14px 28px",
          background: "#28a745",
          color: "#fff",
          border: "none",
          borderRadius: "8px",
          fontSize: "16px",
          cursor: "pointer",
          marginBottom: "20px",
        }}
      >
        + Thêm sản phẩm mới
      </button>

      {loading ? <p>Đang tải...</p> : (
        <div>
          <h3>Danh sách sản phẩm ({products.length})</h3>
          {products.map(p => (
            <div key={p.id} style={{ border: "1px solid #eee", padding: "15px", margin: "12px 0", borderRadius: "10px", background: "#f9f9f9" }}>
              <strong>{p.name}</strong> ({p.brandName} - {p.categoryName})
              <div style={{ fontSize: "14px", color: "#555" }}>
                Giá: {Number(p.price).toLocaleString("vi-VN")}₫
                {p.discountPrice && <span style={{ color: "red", marginLeft: "10px" }}>→ {Number(p.discountPrice).toLocaleString("vi-VN")}₫</span>}
                <br />
                Tồn kho: {p.stock} {p.isFeatured && <span style={{ color: "orange" }}> [Nổi bật]</span>}
              </div>
              <div style={{ marginTop: "10px" }}>
                <button onClick={() => openEdit(p)} style={{ marginRight: "10px", padding: "8px 16px", background: "#007bff", color: "#fff", border: "none", borderRadius: "6px" }}>Sửa</button>
                <button onClick={() => handleDelete(p.id)} style={{ padding: "8px 16px", background: "#dc3545", color: "#fff", border: "none", borderRadius: "6px" }}>Xóa</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* MODAL */}
      {showModal && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", justifyContent: "center", alignItems: "center", zIndex: 999 }}>
          <div style={{ background: "#fff", padding: "30px", borderRadius: "12px", width: "1100px", maxHeight: "90vh", overflowY: "auto" }}>
            <h2 style={{ margin: 0 }}>{isEdit ? "Sửa sản phẩm" : "Thêm sản phẩm mới"}</h2>
            <form onSubmit={handleSubmit} style={{ marginTop: "20px" }}>

              <input placeholder="Tên sản phẩm *" value={name} onChange={e => setName(e.target.value)} required style={{ width: "100%", padding: "12px", margin: "10px 0", borderRadius: "6px", border: "1px solid #ccc" }} />
              <textarea placeholder="Mô tả" value={description} onChange={e => setDescription(e.target.value)} style={{ width: "100%", padding: "12px", height: "100px", borderRadius: "6px", border: "1px solid #ccc", margin: "10px 0" }} />

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
                <input type="number" placeholder="Giá gốc *" value={price} onChange={e => setPrice(e.target.value)} required />
                <input type="number" placeholder="Giá giảm (không bắt buộc)" value={discountPrice} onChange={e => setDiscountPrice(e.target.value)} />
                <select value={brandId} onChange={e => setBrandId(e.target.value)} required>
                  <option value="">Chọn thương hiệu</option>
                  {brands.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                </select>
                <select value={categoryId} onChange={e => setCategoryId(e.target.value)} required>
                  <option value="">Chọn danh mục</option>
                  {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>

              <label style={{ display: "block", margin: "15px 0" }}>
                <input type="checkbox" checked={isFeatured} onChange={e => setIsFeatured(e.target.checked)} /> Sản phẩm nổi bật
              </label>

              <hr style={{ margin: "30px 0" }} />

              {/* ẢNH CHÍNH */}
              {existingImages.length > 0 && (
                <>
                  <h4>Ảnh chính cũ</h4>
                  <div style={{ display: "flex", flexWrap: "wrap" }}>
                    {existingImages.map((url, i) => (
                      <ImagePreview key={i} file={url} onRemove={() => setExistingImages(prev => prev.filter((_, idx) => idx !== i))} />
                    ))}
                  </div>
                </>
              )}
              <h4>Thêm ảnh chính mới</h4>
              <input type="file" multiple accept="image/*" onChange={e => setImageFiles([...imageFiles, ...Array.from(e.target.files)])} />
              <div style={{ display: "flex", flexWrap: "wrap", marginTop: "10px" }}>
                {imageFiles.map((f, i) => <ImagePreview key={i} file={f} onRemove={() => setImageFiles(prev => prev.filter((_, idx) => idx !== i))} />)}
              </div>

              <hr style={{ margin: "30px 0" }} />

              {/* CHI TIẾT MÔ TẢ */}
              <h4>Chi tiết mô tả</h4>
              {details.map((d, i) => (
                <div key={i} style={{ border: "1px dashed #ccc", padding: "15px", margin: "15px 0", borderRadius: "8px" }}>
                  <textarea
                    placeholder="Nội dung chi tiết"
                    value={d.text || ""}
                    onChange={e => {
                      const newD = [...details];
                      newD[i].text = e.target.value;
                      setDetails(newD);
                    }}
                    style={{ width: "100%", height: "100px", padding: "10px" }}
                  />
                  {d.imageUrl && <ImagePreview file={d.imageUrl} onRemove={() => {
                    const newD = [...details];
                    newD[i].imageUrl = null;
                    setDetails(newD);
                  }} />}
                  <input type="file" accept="image/*" onChange={e => {
                    if (e.target.files[0]) {
                      const newD = [...details];
                      newD[i].imageFile = e.target.files[0];
                      setDetails(newD);
                    }
                  }} />
                  {d.imageFile && <ImagePreview file={d.imageFile} onRemove={() => {
                    const newD = [...details];
                    newD[i].imageFile = null;
                    setDetails(newD);
                  }} />}
                  <button type="button" onClick={() => setDetails(prev => prev.filter((_, idx) => idx !== i))} style={{ marginTop: "10px", background: "red", color: "white", padding: "6px 12px", border: "none" }}>
                    Xóa mục này
                  </button>
                </div>
              ))}
              <button type="button" onClick={() => setDetails(prev => [...prev, { text: "", imageFile: null, imageUrl: null }])} style={{ background: "#007bff", color: "white", padding: "10px 20px", border: "none" }}>
                + Thêm mục mô tả
              </button>

              <hr style={{ margin: "30px 0" }} />

              {/* BIẾN THỂ MÀU */}
              <h4>Biến thể màu</h4>
              {colorVariants.map((variant, i) => (
                <div key={i} style={{ border: "2px dashed #aaa", padding: "20px", margin: "20px 0", borderRadius: "12px" }}>
                  <input
                    placeholder="Tên màu (VD: Đỏ, Đen...)"
                    value={variant.color}
                    onChange={e => {
                      const v = [...colorVariants];
                      v[i].color = e.target.value;
                      setColorVariants(v);
                    }}
                    style={{ width: "100%", padding: "10px", marginBottom: "10px", fontSize: "16px" }}
                  />

                  {/* Ảnh cũ */}
                  {(variant.imageUrls || []).length > 0 && (
                    <div style={{ display: "flex", flexWrap: "wrap", marginBottom: "10px" }}>
                      {variant.imageUrls.map((url, idx) => (
                        <ImagePreview key={`old-${idx}`} file={url} onRemove={() => {
                          const v = [...colorVariants];
                          v[i].imageUrls.splice(idx, 1);
                          setColorVariants(v);
                        }} />
                      ))}
                    </div>
                  )}

                  {/* Ảnh mới */}
                  <input type="file" multiple accept="image/*" onChange={e => {
                    const files = Array.from(e.target.files);
                    const v = [...colorVariants];
                    v[i].imageFiles = [...(v[i].imageFiles || []), ...files];
                    setColorVariants(v);
                  }} />
                  <div style={{ display: "flex", flexWrap: "wrap", marginTop: "10px" }}>
                    {(variant.imageFiles || []).map((f, idx) => (
                      <ImagePreview key={`new-${idx}`} file={f} onRemove={() => {
                        const v = [...colorVariants];
                        v[i].imageFiles.splice(idx, 1);
                        setColorVariants(v);
                      }} />
                    ))}
                  </div>

                  <h5 style={{ margin: "15px 0 10px" }}>Kích thước & tồn kho</h5>
                  {variant.sizes.map((size, j) => (
                    <div key={j} style={{ display: "flex", gap: "10px", marginBottom: "10px", alignItems: "center" }}>
                      <input placeholder="Size (M, L...)" value={size.size} onChange={e => {
                        const v = [...colorVariants];
                        v[i].sizes[j].size = e.target.value;
                        setColorVariants(v);
                      }} style={{ width: "120px" }} />
                      <input type="number" placeholder="Tồn kho" value={size.stock} onChange={e => {
                        const v = [...colorVariants];
                        v[i].sizes[j].stock = parseInt(e.target.value) || 0;
                        setColorVariants(v);
                      }} style={{ width: "120px" }} />
                      <button type="button" onClick={() => {
                        const v = [...colorVariants];
                        v[i].sizes.splice(j, 1);
                        setColorVariants(v);
                      }} style={{ background: "red", color: "white", padding: "8px 12px", border: "none" }}>Xóa</button>
                    </div>
                  ))}
                  <button type="button" onClick={() => {
                    const v = [...colorVariants];
                    v[i].sizes.push({ size: "", stock: 0 });
                    setColorVariants(v);
                  }} style={{ background: "#28a745", color: "white", padding: "8px 16px", border: "none" }}>
                    + Thêm size
                  </button>

                  <button type="button" onClick={() => setColorVariants(prev => prev.filter((_, idx) => idx !== i))} style={{ background: "red", color: "white", marginLeft: "20px", padding: "10px 20px", border: "none" }}>
                    Xóa màu này
                  </button>
                </div>
              ))}

              <button type="button" onClick={() => setColorVariants(prev => [...prev, { color: "", imageFiles: [], imageUrls: [], sizes: [{ size: "", stock: 0 }] }])} style={{ background: "#007bff", color: "white", padding: "14px 28px", border: "none", borderRadius: "8px", fontSize: "16px", margin: "20px 0" }}>
                + Thêm biến thể màu mới
              </button>

              <div style={{ textAlign: "right", marginTop: "40px" }}>
                <button type="button" onClick={() => setShowModal(false)} style={{ marginRight: "20px", padding: "12px 30px", background: "#6c757d", color: "#fff", border: "none", borderRadius: "8px" }}>
                  Hủy
                </button>
                <button type="submit" style={{ padding: "14px 50px", background: "#28a745", color: "#fff", border: "none", borderRadius: "8px", fontSize: "18px", fontWeight: "bold" }}>
                  {isEdit ? "CẬP NHẬT" : "THÊM MỚI"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default DashboardPage;