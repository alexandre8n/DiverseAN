// Big debugger problem: not supported subfolders, when using live server
// Normal practice: I start server, I put all needed break points in client, and start debugging, all this worked before using live server and subfolders.
// I see at the moment only 2 option, please correct:
// - learn Vite and try to work with it
// - copy all shared files into woking folder with other files and debugger will work with it?
// cd tbCliTest
// npm create vite@latest
// run vite: npm run dev

// main.js
import AuthClient from "./authClient.js";
import utl1 from "../tbShared/utl1.js";
import { editObject } from "./editObj.js";

document.addEventListener("DOMContentLoaded", () => {
  console.log("main.js loaded");
  main();
});

function main() {
  console.log("main.js loaded");
  //debugger;
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
  document
    .querySelector("#usersListBtn")
    .addEventListener("click", async () => {
      try {
        const data = await api.get("/api/tbAdmin_users");
        console.log("Пользователи:", data);
        window.appendLog("api/tbAdmin_users ответ: " + JSON.stringify(data));
      } catch (e) {
        window.appendLog(
          "api/tbAdmin_users Error: " + JSON.stringify(e.message)
        );
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
    const resp = await api.logout();
    console.log("Вышли из системы");
    window.appendLog("log out is done" + JSON.stringify(resp));
  });

  document.querySelector("#tbRecsBtn").addEventListener("click", async () => {
    const dateRange = 15; // дней
    const endDate = utl1.dateToStdStr(new Date());
    const startDate = utl1.getRandomDate(endDate, -dateRange, 0);
    // the tb - data from the logged in user will be returned
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
  document
    .querySelector("#tbRecByIdBtn")
    .addEventListener("click", async () => {
      const obj = { id: 1 }; // Пример объекта
      const resultObj = await editObject(obj, "Edit Record ID");
      const recId = resultObj.id;
      const url = `/api/tbRecById?id=${encodeURIComponent(recId)}`;
      try {
        const resp = await api.get(url);
        console.log("record by id:", resp);
        window.appendLog("api/tbRecById ответ: " + JSON.stringify(resp));
      } catch (e) {
        window.appendLog("api/tbRecById Error: " + JSON.stringify(e.message));
      }
    });

  document.querySelector("#tbProjsBtn").addEventListener("click", async () => {
    const url = `/api/tbGetProjects`;
    try {
      const resp = await api.get(url);
      window.appendLog("api/tbGetProjects ответ: " + JSON.stringify(resp));
    } catch (e) {
      window.appendLog("api/tbGetProjects Error: " + JSON.stringify(e.message));
    }
  });
  document.querySelector("#tbProjsBtn").addEventListener("click", async () => {
    const url = `/api/tbGetProjects`;
    try {
      const resp = await api.get(url);
      window.appendLog("api/tbGetProjects ответ: " + JSON.stringify(resp));
    } catch (e) {
      window.appendLog("api/tbGetProjects Error: " + JSON.stringify(e.message));
    }
  });
  document
    .querySelector("#tbAddProjBtn")
    .addEventListener("click", async () => {
      const projObj = await editObject(
        { projectName: "" },
        "Enter project name:"
      );
      if (!projObj) return;

      const url = `/api/tbAddProject`;
      try {
        const resp = await api.post(url, projObj);
        window.appendLog("api/tbAddProject ответ: " + JSON.stringify(resp));
      } catch (e) {
        window.appendLog(
          "api/tbAddProject Error: " + JSON.stringify(e.message)
        );
      }
    });
  document
    .querySelector("#tbAddTbRecBtn")
    .addEventListener("click", async () => {
      const user = document.querySelector("#username").value;
      const tbRec = await editObject(
        {
          user: user,
          date: utl1.dateToStdStr(new Date()),
          project: "",
          task: "",
          effort: "0",
          comment: "",
        },
        "Specify timebooking details:"
      );
      if (!tbRec) return;

      const url = `/api/tbAddRec`;
      try {
        const resp = await api.post(url, { tbRec: tbRec });
        window.appendLog("api/tbAddRec ответ: " + JSON.stringify(resp));
      } catch (e) {
        window.appendLog(
          "api/tbAddProject Error: " + JSON.stringify(e.message)
        );
      }
    });
  //todo:      <button id="tbUpdRec">Post /api/tbUpdateRec</button>
  // todo: button id="tbDelRec">Post /api/tbDeleteRec</button>
  // <button id="tbAddUser">Post /api/tbAdmin_addUser</button>
  // <button id="tbDisableUser">Post /api/tbAdmin_changeUser</button>

  // End of main()
}
