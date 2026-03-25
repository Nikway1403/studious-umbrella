import { CONFIG } from "./config.js";

export function setTokens(accessToken, refreshToken) {
    sessionStorage.setItem(CONFIG.STORAGE_KEYS.ACCESS_TOKEN, accessToken);
    sessionStorage.setItem(CONFIG.STORAGE_KEYS.REFRESH_TOKEN, refreshToken);
}

export function getAccessToken() {
    return sessionStorage.getItem(CONFIG.STORAGE_KEYS.ACCESS_TOKEN);
}

export function getRefreshToken() {
    return sessionStorage.getItem(CONFIG.STORAGE_KEYS.REFRESH_TOKEN);
}

export function clearTokens() {
    sessionStorage.removeItem(CONFIG.STORAGE_KEYS.ACCESS_TOKEN);
    sessionStorage.removeItem(CONFIG.STORAGE_KEYS.REFRESH_TOKEN);
}

export function isAuthenticated() {
    return !!getAccessToken();
}