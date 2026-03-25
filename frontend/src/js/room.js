import { requireAuth } from "./auth-guard.js";
import { clearTokens } from "./auth-store.js";
import { logoutRequest } from "./auth-api.js";
import { getCurrentUserId } from "./api.js";
import { CONFIG } from "./config.js";
import { SignalingClient } from "./signaling.js";
import { CallManager } from "./call-manager.js";

const localVideo = document.getElementById("localVideo");
const remoteVideo = document.getElementById("remoteVideo");

const roomIdInput = document.getElementById("roomId");
const createRoomBtn = document.getElementById("createRoomBtn");
const joinRoomBtn = document.getElementById("joinRoomBtn");
const roomInfo = document.getElementById("roomInfo");

const enableMicBtn = document.getElementById("enableMicBtn");
const enableCameraBtn = document.getElementById("enableCameraBtn");
const toggleMuteBtn = document.getElementById("toggleMuteBtn");
const toggleCameraBtn = document.getElementById("toggleCameraBtn");
const shareScreenBtn = document.getElementById("shareScreenBtn");
const hangupBtn = document.getElementById("hangupBtn");
const logoutBtn = document.getElementById("logoutBtn");

const sendMessageBtn = document.getElementById("sendMessageBtn");
const messageInput = document.getElementById("messageInput");
const messages = document.getElementById("messages");
const callStatus = document.getElementById("callStatus");

let signaling = null;
let callManager = null;

let currentRoomId = null;
let alreadyJoined = false;
let joinedAtLeastOnce = false;

function setStatus(text) {
    callStatus.textContent = `Статус: ${text}`;
}

function setRoomInfo(text) {
    roomInfo.textContent = text;
}

function appendMessage(text, own = false) {
    const div = document.createElement("div");
    div.className = own ? "message message-own" : "message";
    div.textContent = text;
    messages.appendChild(div);
    messages.scrollTop = messages.scrollHeight;
}

function generateRoomId() {
    if (window.crypto?.randomUUID) {
        return window.crypto.randomUUID().slice(0, 8);
    }

    return Math.random().toString(36).slice(2, 10);
}

async function sendRenegotiationOffer() {
    if (!alreadyJoined) return;

    try {
        const offer = await callManager.createOffer();
        await signaling.sendOffer(offer);
        setStatus("Обновляем соединение...");
    } catch (error) {
        setStatus(error.message || "Ошибка renegotiation");
    }
}

async function joinRoom(roomId) {
    if (!roomId) {
        throw new Error("Укажи код комнаты");
    }

    if (alreadyJoined && currentRoomId === roomId) {
        setStatus(`Уже в комнате ${roomId}`);
        return;
    }

    currentRoomId = roomId;
    roomIdInput.value = roomId;

    await signaling.joinRoom(roomId);

    alreadyJoined = true;
    joinedAtLeastOnce = true;
    setRoomInfo(`Код комнаты: ${roomId}`);
    setStatus(`Подключён к комнате ${roomId}`);
}

