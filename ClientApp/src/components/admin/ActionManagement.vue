<template>
  <div>
    <AdminPageHeader
      title="참여자 액션 관리"
      description="참석자가 완료해야 하는 필수 액션을 관리합니다"
    >
      <AdminButton :icon="Plus" @click="openCreateModal">액션 추가</AdminButton>
    </AdminPageHeader>

    <!-- BehaviorType 필터 버튼 -->
    <div class="mb-6 flex flex-wrap gap-2">
      <button
        v-for="filter in behaviorFilters"
        :key="filter.value"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === filter.value
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
        @click="selectedBehaviorType = filter.value"
      >
        {{ filter.label }}
      </button>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12 mt-6">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <!-- 액션 목록 -->
    <div v-else-if="filteredActions.length > 0" class="grid gap-4 mt-6">
      <div
        v-for="action in filteredActions"
        :key="action.id"
        class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 sm:p-5 hover:shadow-md transition-shadow overflow-hidden"
      >
        <div
          class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4"
        >
          <div class="flex-1 min-w-0">
            <div class="flex flex-wrap items-center gap-2 mb-2">
              <h3
                class="text-base sm:text-lg font-bold text-gray-900 break-words"
              >
                {{ action.title }}
              </h3>
              <span
                class="px-2 py-1 text-xs font-medium rounded-full flex-shrink-0"
                :class="
                  action.isActive
                    ? 'bg-green-100 text-green-700'
                    : 'bg-gray-100 text-gray-600'
                "
                >{{ action.isActive ? '활성' : '비활성' }}</span
              >
              <span
                class="px-2 py-1 bg-primary-100 text-primary-700 text-xs font-medium rounded-full flex-shrink-0"
                >순서: {{ action.orderNum }}</span
              >
            </div>

            <div class="space-y-2 text-sm">
              <div class="flex items-center text-gray-600 flex-wrap gap-2">
                <Tag class="w-4 h-4 flex-shrink-0" />
                <span class="font-mono text-xs bg-gray-100 px-2 py-1 rounded">{{
                  getBehaviorTypeName(action.behaviorType)
                }}</span>
                <span
                  v-if="action.actionCategory"
                  class="font-mono text-xs bg-blue-50 text-blue-700 px-2 py-1 rounded"
                  >{{ action.actionCategory }}</span
                >
                <template
                  v-if="
                    action.behaviorType === 'FormBuilder' && action.targetId
                  "
                >
                  <span class="text-gray-400">|</span>
                  <span class="text-xs"
                    >Form ID:
                    <strong class="font-semibold text-gray-700">{{
                      action.targetId
                    }}</strong></span
                  >
                </template>
              </div>
              <div
                v-if="action.deadline"
                class="flex items-center text-gray-600 flex-wrap gap-2"
              >
                <Clock class="w-4 h-4 flex-shrink-0" />
                <span>마감: {{ formatDateTime(action.deadline) }}</span>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            <button
              :title="action.isActive ? '비활성화' : '활성화'"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors flex-shrink-0"
              @click="toggleAction(action)"
            >
              <CheckCircle
                v-if="action.isActive"
                class="w-5 h-5 text-green-600"
              /><XCircle v-else class="w-5 h-5 text-gray-400" />
            </button>
            <button
              class="p-2 hover:bg-primary-50 text-primary-600 rounded-lg transition-colors flex-shrink-0"
              title="수정"
              @click="openEditModal(action)"
            >
              <Pencil class="w-5 h-5" />
            </button>
            <button
              class="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors flex-shrink-0"
              title="삭제"
              @click="deleteAction(action)"
            >
              <Trash2 class="w-5 h-5" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <div
      v-else
      class="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-300 mt-6"
    >
      <h3 class="text-lg font-medium text-gray-900 mb-2">
        등록된 액션이 없습니다
      </h3>
      <p class="text-gray-600 mb-4">참석자가 완료해야 할 액션을 추가해보세요</p>
      <button
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
        @click="openCreateModal"
      >
        첫 액션 만들기
      </button>
    </div>

    <!-- 모달 -->
    <BaseModal :is-open="showModal" max-width="lg" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-bold text-gray-900">
          {{
            selectedPresetId === 'checklist'
              ? '체크리스트 관리'
              : editingAction
                ? '액션 수정'
                : modalStep === 0
                  ? '액션 유형 선택'
                  : `${selectedPreset?.label} 추가`
          }}
        </h3>
      </template>

      <template #body>
        <!-- Step 0: 프리셋 선택 (신규 생성 시만) -->
        <div v-if="modalStep === 0 && !editingAction" class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div
              v-for="preset in presets"
              :key="preset.id"
              class="p-4 border-2 rounded-lg cursor-pointer transition-all hover:border-primary-400 hover:bg-primary-50"
              :class="
                selectedPresetId === preset.id
                  ? 'border-primary-600 bg-primary-50'
                  : 'border-gray-200'
              "
              @click="selectPreset(preset)"
            >
              <div class="flex items-center gap-3">
                <component
                  :is="preset.icon"
                  class="w-6 h-6 text-primary-600 flex-shrink-0"
                />
                <div>
                  <div class="font-semibold text-sm text-gray-900">
                    {{ preset.label }}
                  </div>
                  <div class="text-xs text-gray-500 mt-0.5">
                    {{ preset.description }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 고급 설정 링크 -->
          <div class="pt-2 border-t border-gray-200">
            <button
              class="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700 transition-colors"
              @click="openExpertMode"
            >
              <component :is="expertPreset.icon" class="w-4 h-4" />
              고급 설정 (모든 옵션 직접 지정)
            </button>
          </div>
        </div>

        <!-- Step 1-B: 체크리스트 다중 관리 -->
        <div
          v-else-if="
            modalStep === 1 && selectedPresetId === 'checklist' && !isExpertMode
          "
          class="space-y-5"
        >
          <!-- 공통 설정 -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-1.5"
                >표시 위치</label
              >
              <select
                v-model="checklistShared.targetLocation"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
              >
                <option value="HOME_SUB_HEADER">홈 - 배너 아래</option>
                <option value="HOME_CONTENT_TOP">홈 - 콘텐츠 상단</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-1.5"
                >공통 마감일 (선택)</label
              >
              <input
                v-model="checklistShared.deadline"
                type="datetime-local"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
              <p class="text-xs text-gray-500 mt-1">
                개별 마감일이 없는 항목에 적용됩니다
              </p>
            </div>
          </div>

          <!-- 항목 리스트 -->
          <div class="space-y-2">
            <div class="flex items-center justify-between mb-1">
              <label class="text-sm font-semibold text-gray-700"
                >체크리스트 항목</label
              >
              <span class="text-xs text-gray-500"
                >{{ checklistRows.length }}개</span
              >
            </div>
            <div
              v-for="(row, idx) in checklistRows"
              :key="row._key"
              class="flex items-start gap-2 p-3 bg-gray-50 rounded-lg border border-gray-200"
            >
              <span
                class="flex-shrink-0 w-6 h-6 rounded-full bg-primary-100 text-primary-700 text-xs font-bold flex items-center justify-center mt-1"
                >{{ idx + 1 }}</span
              >
              <div class="flex-1 space-y-2">
                <input
                  v-model="row.title"
                  type="text"
                  :placeholder="`항목 ${idx + 1} 제목`"
                  class="w-full px-3 py-1.5 border border-gray-300 rounded-md text-sm"
                />
                <div class="flex items-center gap-3">
                  <input
                    v-model="row.deadline"
                    type="datetime-local"
                    class="flex-1 px-3 py-1.5 border border-gray-300 rounded-md text-sm"
                    placeholder="개별 마감일"
                  />
                  <label
                    v-if="row.id"
                    class="flex items-center gap-1.5 text-xs text-gray-600"
                  >
                    <input
                      v-model="row.isActive"
                      type="checkbox"
                      class="w-3.5 h-3.5 text-primary-600 rounded"
                    />
                    활성
                  </label>
                </div>
              </div>
              <button
                class="flex-shrink-0 p-1.5 text-red-500 hover:bg-red-50 rounded transition-colors mt-1"
                title="삭제"
                @click="removeChecklistRow(idx)"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- 항목 추가 -->
          <button
            type="button"
            class="w-full py-2 border-2 border-dashed border-gray-300 rounded-lg text-sm text-gray-500 hover:border-primary-400 hover:text-primary-600 transition-colors"
            @click="addChecklistRow"
          >
            + 항목 추가
          </button>

          <div
            v-if="errorMessage"
            class="p-3 bg-red-50 text-red-800 rounded-lg text-sm"
          >
            {{ errorMessage }}
          </div>
        </div>

        <!-- Step 1-A: 일반 프리셋 폼 -->
        <div
          v-else-if="modalStep === 1 && selectedPreset && !isExpertMode"
          class="space-y-5"
        >
          <template v-for="field in visibleFields" :key="field.key">
            <!-- text -->
            <div v-if="field.type === 'text'">
              <label class="block text-sm font-semibold text-gray-700 mb-1.5">
                {{ field.label }}
                <span v-if="field.required" class="text-red-500">*</span>
              </label>
              <input
                v-model="formData[field.key]"
                type="text"
                :placeholder="field.placeholder || ''"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>

            <!-- select -->
            <div v-else-if="field.type === 'select'">
              <label class="block text-sm font-semibold text-gray-700 mb-1.5">
                {{ field.label }}
                <span v-if="field.required" class="text-red-500">*</span>
              </label>
              <select
                v-model="formData[field.key]"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
              >
                <option
                  v-for="opt in field.options"
                  :key="opt.value"
                  :value="opt.value"
                >
                  {{ opt.label }}
                </option>
              </select>
            </div>

            <!-- datetime-local -->
            <div v-else-if="field.type === 'datetime-local'">
              <label class="block text-sm font-semibold text-gray-700 mb-1.5">
                {{ field.label }}
              </label>
              <input
                v-model="formData[field.key]"
                type="datetime-local"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
            </div>

            <!-- checkbox -->
            <div
              v-else-if="field.type === 'checkbox'"
              class="flex items-center gap-2"
            >
              <input
                :id="`field-${field.key}`"
                v-model="formData[field.key]"
                type="checkbox"
                class="w-4 h-4 text-primary-600 rounded"
              />
              <label :for="`field-${field.key}`" class="text-sm text-gray-700">
                {{ field.label }}
              </label>
            </div>

            <!-- image (파일 업로드 or 외부 URL) -->
            <div v-else-if="field.type === 'image'">
              <label class="block text-sm font-semibold text-gray-700 mb-1.5">
                {{ field.label }}
                <span v-if="field.required" class="text-red-500">*</span>
              </label>
              <div class="flex gap-2 mb-2">
                <button
                  :class="[
                    'px-3 py-1 text-xs rounded-md transition-colors',
                    (formData[field.key + '_mode'] || 'upload') === 'upload'
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
                  ]"
                  type="button"
                  @click="formData[field.key + '_mode'] = 'upload'"
                >
                  파일 첨부
                </button>
                <button
                  :class="[
                    'px-3 py-1 text-xs rounded-md transition-colors',
                    formData[field.key + '_mode'] === 'url'
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
                  ]"
                  type="button"
                  @click="formData[field.key + '_mode'] = 'url'"
                >
                  외부 URL
                </button>
              </div>
              <!-- 파일 업로드 -->
              <div
                v-if="(formData[field.key + '_mode'] || 'upload') === 'upload'"
              >
                <input
                  type="file"
                  accept="image/*"
                  class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
                  @change="handleImageUpload($event, field.key)"
                />
              </div>
              <!-- 외부 URL 입력 -->
              <div v-else>
                <input
                  v-model="formData[field.key]"
                  type="text"
                  placeholder="https://..."
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
              <!-- 미리보기 -->
              <div
                v-if="formData[field.key]"
                class="mt-2 p-2 border border-gray-200 rounded-lg"
              >
                <img
                  :src="formData[field.key]"
                  class="max-h-32 rounded object-contain"
                  :alt="field.label"
                />
                <div class="flex items-center justify-between mt-1">
                  <span class="text-xs text-gray-400 truncate max-w-[250px]">{{
                    formData[field.key]
                  }}</span>
                  <button
                    type="button"
                    class="text-xs text-red-500 hover:text-red-700"
                    @click="formData[field.key] = ''"
                  >
                    삭제
                  </button>
                </div>
              </div>
            </div>

            <!-- html (리치 텍스트 에디터) -->
            <div v-else-if="field.type === 'html'">
              <label class="block text-sm font-semibold text-gray-700 mb-1.5">
                {{ field.label }}
                <span v-if="field.required" class="text-red-500">*</span>
              </label>
              <div class="flex gap-2 mb-2">
                <button
                  :class="[
                    'px-3 py-1 text-xs rounded-md transition-colors',
                    htmlEditMode === 'visual'
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
                  ]"
                  type="button"
                  @click="htmlEditMode = 'visual'"
                >
                  에디터
                </button>
                <button
                  :class="[
                    'px-3 py-1 text-xs rounded-md transition-colors',
                    htmlEditMode === 'code'
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
                  ]"
                  type="button"
                  @click="htmlEditMode = 'code'"
                >
                  HTML 코드
                </button>
                <button
                  :class="[
                    'px-3 py-1 text-xs rounded-md transition-colors',
                    htmlEditMode === 'preview'
                      ? 'bg-primary-600 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
                  ]"
                  type="button"
                  @click="htmlEditMode = 'preview'"
                >
                  미리보기
                </button>
              </div>
              <!-- 비주얼 에디터 -->
              <div v-show="htmlEditMode === 'visual'">
                <RichTextEditor
                  ref="richTextEditorRef"
                  v-model="formData[field.key]"
                  height="250px"
                />
              </div>
              <!-- HTML 코드 직접 편집 -->
              <textarea
                v-if="htmlEditMode === 'code'"
                v-model="formData[field.key]"
                rows="10"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg font-mono text-sm"
                placeholder="<h2>제목</h2><p>내용을 입력하세요</p>"
              ></textarea>
              <!-- 미리보기 -->
              <div
                v-else
                class="border border-gray-300 rounded-lg p-4 min-h-[150px] bg-white prose prose-sm max-w-none"
                v-html="
                  formData[field.key] ||
                  '<p class=\'text-gray-400\'>내용이 없습니다</p>'
                "
              ></div>
            </div>
          </template>

          <!-- 정렬 순서 (공통) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-1.5"
              >정렬 순서</label
            >
            <input
              v-model.number="formData.orderNum"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <!-- 활성화 (수정 시) -->
          <div v-if="editingAction" class="flex items-center">
            <input
              id="presetIsActive"
              v-model="formData.isActive"
              type="checkbox"
              class="w-4 h-4 text-primary-600 rounded"
            />
            <label for="presetIsActive" class="ml-2 text-sm text-gray-700"
              >활성화</label
            >
          </div>

          <div
            v-if="errorMessage"
            class="p-3 bg-red-50 text-red-800 rounded-lg text-sm"
          >
            {{ errorMessage }}
          </div>
        </div>

        <!-- 고급 설정 (Expert Mode) -->
        <div v-else-if="isExpertMode" class="space-y-6">
          <div
            v-if="editingAction && editingAction.behaviorType === 'GenericForm'"
            class="p-3 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800"
          >
            이 액션은 새로운 '폼 빌더' 타입으로 자동 전환되었습니다.
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >제목 (참석자에게 표시) *</label
            >
            <input
              v-model="expertForm.title"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >실행 방식 (BehaviorType) *</label
            >
            <select
              v-model="expertForm.behaviorType"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
            >
              <option
                v-for="type in behaviorTypes"
                :key="type.value"
                :value="type.value"
              >
                {{ type.label }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >액션 카테고리 *</label
            >
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
              <div
                v-for="category in actionCategories"
                :key="category.key"
                :class="[
                  'p-4 border-2 rounded-lg cursor-pointer transition-all',
                  expertForm.actionCategory === category.key
                    ? 'border-primary-600 bg-primary-50'
                    : 'border-gray-200 hover:border-primary-300',
                ]"
                @click="selectExpertCategory(category)"
              >
                <div class="font-semibold text-sm">
                  {{ category.displayName }}
                </div>
                <div class="text-xs text-gray-600 mt-1">
                  {{ category.description }}
                </div>
              </div>
            </div>
          </div>

          <div v-if="expertForm.actionCategory">
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >표시 위치 *</label
            >
            <select
              v-model="expertForm.targetLocation"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg"
              required
            >
              <option value="">위치를 선택하세요</option>
              <option
                v-for="location in expertFilteredLocations"
                :key="location.key"
                :value="location.key"
              >
                {{ location.displayName }} - {{ location.page }}
              </option>
            </select>
          </div>

          <div
            v-if="expertForm.behaviorType === 'FormBuilder'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">폼 빌더 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 폼 *</label
              >
              <select
                v-model="expertForm.targetId"
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
              >
                <option :value="null" disabled>
                  폼 빌더에서 생성한 폼을 선택하세요
                </option>
                <option
                  v-for="formDef in formDefinitions"
                  :key="formDef.id"
                  :value="formDef.id"
                >
                  {{ formDef.name }} (ID: {{ formDef.id }})
                </option>
              </select>
            </div>
          </div>

          <div
            v-if="expertForm.behaviorType === 'ModuleLink'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">모듈 연동 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 URL *</label
              >
              <div class="flex items-center">
                <span
                  class="px-4 py-2 bg-gray-100 border border-r-0 border-gray-300 rounded-l-lg text-gray-600 font-mono text-sm whitespace-nowrap"
                  >/feature/</span
                >
                <input
                  v-model="expertForm.mapsTo"
                  type="text"
                  required
                  placeholder="surveys/2"
                  class="flex-1 px-4 py-2 border border-gray-300 rounded-r-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  @input="stripFeaturePrefix"
                />
              </div>
            </div>
          </div>

          <div
            v-if="expertForm.behaviorType === 'Link'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">링크 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 URL (MapsTo) *</label
              >
              <input
                v-model="expertForm.mapsTo"
                type="text"
                required
                placeholder="https://example.com 또는 /internal/path"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
            </div>
          </div>

          <div
            v-if="expertForm.behaviorType === 'ShowComponentPopup'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">컴포넌트 팝업 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >팝업 컴포넌트 이름 (MapsTo) *</label
              >
              <input
                v-model="expertForm.mapsTo"
                type="text"
                required
                placeholder="예: MyInfoComponent"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >컴포넌트에 전달할 ID (TargetId)</label
              >
              <input
                v-model.number="expertForm.targetId"
                type="number"
                placeholder="예: 123 (선택 사항)"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
            </div>
          </div>

          <!-- ConfigJson (고급) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >ConfigJson (추가 설정)</label
            >
            <textarea
              v-model="expertForm.configJson"
              rows="4"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg font-mono text-sm"
              placeholder='{"imageUrl": "...", "message": "..."}'
            ></textarea>
            <p class="text-xs text-gray-500 mt-1">
              컴포넌트별 추가 설정을 JSON으로 입력합니다. (배너, 팝업 등)
            </p>
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >마감일</label
            >
            <input
              v-model="expertForm.deadline"
              type="datetime-local"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >정렬 순서</label
            >
            <input
              v-model.number="expertForm.orderNum"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div class="flex items-center">
            <input
              id="expertIsActive"
              v-model="expertForm.isActive"
              type="checkbox"
              class="w-4 h-4 text-primary-600 rounded"
            />
            <label for="expertIsActive" class="ml-2 text-sm text-gray-700"
              >활성화</label
            >
          </div>

          <div
            v-if="errorMessage"
            class="p-3 bg-red-50 text-red-800 rounded-lg text-sm"
          >
            {{ errorMessage }}
          </div>
        </div>
      </template>

      <template #footer>
        <!-- Step 0: 프리셋 선택 -->
        <template v-if="modalStep === 0 && !editingAction">
          <button
            type="button"
            class="px-4 py-2 border rounded-lg"
            @click="closeModal"
          >
            취소
          </button>
          <button
            :disabled="!selectedPresetId"
            class="px-4 py-2 bg-primary-600 text-white rounded-lg disabled:opacity-50"
            @click="goToPresetForm"
          >
            다음
          </button>
        </template>

        <!-- Step 1: 폼 -->
        <template v-else>
          <button
            v-if="!editingAction && !isExpertMode"
            type="button"
            class="px-4 py-2 border rounded-lg"
            @click="modalStep = 0"
          >
            이전
          </button>
          <button
            v-else
            type="button"
            class="px-4 py-2 border rounded-lg"
            @click="closeModal"
          >
            취소
          </button>
          <button
            :disabled="submitting"
            class="px-4 py-2 bg-primary-600 text-white rounded-lg disabled:opacity-50"
            @click="saveAction"
          >
            {{ submitting ? '저장 중...' : '저장' }}
          </button>
        </template>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import {
  Plus,
  Tag,
  Clock,
  CheckCircle,
  XCircle,
  Pencil,
  Trash2,
} from 'lucide-vue-next'
import apiClient from '@/services/api'
import formBuilderService from '@/services/formBuilderService'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import RichTextEditor from '@/components/common/RichTextEditor.vue'
import { ACTION_CATEGORIES } from '@/schemas/actionCategories'
import { getAllowedLocationsForCategory } from '@/schemas/targetLocations'
import {
  ACTION_PRESETS,
  EXPERT_PRESET,
  getPresetById,
} from '@/schemas/actionPresets'

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

