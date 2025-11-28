<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'교통편'" :show-back="true" />

    <div v-if="loading" class="text-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-4 text-gray-600 font-medium">교통편을 불러오는 중...</p>
    </div>

    <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- 카테고리 필터 버튼 (등록된 항목이 있는 것만) -->
      <div class="flex flex-wrap gap-2 mb-4">
        <button
          @click="selectedCategoryFilter = null"
          class="px-3 py-1.5 rounded-full text-sm font-semibold transition-all"
          :class="selectedCategoryFilter === null
            ? 'bg-primary-500 text-white shadow-md'
            : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
        >
          전체
        </button>
        <button
          v-for="category in categoriesWithData"
          :key="category.name"
          @click="selectCategoryFilter(category.name)"
          class="flex items-center gap-1 px-3 py-1.5 rounded-full text-sm font-semibold transition-all"
          :class="selectedCategoryFilter === category.name
            ? 'bg-primary-500 text-white shadow-md'
            : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-200'"
        >
          <component :is="category.icon" class="w-3.5 h-3.5" />
          {{ category.name }}
          <span class="text-xs opacity-80">{{ getFlightsByCategory(category.name).length }}</span>
        </button>
      </div>

      <!-- 카테고리별 섹션 -->
      <div class="space-y-4">
        <div v-for="category in filteredCategories" :key="category.name" class="bg-white rounded-2xl shadow-md overflow-hidden">
          <!-- 카테고리 헤더 -->
          <button
            @click="toggleCategory(category.name)"
            class="w-full p-4 flex justify-between items-center"
          >
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" :class="category.bgClass">
                <component :is="category.icon" class="w-5 h-5" :class="category.iconClass" />
              </div>
              <div class="text-left">
                <h3 class="font-bold text-gray-900">{{ category.name }}</h3>
                <p class="text-xs text-gray-500">{{ getFlightsByCategory(category.name).length }}건</p>
              </div>
            </div>
            <svg
              class="w-5 h-5 text-gray-400 transition-transform"
              :class="{ 'rotate-180': expandedCategories.includes(category.name) }"
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          <!-- 카테고리 내용 -->
          <div v-if="expandedCategories.includes(category.name)" class="border-t border-gray-100">
            <div class="p-4 space-y-3">
              <!-- 교통편 목록 -->
              <div v-if="getFlightsByCategory(category.name).length > 0">
                <template v-if="category.name === '항공편'">
                  <div
                    v-for="flight in getFlightsByCategory(category.name)"
                    :key="flight.id"
                    @click="!effectiveReadonly && openDetailModal(flight)"
                    class="bg-white rounded-3xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] overflow-hidden mb-5 border border-gray-100 relative group w-full"
                    :class="{ 'cursor-pointer': !effectiveReadonly }"
                  >
                    <div class="absolute inset-0 flex items-center justify-center pointer-events-none overflow-hidden z-0">
                      <img src="/images/logo_w.png" alt="Airline Logo" class="w-64 h-64 text-indigo-900 opacity-5 transform -rotate-12" />
                    </div>
                    <div class="h-1.5 w-full relative z-10" :class="getStatusConfig(flight).bg"></div>
                    <div class="p-5 relative z-10">
                      <div class="flex justify-between items-start mb-6">
                        <div class="flex items-center gap-3">
                          <div class="w-10 h-10 rounded-xl bg-gray-50 flex items-center justify-center text-xs font-bold text-gray-700 shadow-sm border border-gray-100">
                            {{ getAirlineLogoText(flight) }}
                          </div>
                          <div>
                            <h3 class="font-bold text-gray-900 text-sm tracking-tight">{{ flight.airline }}</h3>
                            <div class="flex items-center gap-2 text-xs text-gray-500 mt-0.5">
                              <span class="font-medium text-gray-600">{{ flight.flightNumber }}</span>
                            </div>
                          </div>
                        </div>
                        <div class="px-2.5 py-1 rounded-full text-[11px] font-bold tracking-tight flex items-center gap-1.5 shadow-sm" :class="[getStatusConfig(flight).badgeBg, getStatusConfig(flight).text]">
                          <component :is="getStatusConfig(flight).icon" class="w-3 h-3" />
                          {{ getStatusConfig(flight).label }}
                        </div>
                      </div>

                      <div class="flex items-center justify-between mb-6 px-1">
                        <div class="text-left">
                          <div class="text-3xl font-black text-gray-800 tracking-tight">{{ flight.departureAirportCode || (flight.departureLocation ? flight.departureLocation.substring(0,3).toUpperCase() : 'N/A') }}</div>
                          <div class="text-xs font-bold text-gray-500 mt-1">{{ formatTime(flight.departureTime) }}</div>
                        </div>

                        <div class="flex-1 px-4 flex flex-col items-center">
                          <div class="text-[10px] text-gray-400 font-medium mb-1">직항</div>
                          <div class="w-full h-px bg-gray-200 relative flex items-center justify-center">
                            <Plane class="w-5 h-5 text-indigo-400 rotate-90 absolute z-0" />
                            <div class="w-1.5 h-1.5 rounded-full bg-gray-300 absolute left-0"></div>
                            <div class="w-1.5 h-1.5 rounded-full bg-gray-300 absolute right-0"></div>
                          </div>
                          <div class="text-[10px] text-gray-400 font-medium mt-1">비행 시간</div>
                        </div>

                        <div class="text-right">
                          <div class="text-3xl font-black tracking-tight" :class="isDelayed(flight) ? 'text-red-500' : 'text-indigo-600'">
                            {{ flight.arrivalAirportCode || (flight.arrivalLocation ? flight.arrivalLocation.substring(0,3).toUpperCase() : 'N/A') }}
                          </div>
                          <div class="flex flex-col items-end mt-1">
                            <span class="text-xs font-bold" :class="isDelayed(flight) ? 'text-red-500 animate-pulse' : 'text-gray-800'">
                               {{ isDelayed(flight) ? formatTime(flight.estimatedTime) : formatTime(flight.arrivalTime) }}
                            </span>
                            <span v-if="isDelayed(flight)" class="text-[10px] text-gray-400 line-through">{{ formatTime(flight.arrivalTime) }}</span>
                          </div>
                        </div>
                      </div>

                      <div class="bg-gray-50/80 backdrop-blur-[2px] rounded-2xl p-4 grid grid-cols-2 gap-y-4 gap-x-2 border border-gray-100/50">
                        <div class="flex flex-col">
                          <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1"><Calendar class="w-3 h-3" /> 날짜</span>
                          <span class="text-sm font-bold text-gray-700">{{ formatDate(getFlightType(flight) === 'Arrival' ? flight.arrivalTime : flight.departureTime) }}</span>
                        </div>
                        <div class="flex flex-col border-l border-gray-200 pl-4">
                          <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1"><DoorOpen class="w-3 h-3" /> 탑승구</span>
                          <span class="text-sm font-bold text-indigo-600">{{ flight.gate || '미정' }}</span>
                        </div>
                        <div class="flex flex-col">
                          <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1"><Building2 class="w-3 h-3" /> 터미널</span>
                          <span class="text-sm font-bold text-gray-700">{{ flight.terminal || 'N/A' }}</span>
                        </div>
                        <div class="flex flex-col border-l border-gray-200 pl-4">
                           <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1"><MapPin class="w-3 h-3" /> 예약번호</span>
                           <span class="text-sm font-bold text-gray-700">{{ flight.bookingReference || '없음' }}</span>
                        </div>
                      </div>
                      
                      <div class="mt-4 flex items-center justify-center">
                        <div class="text-xs text-gray-500 font-medium bg-white/90 px-3 py-1 rounded-full border border-gray-100 shadow-sm flex items-center gap-1 backdrop-blur-sm">
                          <MapPin class="w-3 h-3 text-indigo-400" />
                          {{ getArrivalCity(flight) }} 도착
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
                <template v-else>
                  <!-- 기차 티켓 스타일 -->
                  <template v-if="category.name === '기차'">
                    <div
                      v-for="flight in getFlightsByCategory(category.name)"
                      :key="flight.id"
                      @click="!effectiveReadonly && openDetailModal(flight)"
                      class="bg-gradient-to-br from-green-50 to-emerald-50 rounded-2xl shadow-lg overflow-hidden mb-4 border-2 border-green-200 relative"
                      :class="{ 'cursor-pointer hover:shadow-xl': !effectiveReadonly }"
                    >
                      <div class="p-6">
                        <!-- Header -->
                        <div class="flex justify-between items-center mb-6">
                          <div class="flex items-center gap-2">
                            <div class="w-12 h-12 rounded-full bg-green-500 flex items-center justify-center">
                              <Train class="w-7 h-7 text-white" />
                            </div>
                            <div>
                              <div class="text-sm font-semibold text-green-700">{{ flight.flightNumber || 'KTX' }}</div>
                              <div class="text-xs text-gray-600">기차</div>
                            </div>
                          </div>
                          <div class="text-right">
                            <div class="text-sm text-gray-600">{{ formatDate(flight.departureTime) }}</div>
                          </div>
                        </div>

                        <!-- Route -->
                        <div class="bg-white rounded-xl p-5 mb-4 shadow-sm">
                                    <div class="flex items-center justify-between gap-4">
                                      <div class="flex-1">
                                        <div class="text-xs text-gray-500 mb-1">출발</div>
                                        <div class="text-xl font-black text-gray-900">{{ flight.departureLocation || '출발역' }}</div>
                                        <div class="text-base font-bold text-green-600 mt-1">{{ formatTime(flight.departureTime) }}</div>
                                      </div>
                          
                                      <div class="flex flex-col items-center px-2">
                                        <Train class="w-6 h-6 text-green-500 mb-1" />
                                        <div class="w-16 h-0.5 bg-green-300"></div>
                                      </div>
                          
                                      <div class="flex-1 text-right">
                                        <div class="text-xs text-gray-500 mb-1">도착</div>
                                        <div class="text-xl font-black text-gray-900">{{ flight.arrivalLocation || '도착역' }}</div>
                                        <div class="text-base font-bold text-green-600 mt-1">{{ formatTime(flight.arrivalTime) }}</div>
                                      </div>
                                    </div>                        </div>

                        <!-- Info -->
                        <div v-if="flight.bookingReference" class="flex items-center justify-end">
                          <div class="text-right">
                            <div class="text-xs text-gray-600 mb-1">예약번호</div>
                            <div class="text-sm font-semibold text-gray-900">{{ flight.bookingReference }}</div>
                          </div>
                        </div>

                        <!-- Notes -->
                        <div v-if="flight.notes" class="mt-4 p-3 bg-white rounded-lg border border-green-200">
                          <p class="text-sm text-gray-700">{{ flight.notes }}</p>
                        </div>
                      </div>
                    </div>
                  </template>

                  <!-- 기타 교통수단 -->
                   <div
                    v-else
                    v-for="flight in getFlightsByCategory(category.name)"
                    :key="flight.id"
                    @click="!effectiveReadonly && openDetailModal(flight)"
                    class="border rounded-xl p-4 bg-gray-50 hover:bg-gray-100 transition-colors"
                    :class="{ 'cursor-pointer': !effectiveReadonly }"
                  >
                    <div class="flex justify-between items-start">
                      <div class="flex-1 min-w-0">

                        <!-- 버스 (간소화) -->
                        <template v-if="flight.category === '버스'">
                          <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-700 mb-1">
                            <span>{{ flight.departureLocation || '출발지' }}</span>
                            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                            </svg>
                            <span>{{ flight.arrivalLocation || '도착지' }}</span>
                          </div>
                          <div v-if="flight.departureTime" class="text-xs text-gray-500">
                            {{ formatDate(flight.departureTime) }}
                          </div>
                        </template>

                        <!-- 택시 -->
                        <template v-else-if="flight.category === '택시'">
                          <div v-if="flight.departureLocation || flight.arrivalLocation" class="flex items-center gap-2 text-sm text-gray-700 mb-1">
                            <span>{{ flight.departureLocation || '출발지' }}</span>
                            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3" />
                            </svg>
                            <span>{{ flight.arrivalLocation || '도착지' }}</span>
                          </div>
                          <div v-if="getLinkedItinerary(flight.itineraryItemId)" class="text-xs text-gray-500">
                            연결된 일정: {{ getLinkedItinerary(flight.itineraryItemId)?.locationName }}
                          </div>
                        </template>

                        <!-- 렌트카/자가용 -->
                        <template v-else-if="['렌트카', '자가용'].includes(flight.category)">
                          <div v-if="flight.airline" class="font-semibold text-gray-900 mb-1">{{ flight.airline }}</div>
                          <div v-if="flight.departureTime" class="text-xs text-gray-500">
                            {{ formatDate(flight.departureTime) }}
                            <span v-if="flight.arrivalTime"> ~ {{ formatDate(flight.arrivalTime) }}</span>
                          </div>
                        </template>

                        <!-- 메모 -->
                        <p v-if="flight.notes" class="text-xs text-gray-500 mt-2 italic">{{ flight.notes }}</p>
                      </div>
                    </div>
                  </div>
                </template>
              </div>
              <div v-else class="text-center py-6 text-gray-400">
                등록된 {{ category.name }} 없습니다
              </div>

              <!-- 추가 버튼 -->
              <button
                v-if="!effectiveReadonly"
                @click="openAddModal(category.name)"
                class="w-full py-3 border-2 border-dashed rounded-xl font-medium transition-all"
                :class="category.addButtonClass"
              >
                + {{ category.name }} 추가
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 교통편 추가/수정 모달 -->
    <SlideUpModal :is-open="isEditModalOpen" @close="closeEditModal" z-index-class="z-[60]">
      <template #header-title>{{ editingFlight?.id ? '교통편 수정' : '교통편 추가' }}</template>
      <template #body>
        <form id="transportation-form" @submit.prevent="saveTransportation" class="space-y-4">
          <!-- 카테고리 (읽기전용) -->
          <div>
            <label class="label">카테고리</label>
            <input v-model="flightData.category" type="text" class="input bg-gray-100" readonly />
          </div>

          <!-- 항공편 전용 필드 -->
          <template v-if="flightData.category === '항공편'">
            <!-- 항공편 조회 버튼 -->
            <div class="bg-blue-50 border border-blue-200 rounded-lg p-3" v-if="!isApiSourcedFlight">
              <button
                type="button"
                @click="openFlightSearchModal"
                class="w-full py-2 px-4 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors flex items-center justify-center gap-2"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                인천공항 항공편 조회
              </button>
              <p class="text-xs text-blue-700 mt-2 text-center">실시간 항공편 정보를 자동으로 입력합니다</p>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">항공사</label>
                <input v-model="flightData.airline" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="대한항공, 아시아나 등" :readonly="isApiSourcedFlight" />
              </div>
              <div>
                <label class="label">편명</label>
                <input v-model="flightData.flightNumber" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="KE123" :readonly="isApiSourcedFlight" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="출발 공항 코드" :readonly="isApiSourcedFlight" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="도착 공항 코드" :readonly="isApiSourcedFlight" />
              </div>
            </div>
            <div>
              <label class="label">출발 일시</label>
              <CommonDatePicker
                v-model:value="flightData.departureTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
                placeholder="날짜와 시간을 선택하세요"
                :disabled="isApiSourcedFlight && getFlightType(editingFlight) === 'Departure'"
                :class="{'bg-gray-100': isApiSourcedFlight && getFlightType(editingFlight) === 'Departure'}"
                class="w-full"
              />
            </div>
            <div>
              <label class="label">도착 일시</label>
              <CommonDatePicker
                v-model:value="flightData.arrivalTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
                placeholder="날짜와 시간을 선택하세요"
                :disabled="isApiSourcedFlight && getFlightType(editingFlight) === 'Arrival'"
                :class="{'bg-gray-100': isApiSourcedFlight && getFlightType(editingFlight) === 'Arrival'}"
                class="w-full"
              />
            </div>
             <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">터미널</label>
                <input v-model="flightData.terminal" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="T1 / T2" :readonly="isApiSourcedFlight" />
              </div>
              <div>
                <label class="label">탑승구</label>
                <input v-model="flightData.gate" type="text" class="input" :class="{'bg-gray-100': isApiSourcedFlight}" placeholder="탑승구 번호" :readonly="isApiSourcedFlight" />
              </div>
            </div>
            <div>
              <label class="label">예약번호</label>
              <input v-model="flightData.bookingReference" type="text" class="input" placeholder="예약번호 (선택)" />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" v-number-format type="text" class="input" placeholder="예: 150000" min="0" step="100" />
            </div>
          </template>

          <!-- 기차 전용 필드 (간소화) -->
          <template v-else-if="flightData.category === '기차'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발역</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="서울역" />
              </div>
              <div>
                <label class="label">도착역</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="부산역" />
              </div>
            </div>
            <div>
              <label class="label">출발 일시</label>
              <CommonDatePicker
                v-model:value="flightData.departureTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
                placeholder="날짜와 시간을 선택하세요"
                class="w-full"
              />
            </div>
            <div>
              <label class="label">도착 일시</label>
              <CommonDatePicker
                v-model:value="flightData.arrivalTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
                placeholder="날짜와 시간을 선택하세요"
                class="w-full"
              />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" v-number-format type="text" class="input" placeholder="예: 50000" min="0" step="100" />
            </div>
          </template>

          <!-- 버스 전용 필드 (간소화) -->
          <template v-else-if="flightData.category === '버스'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="출발 터미널" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="도착 터미널" />
              </div>
            </div>
            <div>
              <label class="label">출발 날짜</label>
              <input v-model="flightData.departureDate" type="date" class="input" />
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" v-number-format type="text" class="input" placeholder="예: 30000" min="0" step="100" />
            </div>
          </template>

          <!-- 택시 전용 필드 -->
          <template v-else-if="flightData.category === '택시'">
            <div>
              <label class="label">연결할 일정 (선택)</label>
              <select v-model="flightData.itineraryItemId" class="input">
                <option :value="null">선택 안함</option>
                <option v-for="item in trip.itineraryItems" :key="item.id" :value="item.id">
                  Day {{ item.dayNumber }} - {{ item.locationName }}
                </option>
              </select>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="input" placeholder="출발지" />
              </div>
              <div>
                <label class="label">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="input" placeholder="도착지" />
              </div>
            </div>
            <div>
              <label class="label">금액 (원)</label>
              <input v-model.number="flightData.amount" v-number-format type="text" class="input" placeholder="예: 15000" min="0" step="100" />
            </div>
          </template>

          <!-- 렌트카 전용 필드 -->
          <template v-else-if="flightData.category === '렌트카'">
            <div>
              <label class="label">렌트회사</label>
              <input v-model="flightData.airline" type="text" class="input" placeholder="렌트회사명" />
            </div>
            <div class="space-y-4">
              <div>
                <label class="label">대여 일시</label>
                <CommonDatePicker
                  v-model:value="flightData.departureTime"
                  type="datetime"
                  format="YYYY-MM-DD HH:mm"
                  value-type="YYYY-MM-DDTHH:mm:ss"
                  placeholder="날짜와 시간을 선택하세요"
                  class="w-full"
                />
              </div>
              <div>
                <label class="label">반납 일시</label>
                <CommonDatePicker
                  v-model:value="flightData.arrivalTime"
                  type="datetime"
                  format="YYYY-MM-DD HH:mm"
                  value-type="YYYY-MM-DDTHH:mm:ss"
                  placeholder="날짜와 시간을 선택하세요"
                  class="w-full"
                />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">렌트비 (원)</label>
                <input v-model.number="flightData.rentalCost" v-number-format type="text" class="input" placeholder="예: 100000" min="0" step="100" />
              </div>
              <div>
                <label class="label">유류비 (원)</label>
                <input v-model.number="flightData.fuelCost" v-number-format type="text" class="input" placeholder="예: 50000" min="0" step="100" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">톨비 (원)</label>
                <input v-model.number="flightData.tollFee" v-number-format type="text" class="input" placeholder="예: 20000" min="0" step="100" />
              </div>
              <div>
                <label class="label">주차비 (원)</label>
                <input v-model.number="flightData.parkingFee" v-number-format type="text" class="input" placeholder="예: 15000" min="0" step="100" />
              </div>
            </div>
          </template>

          <!-- 자가용 전용 필드 -->
          <template v-else-if="flightData.category === '자가용'">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="label">유류비 (원)</label>
                <input v-model.number="flightData.fuelCost" v-number-format type="text" class="input" placeholder="예: 50000" min="0" step="100" />
              </div>
              <div>
                <label class="label">톨비 (원)</label>
                <input v-model.number="flightData.tollFee" v-number-format type="text" class="input" placeholder="예: 20000" min="0" step="100" />
              </div>
            </div>
            <div>
              <label class="label">주차비 (원)</label>
              <input v-model.number="flightData.parkingFee" v-number-format type="text" class="input" placeholder="예: 15000" min="0" step="100" />
            </div>
          </template>

          <!-- 공통: 메모 -->
          <div>
            <label class="label">메모</label>
            <textarea v-model="flightData.notes" rows="2" class="input" placeholder="메모"></textarea>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button v-if="editingFlight?.id" type="button" @click="deleteTransportation" class="py-3 px-4 bg-red-100 text-red-700 rounded-xl font-semibold hover:bg-red-200 transition-colors">
            삭제
          </button>
          <button type="button" @click="closeEditModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 transition-colors">
            취소
          </button>
          <button type="submit" form="transportation-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 transition-all">
            저장
          </button>
        </div>
      </template>
    </SlideUpModal>

    <!-- 항공편 추가 모달 (새로운 방식) -->
    <FlightAddEditModal
      :show="showFlightAddModal"
      @close="showFlightAddModal = false"
      @save="saveFlightsFromModal"
    />

    <!-- 항공편 조회 모달 (기존 방식 - 수기 입력용) -->
    <FlightSearchModal
      :show="showFlightSearchModal"
      @close="showFlightSearchModal = false"
      @apply="applyFlightInfo"
    />

    <!-- Bottom Navigation Bar -->
    <BottomNavigationBar
      v-if="((tripId && tripId !== 'undefined') || (shareToken && shareToken !== 'undefined')) && !uiStore.isModalOpen"
      :trip-id="tripId || trip.id"
      :share-token="shareToken"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import SlideUpModal from '@/components/common/SlideUpModal.vue';
