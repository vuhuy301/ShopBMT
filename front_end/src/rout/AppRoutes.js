// // AppRouter.js
// import React from "react";
// import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
// import Header from "../components/Header";
// import Homepage from "../pages/HomePage";
// import ProductList from "../pages/ProductList";
// import ProductDetails from "../pages/ProductDetails";
// import Footer from "../components/Footer";
// // import các trang khác nếu có
// // import About from "./pages/About";
// // import Contact from "./pages/Contact";

// const AppRouter = () => {
//   return (
//     <BrowserRouter>
//       {/* Header dùng chung cho tất cả trang user */}
//       <Header />

//       <Routes>
//         <Route path="/" element={<Homepage />} />
//          <Route path="/products" element={<ProductList />} />
//          <Route path="/product/:id" element={<ProductDetails />} />
//         <Route path="*" element={<Navigate to="/" />} />
//       </Routes>

//         <Footer />
//     </BrowserRouter>
//   );
// };

// export default AppRouter;
// AppRouter.js
import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Header from "../components/Header";
import Footer from "../components/Footer";
import Homepage from "../pages/HomePage";
import ProductList from "../pages/ProductList";
import ProductDetails from "../pages/ProductDetails";

// ADMIN
import AdminLayout from "../layouts/AdminLayout";

const UserLayout = () => (
  <>
    <Header />
    <Routes>
      <Route path="/" element={<Homepage />} />
      <Route path="/category/:categoryId" element={<ProductList />} />
      <Route path="/product/:id" element={<ProductDetails />} />
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
    <Footer />
  </>
);

const AppRouter = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/*" element={<UserLayout />} />
        <Route path="/admin/*" element={<AdminLayout />} />
      </Routes>
    </BrowserRouter>
  );
};

export default AppRouter;
