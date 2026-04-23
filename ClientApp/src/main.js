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

import dayjs from 'dayjs'
import duration from 'dayjs/plugin/duration'
dayjs.extend(duration)

// Directives
import numberFormat from './directives/numberFormat'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Viewer)
app.use(VCalendar, {})

// Directives 등록
app.directive('number-format', numberFormat)

// Quill 전역 설정
setupQuill()

// Auth 초기화
const authStore = useAuthStore()
authStore.initAuth()

app.mount('#app')

// Capacitor 네이티브 플랫폼 처리 (뒤로가기 + 상태바)
import { Capacitor } from '@capacitor/core'
import { useUIStore } from './stores/ui'
if (Capacitor.isNativePlatform()) {
  document.body.classList.add('capacitor-app')
  import('@capacitor/app').then(({ App }) => {
    const uiStore = useUIStore()
    App.addListener('backButton', ({ canGoBack }) => {
      if (uiStore.hasOpenModal()) {
        uiStore.closeTopModal()
      } else if (canGoBack) {
        window.history.back()
      } else {
        App.exitApp()
      }
    })
  })

  import('@capacitor/status-bar')
    .then(({ StatusBar, Style }) => {
      StatusBar.setOverlaysWebView({ overlay: false })
      StatusBar.setStyle({ style: Style.Light })
      StatusBar.setBackgroundColor({ color: '#FFFFFF' })
    })
    .catch(() => {})
}
