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
        client_id:
          "1013063153881-rd02h43rtk2rtsnjtvla6jlm94mt66f5.apps.googleusercontent.com",
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
              const payload = JSON.parse(atob(data.accessToken.split(".")[1]));
              const exp = payload.exp * 2000;

              localStorage.setItem("accessToken", data.accessToken);
              localStorage.setItem("accessTokenExp", exp);
              localStorage.setItem("role", data.user?.role || "Customer");
              localStorage.setItem("user", JSON.stringify(data.user || {}));

              window.location.href =
                data.user?.role === "Admin" ? "/admin" : "/";
            } else {
              setError("Không nhận được token từ server");
            }
          } catch {
            setError("Lỗi kết nối server");
          } finally {
            setLoading(false);
          }
        },
      });

      window.google.accounts.id.renderButton(
        document.getElementById("googleSignInButton"),
        {
          theme: "outline",
          size: "large",
          text: "continue_with",
          width: "340",
        }
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
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: email.trim().toLowerCase(),
          password,
        }),
      });

      if (!res.ok) throw new Error("Email hoặc mật khẩu không đúng");

      const data = await res.json();
      const payload = JSON.parse(atob(data.accessToken.split(".")[1]));
      const exp = payload.exp * 2000;

      const role =
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
        payload.role ||
        "Customer";

      localStorage.setItem("accessToken", data.accessToken);
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
    <div className={styles.container}>
      <div className={styles.card}>
        <h1 className={styles.title}>SHOP CẦU LÔNG BMT</h1>
        <p className={styles.subtitle}>
          Đăng nhập hệ thống quản lý
        </p>

        <form onSubmit={handleLogin} className={styles.form}>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            disabled={loading}
            required
          />

          <div className={styles.passwordBox}>
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Mật khẩu"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={loading}
              required
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
            >
              👁
            </button>
          </div>

          {error && <div className={styles.error}>{error}</div>}

          <button type="submit" disabled={loading} className={styles.loginBtn}>
            {loading ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        <div className={styles.divider}>HOẶC</div>

        <div id="googleSignInButton" className={styles.googleBtn}></div>

        {/* 🔥 NÚT ĐĂNG KÝ */}
        <div className={styles.registerBox}>
          <span>Bạn chưa có tài khoản?</span>
          <button
            onClick={() => (window.location.href = "/register")}
            className={styles.registerBtn}
          >
            Đăng ký ngay
          </button>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
