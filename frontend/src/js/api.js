import { getAccessToken, getRefreshToken, setTokens, clearTokens } from "./auth-store.js";
import { refreshRequest } from "./auth-api.js";
import { CONFIG } from "./config.js";

let refreshPromise = null;

function parseJwt(token) {
    try {
        const payload = token.split(".")[1];
        return JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
    } catch {
        return null;
    }
}

export function getCurrentUserId() {
    const token = getAccessToken();
    if (!token) return null;

    const payload = parseJwt(token);
    if (!payload) return null;

    return Number(payload.nameid || payload.sub || 0) || null;
}

async function refreshTokens() {
    if (!refreshPromise) {
        refreshPromise = (async () => {
            const refreshToken = getRefreshToken();
            if (!refreshToken) {
                throw new Error("Refresh token отсутствует");
            }

            const result = await refreshRequest(refreshToken);
            setTokens(result.accessToken, result.refreshToken);
            return result.accessToken;
        })().finally(() => {
            refreshPromise = null;
        });
    }

    return refreshPromise;
}

export async function authorizedFetch(path, options = {}) {
    let accessToken = getAccessToken();

    const headers = {
        ...(options.headers || {})
    };

    if (accessToken) {
        headers.Authorization = `Bearer ${accessToken}`;
    }

    let response = await fetch(`${CONFIG.API_BASE}${path}`, {
        ...options,
        headers
    });

    if (response.status !== 401) {
        return response;
    }

    try {
        accessToken = await refreshTokens();

        const retryHeaders = {
            ...(options.headers || {}),
            Authorization: `Bearer ${accessToken}`
        };

        return await fetch(`${CONFIG.API_BASE}${path}`, {
            ...options,
            headers: retryHeaders
        });
    } catch {
        clearTokens();
        window.location.href = CONFIG.ROUTES.LOGIN;
        throw new Error("Сессия истекла");
    }
}

export async function validateToken(accessToken) {
    const response = await fetch(`${CONFIG.API_BASE}/auth/validate`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ accessToken })
    });

    if (!response.ok) {
        return false;
    }

    const result = await response.json();
    return result.isValid === true;
}