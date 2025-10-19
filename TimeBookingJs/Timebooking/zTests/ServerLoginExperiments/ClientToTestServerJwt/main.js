// main.js
import AuthClient from "./authClient.js";

const baseURL = "http://localhost:3000";
// Инициализация
const api = new AuthClient({ baseURL: baseURL });

// Логин
document.querySelector("#loginBtn").addEventListener("click", async () => {
  try {
    const user = document.querySelector("#username").value;
    const pass = document.querySelector("#password").value;
    await api.login(user, pass);
    console.log("Логин успешен");
    window.appendLog("Логин успешен");
  } catch (e) {
    console.error("Login error:", e.message);
    window.appendLog("Login error:" + e.message);
  }
});

// Запрос к защищённому маршруту (авторизация добавится сама)
document.querySelector("#usersListBtn").addEventListener("click", async () => {
  try {
    const data = await api.get("/api/profile");
    console.log("Профиль:", data);
  } catch (e) {
    console.error("Profile error:", e.message);
  }
});

// POST к защищённому маршруту
document.querySelector("#updateBtn").addEventListener("click", async () => {
  try {
    const resp = await api.post("/api/something", { foo: 42 });
    console.log("Ответ:", resp);
  } catch (e) {
    console.error("Update error:", e.message);
  }
});

// Логаут
document.querySelector("#logoutBtn").addEventListener("click", async () => {
  await api.logout();
  console.log("Вышли из системы");
});

document.querySelector("#tbRecsBtn").addEventListener("click", async () => {
  const startDate = "2025-10-01";
  const endDate = "2025-10-15";
  const url = `/api/tbrecs?start=${encodeURIComponent(
    startDate
  )}&end=${encodeURIComponent(endDate)}`;
  const resp = await api.get(url);
  console.log("Ответ:", resp);
  window.appendLog("api/tbrecs ответ: " + JSON.stringify(resp));
});

// Example usage
//getTimeBookings('2025-10-01', '2025-10-15');
