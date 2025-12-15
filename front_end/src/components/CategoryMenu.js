import React from "react";
import styles from "./CategoryMenu.module.css";
import { useNavigate } from "react-router-dom";

const CategoryMenu = ({ categories }) => {
  const navigate = useNavigate();

  return (
    <div className={styles.categoryMenu}>
      <h5 className={styles.categoryTitle}>Danh Mục Sản Phẩm</h5>
      <ul>
        {categories.map((category) => (
          <li
            key={category.id}
            style={{ cursor: "pointer" }}
            onClick={() => navigate(`/category/${category.id}`)}
          >
            {category.name}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default CategoryMenu;
