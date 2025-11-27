import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import AdminHeader from "../components/admin/AdminHeader";
import AdminSidebar from "../components/admin/AdminSidebar";
import DashboardPage from "../pages/admin/DashboardPage";
import "./AdminLayout.module.css";

const AdminLayout = () => {
  return (
    <div className="admin-layout">
      <AdminHeader />
      <div className="admin-container">
        <AdminSidebar />
        <main className="admin-main">
          <Routes>
            <Route index element={<DashboardPage />} />
            <Route path="*" element={<Navigate to="/admin" />} />
          </Routes>
        </main>
      </div>
    </div>
  );
};

export default AdminLayout;