import { fetchWithToken } from "../utils/fetchWithToken";

export const createBanner = async ({ imageFile, imageUrl, isActive }) => {
  if (!imageFile) throw new Error("Vui lòng chọn file ảnh");

  const formData = new FormData();
  formData.append("Image", imageFile);
  if (imageUrl) formData.append("imageUrl", imageUrl);
  formData.append("isActive", isActive ? "true" : "false"); // backend convert sang bool

  const res = await fetchWithToken("/Banners", {
    method: "POST",
    body: formData,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Thêm banner thất bại");
  }

  return await res.json();
};

export const getAllBanners = async () => {
  const res = await fetchWithToken("/Banners");

  if (!res.ok) {
    throw new Error("Không lấy được danh sách banner");
  }

  return await res.json();
};

export const updateBanner = async ({ id, imageFile, link, isActive }) => {
  const formData = new FormData();

  if (imageFile) formData.append("Image", imageFile);
  if (link) formData.append("Link", link);
  formData.append("IsActive", isActive ? "true" : "false");

  const res = await fetchWithToken(`/Banners/${id}`, {
    method: "PUT", // hoặc PATCH tùy backend
    body: formData,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Cập nhật banner thất bại");
  }

  return await res.json();
};
