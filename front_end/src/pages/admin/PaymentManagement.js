import React, { useEffect, useState } from "react";
import styles from "./PaymentManagement.module.css";
import { getPayments } from "../../services/paymentService";
import { getOrderById } from "../../services/orderService";

export default function PaymentManagement() {
    const [payments, setPayments] = useState([]);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [totalPages, setTotalPages] = useState(1);

    const [orderIdSearch, setOrderIdSearch] = useState("");
       const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [loading, setLoading] = useState(false);

    // modal state
    const [openOrderModal, setOpenOrderModal] = useState(false);
    const [selectedOrder, setSelectedOrder] = useState(null);

    const loadData = async () => {
        try {
            setLoading(true);
            const data = await getPayments({
                page,
                pageSize,
                orderId: orderIdSearch,
                startDate,
                endDate,
            });
            setPayments(data.items ?? []);
            setTotalPages(data.totalPages ?? 1);
        } catch (err) {
            console.error(err);
            alert("Load payments failed");
        } finally {
            setLoading(false);
        }
    };

    // get order by orderId
    const viewOrder = async (orderId) => {
        if (!orderId) return;
        try {
            const data = await getOrderById(orderId);
            setSelectedOrder(data);
            setOpenOrderModal(true);
        } catch (err) {
            console.error(err);
            alert("Không lấy được thông tin đơn hàng");
        }
    };

    useEffect(() => {
        loadData();
    }, [page, orderIdSearch, startDate, endDate]);

    return (
        <div className={styles.container}>
            <h2 className={styles.title}>Quản lý Payment</h2>

            {/* FILTER */}
            <div className={styles.form}>
                <input
                    type="text"
                    placeholder="Tìm theo Order ID..."
                    value={orderIdSearch}
                    onChange={(e) => { setPage(1); setOrderIdSearch(e.target.value); }}
                />
                <input
                    type="date"
                    value={startDate}
                    onChange={(e) => { setPage(1); setStartDate(e.target.value); }}
                />
                <input
                    type="date"
                    value={endDate}
                    onChange={(e) => { setPage(1); setEndDate(e.target.value); }}
                />
            </div>

            {/* TABLE */}
            {loading ? (
                <p>Đang tải...</p>
            ) : (
                <table className={styles.table}>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>ID đơn hàng</th>
                            <th>Số tiền</th>
                            <th>Nội dung giao dịch</th>
                            <th>Trạng thái</th>
                            <th>Thời gian đặt hàng</th>
                            <th>Thời gian thanh toán</th>
                            <th>Xem</th>
                        </tr>
                    </thead>
                    <tbody>
                        {payments.length === 0 ? (
                            <tr>
                                <td colSpan="9" style={{ textAlign: "center" }}>Không có dữ liệu</td>
                            </tr>
                        ) : (
                            payments.map((p) => {
                                let raw = {};
                                if (p.rawResponse) {
                                    try { raw = JSON.parse(p.rawResponse); } catch { }
                                }

                                return (
                                    <tr key={p.id}>
                                        <td>{p.id}</td>
                                        <td>{p.orderId}</td>
                                        <td>{p.amount?.toLocaleString()}</td>
                                        <td>{raw.Content || ""}</td>
                                        <td>{p.status}</td>
                                        <td>{new Date(p.createdAt).toLocaleString()}</td>
                                        <td>{p.paidAt ? new Date(p.paidAt).toLocaleString() : ""}</td>
                                        <td>
                                            <button className="btn btn-warning" onClick={() => viewOrder(p.orderId)}>
                                                Xem đơn
                                            </button>
                                        </td>
                                    </tr>
                                );
                            })
                        )}
                    </tbody>
                </table>
            )}

            {/* PAGINATION */}
            <div className={styles.pagination}>
                <button disabled={page === 1} onClick={() => setPage(page - 1)}>← Trước</button>
                <span>Trang {page} / {totalPages}</span>
                <button disabled={page === totalPages} onClick={() => setPage(page + 1)}>Sau →</button>
            </div>

            {/* === BASIC ORDER INFO MODAL === */}
            {openOrderModal && selectedOrder && (
                <div style={overlayStyle}>
                    <div style={modalStyle}>
                        <h3>Thông tin đơn #{selectedOrder.id}</h3>

                        <p><strong>Khách hàng:</strong> {selectedOrder.customerName}</p>
                        <p><strong>Email:</strong> {selectedOrder.email}</p>
                        <p><strong>SĐT:</strong> {selectedOrder.phone}</p>
                        <p><strong>Địa chỉ:</strong> {selectedOrder.shippingAddress}</p>
                        <p><strong>Tổng tiền:</strong> {selectedOrder.totalAmount?.toLocaleString()}₫</p>
                        <p><strong>Trạng thái:</strong> {selectedOrder.status}</p>
                        <p><strong>Ngày tạo:</strong> {new Date(selectedOrder.createdAt).toLocaleString()}</p>

                        <button onClick={() => setOpenOrderModal(false)} style={closeBtnStyle}>
                            Đóng
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

/* ==== STYLE ==== */
const overlayStyle = {
    position: "fixed",
    top: 0, left: 0,
    width: "100%", height: "100%",
    background: "rgba(0,0,0,0.4)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 999,
};

const modalStyle = {
    background: "white",
    padding: "20px",
    width: "350px",
    borderRadius: "8px",
};

const closeBtnStyle = {
    marginTop: "15px",
    padding: "8px 14px",
    border: "none",
    borderRadius: "5px",
    background: "#007bff",
    color: "white",
    cursor: "pointer",
};
