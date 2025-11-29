import React, { useState } from "react";
import styles from "./Cart.module.css";

const Cart = () => {
  const [cartItems, setCartItems] = useState([
    {
      id: 1,
      name: "Váy cầu lông Taro TR025-V08 - Hồng chính hãng",
      size: "L",
      price: 159000,
      quantity: 1,
      image: "/images/tennis-dress.png", // đổi đường dẫn ảnh phù hợp
    },
    {
      id: 2,
      name: "Giày cầu lông Yonex Cascade Drive 3 - Peacock Blue",
      size: "39",
      price: 2450000,
      quantity: 1,
      image: "/images/yonex-shoe.png", // đổi đường dẫn ảnh phù hợp
    },
  ]);

  const handleQuantityChange = (id, delta) => {
    setCartItems(prev =>
      prev.map(item =>
        item.id === id
          ? { ...item, quantity: Math.max(1, item.quantity + delta) }
          : item
      )
    );
  };

  const handleRemove = (id) => {
    setCartItems(prev => prev.filter(item => item.id !== id));
  };

  const totalPrice = cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);

  const formatPrice = (price) => price.toLocaleString("vi-VN") + " đ";

  return (
    <div className={styles.cartContainer}>
      <div className={styles.cartHeader}>GIỎ HÀNG CỦA BẠN</div>
      {cartItems.map(item => (
        <div key={item.id} className={styles.cartItem}>
          <img src={item.image} alt={item.name} className={styles.itemImage} />
          <div className={styles.itemInfo}>
            <div className={styles.itemName}>{item.name}</div>
            <div className={styles.itemSize}>Size: {item.size}</div>
          </div>
          <div className={styles.quantityControl}>
            <button className={styles.quantityBtn} onClick={() => handleQuantityChange(item.id, -1)}>-</button>
            <div className={styles.quantityNumber}>{item.quantity}</div>
            <button className={styles.quantityBtn} onClick={() => handleQuantityChange(item.id, 1)}>+</button>
          </div>
          <div className={styles.itemPrice}>{formatPrice(item.price)}</div>
          <div className={styles.removeBtn} onClick={() => handleRemove(item.id)}>×</div>
        </div>
      ))}

      <div className={styles.totalContainer}>
        Tổng tiền: {formatPrice(totalPrice)}
      </div>

      <button className={styles.orderBtn}>ĐẶT HÀNG</button>
    </div>
  );
};

export default Cart;
