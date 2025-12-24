import React from "react";
import styles from "./CategoryMenu.module.css";
import { useNavigate, useLocation  } from "react-router-dom";

const CategoryMenu = ({ categories }) => {
  const navigate = useNavigate();
  const location = useLocation();

return (
    <div className={styles.categoryMenu}>
      <h5 className={styles.categoryTitle}>Danh Mục Sản Phẩm</h5>
      <ul>
        {categories.map((category) => {
          const isActive = location.pathname === `/category/${category.id}`;

          return (
            <li
              key={category.id}
              onClick={() => navigate(`/category/${category.id}`)}
              className={`${styles.categoryItem} ${
                isActive ? styles.active : ""
              }`}
            >
              {category.name}
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default CategoryMenu;
