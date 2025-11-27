const BASE_URL = process.env.REACT_APP_API_URL;


export const getProducts = async ({ categoryId, page = 1, pageSize = 10 }) => {
  try {
    const res = await fetch(
      `${BASE_URL}/Products/filter?categoryId=${categoryId}&page=${page}&pageSize=${pageSize}`
    );
    if (!res.ok) throw new Error("Failed to fetch products");
    const data = await res.json();
    return data;
  } catch (err) {
    console.error(err);
    return { items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 10 };
  }
};