import FlightSearchModal from '@/components/trip/FlightSearchModal.vue';
import FlightAddEditModal from '@/components/personalTrip/FlightAddEditModal.vue';
import CommonDatePicker from '@/components/common/CommonDatePicker.vue';
import 'vue-datepicker-next/index.css';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import dayjs from 'dayjs';
import { Plane, Train, Bus, Car, Clock, AlertCircle, CheckCircle, DoorOpen, Building2, MapPin, Calendar } from 'lucide-vue-next';

// Props for readonly mode and shared access
const props = defineProps({
  shareToken: String,
  readonly: {
    type: Boolean,
    default: false
  }
})

const getFlightType = (flight) => {
  if (flight.arrivalLocation?.toUpperCase().includes('ICN') || flight.arrivalLocation?.includes('인천')) {
    return 'Arrival';
  }
  return 'Departure';
};

const getStatusConfig = (flight) => {
  const status = flight.status?.toLowerCase() || '';
  
  switch (status) {
    case 'boarding':
      return { label: '탑승중', bg: 'bg-green-500', text: 'text-green-600', badgeBg: 'bg-green-100', icon: DoorOpen };
    case 'delayed':
      return { label: '지연', bg: 'bg-red-500', text: 'text-red-600', badgeBg: 'bg-red-100', icon: AlertCircle };
    case 'on time': case 'ontime':
      return { label: '정시', bg: 'bg-blue-500', text: 'text-blue-600', badgeBg: 'bg-blue-100', icon: CheckCircle };
    case 'cancelled':
      return { label: '결항', bg: 'bg-gray-500', text: 'text-gray-600', badgeBg: 'bg-gray-200', icon: AlertCircle };
    case 'scheduled':
      return { label: '예정', bg: 'bg-indigo-500', text: 'text-indigo-600', badgeBg: 'bg-indigo-100', icon: Clock };
    default:
      return { label: flight.status || '정보 없음', bg: 'bg-gray-500', text: 'text-gray-600', badgeBg: 'bg-gray-100', icon: Clock };
  }
};

