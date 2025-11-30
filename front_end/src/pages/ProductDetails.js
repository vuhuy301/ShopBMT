import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../services/productService";
import { addToCart } from "../utils/cartUtils";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const ProductDetails = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const [product, setProduct] = useState(null);
  const [selectedColor, setSelectedColor] = useState(null);
  const [selectedSize, setSelectedSize] = useState(null);
  const [mainImageIndex, setMainImageIndex] = useState(0);

  // Lấy dữ liệu sản phẩm
  useEffect(() => {
    const fetchData = async () => {
      const data = await getProductById(id);
      setProduct(data);

      // Nếu có biến thể → mặc định chọn màu đầu tiên
      if (data.colorVariants?.length > 0) {
        setSelectedColor(data.colorVariants[0]);
      }
    };
    fetchData();
  }, [id]);

  // Slide ảnh
  useEffect(() => {
    if (!product) return;
    const interval = setInterval(() => {
      setMainImageIndex((prev) =>
        prev === (getImages().length - 1) ? 0 : prev + 1
      );
    }, 3000);
    return () => clearInterval(interval);
  }, [product, selectedColor]);

  if (!product) return <h3 className="container mt-5">Đang tải sản phẩm...</h3>;

  // ------------------------------
  // Lấy ảnh hiển thị theo biến thể
  // ------------------------------
  const getImages = () => {
    if (selectedColor && selectedColor.imageUrls?.length > 0) {
      return selectedColor.imageUrls.map((url) => ({ imageUrl: url }));
    }
    return product.images;
  };

  const images = getImages();
  const mainImage = images[mainImageIndex]?.imageUrl;

  // ------------------------------
  // Giá cuối cùng
  // ------------------------------
  const finalPrice = selectedSize?.finalPrice ?? product.discountPrice ?? product.price;

  // ------------------------------
  // Stock
  // ------------------------------
 let finalStock = 0;

if (product.colorVariants?.length > 0) {
  // Sản phẩm có biến thể màu/size
  if (selectedSize) {
    finalStock = selectedSize.stock;
  } else if (selectedColor) {
    finalStock = selectedColor.sizes?.reduce((sum, sz) => sum + sz.stock, 0) ?? 0;
  } else {
    // Chưa chọn màu → tổng stock tất cả màu
    finalStock = product.colorVariants.reduce(
      (total, color) => total + (color.sizes?.reduce((sum, sz) => sum + sz.stock, 0) ?? 0),
      0
    );
  }
} else {
  // Sản phẩm không có biến thể
  finalStock = product.stock ?? 0;
}


  // ------------------------------
  // Thêm vào giỏ hàng
  // ------------------------------
  const handleAddToCart = () => {
    if (product.colorVariants?.length > 0 && !selectedSize) {
      alert("Vui lòng chọn size trước khi thêm vào giỏ hàng!");
      return;
    }

    addToCart({
      id: product.id,
      name: product.name,
      price: finalPrice,
      image: IMAGE_BASE + mainImage,
      size: selectedSize?.size ?? null,
      color: selectedColor?.color ?? null,
      quantity: 1,
    });

    alert("Đã thêm vào giỏ hàng!");
  };

  const formatPrice = (price) => price.toLocaleString("vi-VN") + " đ";

  return (
    <div className="container mt-4">
      <button onClick={() => navigate(-1)} className="btn btn-secondary mb-3">
        Quay lại
      </button>

      <div className="row">
        {/* Ảnh sản phẩm */}
        <div className="col-md-6">
          <img
            src={IMAGE_BASE + mainImage}
            alt={product.name}
            className="img-fluid rounded border"
          />

          {/* Thumbnail */}
          <div className="d-flex gap-2 mt-3 flex-wrap">
            {images.map((img, index) => (
              <img
                key={index}
                src={IMAGE_BASE + img.imageUrl}
                width="70"
                className={`border rounded p-1 ${mainImageIndex === index ? "border-primary" : ""}`}
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
          <p className="text-danger fw-bold fs-4">{formatPrice(finalPrice)}</p>
          {product.discountPrice && (
            <p className="text-decoration-line-through">{formatPrice(product.price)}</p>
          )}

          {/* Biến thể màu */}
          {product.colorVariants?.length > 0 && (
            <div className="mt-3">
              <h5>Màu sắc</h5>
              <div className="d-flex gap-2 flex-wrap">
                {product.colorVariants.map((variant) => (
                  <button
                    key={variant.id}
                    className={`btn ${selectedColor?.id === variant.id ? "btn-primary" : "btn-outline-primary"}`}
                    onClick={() => {
                      setSelectedColor(variant);
                      setSelectedSize(null);
                      setMainImageIndex(0);
                    }}
                  >
                    {variant.color}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Biến thể size */}
          {selectedColor?.sizes?.length > 0 && (
            <div className="mt-3">
              <h5>Chọn size</h5>
              <div className="d-flex gap-2 flex-wrap">
                {selectedColor.sizes.map((sz) => (
                  <button
                    key={sz.id}
                    disabled={!sz.inStock}
                    className={`btn ${selectedSize?.id === sz.id ? "btn-success" : "btn-outline-success"}`}
                    onClick={() => setSelectedSize(sz)}
                  >
                    {sz.size}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Tình trạng */}
          <p className="mt-3">
            <strong>Tình trạng: </strong>
            {finalStock > 0 ? (
              <span className="badge bg-success">{selectedSize ? "Còn hàng" : `${finalStock} sản phẩm`}</span>
            ) : (
              <span className="badge bg-danger">Hết hàng</span>
            )}
          </p>

          {/* Thêm vào giỏ hàng */}
          <button
            className="btn btn-success mt-3"
            disabled={finalStock === 0}
            onClick={handleAddToCart}
          >
            Thêm vào giỏ hàng
          </button>

          {/* Ưu đãi */}
          <div className="mt-3 p-3 border rounded bg-light">
            <h5>Ưu đãi</h5>
            <ul className="list-unstyled mb-0">
              <li>✅ Miễn phí vận chuyển cho đơn hàng từ 500.000đ</li>
              <li>✅ Giảm 10% cho đơn hàng tiếp theo</li>
              <li>✅ Hỗ trợ đổi trả trong 7 ngày nếu sản phẩm lỗi</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Mô tả chi tiết */}
      {product.details.length > 0 && (
        <div className="mt-5 p-3 border rounded bg-light">
          <h4>Mô tả chi tiết sản phẩm</h4>
          {product.details.map((item) => (
            <div key={item.id} className="mt-4">
              <div style={{ whiteSpace: "pre-wrap" }} className="mb-3">
                {item.text}
              </div>
              {item.imageUrl && (
                <img
                  src={IMAGE_BASE + item.imageUrl}
                  alt={item.name ?? "product detail"}
                  className="img-fluid rounded"
                />
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ProductDetails;
