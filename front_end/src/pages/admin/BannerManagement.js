import React, { useState, useEffect } from "react";
import styles from "./BannerManager.module.css";
import { createBanner, getAllBanners, updateBanner } from "../../services/bannerService";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;
const BannerManager = () => {
  const [banners, setBanners] = useState([]);
  const [showAddModal, setShowAddModal] = useState(false);
  const [editBanner, setEditBanner] = useState(null);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const [form, setForm] = useState({
    imageFile: null,
    imageUrl: "",
    isActive: true,
  });


  const loadBanners = async () => {
    try {
      const data = await getAllBanners();
      setBanners(data);
    } catch (err) {
      alert(err.message);
    }
  };

  useEffect(() => {
    loadBanners();
  }, []);



  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    setForm({ ...form, imageFile: file });
  };

  const handleAddSubmit = async (e) => {
    e.preventDefault();

    if (!validateAddBanner()) return; // 👈 THÊM DÒNG NÀY

    setLoading(true);
    try {
      await createBanner(form);
      setShowAddModal(false);
      setForm({ imageFile: null, link: "", isActive: true });
      setErrors({});
      loadBanners();
    } catch (err) {
      alert(err.message);
    } finally {
      setLoading(false);
    }
  };


  const handleEditSubmit = async (e) => {
    e.preventDefault();
    if (!editBanner) return;

    setLoading(true);
    try {
      await updateBanner({ id: editBanner.id, ...form });
      setEditBanner(null);
      setForm({ imageFile: null, link: "", isActive: true });
      loadBanners();
    } catch (err) { alert(err.message); }
    finally { setLoading(false); }
  };

  const validateAddBanner = () => {
    const newErrors = {};

    // 1. Bắt buộc có ảnh
    if (!form.imageFile) {
      newErrors.imageFile = "Vui lòng chọn ảnh banner";
    } else {
      // 2. Validate loại file
      const allowedTypes = ["image/jpeg", "image/png", "image/jpg", "image/webp"];
      if (!allowedTypes.includes(form.imageFile.type)) {
        newErrors.imageFile = "Chỉ chấp nhận ảnh JPG, PNG, WEBP";
      }

      // 3. Validate dung lượng (2MB)
      const maxSize = 50 * 1024 * 1024;
      if (form.imageFile.size > maxSize) {
        newErrors.imageFile = "Dung lượng ảnh tối đa 50MB";
      }
    }

    // 4. Validate link (nếu có)
    if (form.link && !/^https?:\/\/.+/i.test(form.link)) {
      newErrors.link = "Link không hợp lệ (phải bắt đầu bằng http:// hoặc https://)";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };


  return (
    <div className={styles.container}>
      <h4 className="mb-4">Quản lý Banner</h4>
      <button className="btn btn-success mb-3" onClick={() => setShowAddModal(true)}>+ Thêm banner</button>

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
              <td><img src={IMAGE_BASE + b.imageUrl} alt="banner" /></td>
              <td>{b.link}</td>
              <td>{b.isActive ? "Bật" : "Tắt"}</td>
              <td>
                <button className="btn btn-primary" onClick={() => {
                  setEditBanner(b);
                  setForm({ imageFile: null, link: b.link || "", isActive: b.isActive });
                }}>
                  Chỉnh sửa
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Modal thêm banner */}
      {showAddModal && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h3>Thêm banner</h3>
            <form onSubmit={handleAddSubmit}>
              <input type="file" accept="image/*" onChange={handleFileChange} />
              {errors.imageFile && (
                <div className="text-danger" style={{ fontSize: "13px" }}>
                  {errors.imageFile}
                </div>
              )}
              <input
                type="text"
                placeholder="Link ảnh (tùy chọn)"
                value={form.link}
                onChange={e => setForm({ ...form, link: e.target.value })}
              />
              {errors.link && (
                <div className="text-danger" style={{ fontSize: "13px" }}>
                  {errors.link}
                </div>
              )}

              <label className={styles.checkbox}>
                <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
                Hiển thị
              </label>
              <div className={styles.actions}>
                <button className="btn btn-primary" type="submit" disabled={loading}>{loading ? "Đang lưu..." : "Lưu"}</button>
                <button className="btn btn-danger" type="button" onClick={() => setShowAddModal(false)}>Hủy</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal edit banner */}
      {editBanner && (
        <div className={styles.modalOverlay}>
          <div className={styles.modal}>
            <h3>Chỉnh sửa banner</h3>
            <form onSubmit={handleEditSubmit}>
              <div>
                <input type="file" accept="image/*" onChange={handleFileChange} />
                <div style={{ fontSize: "12px" }}>Bỏ trống nếu không đổi ảnh</div>
              </div>
              <input
                type="text"
                placeholder="Link ảnh"
                value={form.link}
                onChange={e => setForm({ ...form, link: e.target.value })}
              />
              <label className={styles.checkbox}>
                <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
                Hiển thị
              </label>
              <div className={styles.actions}>
                <button className="btn btn-primary" type="submit" disabled={loading}>{loading ? "Đang lưu..." : "Lưu"}</button>
                <button className="btn btn-danger" type="button" onClick={() => setEditBanner(null)}>Hủy</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default BannerManager;