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
      <MainHeader :title="'일정표'" :show-back="true" />

      <div v-if="loading" class="text-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600 font-medium">일정을 불러오는 중...</p>
      </div>

      <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
        <!-- 일정 -->
        <section class="bg-white rounded-2xl shadow-md p-5">
          <!-- Day Filter Tabs with Scroll Arrows -->
          <div v-if="groupedItinerary.length > 0" class="relative">
            <div
              ref="dayFilterContainer"
              class="flex gap-2 overflow-x-auto pb-2 mb-4 no-scrollbar"
              @scroll="handleDayFilterScroll"
            >
              <button
                @click="selectedDay = null"
                :class="['flex items-center justify-center px-5 py-2.5 rounded-xl font-semibold text-sm whitespace-nowrap transition-all', selectedDay === null ? 'text-white shadow-md' : 'bg-white shadow-md text-gray-600 hover:shadow-lg hover:text-gray-800']"
                :style="selectedDay === null ? 'background-color: rgba(23, 177, 133, 1);' : ''"
              >
                전체
              </button>
              <button
                v-for="day in groupedItinerary"
                :key="day.dayNumber"
                @click="selectedDay = day.dayNumber"
                :class="['flex items-center justify-center px-5 py-2.5 rounded-xl font-semibold text-sm whitespace-nowrap transition-all', selectedDay === day.dayNumber ? 'text-white shadow-md' : 'bg-white shadow-md text-gray-600 hover:shadow-lg hover:text-gray-800']"
                :style="selectedDay === day.dayNumber ? 'background-color: rgba(23, 177, 133, 1);' : ''"
              >
                Day {{ day.dayNumber }}
              </button>
            </div>

            <!-- Left Scroll Arrow -->
            <div
              v-if="showLeftDayScroll"
              class="absolute left-0 top-0 bottom-2 flex items-center bg-gradient-to-r from-white to-transparent pr-4 pointer-events-none"
            >
              <button
                @click="scrollDayFilterLeft"
                class="p-1.5 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
              >
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
            </div>

            <!-- Right Scroll Arrow -->
            <div
              v-if="showRightDayScroll"
              class="absolute right-0 top-0 bottom-2 flex items-center bg-gradient-to-l from-white to-transparent pl-4 pointer-events-none"
            >
              <button
                @click="scrollDayFilterRight"
                class="p-1.5 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
              >
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          </div>

          <div v-if="groupedItinerary.length === 0" class="text-center py-12">
            <div class="w-20 h-20 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center">
              <svg class="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <p class="text-gray-500 font-medium mb-4">등록된 일정이 없습니다</p>
            <button v-if="!effectiveReadonly" @click="openItineraryModal()" class="inline-flex items-center gap-2 px-5 py-2.5 bg-primary-500 text-white rounded-full font-semibold hover:shadow-lg transition-shadow">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              첫 일정 추가
            </button>
          </div>

          <div v-else class="space-y-6">
            <div v-for="dayGroup in filteredItinerary" :key="dayGroup.dayNumber">
              <!-- Day Header -->
              <div class="mb-4">
                <div class="flex items-center justify-between mb-2">
                  <h3 class="text-lg font-bold text-gray-900">
                    Day {{ dayGroup.dayNumber }} <span class="text-base font-medium text-gray-500">{{ formatDateWithDay(dayGroup.date) }}</span>
                  </h3>
                  <div v-if="!effectiveReadonly" class="flex items-center gap-3">
                    <!-- 편집 모드일 때: 거리순 재정렬만 표시 -->
                    <button v-if="isEditModeForDay(dayGroup.dayNumber)" @click="optimizeRoute(dayGroup.dayNumber)" class="text-sm font-medium text-primary-600 hover:text-primary-700 transition-colors flex items-center gap-1">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4" />
                      </svg>
                      거리순 재정렬
                    </button>
                    <!-- 편집 버튼 -->
                    <button @click="toggleEditMode(dayGroup.dayNumber)" class="text-sm font-medium transition-colors" :class="isEditModeForDay(dayGroup.dayNumber) ? 'text-primary-600 hover:text-primary-700' : 'text-gray-600 hover:text-gray-700'">
                      {{ isEditModeForDay(dayGroup.dayNumber) ? '완료' : '편집' }}
                    </button>
                  </div>
                </div>
                <div class="flex items-center gap-3">
                  <span class="text-sm text-gray-500">
                    {{ dayGroup.items.filter(i => i.type === 'itinerary').length }}개 일정
                    <span v-if="dayGroup.items.filter(i => i.type === 'transportation').length > 0" class="text-gray-400">
                      · {{ dayGroup.items.filter(i => i.type === 'transportation').length }}개 교통편
                    </span>
                  </span>
                </div>
              </div>

              <!-- Itinerary Items with Original Timeline + Improved Content -->
              <div>
                <div v-for="(item, index) in dayGroup.items" :key="item.id" class="flex gap-3">
                  <!-- Checkbox (편집 모드일 때만, 일반 일정만) -->
                  <div v-if="isEditModeForDay(dayGroup.dayNumber) && item.type === 'itinerary'" class="flex-shrink-0 pt-9">
                    <div
                      @click.stop="toggleItemSelection(item.id)"
                      class="flex-shrink-0 w-5 h-5 rounded-full transition-all flex items-center justify-center"
                      :class="selectedItineraryItems.includes(item.id) ? '' : 'border-2 border-dashed border-gray-300'"
                    >
                      <Check v-if="selectedItineraryItems.includes(item.id)" class="w-5 h-5 text-primary-500" :stroke-width="3" />
                    </div>
                  </div>

                  <!-- 1. Timeline Column (편집 모드가 아닐 때만 표시) -->
                  <div v-if="!isEditModeForDay(dayGroup.dayNumber)" class="relative flex-shrink-0 w-5 flex flex-col items-center">
                    <!-- Top line (hidden for first item) -->
                    <div
                      v-if="index > 0"
                      class="absolute top-0 left-1/2 -translate-x-1/2 h-9"
                      style="width: 0px; border-right: 1px dashed rgba(23, 177, 133, 0.5);"
                    ></div>

                    <!-- Bullet -->
                    <div
                      class="relative z-10 w-3 h-3 mt-9 rounded-full flex-shrink-0"
                      style="background-color: rgba(23, 177, 133, 1);"
                    ></div>

                    <!-- Bottom line & Distance Badge Container -->
                    <div
                      v-if="index < dayGroup.items.length - 1"
                      class="absolute top-9 bottom-0 left-1/2 -translate-x-1/2"
                      style="width: 0px;"
                    >
                      <!-- The actual line -->
                      <div class="absolute inset-0" style="border-right: 1px dashed rgba(23, 177, 133, 0.5);"></div>

                      <!-- Distance Badge (일반 일정만) -->
                      <div
                        v-if="item.type === 'itinerary' && item.distanceToNext"
                        class="absolute z-20 bottom-0 left-1/2 -translate-x-1/2 px-2 py-0.5 rounded-full text-xs font-medium whitespace-nowrap"
                        style="background-color: rgba(23, 177, 133, 0.1); color: rgba(23, 177, 133, 1);"
                      >
                        {{ item.distanceToNext.formatted }}
                      </div>
                    </div>
                  </div>

                  <!-- 2. Content Column (Improved) -->
                  <div class="flex-1 pb-6">
                    <div
                      :ref="(el) => { if (item.id === currentItineraryItemId) currentItineraryItemRef = el }"
                      :data-item-id="item.id"
                      class="group relative bg-white rounded-xl p-4 shadow-md hover:shadow-lg transition-all"
                      :class="{
                        'cursor-pointer': !isEditModeForDay(dayGroup.dayNumber) || item.type === 'transportation',
                      }"
                      :style="currentItineraryItemId === item.id ? 'border: 2px solid rgba(23, 177, 133, 1); box-shadow: 0 4px 6px -1px rgba(23, 177, 133, 0.1);' : ''"
                      @click="item.type === 'transportation' ? router.push(`/trips/${tripId}/transportation`) : (!isEditModeForDay(dayGroup.dayNumber) && openItineraryDetailModal(item))"
                    >
                      <div class="flex items-start justify-between gap-3">
                        <!-- Main content -->
                        <div class="flex-1 min-w-0">
                          <!-- Category Badge -->
                          <div v-if="item.category || item.type === 'transportation'" class="mb-2 flex items-center gap-2">
                            <span v-if="item.type === 'transportation'" class="inline-flex items-center gap-1 px-2 py-1 bg-blue-50 text-blue-700 text-xs font-medium rounded-full">
                              <Plane v-if="item.category === '항공편'" class="w-3 h-3" />
                              <Train v-else-if="item.category === '기차'" class="w-3 h-3" />
                              <Bus v-else-if="item.category === '버스'" class="w-3 h-3" />
                              <Car v-else class="w-3 h-3" />
                              {{ item.category }}
                            </span>
                            <span v-else class="inline-flex items-center gap-1 px-2 py-1 bg-primary-50 text-primary-700 text-xs font-medium rounded-full">
                              <component :is="getCategoryIcon(item.category)" class="w-3 h-3" />
                              {{ item.category }}
                            </span>
                          </div>

                          <!-- Place name -->
                          <h3 class="font-bold text-gray-900 text-base mb-1">{{ item.locationName }}</h3>

                          <!-- Address (일반 일정만) -->
                          <p v-if="item.address && item.type === 'itinerary'" class="text-xs text-gray-500 mb-1">{{ item.address }}</p>

                          <!-- 교통편 시간 (출발/도착 구분) -->
                          <div v-if="item.type === 'transportation' && item.startTime" class="flex items-center gap-3 text-sm text-gray-600 mb-1">
                            <div class="flex items-center gap-1">
                              <span class="text-xs text-gray-500">출발</span>
                              <span class="font-bold text-gray-900">{{ item.startTime.substring(0, 5) }}</span>
                            </div>
                            <template v-if="item.endTime">
                              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                              </svg>
                              <div class="flex items-center gap-1">
                                <span class="text-xs text-gray-500">도착</span>
                                <span class="font-bold text-gray-900">{{ item.endTime.substring(0, 5) }}</span>
                              </div>
                            </template>
                          </div>

                          <!-- 일반 일정 시간 -->
                          <div v-else-if="item.type === 'itinerary' && item.startTime && item.endTime" class="flex items-center gap-1 text-sm text-gray-600 mb-1">
                            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <span class="font-medium">
                              {{ item.startTime.substring(0, 5) }} - {{ item.endTime.substring(0, 5) }}
                            </span>
                          </div>

                          <!-- Notes/Details -->
                          <p v-if="item.notes && item.notes.trim() !== ''" class="text-sm text-gray-600 leading-relaxed mt-2">
                            {{ item.notes }}
                          </p>
                        </div>

                        <!-- Icons (Right-most) -->
                        <div class="flex-shrink-0 ml-auto flex flex-col items-end gap-1">
                            <div class="flex items-center gap-2">
                                <!-- 금액 아이콘 (일반 일정만) -->
                                <svg v-if="item.type === 'itinerary' && item.expenseAmount && item.expenseAmount > 0" class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                <Phone v-if="item.phoneNumber" class="w-5 h-5 text-gray-500" />
                            </div>
                            <!-- Drag Handle (only in edit mode and itinerary type) -->
                            <div v-if="isEditModeForDay(dayGroup.dayNumber) && item.type === 'itinerary'"
                              :draggable="true"
                              @dragstart.stop="onDragStart(item, dayGroup.dayNumber, $event)"
                              @drag.stop="onDrag"
                              @dragend.stop="onDragEnd"
                              @dragover.stop="onDragOver"
                              @drop.stop="onDrop(item, dayGroup.dayNumber)"
                              @touchstart.stop="onTouchStart(item, dayGroup.dayNumber, $event)"
                              class="handle text-gray-400 cursor-grab active:cursor-grabbing p-2"
                            >
                              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h16M4 16h16" />
                              </svg>
                            </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 편집 모드 하단 버튼들 -->
              <div v-if="isEditModeForDay(dayGroup.dayNumber)" class="space-y-3 mb-3">
                <!-- day 전체 선택 버튼 -->
                <button
                  @click="toggleSelectAllForDay(dayGroup.dayNumber)"
                  class="w-full py-3 border-2 rounded-xl font-semibold transition-all"
                  :class="isAllSelectedForDay(dayGroup.dayNumber) ? 'border-primary-500 bg-primary-50 text-primary-700' : 'border-gray-300 bg-white text-gray-700 hover:border-primary-400'"
                >
                  {{ isAllSelectedForDay(dayGroup.dayNumber) ? '전체 선택 해제' : 'day 전체 선택' }}
                </button>

                <!-- 선택 항목 삭제 버튼 (선택된 항목이 있을 때만) -->
                <button
                  v-if="getSelectedItemsForDay(dayGroup.dayNumber).length > 0"
                  @click="bulkDeleteSelectedItems(dayGroup.dayNumber)"
                  class="w-full py-3 border-2 border-red-500 bg-red-50 text-red-700 rounded-xl font-semibold hover:bg-red-100 transition-all"
                >
                  선택 항목 삭제 ({{ getSelectedItemsForDay(dayGroup.dayNumber).length }}개)
                </button>
              </div>

              <!-- Add Itinerary Button (편집 모드가 아닐 때만, readonly 모드에서는 숨김) -->
              <button v-if="!effectiveReadonly && !isEditModeForDay(dayGroup.dayNumber)" @click="openItineraryModal(null, dayGroup.dayNumber)" class="w-full py-3 border-2 border-dashed rounded-xl font-medium hover:opacity-80 transition-all" style="border-color: rgba(23, 177, 133, 0.3); color: rgba(23, 177, 133, 1); background-color: rgba(23, 177, 133, 0.05);">
                + 일정 추가
              </button>
            </div>
          </div>
        </section>
      </div>

      <!-- Modals -->
      <SlideUpModal
        :is-open="isItineraryModalOpen"
        @close="closeItineraryModal"
        z-index-class="z-[60]"
        :disable-history-management="true"
      >
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
              <input v-model.number="itineraryItemData.expenseAmount" v-number-format type="text" inputmode="numeric" placeholder="예: 50000" class="input" />
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

      <SlideUpModal
        :is-open="isItineraryDetailModalOpen"
        @close="closeItineraryDetailModal"
        z-index-class="z-[60]"
        :disable-history-management="true"
      >
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
              <a v-if="selectedItinerary.latitude && selectedItinerary.longitude" :href="`https://map.kakao.com/link/to/${selectedItinerary.locationName},${selectedItinerary.latitude},${selectedItinerary.longitude}`" target="_blank" class="text-primary-600 hover:text-primary-700 text-sm font-medium whitespace-nowrap leading-5" title="카카오맵에서 길찾기">
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
              class="w-full py-2.5 bg-primary-500 text-white rounded-lg font-semibold hover:bg-primary-600 transition-colors"
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
            <button v-if="!effectiveReadonly" type="button" @click="editSelectedItinerary" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all" :disabled="selectedItinerary?.isAutoGenerated">수정</button>
          </div>
        </template>
      </SlideUpModal>

      <!-- Kakao Map Search Modal -->
      <KakaoMapSearchModal
        :is-open="isKakaoMapSearchModalOpen"
        @update:is-open="isKakaoMapSearchModalOpen = $event"
        @select-place="handleKakaoPlaceSelection"
        :initial-location="currentKakaoSearchInitialLocation"
        z-index-class="z-[70]"
      />

    </div>

    <!-- Bottom Navigation Bar -->
    <BottomNavigationBar
      v-if="((tripId && tripId !== 'undefined') || (shareToken && shareToken !== 'undefined')) && !uiStore.isModalOpen"
      :trip-id="tripId || trip.id"
      :share-token="shareToken"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUIStore } from '@/stores/ui'
