const BASE_URL = process.env.REACT_APP_API_URL || "https://localhost:7002";

export async function fetchWithToken(endpoint, options = {}, authRequired = true) {
  const token = localStorage.getItem("accessToken");
  const exp = localStorage.getItem("accessTokenExp");

  if (authRequired && (!token || !exp || Date.now() > exp)) {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("accessTokenExp");
    window.location.href = "/login";
    return;
  }

  let headers = {
    ...(options.headers || {}),
  };

  // ❗ Nếu không phải FormData thì set JSON
  if (!(options.body instanceof FormData)) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (authRequired && res.status === 401) {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("accessTokenExp");
    window.location.href = "/login";
    return;
  }

  return res;
}
