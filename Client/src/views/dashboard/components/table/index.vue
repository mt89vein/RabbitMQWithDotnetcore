<template>
	<div>
		<filter-form v-model="filter"
					 @submit="onSubmitFilter"
		/>
		<publication-task-viewer :publication-tasks="publicationTasksFiltered"
								 :page.sync="filter.Page"
								 :page-size.sync="filter.PageSize"
								 @submit="onSubmitFilter"
								 :total-items="totalItems"
		/>
	</div>
</template>

<script>
	import FilterForm from './components/filterForm'
	import PublicationTaskViewer from './components/publicationTaskViewer'
	import NotificationService from './../../../../services/notificationService'
	import PublishDocumentTaskService from './../../../../services/publicationTaskService'
	import { Loading, Notification } from 'element-ui'
	import {startOfDay, endOfDay, isAfter, isBefore } from 'date-fns'

	export default {
		name: 'publication-task',
		components: {
			PublicationTaskViewer,
			FilterForm,
		},
		data () {
			return {
				filter: {
					Id: null,
					DocumentTypes: [],
					DocumentId: null,
					DocumentRevision: null,
					Enqueued: [startOfDay(new Date()), endOfDay(new Date())],
					IsDelivered: null,
					IsHasEisErrors: null,
					OrganizationId: null,
					UserId: null,
					States: [],
					Page: 1,
					PageSize: 5,
				},
				totalItems: 0,
				publicationTasks: [],
				notificationService: null,
			}
		},
		created () {
			this.notificationService = new NotificationService(this.updateTask)
		},
		mounted () {
			this.getTasks()
		},
		beforeDestroy () {
			this.notificationService = null
		},
		methods: {
			getTasks () {
				const loading = Loading.service({
					target: '.el-table',
					text: 'Загрузка',
				})
				PublishDocumentTaskService.getTasksByFilter(this.filter).then(
					response => {
						this.publicationTasks = response.data.Objects
						this.totalItems = response.data.Count
						loading.close()
					},
					fail => {
						Notification.error({
							title: 'Ошибка',
							message: 'Не удалось получить список задач',
							type: 'error',
						})
						this.publicationTasks = []
						this.totalItems = 0
						loading.close()
					},
				)
			},
			onSubmitFilter () {
				this.getTasks()
			},
			updateTask (publicationTaskRaw) {
				const publicationTask = JSON.parse(publicationTaskRaw)
				const index = this.publicationTasks.findIndex(
					w => w.Id === publicationTask.Id,
				)
				const isSatisfiedForCurrentFilter = this.isSatisfiedForCurrentFilter(
					publicationTask,
				)
				if (index !== -1) {
					this.$set(this.publicationTasks, index, publicationTask)
					if (!isSatisfiedForCurrentFilter && this.totalItems > 0) {
						this.totalItems--
					}
				} else {
					if (this.filter.PageSize > this.publicationTasksFiltered.length && isSatisfiedForCurrentFilter) {
						this.publicationTasks.unshift(publicationTask)
						this.totalItems++
					}
				}
			},
			isSatisfiedForCurrentFilter (publicationTask) {
				let satisfied = true
				if (this.filter.Id) {
					satisfied = this.filter.Id === publicationTask.Id
				}
				if (this.filter.DocumentId !== null) {
					satisfied =
						this.filter.DocumentId === publicationTask.DocumentId
				}
				if (this.filter.DocumentRevision !== null) {
					satisfied =
						this.filter.DocumentRevision ===
						publicationTask.DocumentRevision
				}
				if (
					this.filter.DocumentTypes !== null &&
					this.filter.DocumentTypes.length > 0
				) {
					satisfied = this.filter.DocumentTypes.includes(
						publicationTask.DocumentType,
					)
				}
				if (this.filter.Enqueued && this.filter.Enqueued.length == 2) {
					satisfied = isAfter(publicationTask.CreatedAt, this.filter.Enqueued[0]) &&
						isBefore(publicationTask.CreatedAt, this.filter.Enqueued[1])
				}
				if (this.filter.IsHasEisErrors !== null) {
					satisfied =
						(this.filter.IsHasEisErrors == 'true' &&
							publicationTask.IsHasEisErrors) ||
						(this.filter.IsHasEisErrors == 'false' &&
							!publicationTask.IsHasEisErrors)
				}
				if (this.filter.IsDelivered !== null) {
					satisfied =
						(this.filter.IsDelivered == 'true' &&
							publicationTask.IsDelivered) ||
						(this.filter.IsDelivered == 'false' &&
							!publicationTask.IsDelivered)
				}
				if (this.filter.OrganizationId !== null) {
					satisfied =
						this.filter.OrganizationId ===
						publicationTask.OrganizationId
				}
				if (this.filter.UserId !== null) {
					satisfied = this.filter.UserId === publicationTask.UserId
				}
				if (this.filter.States != null && this.filter.States.length > 0) {
					satisfied = this.filter.States.includes(publicationTask.State)
				}

				return satisfied
			},
		},
		computed: {
			publicationTasksFiltered () {
				return this.publicationTasks.filter(
					this.isSatisfiedForCurrentFilter,
				)
			},
		},
	}
</script>
