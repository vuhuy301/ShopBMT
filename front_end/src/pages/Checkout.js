import React, { useState, useEffect } from "react";
import styles from "./Checkout.module.css";
import { getCart } from "../utils/cartUtils";
import { placeOrder } from "../services/orderService";

const Checkout = () => {
    const [cartItems, setCartItems] = useState([]);
    const [paymentMethod, setPaymentMethod] = useState("cod");
    const [note, setNote] = useState("");
    const [customer, setCustomer] = useState({
        name: "",
        phone: "",
        address: "",
    });

    useEffect(() => {
        setCartItems(getCart());
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

        const items = cartItems.map(item => ({
            productId: item.productId,
            colorVariantId: item.colorVariantId || 0,
            sizeVariantId: item.sizeVariantId || 0,
            quantity: item.quantity,
            price: item.price
        }));

        const orderData = {
            name: customer.name,
            phone: customer.phone,
            address: customer.address,
            note,
            paymentMethod,
            items
        };

        try {
            const result = await placeOrder(orderData);
            console.log("ORDER SUCCESS:", result);
            alert("Đặt hàng thành công!");

            setCartItems([]);
            setCustomer({ name: "", phone: "", address: "" });
            setNote("");
            setPaymentMethod("cod");
        } catch (error) {
            console.error(error);
            alert("Đặt hàng thất bại. Vui lòng thử lại.");
        }
    };

    return (
        <div className={styles.checkoutContainer}>
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

                <textarea
                    placeholder="Địa chỉ giao hàng"
                    className={styles.textarea}
                    value={customer.address}
                    onChange={(e) => setCustomer({ ...customer, address: e.target.value })}
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
                    <input
                        type="radio"
                        value="cod"
                        checked={paymentMethod === "cod"}
                        onChange={() => setPaymentMethod("cod")}
                    />
                    Thanh toán khi nhận hàng (COD)
                </label>

                <label className={styles.radioLabel}>
                    <input
                        type="radio"
                        value="bank"
                        checked={paymentMethod === "bank"}
                        onChange={() => setPaymentMethod("bank")}
                    />
                    Chuyển khoản ngân hàng
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
