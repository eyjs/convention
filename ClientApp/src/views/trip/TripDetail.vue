<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div class="absolute top-20 -right-32 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"></div>
      <div class="absolute bottom-40 -left-40 w-80 h-80 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"></div>
      <div class="absolute top-1/3 left-1/3 w-72 h-72 bg-gradient-to-br from-cyan-200/12 to-sky-200/12 rounded-full blur-3xl"></div>

      <!-- Subtle dot pattern -->
      <div class="absolute inset-0 opacity-[0.02]" style="background-image: url('data:image/svg+xml,%3Csvg width=&quot;20&quot; height=&quot;20&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot;%3E%3Ccircle cx=&quot;2&quot; cy=&quot;2&quot; r=&quot;1&quot;/%3E%3C/g%3E%3C/svg%3E');"></div>
    </div>

    <div class="relative z-10">
      <MainHeader :title="trip.title || '여행 상세'" :show-back="true">
        <template #actions>
          <button v-if="tripId" @click="openShareModal" class="p-2 text-gray-500 hover:bg-gray-100 rounded-lg">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-6 h-6">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5" />
            </svg>
          </button>
        </template>
      </MainHeader>

      <div v-if="loading" class="text-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600 font-medium">여행 정보를 불러오는 중...</p>
      </div>

      <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- Hero Section with Trip Info -->
      <section class="relative overflow-hidden bg-primary-500 rounded-2xl shadow-xl p-6 mb-6 text-white">
        <div class="relative z-10">
          <div class="flex justify-between items-start mb-4">
            <div class="flex-1">
              <h2 class="text-2xl font-bold mb-2">{{ trip.title }}</h2>
              <div class="flex items-center gap-2 text-white/90 mb-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span class="font-medium">{{ trip.startDate }} ~ {{ trip.endDate }}</span>
              </div>
              <div class="flex items-center gap-2 text-white/90">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span class="font-medium">{{ trip.destination }}</span>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <button @click="openTripInfoModal" class="px-4 py-2 bg-white/20 backdrop-blur-sm rounded-lg text-sm font-semibold hover:bg-white/30 transition-colors">
                수정
              </button>
            </div>
          </div>
          <p v-if="trip.description" class="text-white/90 text-sm leading-relaxed">{{ trip.description }}</p>
        </div>
        <!-- Decorative elements -->
        <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
        <div class="absolute bottom-0 left-0 w-24 h-24 bg-white/10 rounded-full -ml-12 -mb-12"></div>
      </section>

            <!-- Quick Actions Grid -->
            <div class="mb-6">
              <!-- 숙소 버튼 -->
              <button @click="openAccommodationManagementModal" class="w-full bg-white rounded-2xl shadow-md p-5 text-left hover:shadow-lg transition-shadow">
                <h2 class="text-lg font-bold text-gray-900 flex items-center gap-2">
                  <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                  숙소 관리
                </h2>
                <p class="text-sm text-gray-500 mt-2">{{ trip.accommodations && trip.accommodations.length > 0 ? `${trip.accommodations.length}개의 숙소 등록됨` : '등록된 숙소 없음' }}</p>
              </button>
            </div>
          </div>

          <!-- Modals -->
          <ShareTripModal
            :is-open="isShareModalOpen"
            :is-shared="trip.isShared"
            :share-url="shareableUrl"
            @close="closeShareModal"
            @update:is-shared="handleSharingToggle"
          />

          <SlideUpModal :is-open="isTripInfoModalOpen" @close="closeTripInfoModal" z-index-class="z-[60]">
            <template #header-title>{{ tripId ? '여행 정보 수정' : '여행 정보 입력' }}</template>
            <template #body>
              <form id="trip-info-form" @submit.prevent="saveTripInfo" class="space-y-4">
                <div><label class="block text-sm font-medium text-gray-700 mb-1">여행 제목</label><input v-model="tripData.title" type="text" class="w-full input" /></div>
                <div><label class="block text-sm font-medium text-gray-700 mb-1">설명</label><textarea v-model="tripData.description" rows="3" class="w-full input"></textarea></div>
                <div>
                  <DateRangePicker v-model="dateRange" label="여행 기간" />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">도시/국가</label>
                  <CountryCitySearch v-model="countryCity" />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">커버 이미지</label>
                  <div class="space-y-3">
                    <div v-if="coverImagePreview" class="relative w-full h-48 rounded-lg overflow-hidden bg-gray-100">
                      <img :src="coverImagePreview" alt="커버 이미지 미리보기" class="w-full h-full object-cover" />
                      <button type="button" @click="removeCoverImage" class="absolute top-2 right-2 p-2.5 bg-red-500 text-white rounded-full hover:bg-red-600 active:scale-95 transition-all shadow-lg">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                    <label class="block">
                      <input type="file" ref="coverImageInput" @change="handleCoverImageChange" accept="image/jpeg,image/jpg,image/png,image/gif,image/webp" class="hidden" />
                      <div class="flex items-center justify-center w-full py-4 px-4 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-400 hover:bg-primary-50 transition-colors active:scale-95">
                        <div class="text-center">
                          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                          <p class="mt-2 text-sm text-gray-600 font-medium">이미지 선택</p>
                          <p class="mt-1 text-xs text-gray-500">JPG, PNG, GIF, WebP (최대 5MB)</p>
                        </div>
                      </div>
                    </label>
                  </div>
                </div>
              </form>
            </template>
            <template #footer>
              <div class="flex gap-3 w-full">
                <button type="button" @click="closeTripInfoModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
                <button type="submit" form="trip-info-form" :disabled="isUploadingImage" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed">{{ isUploadingImage ? '업로드 중...' : '저장' }}</button>
              </div>
            </template>
          </SlideUpModal>
      

          <AccommodationManagementModal
            :is-open="isAccommodationManagementModalOpen"
            :accommodations="trip.accommodations"
            @close="closeAccommodationManagementModal"
            @add="handleAddAccommodation"
            @edit="handleEditAccommodation"
            @delete="handleDeleteAccommodation"
            @select="handleSelectAccommodation"
          />
      
          <SlideUpModal :is-open="isAccommodationEditModalOpen" @close="closeAccommodationEditModal" z-index-class="z-[60]">
            <template #header-title>{{ editingAccommodation?.id ? '숙소 수정' : '숙소 추가' }}</template>
            <template #body>
              <form id="acc-form" @submit.prevent="saveAccommodation" class="space-y-4">
                <div>
                  <label class="label">숙소명</label>
                  <template v-if="isDomestic">
                    <input type="text" :value="accommodationData.name" @focus="openKakaoMapSearchModal('accommodation')" placeholder="숙소명 검색 (카카오맵)" class="input cursor-pointer" readonly />
                  </template>
                  <GooglePlacesAutocomplete v-else v-model="accommodationData" placeholder="숙소명 검색 (구글맵)" />
                </div>
                <div><label class="label">주소</label><input v-model="accommodationData.address" type="text" class="input" readonly /></div>
                <div><label class="label">체크인</label><DateTimePicker v-model="accommodationData.checkInTime" /></div>
                <div><label class="label">체크아웃</label><DateTimePicker v-model="accommodationData.checkOutTime" /></div>
              </form>
            </template>
            <template #footer>
              <div class="flex gap-3 w-full">
                <button type="button" @click="closeAccommodationEditModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
                <button type="submit" form="acc-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all">저장</button>
              </div>
            </template>
          </SlideUpModal>
          
          <SlideUpModal :is-open="isItineraryModalOpen" @close="closeItineraryModal" z-index-class="z-[60]">
            <template #header-title>{{ editingItineraryItem?.id ? '일정 수정' : '일정 추가' }}</template>
            <template #body>
              <form id="itinerary-form" @submit.prevent="saveItineraryItem" class="space-y-4">
                <div>
                  <label class="label">장소</label>
                  <template v-if="isDomestic">
                    <input type="text" :value="itineraryItemData.locationName" @focus="openKakaoMapSearchModal('itinerary')" placeholder="장소 검색 (카카오맵)" class="input cursor-pointer" readonly />
                  </template>
                  <GooglePlacesAutocomplete v-else v-model="itineraryItemData" placeholder="장소 검색 (구글맵)" />
                </div>
                <div>
                  <label class="label">시작 시간</label>
                  <input type="time" v-model="itineraryItemData.startTime" class="input" />
                </div>
                <div>
                  <label class="label">종료 시간</label>
                  <input type="time" v-model="itineraryItemData.endTime" class="input" />
                </div>
                <div>
                  <label class="label">카테고리</label>
                  <input type="text" v-model="itineraryItemData.category" placeholder="카카오맵에서 자동 설정됨" class="input bg-gray-50 cursor-not-allowed" readonly />
                  <div class="flex flex-wrap gap-2 mt-2">
                    <button type="button" @click="itineraryItemData.category = '기타'" class="px-3 py-1 text-sm rounded-full transition-colors" :class="itineraryItemData.category === '기타' ? 'bg-primary-500 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'">
                      기타
                    </button>
                  </div>
                </div>
                <div>
                  <label class="label">전화번호</label>
                  <input type="tel" v-model="itineraryItemData.phoneNumber" placeholder="카카오맵에서 자동 설정됨" class="input bg-gray-50 cursor-not-allowed" readonly />
                </div>
                <div>
                  <label class="label">금액 (원)</label>
                  <input type="number" v-model.number="itineraryItemData.expenseAmount" placeholder="예: 50000" class="input" min="0" step="100" />
                </div>
                <div><label class="label">메모</label><textarea v-model="itineraryItemData.notes" rows="3" class="input"></textarea></div>
              </form>
            </template>
            <template #footer>
              <div class="flex gap-3 w-full">
                <button type="button" @click="closeItineraryModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
                <button type="submit" form="itinerary-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all">저장</button>
              </div>
            </template>
          </SlideUpModal>
          
          <SlideUpModal :is-open="isItineraryDetailModalOpen" @close="closeItineraryDetailModal" z-index-class="z-[60]">
            <template #header-title>일정 상세</template>
            <template #body>
              <div v-if="selectedItinerary" class="space-y-4">
                <!-- Category Badge & Phone Link -->
                <div class="flex items-center justify-between">
                  <div v-if="selectedItinerary.category">
                    <span class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary-50 text-primary-700 text-sm font-medium rounded-full">
                      <component :is="getCategoryIcon(selectedItinerary.category)" class="w-4 h-4" />
                      {{ selectedItinerary.category }}
                    </span>
                  </div>
                  <div v-else class="flex-1"></div>

                  <!-- Phone Call Link -->
                  <a v-if="selectedItinerary.phoneNumber" :href="`tel:${selectedItinerary.phoneNumber}`" class="text-primary-600 hover:text-primary-700 text-sm font-medium">
                    전화걸기
                  </a>
                </div>

                <h3 class="text-xl font-bold">{{ selectedItinerary.locationName }}</h3>

                <!-- 주소 (길찾기 버튼) -->
                <div v-if="selectedItinerary.address" class="flex items-start gap-2">
                  <svg class="w-5 h-5 text-gray-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  <p class="text-gray-600 flex-1 text-sm leading-5">{{ selectedItinerary.address }}</p>
                  <a v-if="selectedItinerary.kakaoPlaceUrl" :href="selectedItinerary.kakaoPlaceUrl" target="_blank" class="text-primary-600 hover:text-primary-700 text-sm font-medium whitespace-nowrap leading-5" title="카카오맵에서 길찾기">
                    길찾기
                  </a>
                  <a v-else-if="selectedItinerary.latitude && selectedItinerary.longitude" :href="`https://map.kakao.com/link/to/${selectedItinerary.locationName},${selectedItinerary.latitude},${selectedItinerary.longitude}`" target="_blank" class="text-primary-600 hover:text-primary-700 text-sm font-medium whitespace-nowrap leading-5" title="카카오맵에서 길찾기">
                    길찾기
                  </a>
                </div>

                <!-- 시간 -->
                <div v-if="selectedItinerary.startTime && selectedItinerary.endTime" class="text-gray-800 font-medium flex items-start gap-2">
                  <svg class="w-5 h-5 text-gray-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span>{{ selectedItinerary.startTime.substring(0, 5) }} - {{ selectedItinerary.endTime.substring(0, 5) }}</span>
                </div>

                <!-- 금액 -->
                <div v-if="selectedItinerary.expenseAmount" class="text-gray-800 font-medium flex items-start gap-2">
                  <svg class="w-5 h-5 text-gray-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <span>{{ selectedItinerary.expenseAmount.toLocaleString() }}원</span>
                </div>

                <!-- 메모 -->
                <p v-if="selectedItinerary.notes" class="whitespace-pre-wrap bg-gray-50 p-3 rounded-lg">{{ selectedItinerary.notes }}</p>

                <button
                  v-if="selectedItinerary.latitude && !showItineraryMap"
                  @click="showItineraryMap = true"
                  class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                >
                  지도 보기
                </button>

                <div v-if="showItineraryMap" class="space-y-2">
                  <div class="h-48 w-full rounded-lg">
                    <KakaoMap
                      v-if="isDomestic && selectedItinerary.latitude && selectedItinerary.longitude"
                      :latitude="selectedItinerary.latitude"
                      :longitude="selectedItinerary.longitude"
                    />
                    <GoogleMapPlaceholder v-else-if="!isDomestic && selectedItinerary.latitude" />
                  </div>
                  <button
                    @click="showItineraryMap = false"
                    class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                  >
                    지도 접기
                  </button>
                </div>
              </div>
            </template>
            <template #footer>
              <div class="flex gap-3 w-full">
                <button type="button" @click="closeItineraryDetailModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">닫기</button>
                <button type="button" @click="editSelectedItinerary" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all" :disabled="selectedItinerary?.isAutoGenerated">수정</button>
              </div>
            </template>
          </SlideUpModal>
      
          <AccommodationDetailModal
            :is-open="isAccommodationDetailModalOpen"
            :accommodation="selectedAccommodation"
            @close="closeAccommodationDetailModal"
            @edit="editSelectedAccommodation"
            :is-domestic="isDomestic"
          />
      
          <!-- Kakao Map Search Modal -->
          <KakaoMapSearchModal
            :is-open="isKakaoMapSearchModalOpen"
            @update:is-open="isKakaoMapSearchModalOpen = $event"
            @select-place="handleKakaoPlaceSelection"
            :initial-location="currentKakaoSearchInitialLocation"
            z-index-class="z-[70]"
          />
      
        </div>
    </div>

    <!-- Bottom Navigation Bar -->
    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />
  
