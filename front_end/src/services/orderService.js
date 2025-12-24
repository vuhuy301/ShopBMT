const BASE_URL = process.env.REACT_APP_API_URL;

export const placeOrder = async (orderData) => {
    const token = localStorage.getItem("accessToken");

    const response = await fetch(`${BASE_URL}/Orders/create`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: token ? `Bearer ${token}` : ""
        },
        body: JSON.stringify(orderData),
    });

    if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || "Đặt hàng thất bại");
    }

    return await response.json();
};

export const getOrderById = async (id) => {
  const res = await fetch(`${BASE_URL}/Orders/${id}`);

  if (!res.ok) {
    throw new Error("Không lấy được đơn hàng");
  }

  return await res.json();
};