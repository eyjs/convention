import * as signalR from '@microsoft/signalr'

class ConventionChatService {
  constructor(conventionId, token) {
    if (!conventionId || !token) {
      throw new Error('Convention ID and token are required.')
    }
    this.conventionId = conventionId
    this.token = token
    this.connection = null
  }

  connect() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`/chathub?conventionId=${this.conventionId}`, {
        accessTokenFactory: () => this.token,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    return this.connection
      .start()
      .then(() => {
        console.log('SignalR Connected.')
      })
      .catch((err) => {
        console.error('SignalR Connection Error: ', err)
        return Promise.reject(err)
      })
  }

  onReceiveMessage(callback) {
    if (this.connection) {
      this.connection.on('ReceiveMessage', (messageData) => {
        callback(messageData)
      })
    }
  }

  onUpdateParticipantCount(callback) {
    if (this.connection) {
      this.connection.on('UpdateParticipantCount', (count) => {
        callback(count)
      })
    }
  }

  onUpdateParticipantList(callback) {
    if (this.connection) {
      this.connection.on('UpdateParticipantList', (list) => {
        callback(list)
      })
    }
  }

  onReconnecting(callback) {
    if (this.connection) {
      this.connection.onreconnecting((error) => {
        console.log('SignalR reconnecting...', error)
        callback(error)
      })
    }
  }

  onReconnected(callback) {
    if (this.connection) {
      this.connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected.', connectionId)
        callback(connectionId)
      })
    }
  }

  sendMessage(message) {
    if (
      this.connection &&
      this.connection.state === signalR.HubConnectionState.Connected
    ) {
      return this.connection.invoke('SendMessage', message, this.conventionId)
    }
    return Promise.reject('SignalR connection not established.')
  }

  async getHistory() {
    const response = await fetch(`/api/chat-history/${this.conventionId}`, {
      headers: {
        Authorization: `Bearer ${this.token}`,
      },
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch chat history: ${response.statusText}`)
    }
    return response.json()
  }

  disconnect() {
    if (this.connection) {
      this.connection.stop()
      this.connection = null
    }
  }
}

export default ConventionChatService
