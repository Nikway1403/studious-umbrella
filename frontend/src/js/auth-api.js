import { CONFIG } from "./config.js";

async function handleJsonResponse(response, defaultMessage) {
    if (!response.ok) {
        let message = defaultMessage;

        try {
            const errorBody = await response.json();
            message = errorBody.message || errorBody.title || message;
        } catch {
            // ignore
        }

        throw new Error(message);
    }

    return response.json();
}

export async function loginRequest(nickname, password) {
    const response = await fetch(`${CONFIG.API_BASE}/auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ nickname, password })
    });

    return handleJsonResponse(response, "Ошибка входа");
}

export async function registerRequest(nickname, password) {
    const response = await fetch(`${CONFIG.API_BASE}/auth/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ nickname, password })
    });

    return handleJsonResponse(response, "Ошибка регистрации");
}

export async function refreshRequest(refreshToken) {
    const response = await fetch(`${CONFIG.API_BASE}/auth/refresh`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ refreshToken })
    });

    return handleJsonResponse(response, "Не удалось обновить токен");
}

export async function logoutRequest(userId) {
    const response = await fetch(`${CONFIG.API_BASE}/auth/logout`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ userId })
    });

    if (!response.ok) {
        throw new Error("Ошибка выхода");
    }
}