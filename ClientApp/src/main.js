import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import './assets/main.css' // 전역 CSS 파일 임포트

// 1. Vue 앱 인스턴스 생성
const app = createApp(App)

// 2. Pinia 플러그인 등록
app.use(createPinia())

// 3. 앱을 index.html의 #app 요소에 마운트
app.mount('#app')