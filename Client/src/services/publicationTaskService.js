import { HTTP } from '../utils/axios'

class PublishDocumentTaskService {
	async getTasksByFilter (filter) {
		return HTTP.get('api/publishDocumentTask/GetTasksByFilter', { params: filter })
	}
	async getAttemptsByTaskId (id) {
		return HTTP.get('api/publishDocumentTask/GetAttemptsByTaskId', { params: { id } })
	}
	async reEnqueueTaskById (id) {
		return HTTP.post('api/publishDocumentTask/ReEnqueueTaskById', `"${id}"`)
	}
}

export default new PublishDocumentTaskService()
