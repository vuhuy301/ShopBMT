import React from "react";
import "./AdminHeader.module.css";

const AdminHeader = () => {
  return (
    <header className="admin-header">
      <div className="admin-header-content">
        <h1>🏸 ADMIN - Shop Cầu Lông</h1>
        <span>Xin chào, Admin</span>
      </div>
    </header>
  );
};

export default AdminHeader;