async function initPage() {
    const ok = await requireAuth();
    if (!ok) return;

    signaling = new SignalingClient();
    callManager = new CallManager(
        localVideo,
        remoteVideo,
        setStatus,
        (message) => appendMessage(message, false)
    );

    await signaling.connect();

    callManager.createPeerConnection(
        async (candidate) => {
            await signaling.sendIceCandidate(candidate);
        },
        async () => {
            if (joinedAtLeastOnce) {
                await sendRenegotiationOffer();
            }
        }
    );

    signaling.onStartCall(async (isInitiator) => {
        try {
            if (isInitiator) {
                const offer = await callManager.createOffer();
                await signaling.sendOffer(offer);
                setStatus("Создан offer, ждём answer");
            } else {
                setStatus("Ожидание offer");
            }
        } catch (error) {
            setStatus(error.message || "Ошибка старта звонка");
        }
    });

    signaling.onReceiveOffer(async (offer) => {
        try {
            const answer = await callManager.handleOffer(offer);
            await signaling.sendAnswer(answer);
            setStatus("Offer получен, answer отправлен");
        } catch (error) {
            setStatus(error.message || "Ошибка обработки offer");
        }
    });

    signaling.onReceiveAnswer(async (answer) => {
        try {
            await callManager.handleAnswer(answer);
            setStatus("Answer получен");
        } catch (error) {
            setStatus(error.message || "Ошибка обработки answer");
        }
    });

    signaling.onReceiveIceCandidate(async (candidate) => {
        try {
            await callManager.addIceCandidate(candidate);
        } catch (error) {
            setStatus(error.message || "Ошибка ICE");
        }
    });

    signaling.onUserDisconnected(() => {
        callManager.clearRemoteMedia();
        setStatus("Собеседник отключился");
    });

    setRoomInfo("Комната ещё не создана");
    setStatus("Готово");
}

enableMicBtn?.addEventListener("click", async () => {
    try {
        await callManager.enableMicrophone();

        if (alreadyJoined) {
            await sendRenegotiationOffer();
        }
    } catch (error) {
        setStatus(error.message || "Ошибка включения микрофона");
    }
});

enableCameraBtn?.addEventListener("click", async () => {
    try {
        await callManager.enableCamera();

        if (alreadyJoined) {
            await sendRenegotiationOffer();
        }
    } catch (error) {
        setStatus(error.message || "Ошибка включения камеры");
    }
});

createRoomBtn?.addEventListener("click", async () => {
    try {
        const roomId = generateRoomId();
        await joinRoom(roomId);
        setStatus(`Комната ${roomId} создана. Передай код собеседнику`);
    } catch (error) {
        setStatus(error.message || "Ошибка создания комнаты");
    }
});

joinRoomBtn?.addEventListener("click", async () => {
    try {
        const roomId = roomIdInput.value.trim();
        await joinRoom(roomId);
    } catch (error) {
        setStatus(error.message || "Ошибка входа в комнату");
    }
});

toggleMuteBtn?.addEventListener("click", () => {
    try {
        const isMuted = callManager.toggleMute();
        toggleMuteBtn.textContent = isMuted ? "Unmute" : "Mute";
    } catch (error) {
        setStatus(error.message || "Ошибка mute");
    }
});

toggleCameraBtn?.addEventListener("click", () => {
    try {
        const isCameraOff = callManager.toggleCamera();
        toggleCameraBtn.textContent = isCameraOff ? "Cam On" : "Cam Off";
    } catch (error) {
        setStatus(error.message || "Ошибка камеры");
    }
});

shareScreenBtn?.addEventListener("click", async () => {
    try {
        await callManager.startScreenShare();
        if (alreadyJoined) {
            await sendRenegotiationOffer();
        }
    } catch (error) {
        setStatus(error.message || "Ошибка демонстрации экрана");
    }
});

hangupBtn?.addEventListener("click", () => {
    callManager.hangup();
    alreadyJoined = false;
    currentRoomId = null;
    toggleMuteBtn.textContent = "Mute";
    toggleCameraBtn.textContent = "Cam Off";
    setRoomInfo("Комната ещё не создана");
});

sendMessageBtn?.addEventListener("click", () => {
    try {
        const text = messageInput.value.trim();
        if (!text) return;

        callManager.sendChatMessage(text);
        appendMessage(text, true);
        messageInput.value = "";
    } catch (error) {
        setStatus(error.message || "Ошибка отправки сообщения");
    }
});

logoutBtn?.addEventListener("click", async () => {
    try {
        const userId = getCurrentUserId();
        if (userId) {
            await logoutRequest(userId);
        }
    } catch {
        // ignore
    } finally {
        clearTokens();
        window.location.href = CONFIG.ROUTES.LOGIN;
    }
});

document.addEventListener("DOMContentLoaded", initPage);