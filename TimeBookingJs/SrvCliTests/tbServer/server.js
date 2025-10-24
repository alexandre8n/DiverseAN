import dotenv from "dotenv";
import express from "express";
import cors from "cors";
import jwt from "jsonwebtoken";
import bcrypt from "bcrypt";

// my modules
// import utl1 from "./utl1.js";
import TbDataManager from "./tbDataManager.js";

const tbDataManager = new TbDataManager();

dotenv.config();

const app = express();
app.use(cors({ origin: "http://localhost:5173", credentials: true }));
//app.use(cors({ origin: "http://127.0.0.1:5501", credentials: true }));
//app.use(cors()); // для тестов с любого источника
app.use(express.json());

const PORT = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET;
const JWT_REFRESH_SECRET = process.env.JWT_REFRESH_SECRET;

// ⚠️ В проде храните refresh-токены в БД (или вообще не храните и делайте ротацию по jti).
const refreshStore = new Set();

/** Вспомогательные функции */
function signAccessToken(payload) {
  // Короткая жизнь access-токена — хорошая практика (10–30 минут)
  return jwt.sign(payload, JWT_SECRET, { expiresIn: "15m" });
}
function signRefreshToken(payload) {
  // Refresh живёт дольше (дни/недели). На проде добавляйте jti и ревокацию.
  return jwt.sign(payload, JWT_REFRESH_SECRET, { expiresIn: "7d" });
}

// Middleware для защиты маршрутов
function authenticate(req, res, next) {
  const auth = req.headers.authorization || "";
  const [, token] = auth.split(" "); // "Bearer <token>"

  if (!token) return res.status(401).json({ error: "No token provided" });

  jwt.verify(token, JWT_SECRET, (err, decoded) => {
    if (err) return res.status(401).json({ error: "Invalid or expired token" });
    req.user = decoded; // { id, username, role, iat, exp }
    next();
  });
}

/** Пример защищённого маршрута с проверкой роли */
function authorize(roles = []) {
  return (req, res, next) => {
    if (!roles.length || roles.includes(req.user.role)) return next();
    return res.status(403).json({ error: "Forbidden" });
  };
}

// --- Area of test routes ---

app.get("/", (req, res) => {
  res.json({
    message: "Timebooking Experiments server",
    port: PORT,
    jwtSecret: JWT_SECRET ? "✅ loaded" : "❌ not found",
    jwtRefreshSecret: JWT_REFRESH_SECRET ? "✅ loaded" : "❌ not found",
  });
});

/** Логин: проверяем пользователя, выдаём пары токенов */
app.post("/api/login", async (req, res) => {
  const { username, password } = req.body || {};
  const user = tbDataManager.getUserByName(username);
  if (!user) return res.status(401).json({ error: "Invalid credentials" });

  const ok = await bcrypt.compare(password, user.passwordHash);
  if (!ok) return res.status(401).json({ error: "Invalid credentials" });

  const payload = { id: user.id, username: user.username, role: user.role };
  const accessToken = signAccessToken(payload);
  const refreshToken = signRefreshToken({ id: user.id });

  refreshStore.add(refreshToken);

  res.json({ accessToken, refreshToken });
});

app.post("/api/tbAddProject", authenticate, async (req, res) => {
  let user = req.user;
  if (user.role === "admin") {
    user = null; // admin can add project for all users
  }
  const { projectName } = req.body || {};
  const project = tbDataManager.addProject(projectName, user);
  if (!project)
    return res
      .status(400)
      .json({ error: "Failed to add project, already exists" });
  res.json({ message: "Project added successfully", project });
});

app.post("/api/tbAddRec", authenticate, async (req, res) => {
  const { tbRec } = req.body || {};
  let user = req.user;

  if (user.role === "admin") {
    user = null; // admin can add project for all users
  }
  const tbRecAdded = tbDataManager.addTbRec(tbRec, user);
  if (!tbRecAdded)
    return res.status(400).json({ error: "Failed to add record" });
  res.json({ message: "Record added successfully", tbRec: tbRecAdded });
});

