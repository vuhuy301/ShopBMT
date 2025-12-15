import Cookies from "js-cookie";

const CART_KEY = "cart";

/**
 * =========================
 * GET CART
 * =========================
 */
export const getCart = () => {
  const cart = Cookies.get(CART_KEY);
  return cart ? JSON.parse(cart) : [];
};

/**
 * =========================
 * SAVE CART
 * =========================
 */
export const saveCart = (cartItems) => {
  Cookies.set(CART_KEY, JSON.stringify(cartItems), {
    expires: 7, // lưu 7 ngày
  });
};

/**
 * =========================
 * ADD TO CART
 * =========================
 * item structure:
 * {
 *   productId,
 *   productName,
 *   price,
 *   image,
 *   colorVariantId,
 *   color,
 *   sizeVariantId,
 *   size,
 *   quantity
 * }
 */
export const addToCart = (item) => {
  const cart = getCart();

  const existingIndex = cart.findIndex(
    (i) =>
      i.productId === item.productId &&
      i.colorVariantId === item.colorVariantId &&
      i.sizeVariantId === item.sizeVariantId
  );

  if (existingIndex !== -1) {
    // Nếu đã tồn tại → cộng số lượng
    cart[existingIndex].quantity += item.quantity;
  } else {
    cart.push(item);
  }

  saveCart(cart);
};

/**
 * =========================
 * REMOVE FROM CART
 * =========================
 */
export const removeFromCart = (item) => {
  const cart = getCart();

  const newCart = cart.filter(
    (i) =>
      !(
        i.productId === item.productId &&
        i.colorVariantId === item.colorVariantId &&
        i.sizeVariantId === item.sizeVariantId
      )
  );

  saveCart(newCart);
};

/**
 * =========================
 * UPDATE QUANTITY
 * delta: +1 | -1
 * =========================
 */
export const updateQuantity = (item, delta) => {
  const cart = getCart();

  const index = cart.findIndex(
    (i) =>
      i.productId === item.productId &&
      i.colorVariantId === item.colorVariantId &&
      i.sizeVariantId === item.sizeVariantId
  );

  if (index !== -1) {
    cart[index].quantity = Math.max(1, cart[index].quantity + delta);
    saveCart(cart);
  }
};

/**
 * =========================
 * CLEAR CART
 * =========================
 */
export const clearCart = () => {
  Cookies.remove(CART_KEY);
};
