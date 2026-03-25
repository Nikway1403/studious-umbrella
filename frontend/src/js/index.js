import { getAccessToken } from "./auth-store.js";
import { CONFIG } from "./config.js";

const mainBtn = document.getElementById("mainActionBtn");

function init() {
    const token = getAccessToken();

    if (token) {
        mainBtn.textContent = "Личный кабинет";
        mainBtn.href = CONFIG.ROUTES.PROFILE;
    } else {
        mainBtn.textContent = "Войти";
        mainBtn.href = CONFIG.ROUTES.LOGIN;
    }
}

document.addEventListener("DOMContentLoaded", init);