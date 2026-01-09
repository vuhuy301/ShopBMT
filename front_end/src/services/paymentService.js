import { fetchWithToken } from "../utils/fetchWithToken";
const BASE_URL = process.env.REACT_APP_API_URL;

export const paymentService = {
  // Tạo Payment
  createPayment: async (orderId) => {
    const response = await fetch(`${BASE_URL}/Payment/create`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({ orderId })
    });

    if (!response.ok) {
      throw new Error("Không thể tạo payment");
    }

    return await response.json(); // { paymentId, transactionCode, amount }
  },

  // Lấy trạng thái Order
  checkOrderStatus: async (orderId) => {
    const response = await fetch(`${BASE_URL}/Orders/${orderId}`);

    if (!response.ok) {
      throw new Error("Không lấy được trạng thái order");
    }

    return await response.json(); // { id, status, ... }
  }
};

export async function getPayments({ page = 1, pageSize = 10, orderId = "", startDate = "", endDate = "" }) {
  const params = new URLSearchParams();
  if (orderId) params.append("orderId", orderId);
  if (startDate) params.append("startDate", startDate);
  if (endDate) params.append("endDate", endDate);
  params.append("page", page);
  params.append("pageSize", pageSize);

  const res = await fetchWithToken(`/Payment?${params.toString()}`);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Failed to load payments");
  }
  return await res.json();
}

