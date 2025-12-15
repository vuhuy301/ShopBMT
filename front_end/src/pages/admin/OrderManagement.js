import React, { useEffect, useState } from "react";
import styles from "./OrderManagement.module.css";
import { getOrders } from "../../services/admin/orderAdminService";
import { toIsoDateTime } from "../../utils/dateUtils";

const OrderManagement = () => {
    const [orders, setOrders] = useState([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);

    const [status, setStatus] = useState("");
    const [fromDate, setFromDate] = useState("");
    const [toDate, setToDate] = useState("");

    const pageSize = 5;

    useEffect(() => {
        fetchOrders();
    }, [page, status, fromDate, toDate]);

  const fetchOrders = async () => {
        try {
            const data = await getOrders({
                page,
                pageSize,
                status,
                fromDate,
                toDate,
            });

            console.log("ORDERS:", data);

            // 🔥 CỰC QUAN TRỌNG
            setOrders(Array.isArray(data) ? data : []);
        } catch (error) {
            console.error(error);
            setOrders([]);
        }
    };

    const formatPrice = (price) =>
        price.toLocaleString("vi-VN") + " đ";

    const formatDate = (date) =>
        new Date(date).toLocaleString("vi-VN");

    return (
        <div className={styles.container}>
            <h2 className={styles.title}>Quản lý đơn hàng</h2>

            {/* FILTER */}
            <div className={styles.filter}>
                <select
                    value={status}
                    onChange={(e) => {
                        setStatus(e.target.value);
                        setPage(1);
                    }}
                >
                    <option value="">Tất cả trạng thái</option>
                    <option value="Pending">Pending</option>
                    <option value="Confirmed">Confirmed</option>
                    <option value="Shipping">Shipping</option>
                    <option value="Completed">Completed</option>
                    <option value="Cancelled">Cancelled</option>
                </select>

                <input
                    type="date"
                    value={fromDate}
                    onChange={(e) => {
                        setFromDate(e.target.value);
                        setPage(1);
                    }}
                />

                <input
                    type="date"
                    value={toDate}
                    onChange={(e) => {
                        setToDate(e.target.value);
                        setPage(1);
                    }}
                />
            </div>

            {/* ORDERS */}
            {orders.map((order) => (
                <div key={order.id} className={styles.orderCard}>
                    <div className={styles.orderHeader}>
                        <div>
                            <strong>Đơn #{order.id}</strong>
                            <span className={styles.status}>
                                {order.status}
                            </span>
                        </div>
                        <div>{formatDate(order.createdAt)}</div>
                    </div>

                    <div className={styles.customer}>
                        <div><b>Khách:</b> {order.customerName}</div>
                        <div><b>SĐT:</b> {order.phone}</div>
                        <div><b>Địa chỉ:</b> {order.shippingAddress}</div>
                        {order.note && (
                            <div><b>Ghi chú:</b> {order.note}</div>
                        )}
                    </div>

                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Sản phẩm</th>
                                <th>Màu</th>
                                <th>Size</th>
                                <th>SL</th>
                                <th>Giá</th>
                                <th>Tổng</th>
                            </tr>
                        </thead>
                        <tbody>
                            {order.items.map((item) => (
                                <tr key={item.id}>
                                    <td>{item.productName}</td>
                                    <td>{item.color}</td>
                                    <td>{item.size}</td>
                                    <td>{item.quantity}</td>
                                    <td>{formatPrice(item.price)}</td>
                                    <td>{formatPrice(item.total)}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    <div className={styles.total}>
                        Tổng tiền:{" "}
                        <strong>
                            {formatPrice(order.totalAmount)}
                        </strong>
                    </div>
                </div>
            ))}

            {/* PAGINATION */}
            <div className={styles.pagination}>
                <button
                    disabled={page === 1}
                    onClick={() => setPage(page - 1)}
                >
                    ← Trước
                </button>

                <span>
                    Trang {page} / {totalPages}
                </span>

                <button
                    disabled={page === totalPages}
                    onClick={() => setPage(page + 1)}
                >
                    Sau →
                </button>
            </div>
        </div>
    );
};

export default OrderManagement;
