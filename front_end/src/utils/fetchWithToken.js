const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

export async function fetchWithToken(endpoint, options = {}) {
  const token = localStorage.getItem("accessToken");
  const exp = localStorage.getItem("accessTokenExp");

  if (!token || !exp || Date.now() > exp) {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("accessTokenExp");
    window.location.href = "/login"; // redirect nếu token hết hạn
    return;
  }

  const res = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      ...(options.headers || {}),
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });

  if (res.status === 401) {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("accessTokenExp");
    window.location.href = "/login";
  }

  return res;
}
