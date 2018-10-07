import { HTTP } from '../utils/axios'

class PublishDocumentTaskService {
	url = 'api/publishDocumentTask'
	async getTasksByFilter (filter) {
		return HTTP.get(this.url + '/GetTasksByFilter', { params: filter })
	}
	async getAttemptsByTaskId (id) {
		return HTTP.get(this.url + '/GetAttemptsByTaskId', { params: { id } })
	}
}

export default new PublishDocumentTaskService()
