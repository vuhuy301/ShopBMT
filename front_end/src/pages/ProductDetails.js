import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../services/productService";
import { addToCart } from "../utils/cartUtils";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const ProductDetails = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const [product, setProduct] = useState(null);
  const [selectedColor, setSelectedColor] = useState(null);   // màu đang chọn
  const [selectedSize, setSelectedSize] = useState(null);     // size đang chọn
  const [mainDisplayImage, setMainDisplayImage] = useState(""); // ảnh chính đang hiển thị to
  const [thumbnailIndex, setThumbnailIndex] = useState(0);    // để highlight thumbnail

  // Load sản phẩm
  useEffect(() => {
    const fetchProduct = async () => {
      const data = await getProductById(id);
      setProduct(data);

      // Chọn màu đầu tiên làm mặc định (nếu có)
      if (data.colorVariants?.length > 0) {
        setSelectedColor(data.colorVariants[0]);
      }

      // Ảnh chính mặc định = ảnh primary hoặc ảnh đầu tiên
      const primaryImg = data.images?.find(img => img.isPrimary)?.imageUrl
        || data.images?.[0]?.imageUrl
        || "";
      setMainDisplayImage(primaryImg);
    };
    fetchProduct();
  }, [id]);

  // Khi đổi màu → đổi ảnh chính thành ảnh đầu tiên của màu đó
  useEffect(() => {
    if (selectedColor?.imageUrls?.length > 0) {
      setMainDisplayImage(selectedColor.imageUrls[0]);
    } else if (product?.images?.length > 0) {
      const primary = product.images.find(img => img.isPrimary)?.imageUrl || product.images[0].imageUrl;
      setMainDisplayImage(primary);
    }
  }, [selectedColor, product]);

  // Auto slide thumbnail + ảnh chính (chỉ khi chưa chọn màu)
  useEffect(() => {
    if (selectedColor) return; // tắt slide khi đang xem màu

    if (!product?.images || product.images.length <= 1) return;

    const interval = setInterval(() => {
      setThumbnailIndex(prev => (prev + 1) % product.images.length);
    }, 4000);

    return () => clearInterval(interval);
  }, [selectedColor, product]);

  // Khi slide → cập nhật ảnh chính
  useEffect(() => {
    if (!selectedColor && product?.images?.[thumbnailIndex]) {
      setMainDisplayImage(product.images[thumbnailIndex].imageUrl);
    }
  }, [thumbnailIndex, selectedColor, product]);

  if (!product) return <h3 className="container mt-5">Đang tải sản phẩm...</h3>;

  // Danh sách ảnh thumbnail – luôn là ảnh chính của sản phẩm
  const thumbnailImages = product.images || [];

  // Tính giá cuối cùng
  const finalPrice = selectedSize?.finalPrice ?? product.discountPrice ?? product.price;

  // Tính tồn kho hiện tại
  const getCurrentStock = () => {
    if (product.colorVariants?.length > 0) {
      if (selectedSize) return selectedSize.stock;
      if (selectedColor) {
        return selectedColor.sizes?.reduce((sum, s) => sum + s.stock, 0) ?? 0;
      }
      return product.colorVariants.reduce((total, cv) => 
        total + (cv.sizes?.reduce((sum, s) => sum + s.stock, 0) ?? 0), 0);
    }
    return product.stock ?? 0;
  };

  const currentStock = getCurrentStock();

  // Thêm vào giỏ hàng
  const handleAddToCart = () => {
    if (product.colorVariants?.length > 0 && !selectedSize) {
      alert("Vui lòng chọn size trước khi thêm vào giỏ hàng!");
      return;
    }

    addToCart({
      id: product.id,
      name: product.name,
      price: finalPrice,
      image: IMAGE_BASE + mainDisplayImage,
      size: selectedSize?.size || null,
      color: selectedColor?.color || null,
      quantity: 1,
    });

    alert("Đã thêm vào giỏ hàng!");
  };

  const formatPrice = (price) => price?.toLocaleString("vi-VN") + " đ";

  return (
    <div className="container mt-4 mb-5">
      <button onClick={() => navigate(-1)} className="btn btn-outline-secondary mb-4">
        ← Quay lại
      </button>

      <div className="row g-5">
        {/* === PHẦN ẢNH === */}
        <div className="col-lg-6">
          {/* Ảnh chính lớn */}
          <div className="position-relative">
            <img
              src={IMAGE_BASE + mainDisplayImage}
              alt={product.name}
              className="img-fluid rounded shadow-sm"
              style={{ width: "100%", height: "560px", objectFit: "cover" }}
            />
            {currentStock === 0 && (
              <div className="position-absolute top-50 start-50 translate-middle bg-dark bg-opacity-75 text-white px-4 py-2 rounded">
                <strong>HẾT HÀNG</strong>
              </div>
            )}
          </div>

          {/* Thumbnail – luôn là ảnh chính sản phẩm */}
          {thumbnailImages.length > 0 && (
            <div className="d-flex gap-2 mt-3 flex-wrap justify-content-center">
              {thumbnailImages.map((img, idx) => (
                <img
                  key={idx}
                  src={IMAGE_BASE + img.imageUrl}
                  alt={`Thumbnail ${idx + 1}`}
                  className={`border rounded cursor-pointer ${
                    img.imageUrl === mainDisplayImage ? "border-primary border-3" : "border"
                  }`}
                  style={{
                    width: "80px",
                    height: "80px",
                    objectFit: "cover",
                    cursor: "pointer",
                    transition: "all 0.2s"
                  }}
                  onClick={() => {
                    setMainDisplayImage(img.imageUrl);
                    setThumbnailIndex(idx);
                  }}
                />
              ))}
            </div>
          )}
        </div>

        {/* === THÔNG TIN SẢN PHẨM === */}
        <div className="col-lg-6">
          <h2 className="fw-bold">{product.name}</h2>
          <p className="text-muted">
            Thương hiệu: <strong>{product.brandName || "Không xác định"}</strong> | 
            Danh mục: <strong>{product.categoryName || "Chưa phân loại"}</strong>
          </p>

          {/* Giá */}
          <div className="my-4">
            <h3 className="text-danger fw-bold">{formatPrice(finalPrice)}</h3>
            {product.discountPrice && product.discountPrice < product.price && (
              <p className="text-decoration-line-through text-muted">
                {formatPrice(product.price)}
              </p>
            )}
          </div>

          <hr />

          {/* Chọn màu */}
          {product.colorVariants?.length > 0 && (
            <div className="mb-4">
              <h5 className="mb-3">Màu sắc</h5>
              <div className="d-flex flex-wrap gap-2">
                {product.colorVariants.map((variant) => (
                  <button
                    key={variant.id}
                    className={`btn px-4 py-2 ${
                      selectedColor?.id === variant.id
                        ? "btn-primary"
                        : "btn-outline-secondary"
                    }`}
                    onClick={() => {
                      setSelectedColor(variant);
                      setSelectedSize(null); // reset size khi đổi màu
                    }}
                  >
                    {variant.color}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Chọn size */}
          {selectedColor?.sizes?.length > 0 && (
            <div className="mb-4">
              <h5 className="mb-3">Kích thước</h5>
              <div className="d-flex flex-wrap gap-2">
                {selectedColor.sizes.map((sz) => (
                  <button
                    key={sz.id}
                    disabled={!sz.inStock || sz.stock === 0}
                    className={`btn px-4 ${
                      selectedSize?.id === sz.id
                        ? "btn-success"
                        : sz.inStock && sz.stock > 0
                        ? "btn-outline-success"
                        : "btn-secondary opacity-50"
                    }`}
                    onClick={() => setSelectedSize(sz)}
                  >
                    {sz.size}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Tồn kho */}
          <p className="mb-4">
            <strong>Tình trạng:</strong>{" "}
            {currentStock > 0 ? (
              <span className="badge bg-success fs-6">
                {selectedSize ? "Còn hàng" : `Còn ${currentStock} sản phẩm`}
              </span>
            ) : (
              <span className="badge bg-danger fs-6">Hết hàng</span>
            )}
          </p>

          {/* Nút thêm giỏ hàng */}
          <button
            className="btn btn-success btn-lg px-5 py-3 fw-bold"
            disabled={currentStock === 0}
            onClick={handleAddToCart}
          >
            Thêm vào giỏ hàng
          </button>

          {/* Ưu đãi */}
          <div className="mt-4 p-4 border rounded bg-light">
            <h5 className="mb-3">Ưu đãi đặc biệt</h5>
            <ul className="list-unstyled mb-0">
              <li className="mb-2">Miễn phí vận chuyển toàn quốc cho đơn từ 500.000đ</li>
              <li className="mb-2">Giảm thêm 10% cho đơn hàng tiếp theo</li>
              <li className="mb-2">Đổi trả miễn phí trong 7 ngày nếu lỗi nhà sản xuất</li>
              <li>Hỗ trợ tư vấn size qua Zalo/Fanpage</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Mô tả chi tiết */}
      {product.details?.length > 0 && (
        <div className="mt-5 p-4 border rounded bg-light">
          <h4 className="mb-4">Thông tin chi tiết sản phẩm</h4>
          {product.details.map((item) => (
            <div key={item.id} className="mb-5">
              {item.text && (
                <div
                  className="mb-3"
                  style={{lineHeight: "1.8" }}
                  dangerouslySetInnerHTML={{ __html: item.text.replace(/\n/g, "<br/>") }}
                />
              )}
              {item.imageUrl && (
                <img
                  src={IMAGE_BASE + item.imageUrl}
                  alt="Chi tiết sản phẩm"
                  className="img-fluid rounded shadow-sm"
                  style={{ maxHeight: "600px", objectFit: "contain" }}
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