export const validateCheckout = (customer, cartItems) => {
  const errors = {};

  // Họ tên
  if (!customer.name || customer.name.trim().length < 2) {
    errors.name = "Vui lòng nhập họ tên hợp lệ";
  }

  // Số điện thoại VN
  const phoneRegex = /^(0[3|5|7|8|9])[0-9]{8}$/;
  if (!customer.phone) {
    errors.phone = "Vui lòng nhập số điện thoại";
  } else if (!phoneRegex.test(customer.phone)) {
    errors.phone = "Số điện thoại không hợp lệ";
  }

  // Email (không bắt buộc)
  if (customer.email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(customer.email)) {
      errors.email = "Email không hợp lệ";
    }
  }

  // Địa chỉ
  if (!customer.address || customer.address.trim().length < 5) {
    errors.address = "Vui lòng nhập địa chỉ giao hàng";
  }

  // Giỏ hàng
  if (!cartItems || cartItems.length === 0) {
    errors.cart = "Giỏ hàng đang trống";
  }

  return errors;
};
