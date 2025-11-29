import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../services/productService";
const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;
const ProductDetails = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [mainImageIndex, setMainImageIndex] = useState(0);

  useEffect(() => {
    const fetchData = async () => {
      const data = await getProductById(id);
      setProduct(data);
    };
    fetchData();
  }, [id]);

  // Auto slideshow
  useEffect(() => {
    if (!product) return;

    const interval = setInterval(() => {
      setMainImageIndex((prev) =>
        prev === product.images.length - 1 ? 0 : prev + 1
      );
    }, 3000);

    return () => clearInterval(interval);
  }, [product]);

  if (!product) return <h3 className="container mt-5">Đang tải sản phẩm...</h3>;

  const mainImage = product.images[mainImageIndex]?.imageUrl;

  return (
    <div className="container mt-4">
      <button onClick={() => navigate(-1)} className="btn btn-secondary">
        Quay lại
      </button>

      <div className="row">
        {/* Ảnh sản phẩm */}
        <div className="col-md-6">
          <img
            src={IMAGE_BASE + mainImage}
            alt=""
            className="img-fluid rounded border"
          />

          {/* Thumbnail */}
          <div className="d-flex gap-2 mt-3">
            {product.images.map((img, index) => (
              <img
                key={index}
                src={ IMAGE_BASE + img.imageUrl}
                width="70"
                className={`border rounded p-1 ${mainImageIndex === index ? "border-primary" : ""
                  }`}
                style={{ cursor: "pointer" }}
                onClick={() => setMainImageIndex(index)}
              />
            ))}
          </div>
        </div>

        {/* Thông tin sản phẩm */}
        <div className="col-md-6">
          <h3>{product.name}</h3>

          <p>
            Thương hiệu: <strong>{product.brandName}</strong>
          </p>

          {/* Giá */}
          <p className="text-danger fw-bold fs-4">
            {product.discountPrice
              ? `${product.discountPrice.toLocaleString()}đ`
              : `${product.price.toLocaleString()}đ`}
          </p>

          {product.discountPrice && (
            <p className="text-decoration-line-through">
              {product.price.toLocaleString()}đ
            </p>
          )}

          {/* 🔥 Tình trạng hàng */}
          <p className="mt-2">
            <strong>Tình trạng: </strong>
            {product.stock > 0 ? (
              <span className="badge bg-success">Còn hàng</span>
            ) : (
              <span className="badge bg-danger">Hết hàng</span>
            )}
          </p>

          {/* Nút thêm vào giỏ — tự disable nếu hết hàng */}
          <button
            className="btn btn-success mt-3"
            disabled={product.stock === 0}
          >
            Thêm vào giỏ hàng
          </button>

          {/* Ưu đãi */}
          <div className="mt-3 p-3 border rounded bg-light">
            <h5>Ưu đãi</h5>
            <ul className="list-unstyled mb-0">
              <li>✅ Miễn phí vận chuyển cho đơn hàng từ 500.000đ</li>
              <li>✅ Giảm 10% khi mua sản phẩm lần tiếp theo</li>
              <li>✅ Hỗ trợ đổi trả trong 7 ngày nếu sản phẩm lỗi</li>
            </ul>
          </div>
        </div>

      </div>

      {/* Mô tả chi tiết từ "details" */}
      <div className="mt-5 p-3 border rounded bg-light">
        <h4>Mô tả chi tiết sản phẩm</h4>

        {product.details.map((item) => (
          <div key={item.id} className="mt-4">

            {/* Text trước */}
            <div style={{ whiteSpace: "pre-wrap" }} className="mb-3">
              {item.text}
            </div>

            {/* Ảnh nằm dưới */}
            {item.imageUrl && (
              <img
                src={IMAGE_BASE + item.imageUrl}
                alt={item.name || "product image"}
                className="img-fluid rounded"
                style={{ maxHeight: "600px" }}
              />
            )}

          </div>
        ))}
      </div>
    </div>
  );
};

export default ProductDetails;
