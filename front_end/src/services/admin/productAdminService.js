// src/services/admin/productAdminService.js
const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

// CHỈ ĐỌC BODY 1 LẦN DUY NHẤT → KHÔNG BAO GIỜ BỊ LỖI "body stream already read"
const handleError = async (response) => {
  if (response.ok) return null;

  let message = "Lỗi không xác định";
  const contentType = response.headers.get("content-type");

  try {
    if (contentType && contentType.includes("application/json")) {
      const errorData = await response.json();
      message = errorData.message || errorData.title || JSON.stringify(errorData);
    } else {
      // Không phải JSON → đọc text
      message = await response.text();
    }
  } catch (err) {
    // Nếu đọc lỗi (rất hiếm), fallback
    message = `HTTP ${response.status} - ${response.statusText}`;
  }

  throw new Error(message || `HTTP ${response.status}`);
};

// Header mặc định cho JSON
const jsonHeaders = {
  "Content-Type": "application/json",
  Accept: "application/json",
};

// === PRODUCT ===
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
    body: formData, // KHÔNG SET headers → browser tự thêm boundary
  });

  const err = await handleError(response);
  if (err) throw err;

  // Nếu backend trả JSON → parse, nếu không → trả về raw
  const contentType = response.headers.get("content-type");
  if (contentType && contentType.includes("application/json")) {
    return response.json();
  }
  return response; // hoặc return true nếu không cần data
};

export const updateProduct = async (id, formData) => {
  const response = await fetch(`${BASE_URL}/admin/AdminProducts/${id}`, {
    method: "PUT",
    body: formData,
  });

  const err = await handleError(response);
  if (err) throw err;

  const contentType = response.headers.get("content-type");
  if (contentType && contentType.includes("application/json")) {
    return response.json();
  }
  return response;
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

// === BRAND & CATEGORY ===
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