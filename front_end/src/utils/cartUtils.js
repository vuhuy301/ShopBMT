import Cookies from "js-cookie";

const CART_KEY = "cart";

export const getCart = () => {
  const cart = Cookies.get(CART_KEY);
  return cart ? JSON.parse(cart) : [];
};

export const saveCart = (cartItems) => {
  Cookies.set(CART_KEY, JSON.stringify(cartItems), { expires: 7 }); // 7 ngày
};

export const addToCart = (item) => {
  const cart = getCart();
  const existingIndex = cart.findIndex(
    (i) =>
      i.id === item.id &&
      i.color === item.color &&
      i.size === item.size
  );

  if (existingIndex !== -1) {
    // Nếu đã tồn tại → cộng số lượng
    cart[existingIndex].quantity += item.quantity;
  } else {
    cart.push(item);
  }

  saveCart(cart);
};

export const removeFromCart = (item) => {
  const cart = getCart();
  const newCart = cart.filter(
    (i) =>
      !(
        i.id === item.id &&
        i.color === item.color &&
        i.size === item.size
      )
  );
  saveCart(newCart);
};

export const updateQuantity = (item, delta) => {
  const cart = getCart();
  const index = cart.findIndex(
    (i) =>
      i.id === item.id &&
      i.color === item.color &&
      i.size === item.size
  );

  if (index !== -1) {
    cart[index].quantity = Math.max(1, cart[index].quantity + delta);
    saveCart(cart);
  }
};
