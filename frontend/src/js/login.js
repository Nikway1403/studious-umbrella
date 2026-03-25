import { loginRequest, registerRequest } from "./auth-api.js";
import { setTokens } from "./auth-store.js";
import { CONFIG } from "./config.js";

const loginTab = document.getElementById("loginTab");
const registerTab = document.getElementById("registerTab");
const authForm = document.getElementById("authForm");
const nicknameInput = document.getElementById("nickname");
const passwordInput = document.getElementById("password");
const statusText = document.getElementById("status");

let mode = "login";

function updateMode() {
    loginTab.classList.toggle("btn-primary", mode === "login");
    registerTab.classList.toggle("btn-primary", mode === "register");
}

loginTab?.addEventListener("click", () => {
    mode = "login";
    updateMode();
});

registerTab?.addEventListener("click", () => {
    mode = "register";
    updateMode();
});

authForm?.addEventListener("submit", async (event) => {
    event.preventDefault();

    const nickname = nicknameInput.value.trim();
    const password = passwordInput.value.trim();

    if (!nickname || !password) {
        statusText.textContent = "Заполни nickname и пароль";
        return;
    }

    try {
        statusText.textContent = "Загрузка...";

        const result = mode === "login"
            ? await loginRequest(nickname, password)
            : await registerRequest(nickname, password);

        setTokens(result.accessToken, result.refreshToken);
        window.location.href = CONFIG.ROUTES.ROOM;
    } catch (error) {
        statusText.textContent = error.message || "Ошибка";
    }
});

updateMode();