import { fetchWithToken } from "../utils/fetchWithToken";

export async function getRoles() {
  try {
    const res = await fetchWithToken('/Roles');

    if (!res.ok) {
      throw new Error(`Request failed: ${res.status}`);
    }

    const data = await res.json();
    return data;
  } catch (error) {
    console.error("Error getRoles:", error);
    throw error;
  }
}