const getAirlineLogoText = (flight) => flight.airline ? flight.airline.substring(0, 2).toUpperCase() : '';
const isDelayed = (flight) => flight.status?.toLowerCase() === 'delayed';
const getArrivalCity = (flight) => flight.arrivalLocation ? flight.arrivalLocation.split('(')[0].trim() : '';

const formatTime = (time) => time ? dayjs(time).format('HH:mm') : '--:--';
const formatDate = (date) => date ? dayjs(date).format('YYYY-MM-DD') : '날짜 정보 없음';
const formatDateTime = (dateTime) => dateTime ? dayjs(dateTime).format('YYYY-MM-DD HH:mm') : '';


const uiStore = useUIStore();
const route = useRoute();
const router = useRouter();

// Determine tripId and readonly mode (filter out undefined strings)
const tripId = computed(() => {
  const id = route.params.id
  return (id && id !== 'undefined') ? id : null
});
const shareToken = computed(() => {
  const token = props.shareToken || route.params.shareToken
  return (token && token !== 'undefined') ? token : null
});
const isSharedView = computed(() => !!shareToken.value);
const effectiveReadonly = computed(() => props.readonly || isSharedView.value);

const loading = ref(true);
const trip = ref({});
const expandedCategories = ref(['항공편']); // 기본으로 항공편 펼침
const selectedCategoryFilter = ref(null); // null = 전체 보기

