import React, { useState, useEffect } from "react";
import Header from "../components/Header"
import styles from "./HomePage.module.css";
import { getProducts } from "../services/productService";

const HomePage = () => {

  const [categories, setCategories] = useState([]);

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const data = await getProducts();
        setCategories(data); // Lưu dữ liệu vào state
        console.log(data);
      } catch (error) {
        console.error("Failed to load categories:", error);
      }
    };

    fetchCategories();
  }, []);

  return (
    <>
      <div className="container mt-3">
        <div className="row">

          {/* LEFT CATEGORY MENU */}
          <div className="col-md-3">
            <div className={styles.categoryMenu}>
              <h5 className={styles.categoryTitle}>Danh Mục Sản Phẩm</h5>
              <ul>
                {categories.map((category) => (
                  <li key={category.id}>{category.name}</li>
                ))}
              </ul>
            </div>
          </div>

          {/* MAIN CONTENT */}
          <div className="col-md-9">

            {/* Banner lớn */}
            <div className={styles.mainBanner}>
              <img src="https://file.hstatic.net/200000852613/file/tuyen_dung_fb__1__5df2c07130b3404ca13cb74e549cb983_1024x1024.png" alt="banner" />
            </div>

            {/* 3 Box nhỏ */}
            <div className="row g-3 mt-2 mb-3">
              <div className="col-md-4">
                <div className={styles.featureBox}>RACKET COLLECTION</div>
              </div>
              <div className="col-md-4">
                <div className={styles.featureBox}>OUTFIT</div>
              </div>
              <div className="col-md-4">
                <div className={styles.featureBox}>ACCESSORIES</div>
              </div>
            </div>

            {/* HOT PRODUCTS */}
            <div className={styles.sectionHeader}>
              <h4>🔥 Hàng Hot Bán Chạy</h4>
              <a href="/danh-muc/vot-cau-long" className={styles.viewMore}>Xem thêm →</a>
            </div>

            <div className="row g-3">
              {[1, 2, 3, 4, 5, 6, 7, 8].map((item) => (
                <div className="col-md-3" key={item}>
                  <div className={styles.productCard}>
                    <img src={`https://cdn.hstatic.net/products/200000852613/1_29d313a3d53546baa19c855057a15cee_grande.png`} alt="" />
                    <p className="mt-2 fw-bold text-center">Sản phẩm {item}</p>
                     <div className={styles.priceWrapper}>
                      <span className={styles.salePrice}>{item * 80}.000đ</span>
                      <span className={styles.originalPrice}>{item}99.000đ</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* ============ DANH MỤC 1 ============= */}
            <div className={styles.sectionHeader}>
              <h4>🏸 Vợt Cầu Lông</h4>
              <a href="/danh-muc/vot-cau-long" className={styles.viewMore}>Xem thêm →</a>
            </div>
            <div className="row g-3">
              {[1, 2, 3, 4, 5, 6, 7, 8].map((item) => (
                <div className="col-md-3" key={`vot-${item}`}>
                  <div className={styles.productCard}>
                    <img
                      src="https://cdn.hstatic.net/products/200000852613/1_29d313a3d53546baa19c855057a15cee_grande.png"
                      alt=""
                    />
                    <p className="mt-2 fw-bold text-center">Vợt cầu lông {item}</p>
                     <div className={styles.priceWrapper}>
                      <span className={styles.salePrice}>{item * 80}.000đ</span>
                      <span className={styles.originalPrice}>{item}99.000đ</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* ============ DANH MỤC 1 ============= */}
            <div className={styles.sectionHeader}>
              <h4>🏸 Giày cầu lông</h4>
              <a href="/danh-muc/vot-cau-long" className={styles.viewMore}>Xem thêm →</a>
            </div>
            <div className="row g-3">
              {[1, 2, 3, 4, 5, 6, 7, 8].map((item) => (
                <div className="col-md-3" key={`vot-${item}`}>
                  <div className={styles.productCard}>
                    <img
                      src="https://cdn.hstatic.net/products/200000852613/1_29d313a3d53546baa19c855057a15cee_grande.png"
                      alt=""
                    />
                    <p className="mt-2 fw-bold text-center">Vợt cầu lông {item}</p>
                     <div className={styles.priceWrapper}>
                      <span className={styles.salePrice}>{item * 80}.000đ</span>
                      <span className={styles.originalPrice}>{item}99.000đ</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

          </div>

        </div>
      </div>
    </>
  );
};

export default HomePage;