</template>
<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUIStore } from '@/stores/ui'
import MainHeader from '@/components/common/MainHeader.vue'
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import DateTimePicker from '@/components/common/DateTimePicker.vue'
import DateRangePicker from '@/components/common/DateRangePicker.vue'
import CountryCitySearch from '@/components/common/CountryCitySearch.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import GooglePlacesAutocomplete from '@/components/common/GooglePlacesAutocomplete.vue'
import KakaoPlacesAutocomplete from '@/components/common/KakaoPlacesAutocomplete.vue'
import KakaoMapSearchModal from '@/components/common/KakaoMapSearchModal.vue'
import AccommodationDetailModal from '@/components/personalTrip/AccommodationDetailModal.vue'
import AccommodationManagementModal from '@/components/personalTrip/AccommodationManagementModal.vue'
import ShareTripModal from '@/components/personalTrip/ShareTripModal.vue'
import apiClient from '@/services/api'
import { useGoogleMaps } from '@/composables/useGoogleMaps'
import { useDistance } from '@/composables/useDistance'
import { Trash2 as Trash2Icon, Utensils, Coffee, ShoppingBag, Landmark, CircleDot, FileText, Phone } from 'lucide-vue-next'
import dayjs from 'dayjs'

const route = useRoute()
const router = useRouter()
const uiStore = useUIStore()
const tripId = computed(() => route.params.id)

