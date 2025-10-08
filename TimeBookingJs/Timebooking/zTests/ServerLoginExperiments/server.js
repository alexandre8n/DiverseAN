require("dotenv").config();
const express = require("express");
const cors = require("cors");
const jwt = require("jsonwebtoken");
const bcrypt = require("bcrypt");

const app = express();
app.use(
  cors({
    origin: "http://127.0.0.1:5500",
    credentials: true,
  })
);
app.use(express.json());

const PORT = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET;
const JWT_REFRESH_SECRET = process.env.JWT_REFRESH_SECRET;

app.get("/", (req, res) => {
  res.json({
    message: "Timebooking Experiments server",
    port: PORT,
    jwtSecret: JWT_SECRET ? "✅ loaded" : "❌ not found",
    jwtRefreshSecret: JWT_REFRESH_SECRET ? "✅ loaded" : "❌ not found",
  });
});

// ⚠️ Демонстрационная «БД»
const users = [
  // пароль "xxx" захэширован (пример: bcrypt.hashSync("xxx", 10))
  {
    id: 1,
    username: "user1",
    passwordHash: bcrypt.hashSync("xxx", 10),
    role: "user",
  },
];

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

/** Логин: проверяем пользователя, выдаём пары токенов */
app.post("/api/login", async (req, res) => {
  const { username, password } = req.body || {};
  const user = users.find((u) => u.username === username);
  if (!user) return res.status(401).json({ error: "Invalid credentials" });

  const ok = await bcrypt.compare(password, user.passwordHash);
  if (!ok) return res.status(401).json({ error: "Invalid credentials" });

  const payload = { id: user.id, username: user.username, role: user.role };
  const accessToken = signAccessToken(payload);
  const refreshToken = signRefreshToken({ id: user.id });

  refreshStore.add(refreshToken);

  res.json({ accessToken, refreshToken });
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
    const user = users.find((u) => u.id === decoded.id);
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

/** Пример защищённого маршрута */
app.get("/api/profile", authenticate, (req, res) => {
  // req.user пришёл из access-токена
  res.json({
    message: "Protected resource",
    user: req.user,
  });
});

/** Пример защищённого маршрута с проверкой роли */
function authorize(roles = []) {
  return (req, res, next) => {
    if (!roles.length || roles.includes(req.user.role)) return next();
    return res.status(403).json({ error: "Forbidden" });
  };
}
app.get("/api/admin", authenticate, authorize(["admin"]), (req, res) => {
  res.json({ message: "Hello, admin!" });
});

app.get("/tb", (req, res) => {
  res.json({ message: "Hello, admin!" });
});

app.post("/tb", (req, res) => {
  const v1 = req.body;
  res.json({ html: "<h1>Hi Sasha</h1><button>ABC</button>" });
});

app.listen(PORT, () => {
  console.log(`Auth server running on http://localhost:${PORT}`);
});
