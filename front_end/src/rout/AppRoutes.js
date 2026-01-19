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
import RegisterPage from "../pages/RegisterPage";

import AdminLayout from "../layouts/AdminLayout";
import ProductAdmin from "../pages/admin/ProductAdmin";
import AddProductPage from "../pages/admin/AddProductPage";
import ProductDetailPage from "../pages/admin/ProductDetailPage";
import ProductUpdatePage from "../pages/admin/UpdateProductPage";
import BrandManagement from "../pages/admin/BrandManagement";
import CategoryManagement from "../pages/admin/CategoryManagement";
import OrderManagement from "../pages/admin/OrderManagement";
import BannerManager from "../pages/admin/BannerManagement";
import PaymentPage from "../pages/PaymentPage";
import OrderDetailPage from "../pages/OrderDetailPage";
import UserManagement from "../pages/admin/UserManagement";
import PaymentManagement from "../pages/admin/PaymentManagement";
import DashboardPage from "../pages/admin/DashboardPage";
import PromotionPage from "../pages/admin/PromotionPage";

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
      <Route path="/payment/:orderId" element={<PaymentPage />} />
      <Route path="/my-order/:id" element={<OrderDetailPage />} />
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
        <Route path="/register" element={<RegisterPage />} />

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
          <Route index element={<Navigate to="/admin/dashboard" replace />} />
          <Route path="/admin/dashboard" element={<DashboardPage />} />
          <Route path="/admin/product" element={<ProductAdmin />} />
          <Route path="/admin/product/add-product" element={<AddProductPage />} />
          <Route path="/admin/product/:id" element={<ProductDetailPage />} />
          <Route path="/admin/product/:id/edit" element={<ProductUpdatePage />} />
          <Route path="/admin/brands" element={<BrandManagement />} />
          <Route path="/admin/categories" element={<CategoryManagement />} />
          <Route path="/admin/orders" element={<OrderManagement />} />
          <Route path="/admin/banners" element={<BannerManager />} />
          <Route path="/admin/users" element={<UserManagement />} />
          <Route path="/admin/payments" element={<PaymentManagement />} />
          <Route path="/admin/promotions" element={<PromotionPage />} />
        </Route>

         <Route
          path="/seller"
          element={
            <RoleRoute allowedRoles={["Seller"]}>
              <AdminLayout />
            </RoleRoute>
          }
        >
          <Route index element={<Navigate to="/seller/orders" replace />} />
          <Route path="/seller/product" element={<ProductAdmin />} />
          <Route path="/seller/product/add-product" element={<AddProductPage />} />
          <Route path="/seller/product/:id" element={<ProductDetailPage />} />
          <Route path="/seller/product/:id/edit" element={<ProductUpdatePage />} />
          <Route path="/seller/orders" element={<OrderManagement />} />
        </Route>


      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;