// Modal states
const isEditModalOpen = ref(false);
const editingFlight = ref(null);
const flightData = ref({});
const showFlightSearchModal = ref(false);
const showFlightAddModal = ref(false);
const isApiSourcedFlight = ref(false);

// 카테고리 정의
const categories = [
  {
    name: '항공편',
    icon: Plane,
    iconClass: 'text-blue-600',
    bgClass: 'bg-blue-100',
    addButtonClass: 'border-blue-300 text-blue-600 bg-blue-50 hover:bg-blue-100'
  },
  {
    name: '기차',
    icon: Train,
    iconClass: 'text-green-600',
    bgClass: 'bg-green-100',
    addButtonClass: 'border-green-300 text-green-600 bg-green-50 hover:bg-green-100'
  },
  {
    name: '버스',
    icon: Bus,
    iconClass: 'text-orange-600',
    bgClass: 'bg-orange-100',
    addButtonClass: 'border-orange-300 text-orange-600 bg-orange-50 hover:bg-orange-100'
  },
  {
    name: '택시',
    icon: Car,
    iconClass: 'text-purple-600',
    bgClass: 'bg-purple-100',
    addButtonClass: 'border-purple-300 text-purple-600 bg-purple-50 hover:bg-purple-100'
  },
  {
    name: '렌트카',
    icon: Car,
    iconClass: 'text-pink-600',
    bgClass: 'bg-pink-100',
    addButtonClass: 'border-pink-300 text-pink-600 bg-pink-50 hover:bg-pink-100'
  },
  {
    name: '자가용',
    icon: Car,
    iconClass: 'text-indigo-600',
    bgClass: 'bg-indigo-100',
    addButtonClass: 'border-indigo-300 text-indigo-600 bg-indigo-50 hover:bg-indigo-100'
  },
];