app.post("/api/tbUpdRec", authenticate, async (req, res) => {
  const { tbRec } = req.body || {};
  let user = req.user;
  if (user.role === "admin") {
    user = null; // admin can add project for all users
  }
  const tbRecUpdated = tbDataManager.updateTbRec(tbRec, user);
  if (!tbRecUpdated)
    return res.status(400).json({ error: "Failed to update record" });
  res.json({ message: "Record updated successfully", tbRec: tbRecUpdated });
});

// delete tb record...
app.post("/api/tbDelRec", authenticate, async (req, res) => {
  const { tbRec } = req.body || {};
  let user = req.user;
  if (user.role === "admin") {
    user = null; // admin can add project for all users
  }
  const tbRecDeleted = tbDataManager.deleteTbRec(tbRec, user);
  if (!tbRecDeleted)
    return res.status(400).json({ error: "Failed to delete record" });
  res.json({ message: "Record deleted successfully", tbRec: tbRecDeleted });
});

/** Обновление access-токена по refresh-токену */
app.post("/api/refresh", (req, res) => {
  const { refreshToken } = req.body || {};
  if (!refreshToken) return res.status(400).json({ error: "No refresh token" });
  if (!refreshStore.has(refreshToken)) {
    return res.status(401).json({ error: "Refresh token revoked or unknown" });
  }

  jwt.verify(refreshToken, JWT_REFRESH_SECRET, (err, decoded) => {
    if (err)
      return res
        .status(401)
        .json({ error: "Invalid or expired refresh token" });

    // Получаем пользователя (минимум id в payload refresh)
    const user = tbDataManager.getUserById(decoded.id);
    if (!user) return res.status(401).json({ error: "User no longer exists" });

    const payload = { id: user.id, username: user.username, role: user.role };
    const newAccessToken = signAccessToken(payload);

    res.json({ accessToken: newAccessToken });
  });
});

/** Логаут: ревокация refresh-токена */
app.post("/api/logout", (req, res) => {
  const { refreshToken } = req.body || {};
  if (refreshToken) refreshStore.delete(refreshToken);
  res.json({ success: true });
});

app.get("/api/tbAdmin_users", authenticate, (req, res) => {
  const user = req.user;
  if (user.role !== "admin") {
    return res
      .status(400)
      .json({ error: "Authorization failed: only for admin user allowed" });
  }
  const users = tbDataManager.getUsers();
  res.json({
    message: "List of users",
    user: req.user,
    users: users,
  });
});

app.get("/api/tbrecs", authenticate, (req, res) => {
  const { start, end } = req.query;
  if (!start || !end) {
    return res.status(400).json({ error: "Missing start or end date" });
  }
  const tbRecs = tbDataManager.getTbRecords(start, end, req.user);
  res.json({
    message: "Hello from /api/tbrecs",
    user: req.user,
    start: start,
    end: end,
    tbRecords: tbRecs,
  });
});

app.get("/api/tbRecById", authenticate, (req, res) => {
  const { id } = req.query;
  if (!id) {
    return res.status(400).json({ error: "Missing record ID" });
  }
  const user = req.user;
  let tbRec = null;
  if (user.role === "admin") {
    tbRec = tbDataManager.getTbRecordById(id, null);
  } else {
    tbRec = tbDataManager.getTbRecordById(id, user);
  }
  if (!tbRec) {
    return res
      .status(404)
      .json({ error: "Record not found or you do not have access to it" });
  }
  res.json({
    message: "Result from /api/tbRecById",
    user: req.user,
    tbRecord: tbRec,
  });
});
//
app.get("/api/tbGetProjects", authenticate, (req, res) => {
  const user = req.user;
  const projects = tbDataManager.getUserProjects(user);
  res.json({
    message: "List of projects",
    user: req.user,
    projects: projects,
  });
});

app.listen(PORT, () => {
  console.log(`Auth server running on http://localhost:${PORT}`);
});
// todo: add user,
// todo: update user,
