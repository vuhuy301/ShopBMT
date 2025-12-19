import React, { useEffect, useState } from "react";
import styles from "./BannerManager.module.css";

const BannerManager = () => {
  const [banners, setBanners] = useState([]);
  const [form, setForm] = useState({
    id: null,
    imageUrl: "",
    link: "",
    isActive: true,
  });

  useEffect(() => {
    // Mock data (sau thay bằng API)
    setBanners([
      {
        id: 1,
        imageUrl: "https://via.placeholder.com/300x120",
        link: "https://example.com",
        isActive: true,
      },
      {
        id: 2,
        imageUrl: "https://via.placeholder.com/300x120",
        link: "https://google.com",
        isActive: false,
      },
    ]);
  }, []);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({
      ...form,
      [name]: type === "checkbox" ? checked : value,
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (form.id) {
      setBanners(banners.map(b => (b.id === form.id ? form : b)));
    } else {
      setBanners([...banners, { ...form, id: Date.now() }]);
    }

    setForm({ id: null, imageUrl: "", link: "", isActive: true });
  };

  const handleEdit = (banner) => {
    setForm(banner);
  };

  const handleDelete = (id) => {
    if (window.confirm("Xóa banner này?")) {
      setBanners(banners.filter(b => b.id !== id));
    }
  };

  return (
    <div className={styles.container}>
      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          type="text"
          name="imageUrl"
          placeholder="Đường dẫn ảnh"
          value={form.imageUrl}
          onChange={handleChange}
          required
        />

        <input
          type="text"
          name="link"
          placeholder="Link khi click"
          value={form.link}
          onChange={handleChange}
        />

        <label className={styles.checkbox}>
          <input
            type="checkbox"
            name="isActive"
            checked={form.isActive}
            onChange={handleChange}
          />
          Hiển thị
        </label>

        <button type="submit">
          {form.id ? "Cập nhật" : "Thêm banner"}
        </button>
      </form>

      <table className={styles.table}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Ảnh</th>
            <th>Link</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          {banners.map(b => (
            <tr key={b.id}>
              <td>{b.id}</td>
              <td>
                <img src={b.imageUrl} alt="banner" />
              </td>
              <td>{b.link}</td>
              <td>{b.isActive ? "Bật" : "Tắt"}</td>
              <td>
                <button onClick={() => handleEdit(b)}>Sửa</button>
                <button
                  className={styles.delete}
                  onClick={() => handleDelete(b.id)}
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
};

export default BannerManager;
