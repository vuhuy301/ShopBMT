// src/AppRouter.jsx
import React from "react";
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  useLocation,
} from "react-router-dom";

// Pages & Layouts
import Header from "../components/Header";
import Footer from "../components/Footer";
import Homepage from "../pages/HomePage";
import ProductList from "../pages/ProductList";
import ProductDetails from "../pages/ProductDetails";
import LoginPage from "../pages/LoginPage";
import AdminLayout from "../layouts/AdminLayout";
import Cart from "../pages/Cart";
// import EmployeeLayout from "../layouts/EmployeeLayout"; // bạn tạo sau

// Role-based Route
const RoleRoute = ({ children, allowedRoles = [] }) => {
  const token = localStorage.getItem("accessToken");
  const role = localStorage.getItem("role"); // "Admin", "Employee", "Customer"
  const location = useLocation();

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles.length > 0 && !allowedRoles.includes(role)) {
    return <Navigate to="/" replace />;
  }

  return children;
};

// Layout cho khách hàng
const CustomerLayout = () => (
  <>
    <Header />
    <Routes>
      <Route path="/" element={<Homepage />} />
      <Route path="/category/:categoryId" element={<ProductList />} />
      <Route path="/product/:id" element={<ProductDetails />} />
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route path="/cart" element={<Cart />} />
    </Routes>
    <Footer />
  </>
);

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        {/* Khách hàng */}
        <Route path="/*" element={<CustomerLayout />} />

        {/* Admin */}
        <Route
          path="/admin/*"
          element={
            <RoleRoute allowedRoles={["Admin"]}>
              <AdminLayout />
            </RoleRoute>
          }
        />

        {/* Nhân viên */}
        {/* <Route
          path="/employee/*"
          element={
            <RoleRoute allowedRoles={["Employee"]}>
              <EmployeeLayout />
            </RoleRoute>
          }
        /> */}
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;