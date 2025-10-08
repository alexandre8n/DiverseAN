// authClient.js
// Лёгкий клиент для JWT-авторизации: логин, авто-рефреш, обёртка над fetch.

/**
 * @typedef {Object} AuthClientOptions
 * @property {string} baseURL               - базовый URL бэкенда (например, "http://localhost:3000")
 * @property {Storage} [storage]            - где хранить токены (по умолчанию localStorage)
 * @property {string} [accessKey]           - ключ для accessToken в storage
 * @property {string} [refreshKey]          - ключ для refreshToken в storage
 * @property {number} [timeoutMs]           - таймаут для fetch (мс), опционально
 */

export default class AuthClient {
  /**
   * @param {AuthClientOptions} opts
   */
  constructor(opts) {
    if (!opts?.baseURL) throw new Error("baseURL is required");
    this.baseURL = opts.baseURL.replace(/\/+$/, "");
    this.storage = opts.storage || window.localStorage;
    this.accessKey = opts.accessKey || "accessToken";
    this.refreshKey = opts.refreshKey || "refreshToken";
    this.timeoutMs = typeof opts.timeoutMs === "number" ? opts.timeoutMs : 0;

    /** @type {Promise<string>|null} */
    this._refreshInFlight = null;
  }

  // ====== Токены ======

  _getAccessToken() {
    return this.storage.getItem(this.accessKey);
  }
  _getRefreshToken() {
    return this.storage.getItem(this.refreshKey);
  }
  _setTokens(accessToken, refreshToken) {
    if (accessToken) this.storage.setItem(this.accessKey, accessToken);
    if (refreshToken) this.storage.setItem(this.refreshKey, refreshToken);
  }
  _clearTokens() {
    this.storage.removeItem(this.accessKey);
    this.storage.removeItem(this.refreshKey);
  }

  // ====== Публичные методы ======

  /**
   * Логин: получает access/refresh токены и сохраняет.
   * Ожидает, что сервер отвечает { accessToken, refreshToken }.
   */
  async login(username, password) {
    const res = await this._rawFetch("/api/login", {
      method: "POST",
      credentials: "include", // иначе cookie не уйдут, but for Jwt - not needed...
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username, password }),
    });

    const data = await this._readJson(res);
    if (!res.ok) throw new Error(data?.error || `Login failed (${res.status})`);

    if (!data.accessToken || !data.refreshToken) {
      throw new Error("Server did not return tokens");
    }
    this._setTokens(data.accessToken, data.refreshToken);
    return data;
  }

  /**
   * Логаут: очищает токены локально и опционально сообщает серверу.
   */
  async logout() {
    const refreshToken = this._getRefreshToken();
    this._clearTokens();
    try {
      if (refreshToken) {
        await this._rawFetch("/api/logout", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken }),
        });
      }
    } catch {
      // Игнорируем сетевые ошибки при логауте
    }
  }

  /**
   * Универсальный запрос с автоматическим проставлением Authorization и ретраем после refresh при 401.
   * @param {string} path - путь относительно baseURL
   * @param {RequestInit & { skipAuth?: boolean }} [options]
   */
  async request(path, options = {}) {
    const url = this._abs(path);
    const opts = { ...options };

    // Добавим Authorization, если не skipAuth
    if (!opts.skipAuth) {
      const token = this._getAccessToken();
      if (token) {
        opts.headers = {
          ...(opts.headers || {}),
          Authorization: `Bearer ${token}`,
        };
      }
    }

    let res = await this._rawFetch(url, opts);

    // Если 401 и у нас есть refreshToken — пробуем обновить и повторить ОДИН раз
    if (res.status === 401 && !options.skipAuth) {
      const refreshed = await this._ensureAccessTokenFresh();
      if (refreshed) {
        const retryOpts = { ...options };
        retryOpts.headers = {
          ...(retryOpts.headers || {}),
          Authorization: `Bearer ${refreshed}`,
        };
        res = await this._rawFetch(url, retryOpts);
      }
    }

    return res;
  }

  /** Удобные шорткаты */
  async get(path, options = {}) {
    const res = await this.request(path, { ...options, method: "GET" });
    return this._readJson(res, true);
  }
  async post(path, body, options = {}) {
    const res = await this.request(path, {
      ...options,
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
      },
      body: JSON.stringify(body),
    });
    return this._readJson(res, true);
  }

  // ====== Обновление токена ======

  /**
   * Обновляет accessToken по refreshToken. Мемоизирует запрос, чтобы не было гонок.
   * Возвращает новый accessToken или null.
   */
  async _ensureAccessTokenFresh() {
    const refreshToken = this._getRefreshToken();
    if (!refreshToken) return null;

    if (!this._refreshInFlight) {
      this._refreshInFlight = (async () => {
        try {
          const res = await this._rawFetch("/api/refresh", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ refreshToken }),
          });
          const data = await this._readJson(res);
          if (!res.ok || !data?.accessToken) {
            this._clearTokens();
            return null;
          }
          this._setTokens(data.accessToken, null);
          return data.accessToken;
        } catch {
          this._clearTokens();
          return null;
        } finally {
          // важный трюк: снимаем ссылку только после микротика, чтобы параллельные await дождались
          const p = this._refreshInFlight;
          setTimeout(() => {
            if (this._refreshInFlight === p) this._refreshInFlight = null;
          }, 0);
        }
      })();
    }
    return this._refreshInFlight;
  }

  // ====== Низкоуровневые утилиты ======

  _abs(path) {
    if (/^https?:\/\//i.test(path)) return path;
    return `${this.baseURL}${path.startsWith("/") ? "" : "/"}${path}`;
  }

  async _rawFetch(inputA, init) {
    const input = `${this.baseURL}${inputA}`;
    if (!this.timeoutMs) return fetch(input, init);
    // Таймаут (если задан)
    return Promise.race([
      fetch(input, init),
      new Promise((_, rej) =>
        setTimeout(() => rej(new Error("Request timeout")), this.timeoutMs)
      ),
    ]);
  }

  async _readJson(res, throwIfNotOk = false) {
    let data = null;
    try {
      data = await res.json();
    } catch {
      // no-op: может быть пустое тело
    }
    if (throwIfNotOk && !res.ok) {
      throw new Error(data?.error || `HTTP ${res.status}`);
    }
    return data;
  }
}
