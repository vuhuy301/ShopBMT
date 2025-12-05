// src/AppRouter.jsx
import React from "react";
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";

import Header from "../components/Header";
import Footer from "../components/Footer";

import Homepage from "../pages/HomePage";
import ProductList from "../pages/ProductList";
import ProductDetails from "../pages/ProductDetails";
import LoginPage from "../pages/LoginPage";
import Cart from "../pages/Cart";
import Checkout from "../pages/Checkout";

import AdminLayout from "../layouts/AdminLayout";
import DashBoardPage from "../pages/admin/DashboardPage";

// Role-based Route
const RoleRoute = ({ children, allowedRoles = [] }) => {
  const token = localStorage.getItem("accessToken");
  const role = localStorage.getItem("role");

  if (!token) return <Navigate to="/login" replace />;
  if (!allowedRoles.includes(role)) return <Navigate to="/" replace />;

  return children;
};

// Layout khách hàng
const CustomerLayout = () => (
  <div className="layout-wrapper">
    <Header />
    <Routes>
      <Route path="/" element={<Homepage />} />
      <Route path="/category/:categoryId" element={<ProductList />} />
      <Route path="/product/:id" element={<ProductDetails />} />
      <Route path="/cart" element={<Cart />} />
      <Route path="/checkout" element={<Checkout />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
    <Footer />
  </div>
);

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>

        {/* Login */}
        <Route path="/login" element={<LoginPage />} />

        {/* ================= CUSTOMER ================= */}
        <Route path="/*" element={<CustomerLayout />} />

        {/* ================= ADMIN ================= */}
        <Route
          path="/admin"
          element={
            <RoleRoute allowedRoles={["Admin"]}>
              <AdminLayout />
            </RoleRoute>
          }
        >
          <Route index element={<Navigate to="/admin/products" replace />} />
          <Route path="/admin/products" element={<DashBoardPage />} />
        </Route>


      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;
