import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import { useAuthStore } from './stores/auth'

import App from './App.vue'
import './assets/main.css'


import 'viewerjs/dist/viewer.css'
import Viewer from 'v-viewer'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Viewer)

// Auth 초기화
const authStore = useAuthStore()
authStore.initAuth()

app.mount('#app')
