import React, { useState, useEffect } from "react";
import styles from "./BrandManagement.module.css";
import { getBrands, addBrand, updateBrand } from "../../services/brandService";

const BrandManagement = () => {
  const [brands, setBrands] = useState([]);
  const [name, setName] = useState("");
  const [editingId, setEditingId] = useState(null);

  useEffect(() => {
    fetchBrands();
  }, []);

  const fetchBrands = async () => {
    const data = await getBrands();
    setBrands(data);
  };

  const handleAddOrUpdate = async () => {
    if (!name.trim()) return;

    if (editingId) {
      const updated = await updateBrand(editingId, name);
      if (updated) {
        setBrands(brands.map(b => (b.id === editingId ? updated : b)));
        setEditingId(null);
      }
    } else {
      const newBrand = await addBrand(name);
      if (newBrand) {
        setBrands([...brands, newBrand]);
      }
    }

    setName("");
  };

  const handleEdit = (brand) => {
    setEditingId(brand.id);
    setName(brand.name);
  };


  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Quản lý thương hiệu cầu lông</h2>

      <div className={styles.form}>
        <input
          placeholder="Tên thương hiệu"
          value={name}
          onChange={e => setName(e.target.value)}
        />
        <button onClick={handleAddOrUpdate}>
          {editingId ? "Cập nhật" : "Thêm"}
        </button>
      </div>

      <table className={styles.table}>
        <thead>
          <tr>
            <th>#</th>
            <th>Thương hiệu</th>
            <th>Số lượng sản phẩm</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          {brands.map((b, index) => (
            <tr key={b.id}>
              <td>{index + 1}</td>
              <td>{b.name}</td>
              <td>{b.productCount}</td>
              <td>
                <button className={styles.editBtn} onClick={() => handleEdit(b)}>Sửa</button>
                <button className={styles.deleteBtn}>Xóa</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default BrandManagement;