const loading = ref(true)
const trip = ref({})
const selectedDay = ref(null)
const dayFilterContainer = ref(null)
const showLeftDayScroll = ref(false)
const showRightDayScroll = ref(false)

const handleDayFilterScroll = () => {
  if (dayFilterContainer.value) {
    showLeftDayScroll.value = dayFilterContainer.value.scrollLeft > 0
    showRightDayScroll.value =
      dayFilterContainer.value.scrollLeft <
      dayFilterContainer.value.scrollWidth - dayFilterContainer.value.clientWidth
  }
}

const scrollDayFilterLeft = () => {
  dayFilterContainer.value?.scrollBy({ left: -200, behavior: 'smooth' })
}

const scrollDayFilterRight = () => {
  dayFilterContainer.value?.scrollBy({ left: 200, behavior: 'smooth' })
}

const isDomestic = computed(() => trip.value.countryCode === 'KR')
const now = ref(new Date())

const categoryIconMap = {
  '음식점': Utensils,
  '카페': Coffee,
  '쇼핑': ShoppingBag,
  '관광': Landmark,
  '기타': CircleDot,
};

function getCategoryIcon(category) {
  return categoryIconMap[category] || CircleDot; // Default icon for unknown categories
}


// Badge colors for itinerary items
const badgeColors = ['#10B981', '#8B5CF6', '#EF4444', '#F59E0B', '#3B82F6', '#EC4899']

