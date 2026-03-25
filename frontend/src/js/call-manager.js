import { CONFIG } from "./config.js";

export class CallManager {
    constructor(localVideo, remoteVideo, onStatusChange, onMessageReceived) {
        this.localVideo = localVideo;
        this.remoteVideo = remoteVideo;
        this.onStatusChange = onStatusChange || (() => {});
        this.onMessageReceived = onMessageReceived || (() => {});

        this.peerConnection = null;

        this.localStream = new MediaStream();
        this.remoteStream = new MediaStream();

        this.localVideo.srcObject = this.localStream;
        this.remoteVideo.srcObject = this.remoteStream;

        this.dataChannel = null;

        this.audioTransceiver = null;
        this.videoTransceiver = null;

        this.isMuted = false;
        this.isCameraOff = false;

        this.isMakingOffer = false;
        this.onNegotiationNeededHandler = null;
    }

    setStatus(message) {
        this.onStatusChange(message);
    }

    ensurePeerConnectionReady() {
        if (!this.peerConnection) {
            throw new Error("Соединение ещё не подготовлено");
        }
    }

    createPeerConnection(onIceCandidate, onNegotiationNeeded) {
        if (this.peerConnection) {
            return;
        }

        this.onNegotiationNeededHandler = onNegotiationNeeded || null;

        this.peerConnection = new RTCPeerConnection(CONFIG.RTC);

        this.audioTransceiver = this.peerConnection.addTransceiver("audio", {
            direction: "sendrecv"
        });

        this.videoTransceiver = this.peerConnection.addTransceiver("video", {
            direction: "sendrecv"
        });

        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                onIceCandidate?.(event.candidate);
            }
        };

        this.peerConnection.ontrack = (event) => {
            const track = event.track;
            if (!track) return;

            const exists = this.remoteStream.getTracks().some((t) => t.id === track.id);
            if (!exists) {
                this.remoteStream.addTrack(track);
            }

            this.remoteVideo.srcObject = this.remoteStream;

            track.onended = () => {
                this.removeRemoteTrack(track.id);
            };
        };

        this.peerConnection.onconnectionstatechange = () => {
            this.setStatus(`Соединение: ${this.peerConnection.connectionState}`);
        };

        this.peerConnection.ondatachannel = (event) => {
            this.dataChannel = event.channel;
            this.setupDataChannel();
        };

        this.peerConnection.onnegotiationneeded = async () => {
            if (!this.onNegotiationNeededHandler) return;

            try {
                await this.onNegotiationNeededHandler();
            } catch (error) {
                console.error("Negotiation error:", error);
            }
        };

        this.setStatus("Соединение подготовлено");
    }

    removeRemoteTrack(trackId) {
        const track = this.remoteStream.getTracks().find((t) => t.id === trackId);
        if (!track) return;

        this.remoteStream.removeTrack(track);
        this.remoteVideo.srcObject = this.remoteStream;
    }

    async enableMicrophone() {
        this.ensurePeerConnectionReady();

        if (!this.audioTransceiver) {
            throw new Error("Audio transceiver не создан");
        }

        if (this.audioTransceiver.sender.track) {
            this.audioTransceiver.sender.track.enabled = true;
            this.isMuted = false;
            this.setStatus("Микрофон включён");
            return;
        }

        const audioStream = await navigator.mediaDevices.getUserMedia({
            audio: {
                echoCancellation: true,
                noiseSuppression: true,
                autoGainControl: true
            },
            video: false
        });

        const audioTrack = audioStream.getAudioTracks()[0];
        if (!audioTrack) {
            throw new Error("Не удалось получить аудио");
        }

        this.localStream.addTrack(audioTrack);
        this.localVideo.srcObject = this.localStream;

        await this.audioTransceiver.sender.replaceTrack(audioTrack);

        this.isMuted = false;
        this.setStatus("Микрофон включён");
    }

    async enableCamera() {
        this.ensurePeerConnectionReady();

        if (!this.videoTransceiver) {
            throw new Error("Video transceiver не создан");
        }

        if (this.videoTransceiver.sender.track) {
            this.videoTransceiver.sender.track.enabled = true;
            this.isCameraOff = false;
            this.setStatus("Камера включена");
            return;
        }

        const videoStream = await navigator.mediaDevices.getUserMedia({
            audio: false,
            video: true
        });

        const videoTrack = videoStream.getVideoTracks()[0];
        if (!videoTrack) {
            throw new Error("Не удалось получить видео");
        }

        this.localStream.addTrack(videoTrack);
        this.localVideo.srcObject = this.localStream;

        await this.videoTransceiver.sender.replaceTrack(videoTrack);

        this.isCameraOff = false;
        this.setStatus("Камера включена");
    }

    createChatChannel() {
        this.ensurePeerConnectionReady();

        if (this.dataChannel) return;

        this.dataChannel = this.peerConnection.createDataChannel("chat");
        this.setupDataChannel();
    }

    setupDataChannel() {
        if (!this.dataChannel) return;

        this.dataChannel.onopen = () => {
            this.setStatus("Чат готов");
        };

        this.dataChannel.onmessage = (event) => {
            this.onMessageReceived(event.data);
        };
    }

    async createOffer() {
        this.ensurePeerConnectionReady();

        if (!this.dataChannel) {
            this.createChatChannel();
        }

        this.isMakingOffer = true;
        try {
            const offer = await this.peerConnection.createOffer();
            await this.peerConnection.setLocalDescription(offer);
            return this.peerConnection.localDescription;
        } finally {
            this.isMakingOffer = false;
        }
    }

    async handleOffer(offer) {
        this.ensurePeerConnectionReady();

        await this.peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);

        return this.peerConnection.localDescription;
    }

    async handleAnswer(answer) {
        this.ensurePeerConnectionReady();
        await this.peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    }

    async addIceCandidate(candidate) {
        this.ensurePeerConnectionReady();
        await this.peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
    }

    sendChatMessage(message) {
        if (!this.dataChannel || this.dataChannel.readyState !== "open") {
            throw new Error("Чат пока не готов");
        }

        this.dataChannel.send(message);
    }

    toggleMute() {
        const track = this.audioTransceiver?.sender?.track;
        if (!track) {
            throw new Error("Микрофон ещё не включён");
        }

        this.isMuted = !this.isMuted;
        track.enabled = !this.isMuted;

        return this.isMuted;
    }

    toggleCamera() {
        const track = this.videoTransceiver?.sender?.track;
        if (!track) {
            throw new Error("Камера ещё не включена");
        }

        this.isCameraOff = !this.isCameraOff;
        track.enabled = !this.isCameraOff;

        return this.isCameraOff;
    }

    async startScreenShare() {
        this.ensurePeerConnectionReady();

        if (!this.videoTransceiver) {
            throw new Error("Видео ещё не подготовлено");
        }

        const currentVideoTrack = this.videoTransceiver.sender.track;
        if (!currentVideoTrack) {
            throw new Error("Сначала включи камеру");
        }

        const displayStream = await navigator.mediaDevices.getDisplayMedia({
            video: true,
            audio: false
        });

        const screenTrack = displayStream.getVideoTracks()[0];
        if (!screenTrack) {
            throw new Error("Не удалось получить экран");
        }

        await this.videoTransceiver.sender.replaceTrack(screenTrack);

        const previewStream = new MediaStream();
        this.localStream.getAudioTracks().forEach((track) => previewStream.addTrack(track));
        previewStream.addTrack(screenTrack);
        this.localVideo.srcObject = previewStream;

        this.setStatus("Демонстрация экрана включена");

        screenTrack.onended = async () => {
            const cameraTrack = this.localStream.getVideoTracks()[0];
            if (!cameraTrack) return;

            await this.videoTransceiver.sender.replaceTrack(cameraTrack);
            this.localVideo.srcObject = this.localStream;
            this.setStatus("Возврат к камере");
        };
    }

    clearRemoteMedia() {
        this.remoteStream.getTracks().forEach((track) => {
            try {
                track.stop();
            } catch {
                // ignore
            }
        });

        this.remoteStream = new MediaStream();
        this.remoteVideo.srcObject = this.remoteStream;
    }

    hangup() {
        this.dataChannel?.close();

        if (this.peerConnection) {
            this.peerConnection.ontrack = null;
            this.peerConnection.onicecandidate = null;
            this.peerConnection.onconnectionstatechange = null;
            this.peerConnection.onnegotiationneeded = null;
            this.peerConnection.close();
        }

        this.localStream.getTracks().forEach((track) => {
            try {
                track.stop();
            } catch {
                // ignore
            }
        });

        this.clearRemoteMedia();

        this.peerConnection = null;
        this.dataChannel = null;
        this.audioTransceiver = null;
        this.videoTransceiver = null;
        this.localStream = new MediaStream();

        this.localVideo.srcObject = this.localStream;
        this.remoteVideo.srcObject = this.remoteStream;

        this.isMuted = false;
        this.isCameraOff = false;
        this.isMakingOffer = false;

        this.setStatus("Звонок завершён");
    }
}