import { getAccessToken } from "./auth-store.js";
import { CONFIG } from "./config.js";

export class SignalingClient {
    constructor() {
        this.connection = null;
        this.handlers = {
            startCall: null,
            receiveOffer: null,
            receiveAnswer: null,
            receiveIceCandidate: null,
            userDisconnected: null
        };
    }

    async connect() {
        if (!window.signalR) {
            throw new Error("SignalR client не подключён");
        }

        this.connection = new window.signalR.HubConnectionBuilder()
            .withUrl(CONFIG.VOICE_HUB_URL, {
                accessTokenFactory: () => getAccessToken() || ""
            })
            .withAutomaticReconnect()
            .build();

        this.connection.on("StartCall", (isInitiator) => {
            this.handlers.startCall?.(isInitiator);
        });

        this.connection.on("ReceiveOffer", (offer) => {
            this.handlers.receiveOffer?.(offer);
        });

        this.connection.on("ReceiveAnswer", (answer) => {
            this.handlers.receiveAnswer?.(answer);
        });

        this.connection.on("ReceiveIceCandidate", (candidate) => {
            this.handlers.receiveIceCandidate?.(candidate);
        });

        this.connection.on("UserDisconnected", () => {
            this.handlers.userDisconnected?.();
        });

        await this.connection.start();
    }

    onStartCall(handler) {
        this.handlers.startCall = handler;
    }

    onReceiveOffer(handler) {
        this.handlers.receiveOffer = handler;
    }

    onReceiveAnswer(handler) {
        this.handlers.receiveAnswer = handler;
    }

    onReceiveIceCandidate(handler) {
        this.handlers.receiveIceCandidate = handler;
    }

    onUserDisconnected(handler) {
        this.handlers.userDisconnected = handler;
    }

    async joinRoom(roomId) {
        await this.connection.invoke("JoinRoom", roomId);
    }

    async sendOffer(offer) {
        await this.connection.invoke("SendOffer", offer);
    }

    async sendAnswer(answer) {
        await this.connection.invoke("SendAnswer", answer);
    }

    async sendIceCandidate(candidate) {
        await this.connection.invoke("SendIceCandidate", candidate);
    }
}