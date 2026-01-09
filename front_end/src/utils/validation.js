// Không được để trống
export const isRequired = (value) => {
  if (value === null || value === undefined) return false;
  return value.toString().trim() !== "";
};

// Giá tiền (số > 0)
export const isValidPrice = (value) => {
  if (!value) return false;
  return !isNaN(value) && Number(value) > 0;
};

// Số lượng (>= 0)
export const isValidQuantity = (value) => {
  if (value === "") return false;
  return Number.isInteger(Number(value)) && Number(value) >= 0;
};

// Email
export const isValidEmail = (email) => {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email);
};

// Số điện thoại VN
export const isValidPhone = (phone) => {
  const regex = /^(0[3|5|7|8|9])[0-9]{8}$/;
  return regex.test(phone);
};

// Độ dài chuỗi
export const minLength = (value, length) => {
  return value && value.length >= length;
};
