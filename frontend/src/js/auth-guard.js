import { isAuthenticated, getRefreshToken, setTokens, clearTokens } from "./auth-store.js";
import { refreshRequest } from "./auth-api.js";
import { CONFIG } from "./config.js";

export async function requireAuth() {
    if (isAuthenticated()) {
        return true;
    }

    const refreshToken = getRefreshToken();
    if (!refreshToken) {
        window.location.href = CONFIG.ROUTES.LOGIN;
        return false;
    }

    try {
        const result = await refreshRequest(refreshToken);
        setTokens(result.accessToken, result.refreshToken);
        return true;
    } catch {
        clearTokens();
        window.location.href = CONFIG.ROUTES.LOGIN;
        return false;
    }
}