<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <div class="flex items-center gap-4">
        <h2 class="text-xl font-semibold">참석자 관리</h2>
        <!-- 선택된 참석자 수 표시 -->
        <span
          v-if="selectedGuests.length > 0"
          class="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium"
        >
          {{ selectedGuests.length }}명 선택됨
        </span>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2">
        <!-- 대량 작업 버튼 -->
        <button
          v-if="selectedGuests.length > 0"
          @click="showBulkAssignModal = true"
          class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
            />
          </svg>
          일정 일괄 배정
        </button>

        <!-- 속성 일괄 매핑 버튼 -->
        <button
          v-if="selectedGuests.length > 0"
          @click="showBulkAttributeModal = true"
          class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
            />
          </svg>
          속성 일괄 매핑
        </button>
        <button
          @click="showCreateModal = true"
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 flex items-center justify-center whitespace-nowrap overflow-hidden text-ellipsis"
        >
          + 참석자 추가
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-center py-8">로딩 중...</div>
    <div
      v-else-if="guests.length === 0"
      class="text-center py-8 bg-white rounded-lg shadow"
    >
      <p class="text-gray-500">등록된 참석자가 없습니다</p>
      <button
        @click="showCreateModal = true"
        class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
      >
        첫 참석자 추가하기
      </button>
    </div>
    <div v-else class="bg-white rounded-lg shadow overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left">
                <input
                  type="checkbox"
                  @change="toggleSelectAll"
                  :checked="
                    selectedGuests.length === guests.length && guests.length > 0
                  "
                  class="rounded"
                />
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                이름
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                전화번호
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                부서
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                일정
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                속성
              </th>
              <th
                class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase"
              >
                작업
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr
              v-for="guest in guests"
              :key="guest.id"
              class="hover:bg-gray-50"
            >
              <td class="px-6 py-4 whitespace-nowrap" @click.stop>
                <input
                  type="checkbox"
                  :value="guest.id"
                  v-model="selectedGuests"
                  class="rounded"
                />
              </td>
              <td
                class="px-6 py-4 whitespace-nowrap cursor-pointer"
                @click="showGuestDetail(guest.id)"
              >
                <div class="font-medium text-gray-900">
                  {{ guest.guestName }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.telephone }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.corpPart || '-' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span
                  v-if="guest.scheduleTemplates.length === 0"
                  class="text-gray-400"
                  >미배정</span
                >
                <div v-else class="flex flex-wrap gap-1">
                  <span
                    v-for="st in guest.scheduleTemplates"
                    :key="st.scheduleTemplateId"
                    class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs"
                  >
                    {{ st.courseName }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span v-if="guest.attributes.length === 0" class="text-gray-400"
                  >-</span
                >
                <span v-else class="text-gray-600"
                  >{{ guest.attributes.length }}개</span
                >
              </td>
              <td
                class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                @click.stop
              >
                <button
                  @click="editGuest(guest)"
                  class="text-primary-600 hover:text-primary-900 mr-3"
                >
                  수정
                </button>
                <button
                  @click="deleteGuest(guest.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  삭제
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 참석자 생성/수정 모달 -->
    <div
      v-if="showCreateModal || editingGuest"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="closeGuestModal"
    >
      <div
        class="bg-white rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto"
      >
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingGuest ? '참석자 수정' : '참석자 추가' }}
          </h2>

          <div class="space-y-4">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">이름 *</label>
                <input
                  v-model="guestForm.guestName"
                  type="text"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">전화번호 *</label>
                <input
                  v-model="guestForm.telephone"
                  type="text"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">부서</label>
                <input
                  v-model="guestForm.corpPart"
                  type="text"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1"
                  >주민등록번호</label
                >
                <input
                  v-model="guestForm.residentNumber"
                  type="text"
                  placeholder="000000-0000000"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">소속</label>
              <input
                v-model="guestForm.affiliation"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
              />
            </div>

            <div>
              <label class="block text-sm font-medium mb-1"
                >초기 비밀번호 (선택)</label
              >
              <input
                v-model="guestForm.password"
                type="password"
                placeholder="미입력 시 주민등록번호 앞 6자리 자동 설정"
                class="w-full px-3 py-2 border rounded-lg"
              />
              <p class="text-xs text-gray-500 mt-1">
                * 주민등록번호가 없거나 비밀번호를 지정하지 않으면 기본 비밀번호
                "123456"이 설정됩니다.
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium mb-2">속성 정보</label>
              <div class="space-y-3">
                <!-- 템플릿 기반 속성 -->
                <div
                  v-for="template in attributeTemplates"
                  :key="template.id"
                  class="space-y-2"
                >
                  <label class="block text-sm font-medium text-gray-700">{{
                    template.attributeKey
                  }}</label>
                  <select
                    v-if="template.attributeValues"
                    v-model="
                      guestForm.templateAttributes[template.attributeKey]
                    "
                    class="w-full px-3 py-2 border rounded-lg"
                  >
                    <option value="">선택하세요</option>
                    <option
                      v-for="value in parseAttributeValues(
                        template.attributeValues,
                      )"
                      :key="value"
                      :value="value"
                    >
                      {{ value }}
                    </option>
                  </select>
                  <input
                    v-else
                    v-model="
                      guestForm.templateAttributes[template.attributeKey]
                    "
                    type="text"
                    class="w-full px-3 py-2 border rounded-lg"
                    :placeholder="`${template.attributeKey} 입력`"
                  />
                </div>

                <!-- 추가 속성 (수기) -->
                <div class="pt-3 border-t">
                  <p class="text-sm text-gray-600 mb-2">
                    추가 속성 (수기 입력)
                  </p>
                  <div
                    v-for="(attr, idx) in guestForm.customAttributes"
                    :key="idx"
                    class="flex flex-col sm:flex-row gap-2 mb-2"
                  >
                    <input
                      v-model="attr.key"
                      placeholder="키 (예: 호차)"
                      class="flex-1 px-3 py-2 border rounded-lg"
                    />
                    <input
                      v-model="attr.value"
                      placeholder="값"
                      class="flex-1 px-3 py-2 border rounded-lg"
                    />
                    <button
                      @click="guestForm.customAttributes.splice(idx, 1)"
                      class="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg whitespace-nowrap overflow-hidden text-ellipsis"
                    >
                      삭제
                    </button>
                  </div>
                  <button
                    @click="
                      guestForm.customAttributes.push({ key: '', value: '' })
                    "
                    class="w-full py-2 border-2 border-dashed rounded-lg text-sm text-gray-600 hover:bg-gray-50 whitespace-nowrap overflow-hidden text-ellipsis"
                  >
                    + 속성 추가
                  </button>
                </div>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium mb-2">일정 배정</label>
              <div
                v-if="availableTemplates.length === 0"
                class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
              >
                일정 템플릿이 없습니다. 먼저 일정 관리에서 템플릿을 생성하세요.
              </div>
              <div v-else>
                <!-- 다른 참석자 일정 복사버튼 -->
                <div class="flex justify-between items-center mb-2">
                  <span class="text-sm text-gray-600"
                    >선택: {{ guestForm.scheduleTemplateIds.length }}개</span
                  >
                  <button
                    @click="showCopyScheduleModal = true"
                    class="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 whitespace-nowrap overflow-hidden text-ellipsis"
                  >
                    <svg
                      class="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
                      />
                    </svg>
                    다른 참석자 일정 복사
                  </button>
                </div>
                <div
                  class="space-y-2 max-h-60 overflow-y-auto border rounded p-2"
                >
                  <label
                    v-for="template in availableTemplates"
                    :key="template.id"
                    class="flex items-start gap-2 p-2 border rounded hover:bg-gray-50 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      :value="template.id"
                      v-model="guestForm.scheduleTemplateIds"
                      class="rounded mt-1"
                    />
                    <div class="flex-1">
                      <div class="font-medium">{{ template.courseName }}</div>
                      <div
                        v-if="template.description"
                        class="text-xs text-gray-500"
                      >
                        {{ template.description }}
                      </div>
                      <div class="text-xs text-gray-400 mt-1">
                        일정 {{ template.scheduleItems?.length || 0 }}개
                      </div>
                    </div>
                  </label>
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button
              @click="closeGuestModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
            <button
              @click="saveGuest"
              class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
            >
              저장
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 참석자 상세 모달 -->
    <div
      v-if="showDetailModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="closeDetailModal"
    >
      <div
        class="bg-white rounded-lg w-full max-w-4xl max-h-[90vh] overflow-y-auto"
      >
        <div class="p-6">
          <div class="flex justify-between items-start mb-6">
            <h2 class="text-2xl font-bold">{{ guestDetail?.guestName }}</h2>
            <button
              @click="closeDetailModal"
              class="text-gray-500 hover:text-gray-700"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <h3 class="font-semibold mb-3">기본 정보</h3>
              <dl class="space-y-2 text-sm">
                <div>
                  <dt class="text-gray-500 inline">전화번호:</dt>
                  <dd class="inline ml-2">{{ guestDetail?.telephone }}</dd>
                </div>
                <div>
                  <dt class="text-gray-500 inline">부서:</dt>
                  <dd class="inline ml-2">
                    {{ guestDetail?.corpPart || '-' }}
                  </dd>
                </div>
                <div>
                  <dt class="text-gray-500 inline">주민번호:</dt>
                  <dd class="inline ml-2">
                    {{ guestDetail?.residentNumber || '-' }}
                  </dd>
                </div>
                <div>
                  <dt class="text-gray-500 inline">소속:</dt>
                  <dd class="inline ml-2">
                    {{ guestDetail?.affiliation || '-' }}
                  </dd>
                </div>
              </dl>
            </div>

            <div
              v-if="
                guestDetail?.attributes &&
                Object.keys(guestDetail.attributes).length > 0
              "
            >
              <h3 class="font-semibold mb-3">속성 정보</h3>
              <dl class="space-y-2 text-sm">
                <div v-for="(value, key) in guestDetail.attributes" :key="key">
                  <dt class="text-gray-500 inline">{{ key }}:</dt>
                  <dd class="inline ml-2 font-medium">{{ value }}</dd>
                </div>
              </dl>
            </div>
          </div>

          <div
            v-if="guestDetail?.schedules && guestDetail.schedules.length > 0"
          >
            <h3 class="font-semibold mb-3">배정된 일정</h3>
            <div class="space-y-4">
              <div
                v-for="schedule in guestDetail.schedules"
                :key="schedule.scheduleTemplateId"
                class="border rounded-lg overflow-hidden"
              >
                <div class="bg-gray-50 px-4 py-3 border-b">
                  <h4 class="font-semibold">{{ schedule.courseName }}</h4>
                  <p v-if="schedule.description" class="text-sm text-gray-600">
                    {{ schedule.description }}
                  </p>
                </div>
                <div class="p-4">
                  <div class="space-y-3">
                    <div
                      v-for="item in schedule.items"
                      :key="item.id"
                      class="flex gap-3 p-3 bg-gray-50 rounded-lg"
                    >
                      <div class="flex-shrink-0 w-24 text-sm">
                        <div class="font-medium">
                          {{ formatDate(item.scheduleDate) }}
                        </div>
                        <div class="text-gray-600">{{ item.startTime }}</div>
                      </div>
                      <div class="flex-1">
                        <p class="font-medium">{{ item.title }}</p>
                        <p v-if="item.location" class="text-sm text-gray-500">
                          📍 {{ item.location }}
                        </p>
                        <p
                          v-if="item.content"
                          class="text-sm text-gray-600 mt-1 whitespace-pre-wrap"
                        >
                          {{ item.content }}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center text-gray-500 py-8 border rounded-lg">
            배정된 일정이 없습니다
          </div>
        </div>
      </div>
    </div>

    <!-- 일정 복사 모달 -->
    <div
      v-if="showCopyScheduleModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="
        () => {
          showCopyScheduleModal = false
          searchQuery = ''
        }
      "
    >
      <div class="bg-white rounded-lg w-full max-w-lg">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">다른 참석자 일정 복사</h2>

          <div class="mb-4">
            <input
              v-model="searchQuery"
              type="text"
              placeholder="참석자 이름 검색..."
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>

          <div class="space-y-2 max-h-96 overflow-y-auto">
            <div
              v-for="guest in filteredGuestsForCopy"
              :key="guest.id"
              class="p-3 border rounded-lg hover:bg-gray-50 cursor-pointer"
              @click="copyScheduleFromGuest(guest)"
            >
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium">{{ guest.guestName }}</p>
                  <p class="text-sm text-gray-500">{{ guest.telephone }}</p>
                  <div
                    v-if="guest.scheduleTemplates.length > 0"
                    class="flex flex-wrap gap-1 mt-2"
                  >
                    <span
                      v-for="st in guest.scheduleTemplates"
                      :key="st.scheduleTemplateId"
                      class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs"
                    >
                      {{ st.courseName }}
                    </span>
                  </div>
                  <p v-else class="text-xs text-gray-400 mt-1">
                    배정된 일정 없음
                  </p>
                </div>
                <button
                  v-if="guest.scheduleTemplates.length > 0"
                  class="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 whitespace-nowrap overflow-hidden text-ellipsis"
                >
                  복사
                </button>
              </div>
            </div>
          </div>

          <div class="flex justify-end mt-6">
            <button
              @click="
                showCopyScheduleModal = false
                searchQuery = ''
              "
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 대량 일정 배정 모달 -->
    <div
      v-if="showBulkAssignModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="closeBulkAssignModal"
    >
      <div class="bg-white rounded-lg w-full max-w-2xl">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">일정 일괄 배정</h2>

          <div class="mb-4 p-4 bg-blue-50 rounded-lg">
            <p class="text-sm font-medium text-blue-900">
              선택된 참석자: {{ selectedGuests.length }}명
            </p>
            <div class="flex flex-wrap gap-2 mt-2">
              <span
                v-for="id in selectedGuests.slice(0, 5)"
                :key="id"
                class="px-2 py-1 bg-white text-sm rounded"
              >
                {{ guests.find((g) => g.id === id)?.guestName }}
              </span>
              <span
                v-if="selectedGuests.length > 5"
                class="px-2 py-1 bg-white text-sm rounded"
              >
                +{{ selectedGuests.length - 5 }}명
              </span>
            </div>
          </div>

          <div class="mb-4">
            <label class="block text-sm font-medium mb-2"
              >배정할 일정 선택</label
            >
            <div
              class="space-y-2 max-h-96 overflow-y-auto border rounded-lg p-3"
            >
              <label
                v-for="template in availableTemplates"
                :key="template.id"
                class="flex items-start gap-2 p-3 border rounded hover:bg-gray-50 cursor-pointer"
              >
                <input
                  type="checkbox"
                  :value="template.id"
                  v-model="bulkAssignTemplateIds"
                  class="rounded mt-1"
                />
                <div class="flex-1">
                  <div class="font-medium">{{ template.courseName }}</div>
                  <div
                    v-if="template.description"
                    class="text-sm text-gray-600"
                  >
                    {{ template.description }}
                  </div>
                  <div class="text-xs text-gray-500 mt-1">
                    일정 {{ template.scheduleItems?.length || 0 }}개 • 참석자
                    {{ template.guestCount || 0 }}명
                  </div>
                </div>
              </label>
            </div>
          </div>

          <div class="mb-4">
            <label class="flex items-center gap-2">
              <input
                type="checkbox"
                v-model="bulkAssignReplace"
                class="rounded"
              />
              <span class="text-sm text-gray-700"
                >기존 일정을 대체 (체크 시 기존 일정 삭제 후 새 일정 배정)</span
              >
            </label>
          </div>

          <div class="flex justify-end space-x-3">
            <button
              @click="closeBulkAssignModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
            <button
              @click="executeBulkAssign"
              :disabled="bulkAssignTemplateIds.length === 0"
              class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed whitespace-nowrap overflow-hidden text-ellipsis"
            >
              {{ selectedGuests.length }}명에게 일정 배정
            </button>
          </div>
        </div>
      </div>
    </div>
    <!-- 파일의 다른 모달들 뒤에 추가 -->
    <!-- 속성 일괄 매핑 모달 -->
    <div
      v-if="showBulkAttributeModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click.self="closeBulkAttributeModal"
    >
      <div
        class="bg-white rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto"
      >
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            속성 일괄 매핑 ({{ selectedGuests.length }}명)
          </h2>

          <div class="mb-4 p-3 bg-blue-50 rounded-lg">
            <p class="text-sm text-blue-800">
              선택한 {{ selectedGuests.length }}명의 참석자에게 동일한 속성을
              설정합니다.
            </p>
          </div>

          <div class="space-y-4">
            <div
              v-for="template in attributeTemplates"
              :key="template.id"
              class="space-y-2"
            >
              <label class="block text-sm font-medium text-gray-700">
                {{ template.attributeKey }}
              </label>

              <!-- 선택 값이 있는 경우 드롭다운 -->
              <select
                v-if="template.attributeValues"
                v-model="bulkAttributeForm[template.attributeKey]"
                class="w-full px-3 py-2 border rounded-lg"
              >
                <option value="">선택하세요</option>
                <option
                  v-for="value in parseAttributeValues(
                    template.attributeValues,
                  )"
                  :key="value"
                  :value="value"
                >
                  {{ value }}
                </option>
              </select>

              <!-- 선택 값이 없는 경우 텍스트 입력 -->
              <input
                v-else
                v-model="bulkAttributeForm[template.attributeKey]"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                :placeholder="`${template.attributeKey} 입력`"
              />
            </div>

            <!-- 미리보기 -->
            <div class="mt-6 p-4 bg-gray-50 rounded-lg">
              <h4 class="text-sm font-semibold mb-2">미리보기</h4>
              <div class="space-y-2 max-h-40 overflow-y-auto">
                <div
                  v-for="guestId in selectedGuests.slice(0, 5)"
                  :key="guestId"
                  class="text-sm"
                >
                  <span class="font-medium">{{
                    getGuestNameById(guestId)
                  }}</span>
                  <span class="text-gray-600 ml-2">
                    →
                    <span
                      v-for="(value, key) in bulkAttributeForm"
                      :key="key"
                      class="ml-1"
                    >
                      <template v-if="value"> {{ key }}: {{ value }} </template>
                    </span>
                  </span>
                </div>
                <div
                  v-if="selectedGuests.length > 5"
                  class="text-xs text-gray-500"
                >
                  ... 외 {{ selectedGuests.length - 5 }}명
                </div>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button
              @click="closeBulkAttributeModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
            <button
              @click="submitBulkAttribute"
              :disabled="submittingAttribute"
              class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 whitespace-nowrap overflow-hidden text-ellipsis"
            >
              {{ submittingAttribute ? '저장 중...' : '일괄 저장' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const guests = ref([])
const availableTemplates = ref([])
const attributeTemplates = ref([])
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const showCopyScheduleModal = ref(false)
const showBulkAssignModal = ref(false)
const editingGuest = ref(null)
const guestDetail = ref(null)
const loading = ref(true)
const searchQuery = ref('')
const selectedGuests = ref([])
const bulkAssignTemplateIds = ref([])
const bulkAssignReplace = ref(false)
const showBulkAttributeModal = ref(false)
const bulkAttributeForm = ref({})
const submittingAttribute = ref(false)

const guestForm = ref({
  guestName: '',
  telephone: '',
  corpPart: '',
  residentNumber: '',
  affiliation: '',
  password: '',
  scheduleTemplateIds: [],
  templateAttributes: {},
  customAttributes: [],
})

const filteredGuestsForCopy = computed(() => {
  if (!searchQuery.value) return guests.value
  return guests.value.filter((g) =>
    g.guestName.toLowerCase().includes(searchQuery.value.toLowerCase()),
  )
})

const loadGuests = async () => {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    console.log('✅ Guests loaded:', response.data)
    guests.value = response.data
  } catch (error) {
    console.error('❌ Failed to load guests:', error)
    console.error('Error details:', error.response?.data)
  } finally {
    loading.value = false
  }
}

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/schedule-templates`,
    )
    console.log('✅ Templates loaded:', response.data)
    availableTemplates.value = response.data
  } catch (error) {
    console.error('❌ Failed to load templates:', error)
  }
}

const loadAttributeTemplates = async () => {
  try {
    const response = await apiClient.get(
      `/attributetemplate/conventions/${props.conventionId}`,
    )
    attributeTemplates.value = response.data
  } catch (error) {
    console.error('Failed to load attribute templates:', error)
  }
}

const parseAttributeValues = (valuesStr) => {
  if (!valuesStr) return []
  try {
    return JSON.parse(valuesStr)
  } catch {
    return []
  }
}

const editGuest = (guest) => {
  editingGuest.value = guest

  // 기존 속성을 템플릿/커스텀으로 분리
  const templateAttrs = {}
  const customAttrs = []
  const templateKeys = attributeTemplates.value.map((t) => t.attributeKey)

  guest.attributes.forEach((attr) => {
    if (templateKeys.includes(attr.attributeKey)) {
      templateAttrs[attr.attributeKey] = attr.attributeValue
    } else {
      customAttrs.push({ key: attr.attributeKey, value: attr.attributeValue })
    }
  })

  guestForm.value = {
    guestName: guest.guestName,
    telephone: guest.telephone,
    corpPart: guest.corpPart || '',
    residentNumber: guest.residentNumber || '',
    affiliation: guest.affiliation || '',
    password: '',
    scheduleTemplateIds: guest.scheduleTemplates.map(
      (st) => st.scheduleTemplateId,
    ),
    templateAttributes: templateAttrs,
    customAttributes: customAttrs,
  }
}

const closeGuestModal = () => {
  showCreateModal.value = false
  editingGuest.value = null
  guestForm.value = {
    guestName: '',
    telephone: '',
    corpPart: '',
    residentNumber: '',
    affiliation: '',
    password: '',
    scheduleTemplateIds: [],
    templateAttributes: {},
    customAttributes: [],
  }
}

const saveGuest = async () => {
  try {
    // 템플릿 + 커스텀 속성 병합
    const attributes = {}

    // 템플릿 속성
    Object.entries(guestForm.value.templateAttributes).forEach(
      ([key, value]) => {
        if (value) {
          attributes[key] = value
        }
      },
    )

    // 커스텀 속성
    guestForm.value.customAttributes.forEach((attr) => {
      if (attr.key && attr.value) {
        attributes[attr.key] = attr.value
      }
    })

    const data = {
      name: guestForm.value.guestName,
      phone: guestForm.value.telephone,
      corpPart: guestForm.value.corpPart,
      residentNumber: guestForm.value.residentNumber,
      affiliation: guestForm.value.affiliation,
      password: guestForm.value.password,
      attributes: Object.keys(attributes).length > 0 ? attributes : null,
    }

    let guestId
    if (editingGuest.value) {
      await apiClient.put(`/admin/guests/${editingGuest.value.id}`, data)
      guestId = editingGuest.value.id
    } else {
      const response = await apiClient.post(
        `/admin/conventions/${props.conventionId}/guests`,
        data,
      )
      guestId = response.data.id
    }

    // 일정 배정
    await apiClient.post(`/admin/guests/${guestId}/schedules`, {
      scheduleTemplateIds: guestForm.value.scheduleTemplateIds,
    })

    await loadGuests()
    closeGuestModal()
  } catch (error) {
    console.error('Failed to save guest:', error)
    alert('저장 실패: ' + (error.response?.data?.message || error.message))
  }
}

const deleteGuest = async (id) => {
  if (!confirm('참석자를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/guests/${id}`)
    await loadGuests()
  } catch (error) {
    console.error('Failed to delete guest:', error)
    alert('삭제 실패')
  }
}

