import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../services/productService";
import { addToCart } from "../utils/cartUtils";
import Breadcrumb from "../components/Breadcrumb";
import styles from "./ProductDetails.module.css";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const ProductDetails = () => {
  const navigate = useNavigate();
  const { id } = useParams();

  const [product, setProduct] = useState(null);
  const [selectedColor, setSelectedColor] = useState(null);
  const [selectedSize, setSelectedSize] = useState(null);
  const [mainDisplayImage, setMainDisplayImage] = useState("");
  const [thumbnailIndex, setThumbnailIndex] = useState(0);

  // Load sản phẩm
  useEffect(() => {
    const fetchProduct = async () => {
      const data = await getProductById(id);
      setProduct(data);

      if (data.colorVariants?.length > 0) {
        setSelectedColor(data.colorVariants[0]);
      }

      const primaryImg =
        data.images?.find((img) => img.isPrimary)?.imageUrl ||
        data.images?.[0]?.imageUrl ||
        "";
      setMainDisplayImage(primaryImg);
    };
    fetchProduct();
  }, [id]);

  // Khi đổi màu
  useEffect(() => {
    if (selectedColor?.imageUrls?.length > 0) {
      setMainDisplayImage(selectedColor.imageUrls[0]);
    } else if (product?.images?.length > 0) {
      const primary =
        product.images.find((img) => img.isPrimary)?.imageUrl ||
        product.images[0].imageUrl;
      setMainDisplayImage(primary);
    }
  }, [selectedColor, product]);

  // Auto slide thumbnail
  useEffect(() => {
    if (selectedColor) return;
    if (!product?.images || product.images.length <= 1) return;

    const interval = setInterval(() => {
      setThumbnailIndex((prev) => (prev + 1) % product.images.length);
    }, 4000);

    return () => clearInterval(interval);
  }, [selectedColor, product]);

  // Update ảnh chính khi slide
  useEffect(() => {
    if (!selectedColor && product?.images?.[thumbnailIndex]) {
      setMainDisplayImage(product.images[thumbnailIndex].imageUrl);
    }
  }, [thumbnailIndex, selectedColor, product]);

  if (!product) return <h3 className="container mt-5">Đang tải sản phẩm...</h3>;

  const thumbnailImages = product.images || [];
  const finalPrice =
    selectedSize?.finalPrice ?? product.discountPrice ?? product.price;

  const getCurrentStock = () => {
    if (product.colorVariants?.length > 0) {
      if (selectedSize) return selectedSize.stock;
      if (selectedColor) {
        return (
          selectedColor.sizes?.reduce((sum, s) => sum + s.stock, 0) ?? 0
        );
      }
      return product.colorVariants.reduce(
        (total, cv) =>
          total + (cv.sizes?.reduce((sum, s) => sum + s.stock, 0) ?? 0),
        0
      );
    }
    return product.stock ?? 0;
  };

  const currentStock = getCurrentStock();

  const handleAddToCart = () => {
    if (product.colorVariants?.length > 0 && !selectedSize) {
      alert("Vui lòng chọn size trước khi thêm vào giỏ hàng!");
      return;
    }

    addToCart({
      productId: product.id,
      name: product.name,
      price: finalPrice,
      image: IMAGE_BASE + mainDisplayImage,
      colorVariantId: selectedColor?.id,
      color: selectedColor?.color,
      sizeVariantId: selectedSize?.id,
      size: selectedSize?.size,
      quantity: 1,
    });

    alert("Đã thêm vào giỏ hàng!");
  };

  const formatPrice = (price) =>
    price?.toLocaleString("vi-VN") + " đ";

  return (
    <div className="container mt-4 mb-5">
      <Breadcrumb
        items={[
          { label: "Trang chủ", path: "/" },
          {
            label: product.categoryName,
            path: `/products/${product.categoryId}`,
          },
          { label: product.name, path: null },
        ]}
      />

      <div className="row g-5">
        {/* === ẢNH === */}
        <div className="col-lg-6">
          <div className="position-relative">
            <img
              src={IMAGE_BASE + mainDisplayImage}
              alt={product.name}
              className={`img-fluid ${styles.mainImage}`}
              style={{ width: "100%", height: "560px", objectFit: "cover" }}
            />

            {currentStock === 0 && (
              <div
                className={`position-absolute top-50 start-50 translate-middle bg-dark bg-opacity-75 text-white px-4 py-2 ${styles.outOfStock}`}
              >
                <strong>HẾT HÀNG</strong>
              </div>
            )}
          </div>

          {/* Thumbnail */}
          <div className="d-flex gap-2 mt-3 flex-wrap justify-content-center">
            {thumbnailImages.map((img, idx) => (
              <img
                key={idx}
                src={IMAGE_BASE + img.imageUrl}
                alt={`Thumbnail ${idx + 1}`}
                className={`${styles.thumbnail} ${img.imageUrl === mainDisplayImage
                  ? styles.thumbnailActive
                  : ""
                  }`}
                style={{
                  width: "80px",
                  height: "80px",
                  objectFit: "cover",
                }}
                onClick={() => {
                  setMainDisplayImage(img.imageUrl);
                  setThumbnailIndex(idx);
                }}
              />
            ))}
          </div>
        </div>

        {/* === THÔNG TIN === */}
        <div className="col-lg-6">
          <h2 className={styles.productTitle}>{product.name}</h2>

          <p className="text-muted">
            Thương hiệu: <strong>{product.brandName || "Không xác định"}</strong>{" "}
            | Danh mục:{" "}
            <strong>{product.categoryName || "Chưa phân loại"}</strong>
          </p>

          <div>
            <div className={styles.priceRow}>
              <span className={styles.price}>{formatPrice(finalPrice)}</span>

              {product.discountPrice && product.discountPrice < product.price && (
                <span className={styles.oldPrice}>
                  {formatPrice(product.price)}
                </span>
              )}
            </div>

          </div>


          <hr />

          {/* Màu */}
          {product.colorVariants?.length > 0 && (
            <div className="mb-3">
              <h5 className="mb-2">Màu sắc</h5>
              <div className="d-flex flex-wrap gap-2">
                {product.colorVariants.map((variant) => (
                  <button
                    key={variant.id}
                    className={`btn px-4 py-2 ${styles.optionBtn} ${selectedColor?.id === variant.id
                      ? "btn-primary"
                      : "btn-outline-secondary"
                      }`}
                    onClick={() => {
                      setSelectedColor(variant);
                      setSelectedSize(null);
                    }}
                  >
                    {variant.color}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Size */}
          {selectedColor?.sizes?.length > 0 && (
            <div className="mb-3">
              <h5 className="mb-2">Kích thước</h5>
              <div className="d-flex flex-wrap gap-2">
                {selectedColor.sizes.map((sz) => (
                  <button
                    key={sz.id}
                    disabled={!sz.inStock || sz.stock === 0}
                    className={`btn px-4 ${styles.optionBtn} ${selectedSize?.id === sz.id
                      ? "btn-warning"
                      : sz.inStock && sz.stock > 0
                        ? "btn-outline-warning"
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
          <p className="mb-3">
            <strong>Tình trạng:</strong>{" "}
            {currentStock > 0 ? (
              <span
                className={`badge bg-success fs-6 ${styles.stockBadge}`}
              >
                Còn hàng
              </span>
            ) : (
              <span
                className={`badge bg-danger fs-6 ${styles.stockBadge}`}
              >
                Hết hàng
              </span>
            )}
          </p>
          <div className="mb-3">
            <p>{product.description}</p>
          </div >

          {/* Add to cart */}
          <button
            className={`btn btn-success btn-lg px-5 py-3 fw-bold ${styles.addToCartBtn}`}
            disabled={currentStock === 0}
            onClick={handleAddToCart}
          >
            Thêm vào giỏ hàng
          </button>

          {/* Ưu đãi */}
          {product.promotions?.length > 0 && (
            <div className={`mt-3 p-4 border ${styles.promoBox}`}>
              <h5 className="mb-3">🎁 Ưu đãi đặc biệt 🎁</h5>
              <ul className="list-unstyled mb-0">
                {product.promotions.map((promo) => (
                  <li key={promo.id}>{promo.name}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>

      {/* Chi tiết */}
      {product.details?.length > 0 && (
        <div className={`mt-5 p-4 border ${styles.detailBox}`}>
          <h4 className="mb-4">Thông tin chi tiết sản phẩm</h4>
          {product.details.map((item) => (
            <div key={item.id} className="mb-5">
              {item.text && (
                <div
                  className="mb-3"
                  style={{ lineHeight: "1.8" }}
                  dangerouslySetInnerHTML={{
                    __html: item.text.replace(/\n/g, "<br/>"),
                  }}
                />
              )}
              {item.imageUrl && (
                <img
                  src={IMAGE_BASE + item.imageUrl}
                  alt="Chi tiết"
                  className={`img-fluid ${styles.detailImage}`}
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
