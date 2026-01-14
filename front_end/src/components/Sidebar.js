import React from "react";
import { NavLink, useNavigate } from "react-router-dom";
import {
  BiSolidDashboard,
  BiAlignRight,
  BiSolidHourglass,
  BiSolidUserDetail,
  BiPackage,
  BiInfoSquare,
  BiLogOut,
  BiSolidReport,
  BiCreditCard
} from "react-icons/bi";

const Sidebar = () => {
  const navigate = useNavigate();

  // Lấy role từ localStorage
  const role = (localStorage.getItem("role") || "").toLowerCase();

  const menuConfig = {
    admin: [
      { path: "/admin/dashboard", icon: <BiSolidDashboard />, label: "Dashboard" },
      { path: "/admin/product", icon: <BiPackage />, label: "Sản phẩm" },
      { path: "/admin/orders", icon: <BiAlignRight />, label: "Đơn hàng" },
      { path: "/admin/categories", icon: <BiSolidUserDetail />, label: "Danh mục sản phẩm" },
      { path: "/admin/brands", icon: <BiSolidHourglass />, label: "Thương hiệu" },
      { path: "/admin/promotions", icon: <BiSolidHourglass />, label: "Ưu đãi" },
      { path: "/admin/banners", icon: <BiInfoSquare />, label: "Banner" },
      { path: "/admin/payments", icon: <BiCreditCard />, label: "Giao dịch" },
      { path: "/admin/users", icon: <BiCreditCard />, label: "Người dùng" },
    ],

    seller: [
      { path: "/seller/orders", icon: <BiAlignRight />, label: "Đơn hàng" },
      { path: "/seller/product", icon: <BiPackage />, label: "Sản phẩm" },
    ],

    guest: []
  };

  const menuItems = menuConfig[role] || [];

  const handleLogout = () => {
    localStorage.clear();
    navigate("/login");
  };

  return (
    <div className="sidebar d-none d-md-block">
      <div className="mb-4">
        <h5
      className="d-flex align-items-center"
      style={{ cursor: "pointer" }}
      onClick={() => navigate("/")}
    >
      Shop cầu lông
    </h5>
      </div>

      <nav className="nav flex-column">
        {menuItems.map((item, index) => (
          <NavLink key={index} to={item.path} className="nav-link" end={false}>
            {item.icon} {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="mb-4 border-top pt-3 text-danger">
        <button onClick={handleLogout} type="button" className="btn btn-default btn-sm">
          <BiLogOut size={18} /> Log out
        </button>
      </div>
    </div>
  );
};

export default Sidebar;
