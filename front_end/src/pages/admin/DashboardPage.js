// src/pages/admin/DashboardPage.js
import React, { useState, useEffect } from "react";
import {
  getAllProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  getAllBrands,
  getAllCategories,
} from "../../services/admin/productAdminService";

// Component preview ảnh
const ImagePreview = ({ file, onRemove }) => {
  const [preview, setPreview] = useState(null);

  useEffect(() => {
    if (!file) return;
    const reader = new FileReader();
    reader.onloadend = () => setPreview(reader.result);
    reader.readAsDataURL(file);
    return () => reader.abort();
  }, [file]);

  if (!file) return null;

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

  // Form state
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [discountPrice, setDiscountPrice] = useState("");
  const [brandId, setBrandId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [isFeatured, setIsFeatured] = useState(false);

  const [imageFiles, setImageFiles] = useState([]);           // ảnh chính
  const [details, setDetails] = useState([]);                 // mô tả chi tiết
  const [colorVariants, setColorVariants] = useState([]);     // danh sách màu

  // Load dữ liệu lần đầu
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
    setName("");
    setDescription("");
    setPrice("");
    setDiscountPrice("");
    setBrandId("");
    setCategoryId("");
    setIsFeatured(false);
    setImageFiles([]);
    setDetails([]);
    setColorVariants([]);
    setIsEdit(false);
    setCurrentId(null);
  };

  const openAdd = () => {
    resetForm();
    setShowModal(true);
  };

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

  const handleSubmit = async (e) => {
    e.preventDefault();
    const formData = new FormData();

    // Thông tin cơ bản
    formData.append("Name", name.trim());
    if (description.trim()) formData.append("Description", description.trim());
    formData.append("Price", price);
    if (discountPrice) formData.append("DiscountPrice", discountPrice);
    formData.append("BrandId", brandId);
    formData.append("CategoryId", categoryId);
    formData.append("IsFeatured", isFeatured.toString());

    // Ảnh chính sản phẩm
    imageFiles.forEach((file) => formData.append("ImageFiles", file));

    // Chi tiết mô tả
    details.forEach((d, i) => {
      formData.append(`Details[${i}].Text`, d.text || "");
      formData.append(`Details[${i}].SortOrder`, (i + 1).toString());
      if (d.imageFile) formData.append(`Details[${i}].ImageFile`, d.imageFile);
    });

    // Biến thể màu - ĐÚNG 100% với backend
    colorVariants.forEach((variant, i) => {
      formData.append(`ColorVariants[${i}].Color`, variant.color?.trim() || "");

      // Nhiều ảnh cho 1 màu
      if (variant.imageFiles && variant.imageFiles.length > 0) {
        variant.imageFiles.forEach((file) =>
          formData.append(`ColorVariants[${i}].ImageFiles`, file)
        );
      }

      // Danh sách size
      if (variant.sizes && variant.sizes.length > 0) {
        variant.sizes.forEach((size, j) => {
          formData.append(`ColorVariants[${i}].Sizes[${j}].Size`, size.size?.trim() || "");
          formData.append(`ColorVariants[${i}].Sizes[${j}].Stock`, size.stock?.toString() || "0");
          if (size.price && size.price > 0) {
            formData.append(`ColorVariants[${i}].Sizes[${j}].Price`, size.price.toString());
          }
        });
      }
    });

    try {
      if (isEdit) {
        await updateProduct(currentId, formData);
        alert("Cập nhật thành công!");
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
          cursor: "pointer",
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
                margin: "12px 0",
                borderRadius: "10px",
                background: "#f9f9f9",
              }}
            >
              <strong>{p.name}</strong> ({p.brandName} - {p.categoryName})
              <div style={{ fontSize: "14px", color: "#555" }}>
                Giá: {Number(p.price).toLocaleString("vi-VN")}₫
                {p.discountPrice && (
                  <span style={{ color: "red", marginLeft: "10px" }}>
                    → {Number(p.discountPrice).toLocaleString("vi-VN")}₫
                  </span>
                )}
                <br />
                Tồn kho: {p.stock} {p.isFeatured && <span style={{ color: "orange" }}> [Nổi bật]</span>}
              </div>
              <div style={{ marginTop: "10px" }}>
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

      {/* MODAL THÊM/SỬA */}
      {showModal && (
        <div
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.75)",
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
              width: "1100px",
              maxHeight: "90vh",
              overflowY: "auto",
            }}
          >
            <h2 style={{ margin: 0 }}>
              {isEdit ? "Sửa sản phẩm" : "Thêm sản phẩm mới"}
            </h2>

            <form onSubmit={handleSubmit} style={{ marginTop: "20px" }}>
              {/* Các field cơ bản */}
              <input
                placeholder="Tên sản phẩm *"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                style={{ width: "100%", padding: "12px", margin: "10px 0", borderRadius: "6px", border: "1px solid #ccc" }}
              />
              <textarea
                placeholder="Mô tả"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                style={{ width: "100%", padding: "12px", height: "100px", borderRadius: "6px", border: "1px solid #ccc", margin: "10px 0" }}
              />

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
                <input type="number" placeholder="Giá gốc *" value={price} onChange={(e) => setPrice(e.target.value)} required />
                <input type="number" placeholder="Giá giảm" value={discountPrice} onChange={(e) => setDiscountPrice(e.target.value)} />
                <select value={brandId} onChange={(e) => setBrandId(e.target.value)} required>
                  <option value="">Chọn thương hiệu</option>
                  {brands.map((b) => (
                    <option key={b.id} value={b.id}>
                      {b.name}
                    </option>
                  ))}
                </select>
                <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
                  <option value="">Chọn danh mục</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>

              <label style={{ display: "block", margin: "15px 0" }}>
                <input type="checkbox" checked={isFeatured} onChange={(e) => setIsFeatured(e.target.checked)} /> Sản phẩm nổi bật
              </label>

              <hr />

              <h4>Ảnh chính (nhiều ảnh)</h4>
              <input
                type="file"
                multiple
                accept="image/*"
                onChange={(e) => setImageFiles([...e.target.files])}
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

              <h4 style={{ marginTop: "30px" }}>Chi tiết mô tả</h4>
              {details.map((d, i) => (
                <div key={i} style={{ border: "1px dashed #ccc", padding: "15px", margin: "10px 0", borderRadius: "8px" }}>
                  <input
                    placeholder="Nội dung"
                    value={d.text || ""}
                    onChange={(e) => {
                      const nd = [...details];
                      nd[i].text = e.target.value;
                      setDetails(nd);
                    }}
                   style={{ 
                    width: "100%",       // giữ nguyên rộng 100%
                    padding: "10px",     // padding bên trong
                    height: "120px",     // tăng chiều cao (có thể thay đổi tùy ý)
                    fontSize: "16px",    // tăng cỡ chữ cho dễ nhìn
                    boxSizing: "border-box"  // để padding không làm tràn kích thước
                  }}
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
                  <button type="button" onClick={() => setDetails((prev) => prev.filter((_, idx) => idx !== i))} style={{ color: "red" }}>
                    Xóa đoạn
                  </button>
                </div>
              ))}
              <button type="button" onClick={() => setDetails([...details, { text: "", imageFile: null }])}>
                + Thêm đoạn mô tả
              </button>

              <h4 style={{ marginTop: "40px" }}>Biến thể màu</h4>
              {colorVariants.map((variant, i) => (
                <div
                  key={i}
                  style={{
                    border: "2px dashed #007bff",
                    padding: "20px",
                    margin: "20px 0",
                    borderRadius: "12px",
                    background: "#f0f8ff",
                  }}
                >
                  <input
                    placeholder="Tên màu (Đỏ, Xanh, Trắng...)"
                    value={variant.color || ""}
                    onChange={(e) => {
                      const nv = [...colorVariants];
                      nv[i].color = e.target.value;
                      setColorVariants(nv);
                    }}
                    style={{ width: "100%", padding: "12px", fontWeight: "bold" }}
                  />

                  <p style={{ margin: "10px 0 5px 0" }}>
                    <strong>Ảnh của màu này:</strong>
                  </p>
                  <input
                    type="file"
                    multiple
                    accept="image/*"
                    onChange={(e) => {
                      const nv = [...colorVariants];
                      nv[i].imageFiles = [...(nv[i].imageFiles || []), ...e.target.files];
                      setColorVariants(nv);
                    }}
                  />
                  <div style={{ display: "flex", flexWrap: "wrap" }}>
                    {variant.imageFiles?.map((f, j) => (
                      <ImagePreview
                        key={j}
                        file={f}
                        onRemove={() => {
                          const nv = [...colorVariants];
                          nv[i].imageFiles.splice(j, 1);
                          setColorVariants(nv);
                        }}
                      />
                    ))}
                  </div>

                  <h5 style={{ margin: "15px 0" }}>Size & Tồn kho</h5>
                  {variant.sizes?.map((size, j) => (
                    <div
                      key={j}
                      style={{
                        display: "grid",
                        gridTemplateColumns: "2fr 1fr 1fr auto",
                        gap: "10px",
                        marginBottom: "8px",
                      }}
                    >
                      <input
                        placeholder="Size"
                        value={size.size || ""}
                        onChange={(e) => {
                          const nv = [...colorVariants];
                          nv[i].sizes[j].size = e.target.value;
                          setColorVariants(nv);
                        }}
                      />
                      <input
                        type="number"
                        placeholder="Tồn kho"
                        value={size.stock || ""}
                        onChange={(e) => {
                          const nv = [...colorVariants];
                          nv[i].sizes[j].stock = parseInt(e.target.value) || 0;
                          setColorVariants(nv);
                        }}
                      />
                      <input
                        type="number"
                        placeholder="Giá riêng (tùy chọn)"
                        value={size.price || ""}
                        onChange={(e) => {
                          const nv = [...colorVariants];
                          nv[i].sizes[j].price = e.target.value ? parseFloat(e.target.value) : null;
                          setColorVariants(nv);
                        }}
                      />
                      <button
                        type="button"
                        style={{ background: "#dc3545", color: "#fff" }}
                        onClick={() => {
                          const nv = [...colorVariants];
                          nv[i].sizes.splice(j, 1);
                          setColorVariants(nv);
                        }}
                      >
                        Xóa
                      </button>
                    </div>
                  ))}
                  <button
                    type="button"
                    onClick={() => {
                      const nv = [...colorVariants];
                      if (!nv[i].sizes) nv[i].sizes = [];
                      nv[i].sizes.push({ size: "", stock: 0, price: null });
                      setColorVariants(nv);
                    }}
                  >
                    + Thêm size
                  </button>

                  <button
                    type="button"
                    style={{ color: "red", marginTop: "20px", display: "block" }}
                    onClick={() => setColorVariants((prev) => prev.filter((_, idx) => idx !== i))}
                  >
                    Xóa màu này
                  </button>
                </div>
              ))}

              <button
                type="button"
                onClick={() =>
                  setColorVariants([...colorVariants, { color: "", imageFiles: [], sizes: [] }])
                }
                style={{ margin: "20px 0" }}
              >
                + Thêm một màu mới
              </button>

              <div style={{ marginTop: "50px", textAlign: "right" }}>
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  style={{
                    marginRight: "20px",
                    padding: "12px 24px",
                    background: "#6c757d",
                    color: "#fff",
                    borderRadius: "8px",
                  }}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  style={{
                    padding: "14px 40px",
                    background: "#28a745",
                    color: "#fff",
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