function getBadgeColor(index) {
  return badgeColors[index % badgeColors.length]
}
// Modal states
const isTripInfoModalOpen = ref(false)
const isAccommodationManagementModalOpen = ref(false)
const isAccommodationEditModalOpen = ref(false)
const isAccommodationDetailModalOpen = ref(false)
const isItineraryModalOpen = ref(false)
const isItineraryDetailModalOpen = ref(false)
const showItineraryMap = ref(false)
const isKakaoMapSearchModalOpen = ref(false)
const isShareModalOpen = ref(false)
const selectedItineraryItems = ref([]) // For bulk actions
const isBulkChangeDayModalOpen = ref(false)
const bulkChangeDayTargetDay = ref(1) // Default to day 1

// Data for modals
const tripData = ref({})
const countryCity = ref({ destination: '', countryCode: '' })
const accommodationData = ref({ name: '', address: '', postalCode: null, latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null })
const itineraryItemData = ref({ locationName: '', address: '', latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null, phoneNumber: null, kakaoPlaceUrl: null, expenseAmount: null })
const presetCategories = ['음식점', '카페', '쇼핑', '관광', '기타'];

const shareableUrl = computed(() => {
  if (trip.value.isShared && trip.value.shareToken) {
    return `${window.location.origin}/trips/share/${trip.value.shareToken}`;
  }
  return '';
});

const dateRange = computed({
  get() {
    return {
      start: tripData.value.startDate ? new Date(tripData.value.startDate) : null,
      end: tripData.value.endDate ? new Date(tripData.value.endDate) : null,
    }
  },
  set(newRange) {
    if (!newRange) {
      tripData.value.startDate = null
      tripData.value.endDate = null
      return
    }
    tripData.value.startDate = newRange.start ? dayjs(newRange.start).format('YYYY-MM-DD') : null
    tripData.value.endDate = newRange.end ? dayjs(newRange.end).format('YYYY-MM-DD') : null
  },
})

// Cover image upload
const coverImageInput = ref(null)
const coverImageFile = ref(null)
const coverImagePreview = ref(null)
const isUploadingImage = ref(false)

// Editing items
const editingAccommodation = ref(null)
const editingItineraryItem = ref(null)
const selectedItinerary = ref(null)
const selectedAccommodation = ref(null) // New ref for selected accommodation
const currentItineraryItemRef = ref(null)

// Google Maps and Places
const { isLoaded, loadScript } = useGoogleMaps()

// For Kakao Map Search Modal
const currentKakaoSearchTarget = ref(null) // 'accommodation' or 'itinerary'
const currentKakaoSearchInitialLocation = computed(() => {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    return {
      name: accommodationData.value.name,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude
    }
  } else if (currentKakaoSearchTarget.value === 'itinerary') {
    return {
      name: itineraryItemData.value.locationName,
      address: itineraryItemData.value.address,
      latitude: itineraryItemData.value.latitude,
      longitude: itineraryItemData.value.longitude
    }
  }
  return { latitude: null, longitude: null, name: '', address: '' }
})

// --- Lifecycle and Data Loading ---
onMounted(async () => {
  await loadTrip()
  loadScript()
  setInterval(() => { now.value = new Date() }, 60000) // Update time every minute for highlight
  await nextTick()
  handleDayFilterScroll()
})


async function loadTrip() {
  loading.value = true
  try {
    // 새 여행 생성 모드 (tripId가 없을 때)
    if (!tripId.value) {
      trip.value = {
        title: '새 여행',
        description: '',
        startDate: '',
        endDate: '',
        destination: '',
        countryCode: '',
        flights: [],
        accommodations: [],
        itineraryItems: []
      }
      // 새 여행 생성 시 바로 여행 정보 입력 모달 열기
      await nextTick()
      openTripInfoModal()
    } else {
      // 기존 여행 조회 모드
      const response = await apiClient.get(`/personal-trips/${tripId.value}?_=${new Date().getTime()}`)
      trip.value = response.data
    }
  } catch (error) {
    console.error('Failed to load trip:', error)
    alert('여행 정보를 불러오는 데 실패했습니다.')
    router.push('/trips')
  } finally {
    loading.value = false
  }
}

// --- Sharing ---
function openShareModal() {
  isShareModalOpen.value = true;
}
function closeShareModal() {
  isShareModalOpen.value = false;
}
async function handleSharingToggle(isShared) {
  try {
    if (isShared) {
      const response = await apiClient.post(`/personal-trips/${tripId.value}/share`);
      trip.value.shareToken = response.data.shareToken;
      trip.value.isShared = true;
    } else {
      await apiClient.delete(`/personal-trips/${tripId.value}/share`);
      trip.value.isShared = false;
    }
  } catch (error) {
    console.error('Failed to update sharing status:', error);
    alert('공유 상태 변경에 실패했습니다.');
    // Revert UI on failure
    trip.value.isShared = !isShared;
  }
}

// --- Trip Info ---
function openTripInfoModal() {
  tripData.value = { ...trip.value }
  countryCity.value = { destination: trip.value.destination, countryCode: trip.value.countryCode }
  // 기존 이미지가 있으면 미리보기 설정
  if (trip.value.coverImageUrl) {
    coverImagePreview.value = trip.value.coverImageUrl
  } else {
    coverImagePreview.value = null
  }
  coverImageFile.value = null
  isTripInfoModalOpen.value = true
}
function closeTripInfoModal() {
  isTripInfoModalOpen.value = false
  // 새 여행 생성 모드에서 모달을 닫으면 목록으로 돌아가기
  if (!tripId.value && !trip.value.id) {
    router.push('/trips')
  }
}

