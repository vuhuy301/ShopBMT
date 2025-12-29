import React, { useEffect, useState } from "react";
import styles from "./OrderManagement.module.css";
import { getOrders, updateOrderStatus } from "../../services/admin/orderAdminService";

const OrderManagement = () => {
  const [orders, setOrders] = useState([]);
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [newStatus, setNewStatus] = useState("");

  const [statusFilter, setStatusFilter] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [searchOrderId, setSearchOrderId] = useState("");
  const [searchPhone, setSearchPhone] = useState("");

  const pageSize = 5;

  // Luồng trạng thái hợp lệ
  const statusFlow = {
    "Chờ xác nhận": ["Đã thanh toán", "Đang giao", "Đã hủy"],
    "Đã thanh toán": ["Đang giao", "Đã hủy"],
    "Đang giao": ["Hoàn thành"],
    "Hoàn thành": [],
    "Đã hủy": []
  };

  useEffect(() => {
    fetchOrders();
  }, [page, statusFilter, fromDate, toDate, searchOrderId, searchPhone]);

  const fetchOrders = async () => {
    try {
      const data = await getOrders({
        page,
        pageSize,
        status: statusFilter || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
        orderId: searchOrderId ? Number(searchOrderId) : undefined,
        phone: searchPhone || undefined,
      });
      setOrders(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error(error);
      setOrders([]);
    }
  };

  const formatPrice = (price) => price.toLocaleString("vi-VN") + " đ";
  const formatDate = (date) => new Date(date).toLocaleString("vi-VN");

  const handleUpdateStatus = async () => {
    if (!selectedOrder) return;

    if (!newStatus) {
      alert("Vui lòng chọn trạng thái mới.");
      return;
    }

    if (!statusFlow[selectedOrder.status].includes(newStatus)) {
      alert(`Không thể cập nhật từ "${selectedOrder.status}" sang "${newStatus}"`);
      return;
    }

    const confirm = window.confirm(
      `Bạn có chắc chắn muốn cập nhật trạng thái đơn #${selectedOrder.id} sang "${newStatus}" không?`
    );
    if (!confirm) return;

    try {
      // Optimistic update
      setOrders((prev) =>
        prev.map((o) =>
          o.id === selectedOrder.id ? { ...o, status: newStatus } : o
        )
      );
      setModalOpen(false);
      setSelectedOrder(null);

      await updateOrderStatus(selectedOrder.id, newStatus);
      fetchOrders(); // đồng bộ server
    } catch (err) {
      console.error(err);
      alert("Cập nhật thất bại! Vui lòng thử lại.");
      fetchOrders(); // rollback
    }
  };

  return (
    <div className={styles.container}>
      <h2 className={styles.title}>Quản lý đơn hàng</h2>

      {/* FILTER + SEARCH */}
      <div className={styles.filter}>
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}>
          <option value="">Tất cả trạng thái</option>
          <option value="Chờ xác nhận">Chờ xác nhận</option>
          <option value="Đã thanh toán">Đã thanh toán</option>
          <option value="Đang giao">Đang giao</option>
          <option value="Hoàn thành">Hoàn thành</option>
          <option value="Đã hủy">Đã hủy</option>
        </select>

        <input type="date" value={fromDate} onChange={(e) => { setFromDate(e.target.value); setPage(1); }} />
        <input type="date" value={toDate} onChange={(e) => { setToDate(e.target.value); setPage(1); }} />
        <input type="number" placeholder="Tìm theo ID đơn" value={searchOrderId} onChange={(e) => { setSearchOrderId(e.target.value); setPage(1); }} />
        <input type="text" placeholder="Tìm theo số điện thoại" value={searchPhone} onChange={(e) => { setSearchPhone(e.target.value); setPage(1); }} />
      </div>

      {/* ORDERS */}
      {orders.map((order) => (
        <div key={order.id} className={styles.orderCard}>
          <div className={styles.orderHeader}>
            <div>
              <strong>Đơn #{order.id}</strong>
              <span className={styles.status}>{order.status}</span>
            </div>
            <div>{formatDate(order.createdAt)}</div>
          </div>

          <div className={styles.customer}>
            <div><b>Khách:</b> {order.customerName}</div>
            <div><b>SĐT:</b> {order.phone}</div>
            <div><b>Địa chỉ:</b> {order.shippingAddress}</div>
            {order.note && <div><b>Ghi chú:</b> {order.note}</div>}
          </div>

          <table className={styles.table} style={{ tableLayout: "fixed", width: "100%" }}>
            <thead>
              <tr>
                <th style={{ width: "40%" }}>Sản phẩm</th>
                <th style={{ width: "10%" }}>Màu</th>
                <th style={{ width: "10%" }}>Size</th>
                <th style={{ width: "10%" }}>SL</th>
                <th style={{ width: "15%" }}>Giá</th>
                <th style={{ width: "15%" }}>Tổng</th>
              </tr>
            </thead>
            <tbody>
              {order.items.map((item) => (
                <tr key={item.id}>
                  <td style={{ width: "40%", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{item.productName}</td>
                  <td style={{ width: "10%", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{item.color}</td>
                  <td style={{ width: "10%" }}>{item.size}</td>
                  <td style={{ width: "10%" }}>{item.quantity}</td>
                  <td style={{ width: "15%" }}>{formatPrice(item.price)}</td>
                  <td style={{ width: "15%" }}>{formatPrice(item.price * item.quantity)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className={styles.orderActions}>
            <button className="btn btn-primary" onClick={() => {
              setSelectedOrder(order);
              setNewStatus(""); // Mặc định chưa chọn trạng thái
              setModalOpen(true);
            }}>
              Cập nhật trạng thái
            </button>
            <div className={styles.total}>
              Tổng tiền: <strong>{formatPrice(order.totalAmount)}</strong>
            </div>
          </div>
        </div>
      ))}

      {/* PAGINATION */}
      <div className={styles.pagination}>
        <button disabled={page === 1} onClick={() => setPage(page - 1)}>← Trước</button>
        <span>Trang {page}</span>
        <button disabled={orders.length < pageSize} onClick={() => setPage(page + 1)}>Sau →</button>
      </div>

      {/* MODAL */}
      {modalOpen && selectedOrder && (
        <div className={styles.modalBackdrop}>
          <div className={styles.modal}>
            <h3>Cập nhật trạng thái đơn #{selectedOrder.id}</h3>

            <select value={newStatus} onChange={(e) => setNewStatus(e.target.value)}>
              <option value="" disabled>Chọn trạng thái mới</option>
              {statusFlow[selectedOrder.status].map((status) => (
                <option key={status} value={status}>{status}</option>
              ))}
            </select>

            <div className={styles.modalActions}>
              <button className="btn btn-success" onClick={handleUpdateStatus}>
                Xác nhận
              </button>
              <button className="btn btn-danger" onClick={() => { setModalOpen(false); setSelectedOrder(null); }}>
                Hủy
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrderManagement;
