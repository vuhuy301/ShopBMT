// src/pages/auth/LoginPage.js
import React, { useState } from "react";
import styles from "./LoginPage.module.css";

const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Giải mã JWT thuần JS
  const parseJwt = (token) => {
    try {
      const base64Url = token.split(".")[1];
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      );
      return JSON.parse(jsonPayload);
    } catch {
      return {};
    }
  };

  // LOGIN THƯỜNG
  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const res = await fetch(`${BASE_URL}/api/Users/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          accept: "*/*",
        },
        body: JSON.stringify({
          email: email.trim().toLowerCase(),
          password,
        }),
      });

      if (!res.ok) {
        const err = await res.text();
        throw new Error(err || "Email hoặc mật khẩu không đúng");
      }

      const { accessToken } = await res.json();
      const payload = parseJwt(accessToken);

      const role =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        payload["role"] ||
        "Customer";

      const fullName =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
        payload["unique_name"] ||
        payload["sub"] ||
        email.split("@")[0];

      const userEmail =
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ||
        payload["email"] ||
        email;

      localStorage.setItem("accessToken", accessToken);
      localStorage.setItem("role", role);
      localStorage.setItem("user", JSON.stringify({ email: userEmail, fullName, role }));

      // Điều hướng
      if (role === "Admin") window.location.href = "/admin/dashboard";
      else if (role === "Employee") window.location.href = "/employee";
      else window.location.href = "/";
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // GOOGLE LOGIN (chỉ cần thay CLIENT_ID thật khi deploy)
  const handleGoogleLogin = () => {
    const clientId = "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"; // ← THAY DÒNG NÀY KHI ĐI PRODUCTION
    const redirectUri = encodeURIComponent(`${window.location.origin}/google-callback.html`);
    const scope = "email profile openid";

    const authUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=token&scope=${scope}`;

    const width = 500, height = 600;
    const left = window.screenX + (window.outerWidth - width) / 2;
    const top = window.screenY + (window.outerHeight - height) / 2;

    const popup = window.open(
      authUrl,
      "google-login",
      `width=${width},height=${height},left=${left},top=${top},resizable=yes,scrollbars=yes`
    );

    const check = setInterval(() => {
      if (!popup || popup.closed) {
        clearInterval(check);
        return;
      }
      try {
        if (popup.location.href.includes("access_token")) {
          const hash = popup.location.hash.substring(1);
          const params = new URLSearchParams(hash);
          const googleToken = params.get("access_token");

          if (googleToken) {
            fetch(`${BASE_URL}/api/Users/google-login`, {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ token: googleToken }),
            })
              .then((r) => r.json())
              .then((data) => {
                if (data.accessToken) {
                  localStorage.setItem("accessToken", data.accessToken);
                  localStorage.setItem("role", data.user?.role || "Customer");
                  localStorage.setItem("user", JSON.stringify(data.user || {}));
                  window.location.href = data.user?.role === "Admin" ? "/admin/dashboard" : "/";
                }
              })
              .catch(() => setError("Google login thất bại"));
          }
          popup.close();
          clearInterval(check);
        }
      } catch {}
    }, 500);
  };

  return (
    <>
      <link
        rel="stylesheet"
        href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.6.0/css/all.min.css"
        integrity="sha512-Kc323vGBEqzTmouAECnVceyQqyqdsSiqLQISBL29aUW4U/M7pSPA/gEUZQqv1cwx4OnYxTxve5UMg5GT6L4JJg=="
        crossOrigin="anonymous"
        referrerPolicy="no-referrer"
      />

      <div className={styles.container}>
        <div className={styles.overlay}></div>

        <div className={styles.card}>
          {/* Header */}
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

          {/* Form */}
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
              {loading ? (
                <>
                  <i className="fas fa-spinner fa-spin"></i> Đang đăng nhập...
                </>
              ) : (
                "ĐĂNG NHẬP NGAY"
              )}
            </button>
          </form>

          {/* Divider */}
          <div className={styles.divider}>
            <span>HOẶC</span>
          </div>

          {/* Google Login */}
          <button onClick={handleGoogleLogin} className={styles.googleBtn}>
            <img src="/google-icon.svg" alt="G" width="20" height="20" />
            Đăng nhập bằng Google
          </button>

          {/* Test accounts (xóa khi deploy) */}
          <div className={styles.testAccounts}>
            <small>
              <strong>Test nhanh:</strong><br />
              Admin: <code>admin@shopbmt.com</code> / 123456<br />
              Nhân viên: <code>employee@shopbmt.com</code> / 123456
            </small>
          </div>

          <div className={styles.footer}>
            © 2025 Shop Cầu Lông BMT • Được xây dựng với ❤️ bởi Team BMT
          </div>
        </div>
      </div>
    </>
  );
};

export default LoginPage;