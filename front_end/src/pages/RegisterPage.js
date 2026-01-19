import React, { useState } from "react";
import styles from "./RegisterPage.module.css";

const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

const RegisterPage = () => {
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    phone: "",
    address: "",
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);

    try {
      const res = await fetch(`${BASE_URL}/Users/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          accept: "*/*",
        },
        body: JSON.stringify({
          fullName: form.fullName.trim(),
          email: form.email.trim().toLowerCase(),
          password: form.password,
          phone: form.phone.trim(),
          address: form.address.trim(),
        }),
      });

      if (!res.ok) {
        const msg = await res.json();
        throw new Error(msg.message || "Đăng ký thất bại");
      }

      setSuccess("🎉 Đăng ký thành công! Vui lòng đăng nhập.");
      setForm({
        fullName: "",
        email: "",
        password: "",
        phone: "",
        address: "",
      });

      setTimeout(() => {
        window.location.href = "/login";
      }, 1500);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h1 className={styles.title}>ĐĂNG KÝ TÀI KHOẢN</h1>
        <p className={styles.subtitle}>Shop Cầu Lông BMT</p>

        <form onSubmit={handleRegister} className={styles.form}>
          <input
            name="fullName"
            placeholder="Họ và tên"
            value={form.fullName}
            onChange={handleChange}
            required
          />

          <input
            type="email"
            name="email"
            placeholder="Email"
            value={form.email}
            onChange={handleChange}
            required
          />

          <input
            type="password"
            name="password"
            placeholder="Mật khẩu"
            value={form.password}
            onChange={handleChange}
            required
          />

          <input
            name="phone"
            placeholder="Số điện thoại"
            value={form.phone}
            onChange={handleChange}
          />

          <input
            name="address"
            placeholder="Địa chỉ"
            value={form.address}
            onChange={handleChange}
          />

          {error && <div className={styles.error}>{error}</div>}
          {success && <div className={styles.success}>{success}</div>}

          <button type="submit" disabled={loading}>
            {loading ? "Đang đăng ký..." : "Đăng ký"}
          </button>
        </form>

        <div className={styles.footer}>
          Đã có tài khoản?{" "}
          <span onClick={() => (window.location.href = "/login")}>
            Đăng nhập
          </span>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