// 데이터 로드
async function loadTrip() {
  try {
    loading.value = true;
    if (shareToken.value) {
      const response = await apiClient.get(`/personal-trips/public/${shareToken.value}`);
      trip.value = response.data;
    } else {
      const response = await apiClient.get(`/personal-trips/${tripId.value}`);
      trip.value = response.data;
    }
  } catch (error) {
    console.error('Failed to load trip:', error);
    alert('여행 정보를 불러오는데 실패했습니다.');
    if (isSharedView.value) {
      router.push('/home');
    } else {
      router.push('/trips');
    }
  } finally {
    loading.value = false;
  }
}

// 계산된 값들
const totalTransportationCost = computed(() => {
  if (!trip.value.flights) return 0;
  return trip.value.flights.reduce((sum, f) => sum + getFlightTotalCost(f), 0);
});

// 등록된 데이터가 있는 카테고리만 필터링
const categoriesWithData = computed(() => {
  return categories.filter(category => getFlightsByCategory(category.name).length > 0);
});

// 선택된 필터에 따라 표시할 카테고리
const filteredCategories = computed(() => {
  if (selectedCategoryFilter.value === null) {
    // 전체 보기 - 모든 카테고리 표시
    return categories;
  }
  // 특정 카테고리만 표시
  return categories.filter(cat => cat.name === selectedCategoryFilter.value);
});

