import { fetchWithToken } from "../utils/fetchWithToken";

// Lấy tất cả brand đang active
export const getBrands = async () => {
  try {
    const res = await fetchWithToken("/admin/AdminBrands",{} ,false);
    if (!res.ok) throw new Error("Failed to fetch brands");

    const data = await res.json();
    return data.filter(b => b.isActive); // lọc brand active
  } catch (err) {
    console.error("Error fetching brands:", err);
    return [];
  }
};

// Lấy tất cả brand (không lọc)
export const getAllBrands = async () => {
  try {
    const res = await fetchWithToken("/admin/AdminBrands",{},false);
    if (!res.ok) throw new Error("Failed to fetch brands");
    return await res.json();
  } catch (err) {
    console.error("Error fetching brands:", err);
    return [];
  }
};

// Thêm brand mới
export const addBrand = async (brandName) => {
  try {
    const res = await fetchWithToken("/admin/AdminBrands", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name: brandName }),
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.message || "Thêm thương hiệu thất bại");
    }

    return data;
  } catch (err) {
    console.error("Error adding brand:", err.message);
    throw err; // ⚠️ NÉM LÊN COMPONENT
  }
};


// Cập nhật brand
export const updateBrand = async (id, name) => {
  try {
    const res = await fetchWithToken(`/admin/AdminBrands/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name }),
    });

    const data = await res.json();

    if (!res.ok) {
      throw new Error(data.message || "Cập nhật thương hiệu thất bại");
    }

    return data;
  } catch (err) {
    console.error("Update brand error:", err.message);
    throw err;
  }
};


// Toggle trạng thái active của brand
export const toggleBrandActive = async (id, isActive) => {
  try {
    const res = await fetchWithToken(`/admin/AdminBrands/${id}/toggle?isActive=${isActive}`, {
      method: "PATCH",
    });

    if (!res.ok) throw new Error("Toggle brand active thất bại");
    return await res.json();
  } catch (err) {
    console.error(err);
    return null;
  }
};
