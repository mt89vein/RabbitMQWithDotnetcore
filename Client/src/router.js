import Vue from 'vue'
import Router from 'vue-router'
import Layout from './views/layout/index'

Vue.use(Router)

export default new Router({
	mode: 'history',
	scrollBehavior: () => ({ y: 0 }),
	routes: [
		{
			path: '',
			component: Layout,
			redirect: 'dashboard',
			children: [
				{
					path: 'dashboard',
					component: () => import('./views/dashboard/index'),
					name: 'Dashboard',
					meta: {
						title: 'dashboard', icon: 'dashboard',
					},
				},
			],
		},
		{
			path: '/404',
			component: () => import('./views/errorPage/404'),
			hidden: true,
		},
		{
			path: '*', redirect: '/404',
		},
	],
})