import MainHeader from '@/components/common/MainHeader.vue'
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import GooglePlacesAutocomplete from '@/components/common/GooglePlacesAutocomplete.vue'
import KakaoMapSearchModal from '@/components/common/KakaoMapSearchModal.vue'
import apiClient from '@/services/api'
import { useGoogleMaps } from '@/composables/useGoogleMaps'
import { useDistance } from '@/composables/useDistance'
import { Utensils, Coffee, ShoppingBag, Landmark, CircleDot, Phone, Check, Plane, Train, Bus, Car } from 'lucide-vue-next'
import dayjs from 'dayjs'

// Props for readonly mode and shared access
const props = defineProps({
  id: String,               // 라우터에서 자동 주입 (params.id)
  shareToken: String,       // 공유 접근용 토큰
  readonly: {               // Readonly 모드 플래그
    type: Boolean,
    default: false
  }
})

const router = useRouter()
const uiStore = useUIStore()

// Determine tripId and readonly mode
const tripId = computed(() => props.id || null)
const shareToken = computed(() => props.shareToken || null)
const isSharedView = computed(() => !!shareToken.value)
const effectiveReadonly = computed(() => props.readonly || isSharedView.value)

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
  return categoryIconMap[category] || CircleDot;
}

// Modal states
const isItineraryModalOpen = ref(false)
const isItineraryDetailModalOpen = ref(false)
const showItineraryMap = ref(false)
const isKakaoMapSearchModalOpen = ref(false)
const selectedItineraryItems = ref([])

