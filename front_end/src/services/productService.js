const BASE_URL = process.env.REACT_APP_API_URL;

export const getProducts = async () => {
  try {
    const response = await fetch(`${BASE_URL}/Categories`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });

    if (!response.ok) {
      throw new Error("Fetch failed");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching products:", error);
    throw error;
  }
};
