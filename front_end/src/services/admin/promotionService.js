import { fetchWithToken } from "../../utils/fetchWithToken";

export const getAllPromotions = async () => {
  const res = await fetchWithToken("/admin/AdminPromotion", {}, false);
  if (!res.ok) throw new Error("Không lấy được danh sách ưu đãi");
  return await res.json();
};

export const createPromotion = async ({ name, description }) => {
  if (!name || !name.trim()) throw new Error("Tên ưu đãi không được để trống");

  const res = await fetchWithToken("/admin/AdminPromotion", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, description }),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Thêm ưu đãi thất bại");
  }

  return await res.json();
};

export const updatePromotion = async ({ id, name, description }) => {
  if (!name || !name.trim()) throw new Error("Tên ưu đãi không được để trống");

  const res = await fetchWithToken(`/admin/AdminPromotion/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, description }),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Cập nhật ưu đãi thất bại");
  }

  return await res.json();
};

export const deletePromotion = async (id) => {
  const res = await fetchWithToken(`/admin/AdminPromotion/${id}`, { method: "DELETE" });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Xóa ưu đãi thất bại");
  }
  return true;
};
