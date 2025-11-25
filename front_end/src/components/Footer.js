import React from "react";
import styles from "./Footer.module.css";
import { FaFacebook, FaInstagram, FaPhoneAlt, FaEnvelope, FaMapMarkerAlt } from "react-icons/fa";

const Footer = () => {
  return (
    <footer className={styles.footer}>
      <div className="container py-4">
        <div className="row">

          {/* Cột 1 – Giới thiệu */}
          <div className="col-md-3">
            <h5 className={styles.title}>🏸 Shop Cầu Lông Pro</h5>
            <p>
              Chuyên cung cấp vợt cầu lông, giày cầu lông, balo, túi vợt và phụ kiện chính hãng.
              Cam kết 100% sản phẩm chất lượng – bảo hành uy tín.
            </p>
          </div>

          {/* Cột 2 – Danh mục sản phẩm */}
          <div className="col-md-3">
            <h5 className={styles.title}>Sản Phẩm</h5>
            <ul className={styles.list}>
              <li>Vợt cầu lông</li>
              <li>Giày cầu lông</li>
              <li>Balo – Túi vợt</li>
              <li>Áo quần cầu lông</li>
              <li>Phụ kiện cầu lông</li>
            </ul>
          </div>

          {/* Cột 3 – Hỗ trợ */}
          <div className="col-md-3">
            <h5 className={styles.title}>Hỗ Trợ</h5>
            <ul className={styles.list}>
              <li>Hướng dẫn mua hàng</li>
              <li>Chính sách bảo hành</li>
              <li>Chính sách đổi trả</li>
              <li>Chính sách giao hàng</li>
              <li>Liên hệ hỗ trợ</li>
            </ul>
          </div>

          {/* Cột 4 – Liên hệ */}
          <div className="col-md-3">
            <h5 className={styles.title}>Liên Hệ</h5>
            <ul className={styles.contactList}>
              <li><FaMapMarkerAlt /> 123 Nguyễn Trãi, Hà Nội</li>
              <li><FaPhoneAlt /> 0909 999 999</li>
              <li><FaEnvelope /> shopcaulong@gmail.com</li>
            </ul>

            <div className={styles.socialIcons}>
              <FaFacebook />
              <FaInstagram />
            </div>
          </div>

        </div>

        {/* COPYRIGHT */}
        <div className={styles.copy}>
          © 2025 Shop Cầu Lông Pro – All Rights Reserved.
        </div>
      </div>
    </footer>
  );
};

export default Footer;
