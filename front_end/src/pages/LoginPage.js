// src/pages/auth/LoginPage.js
import React, { useState, useEffect } from "react";
import styles from "./LoginPage.module.css";

const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // ==================== GOOGLE LOGIN ====================
  useEffect(() => {
    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    document.body.appendChild(script);

    script.onload = () => {
      window.google.accounts.id.initialize({
        client_id: "1013063153881-rd02h43rtk2rtsnjtvla6jlm94mt66f5.apps.googleusercontent.com",
        callback: async (response) => {
          if (!response.credential) {
            setError("Đăng nhập Google thất bại");
            return;
          }

          setLoading(true);
          try {
            const res = await fetch(`${BASE_URL}/Users/google-login`, {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ token: response.credential }),
            });

            const data = await res.json();
            if (data.accessToken) {
              // Decode exp
              const payload = JSON.parse(atob(data.accessToken.split(".")[1]));
              const exp = payload.exp * 1000;

              localStorage.setItem("accessToken", data.accessToken);
              localStorage.setItem("accessTokenExp", exp);
              localStorage.setItem("role", data.user?.role || "Customer");
              localStorage.setItem("user", JSON.stringify(data.user || {}));

              window.location.href = data.user?.role === "Admin" ? "/admin" : "/";
            } else {
              setError("Không nhận được token từ server");
            }
          } catch (err) {
            setError("Lỗi kết nối server");
          } finally {
            setLoading(false);
          }
        },
      });

      window.google.accounts.id.renderButton(
        document.getElementById("googleSignInButton"),
        { theme: "outline", size: "large", text: "continue_with", width: "340", logo_alignment: "left" }
      );
    };
  }, []);

  // ==================== LOGIN THƯỜNG ====================
  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await fetch(`${BASE_URL}/Users/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json", accept: "*/*" },
        body: JSON.stringify({
          email: email.trim().toLowerCase(),
          password,
        }),
      });

      if (!res.ok) {
        const err = await res.text();
        throw new Error(err.message || "Email hoặc mật khẩu không đúng");
      }

      const data = await res.json();
      const accessToken = data.accessToken;

      // Decode exp
      const payload = JSON.parse(atob(accessToken.split(".")[1]));
      const exp = payload.exp * 1000;

      // Giải mã role
      const role =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        payload.role ||
        "Customer";

      localStorage.setItem("accessToken", accessToken);
      localStorage.setItem("accessTokenExp", exp);
      localStorage.setItem("role", role);
      localStorage.setItem("user", JSON.stringify({ email, role }));

      if (role === "Admin") window.location.href = "/admin";
      else if (role === "Seller") window.location.href = "/seller";
      else window.location.href = "/";
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <link
        rel="stylesheet"
        href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.6.0/css/all.min.css"
        crossOrigin="anonymous"
        referrerPolicy="no-referrer"
      />

      <div className={styles.container}>
        <div className={styles.overlay}></div>

        <div className={styles.card}>
          <div className={styles.header}>
            <div className={styles.logoCircle}>
              <img
                src="https://aocaulongthietke.com/wp-content/uploads/2022/10/Mau-logo-doi-club-cau-lac-bo-cau-long-thiet-ke-dep-1-400x400.png"
                alt="Shop Cầu Lông BMT"
                className={styles.logo}
              />
            </div>
            <h1>SHOP CẦU LÔNG BMT</h1>
            <p>Hệ thống quản trị • Nhân viên • Kho hàng</p>
          </div>

          <form onSubmit={handleLogin} className={styles.form}>
            <div className={styles.inputGroup}>
              <label><i className="fas fa-envelope me-2"></i>Email</label>
              <input
                type="email"
                placeholder="admin@shopbmt.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={loading}
                required
                className={styles.input}
              />
            </div>

            <div className={styles.inputGroup}>
              <label><i className="fas fa-lock me-2"></i>Mật khẩu</label>
              <div className={styles.passwordWrapper}>
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={loading}
                  required
                  className={styles.input}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className={styles.eyeBtn}
                >
                  <i className={showPassword ? "fas fa-eye-slash" : "fas fa-eye"}></i>
                </button>
              </div>
            </div>

            {error && <div className={styles.error}>{error}</div>}

            <button type="submit" disabled={loading} className={styles.primaryBtn}>
              {loading ? <><i className="fas fa-spinner fa-spin"></i> Đang đăng nhập...</> : "ĐĂNG NHẬP NGAY"}
            </button>
          </form>

          <div className={styles.divider}>
            <span>HOẶC</span>
          </div>

          <div id="googleSignInButton" style={{ margin: "20px auto", display: "flex", justifyContent: "center" }}></div>

          <div className={styles.footer}>
            © 2025 Shop Cầu Lông BMT • Được xây dựng với Team BMT
          </div>
        </div>
      </div>
    </>
  );
};

export default LoginPage;
