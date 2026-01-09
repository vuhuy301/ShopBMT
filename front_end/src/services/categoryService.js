import { fetchWithToken } from "../utils/fetchWithToken";

// Lấy tất cả category đang active
export const getCategories = async () => {
  try {
    const res = await fetchWithToken("/Categories", {} ,false);
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);

    const data = await res.json();
    return data.filter(c => c.isActive); // chỉ lấy category đang hiển thị
  } catch (err) {
    console.error("Error fetching categories:", err);
    return [];
  }
};

// Lấy tất cả category (không lọc)
export const getAllCategories = async () => {
  try {
    const res = await fetchWithToken("/admin/AdminCategories");
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
    return await res.json();
  } catch (err) {
    console.error("Error fetching categories:", err);
    throw err;
  }
};

// Thêm category mới
export const addCategory = async (name, description) => {
  try {
    const res = await fetchWithToken("/admin/AdminCategories", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, description }),
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.message || "Thêm danh mục thất bại");
    }

    return data;
  } catch (err) {
    console.error("Add category error:", err.message);
    throw err; // ⚠️ QUAN TRỌNG: ném lên component xử lý
  }
};


// Cập nhật category
export const updateCategory = async (id, name, description) => {
  try {
    const res = await fetchWithToken(`/admin/AdminCategories/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, description }),
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.message || "Cập nhật danh mục thất bại");
    }

    return data;
  } catch (err) {
    console.error("Update category error:", err.message);
    throw err;
  }
};

// Toggle trạng thái active của category
export const toggleCategoryActive = async (id, isActive) => {
  try {
    const res = await fetchWithToken(`/admin/AdminCategories/${id}/toggle?isActive=${isActive}`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
    });

    if (!res.ok) throw new Error("Toggle category active thất bại");
    return await res.json();
  } catch (err) {
    console.error(err);
    return null;
  }
};
