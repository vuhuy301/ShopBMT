import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import styles from "./ProductList.module.css";
import { getProducts } from "../services/productService";
import { getCategories } from "../services/categoryService";
import CategoryMenu from "../components/CategoryMenu";
import Breadcrumb from "../components/Breadcrumb";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const priceOptions = [
  { label: "Tất cả", min: 0, max: Infinity },
  { label: "Dưới 1 triệu", min: 0, max: 1000000 },
  { label: "1 - 2 triệu", min: 1000000, max: 2000000 },
  { label: "2 - 3 triệu", min: 2000000, max: 3000000 },
  { label: "3 - 4 triệu", min: 3000000, max: 4000000 },
  { label: "4 - 5 triệu", min: 4000000, max: 5000000 },
  { label: "Trên 5 triệu", min: 5000000, max: Infinity },
];

const ProductList = () => {
  const navigate = useNavigate();
  const { categoryId } = useParams();

  const [categories, setCategories] = useState([]);

  const [cateName, setCateName] = useState("");

  const [products, setProducts] = useState([]);
  const [totalPages, setTotalPages] = useState(1);
  const [currentPage, setCurrentPage] = useState(1);

  const [search, setSearch] = useState("");
  const [selectedBrand, setSelectedBrand] = useState("all");
  const [selectedPrice, setSelectedPrice] = useState(priceOptions[0]);
  const [sortOrder, setSortOrder] = useState("");

  // Load categories
  useEffect(() => {
    const loadCategories = async () => {
      const data = await getCategories();
      setCategories(data);
    };
    loadCategories();
  }, []);

  // Load products
  useEffect(() => {
  const fetchData = async () => {
    const data = await getProducts({
      categoryId,
      page: currentPage,
      pageSize: 12,
      search,
    });

    setProducts(data.items);
    setTotalPages(data.totalPages);

    // Nếu có sản phẩm -> set tên category
    if (data.items?.length > 0) {
      setCateName(data.items[0].categoryName);
    } else {
      setCateName(""); // hoặc null nếu bạn muốn
    }
  };

  fetchData();
}, [categoryId, currentPage, search]);


  const brands = ["all", ...new Set(products.map((p) => p.brandName))];

  let filteredProducts = products.filter(
    (p) =>
      (selectedBrand === "all" || p.brandName === selectedBrand) &&
      p.discountPrice >= selectedPrice.min &&
      p.discountPrice <= selectedPrice.max
  );

  if (sortOrder === "asc") filteredProducts.sort((a, b) => a.discountPrice - b.discountPrice);
  if (sortOrder === "desc") filteredProducts.sort((a, b) => b.discountPrice - a.discountPrice);

  return (
    <div className="container mt-4">
      <div className="row">

        {/* LEFT MENU */}
        <div className="col-md-3">
          <CategoryMenu categories={categories} />
        </div>

        {/* RIGHT CONTENT */}
        <div className="col-md-9">
          <Breadcrumb
  items={[
    { label: "Trang chủ", path: "/" },
    { label: "Sản phẩm", path: null },
    {
      label: cateName || "Danh mục",
      path: null, // trang hiện tại
    },
  ]}
/>

          <h2 className="fw-bold mb-3">{cateName}</h2>

          {/* Filters */}
          <div className="row g-3 mb-4 align-items-end">
            <div className="col-md-3">
              <label className="form-label">Tìm kiếm</label>
              <input
                type="text"
                className="form-control"
                placeholder="Nhập tên sản phẩm..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>

            <div className="col-md-3">
              <label className="form-label">Thương hiệu</label>
              <select
                className="form-select"
                value={selectedBrand}
                onChange={(e) => setSelectedBrand(e.target.value)}
              >
                {brands.map((b, i) => (
                  <option key={i} value={b}>
                    {b === "all" ? "Tất cả" : b}
                  </option>
                ))}
              </select>
            </div>

            <div className="col-md-3">
              <label className="form-label">Khoảng giá</label>
              <select
                className="form-select"
                value={selectedPrice.label}
                onChange={(e) =>
                  setSelectedPrice(priceOptions.find((o) => o.label === e.target.value))
                }
              >
                {priceOptions.map((option, i) => (
                  <option key={i} value={option.label}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="col-md-3">
              <label className="form-label">Sắp xếp</label>
              <select
                className="form-select"
                value={sortOrder}
                onChange={(e) => setSortOrder(e.target.value)}
              >
                <option value="">Mặc định</option>
                <option value="asc">Giá tăng dần</option>
                <option value="desc">Giá giảm dần</option>
              </select>
            </div>
          </div>

          {/* Product Grid */}
          <div className={styles.productGrid}>
            {filteredProducts.length > 0 ? (
              filteredProducts.map((product) => {
                const mainImage =
                  product.images.find((img) => img.isPrimary)?.imageUrl ||
                  "https://via.placeholder.com/300x300?text=No+Image";

                return (
                  <div key={product.id} className={styles.productCard}>
                    <div className={styles.imgWrapper}>
                      <img src={IMAGE_BASE + mainImage} alt={product.name} />
                    </div>

                    <h6
                      className={styles.productName}
                      onClick={() => navigate(`/product/${product.id}`)}
                    >
                      {product.name}
                    </h6>

                    {product.discountPrice < product.price ? (
                      <div>
                        <span className="text-muted text-decoration-line-through me-2">
                          {product.price.toLocaleString()} đ
                        </span>
                        <strong className="text-danger">
                          {product.discountPrice.toLocaleString()} đ
                        </strong>
                      </div>
                    ) : (
                      <strong className="text-danger">
                        {product.price.toLocaleString()} đ
                      </strong>
                    )}

                    <p className="text-muted small">{product.brandName}</p>
                    <button onClick={() => navigate(`/product/${product.id}`)} className="btn btn-success mt-auto">Chi tiết</button>
                  </div>
                );
              })
            ) : (
              <p className="text-center text-muted mt-3">Không tìm thấy sản phẩm nào.</p>
            )}
          </div>

          {/* Pagination */}
          <div className="d-flex justify-content-center mt-4 mb-4">
            <button
              className="btn btn-outline-primary me-2"
              disabled={currentPage <= 1}
              onClick={() => setCurrentPage((prev) => prev - 1)}
            >
              Trước
            </button>

            <span className="align-self-center">
              Trang {currentPage} / {totalPages}
            </span>

            <button
              className="btn btn-outline-primary ms-2"
              disabled={currentPage >= totalPages}
              onClick={() => setCurrentPage((prev) => prev + 1)}
            >
              Sau
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductList;
