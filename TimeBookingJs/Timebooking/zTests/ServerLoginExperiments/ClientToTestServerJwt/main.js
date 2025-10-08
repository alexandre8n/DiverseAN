// main.js
import AuthClient from "./authClient.js";

// Инициализация
const api = new AuthClient({ baseURL: "http://localhost:3000" });

// Логин
document.querySelector("#loginBtn").addEventListener("click", async () => {
  try {
    await api.login("user1", "xxx");
    console.log("Логин успешен");
    window.appendLog("Логин успешен");
  } catch (e) {
    console.error("Login error:", e.message);
  }
});

// Запрос к защищённому маршруту (авторизация добавится сама)
document.querySelector("#profileBtn").addEventListener("click", async () => {
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
