module.exports = {
	root: true,
	env: {
		node: true,
		es6: true,
	},
	'extends': [
		'plugin:vue/recommended',
		'@vue/standard',
	],
	plugins: [
		'vue',
		'html',
	],
	rules: {
		'no-console': process.env.NODE_ENV === 'production' ? 'error' : 'off',
		'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
		'vue/html-indent': [
			'error',
			'tab'
		],
		'no-unneeded-ternary': [
			'error',
			{
				defaultAssignment: false
			}
		],
		'arrow-parens': 0,
		'generator-star-spacing': 'off',
		'no-console': 'off',
		'no-debugger': 'off',
		'no-tabs': 0,
		indent: [
			'error',
			'tab'
		],
		quotes: [
			'error',
			'single'
		],
		semi: [
			'error',
			'never'
		],
		'indent-size': [
			true,
			2
		],
		'no-mixed-spaces-and-tabs': [
			'error',
			'smart-tabs'
		],
		'comma-dangle': [
			'error',
			'always-multiline'
		],
		'vue/attribute-hyphenation': [
			'error',
			'always'
		],
		'vue/html-end-tags': 'error',
		'vue/html-self-closing': 'error',
		'vue/require-prop-types': 'error',
		'vue/attributes-order': 'error',
		'vue/html-quotes': [
			'error',
			'double'
		],
		'vue/order-in-components': 'error'
	},
	parserOptions: {
		parser: 'babel-eslint'
	},
}
