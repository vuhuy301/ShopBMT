import { fetchWithToken } from "../../utils/fetchWithToken";

/**
 * Lấy KPI tổng quan theo năm hoặc tháng
 * @param {number} year năm cần thống kê
 * @param {number} [month] tháng cần thống kê (tùy chọn)
 */
export const getDashboardKpi = async (year, month) => {
  const url = month
    ? `/admin/dashboard/kpi?year=${year}&month=${month}`
    : `/admin/dashboard/kpi?year=${year}`;

  const res = await fetchWithToken(url);
  if (!res.ok) {
    const err = await res.json();
    throw new Error(err?.message || "Load KPI failed");
  }
  return await res.json();
};

/**
 * Doanh thu theo từng ngày trong khoảng days gần nhất
 * @param {number} days số ngày muốn thống kê (mặc định 30)
 */
export const getRevenueByDays = async (days = 30) => {
  const res = await fetchWithToken(`/admin/dashboard/revenue-by-days?days=${days}`);
  if (!res.ok) {
    const err = await res.json();
    throw new Error(err?.message || "Load revenue chart failed");
  }
  return await res.json();
};

/**
 * Top sản phẩm bán chạy
 * @param {number} top số sản phẩm muốn lấy (mặc định 5)
 */
export const getTopProducts = async (top = 5) => {
  const res = await fetchWithToken(`/admin/dashboard/top-products?top=${top}`);
  if (!res.ok) {
    const err = await res.json();
    throw new Error(err?.message || "Load top products failed");
  }
  return await res.json();
};
