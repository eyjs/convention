import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import { useAuthStore } from './stores/auth'

import App from './App.vue'
import './assets/main.css'
import { setupQuill } from './plugins/quill'

import 'viewerjs/dist/viewer.css'
import 'v-calendar/style.css'
import Viewer from 'v-viewer'
import VCalendar from 'v-calendar'

import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
dayjs.extend(duration);

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Viewer)
app.use(VCalendar, {})

// Quill 전역 설정
setupQuill()

// Auth 초기화
const authStore = useAuthStore()
authStore.initAuth()

app.mount('#app')
