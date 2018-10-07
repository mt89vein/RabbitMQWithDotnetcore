import Axios from 'axios'
import qs from 'qs'

const HTTP = Axios.create({
	baseURL: process.env.VUE_APP_BASE_URL,
	headers: { 'Content-Type': 'application/json' },
})

HTTP.defaults.paramsSerializer = params => {
	return qs.stringify(params, { arrayFormat: 'repeat' })
}

export { HTTP }
