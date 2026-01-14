import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getOrderById } from "../services/orderService";
import styles from "./OrderDetail.module.css";
import Breadcrumb from "../components/Breadcrumb";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const OrderDetailPage = () => {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
  const fetchOrder = async () => {
    try {
      const data = await getOrderById(id);
      setOrder(data);
    } catch (err) {
      console.error(err);
      alert("Không lấy được chi tiết đơn hàng");
    } finally {
      setLoading(false);
    }
  };

  fetchOrder();
}, [id]);

  const formatPrice = (price) => price?.toLocaleString("vi-VN") + " đ";

  if (loading) return <div className={styles.container}>Đang tải...</div>;
  if (!order) return <div className={styles.container}>Không tìm thấy đơn hàng</div>;

  

  return (
    <div className={styles.container}>
      <Breadcrumb
    items={[
      { label: "Trang chủ", path: "/" },
      {
        label: "Chi tiết đơn hàng",
        path: null,
      }
    ]}
  />
      <h2 className={styles.title}>Chi tiết đơn hàng #{order.id}</h2>

      {/* Thông tin khách hàng */}
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>Thông tin khách hàng</h3>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Họ và tên:</span>
          <span className={styles.infoValue}>{order.customerName}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Email:</span>
          <span className={styles.infoValue}>{order.email || "N/A"}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Số điện thoại:</span>
          <span className={styles.infoValue}>{order.phone}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Địa chỉ:</span>
          <span className={styles.infoValue}>{order.shippingAddress}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Ghi chú:</span>
          <span className={styles.infoValue}>{order.note || "Không có"}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Trạng thái:</span>
          <span className={`${styles.status} ${styles[order.status]}`}>{order.status}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.infoLabel}>Phương thức thanh toán:</span>
          <span className={styles.infoValue}>{order.paymentMethod}</span>
        </div>
      </div>

      {/* Danh sách sản phẩm */}
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>Sản phẩm trong đơn</h3>
        {order.items?.map((item) => (
          <div className={styles.itemRow} key={item.Id}>
            <img src={IMAGE_BASE + item.imageUrl} alt={item.productName} className={styles.itemImage} />
            <div className={styles.itemInfo}>
              <div className={styles.itemName}>{item.productName}</div>
              <div className={styles.itemDetail}>Màu: {item.color}</div>
              <div className={styles.itemDetail}>Size: {item.size}</div>
              <div className={styles.itemDetail}>SL: {item.quantity} × {formatPrice(item.price)}</div>
            </div>
            <div className={styles.itemTotal}>
              {formatPrice(item.price * item.quantity)}
            </div>
          </div>
        ))}
        <div className={styles.totalRow}>
          <span>Tổng cộng:</span>
          <span>{formatPrice(order.totalAmount)}</span>
        </div>
      </div>
    </div>
  );
};

export default OrderDetailPage;