function handleCoverImageChange(event) {
  const file = event.target.files?.[0]
  if (!file) {
    console.log('No file selected')
    return
  }

  console.log('File selected:', file.name, file.type, file.size)

  // 파일 크기 체크 (5MB)
  if (file.size > 5 * 1024 * 1024) {
    alert('파일 크기는 5MB를 초과할 수 없습니다.')
    if (coverImageInput.value) {
      coverImageInput.value.value = ''
    }
    return
  }

  // 파일 타입 체크
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    alert('지원하지 않는 파일 형식입니다. (JPG, PNG, GIF, WebP만 가능)')
    if (coverImageInput.value) {
      coverImageInput.value.value = ''
    }
    return
  }

  coverImageFile.value = file

  // 미리보기 생성
  const reader = new FileReader()
  reader.onload = (e) => {
    coverImagePreview.value = e.target.result
    console.log('Preview generated successfully')
  }
  reader.onerror = (error) => {
    console.error('Failed to read file:', error)
    alert('이미지 파일을 읽는 데 실패했습니다.')
  }
  reader.readAsDataURL(file)
}

function removeCoverImage() {
  coverImageFile.value = null
  coverImagePreview.value = null
  tripData.value.coverImageUrl = null
  if (coverImageInput.value) {
    coverImageInput.value.value = ''
  }
}

async function uploadCoverImage() {
  if (!coverImageFile.value) {
    console.log('No cover image file to upload')
    return null
  }

  console.log('Uploading cover image:', coverImageFile.value.name)

  const formData = new FormData()
  formData.append('file', coverImageFile.value)

  try {
    isUploadingImage.value = true
    console.log('Starting upload request...')
    const response = await apiClient.post('/personal-trips/upload-cover-image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    console.log('Upload successful:', response.data)
    return response.data.url
  } catch (error) {
    console.error('Failed to upload cover image:', error)
    console.error('Error details:', error.response?.data || error.message)
    alert(`이미지 업로드에 실패했습니다.\n${error.response?.data?.message || error.message}`)
    return null
  } finally {
    isUploadingImage.value = false
  }
}

async function saveTripInfo() {
  tripData.value.destination = countryCity.value.destination
  tripData.value.countryCode = countryCity.value.countryCode

  try {
    // 새 이미지 파일이 있으면 업로드
    if (coverImageFile.value) {
      const imageUrl = await uploadCoverImage()
      if (imageUrl) {
        tripData.value.coverImageUrl = imageUrl
      }
    }

    if (!tripId.value) {
      // 새 여행 생성
      const response = await apiClient.post('/personal-trips', tripData.value)
      const newTripId = response.data.id
      // 새로 생성된 여행 페이지로 이동
      router.push(`/trips/${newTripId}`)
    } else {
      // 기존 여행 수정
      const response = await apiClient.put(`/personal-trips/${tripId.value}`, tripData.value)
      // 로컬 trip 데이터를 즉시 업데이트
      trip.value = response.data
    }
    closeTripInfoModal()
  } catch (error) {
    console.error('Failed to save trip info:', error)
    alert('저장에 실패했습니다.')
  }
}

// --- Accommodations ---
function openAccommodationManagementModal() { isAccommodationManagementModalOpen.value = true; }
function closeAccommodationManagementModal() { isAccommodationManagementModalOpen.value = false; }

function handleAddAccommodation() {
  openAccommodationEditModal();
}

function handleEditAccommodation(acc) {
  openAccommodationEditModal(acc);
}

function handleDeleteAccommodation(accId) {
  deleteAccommodationFromList(accId);
}

function handleSelectAccommodation(acc) {
  openAccommodationDetailModal(acc);
}

function openAccommodationEditModal(acc = null) { // Renamed
  editingAccommodation.value = acc
  accommodationData.value = acc ? { 
    name: acc.name, 
    address: acc.address, 
    postalCode: acc.postalCode,
    latitude: acc.latitude, 
    longitude: acc.longitude, 
    googlePlaceId: acc.googlePlaceId,
    kakaoPlaceId: acc.kakaoPlaceId,
    checkInTime: acc.checkInTime,
    checkOutTime: acc.checkOutTime
  } : { name: '', address: '', postalCode: null, latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null }
  isAccommodationEditModalOpen.value = true
}
function closeAccommodationEditModal() { isAccommodationEditModalOpen.value = false } // Renamed
function openAccommodationDetailModal(acc) { // New function
  selectedAccommodation.value = acc
  isAccommodationDetailModalOpen.value = true
}
function closeAccommodationDetailModal() { // New function
  isAccommodationDetailModalOpen.value = false
}
function editSelectedAccommodation() { // New function
  closeAccommodationDetailModal()
  openAccommodationEditModal(selectedAccommodation.value)
}
async function saveAccommodation() {
  try {
    const payload = { 
      ...accommodationData.value, 
      personalTripId: tripId.value,
      name: accommodationData.value.name,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude,
      googlePlaceId: accommodationData.value.googlePlaceId,
      kakaoPlaceId: accommodationData.value.kakaoPlaceId
    }
    if (editingAccommodation.value?.id) {
      await apiClient.put(`/personal-trips/accommodations/${editingAccommodation.value.id}`, payload)
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/accommodations`, payload)
    }
    await loadTrip()
    closeAccommodationEditModal() // Renamed
  } catch (error) {
    console.error('Failed to save accommodation:', error)
    alert('저장에 실패했습니다.')
  }
}
async function deleteAccommodation(id) { // Modified to accept id
  if (!confirm('이 숙소를 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/accommodations/${id}`)
    await loadTrip()
    closeAccommodationEditModal() // Renamed
  } catch (error) {
    console.error('Failed to delete accommodation:', error)
    alert('삭제에 실패했습니다.')
  }
}
function deleteAccommodationFromList(id) {
  deleteAccommodation(id);
}

// --- Itinerary ---
const {  calculateItemDistances, optimizeRouteByDistance, calculateTotalDistance } = useDistance()
const editModeByDay = ref({}) // 각 날짜별 편집 모드 상태
const draggedItem = ref(null)
const draggedDay = ref(null)
const touchStartY = ref(0)
const touchDraggedElement = ref(null)

const groupedItinerary = computed(() => {
  if (!trip.value.startDate || !trip.value.endDate) return []

  // 여행 기간의 총 일수 계산
  const startDate = new Date(trip.value.startDate)
  const endDate = new Date(trip.value.endDate)
  const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24)) + 1

  // 기존 itineraryItems를 dayNumber로 그룹화
  const itemsByDay = {}
  if (trip.value.itineraryItems) {
    trip.value.itineraryItems.forEach(item => {
      const day = item.dayNumber || 1
      if (!itemsByDay[day]) itemsByDay[day] = []
      itemsByDay[day].push(item)
    })
  }

  // 전체 날짜 범위에 대해 Day 생성 (빈 날짜 포함)
  const allDays = []
  for (let i = 1; i <= daysDiff; i++) {
    let items = itemsByDay[i] || []

    // Sort items within each day by orderNum
    items.sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0));

    const itemsWithDistance = calculateItemDistances(items)
    const totalDistance = calculateTotalDistance(items)

    // 날짜 계산
    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i - 1)

    allDays.push({
      dayNumber: i,
      date: currentDate,
      items: itemsWithDistance,
      totalDistance
    })
  }

  return allDays
})

