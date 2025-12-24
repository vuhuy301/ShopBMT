import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./Checkout.module.css";
import { getCart } from "../utils/cartUtils";
import { placeOrder } from "../services/orderService";
import Breadcrumb from "../components/Breadcrumb";

const Checkout = () => {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState([]);
  const [paymentMethod, setPaymentMethod] = useState("cod");
  const [note, setNote] = useState("");
  const [customer, setCustomer] = useState({
    name: "",
    phone: "",
    address: "",
    email: "", // thêm email
  });

  useEffect(() => {
    setCartItems(getCart());

    // Lấy user từ localStorage
    const user = JSON.parse(localStorage.getItem("user") || "{}");
    if (user.email) {
      setCustomer(prev => ({ ...prev, email: user.email }));
    }
  }, []);

  const totalPrice = cartItems.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  const formatPrice = (price) => price?.toLocaleString("vi-VN") + " đ";

  const handleOrder = async () => {
    if (!customer.name || !customer.phone || !customer.address) {
      alert("Vui lòng điền đầy đủ thông tin giao hàng.");
      return;
    }

    if (cartItems.length === 0) {
      alert("Giỏ hàng đang trống.");
      return;
    }

    const items = cartItems.map((item) => ({
      productId: item.productId,
      colorVariantId: item.colorVariantId || 0,
      sizeVariantId: item.sizeVariantId || 0,
      quantity: item.quantity,
      price: item.price,
    }));

    const orderData = {
      name: customer.name,
      phone: customer.phone,
      address: customer.address,
      email: customer.email,
      note,
      paymentMethod,
      items,
    };

    try {
      const order = await placeOrder(orderData);
      console.log("ORDER SUCCESS:", order);

      // Nếu COD → thông báo thành công và clear cart
      if (paymentMethod === "cod") {
        alert("Đặt hàng thành công! Nhân viên sẽ liên hệ bạn để giao hàng.");
        setCartItems([]);
        setCustomer({ name: "", phone: "", address: "" });
        setNote("");
        setPaymentMethod("cod");
        navigate(`/my-order/${order.orderId}`);
      }
      // Nếu thanh toán online → tạo Payment và chuyển sang trang PaymentPage
      else if (paymentMethod === "bank") {
        // Chuyển sang trang PaymentPage kèm orderId
        navigate(`/payment/${order.orderId}`);
      }

    } catch (error) {
      console.error(error);
      alert("Đặt hàng thất bại. Vui lòng thử lại.");
    }
  };

  return (
    <div className={styles.checkoutContainer}>
      <Breadcrumb
        items={[
          { label: "Trang chủ", path: "/" },
          { label: "Giỏ hàng", path: "/cart" },
          { label: "Thanh toán", path: null },
        ]}
      />
      <h3 className={styles.title}>Thanh toán</h3>

      {/* Thông tin giao hàng */}
      <div className={styles.section}>
        <h4 className={styles.sectionTitle}>Thông tin giao hàng</h4>
        <input
          type="text"
          placeholder="Họ và tên"
          className={styles.input}
          value={customer.name}
          onChange={(e) => setCustomer({ ...customer, name: e.target.value })}
        />
        <input
          type="text"
          placeholder="Số điện thoại"
          className={styles.input}
          value={customer.phone}
          onChange={(e) => setCustomer({ ...customer, phone: e.target.value })}
        />
        <input
          type="email"
          placeholder="Email"
          className={styles.input}
          value={customer.email}
          onChange={(e) => setCustomer({ ...customer, email: e.target.value })}
        />

        <textarea
          placeholder="Địa chỉ giao hàng"
          className={styles.textarea}
          value={customer.address}
          onChange={(e) =>
            setCustomer({ ...customer, address: e.target.value })
          }
        ></textarea>
        <textarea
          placeholder="Ghi chú (tuỳ chọn)"
          className={styles.textarea}
          value={note}
          onChange={(e) => setNote(e.target.value)}
        ></textarea>
      </div>

      {/* Phương thức thanh toán */}
      <div className={styles.section}>
        <h4 className={styles.sectionTitle}>Phương thức thanh toán</h4>
        <label className={styles.radioLabel}>
          <span className={styles.radioText}>
            Thanh toán khi nhận hàng (COD)
          </span>
          <input
            type="radio"
            value="cod"
            checked={paymentMethod === "cod"}
            onChange={() => setPaymentMethod("cod")}
          />
        </label>

        <label className={styles.radioLabel}>
          <span className={styles.radioText}>
            Thanh toán online / chuyển khoản
          </span>

          <input
            type="radio"
            value="bank"
            checked={paymentMethod === "bank"}
            onChange={() => setPaymentMethod("bank")}
          />
        </label>

      </div>

      {/* Danh sách sản phẩm */}
      <div className={styles.section}>
        <h4 className={styles.sectionTitle}>Đơn hàng của bạn</h4>

        {cartItems.map((item, index) => (
          <div className={styles.orderItem} key={index}>
            <img src={item.image} alt={item.name} className={styles.itemImage} />

            <div className={styles.itemInfo}>
              <div className={styles.itemName}>{item.name}</div>
              {item.color && (
                <div className={styles.itemDetail}>Màu: {item.color}</div>
              )}
              {item.size && (
                <div className={styles.itemDetail}>Size: {item.size}</div>
              )}
              <div className={styles.itemDetail}>
                SL: {item.quantity} × {formatPrice(item.price)}
              </div>
            </div>

            <div className={styles.itemTotal}>
              {formatPrice(item.price * item.quantity)}
            </div>
          </div>
        ))}

        <div className={styles.totalRow}>
          <span>Tổng cộng:</span>
          <strong>{formatPrice(totalPrice)}</strong>
        </div>
      </div>

      <button className={styles.orderBtn} onClick={handleOrder}>
        XÁC NHẬN ĐẶT HÀNG
      </button>
    </div>
  );
};

export default Checkout;
