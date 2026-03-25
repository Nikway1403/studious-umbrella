import { getAccessToken, getRefreshToken, clearTokens } from "./auth-store.js";
import { logoutRequest } from "./auth-api.js";
import { CONFIG } from "./config.js";
import { getCurrentUserId, validateToken } from "./api.js";

const accessTokenEl = document.getElementById("accessToken");
const refreshTokenEl = document.getElementById("refreshToken");
const logoutBtn = document.getElementById("logoutBtn");

async function init() {
    const access = getAccessToken();
    const refresh = getRefreshToken();

    if (!access) {
        window.location.href = CONFIG.ROUTES.LOGIN;
        return;
    }

    const isValid = await validateToken(access);

    if (!isValid) {
        clearTokens();
        window.location.href = CONFIG.ROUTES.LOGIN;
        return;
    }

    accessTokenEl.textContent = access;
    refreshTokenEl.textContent = refresh;
}

logoutBtn?.addEventListener("click", async () => {
    try {
        const userId = getCurrentUserId();
        if (userId) {
            await logoutRequest(userId);
        }
    } catch {}

    clearTokens();
    window.location.href = CONFIG.ROUTES.LOGIN;
});

document.addEventListener("DOMContentLoaded", init);