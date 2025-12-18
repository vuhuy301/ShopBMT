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
      { path: "/admin/products", icon: <BiPackage />, label: "Sản phẩm" },
      { path: "/admin/orders", icon: <BiAlignRight />, label: "Đơn hàng" },
      { path: "/admin/categories", icon: <BiSolidUserDetail />, label: "Danh mục sản phẩm" },
      { path: "/admin/brands", icon: <BiSolidHourglass />, label: "Thương hiệu" },
      { path: "/admin/banners", icon: <BiInfoSquare />, label: "Banner" },
      { path: "/admin/payment", icon: <BiCreditCard />, label: "Giao dịch" },
      { path: "/admin/report", icon: <BiSolidReport />, label: "Báo cáo" },
      { path: "/admin/profile", icon: <BiInfoSquare />, label: "Thông tin cá nhân" },
    ],

    staff: [
      { path: "/staff/dashboard", icon: <BiSolidDashboard />, label: "Dashboard" },
      { path: "/staff/orders", icon: <BiAlignRight />, label: "Đơn hàng" },
      { path: "/staff/shipping", icon: <BiSolidHourglass />, label: "Vận đơn" },
      { path: "/staff/inventory", icon: <BiPackage />, label: "Kho hàng" },
      { path: "/staff/report", icon: <BiSolidReport />, label: "Báo cáo" },
      { path: "/staff/profile", icon: <BiInfoSquare />, label: "Thông tin cá nhân" },
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
        <h5 className="d-flex align-items-center">MetaSale</h5>
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
