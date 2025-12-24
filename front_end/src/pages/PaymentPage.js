import React, { useEffect, useState, useRef } from "react";
import { useParams } from "react-router-dom";
import { paymentService } from "../services/paymentService";

const PaymentPage = () => {
  const { orderId } = useParams();

  const [paymentInfo, setPaymentInfo] = useState(null);
  const [loading, setLoading] = useState(true);
  const [statusMessage, setStatusMessage] = useState("");

  const pollingRef = useRef(null); // tránh tạo nhiều interval

  // Tạo payment + poll trạng thái
  const createPaymentAndPolling = async () => {
    try {
      // 1️⃣ Tạo payment
      const data = await paymentService.createPayment(orderId);
      setPaymentInfo(data);

      // 2️⃣ Poll trạng thái order
      pollingRef.current = setInterval(async () => {
        const order = await paymentService.checkOrderStatus(orderId);

        if (order.status === "Paid") {
          clearInterval(pollingRef.current);
          setStatusMessage("✅ Thanh toán thành công!");
        }
      }, 5000);

    } catch (error) {
      console.error(error);
      setStatusMessage("❌ Không thể tạo mã thanh toán, vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  // Auto chạy khi vào trang
  useEffect(() => {
    if (orderId) {
      createPaymentAndPolling();
    }

    // Cleanup interval khi rời trang
    return () => {
      if (pollingRef.current) {
        clearInterval(pollingRef.current);
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

      {/* Loading */}
      {loading && <p>⏳ Đang tạo mã thanh toán...</p>}

      {/* QR Payment */}
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

      {/* Status */}
      {statusMessage && (
        <p style={{ marginTop: 20, fontWeight: "bold" }}>
          {statusMessage}
        </p>
      )}
    </div>
  );
};

export default PaymentPage;
