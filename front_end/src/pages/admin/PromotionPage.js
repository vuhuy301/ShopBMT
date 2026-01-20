import React, { useEffect, useState } from "react";
import styles from "./PromotionPage.module.css";
import {
  getAllPromotions,
  createPromotion,
  updatePromotion,
  deletePromotion,
} from "../../services/admin/promotionService";

export default function PromotionPage() {
  const [promotions, setPromotions] = useState([]);
  const [formData, setFormData] = useState({ name: "", description: "" });
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");

  const fetchPromotions = async () => {
    try {
      const data = await getAllPromotions();
      setPromotions(data);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchPromotions();
  }, []);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    try {
      if (editingId) {
        await updatePromotion({ id: editingId, ...formData });
      } else {
        await createPromotion(formData);
      }
      setFormData({ name: "", description: "" });
      setEditingId(null);
      fetchPromotions();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleEdit = (promotion) => {
    setFormData({ name: promotion.name, description: promotion.description });
    setEditingId(promotion.id);
    setError("");
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Bạn có chắc muốn xóa ưu đãi này?")) return;
    try {
      await deletePromotion(id);
      fetchPromotions();
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Quản lý Ưu đãi</h2>

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>{error && <div className={styles.error}>{error}</div>}</div>
        <div className={styles.textBtnForm}><input
          type="text"
          name="name"
          placeholder="Tên ưu đãi"
          value={formData.name}
          onChange={handleChange}
          className={styles.input}
        />
        <input
          name="description"
          placeholder="Mô tả"
          value={formData.description}
          onChange={handleChange}
           className={styles.input}
        />
        <button type="submit" className={styles.button}>
          {editingId ? "Cập nhật" : "Thêm"}
        </button></div>
      </form>

      <table className={styles.table}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Tên</th>
            <th>Mô tả</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          {promotions.map((p) => (
            <tr key={p.id}>
              <td>{p.id}</td>
              <td>{p.name}</td>
              <td>{p.description}</td>
              <td>
                <button className={styles.edit} onClick={() => handleEdit(p)}>
                  Sửa
                </button>
                <button
                  className={styles.delete}
                  onClick={() => handleDelete(p.id)}
                >
                  Xóa
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
