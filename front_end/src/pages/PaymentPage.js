import React, { useEffect, useRef, useState } from "react";
import { useParams, useNavigate  } from "react-router-dom";
import { paymentService } from "../services/paymentService";

const PaymentPage = () => {
  const { orderId } = useParams();

  const navigate = useNavigate();

  const [paymentInfo, setPaymentInfo] = useState(null);
  const [loading, setLoading] = useState(true);
  const [statusMessage, setStatusMessage] = useState("");

  const pollingRef = useRef(null);
  const createdRef = useRef(false); // chỉ chặn create payment

  // 🔁 Poll trạng thái order (LUÔN chạy)
  const startPolling = () => {
    if (pollingRef.current) return;

    pollingRef.current = setInterval(async () => {
      try {
        const order = await paymentService.checkOrderStatus(orderId);

        if (order.status === "Đã thanh toán") {
          clearInterval(pollingRef.current);
          pollingRef.current = null;
          setStatusMessage("✅ Thanh toán thành công!");
        }
      } catch (err) {
        console.error("Polling error:", err);
      }
    }, 5000);
  };

  // 🆕 Chỉ tạo payment 1 lần
  const createPaymentOnce = async () => {
    if (createdRef.current) return;
    createdRef.current = true;

    try {
      const data = await paymentService.createPayment(orderId);
      setPaymentInfo(data);
    } catch (error) {
      console.error(error);
      setStatusMessage("❌ Không thể tạo mã thanh toán.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!orderId) return;

    createPaymentOnce(); // tạo (hoặc lấy) payment
    startPolling();      // 🔁 luôn poll

    return () => {
      if (pollingRef.current) {
        clearInterval(pollingRef.current);
        pollingRef.current = null;
      }
    };
  }, [orderId]);

  return (
    <div
      style={{
        maxWidth: 420,
        margin: "50px auto",
        padding: 20,
        border: "1px solid #ddd",
        borderRadius: 8,
        textAlign: "center",
      }}
    >
      <h2>Thanh toán Online</h2>

      {loading && <p>⏳ Đang tạo mã thanh toán...</p>}

      {!loading && paymentInfo && (
        <>
          <p style={{ fontWeight: "bold" }}>
            Nội dung CK: {paymentInfo.transactionCode}
          </p>

          <p>
            Số tiền:{" "}
            <strong>
              {paymentInfo.amount.toLocaleString("vi-VN")} đ
            </strong>
          </p>

          <img
            src={`https://qr.sepay.vn/img?acc=962473KB1Y&bank=BIDV&amount=${paymentInfo.amount}&des=${paymentInfo.transactionCode}`}
            alt="QR Thanh toán"
            style={{ marginTop: 20, width: 220, height: 220 }}
          />

          <p style={{ color: "red", marginTop: 12 }}>
            Vui lòng chuyển khoản đúng nội dung để hệ thống tự động xác nhận
          </p>
        </>
      )}

      {statusMessage && (
  <div style={{ marginTop: 20 }}>
    <p style={{ fontWeight: "bold", color: "green" }}>
      {statusMessage}
    </p>

    <button
      onClick={() => navigate(`/my-order/${orderId}`)}
      style={{
        marginTop: 12,
        padding: "10px 16px",
        backgroundColor: "#28a745",
        color: "#fff",
        border: "none",
        borderRadius: 6,
        cursor: "pointer",
        fontWeight: "bold",
      }}
    >
      Xem chi tiết đơn hàng
    </button>
  </div>
)}

    </div>
  );
};

export default PaymentPage;
