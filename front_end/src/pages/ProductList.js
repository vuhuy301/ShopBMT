import React, { useState } from "react";
import styles from "./ProductList.module.css";

const products = [
  {
    id: 1,
    name: "Vợt Pickleball Gearbox G3 Elongated Chính Hãng",
    price: 3950000,
    brand: "Gearbox",
    color: "Đỏ",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
  {
    id: 2,
    name: "Vợt Pickleball Gearbox G20 Chính Hãng",
    price: 4500000,
    brand: "Gearbox",
    color: "Xanh",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
  {
    id: 3,
    name: "Vợt Pickleball GBX G16 With Molded Texture",
    price: 2650000,
    brand: "GBX",
    color: "Đen",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
  {
    id: 4,
    name: "Vợt Pickleball GBX G16 With Molded Texture",
    price: 2650000,
    brand: "GBX",
    color: "Đỏ",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
  {
    id: 5,
    name: "Vợt Pickleball Power Strike Pro",
    price: 3200000,
    brand: "PowerStrike",
    color: "Xanh",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
  {
    id: 6,
    name: "Vợt Pickleball Thunder Elite",
    price: 4100000,
    brand: "Thunder",
    color: "Đen",
    image:
      "https://cdn.hstatic.net/products/200000852613/1_547ae551a37040a0a8f685bee0c853c6_grande.png",
  },
];

const priceOptions = [
  { label: "Tất cả", min: 0, max: Infinity },
  { label: "Dưới 1 triệu", min: 0, max: 1000000 },
  { label: "1 - 2 triệu", min: 1000000, max: 2000000 },
  { label: "2 - 3 triệu", min: 2000000, max: 3000000 },
  { label: "3 - 4 triệu", min: 3000000, max: 4000000 },
  { label: "4 - 5 triệu", min: 4000000, max: 5000000 },
];

const ProductList = () => {
  const [search, setSearch] = useState("");
  const [selectedBrand, setSelectedBrand] = useState("all");
  const [selectedColor, setSelectedColor] = useState("all");
  const [selectedPrice, setSelectedPrice] = useState(priceOptions[0]);
  const [sortOrder, setSortOrder] = useState("");

  const brands = ["all", ...new Set(products.map((p) => p.brand))];
  const colors = ["all", ...new Set(products.map((p) => p.color))];

  // Lọc sản phẩm
  let filteredProducts = products.filter(
    (p) =>
      p.name.toLowerCase().includes(search.toLowerCase()) &&
      (selectedBrand === "all" || p.brand === selectedBrand) &&
      (selectedColor === "all" || p.color === selectedColor) &&
      p.price >= selectedPrice.min &&
      p.price <= selectedPrice.max
  );

  // Sắp xếp
  if (sortOrder === "asc") filteredProducts.sort((a, b) => a.price - b.price);
  if (sortOrder === "desc") filteredProducts.sort((a, b) => b.price - a.price);

  return (
    <div className="container mt-4">
      <h2 className="fw-bold mb-3">Danh sách sản phẩm</h2>

      {/* Bộ lọc */}
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

        <div className="col-md-2">
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

        <div className="col-md-2">
          <label className="form-label">Màu sắc</label>
          <select
            className="form-select"
            value={selectedColor}
            onChange={(e) => setSelectedColor(e.target.value)}
          >
            {colors.map((c, i) => (
              <option key={i} value={c}>
                {c === "all" ? "Tất cả" : c}
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
              setSelectedPrice(
                priceOptions.find((option) => option.label === e.target.value)
              )
            }
          >
            {priceOptions.map((option, i) => (
              <option key={i} value={option.label}>
                {option.label}
              </option>
            ))}
          </select>
        </div>

        <div className="col-md-2">
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

      {/* Grid sản phẩm */}
      <div className={styles.productGrid}>
        {filteredProducts.length > 0 ? (
          filteredProducts.map((product) => (
            <div key={product.id} className={styles.productCard}>
              <div className={styles.imgWrapper}>
                <img src={product.image} alt={product.name} />
              </div>
              <h6 className={styles.productName}>{product.name}</h6>
              <strong className="text-danger">{product.price.toLocaleString()} đ</strong>
              <p className="text-muted small">{product.brand}</p>
              <button className="btn btn-success mt-auto">Thêm vào giỏ</button>
            </div>
          ))
        ) : (
          <p className="text-center text-muted mt-3">Không tìm thấy sản phẩm nào.</p>
        )}
      </div>
    </div>
  );
};

export default ProductList;
