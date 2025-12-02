// src/services/admin/productAdminService.js

const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002/api";

// Chỉ đọc body 1 lần duy nhất → không bao giờ lỗi "body already read"
const handleError = async (response) => {
  if (response.ok) return null;

  let message = "Lỗi không xác định";
  const contentType = response.headers.get("content-type");

  try {
    if (contentType && contentType.includes("application/json")) {
      const errorData = await response.json();
      message = errorData.message || errorData.title || JSON.stringify(errorData);
    } else {
      message = await response.text();
    }
  } catch (err) {
    message = `HTTP ${response.status} - ${response.statusText}`;
  }

  throw new Error(message || `HTTP ${response.status}`);
};

const jsonHeaders = {
  "Content-Type": "application/json",
  Accept: "application/json",
};

// ==================== PRODUCT ====================
export const getAllProducts = async () => {
  const res = await fetch(`${BASE_URL}/admin/AdminProducts`, {
    headers: jsonHeaders,
  });
  const err = await handleError(res);
  if (err) throw err;
  return res.json();
};

export const createProduct = async (formData) => {
  const response = await fetch(`${BASE_URL}/admin/AdminProducts`, {
    method: "POST",
    body: formData, // không set header → browser tự thêm boundary
  });
  const err = await handleError(response);
  if (err) throw err;
  const ct = response.headers.get("content-type");
  return ct && ct.includes("application/json") ? response.json() : true;
};

export const updateProduct = async (id, formData) => {
  const response = await fetch(`${BASE_URL}/admin/AdminProducts/${id}`, {
    method: "PUT",
    body: formData,
  });
  const err = await handleError(response);
  if (err) throw err;
  const ct = response.headers.get("content-type");
  return ct && ct.includes("application/json") ? response.json() : true;
};

export const deleteProduct = async (id) => {
  const res = await fetch(`${BASE_URL}/admin/AdminProducts/${id}`, {
    method: "DELETE",
    headers: jsonHeaders,
  });
  const err = await handleError(res);
  if (err) throw err;
  return res.status === 204 || res.status === 200;
};

// ==================== BRAND & CATEGORY ====================
export const getAllBrands = async () => {
  const res = await fetch(`${BASE_URL}/admin/AdminBrands`, { headers: jsonHeaders });
  const err = await handleError(res);
  if (err) throw err;
  return res.json();
};

export const getAllCategories = async () => {
  const res = await fetch(`${BASE_URL}/admin/AdminCategories`, { headers: jsonHeaders });
  const err = await handleError(res);
  if (err) throw err;
  return res.json();
};