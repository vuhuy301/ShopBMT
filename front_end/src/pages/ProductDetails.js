import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";

const products = [
  {
    id: 1,
    name: "Vợt Cầu Lông Kumpoo Power Control K520 Pro",
    price: 850000,
    brand: "Kumpoo",
    colors: ["Đen", "Xanh lá", "Đỏ"],
    images: [
      "https://shopvnb.com//uploads/san_pham/vot-cau-long-yonex-astrox-01a-chinh-hang-1.webp",
      "https://shopvnb.com//uploads/san_pham/vot-cau-long-yonex-astrox-01a-chinh-hang-1.webp",
      "https://shopvnb.com//uploads/san_pham/vot-cau-long-yonex-astrox-01a-chinh-hang-1.webp",
    ],
    description: `Vợt Cầu Lông Kumpoo Power Control K520 Pro - Nâng Cấp Thiết Kế, Chất Lượng Tốt Hơn

1. Giới thiệu vợt cầu lông Kumpoo Power Control K520 Pro
- Dành cho người chơi phong trào tầm thấp.
- Trọng lượng 4U, dễ điều khiển, dễ đánh.
- Khung vợt dạng hộp hỗ trợ lực tốt.

2. Thông số kỹ thuật
- Độ cứng: Trung bình (8.5)
- Trọng lượng: 82 ± 2 g (4U)
- Điểm cân bằng: 290 ± 5 mm

3. Đối tượng phù hợp
- Lối chơi toàn diện.
- Người mới chơi hoặc trình độ trung bình.

📷 Một số hình ảnh minh họa khác:
![minhhoa1](https://shopvnb.com//uploads/san_pham/vot-cau-long-yonex-astrox-01a-chinh-hang-1.webp)
![minhhoa2](https://shopvnb.com//uploads/san_pham/vot-cau-long-yonex-astrox-01a-chinh-hang-1.webp)
`
  },
];

const ProductDetails = () => {
  const { id } = useParams();
  const product = products.find((p) => p.id === parseInt(id));

  const [mainImageIndex, setMainImageIndex] = useState(0);
  const [selectedColor, setSelectedColor] = useState(product?.colors[0]);

  const mainImage = product.images[mainImageIndex];

  // Auto slideshow
  useEffect(() => {
    const interval = setInterval(() => {
      setMainImageIndex((prev) =>
        prev === product.images.length - 1 ? 0 : prev + 1
      );
    }, 3000);
    return () => clearInterval(interval);
  }, [product.images.length]);

  if (!product) {
    return (
      <div className="container mt-4">
        <h3>Sản phẩm không tồn tại!</h3>
        <Link to="/" className="btn btn-primary mt-3">Quay lại</Link>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <Link to="/" className="btn btn-secondary mb-3">← Quay lại</Link>

      <div className="row">
        {/* Ảnh sản phẩm */}
        <div className="col-md-6">
          <img src={mainImage} alt="" className="img-fluid rounded border" />
          <div className="d-flex gap-2 mt-3">
            {product.images.map((img, index) => (
              <img
                key={index}
                src={img}
                width="70"
                className={`border rounded p-1 ${mainImageIndex === index ? "border-primary" : ""}`}
                style={{ cursor: "pointer" }}
                onClick={() => setMainImageIndex(index)}
              />
            ))}
          </div>
        </div>

        {/* Chi tiết sản phẩm */}
        <div className="col-md-6">
          <h3>{product.name}</h3>
             {/* Thương hiệu */}
          <p>Thương hiệu: <strong>{product.brand}</strong></p>
          <p className="text-danger fw-bold fs-4">{product.price.toLocaleString()}đ</p>
       
          {/* Chọn màu sắc */}
          <div className="mt-3">
            <strong>Màu sắc:</strong>
            <div className="d-flex gap-2 mt-2">
              {product.colors.map((color) => (
                <button
                  key={color}
                  className={`btn ${selectedColor === color ? "btn-primary" : "btn-outline-secondary"}`}
                  onClick={() => setSelectedColor(color)}
                >
                  {color}
                </button>
              ))}
            </div>
          </div>

          <button className="btn btn-success mt-3">Thêm vào giỏ hàng</button>
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

      {/* Phần mô tả xuống trang khác */}
      <div className="mt-5 p-3 border rounded bg-light">
        <h4>Mô tả chi tiết sản phẩm</h4>
        <div style={{ whiteSpace: "pre-wrap", fontFamily: "inherit" }}>
          {product.description.split("\n").map((line, idx) => (
            <p key={idx}>{line}</p>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ProductDetails;
