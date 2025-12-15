const BASE_URL = process.env.REACT_APP_API_URL;

export const getOrders = async ({
    page = 1,
    pageSize = 10,
    status = "",
    fromDate = "",
    toDate = ""
}) => {
    const params = new URLSearchParams();
    params.append("Page", page);
    params.append("PageSize", pageSize);

    if (status) params.append("Status", status);
    if (fromDate) params.append("FromDate", fromDate);
    if (toDate) params.append("ToDate", toDate);

    const response = await fetch(
        `${BASE_URL}/Orders/all?${params.toString()}`
    );

    if (!response.ok) {
        throw new Error("Không tải được danh sách đơn hàng");
    }

    return await response.json();
};
