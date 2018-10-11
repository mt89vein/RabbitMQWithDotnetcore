<template>
	<div class="filter-form--wrapper">
		<div class="filter-form--header">
			<i :class="filterIconClass"
			   @click="showFilterForm = !showFilterForm"></i>
			Фильтр по задачам на публикацию
		</div>
		<div class="filter-form--body">
			<el-collapse-transition>
				<el-form ref="form" label-width="250px" v-show="showFilterForm">
					<el-form-item label="Идентификатор">
						<el-input v-model="model.Id" type="text"/>
					</el-form-item>
					<el-form-item label="Тип документа">
						<el-select v-model="model.DocumentTypes" placeholder="Выберите тип документа" multiple>
							<el-option v-for="(ps, $index) in DocumentType"
									   :label="ps.Label"
									   :value="ps.Value"
									   :key="$index"
							/>
						</el-select>
					</el-form-item>
					<el-form-item label="Идентификаторы документа">
						<el-col :span="12">
							<el-input v-model="model.DocumentId" type="number" placeholder="Идентификатор документа"/>
						</el-col>
						<el-col :span="12">
							<el-input v-model="model.DocumentRevision" type="number" placeholder="Ревизия документа"/>
						</el-col>
					</el-form-item>
					<el-form-item label="По доставке">
						<el-select v-model="model.IsDelivered" clearable>
							<el-option label="Недоставлено" value="false"/>
							<el-option label="Доставлено" value="true"/>
						</el-select>
					</el-form-item>
					<el-form-item label="По дате отправки на публикацию">
						<el-date-picker v-model="model.Enqueued"
										type="daterange"
										range-separator="по"
										start-placeholder="Start date"
										end-placeholder="End date"
						/>
					</el-form-item>
					<el-form-item label="Состояние публикации">
						<el-select v-model="model.States" multiple placeholder="Выберите состояние">
							<el-option v-for="(ps, $index) in PublishState"
									   :label="ps.Name"
									   :value="ps.Value"
									   :key="$index"
							/>
						</el-select>
					</el-form-item>
					<el-form-item label="Идентификатор пользователя">
						<el-input v-model="model.UserId" type="number"/>
					</el-form-item>
					<el-form-item label="Идентификатор организации">
						<el-input v-model="model.OrganizationId" type="number"/>
					</el-form-item>
					<el-form-item label="Внутренние ошибки ЕИС">
						<el-select v-model="model.HasEisExceptions" clearable>
							<el-option label="Без ошибок" value="false"/>
							<el-option label="С ошибками" value="true"/>
						</el-select>
					</el-form-item>
					<el-form-item>
						<el-button type="primary" @click="onSubmit">Применить</el-button>
						<el-button type="default" @click="onClear">Очистить</el-button>
					</el-form-item>
				</el-form>
			</el-collapse-transition>
		</div>
	</div>
</template>

<script>
	import { Button, Col, DatePicker, Form, FormItem, Input, Option, Select } from 'element-ui'
	import CollapseTransition from 'element-ui/lib/transitions/collapse-transition'
	import { DocumentType, PublishState } from '../../../../../shared/enums'

	export default {
		name: 'filter-form',
		components: {
			'el-form': Form,
			'el-form-item': FormItem,
			'el-col': Col,
			'el-select': Select,
			'el-input': Input,
			'el-button': Button,
			'el-option': Option,
			'el-date-picker': DatePicker,
			'el-collapse-transition': CollapseTransition,
		},
		props: {
			filter: {
				type: Object,
				required: true,
			},
		},
		model: {
			prop: 'filter',
			event: 'submit',
		},
		data () {
			return {
				DocumentType,
				PublishState,
				model: {
					Id: null,
					DocumentTypes: [],
					DocumentId: null,
					DocumentRevision: null,
					Enqueued: [],
					IsDelivered: null,
					HasEisExceptions: null,
					OrganizationId: null,
					UserId: null,
					States: [],
					Page: 1,
					PageSize: 5,
				},
				showFilterForm: false,
			}
		},
		methods: {
			onSubmit () {
				this.$emit('submit', {...this.model})
			},
			onClear () {
				this.model.DocumentTypes = []
				this.model.DocumentId = null
				this.model.DocumentRevision = null
				this.model.Enqueued = []
				this.model.IsDelivered = null
				this.model.HasEisExceptions = null
				this.model.OrganizationId = null
				this.model.UserId = null
				this.model.States = []
				this.onSubmit()
			},
		},
		computed: {
			filterIconClass () {
				return {
					'el-icon-arrow-down': !this.showFilterForm,
					'el-icon-arrow-up': this.showFilterForm,
				}
			},
		},
		watch: {
			filter: {
				immediate: true,
				deep: true,
				handler (filter) {
					this.model = {...filter}
				}
			}
		}
	}
</script>

<style lang="scss">
	@import "../../../../../../element-variables";

	.el-date-editor.el-range-editor.el-input__inner.el-date-editor--daterange, .el-select {
		width: 100%;
	}

	.filter-form--body {
		width: 900px;
		margin: auto;
	}

	.filter-form--header {
		height: 25px;
		line-height: 25px;
		padding: 10px;
		margin-bottom: 25px;
		background-color: $--color-primary-light-6;
		border-radius: 4px;
		border: 1px solid $--color-primary;
	}
</style>
