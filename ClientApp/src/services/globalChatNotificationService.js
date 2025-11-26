import * as signalR from '@microsoft/signalr'

/**
 * 글로벌 채팅 알림 서비스
 * 사용자가 로그인하면 자신의 개인 그룹(user-{userId})에 가입하여
 * 실시간으로 unread count 업데이트를 받습니다.
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

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`/chathub?userId=${userId}`, {
                accessTokenFactory: () => token,
            })
            .withAutomaticReconnect()
            .build()

        try {
            await this.connection.start()
            console.log('Global SignalR Connected.')
            this.isConnecting = false

            // OnConnectedAsync에서 개인 그룹 가입을 처리하지만, 만약을 위해 클라이언트에서도 호출
            this.connection.invoke('JoinUserGroup', userId).catch((err) => {
                return console.error('Failed to join user group:', err.toString())
            })
        } catch (error) {
            console.error('Global SignalR connection failed: ', error)
            this.isConnecting = false
            // 5초 후 재연결 시도
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