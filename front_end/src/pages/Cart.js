import React, { useState, useEffect } from "react";
import styles from "./Cart.module.css";
import { getCart, addToCart, removeFromCart, updateQuantity } from "../utils/cartUtils";

const Cart = () => {
  const [cartItems, setCartItems] = useState([]);

  useEffect(() => {
    setCartItems(getCart());
  }, []);

  const handleQuantityChange = (item, delta) => {
    updateQuantity(item, delta);
    setCartItems(getCart());
  };

  const handleRemove = (item) => {
    removeFromCart(item);
    setCartItems(getCart());
  };

  const totalPrice = cartItems.reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  const formatPrice = (price) => price.toLocaleString("vi-VN") + " đ";

  return (
    <div className={styles.cartContainer}>
      <h3 className={styles.cartHeader}>GIỎ HÀNG CỦA BẠN</h3>

      {cartItems.length === 0 && <p>Chưa có sản phẩm nào trong giỏ.</p>}

      {cartItems.map((item, index) => (
        <div key={index} className={styles.cartItem}>
          <img src={item.image} alt={item.name} className={styles.itemImage} />
          <div className={styles.itemInfo}>
            <div className={styles.itemName}>{item.name}</div>
            {item.color && <div className={styles.itemColor}>Màu: {item.color}</div>}
            {item.size && <div className={styles.itemSize}>Size: {item.size}</div>}
          </div>

          <div className={styles.quantityControl}>
            <button
              className={styles.quantityBtn}
              onClick={() => handleQuantityChange(item, -1)}
            >
              -
            </button>
            <div className={styles.quantityNumber}>{item.quantity}</div>
            <button
              className={styles.quantityBtn}
              onClick={() => handleQuantityChange(item, 1)}
            >
              +
            </button>
          </div>

          <div className={styles.itemPrice}>{formatPrice(item.price)}</div>
          <div className={styles.removeBtn} onClick={() => handleRemove(item)}>
            ×
          </div>
        </div>
      ))}

      {cartItems.length > 0 && (
        <>
          <div className={styles.totalContainer}>
            Tổng tiền: {formatPrice(totalPrice)}
          </div>

          <button className={styles.orderBtn}>ĐẶT HÀNG</button>
        </>
      )}
    </div>
  );
};

export default Cart;