function getFlightsByCategory(category) {
  if (!trip.value.flights) return [];
  const flights = trip.value.flights.filter(f => f.category === category);

  if (category === '항공편') {
    flights.sort((a, b) => {
      const typeA = getFlightType(a);
      const typeB = getFlightType(b);

      if (typeA === 'Departure' && typeB === 'Arrival') {
        return -1; // Departures first
      }
      if (typeA === 'Arrival' && typeB === 'Departure') {
        return 1; // Arrivals second
      }

      // For same type, sort chronologically
      const timeA = a.departureTime || a.arrivalTime;
      const timeB = b.departureTime || b.arrivalTime;
      if (timeA && timeB) {
        return new Date(timeA) - new Date(timeB);
      }
      return 0;
    });
  }

  return flights;
}

function getCategoryTotal(category) {
  return getFlightsByCategory(category).reduce((sum, f) => sum + getFlightTotalCost(f), 0);
}

function getFlightTotalCost(flight) {
  if (['렌트카', '자가용'].includes(flight.category)) {
    return (flight.rentalCost || 0) + (flight.fuelCost || 0) + (flight.tollFee || 0) + (flight.parkingFee || 0);
  }
  return flight.amount || 0;
}

function getLinkedItinerary(itineraryItemId) {
  if (!itineraryItemId || !trip.value.itineraryItems) return null;
  return trip.value.itineraryItems.find(i => i.id === itineraryItemId);
}

