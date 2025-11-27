// src/services/admin/productAdminService.js

const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

// Xử lý lỗi chung
const handleError = async (res) => {
  let msg = "Lỗi không xác định";
  try {
    const err = await res.json();
    msg = err.message || err.title || err.errors?.join(", ") || JSON.stringify(err);
  } catch {
    msg = await res.text();
  }
  throw new Error(msg || `HTTP ${res.status} - ${res.statusText}`);
};

// Header mặc định cho các request JSON (GET, DELETE...)
const jsonHeaders = {
  "Content-Type": "application/json",
  Accept: "application/json",
};

// === PRODUCT ===
export const getAllProducts = async () => {
  const res = await fetch(`${BASE_URL}/api/admin/AdminProducts`, {
    headers: jsonHeaders,
  });
  if (!res.ok) await handleError(res);
  return res.json();
};

export const createProduct = async (formData) => {
  const response = await fetch(`${BASE_URL}/api/admin/AdminProducts`, {
    method: "POST",
    // KHÔNG ĐƯỢC ĐỘNG VÀO headers → browser tự thêm boundary
    body: formData,
  });
  if (!response.ok) await handleError(response);
  return response.json();
};

export const updateProduct = async (id, formData) => {
  const response = await fetch(`${BASE_URL}/api/admin/AdminProducts/${id}`, {
    method: "PUT",
    // Không set headers → để browser tự sinh multipart/form-data + boundary
    body: formData,
  });
  if (!response.ok) await handleError(response);
  return response.json();
};

export const deleteProduct = async (id) => {
  const res = await fetch(`${BASE_URL}/api/admin/AdminProducts/${id}`, {
    method: "DELETE",
    headers: jsonHeaders, // DELETE không có body → có thể set JSON header
  });
  if (!res.ok) await handleError(res);
  return res.status === 204 ? true : res.json();
};

// === BRAND & CATEGORY ===
export const getAllBrands = async () => {
  const res = await fetch(`${BASE_URL}/api/admin/AdminBrands`, {
    headers: jsonHeaders,
  });
  if (!res.ok) await handleError(res);
  return res.json();
};

export const getAllCategories = async () => {
  const res = await fetch(`${BASE_URL}/api/admin/AdminCategories`, {
    headers: jsonHeaders,
  });
  if (!res.ok) await handleError(res);
  return res.json();
};