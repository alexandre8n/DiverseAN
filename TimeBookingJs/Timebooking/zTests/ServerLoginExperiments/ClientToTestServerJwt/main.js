// main.js
import AuthClient from "./authClient.js";
import utl1 from "./utl1.js";

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
    const data = await api.get("/api/tbAdmin_users");
    console.log("Пользователи:", data);
    window.appendLog("api/tbAdmin_users ответ: " + JSON.stringify(data));
  } catch (e) {
    window.appendLog("api/tbAdmin_users Error: " + JSON.stringify(e.message));
    console.error("Get users request error:", e.message);
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
  const dateRange = 15; // дней
  const endDate = new Date();
  const startDate = utl1.getRandomDate(today, -dateRange, 0);
  const url = `/api/tbrecs?start=${encodeURIComponent(
    startDate
  )}&end=${encodeURIComponent(endDate)}`;
  const resp = await api.get(url);
  console.log("Ответ:", resp);
  window.appendLog("api/tbrecs ответ: " + JSON.stringify(resp));
});

// Example usage
//getTimeBookings('2025-10-01', '2025-10-15');
// "tbRecByIdBtn"
document.querySelector("#tbRecByIdBtn").addEventListener("click", async () => {
  const recId = 1; // Пример ID записи
  const url = `/api/tbRecById?id=${encodeURIComponent(recId)}`;
  const resp = await api.get(url);
  console.log("Ответ:", resp);
  window.appendLog("api/tbRecById ответ: " + JSON.stringify(resp));
});