// 카테고리 토글
function toggleCategory(category) {
  const index = expandedCategories.value.indexOf(category);
  if (index > -1) {
    expandedCategories.value.splice(index, 1);
  } else {
    expandedCategories.value.push(category);
  }
}

// 카테고리 필터 선택
function selectCategoryFilter(categoryName) {
  selectedCategoryFilter.value = categoryName;
  // 선택된 카테고리를 자동으로 펼침
  if (!expandedCategories.value.includes(categoryName)) {
    expandedCategories.value.push(categoryName);
  }
}

// 모달 관련
function openAddModal(category) {
  // 항공편 카테고리는 새로운 FlightAddEditModal 사용
  if (category === '항공편') {
    showFlightAddModal.value = true;
    return;
  }

  // 다른 카테고리는 기존 모달 사용
  editingFlight.value = null;
  flightData.value = {
    category,
    itineraryItemId: null,
    airline: '',
    flightNumber: '',
    departureLocation: '',
    arrivalLocation: '',
    departureTime: '',
    arrivalTime: '',
    departureDate: '', // 버스용 날짜
    bookingReference: '',
    seatNumber: '',
    amount: null,
    rentalCost: null,
    fuelCost: null,
    tollFee: null,
    parkingFee: null,
    notes: ''
  };
  isEditModalOpen.value = true;
}

function openDetailModal(flight) {
  editingFlight.value = flight;
  isApiSourcedFlight.value = !!flight.flightNumber; // 편명이 있으면 API 조회로 간주

  flightData.value = {
    ...flight,
    departureLocation: flight.departureAirportCode || flight.departureLocation,
    arrivalLocation: flight.arrivalAirportCode || flight.arrivalLocation,
    terminal: flight.terminal || '',
    gate: flight.gate || '',
    departureTime: flight.departureTime ? dayjs(flight.departureTime).format('YYYY-MM-DDTHH:mm:ss') : '',
    arrivalTime: flight.arrivalTime ? dayjs(flight.arrivalTime).format('YYYY-MM-DDTHH:mm:ss') : '',
    departureDate: flight.departureTime ? dayjs(flight.departureTime).format('YYYY-MM-DD') : ''
  };
  isEditModalOpen.value = true;
}

function closeEditModal() {
  isEditModalOpen.value = false;
  editingFlight.value = null;
}

// 항공편 조회 모달 열기
function openFlightSearchModal() {
  showFlightSearchModal.value = true;
}

