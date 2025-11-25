import React from "react";
import styles from "./Header.module.css";

const Header = () => {
  return (
    <div className={styles.headerTop}>
      <div className="container d-flex justify-content-between align-items-center py-2">

        {/* Logo */}
        <div className="d-flex align-items-center">
          <img src="https://aocaulongthietke.com/wp-content/uploads/2022/10/Mau-logo-doi-club-cau-lac-bo-cau-long-thiet-ke-dep-1-400x400.png" alt="logo" className={styles.logo} />
        </div>

        {/* Search */}
        <div className={`d-flex align-items-center ${styles.searchBox}`}>
          <input
            className="form-control me-2"
            placeholder="Bạn đang tìm gì?"
          />
          <button className="btn btn-light">🔍</button>
        </div>

        {/* Right section */}
        <div className={`d-flex align-items-center ${styles.rightMenu}`}>
          <span>📞 Hotline: <b>0888 666 441</b></span>
          <a href="#" className="ms-4">Đăng nhập</a>
          <a href="#" className="ms-3">Đăng ký</a>
          <a href="#" className="ms-4">🛒 Giỏ hàng</a>
        </div>

      </div>
    </div>
  );
};

export default Header;
