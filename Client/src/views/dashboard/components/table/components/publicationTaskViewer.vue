<template>
	<div>
		<el-table :data="publicationTasks" stripe bordered>
			<el-table-column type="expand">
				<template slot-scope="{row}">
					<p><strong>Guid:</strong> {{row.Id}}</p>
					<p><strong>UserId:</strong> {{row.UserId}}</p>
					<p><strong>OrganizationId:</strong> {{row.OrganizationId}}</p>
					<p><strong>RefId:</strong> {{row.RefId}}</p>
					<p><strong>LoadId:</strong> {{row.LoadId}}</p>
					<p><strong>DocumentId:</strong> {{row.DocumentId}}</p>
					<p><strong>DocumentRevision:</strong> {{row.DocumentRevision}}</p>
				</template>
			</el-table-column>
			<el-table-column type="index" :width="45"/>
			<el-table-column label="Guid" property="Id"/>
			<el-table-column label="UserId" property="UserId" :width="130"/>
			<el-table-column label="OrgId" property="OrganizationId" :width="130"/>
			<el-table-column label="DocumentType"
							 property="DocumentType"
							 :formatter="convertToDocumentType"
							 :width="250"
			/>
			<el-table-column label="CreatedAt" property="CreatedAt" :width="130">
				<template slot-scope="{row}">
					<el-popover :content="dateFormat(row.CreatedAt)" trigger="hover">
						<span slot="reference">{{distanceInWordsToNow(row.CreatedAt)}} </span>
					</el-popover>
				</template>
			</el-table-column>
			<el-table-column label="UpdatedAt" property="UpdatedAt" :width="130">
				<template slot-scope="{row}">
					<el-popover v-if="row.UpdatedAt" :content="dateFormat(row.UpdatedAt)" trigger="hover">
						<span slot="reference">{{distanceInWordsToNow(row.UpdatedAt)}} </span>
					</el-popover>
				</template>
			</el-table-column>
			<el-table-column label="IsHasEisError" property="IsHasEisError" :width="120">
				<template slot-scope="{row}">
					<el-tag :type="row.IsHasEisError ? 'danger' : 'success'">{{row.IsHasEisError ? 'Ошибка' : 'Ок'}}
					</el-tag>
				</template>
			</el-table-column>
			<el-table-column label="IsDelivered" property="IsDelivered" :width="125">
				<template slot-scope="{row}">
					<el-tag :type="row.IsDelivered ? 'success' : 'info'">
						{{row.IsDelivered ? 'Доставлено' : 'Недоставлено'}}
					</el-tag>
				</template>
			</el-table-column>
			<el-table-column label="State" property="State" :width="130">
				<template slot-scope="{row}">
					<el-tag :type="getStateTagType(row.State)">{{convertToState(row.State)}}</el-tag>
				</template>
			</el-table-column>
		</el-table>

		<el-pagination background
					   :current-page="page"
					   @size-change="handlePageSizeChange"
					   @current-change="handleCurrentPageChange"
					   :page-sizes="[5,10,20,50,100]"
					   :page-size="pageSize"
					   layout="total, sizes, prev, pager, next, jumper"
					   :total="totalItems"
		/>
	</div>
</template>

<script>
	import { Pagination, Popover, Table, TableColumn, Tag } from 'element-ui'
	import { DocumentType, PublishState } from '../../../../../shared/enums'
	import PublicationTaskService from '../../../../../services/publicationTaskService'
	import { distanceInWordsToNow, format } from 'date-fns'
	import ruLocale from 'date-fns/locale/ru'

	export default {
		name: 'publication-task-viewer',
		components: {
			'el-table': Table,
			'el-table-column': TableColumn,
			'el-pagination': Pagination,
			'el-tag': Tag,
			'el-popover': Popover,
		},
		props: {
			publicationTasks: {
				type: Array,
			},
			page: {
				type: Number,
				required: true,
			},
			pageSize: {
				type: Number,
				required: true,
			},
			totalItems: {
				type: Number,
				required: true,
			},
		},
		methods: {
			dateFormat (value) {
				return format(value, 'DD MMM YYYY HH:mm:ss', {locale: ruLocale})
			},
			distanceInWordsToNow (value) {
				return distanceInWordsToNow(value, {locale: ruLocale})
			},
			getAttempts (taskId) {
				return PublicationTaskService.getAttemptsByTaskId(taskId)
					.then(response => response.data)
			},
			convertToDocumentType (row, column, documentType) {
				let doc = DocumentType.find(w => w.Value === documentType)
				return doc !== undefined ? doc.Name : 'Name not found'
			},
			convertToState (stateValue) {
				let state = PublishState.find(w => w.Value === stateValue)
				return state !== undefined ? state.Name : 'Name not found'
			},
			getStateTagType (stateValue) {
				let state = PublishState.find(w => w.Value === stateValue)
				if (state !== undefined) {
					switch (state.Name) {
						case 'Canceled':
						case 'None':
							return 'info'
						case 'Processing':
							return ''
						case 'Failed':
						case 'XmlValidationError':
							return 'danger'
						case 'Published':
							return 'success'
					}
				}
				return ''
			},
			handleCurrentPageChange (newPage) {
				this.$emit('update:page', newPage)
				this.emitEvents()
			},
			handlePageSizeChange (newPageSize) {
				this.$emit('update:pageSize', newPageSize)
				this.emitEvents()
			},
			emitEvents () {
				this.$emit('submit')
			},
		},
	}
</script>

<style>
	.el-table + .el-pagination {
		margin-bottom: 15px;
	}
</style>
