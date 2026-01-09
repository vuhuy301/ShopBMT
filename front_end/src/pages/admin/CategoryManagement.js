import React, { useState, useEffect } from "react";
import styles from "./CategoryManagement.module.css";
import { getAllCategories, addCategory, updateCategory, toggleCategoryActive } from "../../services/categoryService";

const CategoryManagement = () => {
  const [categories, setCategories] = useState([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      setLoading(true);
      const data = await getAllCategories();
      setCategories(data);
    } catch (err) {
      console.error("Fetch categories thất bại", err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddOrUpdate = async () => {
  if (!name.trim()) {
    alert("Tên danh mục không được để trống");
    return;
  }

  try {
    if (editingId) {
      const updated = await updateCategory(editingId, name, description);
      setCategories((prev) =>
        prev.map((c) => (c.id === editingId ? updated : c))
      );
      alert("Cập nhật danh mục thành công");
      setEditingId(null);
    } else {
      const newCategory = await addCategory(name, description);
      setCategories((prev) => [...prev, newCategory]);
      alert("Thêm danh mục thành công");
    }

    setName("");
    setDescription("");
  } catch (err) {
    alert(err.message); // 👈 HIỂN THỊ "Danh mục đã tồn tại"
  }
};


  const handleEdit = (category) => {
    setEditingId(category.id);
    setName(category.name);
    setDescription(category.description || "");
  };

  const handleToggleActive = async (category) => {
    if (!category || category.id == null) return;

    const confirmMsg = category.isActive
      ? `Bạn có chắc muốn Ẩn danh mục "${category.name}" không?`
      : `Bạn có chắc muốn Hiển thị danh mục "${category.name}" không?`;

    if (!window.confirm(confirmMsg)) return;

    const updated = await toggleCategoryActive(category.id, !category.isActive);

    if (updated) {
      // fetch lại toàn bộ để UI đồng bộ
      await fetchCategories();
    }
  };

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Quản lý danh mục sản phẩm</h2>

      <div className={styles.form}>
        <input
          placeholder="Tên danh mục"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <input
          placeholder="Mô tả"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <button onClick={handleAddOrUpdate}>
          {editingId ? "Cập nhật" : "Thêm"}
        </button>
      </div>

      {loading ? (
        <p>Đang tải danh mục...</p>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>#</th>
              <th>Tên danh mục</th>
              <th>Mô tả</th>
              <th>Số lượng sản phẩm</th>
              <th>Trạng thái</th>
              <th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            {categories.map((c, index) => (
              <tr key={c.id}>
                <td>{index + 1}</td>
                <td>{c.name}</td>
                <td>{c.description}</td>
                <td>{c.productCount || 0}</td>
                <td>{c.isActive ? "Hiển thị" : "Ẩn"}</td>
                <td>
                  <button
                    className={styles.editBtn}
                    onClick={() => handleEdit(c)}
                  >
                    Sửa
                  </button>
                  <button
                    className={styles.deleteBtn}
                    onClick={() => handleToggleActive(c)}
                  >
                    {c.isActive ? "Ẩn" : "Hiện"}
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

export default CategoryManagement;
