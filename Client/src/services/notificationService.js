import * as signalR from '@aspnet/signalr'
import { PublishState } from '../shared/enums'

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
		window.console.log('subscribed')
		for (let ps of PublishState) {
			this.hubConnection.on(ps.Name, this.callback)
		}
		this.hubConnection.start()
		this.hubConnection.onclose(() => {
			this.subscribe()
		})
	}

	unsubscribe () {
		if (this.hubConnection) {
			this.hubConnection.stop()
		}
	}
}