const showGuestDetail = async (guestId) => {
  try {
    const response = await apiClient.get(`/admin/guests/${guestId}/detail`)
    console.log('✅ Guest detail loaded:', response.data)
    guestDetail.value = response.data
    showDetailModal.value = true
  } catch (error) {
    console.error('❌ Failed to load guest detail:', error)
  }
}

const closeDetailModal = () => {
  showDetailModal.value = false
  guestDetail.value = null
}

const copyScheduleFromGuest = (guest) => {
  if (guest.scheduleTemplates.length === 0) {
    alert('이 참석자는 배정된 일정이 없습니다.')
    return
  }

  // 일정 복사
  guestForm.value.scheduleTemplateIds = guest.scheduleTemplates.map(
    (st) => st.scheduleTemplateId,
  )
  showCopyScheduleModal.value = false
  searchQuery.value = ''
  alert(
    `${guest.guestName}님의 일정 ${guest.scheduleTemplates.length}개를 복사했습니다.`,
  )
}

const toggleSelectAll = (event) => {
  if (event.target.checked) {
    selectedGuests.value = guests.value.map((g) => g.id)
  } else {
    selectedGuests.value = []
  }
}

const closeBulkAssignModal = () => {
  showBulkAssignModal.value = false
  bulkAssignTemplateIds.value = []
  bulkAssignReplace.value = false
}

