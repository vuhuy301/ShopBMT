const BASE_URL = process.env.REACT_APP_API_URL;

export const checkStock = async (cartItems) => {
    try {
        const res = await fetch(`${BASE_URL}/cart/check-stock`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(cartItems)
        });

        const data = await res.json();

        if (!res.ok) {
            // nếu thiếu hàng
            throw data;
        }

        return data; // đủ hàng
    } catch (err) {
        console.error("❌ CHECK STOCK ERROR:", err);
        throw err;
    }
};