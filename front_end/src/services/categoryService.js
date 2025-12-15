const BASE_URL = process.env.REACT_APP_API_URL

export const getCategories = async () => {
  try {
    const response = await fetch(`${BASE_URL}/admin/AdminCategories`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const data = await response.json();
    return data; // trả về mảng categories
  } catch (error) {
    console.error("Error fetching categories:", error);
    throw error;
  }
};

export const addCategory = async (name, description) => {
  try {
    const res = await fetch(`${BASE_URL}/admin/AdminCategories`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, description })
    });
    if (!res.ok) throw new Error("Failed to add category");
    return await res.json();
  } catch (err) {
    console.error(err);
    return null;
  }
};

export const updateCategory = async (id, name, description) => {
  try {
    const res = await fetch(`${BASE_URL}/admin/AdminCategories/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, description })
    });
    if (!res.ok) throw new Error("Failed to update category");
    return await res.json();
  } catch (err) {
    console.error(err);
    return null;
  }
};
