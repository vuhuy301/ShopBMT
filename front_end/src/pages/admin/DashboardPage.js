
// export default DashboardPage;
// src/pages/admin/DashboardPage.jsx
import React, { useState, useEffect } from "react";
import {
  getAllProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  getAllBrands,
  getAllCategories,
} from "../../services/admin/productAdminService";

// Component preview ảnh + nút xóa
const ImagePreview = ({ file, onRemove }) => {
  const [preview, setPreview] = useState(null);

  useEffect(() => {
    if (!file) return;
    const reader = new FileReader();
    reader.onloadend = () => setPreview(reader.result);
    reader.readAsDataURL(file);
  }, [file]);

  if (!file) return null;

  return (
    <div style={{ position: "relative", display: "inline-block", margin: "8px" }}>
      <img
        src={preview}
        alt="preview"
        style={{ width: "120px", height: "120px", objectFit: "cover", borderRadius: "8px", border: "2px solid #ddd" }}
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
          fontSize: "16px",
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

  // Form state
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [discountPrice, setDiscountPrice] = useState("");
  const [brandId, setBrandId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [isFeatured, setIsFeatured] = useState(false);

  const [imageFiles, setImageFiles] = useState([]); // File[]
  const [details, setDetails] = useState([]);       // { text: string, imageFile: File|null }
  const [variants, setVariants] = useState([]);     // { size, color, stock, imageFile }

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
    setImageFiles([]); setDetails([]); setVariants([]);
  };

  const openAdd = () => { resetForm(); setIsEdit(false); setShowModal(true); };

  const openEdit = (p) => {
    resetForm();
    setIsEdit(true);
    setCurrentId(p.id);
    setName(p.name || "");
    setDescription(p.description || "");
    setPrice(p.price?.toString() || "");
    setDiscountPrice(p.discountPrice?.toString() || "");
    setBrandId(p.brandId?.toString() || "");
    setCategoryId(p.categoryId?.toString() || "");
    setIsFeatured(p.isFeatured || false);
    setShowModal(true);
  };

  // QUAN TRỌNG NHẤT: GỬI ĐÚNG TÊN FIELD PASCALCASE CHO .NET
  const handleSubmit = async (e) => {
    e.preventDefault();
    const formData = new FormData();

    formData.append("Name", name.trim());
    if (description) formData.append("Description", description.trim());
    formData.append("Price", price);
    if (discountPrice) formData.append("DiscountPrice", discountPrice);
    formData.append("BrandId", brandId);
    formData.append("CategoryId", categoryId);
    formData.append("IsFeatured", isFeatured.toString());

    // Ảnh chính
    imageFiles.forEach((f) => formData.append("ImageFiles", f));

    // Chi tiết mô tả
    details.forEach((d, i) => {
      formData.append(`Details[${i}].Text`, d.text || "");
      formData.append(`Details[${i}].SortOrder`, (i + 1).toString());
      if (d.imageFile) formData.append(`Details[${i}].ImageFile`, d.imageFile);
    });

    // Biến thể
    variants.forEach((v, i) => {
      formData.append(`Variants[${i}].Size`, v.size || "");
      formData.append(`Variants[${i}].Color`, v.color || "");
      formData.append(`Variants[${i}].Stock`, v.stock?.toString() || "0");
      formData.append(`Variants[${i}].Price`, price);
      if (v.discountPrice) formData.append(`Variants[${i}].DiscountPrice`, v.discountPrice);
      if (v.imageFile) formData.append(`Variants[${i}].ImageFile`, v.imageFile);
    });

    try {
      isEdit
        ? await updateProduct(currentId, formData)
        : await createProduct(formData);

      alert(isEdit ? "Cập nhật thành công!" : "Thêm sản phẩm thành công!");
      setShowModal(false);
      const prods = await getAllProducts();
      setProducts(prods);
    } catch (err) {
      console.error(err);
      alert("Lỗi: " + (err.message || "Không thể lưu sản phẩm"));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa sản phẩm này thật chứ?")) return;
    try {
      await deleteProduct(id);
      setProducts((prev) => prev.filter((p) => p.id !== id));
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
        }}
      >
        + Thêm sản phẩm mới
      </button>

      {loading ? (
        <p>Đang tải...</p>
      ) : (
        <div style={{ marginTop: "20px" }}>
          <h3>Danh sách sản phẩm ({products.length})</h3>
          {products.map((p) => (
            <div
              key={p.id}
              style={{
                border: "1px solid #eee",
                padding: "15px",
                margin: "10px 0",
                borderRadius: "10px",
                display: "flex",
                justifyContent: "space-between",
                background: "#fafafa",
              }}
            >
              <div>
                <strong>{p.name}</strong>{" "}
                <small>
                  ({p.brandName} - {p.categoryName})
                </small>
                <div style={{ fontSize: "14px", color: "#555" }}>
                  Giá: {Number(p.price).toLocaleString()}₫
                  {p.discountPrice && (
                    <span style={{ color: "red", marginLeft: "8px" }}>
                      → {Number(p.discountPrice).toLocaleString()}₫
                    </span>
                  )}
                  <br />
                  Tồn kho: {p.stock} {p.isFeatured && " [Nổi bật]"}
                </div>
              </div>
              <div>
                <button
                  onClick={() => openEdit(p)}
                  style={{
                    marginRight: "10px",
                    padding: "8px 16px",
                    background: "#007bff",
                    color: "#fff",
                    border: "none",
                    borderRadius: "6px",
                  }}
                >
                  Sửa
                </button>
                <button
                  onClick={() => handleDelete(p.id)}
                  style={{
                    padding: "8px 16px",
                    background: "#dc3545",
                    color: "#fff",
                    border: "none",
                    borderRadius: "6px",
                  }}
                >
                  Xóa
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal */}
      {showModal && (
        <div
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.8)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 999,
          }}
        >
          <div
            style={{
              background: "#fff",
              padding: "30px",
              borderRadius: "12px",
              width: "1000px",
              maxHeight: "90vh",
              overflowY: "auto",
            }}
          >
            <h2 style={{ marginTop: 0 }}>
              {isEdit ? "Sửa sản phẩm" : "Thêm sản phẩm mới"}
            </h2>
            <form onSubmit={handleSubmit}>
              <input
                placeholder="Tên sản phẩm *"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                style={{
                  width: "100%",
                  padding: "12px",
                  margin: "10px 0",
                  borderRadius: "6px",
                  border: "1px solid #ddd",
                }}
              />

              <textarea
                placeholder="Mô tả"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                style={{
                  width: "100%",
                  padding: "12px",
                  height: "100px",
                  borderRadius: "6px",
                  border: "1px solid #ddd",
                  margin: "10px 0",
                }}
              />

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
                <input
                  type="number"
                  placeholder="Giá gốc *"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  required
                  style={{ padding: "12px" }}
                />
                <input
                  type="number"
                  placeholder="Giá giảm"
                  value={discountPrice}
                  onChange={(e) => setDiscountPrice(e.target.value)}
                  style={{ padding: "12px" }}
                />
                <select
                  value={brandId}
                  onChange={(e) => setBrandId(e.target.value)}
                  required
                  style={{ padding: "12px" }}
                >
                  <option value="">Chọn thương hiệu</option>
                  {brands.map((b) => (
                    <option key={b.id} value={b.id}>
                      {b.name}
                    </option>
                  ))}
                </select>
                <select
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  required
                  style={{ padding: "12px" }}
                >
                  <option value="">Chọn danh mục</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <label style={{ display: "block", margin: "15px 0" }}>
                <input
                  type="checkbox"
                  checked={isFeatured}
                  onChange={(e) => setIsFeatured(e.target.checked)}
                />{" "}
                Sản phẩm nổi bật
              </label>

              <hr style={{ margin: "25px 0" }} />

              <h4>Ảnh chính (nhiều ảnh)</h4>
              <input
                type="file"
                multiple
                accept="image/*"
                onChange={(e) =>
                  setImageFiles([...imageFiles, ...Array.from(e.target.files)])
                }
              />
              <div style={{ display: "flex", flexWrap: "wrap", marginTop: "10px" }}>
                {imageFiles.map((f, i) => (
                  <ImagePreview
                    key={i}
                    file={f}
                    onRemove={() => setImageFiles((prev) => prev.filter((_, idx) => idx !== i))}
                  />
                ))}
              </div>

              <h4 style={{ marginTop: "25px" }}>Chi tiết mô tả</h4>
              {details.map((d, i) => (
                <div
                  key={i}
                  style={{
                    border: "2px dashed #ccc",
                    padding: "15px",
                    borderRadius: "10px",
                    margin: "15px 0",
                  }}
                >
                  <input
                    placeholder="Nội dung mô tả"
                    value={d.text || ""}
                    onChange={(e) => {
                      const nd = [...details];
                      nd[i].text = e.target.value;
                      setDetails(nd);
                    }}
                    style={{ width: "100%", padding: "10px", marginBottom: "10px" }}
                  />
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => {
                      const nd = [...details];
                      nd[i].imageFile = e.target.files[0] || null;
                      setDetails(nd);
                    }}
                  />
                  {d.imageFile && (
                    <ImagePreview
                      file={d.imageFile}
                      onRemove={() => {
                        const nd = [...details];
                        nd[i].imageFile = null;
                        setDetails(nd);
                      }}
                    />
                  )}
                  <button
                    type="button"
                    onClick={() => setDetails((prev) => prev.filter((_, idx) => idx !== i))}
                    style={{ color: "red", marginTop: "10px" }}
                  >
                    Xóa đoạn
                  </button>
                </div>
              ))}
              <button
                type="button"
                onClick={() => setDetails([...details, { text: "", imageFile: null }])}
                style={{
                  padding: "10px 20px",
                  background: "#007bff",
                  color: "#fff",
                  border: "none",
                  borderRadius: "6px",
                }}
              >
                + Thêm đoạn mô tả
              </button>

              <h4 style={{ marginTop: "30px" }}>Biến thể (Size + Màu)</h4>
              {variants.map((v, i) => (
                <div
                  key={i}
                  style={{
                    border: "2px dashed #999",
                    padding: "15px",
                    borderRadius: "10px",
                    margin: "15px 0",
                  }}
                >
                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "10px" }}>
                    <input
                      placeholder="Size (3U/ 4u)"
                      value={v.size || ""}
                      onChange={(e) => {
                        const nv = [...variants];
                        nv[i].size = e.target.value;
                        setVariants(nv);
                      }}
                    />
                    <input
                      placeholder="Màu (DO , Xanh)"
                      value={v.color || ""}
                      onChange={(e) => {
                        const nv = [...variants];
                        nv[i].color = e.target.value;
                        setVariants(nv);
                      }}
                    />
                    <input
                      type="number"
                      placeholder="Số lượng"
                      value={v.stock || ""}
                      onChange={(e) => {
                        const nv = [...variants];
                        nv[i].stock = e.target.value;
                        setVariants(nv);
                      }}
                    />
                  </div>

                  {/* ĐÃ SỬA DÒNG LỖI TẠI ĐÂY */}
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => {
                      const nv = [...variants];
                      nv[i].imageFile = e.target.files[0] || null;
                      setVariants(nv);
                    }}
                    style={{ margin: "10px 0" }}
                  />

                  {v.imageFile && (
                    <ImagePreview
                      file={v.imageFile}
                      onRemove={() => {
                        const nv = [...variants];
                        nv[i].imageFile = null;
                        setVariants(nv);
                      }}
                    />
                  )}
                  <button
                    type="button"
                    onClick={() => setVariants((prev) => prev.filter((_, idx) => idx !== i))}
                    style={{ color: "red" }}
                  >
                    Xóa biến thể
                  </button>
                </div>
              ))}
              <button
                type="button"
                onClick={() =>
                  setVariants([...variants, { size: "", color: "", stock: 0, imageFile: null }])
                }
                style={{
                  padding: "10px 20px",
                  background: "#28a745",
                  color: "#fff",
                  border: "none",
                  borderRadius: "6px",
                }}
              >
                + Thêm biến thể
              </button>

              <div style={{ marginTop: "40px", textAlign: "right" }}>
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  style={{
                    marginRight: "15px",
                    padding: "12px 24px",
                    background: "#6c757d",
                    color: "#fff",
                    border: "none",
                    borderRadius: "8px",
                  }}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  style={{
                    padding: "14px 32px",
                    background: "#28a745",
                    color: "#fff",
                    border: "none",
                    borderRadius: "8px",
                    fontSize: "16px",
                  }}
                >
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