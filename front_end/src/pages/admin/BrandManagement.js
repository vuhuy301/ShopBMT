import React, { useState, useEffect } from "react";
import styles from "./BrandManagement.module.css";
import { getAllBrands, addBrand, updateBrand, toggleBrandActive } from "../../services/brandService";

const BrandManagement = () => {
  const [brands, setBrands] = useState([]);
  const [name, setName] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(false); // trạng thái loading

  useEffect(() => {
    fetchBrands();
  }, []);

  const fetchBrands = async () => {
    try {
      setLoading(true);
      const data = await getAllBrands();
      setBrands(data);
    } catch (err) {
      console.error("Fetch brands thất bại", err);
    } finally {
      setLoading(false);
    }
  };

 const handleAddOrUpdate = async () => {
  if (!name.trim()) {
    alert("Tên thương hiệu không được để trống");
    return;
  }

  try {
    if (editingId) {
      const updated = await updateBrand(editingId, name);
      if (updated) {
        setBrands((prev) =>
          prev.map((b) => (b.id === editingId ? updated : b))
        );
        alert("Cập nhật thương hiệu thành công");
        setEditingId(null);
      }
    } else {
      const newBrand = await addBrand(name);
      if (newBrand) {
        setBrands((prev) => [...prev, newBrand]);
        alert("Thêm thương hiệu thành công");
      }
    }
    setName("");
  } catch (err) {
    alert(err.message); // 👈 "Thương hiệu đã tồn tại"
  }
};


  const handleEdit = (brand) => {
    setEditingId(brand.id);
    setName(brand.name);
  };

  const handleToggleActive = async (brand) => {
  if (!brand || brand.id == null) return;

  // hỏi xác nhận
  const confirmMsg = brand.isActive
    ? `Bạn có chắc muốn Ẩn thương hiệu "${brand.name}" không?`
    : `Bạn có chắc muốn Hiển thị thương hiệu "${brand.name}" không?`;

  if (!window.confirm(confirmMsg)) return;

  const updated = await toggleBrandActive(brand.id, !brand.isActive);

  if (updated) {
    // cách 1: fetch lại toàn bộ để đảm bảo dữ liệu chính xác
    await fetchBrands(); 

    // nếu bạn muốn UI nhanh hơn có thể dùng setBrands như cũ:
    // setBrands(prev => prev.map(b => b.id === updated.id ? updated : b));
  }
};


  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Quản lý thương hiệu cầu lông</h2>

      <div className={styles.form}>
        <input
          placeholder="Tên thương hiệu"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <button onClick={handleAddOrUpdate}>
          {editingId ? "Cập nhật" : "Thêm"}
        </button>
      </div>

      {loading ? (
        <p>Đang tải thương hiệu...</p>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>#</th>
              <th>Thương hiệu</th>
              <th>Số lượng sản phẩm</th>
              <th>Trạng thái</th>
              <th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            {brands.map((b, index) => (
              <tr key={b.id}>
                <td>{index + 1}</td>
                <td>{b.name}</td>
                <td>{b.productCount}</td>
                <td>{b.isActive ? "Hiển thị" : "Ẩn"}</td>
                <td>
                  <button
                    className={styles.editBtn}
                    onClick={() => handleEdit(b)}
                  >
                    Sửa
                  </button>
                  <button
                    className={styles.deleteBtn}
                    onClick={() => handleToggleActive(b)}
                  >
                    {b.isActive ? "Ẩn" : "Hiện"}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default BrandManagement;
