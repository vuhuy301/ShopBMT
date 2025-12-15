import React from "react";
import { Link } from "react-router-dom";
import styles from "./Header.module.css";
import { useNavigate } from "react-router-dom";

const Header = () => {

  const navigate = useNavigate();
  const token = localStorage.getItem("accessToken");
  const user = JSON.parse(localStorage.getItem("user") || "{}");
  const role = localStorage.getItem("role"); // "Admin", "Employee", "Customer"

  const handleLogout = () => {
    localStorage.clear();
    window.location.href = "/";
  };

  return (
    <div className={styles.headerTop}>
      <div className="container d-flex justify-content-between align-items-center py-2">

        {/* Logo */}

        <div
          className="d-flex align-items-center"
          onClick={() => navigate("/")}
          style={{ cursor: "pointer" }}
        >
          <img
            src="https://aocaulongthietke.com/wp-content/uploads/2022/10/Mau-logo-doi-club-cau-lac-bo-cau-long-thiet-ke-dep-1-400x400.png"
            alt="logo"
            className={styles.logo}
          />
        </div>

        {/* Search */}
        <div className={`d-flex align-items-center ${styles.searchBox}`}>
          <input
            className="form-control me-2"
            placeholder="Bạn đang tìm gì?"
            aria-label="Tìm kiếm sản phẩm"
          />
          <button className="btn btn-light" aria-label="Tìm kiếm">
            Search
          </button>
        </div>

        {/* Right section */}
        <div className={`d-flex align-items-center ${styles.rightMenu}`}>
          <span className="me-4">
            Hotline: <b>0888 666 441</b>
          </span>

          {/* === CHƯA ĐĂNG NHẬP === */}
          {!token ? (
            <>
              <Link to="/login" className="ms-4 fw-bold">
                Đăng nhập
              </Link>

              {/* Nếu bạn có trang đăng ký khách hàng thì để Link, không thì dùng button */}
                <Link to="/register" className="ms-4 fw-bold">
                Đăng ký
              </Link>
            </>
          ) : (
            /* === ĐÃ ĐĂNG NHẬP === */
            <>
              <span className="me-3">
                Xin chào, <strong>{user.fullName || user.email || "User"}</strong>
              </span>

              <span className="badge bg-success me-3">
                {role === "Admin" ? "QUẢN TRỊ" : role === "Employee" ? "NHÂN VIÊN" : "KHÁCH"}
              </span>

              {role === "Admin" && (
                <Link to="/admin" className="me-3 text-info small">
                  (Quản trị)
                </Link>
              )}
              {role === "Employee" && (
                <Link to="/employee" className="me-3 text-info small">
                  (Nhân viên)
                </Link>
              )}

              <button
                onClick={handleLogout}
                className="btn btn-sm btn-outline-danger"
              >
                Đăng xuất
              </button>
            </>
          )}

          {/* Giỏ hàng */}
          <Link to="/cart" className="ms-4">
            Giỏ hàng
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Header;