const filteredItinerary = computed(() => {
  if (selectedDay.value === null) {
    return groupedItinerary.value
  }
  return groupedItinerary.value.filter(day => day.dayNumber === selectedDay.value)
})

watch(groupedItinerary, async () => {
  await nextTick()
  handleDayFilterScroll()
})

const currentItineraryItemId = computed(() => {
  const currentTime = now.value
  for (const dayGroup of groupedItinerary.value) {
    const tripDate = new Date(trip.value.startDate)
    tripDate.setDate(tripDate.getDate() + dayGroup.dayNumber - 1)
    for (const item of dayGroup.items) {
      if (item.startTime && item.endTime) {
        const startDateTime = new Date(`${tripDate.toISOString().split('T')[0]}T${item.startTime}`)
        const endDateTime = new Date(`${tripDate.toISOString().split('T')[0]}T${item.endTime}`)
        if (currentTime >= startDateTime && currentTime <= endDateTime) return item.id
      }
    }
  }
  return null
})

// Auto-scroll to current itinerary item
watch(currentItineraryItemId, async (newId) => {
  if (newId) {
    await nextTick();
    if (currentItineraryItemRef.value) {
      currentItineraryItemRef.value.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      });
    }
  }
});

function openItineraryModal(item = null, dayNumber = null) {
  if (!trip.value.startDate) {
    alert('여행 기간을 먼저 설정해주세요.');
    openTripInfoModal();
    return;
  }

  editingItineraryItem.value = item;
  if (item) {
    // Editing existing item - explicitly copy all fields
    itineraryItemData.value = {
      locationName: item.locationName || '',
      address: item.address || '',
      latitude: item.latitude || null,
      longitude: item.longitude || null,
      googlePlaceId: item.googlePlaceId || null,
      kakaoPlaceId: item.kakaoPlaceId || null,
      phoneNumber: item.phoneNumber || null,
      category: item.category || null,
      kakaoPlaceUrl: item.kakaoPlaceUrl || null,
      expenseAmount: item.expenseAmount || null,
      dayNumber: item.dayNumber,
      startTime: item.startTime || '',
      endTime: item.endTime || '',
      notes: item.notes || ''
    };
  } else {
    // Adding new item
    const targetDayNumber = dayNumber || 1;
    let defaultStartTime = '09:00';
    let defaultEndTime = '10:00';

    // Find the last item for the target day, sorted by orderNum
    const itemsForDay = trip.value.itineraryItems
      .filter(i => i.dayNumber === targetDayNumber)
      .sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0));

    if (itemsForDay.length > 0) {
      const lastItem = itemsForDay[itemsForDay.length - 1];
      if (lastItem.endTime) {
        // Set new item's start time to last item's end time
        defaultStartTime = lastItem.endTime;
        
        // Calculate default end time (lastItem.endTime + 1 hour)
        const [lastEndHour, lastEndMinute] = lastItem.endTime.split(':').map(Number);
        const nextEndTime = dayjs().hour(lastEndHour).minute(lastEndMinute).add(1, 'hour');
        defaultEndTime = nextEndTime.format('HH:mm');
      }
    }

    itineraryItemData.value = {
      locationName: '',
      address: '',
      latitude: null,
      longitude: null,
      googlePlaceId: null,
      kakaoPlaceId: null,
      notes: '',
      category: '', // Initialize category
      phoneNumber: null,
      kakaoPlaceUrl: null,
      expenseAmount: null,
      dayNumber: targetDayNumber,
      startTime: defaultStartTime,
      endTime: defaultEndTime,
    };
  }
  isItineraryModalOpen.value = true;
}
function closeItineraryModal() { isItineraryModalOpen.value = false }
async function saveItineraryItem() {
  try {
    const isNewItem = !editingItineraryItem.value?.id;
    let targetItemId = isNewItem ? null : editingItineraryItem.value.id;

    const payload = { 
      ...itineraryItemData.value, 
      personalTripId: tripId.value,
    };

    if (isNewItem) {
      const response = await apiClient.post(`/personal-trips/${tripId.value}/items`, payload);
      targetItemId = response.data.id;
    } else {
      await apiClient.put(`/personal-trips/items/${editingItineraryItem.value.id}`, payload);
    }

    await loadTrip();
    closeItineraryModal();

    if (targetItemId) {
      await nextTick();
      const itemElement = document.querySelector(`[data-item-id="${targetItemId}"]`);
      if (itemElement) {
        itemElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  } catch (error) {
    console.error('Failed to save itinerary item:', error);
    alert('저장에 실패했습니다.');
  }
}
async function deleteItineraryItem(id) { // Modified to accept id
  if (!confirm('이 일정을 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/items/${id}`)
    await loadTrip()
    closeItineraryModal()
  }  catch (error) {
    console.error('Failed to delete itinerary item:', error)
    alert('삭제에 실패했습니다.')
  }
}
function deleteItineraryItemFromList(id) {
  deleteItineraryItem(id);
}

// --- 드래그앤드롭 (데스크톱) ---
function onDragStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  draggedItem.value = item
  draggedDay.value = dayNumber

  // 드래그 중 스크롤 방지
  document.body.style.overflow = 'hidden'
  document.body.style.touchAction = 'none'

  // 드래그 이미지 설정 (선택사항)
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

function onDrag(e) {
  // 드래그 중 스크롤 방지 유지
  e.preventDefault()
}

function onDragEnd() {
  // 드래그 종료 시 스크롤 복원
  document.body.style.overflow = ''
  document.body.style.touchAction = ''
}

function onDragOver(e) {
  e.preventDefault()
  e.stopPropagation()
}

function onDrop(targetItem, targetDayNumber) {
  if (!isEditModeForDay(targetDayNumber) || !draggedItem.value) return

  // 스크롤 복원
  document.body.style.overflow = ''
  document.body.style.touchAction = ''

  const items = trip.value.itineraryItems
  const draggedIndex = items.findIndex(i => i.id === draggedItem.value.id)
  const targetIndex = items.findIndex(i => i.id === targetItem.id)

  if (draggedIndex === -1 || targetIndex === -1) return

  // 같은 날짜 내에서만 이동 허용
  if (draggedDay.value !== targetDayNumber) {
    alert('같은 날짜 내에서만 순서를 변경할 수 있습니다.')
    draggedItem.value = null
    draggedDay.value = null
    return
  }

  // 순서 변경
  const [removed] = items.splice(draggedIndex, 1)
  items.splice(targetIndex, 0, removed)

  // API 업데이트
  saveItineraryOrder(targetDayNumber)

  draggedItem.value = null
  draggedDay.value = null
}

// --- 터치 드래그앤드롭 (모바일) ---
function onTouchStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  draggedItem.value = item
  draggedDay.value = dayNumber
  touchStartY.value = event.touches[0].clientY
  touchDraggedElement.value = event.currentTarget

  // 드래그 시작 시각 피드백
  event.currentTarget.style.opacity = '0.5'
  event.currentTarget.style.transform = 'scale(1.05)'
}

function onTouchMove(event) {
  if (!draggedItem.value) return

  event.preventDefault()

  const touch = event.touches[0]
  const currentY = touch.clientY

  // 터치 위치에서 요소 찾기
  const elements = document.elementsFromPoint(touch.clientX, currentY)
  const targetCard = elements.find(el => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value)

  if (targetCard) {
    // 호버 효과
    targetCard.style.borderColor = 'rgba(23, 177, 133, 1)'
  }
}

function onTouchEnd(targetItem, targetDayNumber, event) {
  if (!draggedItem.value) return

  // 시각 피드백 복원
  if (touchDraggedElement.value) {
    touchDraggedElement.value.style.opacity = ''
    touchDraggedElement.value.style.transform = ''
  }

  const touch = event.changedTouches[0]
  const elements = document.elementsFromPoint(touch.clientX, touch.clientY)
  const targetCard = elements.find(el => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value)

  if (targetCard) {
    const targetId = parseInt(targetCard.getAttribute('data-item-id'))
    const target = trip.value.itineraryItems.find(i => i.id === targetId)

    if (target) {
      // 같은 날짜 내에서만 이동 허용
      if (draggedDay.value !== target.dayNumber) {
        alert('같은 날짜 내에서만 순서를 변경할 수 있습니다.')
        draggedItem.value = null
        draggedDay.value = null
        touchDraggedElement.value = null
        return
      }

      const items = trip.value.itineraryItems
      const draggedIndex = items.findIndex(i => i.id === draggedItem.value.id)
      const targetIndex = items.findIndex(i => i.id === targetId)

      if (draggedIndex !== -1 && targetIndex !== -1 && draggedIndex !== targetIndex) {
        // 순서 변경
        const [removed] = items.splice(draggedIndex, 1)
        items.splice(targetIndex, 0, removed)

        // API 업데이트
        saveItineraryOrder(target.dayNumber)
      }
    }
  }

  draggedItem.value = null
  draggedDay.value = null
  touchDraggedElement.value = null
}

async function saveItineraryOrder(dayNumber) {
  const dayItems = trip.value.itineraryItems
    .filter(item => item.dayNumber === dayNumber)
    .map((item, index) => ({
      id: item.id,
      orderNum: index
    }))

  try {
    await apiClient.put(`/personal-trips/${tripId.value}/items/reorder`, { items: dayItems })
    // 로컬 상태만 업데이트 (페이지 새로고침 방지)
    dayItems.forEach(({ id, orderNum }) => {
      const item = trip.value.itineraryItems.find(i => i.id === id)
      if (item) {
        item.orderNum = orderNum
      }
    })
  } catch (error) {
    console.error('Failed to save order:', error)
    alert('순서 저장에 실패했습니다.')
    // 실패 시에만 전체 데이터 다시 로드
    await loadTrip()
  }
}

// --- 경로 최적화 ---
async function optimizeRoute(dayNumber) {
  const dayGroup = groupedItinerary.value.find(g => g.dayNumber === dayNumber);
  if (!dayGroup) return;

  const dayItems = dayGroup.items;
  if (dayItems.length < 2) {
    alert('경로를 최적화하려면 2개 이상의 일정이 필요합니다.');
    return;
  }

  // Pass only the dayItems to the optimization function
  const optimized = optimizeRouteByDistance(dayItems);

  if (optimized.length === dayItems.length) {
    const updatePayload = optimized.map((item, index) => ({
      id: item.id,
      orderNum: index
    }));

    try {
      await apiClient.put(`/personal-trips/${tripId.value}/items/reorder`, { items: updatePayload });
      await loadTrip();
      alert('경로가 최적화되었습니다!');
    } catch (error) {
      console.error('Failed to optimize route:', error);
      alert('경로 최적화에 실패했습니다.');
    }
  }
}

function toggleEditMode(dayNumber) {
  editModeByDay.value[dayNumber] = !editModeByDay.value[dayNumber]

  // 편집 모드 종료 시 선택 항목 초기화
  if (!editModeByDay.value[dayNumber]) {
    const dayItems = trip.value.itineraryItems?.filter(item => item.dayNumber === dayNumber) || []
    selectedItineraryItems.value = selectedItineraryItems.value.filter(id =>
      !dayItems.some(item => item.id === id)
    )
  }
}

function isEditModeForDay(dayNumber) {
  return editModeByDay.value[dayNumber] || false
}

// 개별 아이템 선택/해제
function toggleItemSelection(id) {
  const index = selectedItineraryItems.value.indexOf(id)
  if (index > -1) {
    selectedItineraryItems.value.splice(index, 1)
  } else {
    selectedItineraryItems.value.push(id)
  }
}

// 특정 day의 선택된 아이템 개수
function getSelectedItemsForDay(dayNumber) {
  const dayItems = trip.value.itineraryItems?.filter(item => item.dayNumber === dayNumber) || []
  return selectedItineraryItems.value.filter(id =>
    dayItems.some(item => item.id === id)
  )
}

// --- Itinerary Detail Modal ---
function openItineraryDetailModal(item) {
  selectedItinerary.value = item
  showItineraryMap.value = false
  isItineraryDetailModalOpen.value = true
}
function closeItineraryDetailModal() {
  isItineraryDetailModalOpen.value = false
  showItineraryMap.value = false
}
function editSelectedItinerary() {
  closeItineraryDetailModal()
  openItineraryModal(selectedItinerary.value)
}

// --- Date Formatting ---
function formatDateWithDay(date) {
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  return `${dayjs(date).format('M/D')}(${weekdays[dayjs(date).day()]})`
}

// --- Copy to Clipboard ---
async function copyToClipboard(text, label = '내용') {
  try {
    await navigator.clipboard.writeText(text)
    alert(`${label}가 복사되었습니다.`)
  } catch (error) {
    console.error('Failed to copy to clipboard:', error)
    alert('복사에 실패했습니다.')
  }
}

// --- Bulk Actions ---
function isAllSelectedForDay(dayNumber) {
  const dayItems = trip.value.itineraryItems?.filter(item => item.dayNumber === dayNumber && !item.isAutoGenerated) || []
  if (dayItems.length === 0) return false
  return dayItems.every(item => selectedItineraryItems.value.includes(item.id))
}

function toggleSelectAllForDay(dayNumber) {
  const dayItems = trip.value.itineraryItems?.filter(item => item.dayNumber === dayNumber && !item.isAutoGenerated) || []
  const allSelected = isAllSelectedForDay(dayNumber)

  if (allSelected) {
    selectedItineraryItems.value = selectedItineraryItems.value.filter(id =>
      !dayItems.some(item => item.id === id)
    )
  } else {
    const dayItemIds = dayItems.map(item => item.id)
    selectedItineraryItems.value = [...new Set([...selectedItineraryItems.value, ...dayItemIds])]
  }
}

async function bulkDeleteSelectedItems(dayNumber) {
  const itemsToDelete = getSelectedItemsForDay(dayNumber)
  if (itemsToDelete.length === 0) return
  if (!confirm(`${itemsToDelete.length}개의 일정을 삭제하시겠습니까?`)) return

  try {
    await Promise.all(
      itemsToDelete.map(id =>
        apiClient.delete(`/personal-trips/items/${id}`)
      )
    )
    // 삭제된 항목을 선택 목록에서 제거
    selectedItineraryItems.value = selectedItineraryItems.value.filter(id =>
      !itemsToDelete.includes(id)
    )
    await loadTrip()
  } catch (error) {
    console.error('Failed to delete items:', error)
    alert('일정 삭제에 실패했습니다.')
  }
}

function openBulkChangeDayModal() {
  if (selectedItineraryItems.value.length === 0) return
  isBulkChangeDayModalOpen.value = true
}

function closeBulkChangeDayModal() {
  isBulkChangeDayModalOpen.value = false
}

async function saveBulkChangeDay() {
  if (selectedItineraryItems.value.length === 0) return

  try {
    await Promise.all(
      selectedItineraryItems.value.map(id =>
        apiClient.put(`/personal-trips/items/${id}`, {
          dayNumber: bulkChangeDayTargetDay.value
        })
      )
    )
    selectedItineraryItems.value = []
    await loadTrip()
    closeBulkChangeDayModal()
  } catch (error) {
    console.error('Failed to change day:', error)
    alert('날짜 변경에 실패했습니다.')
  }
}
// --- Kakao Map Search Modal ---
function openKakaoMapSearchModal(target) {
  currentKakaoSearchTarget.value = target
  isKakaoMapSearchModalOpen.value = true
}

function handleKakaoPlaceSelection(place) {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    accommodationData.value.name = place.name
    accommodationData.value.address = place.address
    accommodationData.value.postalCode = place.postalCode // Add postalCode
    accommodationData.value.latitude = place.latitude
    accommodationData.value.longitude = place.longitude
    accommodationData.value.kakaoPlaceId = place.kakaoPlaceId
  } else if (currentKakaoSearchTarget.value === 'itinerary') {
    itineraryItemData.value.locationName = place.name
    itineraryItemData.value.address = place.address
    itineraryItemData.value.latitude = place.latitude
    itineraryItemData.value.longitude = place.longitude
    itineraryItemData.value.kakaoPlaceId = place.kakaoPlaceId
    itineraryItemData.value.phoneNumber = place.phoneNumber || null
    itineraryItemData.value.category = place.category || null
    itineraryItemData.value.kakaoPlaceUrl = place.kakaoPlaceUrl || null
  }
  isKakaoMapSearchModalOpen.value = false
}
</script>