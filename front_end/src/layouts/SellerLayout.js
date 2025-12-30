import React from "react";
import Sidebar from "../components/Sidebar";
import { Outlet } from "react-router-dom"; 
import '../components/Layout.css';
const SellerLayout = () => {
  return (
    <div>
      <Sidebar />
      <div>
        <Outlet />
      </div>
    </div>
  );
};

export default SellerLayout;