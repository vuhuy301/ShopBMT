const BASE_URL = process.env.REACT_APP_API_URL;

export const createBanner = async ({ imageFile, imageUrl, isActive }) => {
  if (!imageFile) throw new Error("Vui lòng chọn file ảnh");

  const formData = new FormData();
  formData.append("Image", imageFile);
  if (imageUrl) formData.append("imageUrl", imageUrl);
  formData.append("isActive", isActive ? "true" : "false"); // backend convert sang bool

  const res = await fetch(`${BASE_URL}/Banners`, {
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
  const response = await fetch(`${BASE_URL}/Banners`);

  if (!response.ok) {
    throw new Error("Không lấy được danh sách banner");
  }

  return await response.json();
};

// Update banner (cả ảnh, link, isActive)
export const updateBanner = async ({ id, imageFile, link, isActive }) => {
  const formData = new FormData();

  // Nếu người dùng chọn file mới
  if (imageFile) formData.append("Image", imageFile);

  if (link) formData.append("Link", link);
  formData.append("IsActive", isActive ? "true" : "false"); // backend convert sang bool

  const res = await fetch(`${BASE_URL}/Banners/${id}`, {
    method: "PUT", // hoặc PATCH tùy backend
    body: formData,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Cập nhật banner thất bại");
  }

  return await res.json();
};
