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


export const getProductById = async (id) => {
  try {
    const res = await fetch(`${BASE_URL}/Products/${id}`);
    if (!res.ok) throw new Error("Failed to fetch product");
    return await res.json();
  } catch (error) {
    console.error("Error:", error);
    return null;
  }
};

export const getTopNewProductsByCategory = async (categoryId) => {
  try {
    const response = await fetch(`${BASE_URL}/Products/top-new/category/${categoryId}`);
    
    if (!response.ok) {
      throw new Error("Failed to fetch top new products");
    }

    const data = await response.json();
    return data; // trả về mảng sản phẩm
  } catch (error) {
    console.error("Error fetching top new products:", error);
    return []; // trả về mảng rỗng khi lỗi
  }
};