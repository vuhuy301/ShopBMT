
import { fetchWithToken } from "../../utils/fetchWithToken";
const BASE_URL = process.env.REACT_APP_API_URL;

export const getProducts = async ({ categoryId, page = 1, pageSize = 12, search = "" }) => {
  try {
    const queryParams = new URLSearchParams({
      categoryId,
      page,
      pageSize,
    });

    if (search) queryParams.append("search", search);

    const res = await fetch(`${BASE_URL}/Products/filter?${queryParams.toString()}`);

    if (!res.ok) throw new Error("Failed to fetch products");
    const data = await res.json();
    return data;
  } catch (err) {
    console.error(err);
    return { items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 12 };
  }
};

export const createProduct = async (formData) => {
  try {
    const res = await fetchWithToken(`/Admin/AdminProducts`, {
      method: "POST",
      body: formData
    });

    if (!res.ok) throw new Error("Create product failed");

    return await res.json();
  } catch (err) {
    console.error("❌ Create product error:", err);
    throw err;
  }
};

export const getProductById = async (id) => {
  try {
    const response = await fetch(`${BASE_URL}/Products/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`Lỗi lấy sản phẩm ID: ${id}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error("getProductById error:", error);
    return null;
  }
};

export const updateProduct = async (id, formData) => {
  try {
    const response = await fetchWithToken(`/admin/AdminProducts/${id}`, {
      method: "PUT",
      body: formData, 
    });

    if (!response.ok) {
      throw new Error("Update product failed");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updateProduct:", error);
    throw error;
  }
};

export const refreshVectorData = async () => {
  try {
    const response = await fetchWithToken(
      "/admin/vector/refresh", 
      {
        method: "POST", 
      }
    );

    if (!response.ok) {
      throw new Error("Refresh vector data failed");
    }

    return await response.json();
  } catch (error) {
    console.error("Error refreshVectorData:", error);
    throw error;
  }
};