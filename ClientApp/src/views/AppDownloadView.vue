<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <MainHeader title="앱 다운로드" :show-back="true" />

    <div class="px-4 py-8">
      <div class="max-w-sm mx-auto">
        <!-- 앱 아이콘 + 이름 -->
        <div class="text-center mb-8">
          <div
            class="w-20 h-20 bg-blue-600 rounded-2xl mx-auto flex items-center justify-center shadow-lg mb-4"
          >
            <Smartphone class="w-10 h-10 text-white" />
          </div>
          <h2 class="text-xl font-bold text-gray-900">StarTour</h2>
          <p class="text-sm text-gray-500 mt-1">행사 관리 앱</p>
        </div>

        <!-- 다운로드 카드 -->
        <div class="bg-white rounded-2xl border border-gray-100 shadow-sm p-5">
          <div class="flex items-center gap-3 mb-4">
            <div
              class="w-10 h-10 bg-green-50 rounded-xl flex items-center justify-center"
            >
              <Download class="w-5 h-5 text-green-600" />
            </div>
            <div>
              <p class="text-sm font-semibold text-gray-800">Android APK</p>
              <p v-if="fileInfo" class="text-xs text-gray-400">
                {{ fileInfo.size }} | {{ fileInfo.date }}
              </p>
              <p v-else class="text-xs text-gray-400">최신 버전</p>
            </div>
          </div>

          <a
            :href="downloadUrl"
            download="StarTour.apk"
            class="block w-full py-3 bg-blue-600 text-white text-center text-sm font-semibold rounded-xl hover:bg-blue-700 active:scale-[0.98] transition-all"
          >
            다운로드
          </a>

          <p class="text-xs text-gray-400 text-center mt-3">
            설치 시 '출처를 알 수 없는 앱' 허용이 필요합니다
          </p>
        </div>

        <!-- 설치 안내 -->
        <div
          class="mt-6 bg-white rounded-2xl border border-gray-100 shadow-sm p-5"
        >
          <p class="text-sm font-semibold text-gray-800 mb-3">설치 방법</p>
          <ol class="space-y-2 text-sm text-gray-600">
            <li class="flex gap-2">
              <span
                class="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full text-xs flex items-center justify-center font-bold"
                >1</span
              >
              <span>위 다운로드 버튼을 눌러 APK 파일을 받습니다</span>
            </li>
            <li class="flex gap-2">
              <span
                class="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full text-xs flex items-center justify-center font-bold"
                >2</span
              >
              <span>다운로드된 파일을 열어 설치합니다</span>
            </li>
            <li class="flex gap-2">
              <span
                class="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full text-xs flex items-center justify-center font-bold"
                >3</span
              >
              <span
                >'출처를 알 수 없는 앱 설치 허용' 팝업이 뜨면 허용을
                누릅니다</span
              >
            </li>
          </ol>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Smartphone, Download } from 'lucide-vue-next'
import MainHeader from '@/components/common/MainHeader.vue'

const downloadUrl = '/downloads/StarTour.apk'
const fileInfo = ref(null)

onMounted(async () => {
  try {
    const res = await fetch(downloadUrl, { method: 'HEAD' })
    if (res.ok) {
      const size = res.headers.get('content-length')
      const lastModified = res.headers.get('last-modified')
      fileInfo.value = {
        size: size ? `${(size / 1024 / 1024).toFixed(1)}MB` : '',
        date: lastModified
          ? new Date(lastModified).toLocaleString('ko-KR', {
              year: 'numeric',
              month: '2-digit',
              day: '2-digit',
              hour: '2-digit',
              minute: '2-digit',
            })
          : '',
      }
    }
  } catch {
    // 파일 정보를 못 가져와도 다운로드는 가능
  }
})
</script>