// Data for modals
const itineraryItemData = ref({ locationName: '', address: '', latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null, phoneNumber: null, kakaoPlaceUrl: null, expenseAmount: null })

// Editing items
const editingItineraryItem = ref(null)
const selectedItinerary = ref(null)
const currentItineraryItemRef = ref(null)

// Google Maps and Places
const { isLoaded, loadScript } = useGoogleMaps()

// For Kakao Map Search Modal
const currentKakaoSearchTarget = ref(null)
const currentKakaoSearchInitialLocation = computed(() => {
  if (currentKakaoSearchTarget.value === 'itinerary') {
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
  setInterval(() => { now.value = new Date() }, 60000)
  await nextTick()
  handleDayFilterScroll()
})

// Watch for route changes (when navigating between different trips)
watch(() => props.id, async (newId, oldId) => {
  if (newId && newId !== oldId) {
    await loadTrip()
  }
})

async function loadTrip() {
  loading.value = true
  try {
    // 공유 링크로 접근하는 경우
    if (shareToken.value) {
      const response = await apiClient.get(`/personal-trips/public/${shareToken.value}`)
      trip.value = response.data
    }
    // 일반 접근 (인증 필요)
    else {
      const response = await apiClient.get(`/personal-trips/${tripId.value}?_=${new Date().getTime()}`)
      trip.value = response.data
    }
  } catch (error) {
    console.error('Failed to load trip:', error)
    alert('여행 정보를 불러오는 데 실패했습니다.')
    if (isSharedView.value) {
      router.push('/home')
    } else {
      router.push('/trips')
    }
  } finally {
    loading.value = false
  }
}

// --- Itinerary ---
const { calculateItemDistances, optimizeRouteByDistance, calculateTotalDistance } = useDistance()
const editModeByDay = ref({})
const draggedItem = ref(null)
const draggedDay = ref(null)
const touchStartY = ref(0)
const touchDraggedElement = ref(null)
const ghostElement = ref(null)
const touchOffsetX = ref(0)
const touchOffsetY = ref(0)

const groupedItinerary = computed(() => {
  if (!trip.value.startDate || !trip.value.endDate) return []

  const startDate = new Date(trip.value.startDate)
  const endDate = new Date(trip.value.endDate)
  const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24)) + 1

  const itemsByDay = {}

  // 기존 일정 아이템 추가
  if (trip.value.itineraryItems) {
    trip.value.itineraryItems.forEach(item => {
      const day = item.dayNumber || 1
      if (!itemsByDay[day]) itemsByDay[day] = []
      itemsByDay[day].push({
        ...item,
        type: 'itinerary'
      })
    })
  }

  // 교통편 추가 (날짜 기준으로 자동 매칭)
  if (trip.value.flights) {
    trip.value.flights.forEach(flight => {
      if (flight.departureTime) {
        const flightDate = new Date(flight.departureTime)

        // 시간 부분을 제거하고 날짜만 비교
        const flightDateOnly = new Date(flightDate.getFullYear(), flightDate.getMonth(), flightDate.getDate())
        const startDateOnly = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate())

        // 날짜 차이 계산 (0부터 시작하므로 +1)
        const dayNumber = Math.floor((flightDateOnly - startDateOnly) / (1000 * 60 * 60 * 24)) + 1

        // 여행 기간 내의 교통편만 표시
        if (dayNumber >= 1 && dayNumber <= daysDiff) {
          if (!itemsByDay[dayNumber]) itemsByDay[dayNumber] = []

          // 교통편을 일정 아이템 형태로 변환
          itemsByDay[dayNumber].push({
            id: `flight-${flight.id}`,
            type: 'transportation',
            category: flight.category,
            transportationId: flight.id,
            locationName: `${flight.departureLocation || ''} → ${flight.arrivalLocation || ''}`,
            startTime: flight.departureTime ? dayjs(flight.departureTime).format('HH:mm') : null,
            endTime: flight.arrivalTime ? dayjs(flight.arrivalTime).format('HH:mm') : null,
            departureLocation: flight.departureLocation,
            arrivalLocation: flight.arrivalLocation,
            departureTime: flight.departureTime,
            arrivalTime: flight.arrivalTime,
            amount: flight.amount,
            airline: flight.airline,
            flightNumber: flight.flightNumber,
            bookingReference: flight.bookingReference,
            notes: flight.notes,
            isAutoGenerated: true, // 자동 생성된 카드
            orderNum: -1 // 시간순 정렬을 위해 임시값
          })
        }
      }
    })
  }

  const allDays = []
  for (let i = 1; i <= daysDiff; i++) {
    let items = itemsByDay[i] || []

    // 시간순으로 정렬 (startTime 기준)
    items.sort((a, b) => {
      const timeA = a.startTime || '00:00'
      const timeB = b.startTime || '00:00'
      return timeA.localeCompare(timeB)
    });

    // 일반 일정만 필터링해서 거리 계산
    const itineraryOnly = items.filter(item => item.type === 'itinerary')
    const itemsWithDistance = calculateItemDistances(itineraryOnly)
    const totalDistance = calculateTotalDistance(itineraryOnly)

    // 거리 정보를 원본 items 배열에 병합
    const distanceMap = new Map()
    itemsWithDistance.forEach(item => {
      distanceMap.set(item.id, item.distanceToNext)
    })

    // 전체 items에 거리 정보 추가 (일반 일정만)
    items.forEach(item => {
      if (item.type === 'itinerary' && distanceMap.has(item.id)) {
        item.distanceToNext = distanceMap.get(item.id)
      }
    })

    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i - 1)

    allDays.push({
      dayNumber: i,
      date: currentDate,
      items: items, // 교통편 포함된 전체 아이템 (거리 정보 포함)
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
    return;
  }

  editingItineraryItem.value = item;
  if (item) {
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
    const targetDayNumber = dayNumber || 1;
    let defaultStartTime = '09:00';
    let defaultEndTime = '10:00';

    const itemsForDay = trip.value.itineraryItems
      .filter(i => i.dayNumber === targetDayNumber)
      .sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0));

    if (itemsForDay.length > 0) {
      const lastItem = itemsForDay[itemsForDay.length - 1];
      if (lastItem.endTime) {
        defaultStartTime = lastItem.endTime;

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
      category: '',
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

function closeItineraryModal() {
  isItineraryModalOpen.value = false
}

async function saveItineraryItem() {
  try {
    const isNewItem = !editingItineraryItem.value?.id;
    let targetItemId = isNewItem ? null : editingItineraryItem.value.id;

    // 금액이 빈값이면 0으로 설정
    if (itineraryItemData.value.expenseAmount === null || itineraryItemData.value.expenseAmount === '' || itineraryItemData.value.expenseAmount === undefined) {
      itineraryItemData.value.expenseAmount = 0;
    }

    // 페이로드 생성 및 빈 문자열을 null로 변환
    const payload = {
      personalTripId: tripId.value,
      dayNumber: itineraryItemData.value.dayNumber,
      locationName: itineraryItemData.value.locationName || null,
      address: itineraryItemData.value.address || null,
      latitude: itineraryItemData.value.latitude || null,
      longitude: itineraryItemData.value.longitude || null,
      googlePlaceId: itineraryItemData.value.googlePlaceId || null,
      kakaoPlaceId: itineraryItemData.value.kakaoPlaceId || null,
      phoneNumber: itineraryItemData.value.phoneNumber || null,
      category: itineraryItemData.value.category || null,
      kakaoPlaceUrl: itineraryItemData.value.kakaoPlaceUrl || null,
      expenseAmount: itineraryItemData.value.expenseAmount,
      startTime: itineraryItemData.value.startTime || null,
      endTime: itineraryItemData.value.endTime || null,
      notes: itineraryItemData.value.notes || null,
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

// --- 드래그앤드롭 (데스크톱) ---
function onDragStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  draggedItem.value = item
  draggedDay.value = dayNumber

  document.body.style.overflow = 'hidden'
  document.body.style.touchAction = 'none'

  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

function onDrag(e) {
  e.preventDefault()
}

function onDragEnd() {
  document.body.style.overflow = ''
  document.body.style.touchAction = ''
}

function onDragOver(e) {
  e.preventDefault()
  e.stopPropagation()
}

function onDrop(targetItem, targetDayNumber) {
  if (!isEditModeForDay(targetDayNumber) || !draggedItem.value) return

  document.body.style.overflow = ''
  document.body.style.touchAction = ''

  const items = trip.value.itineraryItems
  const draggedIndex = items.findIndex(i => i.id === draggedItem.value.id)
  const targetIndex = items.findIndex(i => i.id === targetItem.id)

  if (draggedIndex === -1 || targetIndex === -1) return

  if (draggedDay.value !== targetDayNumber) {
    alert('같은 날짜 내에서만 순서를 변경할 수 있습니다.')
    draggedItem.value = null
    draggedDay.value = null
    return
  }

  const [removed] = items.splice(draggedIndex, 1)
  items.splice(targetIndex, 0, removed)

  saveItineraryOrder(targetDayNumber)

  draggedItem.value = null
  draggedDay.value = null
}

// --- 터치 드래그앤드롭 (모바일) ---
function onTouchStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  const touch = event.touches[0];
  draggedItem.value = item
  draggedDay.value = dayNumber
  
  const cardElement = event.currentTarget.closest('[data-item-id]');
  touchDraggedElement.value = cardElement;
  const rect = cardElement.getBoundingClientRect();

  touchOffsetX.value = touch.clientX - rect.left;
  touchOffsetY.value = touch.clientY - rect.top;

  // Create ghost element
  ghostElement.value = cardElement.cloneNode(true);
  ghostElement.value.style.position = 'fixed';
  ghostElement.value.style.zIndex = '1000';
  ghostElement.value.style.opacity = '0.8';
  ghostElement.value.style.pointerEvents = 'none';
  ghostElement.value.style.width = `${rect.width}px`;
  document.body.appendChild(ghostElement.value);

  // Initial position
  ghostElement.value.style.left = `${touch.clientX - touchOffsetX.value}px`;
  ghostElement.value.style.top = `${touch.clientY - touchOffsetY.value}px`;

  cardElement.style.opacity = '0.5'

  window.addEventListener('touchmove', onTouchMove, { passive: false });
  window.addEventListener('touchend', onTouchEnd);
}

function onTouchMove(event) {
  if (!draggedItem.value || !ghostElement.value) return

  event.preventDefault()

  const touch = event.touches[0]
  
  // Move ghost element
  ghostElement.value.style.left = `${touch.clientX - touchOffsetX.value}px`;
  ghostElement.value.style.top = `${touch.clientY - touchOffsetY.value}px`;


  const elements = document.elementsFromPoint(touch.clientX, touch.clientY)
  const targetCard = elements.find(el => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value)

  if (targetCard) {
    targetCard.style.borderColor = 'rgba(23, 177, 133, 1)'
  }
}

function onTouchEnd(event) {
  if (!draggedItem.value) return

  window.removeEventListener('touchmove', onTouchMove);
  window.removeEventListener('touchend', onTouchEnd);

  if (touchDraggedElement.value) {
    touchDraggedElement.value.style.opacity = ''
  }
  
  // Remove ghost element
  if (ghostElement.value) {
    ghostElement.value.remove();
    ghostElement.value = null;
  }

  const touch = event.changedTouches[0]
  const elements = document.elementsFromPoint(touch.clientX, touch.clientY)
  const targetCard = elements.find(el => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value)

  if (targetCard) {
    const targetId = parseInt(targetCard.getAttribute('data-item-id'))
    const target = trip.value.itineraryItems.find(i => i.id === targetId)

    if (target) {
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
        const [removed] = items.splice(draggedIndex, 1)
        items.splice(targetIndex, 0, removed)

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
    dayItems.forEach(({ id, orderNum }) => {
      const item = trip.value.itineraryItems.find(i => i.id === id)
      if (item) {
        item.orderNum = orderNum
      }
    })
  } catch (error) {
    console.error('Failed to save order:', error)
    alert('순서 저장에 실패했습니다.')
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

function toggleItemSelection(id) {
  const index = selectedItineraryItems.value.indexOf(id)
  if (index > -1) {
    selectedItineraryItems.value.splice(index, 1)
  } else {
    selectedItineraryItems.value.push(id)
  }
}

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
    selectedItineraryItems.value = selectedItineraryItems.value.filter(id =>
      !itemsToDelete.includes(id)
    )
    await loadTrip()
  } catch (error) {
    console.error('Failed to delete items:', error)
    alert('일정 삭제에 실패했습니다.')
  }
}

// --- Kakao Map Search Modal ---
function openKakaoMapSearchModal(target) {
  currentKakaoSearchTarget.value = target
  isKakaoMapSearchModalOpen.value = true
}

function handleKakaoPlaceSelection(place) {
  if (currentKakaoSearchTarget.value === 'itinerary') {
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

<style scoped>
.no-scrollbar::-webkit-scrollbar {
  display: none;
}

.no-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}

.label {
  @apply block text-sm font-medium text-gray-700 mb-1;
}

.input {
  @apply w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500;
}
</style>
