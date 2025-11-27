const BASE_URL = process.env.REACT_APP_API_URL

export const getCategories = async () => {
  try {
    const response = await fetch(`${BASE_URL}/Categories`);
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