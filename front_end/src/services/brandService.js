const BASE_URL = process.env.REACT_APP_API_URL;

export const getBrands = async () => {
  try {
    const res = await fetch(`${BASE_URL}/admin/AdminBrands`);
    if (!res.ok) throw new Error("Failed to fetch brands");
    return await res.json();
  } catch (err) {
    console.error("Error fetching brands:", err);
    return [];
  }
};

export const addBrand = async (brandName) => {
  try {
    const response = await fetch(`${BASE_URL}/admin/AdminBrands`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ name: brandName })
    });

    if (!response.ok) throw new Error("Failed to add brand");
    return await response.json(); // trả về brand vừa thêm nếu backend trả về
  } catch (error) {
    console.error("Error adding brand:", error);
    return null;
  }
};

export const updateBrand = async (id, name) => {
  try {
    const res = await fetch(`${BASE_URL}/admin/AdminBrands/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name })
    });
    if (!res.ok) throw new Error("Failed to update brand");
    return await res.json();
  } catch (err) {
    console.error(err);
    return null;
  }
};


