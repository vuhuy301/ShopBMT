const BASE_URL = process.env.REACT_APP_API_URL;

export const getOrders = async ({
    page = 1,
    pageSize = 10,
    status = "",
    fromDate = "",
    toDate = "",
    orderId,       // mới
    phone          // mới
}) => {
    const params = new URLSearchParams();
    params.append("Page", page);
    params.append("PageSize", pageSize);

    if (status) params.append("Status", status);
    if (fromDate) params.append("FromDate", fromDate);
    if (toDate) params.append("ToDate", toDate);
    if (orderId) params.append("OrderId", orderId);  // thêm
    if (phone) params.append("Phone", phone);        // thêm

    const response = await fetch(
        `${BASE_URL}/Orders/all?${params.toString()}`
    );

    if (!response.ok) {
        throw new Error("Không tải được danh sách đơn hàng");
    }

    return await response.json();
};


export const updateOrderStatus = async (orderId, newStatus) => {
    const response = await fetch(`${BASE_URL}/Orders/${orderId}/status`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ newStatus })
    });

    if (!response.ok) {
        throw new Error("Cập nhật trạng thái thất bại");
    }

    return await response.json();
};