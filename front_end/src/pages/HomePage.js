import React, { useState, useEffect } from "react";
// import Header from "../components/Header";
import styles from "./HomePage.module.css";
import { useNavigate } from "react-router-dom";
import { getCategories } from "../services/categoryService";
import { getTopNewProductsByCategory } from "../services/productService";
import CategoryMenu from "../components/CategoryMenu";
import ChatBot from "../components/ChatBot";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;
const HomePage = () => {
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [productsByCategory, setProductsByCategory] = useState({}); // lưu sản phẩm theo categoryId

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const data = await getCategories();
        setCategories(data);
      } catch (error) {
        console.error("Failed to load categories:", error);
      }
    };
    fetchCategories();
  }, []);

  useEffect(() => {
    const fetchProducts = async () => {
      const categoryProducts = {};
      for (const category of categories) {
        const products = await getTopNewProductsByCategory(category.id);
        categoryProducts[category.id] = products;
      }
      setProductsByCategory(categoryProducts);
    };

    if (categories.length > 0) fetchProducts();
  }, [categories]);

  return (
    <>
      <div className="container mt-3">
        <div className="row">

          {/* LEFT CATEGORY MENU */}
          <div className="col-md-3">
             <CategoryMenu categories={categories} />
          </div>

          {/* MAIN CONTENT */}
          <div className="col-md-9">

            {/* Banner lớn */}
            <div className={styles.mainBanner}>
              <img
                src="https://file.hstatic.net/200000852613/file/tuyen_dung_fb__1__5df2c07130b3404ca13cb74e549cb983_1024x1024.png"
                alt="banner"
              />
            </div>

            {/* 3 Box nhỏ */}
            <div className="row g-3 mt-2 mb-3">
              <div className="col-md-4">
                <div className={styles.featureBox}>Vận chuyển TOÀN QUỐC <br></br>
Thanh toán khi nhận hàng</div>
              </div>
              <div className="col-md-4">
                <div className={styles.featureBox}>Bảo đảm chất lượng<br></br>
Sản phẩm bảo đảm chất lượng.</div>
              </div>
              <div className="col-md-4">
                <div className={styles.featureBox}>Tiến hành THANH TOÁN<br></br>
Với nhiều PHƯƠNG THỨC</div>
              </div>
            </div>

            {/* ==== Hiển thị sản phẩm theo từng category ==== */}
            {categories.map((category) => (
              <div key={category.id} className="mb-4">
                <div className={styles.sectionHeader}>
                  <h4>🏸 {category.name}</h4>
                  <a onClick={() => navigate(`/category/${category.id}`)} style={{ cursor: "pointer" }} className={styles.viewMore}>
                    Xem thêm →
                  </a>
                </div>

                <div className="row g-3">
                  {productsByCategory[category.id]?.map((product) => (
                    <div className="col-md-3" key={product.id}>
                      <div
                        className={styles.productCard}
                        style={{ cursor: "pointer" }}
                        onClick={() => navigate(`/product/${product.id}`)}
                      >
                        <img
                          src={
                            IMAGE_BASE +
                            (
                              product.images.find(img => img.isPrimary)?.imageUrl ||
                              product.images[0]?.imageUrl
                            )
                          }
                          alt={product.name}
                        />
                        <p className="mt-2 fw-bold text-center">{product.name}</p>
                        <div className={styles.priceWrapper}>
                          <span className={styles.salePrice}>
                            {product.discountPrice
                              ? product.discountPrice.toLocaleString() + "đ"
                              : product.price.toLocaleString() + "đ"}
                          </span>
                          {product.discountPrice && (
                            <span className={styles.originalPrice}>
                              {product.price.toLocaleString()}đ
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}

          </div>

        </div>
      </div>
      <ChatBot />
    </>
  );
};

export default HomePage;