// 항공편 정보 적용
function applyFlightInfo(info) {
  if (!info) return;

  // FlightSearchModal에서 받은 정보를 flightData에 적용
  flightData.value.airline = info.airline || '';
  flightData.value.flightNumber = info.flightNumber || '';
  flightData.value.departureLocation = info.departureAirport || '';
  flightData.value.arrivalLocation = info.arrivalAirport || '';
  flightData.value.departureAirportCode = info.departureAirportCode || '';
  flightData.value.arrivalAirportCode = info.arrivalAirportCode || '';

  // 날짜/시간 정보 (이미 YYYY-MM-DDTHH:mm 형식)
  if (info.departureTime) {
    flightData.value.departureTime = info.departureTime;
  }
  if (info.arrivalTime) {
    flightData.value.arrivalTime = info.arrivalTime;
  }

  // 터미널, 게이트, 상태 정보
  if (info.terminal) {
    flightData.value.terminal = info.terminal;
  }
  if (info.gate) {
    flightData.value.gate = info.gate;
  }
  if (info.status) {
    flightData.value.status = info.status;
  }

  console.log('항공편 정보 적용:', flightData.value);
}

async function saveTransportation() {
  try {
    // 렌트카/자가용은 합계를 amount에 저장
    if (['렌트카', '자가용'].includes(flightData.value.category)) {
      flightData.value.amount = getFlightTotalCost(flightData.value);
    }

    // 택시가 아니면 일정 연결 제거
    if (flightData.value.category !== '택시') {
      flightData.value.itineraryItemId = null;
    }

    // 버스는 날짜만 사용 -> departureTime으로 변환
    if (flightData.value.category === '버스' && flightData.value.departureDate) {
      flightData.value.departureTime = flightData.value.departureDate + 'T00:00';
    }

    // 페이로드 생성 및 데이터 정리
    const payload = {
      category: flightData.value.category,
      itineraryItemId: flightData.value.itineraryItemId || null,
      amount: flightData.value.amount || null,
      tollFee: flightData.value.tollFee || null,
      fuelCost: flightData.value.fuelCost || null,
      parkingFee: flightData.value.parkingFee || null,
      rentalCost: flightData.value.rentalCost || null,
      airline: flightData.value.airline || null,
      flightNumber: flightData.value.flightNumber || null,
      departureLocation: flightData.value.departureLocation || null,
      arrivalLocation: flightData.value.arrivalLocation || null,
      departureTime: flightData.value.departureTime || null,
      arrivalTime: flightData.value.arrivalTime || null,
      bookingReference: flightData.value.bookingReference || null,
      seatNumber: flightData.value.seatNumber || null,
      notes: flightData.value.notes || null,
    };

    if (editingFlight.value?.id) {
      await apiClient.put(`/personal-trips/flights/${editingFlight.value.id}`, payload);
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, payload);
    }

    await loadTrip();
    closeEditModal();
  } catch (error) {
    console.error('Failed to save transportation:', error);
    alert('저장에 실패했습니다.');
  }
}

async function deleteTransportation() {
  if (!confirm('이 교통편을 삭제하시겠습니까?')) return;
  try {
    await apiClient.delete(`/personal-trips/flights/${editingFlight.value.id}`);
    await loadTrip();
    closeEditModal();
  } catch (error) {
    console.error('Failed to delete transportation:', error);
    alert('삭제에 실패했습니다.');
  }
}

// 새로운 항공편 모달에서 여러 항공편 저장
async function saveFlightsFromModal(flights) {
  try {
    for (const flight of flights) {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, flight);
    }
    await loadTrip();
    showFlightAddModal.value = false;
  } catch (error) {
    console.error('Failed to save flights:', error);
    alert('항공편 저장에 실패했습니다.');
  }
}


onMounted(async () => {
  try {
    // 1. Ensure tripId or shareToken is valid
    const routeId = route.params.id
    const routeToken = route.params.shareToken

    // undefined 문자열이 온 경우 trips 목록으로 리다이렉트
    if (routeId === 'undefined' || routeToken === 'undefined') {
      console.warn('Invalid route params detected:', { routeId, routeToken })
      if (isSharedView.value) {
        router.push('/home')
      } else {
        router.push('/trips')
      }
      return
    }

    // 2. Load trip data
    await loadTrip()

    // 3. Set initial filter to the first category with data, if any
    if (categoriesWithData.value.length > 0) {
      selectedCategoryFilter.value = categoriesWithData.value[0].name;
      // 또한 해당 카테고리를 확장된 카테고리 목록에 추가하여 펼쳐진 상태로 만듭니다.
      if (!expandedCategories.value.includes(selectedCategoryFilter.value)) {
        expandedCategories.value.push(selectedCategoryFilter.value);
      }
    }
  } catch (error) {
    console.error('Failed to initialize TripTransportation:', error)
  }
});
</script>

<style scoped>
.label {
  @apply block text-sm font-medium text-gray-700 mb-1;
}

.input {
  @apply w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500;
}
</style>