const executeBulkAssign = async () => {
  if (bulkAssignTemplateIds.value.length === 0) {
    alert('배정할 일정을 선택해주세요.')
    return
  }

  const confirmMessage = bulkAssignReplace.value
    ? `${selectedGuests.value.length}명의 기존 일정을 모두 삭제하고 새 일정을 배정하시겠습니까?`
    : `${selectedGuests.value.length}명에게 일정을 추가로 배정하시겠습니까?`

  if (!confirm(confirmMessage)) return

  try {
    loading.value = true
    let successCount = 0
    let failCount = 0

    for (const guestId of selectedGuests.value) {
      try {
        if (bulkAssignReplace.value) {
          // 대체 모드: 기존 일정 삭제 후 새 일정 배정
          await apiClient.post(`/admin/guests/${guestId}/schedules`, {
            scheduleTemplateIds: bulkAssignTemplateIds.value,
          })
        } else {
          // 추가 모드: 기존 일정 + 새 일정
          const guest = guests.value.find((g) => g.id === guestId)
          const existingIds =
            guest?.scheduleTemplates.map((st) => st.scheduleTemplateId) || []
          const mergedIds = [
            ...new Set([...existingIds, ...bulkAssignTemplateIds.value]),
          ]

          await apiClient.post(`/admin/guests/${guestId}/schedules`, {
            scheduleTemplateIds: mergedIds,
          })
        }
        successCount++
      } catch (error) {
        console.error(`Failed to assign schedules to guest ${guestId}:`, error)
        failCount++
      }
    }

    await loadGuests()
    await loadTemplates()
    closeBulkAssignModal()
    selectedGuests.value = []

    alert(`완료! 성공: ${successCount}명, 실패: ${failCount}명`)
  } catch (error) {
    console.error('Bulk assign failed:', error)
    alert('일괄 배정 실패: ' + (error.response?.data?.message || error.message))
  } finally {
    loading.value = false
  }
}

