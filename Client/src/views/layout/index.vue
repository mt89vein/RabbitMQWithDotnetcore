<template>
	<el-container>
		<el-main>
			<router-view />
		</el-main>
	</el-container>
</template>

<script>
	import {Main, Container} from 'element-ui'

	export default {
		name: 'Layout',
		components: {
			'el-main': Main,
			'el-container': Container,
		},
	}
</script>
