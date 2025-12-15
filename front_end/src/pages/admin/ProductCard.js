import React from "react";
import styles from "./ProductAdmin.module.css";
import { useNavigate } from "react-router-dom";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const ProductCard = ({ product }) => {
  const navigate = useNavigate();

  const handleDetail = () => {
    navigate(`/admin/product/${product.id}`);
  };

  const primaryImage =
    product.images?.find((img) => img.isPrimary)?.imageUrl ||
    product.images?.[0]?.imageUrl ||
    "/no-image.jpg";

  return (
    <div className={styles.card}>
      <img src={IMAGE_BASE + primaryImage} alt={product.name} className={styles.image} />

      <h3 className={styles.name}>{product.name}</h3>

      <p className={styles.price}>
        Giá: {product.discountPrice?.toLocaleString()}₫
      </p>

      <p className={styles.stock}>
        Tồn kho: <strong>{product.stock}</strong>
      </p>

      <div className={styles.actions}>
        <button className={styles.btnEdit} >Sửa</button>
        <button className={styles.btnDetail} onClick={handleDetail} >Chi tiết</button>
      </div>
    </div>
  );
};

export default ProductCard;
