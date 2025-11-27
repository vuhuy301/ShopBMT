import React from "react";
import { NavLink } from "react-router-dom";
import "./AdminSidebar.module.css";

const AdminSidebar = () => {
  return (
    <div className="admin-sidebar">
      <NavLink to="/admin" className="sidebar-item" end>
        📊 Tổng quan
      </NavLink>
      <NavLink to="/admin/products" className="sidebar-item">
        🏸 Sản phẩm
      </NavLink>
    </div>
  );
};

export default AdminSidebar;