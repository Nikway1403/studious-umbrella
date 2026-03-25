export const CONFIG = {
    API_BASE: "/api",
    VOICE_HUB_URL: "/voice",
    STORAGE_KEYS: {
        ACCESS_TOKEN: "voice_access_token",
        REFRESH_TOKEN: "voice_refresh_token"
    },
    ROUTES: {
        INDEX: "/index.html",
        LOGIN: "/login.html",
        ROOM: "/room.html",
        PROFILE: "/profile.html"
    },
    RTC: {
        iceServers: [
            { urls: "stun:stun.l.google.com:19302" }
        ]
    }
};