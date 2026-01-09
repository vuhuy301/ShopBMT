import { fetchWithToken } from "../../utils/fetchWithToken";

export async function getUsers({ roleId, pageNumber = 1, pageSize = 6, email }) {
  try {
    const params = new URLSearchParams();
    if (roleId) params.append("RoleId", roleId);
    if (pageNumber) params.append("PageNumber", pageNumber);
    if (pageSize) params.append("PageSize", pageSize);
    if (email) params.append("Email", email); // <- tìm kiếm email

    const url = `/admin/AdminUsers/users?${params.toString()}`;
    console.log("API request:", url);

    const res = await fetchWithToken(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    });

    if (!res.ok) {
      throw new Error(`Request failed: ${res.status}`);
    }

    return await res.json();
  } catch (error) {
    console.error("Error getUsers:", error);
    throw error;
  }
}

export async function createEmployee({ fullName, email, password, role }) {
  const res = await fetchWithToken(`/admin/AdminUsers/create-employee`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ fullName, email, password, role }),
  });

  if (!res.ok) {
    const errorData = await res.json();
    throw new Error(errorData?.message || "Tạo nhân viên thất bại");
  }

  return await res.json();
}

export async function toggleUserActive(userId, active) {
  const res = await fetchWithToken(
    `/admin/AdminUsers/users/${userId}/toggle-active?active=${active}`,
    { method: "PUT" }
  );

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Toggle user active failed");
  }

  return await res.json();
}