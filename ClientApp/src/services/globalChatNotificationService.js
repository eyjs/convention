import * as signalR from '@microsoft/signalr'

/**
 * �۷ι� ä�� �˸� ����
 * ����ڰ� �α����ϸ� �ڽ��� ���� �׷�(user-{userId})�� �����Ͽ�
 * �ǽð����� unread count ������Ʈ�� �޽��ϴ�.
 */
class GlobalChatNotificationService {
    constructor() {
        this.connection = null
        this.isConnecting = false
    }

    async connect(userId, token) {
        if (this.connection || this.isConnecting) {
            console.log('Global SignalR connection already exists or is connecting.')
            return
        }

        this.isConnecting = true

        // 배포 환경에서는 백엔드 직접 연결 (Vercel proxy는 WebSocket 미지원)
        const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || '/chathub'

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${hubUrl}?userId=${userId}`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .build()

        try {
            await this.connection.start()
            console.log('Global SignalR Connected.')
            this.isConnecting = false

            // OnConnectedAsync���� ���� �׷� ������ ó��������, ������ ���� Ŭ���̾�Ʈ������ ȣ��
            this.connection.invoke('JoinUserGroup', userId).catch((err) => {
                return console.error('Failed to join user group:', err.toString())
            })
        } catch (error) {
            this.isConnecting = false

            // Mixed Content 에러는 조용히 처리 (HTTPS 백엔드 없음)
            if (error.message && error.message.includes('Failed to fetch')) {
                console.warn('SignalR connection failed (Mixed Content). Real-time notifications disabled.')
                return // 재연결 시도 안 함
            }

            console.error('Global SignalR connection failed: ', error)
            // 다른 에러는 5초 후 재연결 시도
            setTimeout(() => this.connect(userId, token), 5000)
        }
    }

    disconnect() {
        if (this.connection) {
            this.connection.stop()
            this.connection = null
            console.log('Global SignalR Disconnected.')
        }
    }

    onUnreadCountIncrement(callback) {
        if (!this.connection) return
        this.connection.on('UnreadCountIncrement', (data) => {
            callback(data.conventionId)
        })
    }

    onReconnected(callback) {
        if (!this.connection) return
        this.connection.onreconnected(callback)
    }

    get isConnected() {
        return (
            this.connection &&
            this.connection.state === signalR.HubConnectionState.Connected
        )
    }
}

const service = new GlobalChatNotificationService()
export default service