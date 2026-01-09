import React, { useEffect, useState } from "react";
import {
  getDashboardKpi,
  getRevenueByDays,
  getTopProducts,
} from "../../services/admin/dashboardService";
import { Line } from "react-chartjs-2";
import "chart.js/auto";
import styles from "./DashboardPage.module.css";

export default function DashboardPage() {
  const [kpi, setKpi] = useState(null);
  const [revenueData, setRevenueData] = useState([]);
  const [topProducts, setTopProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  const currentYear = new Date().getFullYear();
  const [year, setYear] = useState(currentYear);
  const [month, setMonth] = useState(null); // null = cả năm

  const loadDashboard = async () => {
    try {
      setLoading(true);

      const [kpiRes, revenueRes, topRes] = await Promise.all([
        getDashboardKpi(year, month),
        getRevenueByDays(30),
        getTopProducts(5),
      ]);

      setKpi(kpiRes);
      setRevenueData(revenueRes);
      setTopProducts(topRes);
    } catch (err) {
      console.error("Load dashboard failed:", err);
      alert(err.message || "Không thể load dashboard");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDashboard();
  }, [year, month]);

  const revenueChart = {
    labels: revenueData.map((d) =>
      new Date(d.date).toLocaleDateString("vi-VN")
    ),
    datasets: [
      {
        label: "Doanh thu",
        data: revenueData.map((d) => d.revenue),
        borderColor: "#4caf50",
        backgroundColor: "rgba(76, 175, 80, 0.2)",
        fill: true,
        tension: 0.3,
      },
    ],
  };

  // Danh sách 5 năm gần nhất
  const years = Array.from({ length: 6 }, (_, i) => currentYear - i);
  // Danh sách tháng
  const months = [
    { value: null, label: "Cả năm" },
    { value: 1, label: "Tháng 1" },
    { value: 2, label: "Tháng 2" },
    { value: 3, label: "Tháng 3" },
    { value: 4, label: "Tháng 4" },
    { value: 5, label: "Tháng 5" },
    { value: 6, label: "Tháng 6" },
    { value: 7, label: "Tháng 7" },
    { value: 8, label: "Tháng 8" },
    { value: 9, label: "Tháng 9" },
    { value: 10, label: "Tháng 10" },
    { value: 11, label: "Tháng 11" },
    { value: 12, label: "Tháng 12" },
  ];

  return (
    <div className={styles.container}>
      <h2>Dashboard</h2>

      {/* --- Filter năm & tháng --- */}
      <div className={styles.filterContainer}>
        <label htmlFor="yearSelect">Chọn năm:</label>
        <select
          id="yearSelect"
          value={year}
          onChange={(e) => setYear(Number(e.target.value))}
        >
          {years.map((y) => (
            <option key={y} value={y}>
              {y}
            </option>
          ))}
        </select>

        <label htmlFor="monthSelect">Chọn tháng:</label>
        <select
          id="monthSelect"
          value={month === null ? "" : month}
          onChange={(e) =>
            setMonth(e.target.value === "" ? null : Number(e.target.value))
          }
        >
          {months.map((m) => (
            <option key={m.value ?? "all"} value={m.value ?? ""}>
              {m.label}
            </option>
          ))}
        </select>
      </div>

      {loading && <p>Đang tải dữ liệu...</p>}

      {!loading && kpi && (
        <>
          {/* --- KPI --- */}
          <div className={styles.kpiContainer}>
            <div className={styles.kpiCard}>
              <h4>Tổng doanh thu</h4>
              <p>{kpi.totalRevenue?.toLocaleString("vi-VN")} đ</p>
            </div>
            <div className={styles.kpiCard}>
              <h4>Tổng đơn hàng</h4>
              <p>{kpi.totalOrders ?? 0}</p>
            </div>
            <div className={styles.kpiCard}>
              <h4>Tổng sản phảm</h4>
              <p>{kpi.totalProfit?.toLocaleString("vi-VN")} sản phẩm</p>
            </div>
          </div>

          {/* --- Biểu đồ doanh thu --- */}
          <div className={styles.chartContainer}>
            <h4>Doanh thu 30 ngày gần nhất</h4>
            <Line
              data={revenueChart}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                  legend: { display: true, position: "top" },
                },
                scales: {
                  y: {
                    ticks: {
                      callback: (value) =>
                        value.toLocaleString("vi-VN") + " đ",
                    },
                  },
                },
              }}
              height={350}
            />
          </div>

          {/* --- Top sản phẩm bán chạy --- */}
          <div className={styles.topProducts}>
            <h4>Top sản phẩm bán chạy</h4>
            <table>
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Sản phẩm</th>
                  <th>Số lượng bán</th>
                  <th>Doanh thu</th>
                </tr>
              </thead>
              <tbody>
                {topProducts.length > 0 ? (
                  topProducts.map((p, idx) => (
                    <tr key={p.productId}>
                      <td>{idx + 1}</td>
                      <td>{p.productName}</td>
                      <td>{p.quantitySold}</td>
                      <td>{p.revenue?.toLocaleString("vi-VN")} đ</td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan="4" style={{ textAlign: "center" }}>
                      Không có dữ liệu
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
