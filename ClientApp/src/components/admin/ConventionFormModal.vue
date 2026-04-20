<template>
  <BaseModal :is-open="true" max-width="2xl" @close="$emit('close')">
    <template #header>
      <h2 class="text-xl font-bold">
        {{ convention ? '행사 수정' : '새 행사 만들기' }}
      </h2>
    </template>
    <template #body>
      <form class="space-y-6" @submit.prevent="handleSubmit">
        <!-- 행사명 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사명 <span class="text-red-500">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            required
            class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="예: iFA STAR TOUR @ ROMA"
          />
        </div>

        <!-- 행사 유형 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사 유형 <span class="text-red-500">*</span>
          </label>
          <div class="grid grid-cols-2 gap-4">
            <label
              class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{
                'border-primary-500 bg-primary-50':
                  form.conventionType === 'DOMESTIC',
              }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="DOMESTIC"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">국내 행사</div>
                <div class="text-sm text-gray-500">국내에서 진행되는 행사</div>
              </div>
            </label>
            <label
              class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{
                'border-primary-500 bg-primary-50':
                  form.conventionType === 'OVERSEAS',
              }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="OVERSEAS"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">해외 행사</div>
                <div class="text-sm text-gray-500">해외에서 진행되는 행사</div>
              </div>
            </label>
          </div>
        </div>

        <!-- 기간 -->
        <div>
          <DateRangePicker v-model="dateRange" label="기간" />
        </div>

        <!-- 커버 이미지 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2"
            >커버 이미지</label
          >
          <div class="space-y-3">
            <div
              v-if="coverImagePreview"
              class="relative w-full h-48 rounded-lg overflow-hidden bg-gray-100"
            >
              <img
                loading="lazy"
                :src="coverImagePreview"
                alt="커버 이미지 미리보기"
                class="w-full h-full object-cover"
              />
              <button
                type="button"
                class="absolute top-2 right-2 p-2.5 bg-red-500 text-white rounded-full hover:bg-red-600 active:scale-95 transition-all shadow-lg"
                @click="removeCoverImage"
              >
                <X class="w-5 h-5" />
              </button>
            </div>
            <label class="block">
              <input
                ref="coverImageInput"
                type="file"
                accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                class="hidden"
                @change="handleCoverImageChange"
              />
              <div
                class="flex items-center justify-center w-full py-4 px-4 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-400 hover:bg-primary-50 transition-colors active:scale-95"
              >
                <div class="text-center">
                  <ImageIcon class="mx-auto h-12 w-12 text-gray-400" />
                  <p class="mt-2 text-sm text-gray-600 font-medium">
                    이미지 선택
                  </p>
                  <p class="mt-1 text-xs text-gray-500">
                    JPG, PNG, GIF, WebP (최대 5MB)
                  </p>
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- 여행 정보 (국내/해외 분기) -->
        <div class="border rounded-lg p-4 bg-gray-50 space-y-4">
          <h3 class="text-sm font-semibold text-gray-700">
            {{ form.conventionType === 'OVERSEAS' ? '해외' : '국내' }} 여행 정보
          </h3>

          <!-- 행사 장소 (공통) -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              행사 장소
              <span
                v-if="form.conventionType === 'DOMESTIC'"
                class="text-red-500"
                >*</span
              >
            </label>
            <input
              v-model="form.location"
              type="text"
              :required="form.conventionType === 'DOMESTIC'"
              class="w-full px-3 py-2 border rounded-lg"
              :placeholder="
                form.conventionType === 'OVERSEAS'
                  ? '예: 로마 힐튼호텔'
                  : '예: 제주 신라호텔'
              "
            />
          </div>

          <!-- 해외 전용: 목적지 국가/도시 -->
          <div v-if="form.conventionType === 'OVERSEAS'">
            <label class="block text-sm font-medium text-gray-700 mb-1">
              목적지 국가/도시 <span class="text-red-500">*</span>
            </label>
            <div class="grid grid-cols-2 gap-3">
              <select
                v-model="selectedCountry"
                required
                class="w-full px-3 py-2 border rounded-lg text-sm"
                @change="onCountryChange"
              >
                <option value="">국가 선택</option>
                <option
                  v-for="c in overseasCountryList"
                  :key="c.code"
                  :value="c.code"
                >
                  {{ c.name }}
                </option>
              </select>
              <select
                v-model="form.destinationCity"
                required
                class="w-full px-3 py-2 border rounded-lg text-sm"
                :disabled="!selectedCountry"
              >
                <option value="">도시 선택</option>
                <option
                  v-for="city in filteredCities"
                  :key="city.en"
                  :value="city.en"
                >
                  {{ city.ko }} ({{ city.en }})
                </option>
              </select>
            </div>
          </div>

          <div>
            <div class="flex items-center justify-between mb-1">
              <label class="text-sm font-medium text-gray-700"
                >긴급 연락처</label
              >
              <button
                type="button"
                class="text-xs text-primary-600 hover:underline"
                @click="addEmergencyContact"
              >
                + 추가
              </button>
            </div>
            <div class="space-y-2">
              <div
                v-for="(contact, i) in emergencyContacts"
                :key="i"
                class="flex gap-2 items-center"
              >
                <input
                  v-model="contact.role"
                  type="text"
                  class="w-24 px-2 py-1.5 border rounded text-sm"
                  placeholder="역할"
                />
                <input
                  v-model="contact.name"
                  type="text"
                  class="flex-1 px-2 py-1.5 border rounded text-sm"
                  placeholder="이름"
                />
                <input
                  v-model="contact.phone"
                  type="text"
                  class="flex-1 px-2 py-1.5 border rounded text-sm"
                  placeholder="연락처"
                />
                <button
                  type="button"
                  class="text-red-400 hover:text-red-600"
                  @click="emergencyContacts.splice(i, 1)"
                >
                  <X class="w-4 h-4" />
                </button>
              </div>
              <p
                v-if="emergencyContacts.length === 0"
                class="text-xs text-gray-400"
              >
                인솔자, 현지 가이드 등 긴급 연락처를 등록하세요
              </p>
            </div>
          </div>
        </div>

        <!-- 브랜드 컬러 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            브랜드 컬러
          </label>
          <div class="flex items-center space-x-4 mb-2">
            <input
              v-model="form.brandColor"
              type="color"
              class="w-16 h-10 rounded cursor-pointer"
              :disabled="useDefaultColor"
            />
            <input
              v-model="form.brandColor"
              type="text"
              class="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="#6366f1"
              :disabled="useDefaultColor"
            />
          </div>
          <label class="flex items-center space-x-2 cursor-pointer">
            <input
              v-model="useDefaultColor"
              type="checkbox"
              class="w-4 h-4 text-[#17B185] border-gray-300 rounded focus:ring-[#17B185]"
            />
            <span class="text-sm text-gray-700">기본색상 사용 (#17B185)</span>
          </label>
          <p class="text-sm text-gray-500 mt-1">
            행사의 메인 컬러를 설정합니다
          </p>
        </div>
      </form>
    </template>
    <template #footer>
      <button
        type="button"
        class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
        @click="$emit('close')"
      >
        취소
      </button>
      <button
        :disabled="saving || isUploadingImage"
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white rounded-lg transition-colors flex items-center space-x-2"
        @click="handleSubmit"
      >
        <Loader2
          v-if="saving || isUploadingImage"
          class="animate-spin w-4 h-4"
        />
        <span>{{
          isUploadingImage
            ? '업로드 중...'
            : saving
              ? '저장 중...'
              : convention
                ? '수정'
                : '생성'
        }}</span>
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { X, ImageIcon, Loader2 } from 'lucide-vue-next'
import BaseModal from '@/components/common/BaseModal.vue'
import DateRangePicker from '@/components/common/DateRangePicker.vue'
import apiClient from '@/services/api'
import { compressImage } from '@/utils/fileUpload'
import dayjs from 'dayjs'

const props = defineProps({
  convention: Object,
})

const emit = defineEmits(['close', 'save'])

const saving = ref(false)
const useDefaultColor = ref(false)
const DEFAULT_BRAND_COLOR = '#17B185'

const form = ref({
  title: '',
  conventionType: 'DOMESTIC',
  startDate: '',
  endDate: '',
  brandColor: '#6366f1',
  conventionImg: null,
  renderType: 'STANDARD',
  themePreset: 'default',
  location: '',
  destinationCity: '',
  destinationCountryCode: '',
})

const emergencyContacts = ref([])
const selectedCountry = ref('')

const countryList = [
  { code: 'JP', name: '일본' },
  { code: 'CN', name: '중국' },
  { code: 'TW', name: '대만' },
  { code: 'HK', name: '홍콩' },
  { code: 'TH', name: '태국' },
  { code: 'VN', name: '베트남' },
  { code: 'SG', name: '싱가포르' },
  { code: 'MY', name: '말레이시아' },
  { code: 'ID', name: '인도네시아' },
  { code: 'PH', name: '필리핀' },
  { code: 'IN', name: '인도' },
  { code: 'AE', name: 'UAE' },
  { code: 'TR', name: '튀르키예' },
  { code: 'US', name: '미국' },
  { code: 'CA', name: '캐나다' },
  { code: 'MX', name: '멕시코' },
  { code: 'GB', name: '영국' },
  { code: 'FR', name: '프랑스' },
  { code: 'DE', name: '독일' },
  { code: 'IT', name: '이탈리아' },
  { code: 'ES', name: '스페인' },
  { code: 'PT', name: '포르투갈' },
  { code: 'CH', name: '스위스' },
  { code: 'AT', name: '오스트리아' },
  { code: 'CZ', name: '체코' },
  { code: 'HU', name: '헝가리' },
  { code: 'GR', name: '그리스' },
  { code: 'AU', name: '호주' },
  { code: 'NZ', name: '뉴질랜드' },
  { code: 'KR', name: '대한민국' },
]

const cityData = {
  JP: [
    { ko: '도쿄', en: 'Tokyo' },
    { ko: '오사카', en: 'Osaka' },
    { ko: '교토', en: 'Kyoto' },
    { ko: '후쿠오카', en: 'Fukuoka' },
    { ko: '삿포로', en: 'Sapporo' },
    { ko: '나고야', en: 'Nagoya' },
    { ko: '오키나와', en: 'Okinawa' },
  ],
  CN: [
    { ko: '베이징', en: 'Beijing' },
    { ko: '상하이', en: 'Shanghai' },
    { ko: '광저우', en: 'Guangzhou' },
    { ko: '선전', en: 'Shenzhen' },
    { ko: '청두', en: 'Chengdu' },
  ],
  TW: [
    { ko: '타이베이', en: 'Taipei' },
    { ko: '가오슝', en: 'Kaohsiung' },
  ],
  HK: [{ ko: '홍콩', en: 'Hong Kong' }],
  TH: [
    { ko: '방콕', en: 'Bangkok' },
    { ko: '치앙마이', en: 'Chiang Mai' },
    { ko: '푸켓', en: 'Phuket' },
    { ko: '파타야', en: 'Pattaya' },
  ],
  VN: [
    { ko: '하노이', en: 'Hanoi' },
    { ko: '호치민', en: 'Ho Chi Minh City' },
    { ko: '다낭', en: 'Da Nang' },
    { ko: '나트랑', en: 'Nha Trang' },
  ],
  SG: [{ ko: '싱가포르', en: 'Singapore' }],
  MY: [
    { ko: '쿠알라룸푸르', en: 'Kuala Lumpur' },
    { ko: '코타키나발루', en: 'Kota Kinabalu' },
  ],
  ID: [
    { ko: '발리', en: 'Bali' },
    { ko: '자카르타', en: 'Jakarta' },
  ],
  PH: [
    { ko: '마닐라', en: 'Manila' },
    { ko: '세부', en: 'Cebu' },
    { ko: '보라카이', en: 'Boracay' },
  ],
  IN: [
    { ko: '뉴델리', en: 'New Delhi' },
    { ko: '뭄바이', en: 'Mumbai' },
  ],
  AE: [
    { ko: '두바이', en: 'Dubai' },
    { ko: '아부다비', en: 'Abu Dhabi' },
  ],
  TR: [
    { ko: '이스탄불', en: 'Istanbul' },
    { ko: '안탈리아', en: 'Antalya' },
  ],
  US: [
    { ko: '뉴욕', en: 'New York' },
    { ko: 'LA', en: 'Los Angeles' },
    { ko: '라스베이거스', en: 'Las Vegas' },
    { ko: '샌프란시스코', en: 'San Francisco' },
    { ko: '시카고', en: 'Chicago' },
    { ko: '하와이', en: 'Honolulu' },
  ],
  CA: [
    { ko: '밴쿠버', en: 'Vancouver' },
    { ko: '토론토', en: 'Toronto' },
  ],
  MX: [
    { ko: '칸쿤', en: 'Cancun' },
    { ko: '멕시코시티', en: 'Mexico City' },
  ],
  GB: [
    { ko: '런던', en: 'London' },
    { ko: '에든버러', en: 'Edinburgh' },
  ],
  FR: [
    { ko: '파리', en: 'Paris' },
    { ko: '니스', en: 'Nice' },
  ],
  DE: [
    { ko: '베를린', en: 'Berlin' },
    { ko: '뮌헨', en: 'Munich' },
    { ko: '프랑크푸르트', en: 'Frankfurt' },
  ],
  IT: [
    { ko: '로마', en: 'Rome' },
    { ko: '밀라노', en: 'Milan' },
    { ko: '피렌체', en: 'Florence' },
    { ko: '베네치아', en: 'Venice' },
  ],
  ES: [
    { ko: '바르셀로나', en: 'Barcelona' },
    { ko: '마드리드', en: 'Madrid' },
  ],
  PT: [
    { ko: '리스본', en: 'Lisbon' },
    { ko: '포르투', en: 'Porto' },
  ],
  CH: [
    { ko: '취리히', en: 'Zurich' },
    { ko: '제네바', en: 'Geneva' },
    { ko: '루체른', en: 'Lucerne' },
  ],
  AT: [
    { ko: '비엔나', en: 'Vienna' },
    { ko: '잘츠부르크', en: 'Salzburg' },
  ],
  CZ: [{ ko: '프라하', en: 'Prague' }],
  HU: [{ ko: '부다페스트', en: 'Budapest' }],
  GR: [
    { ko: '아테네', en: 'Athens' },
    { ko: '산토리니', en: 'Santorini' },
  ],
  AU: [
    { ko: '시드니', en: 'Sydney' },
    { ko: '멜버른', en: 'Melbourne' },
  ],
  NZ: [
    { ko: '오클랜드', en: 'Auckland' },
    { ko: '퀸스타운', en: 'Queenstown' },
  ],
  KR: [
    { ko: '서울', en: 'Seoul' },
    { ko: '부산', en: 'Busan' },
    { ko: '제주', en: 'Jeju' },
  ],
}

// 해외 국가 목록 (KR 제외)
const overseasCountryList = computed(() =>
  countryList.filter((c) => c.code !== 'KR'),
)

const filteredCities = computed(() => {
  return cityData[selectedCountry.value] || []
})

function onCountryChange() {
  form.value.destinationCountryCode = selectedCountry.value
  form.value.destinationCity = ''
}

// 행사 유형 변경 시 반대편 필드 클리어
watch(
  () => form.value.conventionType,
  (newType, oldType) => {
    if (!oldType || newType === oldType) return
    if (newType === 'DOMESTIC') {
      // 국내 전환: 국가 강제 KR, 도시 초기화
      form.value.destinationCountryCode = 'KR'
      form.value.destinationCity = ''
      selectedCountry.value = ''
    } else if (newType === 'OVERSEAS') {
      // 해외 전환: 국가/도시 초기화 (사용자 재선택)
      form.value.destinationCountryCode = ''
      form.value.destinationCity = ''
      selectedCountry.value = ''
    }
  },
)

function addEmergencyContact() {
  emergencyContacts.value.push({ role: '', name: '', phone: '' })
}

// Image upload state
const coverImageInput = ref(null)
const coverImageFile = ref(null)
const coverImagePreview = ref(null)
const isUploadingImage = ref(false)

const dateRange = computed({
  get() {
    return {
      start: form.value.startDate ? new Date(form.value.startDate) : null,
      end: form.value.endDate ? new Date(form.value.endDate) : null,
    }
  },
  set(newRange) {
    if (!newRange) {
      form.value.startDate = null
      form.value.endDate = null
      return
    }
    form.value.startDate = newRange.start
      ? dayjs(newRange.start).format('YYYY-MM-DD')
      : null
    form.value.endDate = newRange.end
      ? dayjs(newRange.end).format('YYYY-MM-DD')
      : null
  },
})

// 기본색상 체크박스 변경 시 브랜드 컬러 자동 설정
watch(useDefaultColor, (isDefault) => {
  if (isDefault) {
    form.value.brandColor = DEFAULT_BRAND_COLOR
  }
})

// 수정 모드일 때 기존 데이터 로드
watch(
  () => props.convention,
  (newVal) => {
    if (newVal) {
      const brandColor = newVal.brandColor || '#6366f1'
      form.value = {
        title: newVal.title,
        conventionType: newVal.conventionType,
        startDate: newVal.startDate ? newVal.startDate.split('T')[0] : '',
        endDate: newVal.endDate ? newVal.endDate.split('T')[0] : '',
        brandColor: brandColor,
        conventionImg: newVal.conventionImg,
        renderType: newVal.renderType || 'STANDARD',
        themePreset: newVal.themePreset || 'default',
        location: newVal.location || '',
        destinationCity: newVal.destinationCity || '',
        // 국내는 KR 강제, 해외는 기존 값 유지
        destinationCountryCode:
          newVal.conventionType === 'DOMESTIC'
            ? 'KR'
            : newVal.destinationCountryCode || '',
      }
      selectedCountry.value =
        newVal.conventionType === 'OVERSEAS'
          ? newVal.destinationCountryCode || ''
          : ''
      // 긴급연락처 파싱
      try {
        const contacts = newVal.emergencyContactsJson
          ? JSON.parse(newVal.emergencyContactsJson)
          : []
        emergencyContacts.value = Array.isArray(contacts) ? contacts : []
      } catch {
        emergencyContacts.value = []
      }
      // 기본색상이면 체크박스 체크
      useDefaultColor.value = brandColor === DEFAULT_BRAND_COLOR
      // 이미지 미리보기 설정
      if (newVal.conventionImg) {
        coverImagePreview.value = newVal.conventionImg
      }
    }
  },
  { immediate: true },
)

function handleCoverImageChange(event) {
  const file = event.target.files?.[0]
  if (!file) return

  if (file.size > 5 * 1024 * 1024) {
    alert('파일 크기는 5MB를 초과할 수 없습니다.')
    return
  }

  coverImageFile.value = file
  const reader = new FileReader()
  reader.onload = (e) => {
    coverImagePreview.value = e.target.result
  }
  reader.readAsDataURL(file)
}

function removeCoverImage() {
  coverImageFile.value = null
  coverImagePreview.value = null
  form.value.conventionImg = null
  if (coverImageInput.value) {
    coverImageInput.value.value = ''
  }
}

async function uploadConventionCoverImage() {
  if (!coverImageFile.value) return null

  const formData = new FormData()
  formData.append('file', await compressImage(coverImageFile.value))
  isUploadingImage.value = true

  try {
    const response = await apiClient.post(
      '/admin/conventions/upload-cover-image',
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )
    return response.data.url
  } catch (error) {
    console.error('Failed to upload cover image:', error)
    alert('커버 이미지 업로드에 실패했습니다.')
    return null
  } finally {
    isUploadingImage.value = false
  }
}

const handleSubmit = async () => {
  // 국내/해외 분기 검증
  if (form.value.conventionType === 'DOMESTIC') {
    if (!form.value.location || form.value.location.trim() === '') {
      alert('국내 행사는 행사 장소를 입력해주세요.')
      return
    }
    // 국내는 KR 강제, 도시 정보 비움
    form.value.destinationCountryCode = 'KR'
    form.value.destinationCity = ''
  } else if (form.value.conventionType === 'OVERSEAS') {
    if (!form.value.destinationCountryCode) {
      alert('해외 행사는 목적지 국가를 선택해주세요.')
      return
    }
    if (form.value.destinationCountryCode === 'KR') {
      alert('해외 행사에 대한민국을 선택할 수 없습니다. 국내 행사로 변경해주세요.')
      return
    }
    if (!form.value.destinationCity) {
      alert('해외 행사는 목적지 도시를 선택해주세요.')
      return
    }
  }

  saving.value = true
  try {
    if (coverImageFile.value) {
      const imageUrl = await uploadConventionCoverImage()
      if (imageUrl) {
        form.value.conventionImg = imageUrl
      } else {
        // 업로드 실패 시 저장 중단
        saving.value = false
        return
      }
    }
    const payload = {
      ...form.value,
      emergencyContactsJson:
        emergencyContacts.value.length > 0
          ? JSON.stringify(
              emergencyContacts.value.filter((c) => c.name || c.phone),
            )
          : null,
    }
    await emit('save', payload)
  } finally {
    saving.value = false
  }
}
</script>