// --- 상태 ---
const actions = ref([])
const loading = ref(false)
const showModal = ref(false)
const editingAction = ref(null)
const submitting = ref(false)
const errorMessage = ref('')
const formDefinitions = ref([])

// 모달 상태
const modalStep = ref(0) // 0: 프리셋 선택, 1: 폼 입력
const selectedPresetId = ref(null)
const isExpertMode = ref(false)
const htmlEditMode = ref('visual')
const richTextEditorRef = ref(null)

// 이미지 파일 업로드 핸들러
async function handleImageUpload(event, fieldKey) {
  const file = event.target.files[0]
  if (!file) return

  if (file.size > 10 * 1024 * 1024) {
    alert('10MB 이하 이미지만 업로드 가능합니다.')
    return
  }

  try {
    const fd = new FormData()
    fd.append('file', file)
    const response = await apiClient.post('/file/upload/image', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    formData.value[fieldKey] = response.data.url
  } catch (error) {
    alert('이미지 업로드 실패: ' + (error.response?.data?.message || error.message))
  }
}

// 체크리스트 다중 관리
let checklistKeyCounter = 0
const checklistRows = ref([])
const checklistDeleteQueue = []
const checklistShared = ref({
  targetLocation: 'HOME_CONTENT_TOP',
  deadline: '',
})

// 프리셋 목록
const presets = ACTION_PRESETS
const expertPreset = EXPERT_PRESET
const actionCategories = ACTION_CATEGORIES

const selectedPreset = computed(() =>
  selectedPresetId.value ? getPresetById(selectedPresetId.value) : null,
)

// 프리셋 폼 데이터
const formData = ref({})

// 고급 모드 폼
const expertForm = ref({
  title: '',
  actionCategory: '',
  targetLocation: '',
  mapsTo: '',
  deadline: '',
  orderNum: 0,
  isActive: true,
  behaviorType: 'StatusOnly',
  targetId: null,
  configJson: '',
})

// 필터
const behaviorFilters = [
  { value: 'All', label: '전체' },
  { value: 'StatusOnly', label: '단순 완료' },
  { value: 'FormBuilder', label: '폼 빌더' },
  { value: 'ModuleLink', label: '모듈 연동' },
  { value: 'Link', label: '링크' },
]
const selectedBehaviorType = ref('All')

const behaviorTypes = [
  { value: 'StatusOnly', label: '단순 완료 처리' },
  { value: 'FormBuilder', label: '폼 빌더' },
  { value: 'ModuleLink', label: '모듈 연동' },
  { value: 'Link', label: '링크' },
  { value: 'ShowComponentPopup', label: '컴포넌트 팝업' },
]

// --- 프리셋 폼 필드 ---
const visibleFields = computed(() => {
  if (!selectedPreset.value) return []
  return selectedPreset.value.formFields.filter(
    (f) => !f.showIf || f.showIf(formData.value),
  )
})

const expertFilteredLocations = computed(() => {
  if (!expertForm.value.actionCategory) return []
  return getAllowedLocationsForCategory(expertForm.value.actionCategory)
})

const filteredActions = computed(() => {
  if (selectedBehaviorType.value === 'All') return actions.value
  if (selectedBehaviorType.value === 'FormBuilder') {
    return actions.value.filter(
      (a) =>
        a.behaviorType === 'FormBuilder' || a.behaviorType === 'GenericForm',
    )
  }
  return actions.value.filter(
    (a) => a.behaviorType === selectedBehaviorType.value,
  )
})

// --- 프리셋 폼 초기화 ---
function initFormData(preset) {
  const data = { orderNum: actions.value.length, isActive: true }
  for (const field of preset.formFields) {
    if (field.default !== undefined) {
      data[field.key] = field.default
    } else if (field.type === 'checkbox') {
      data[field.key] = false
    } else {
      data[field.key] = ''
    }
  }
  return data
}

// --- 수정 시 프리셋 자동 감지 ---
function detectPreset(action) {
  const cat = action.actionCategory
  if (cat === 'CHECKLIST_CARD') return 'checklist'
  if (cat === 'MENU') return 'link-menu'
  if (cat === 'BANNER') return 'banner'
  if (cat === 'CARD') return 'info-card'
  if (cat === 'AUTO_POPUP') return 'popup'
  if (cat === 'BUTTON') return 'button'
  return null
}

function populatePresetFormFromAction(presetId, action) {
  const preset = getPresetById(presetId)
  if (!preset) return {}

  const data = initFormData(preset)
  data.orderNum = action.orderNum
  data.isActive = action.isActive

  // ConfigJson 파싱
  let config = {}
  try {
    if (action.configJson) {
      config =
        typeof action.configJson === 'string'
          ? JSON.parse(action.configJson)
          : action.configJson
    }
  } catch {
    config = {}
  }

  switch (presetId) {
    case 'checklist':
      data.title = action.title
      data.deadline = action.deadline
        ? formatDateTimeForInput(action.deadline)
        : ''
      data.targetLocation = action.targetLocation || 'HOME_CONTENT_TOP'
      break

    case 'link-menu':
      data.title = action.title
      data.linkType = action.behaviorType === 'Link' ? 'external' : 'internal'
      data.mapsTo =
        data.linkType === 'internal'
          ? (action.mapsTo || '').replace(/^\/feature\//, '')
          : action.mapsTo || ''
      break

    case 'banner':
      data.title = action.title
      data.imageUrl = config.imageUrl || ''
      data.height = config.height || 'md'
      data.targetLocation = action.targetLocation || 'HOME_SUB_HEADER'
      if (config.url) {
        data.linkType = 'internal'
        data.linkUrl = config.url
      } else if (config.externalUrl) {
        data.linkType = 'external'
        data.linkUrl = config.externalUrl
      } else {
        data.linkType = 'none'
        data.linkUrl = ''
      }
      break

    case 'info-card':
      data.title = action.title
      data.targetLocation = action.targetLocation || 'HOME_CONTENT_TOP'
      if (action.behaviorType === 'ModuleLink') {
        data.linkType = 'module'
        data.mapsTo = (action.mapsTo || '').replace(/^\/feature\//, '')
      } else if (action.behaviorType === 'Link') {
        data.linkType = 'link'
        data.mapsTo = action.mapsTo || ''
      } else {
        data.linkType = 'none'
        data.mapsTo = ''
      }
      break

    case 'popup':
      data.title = action.title
      data.message = config.message || ''
      data.size = config.size || 'md'
      data.showOnce = config.showOnce ?? true
      if (config.buttons && config.buttons.length > 0) {
        const btn = config.buttons[0]
        data.buttonLabel = btn.label || ''
        if (btn.url) {
          data.buttonLinkType = 'internal'
          // /conventions/1/schedule → schedule
          data.buttonUrl = btn.url.replace(/^\/conventions\/\d+\//, '')
        } else if (btn.externalUrl) {
          data.buttonLinkType = 'external'
          data.buttonExternalUrl = btn.externalUrl
        } else {
          data.buttonLinkType = 'close'
        }
      } else {
        data.buttonLabel = ''
        data.buttonLinkType = 'close'
        data.buttonUrl = ''
      }
      break

    case 'button':
      data.title = action.title
      data.targetLocation = action.targetLocation || 'HOME_SUB_HEADER'
      data.buttonStyle = config.style || 'primary'
      data.buttonSize = config.size || 'md'
      if (action.behaviorType === 'ModuleLink' && action.mapsTo) {
        data.actionType = 'internal'
        data.internalPage = action.mapsTo.replace(/^\/conventions\/\d+\//, '')
      } else if (action.behaviorType === 'Link' && action.mapsTo) {
        data.actionType = 'external'
        data.externalUrl = action.mapsTo
      } else if (action.behaviorType === 'ShowComponentPopup' && config.popupImageUrl) {
        data.actionType = 'imagePopup'
        data.popupImage = config.popupImageUrl
      } else if (action.behaviorType === 'ShowComponentPopup' && action.mapsTo) {
        data.actionType = 'component'
        data.componentName = action.mapsTo
      } else {
        data.actionType = 'internal'
      }
      break
  }

  return data
}

// --- 모달 열기/닫기 ---
function openCreateModal() {
  editingAction.value = null
  modalStep.value = 0
  selectedPresetId.value = null
  isExpertMode.value = false
  formData.value = {}
  htmlEditMode.value = 'visual'
  errorMessage.value = ''
  checklistDeleteQueue.length = 0
  showModal.value = true
}

async function openEditModal(action) {
  editingAction.value = action
  errorMessage.value = ''
  htmlEditMode.value = 'visual'

  const presetId = detectPreset(action)
  if (presetId === 'checklist') {
    // 체크리스트는 항상 bulk 모드로 열기
    editingAction.value = null // bulk 모드에서는 개별 편집 아님
    selectedPresetId.value = 'checklist'
    isExpertMode.value = false
    checklistDeleteQueue.length = 0
    loadChecklistRows()
    modalStep.value = 1
    showModal.value = true
    return
  } else if (presetId) {
    selectedPresetId.value = presetId
    isExpertMode.value = false
    modalStep.value = 1
    formData.value = populatePresetFormFromAction(presetId, action)
  } else {
    // 감지 안 되면 고급 모드
    isExpertMode.value = true
    modalStep.value = 1
    selectedPresetId.value = null
    await loadFormDefinitions()

    let behaviorType = action.behaviorType
    if (behaviorType === 'GenericForm') behaviorType = 'FormBuilder'

    let formTargetId = parseTargetId(action.targetId)
    if (
      formTargetId === null &&
      behaviorType === 'FormBuilder' &&
      action.targetModuleId != null
    ) {
      formTargetId = parseTargetId(action.targetModuleId)
    }

    let cleanedMapsTo = action.mapsTo || ''
    if (
      behaviorType === 'ModuleLink' &&
      cleanedMapsTo.startsWith('/feature/')
    ) {
      cleanedMapsTo = cleanedMapsTo.substring(9)
    }

    expertForm.value = {
      title: action.title,
      actionCategory: action.actionCategory || '',
      targetLocation: action.targetLocation || '',
      mapsTo: cleanedMapsTo,
      deadline: action.deadline ? formatDateTimeForInput(action.deadline) : '',
      orderNum: action.orderNum,
      isActive: action.isActive,
      behaviorType,
      targetId: formTargetId,
      configJson: action.configJson || '',
    }
  }

  showModal.value = true
}

function selectPreset(preset) {
  selectedPresetId.value = preset.id
}

function goToPresetForm() {
  if (!selectedPresetId.value) return

  if (selectedPresetId.value === 'checklist') {
    // 체크리스트: 기존 항목 로드 + 빈 행 추가
    loadChecklistRows()
    modalStep.value = 1
    return
  }

  formData.value = initFormData(getPresetById(selectedPresetId.value))
  modalStep.value = 1
}

function loadChecklistRows() {
  const existing = actions.value.filter(
    (a) => a.actionCategory === 'CHECKLIST_CARD',
  )
  checklistRows.value = existing.map((a) => ({
    _key: ++checklistKeyCounter,
    id: a.id,
    title: a.title,
    deadline: a.deadline ? formatDateTimeForInput(a.deadline) : '',
    isActive: a.isActive,
    targetLocation: a.targetLocation || 'HOME_CONTENT_TOP',
    orderNum: a.orderNum,
  }))

  // 공통 위치 감지
  if (existing.length > 0) {
    checklistShared.value.targetLocation =
      existing[0].targetLocation || 'HOME_CONTENT_TOP'
  } else {
    checklistShared.value.targetLocation = 'HOME_CONTENT_TOP'
  }
  checklistShared.value.deadline = ''

  // 기존 항목 없으면 빈 행 3개 추가
  if (checklistRows.value.length === 0) {
    for (let i = 0; i < 3; i++) addChecklistRow()
  }
}

function addChecklistRow() {
  checklistRows.value.push({
    _key: ++checklistKeyCounter,
    id: null,
    title: '',
    deadline: '',
    isActive: true,
    targetLocation: '',
    orderNum: 0,
  })
}

function removeChecklistRow(idx) {
  const row = checklistRows.value[idx]
  if (row.id) {
    // 기존 항목 삭제 → 마킹
    row._deleted = true
    // UI에서는 숨기기 위해 별도 배열 사용하지 않고 필터링
    checklistRows.value.splice(idx, 1)
    // 삭제 대기열에 보관
    checklistDeleteQueue.push(row.id)
  } else {
    checklistRows.value.splice(idx, 1)
  }
}

async function openExpertMode() {
  isExpertMode.value = true
  modalStep.value = 1
  selectedPresetId.value = null
  expertForm.value = {
    title: '',
    actionCategory: '',
    targetLocation: '',
    mapsTo: '',
    deadline: '',
    orderNum: actions.value.length,
    isActive: true,
    behaviorType: 'StatusOnly',
    targetId: null,
    configJson: '',
  }
  await loadFormDefinitions()
}

function closeModal() {
  showModal.value = false
  editingAction.value = null
  errorMessage.value = ''
}

function selectExpertCategory(category) {
  expertForm.value.actionCategory = category.key
  expertForm.value.targetLocation = ''
}

function stripFeaturePrefix(event) {
  const value = event.target.value
  if (value.startsWith('/feature/')) {
    expertForm.value.mapsTo = value.substring(9)
  } else if (value.startsWith('feature/')) {
    expertForm.value.mapsTo = value.substring(8)
  } else if (value.startsWith('/feature')) {
    expertForm.value.mapsTo = value.substring(8)
  }
}

// --- 저장 ---
async function saveAction() {
  errorMessage.value = ''

  // 체크리스트 bulk 저장
  if (selectedPresetId.value === 'checklist' && !isExpertMode.value) {
    await saveChecklistBulk()
    return
  }

  let payload

  if (isExpertMode.value) {
    payload = buildExpertPayload()
  } else {
    payload = buildPresetPayload()
  }

  if (!payload) return // 유효성 검사 실패

  submitting.value = true
  try {
    if (editingAction.value) {
      await apiClient.put(
        `/admin/action-management/actions/${editingAction.value.id}`,
        payload,
      )
    } else {
      await apiClient.post('/admin/action-management/actions', payload)
    }
    closeModal()
    await loadActions()
  } catch (error) {
    errorMessage.value = error.response?.data?.message || '저장에 실패했습니다.'
  } finally {
    submitting.value = false
  }
}

async function saveChecklistBulk() {
  const preset = getPresetById('checklist')
  const validRows = checklistRows.value.filter((r) => r.title.trim())

  if (validRows.length === 0) {
    errorMessage.value = '최소 1개 항목의 제목을 입력해주세요.'
    return
  }

  submitting.value = true
  try {
    // 1. 삭제 대기열 처리
    for (const id of checklistDeleteQueue) {
      await apiClient.delete(`/admin/action-management/actions/${id}`)
    }
    checklistDeleteQueue.length = 0

    // 2. 기존 항목 업데이트 + 신규 항목 생성
    // 기존 orderNum 최대값 (체크리스트 아닌 것들)
    const nonChecklistMax = actions.value
      .filter((a) => a.actionCategory !== 'CHECKLIST_CARD')
      .reduce((max, a) => Math.max(max, a.orderNum), -1)

    for (let i = 0; i < validRows.length; i++) {
      const row = validRows[i]
      const orderNum = nonChecklistMax + 1 + i
      const deadline = row.deadline || checklistShared.value.deadline || null
      const targetLocation =
        row.targetLocation ||
        checklistShared.value.targetLocation ||
        'HOME_CONTENT_TOP'

      const payload = preset.buildRowPayload(
        { ...row, deadline, targetLocation },
        props.conventionId,
        orderNum,
      )

      if (row.id) {
        // 기존 항목 업데이트
        payload.isActive = row.isActive ?? true
        await apiClient.put(
          `/admin/action-management/actions/${row.id}`,
          payload,
        )
      } else {
        // 신규 생성
        await apiClient.post('/admin/action-management/actions', payload)
      }
    }

    closeModal()
    await loadActions()
  } catch (error) {
    errorMessage.value = error.response?.data?.message || '저장에 실패했습니다.'
  } finally {
    submitting.value = false
  }
}

function buildPresetPayload() {
  const preset = selectedPreset.value
  if (!preset || preset.isBulk) {
    errorMessage.value = '프리셋을 선택해주세요.'
    return null
  }

  // 필수 필드 검증
  for (const field of preset.formFields) {
    if (field.required && field.showIf && !field.showIf(formData.value))
      continue
    if (field.required && !formData.value[field.key]) {
      errorMessage.value = `${field.label}을(를) 입력해주세요.`
      return null
    }
  }

  const payload = preset.buildPayload(formData.value, props.conventionId)
  payload.orderNum = formData.value.orderNum ?? 0

  // 수정 시 활성화 상태 반영
  if (editingAction.value) {
    payload.isActive = formData.value.isActive ?? true
  }

  return payload
}

function buildExpertPayload() {
  const f = expertForm.value

  if (!f.title) {
    errorMessage.value = '제목을 입력해주세요.'
    return null
  }
  if (!f.actionCategory) {
    errorMessage.value = '액션 카테고리를 선택해주세요.'
    return null
  }
  if (!f.targetLocation) {
    errorMessage.value = '표시 위치를 선택해주세요.'
    return null
  }

  switch (f.behaviorType) {
    case 'FormBuilder':
      if (f.targetId == null || isNaN(f.targetId)) {
        errorMessage.value = '연결할 폼을 선택해주세요.'
        return null
      }
      break
    case 'ModuleLink':
    case 'Link':
    case 'ShowComponentPopup':
      if (!f.mapsTo) {
        errorMessage.value = '연결할 URL 또는 컴포넌트 이름을 입력해주세요.'
        return null
      }
      break
  }

  const payload = {
    conventionId: props.conventionId,
    title: f.title,
    actionCategory: f.actionCategory,
    targetLocation: f.targetLocation,
    deadline: f.deadline || null,
    orderNum: f.orderNum,
    isActive: f.isActive,
    behaviorType: f.behaviorType,
    mapsTo: null,
    targetId: null,
    configJson: f.configJson || null,
  }

  switch (f.behaviorType) {
    case 'FormBuilder':
      payload.targetId = parseInt(f.targetId, 10)
      break
    case 'ModuleLink':
      payload.mapsTo = '/feature/' + f.mapsTo
      break
    case 'Link':
      payload.mapsTo = f.mapsTo
      break
    case 'ShowComponentPopup':
      payload.mapsTo = f.mapsTo
      if (f.targetId != null && !isNaN(f.targetId)) {
        payload.targetId = parseInt(f.targetId, 10)
      }
      break
  }

  return payload
}

// --- 데이터 로딩 ---
async function loadActions() {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/action-management/convention/${props.conventionId}`,
    )
    actions.value = response.data || []
  } catch (error) {
    console.error('Failed to load actions:', error)
  } finally {
    loading.value = false
  }
}

async function loadFormDefinitions() {
  if (!props.conventionId) return
  try {
    const response = await formBuilderService.getFormDefinitions(
      props.conventionId,
    )
    formDefinitions.value = response.data || []
  } catch (error) {
    console.error('Failed to load form definitions:', error)
  }
}

// --- 액션 조작 ---
async function toggleAction(action) {
  try {
    await apiClient.put(`/admin/action-management/actions/${action.id}/toggle`)
    action.isActive = !action.isActive
  } catch (error) {
    alert('상태 변경 실패: ' + error.message)
  }
}

async function deleteAction(action) {
  if (!confirm(`"${action.title}" 액션을 삭제하시겠습니까?`)) return
  try {
    await apiClient.delete(`/admin/action-management/actions/${action.id}`)
    await loadActions()
  } catch (error) {
    alert('삭제 실패: ' + error.message)
  }
}

// --- 유틸리티 ---
function parseTargetId(value) {
  if (value == null) return null
  const parsed = parseInt(value, 10)
  return isNaN(parsed) ? null : parsed
}

function formatDateTime(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('ko-KR')
}

function formatDateTimeForInput(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

function getBehaviorTypeName(type) {
  switch (type) {
    case 'StatusOnly':
      return '단순 완료'
    case 'FormBuilder':
      return '폼 빌더'
    case 'GenericForm':
      return '폼 빌더 (구)'
    case 'ModuleLink':
      return '모듈 연동'
    case 'Link':
      return '링크'
    case 'ShowComponentPopup':
      return '컴포넌트 팝업'
    default:
      return type
  }
}

onMounted(() => {
  loadActions()
})
</script>