// 메서드 추가
const closeBulkAttributeModal = () => {
  showBulkAttributeModal.value = false
  bulkAttributeForm.value = {}
}

const getGuestNameById = (guestId) => {
  const guest = guests.value.find((g) => g.id === guestId)
  return guest ? guest.guestName : ''
}

const submitBulkAttribute = async () => {
  // 빈 값 제거
  const filteredAttributes = Object.entries(bulkAttributeForm.value)
    .filter(([_, value]) => value !== '')
    .reduce((acc, [key, value]) => {
      acc[key] = value
      return acc
    }, {})

  if (Object.keys(filteredAttributes).length === 0) {
    alert('최소 하나의 속성을 입력해주세요')
    return
  }

  if (
    !confirm(
      `${selectedGuests.value.length}명의 참석자에게 속성을 일괄 매핑하시겠습니까?`,
    )
  ) {
    return
  }

  submittingAttribute.value = true

  try {
    const payload = {
      conventionId: props.conventionId,
      guestMappings: selectedGuests.value.map((guestId) => ({
        guestId,
        attributes: filteredAttributes,
      })),
    }

    const response = await apiClient.post(
      '/guest/bulk-assign-attributes',
      payload,
    )

    if (response.data.success) {
      alert(response.data.message)
      closeBulkAttributeModal()
      selectedGuests.value = []
      await loadGuests() // 목록 새로고침
    } else {
      alert(response.data.message)
    }
  } catch (error) {
    console.error('Failed to bulk assign attributes:', error)
    alert(
      '속성 할당에 실패했습니다: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    submittingAttribute.value = false
  }
}

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}/${date.getDate()}`
}

onMounted(() => {
  loadGuests()
  loadTemplates()
  loadAttributeTemplates()
})
</script>
