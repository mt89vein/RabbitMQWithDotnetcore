import * as signalR from '@aspnet/signalr'

export default class NotificationService {
	constructor (callback) {
		this.callback = callback
		this.connect()
	}

	connect () {
		this.hubConnection = new signalR.HubConnectionBuilder()
			.withUrl(`${process.env.VUE_APP_BASE_URL}/notifications`)
			.configureLogging(signalR.LogLevel.Information)
			.build()

		this.subscribe()
	}

	subscribe () {
		this.hubConnection.on('OnPublicationStateChanged', this.callback)
		this.hubConnection.start()
		this.hubConnection.onclose(() => {
			this.subscribe()
		})
	}
}
