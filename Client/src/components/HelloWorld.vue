<template>
	<div class="hello">
		<publication-task-viewer :publication-tasks="notReadyPublicationTasks"/>
	</div>
</template>

<script>
	import * as signalR from '@aspnet/signalr'
	import PublicationTaskViewer from '../views/dashboard/components/tables/PublicationTaskViewer'
	import axios from 'axios'

	export default {
		name: 'HelloWorld',
		components: {PublicationTaskViewer},
		data() {
			return {
				publicationTasks: [],
				hubConnection: null,
			}
		},
		created() {
			axios.get('https://localhost:44364/api/monitoring')
				.then(response => {
					this.publicationTasks = response.data

					this.hubConnection = new signalR.HubConnectionBuilder()
						.withUrl('https://localhost:44364/notifications')
						.configureLogging(signalR.LogLevel.Information)
						.build()

					this.hubConnection.on('Processing', this.updateTask)
					this.hubConnection.start()
				})
		},
		methods: {

		},
		computed: {
			notReadyPublicationTasks () {
				return this.publicationTasks.filter(w => w.State === 3)
			}
		},
		beforeDestroy() {
			this.hubConnection.close()
			this.hubConnection = null
		},
	}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
	h3 {
		margin: 40px 0 0;
	}

	ul {
		list-style-type: none;
		padding: 0;
	}

	li {
		display: inline-block;
		margin: 0 10px;
	}

	a {
		color: #42b983;
	}
</style>
