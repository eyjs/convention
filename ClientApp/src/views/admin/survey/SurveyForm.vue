<template>
  <div class="pb-24">
    <!-- 토스트 메시지 -->
    <div
      v-if="toastMessage"
      class="fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white text-sm transition-all duration-300"
      :class="toastType === 'success' ? 'bg-green-500' : 'bg-red-500'"
    >
      {{ toastMessage }}
    </div>

    <form @submit.prevent="saveSurvey">
      <!-- 기본 정보 섹션 -->
      <div
        class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm mb-6"
      >
        <div class="px-5 py-4 border-b border-gray-100 dark:border-gray-700">
          <h3 class="text-base font-semibold text-gray-800 dark:text-gray-200">
            기본 정보
          </h3>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            설문의 기본 설정을 구성합니다.
          </p>
        </div>

        <div class="p-5 space-y-5">
          <!-- 제목 -->
          <div>
            <label
              for="title"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              설문 제목 <span class="text-red-500">*</span>
            </label>
            <input
              id="title"
              v-model="survey.title"
              type="text"
              placeholder="설문 제목을 입력하세요"
              class="block w-full rounded-lg px-3.5 py-2.5 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
              :class="
                errors.title
                  ? 'border-red-400 dark:border-red-500 bg-red-50 dark:bg-red-900/10 animate-shake'
                  : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
              "
            />
            <p
              v-if="errors.title"
              class="mt-1.5 text-sm text-red-500 flex items-center gap-1"
            >
              <span class="inline-block w-1 h-1 rounded-full bg-red-500"></span>
              {{ errors.title }}
            </p>
          </div>

          <!-- 설명 -->
          <div>
            <label
              for="description"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
            >
              설명
            </label>
            <textarea
              id="description"
              v-model="survey.description"
              rows="3"
              placeholder="설문에 대한 설명을 입력하세요 (선택사항)"
              class="block w-full rounded-lg px-3.5 py-2.5 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
            ></textarea>
          </div>

          <!-- 날짜 -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label
                for="startDate"
                class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
              >
                시작일
              </label>
              <input
                id="startDate"
                v-model="survey.startDate"
                type="datetime-local"
                class="block w-full rounded-lg px-3.5 py-2.5 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
                :class="
                  errors.dateRange
                    ? 'border-red-400 dark:border-red-500 bg-red-50 dark:bg-red-900/10'
                    : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
                "
              />
            </div>
            <div>
              <label
                for="endDate"
                class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
              >
                종료일
              </label>
              <input
                id="endDate"
                v-model="survey.endDate"
                type="datetime-local"
                class="block w-full rounded-lg px-3.5 py-2.5 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
                :class="
                  errors.dateRange
                    ? 'border-red-400 dark:border-red-500 bg-red-50 dark:bg-red-900/10'
                    : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
                "
              />
            </div>
            <p
              v-if="errors.dateRange"
              class="col-span-full text-sm text-red-500 -mt-2 flex items-center gap-1"
            >
              <span class="inline-block w-1 h-1 rounded-full bg-red-500"></span>
              {{ errors.dateRange }}
            </p>
          </div>

          <!-- 활성 상태 토글 -->
          <div
            class="flex items-center justify-between rounded-lg bg-gray-50 dark:bg-gray-700/50 px-4 py-3"
          >
            <div>
              <p class="text-sm font-medium text-gray-700 dark:text-gray-300">
                활성 상태
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                활성화하면 참석자가 설문에 응답할 수 있습니다.
              </p>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input
                v-model="survey.isActive"
                type="checkbox"
                class="sr-only peer"
              />
              <div
                class="w-11 h-6 bg-gray-300 rounded-full peer peer-checked:bg-indigo-600 peer-focus:ring-2 peer-focus:ring-indigo-500/20 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:after:translate-x-full dark:bg-gray-600"
              ></div>
            </label>
          </div>
        </div>
      </div>

      <!-- 질문 구성 섹션 -->
      <div
        class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm"
      >
        <div
          class="px-5 py-4 border-b border-gray-100 dark:border-gray-700 flex items-center justify-between"
        >
          <div>
            <h3
              class="text-base font-semibold text-gray-800 dark:text-gray-200"
            >
              질문 구성
            </h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
              {{ topLevelQuestions.length }}개의 질문이 등록되어 있습니다.
            </p>
          </div>
          <!-- prettier-ignore -->
          <button
            type="button"
            class="inline-flex items-center gap-2 px-4 py-2.5 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 text-sm font-medium transition-colors shadow-sm"
            @click="addQuestion()"
          >
            <Plus class="w-4 h-4" />
            질문 추가
          </button>
        </div>

        <div class="p-5">
          <p
            v-if="errors.questions"
            class="text-sm text-red-500 mb-4 flex items-center gap-1"
          >
            <span class="inline-block w-1 h-1 rounded-full bg-red-500"></span>
            {{ errors.questions }}
          </p>

          <!-- 빈 상태 -->
          <div
            v-if="topLevelQuestions.length === 0"
            class="text-center py-16 border-2 border-dashed border-gray-200 dark:border-gray-700 rounded-xl"
          >
            <div
              class="w-16 h-16 mx-auto mb-4 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center"
            >
              <Plus class="w-8 h-8 text-gray-400" />
            </div>
            <p class="text-gray-500 dark:text-gray-400 font-medium mb-1">
              아직 질문이 없습니다
            </p>
            <p class="text-sm text-gray-400 dark:text-gray-500 mb-4">
              질문을 추가하여 설문을 구성하세요.
            </p>
            <!-- prettier-ignore -->
            <button
              type="button"
              class="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-indigo-600 dark:text-indigo-400 border border-indigo-300 dark:border-indigo-600 rounded-lg hover:bg-indigo-50 dark:hover:bg-indigo-900/20 transition-colors"
              @click="addQuestion()"
            >
              <Plus class="w-4 h-4" />
              첫 번째 질문 추가
            </button>
          </div>

          <!-- 질문 카드 목록 (최상위만) -->
          <div class="space-y-4">
            <template
              v-for="(question, qIndex) in survey.questions"
              :key="question._key"
            >
              <!-- 최상위 질문만 렌더링 (꼬리질문은 옵션 하위에 렌더링) -->
              <QuestionCard
                v-if="!question.parentOptionId"
                :question="question"
                :q-index="qIndex"
                :survey="survey"
                :errors="errors"
                :top-level-index="getTopLevelIndex(question)"
                @move="moveQuestion(qIndex, $event)"
                @remove="removeQuestion(qIndex)"
                @add-option="addOption(qIndex)"
                @remove-option="removeOption(qIndex, $event)"
                @type-change="onQuestionTypeChange(question)"
                @add-follow-up="addFollowUpQuestion"
                @remove-follow-up="removeFollowUpQuestion"
              />
            </template>
          </div>
        </div>
      </div>
    </form>

    <!-- 하단 고정 액션 바 -->
    <div
      class="fixed bottom-0 left-0 right-0 z-40 bg-white/95 dark:bg-gray-800/95 backdrop-blur-sm border-t border-gray-200 dark:border-gray-700 px-6 py-3"
    >
      <div class="max-w-5xl mx-auto flex items-center justify-between">
        <div class="text-sm text-gray-500 dark:text-gray-400">
          <span v-if="isDirty" class="flex items-center gap-1.5">
            <span
              class="w-2 h-2 rounded-full bg-amber-400 animate-pulse"
            ></span>
            저장하지 않은 변경사항이 있습니다
          </span>
        </div>
        <div class="flex items-center gap-3">
          <!-- prettier-ignore -->
          <button
            type="button"
            class="inline-flex items-center gap-2 px-5 py-2.5 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 font-medium text-sm transition-colors"
            @click="emit('cancel')"
          >
            <ArrowLeft class="w-4 h-4" />
            취소
          </button>
          <!-- prettier-ignore -->
          <button
            type="button"
            :disabled="isSaving"
            class="inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium text-sm transition-colors shadow-sm disabled:bg-indigo-400 disabled:cursor-not-allowed"
            @click="saveSurvey"
          >
            <Save class="w-4 h-4" />
            {{ isSaving ? '저장 중...' : '저장' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch, onUnmounted, reactive, h } from 'vue'
import {
  ChevronUp,
  ChevronDown,
  Trash2,
  Plus,
  GripVertical,
  X,
  Save,
  ArrowLeft,
  CornerDownRight,
} from 'lucide-vue-next'
import api from '@/services/api'
import { formatDateTimeForInput } from '@/utils/date'
import { useToast } from '@/composables/useToast'
import { QUESTION_TYPE_LABELS, isChoiceType } from '@/constants/survey'

// ─── QuestionCard (inline component) ───
const QuestionCard = {
  name: 'QuestionCard',
  props: {
    question: Object,
    qIndex: Number,
    survey: Object,
    errors: Object,
    topLevelIndex: Number,
    isFollowUp: { type: Boolean, default: false },
    parentOptionText: { type: String, default: '' },
  },
  emits: [
    'move',
    'remove',
    'add-option',
    'remove-option',
    'type-change',
    'add-follow-up',
    'remove-follow-up',
  ],
  setup(props, { emit }) {
    function getFollowUpQuestions(optionId) {
      return props.survey.questions.filter((q) => q.parentOptionId === optionId)
    }

    function getGlobalIndex(q) {
      return props.survey.questions.indexOf(q)
    }

    return () => {
      const q = props.question
      const qIdx = props.qIndex
      const errs = props.errors

      // 선택지 렌더
      function renderOptions() {
        if (!isChoiceType(q.type)) return null

        const optionItems = (q.options || []).map((option, oIndex) => {
          const followUps = getFollowUpQuestions(
            option.id > 0 ? option.id : option._tempKey,
          )

          const followUpCards = followUps.map((fuQ) => {
            const fuIdx = getGlobalIndex(fuQ)
            return h(QuestionCard, {
              key: fuQ._key,
              question: fuQ,
              qIndex: fuIdx,
              survey: props.survey,
              errors: errs,
              topLevelIndex: null,
              isFollowUp: true,
              parentOptionText: option.optionText,
              onRemove: () => emit('remove-follow-up', fuIdx),
              'onType-change': () => emit('type-change'),
              'onAdd-option': () => emit('add-option'),
              'onRemove-option': (oi) => emit('remove-option', oi),
              'onAdd-follow-up': (payload) => emit('add-follow-up', payload),
              'onRemove-follow-up': (idx) => emit('remove-follow-up', idx),
            })
          })

          return h('div', { key: option.id || option._tempKey || oIndex }, [
            h('div', { class: 'flex items-center gap-2' }, [
              h(
                'span',
                {
                  class:
                    'flex-shrink-0 w-6 h-6 rounded-full bg-white dark:bg-gray-700 border border-gray-200 dark:border-gray-600 text-xs flex items-center justify-center text-gray-500 dark:text-gray-400 font-medium',
                },
                `${oIndex + 1}`,
              ),
              h('input', {
                value: option.optionText,
                onInput: (e) => {
                  option.optionText = e.target.value
                },
                type: 'text',
                placeholder: `선택지 ${oIndex + 1}`,
                class: [
                  'flex-grow rounded-lg px-3 py-2 text-sm shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none',
                  errs[`question_${qIdx}_option_${oIndex}`]
                    ? 'border-red-400 dark:border-red-500 bg-red-50 dark:bg-red-900/10'
                    : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200',
                ].join(' '),
              }),
              // 꼬리질문 추가 버튼
              h(
                'button',
                {
                  type: 'button',
                  title: '꼬리질문 추가',
                  class:
                    'flex-shrink-0 p-1.5 rounded-md text-gray-400 hover:text-indigo-500 hover:bg-indigo-50 dark:hover:bg-indigo-900/20 transition-colors',
                  onClick: () =>
                    emit('add-follow-up', {
                      optionId: option.id > 0 ? option.id : option._tempKey,
                      afterIndex: qIdx,
                    }),
                },
                [h(CornerDownRight, { class: 'w-4 h-4' })],
              ),
              // 선택지 삭제
              h(
                'button',
                {
                  type: 'button',
                  title: '선택지 삭제',
                  class:
                    'flex-shrink-0 p-1.5 rounded-md text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors',
                  onClick: () => emit('remove-option', oIndex),
                },
                [h(X, { class: 'w-4 h-4' })],
              ),
            ]),
            // 꼬리질문 카드들
            ...followUpCards,
          ])
        })

        return h(
          'div',
          {
            class: 'mt-2 rounded-lg bg-gray-50 dark:bg-gray-700/30 p-4',
          },
          [
            h('div', { class: 'flex justify-between items-center mb-3' }, [
              h(
                'h5',
                {
                  class:
                    'text-sm font-semibold text-gray-700 dark:text-gray-300',
                },
                '선택지',
              ),
              h(
                'button',
                {
                  type: 'button',
                  class:
                    'inline-flex items-center gap-1 text-sm text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium transition-colors',
                  onClick: () => emit('add-option'),
                },
                [h(Plus, { class: 'w-3.5 h-3.5' }), '선택지 추가'],
              ),
            ]),
            errs[`question_${qIdx}_options`]
              ? h(
                  'p',
                  { class: 'text-sm text-red-500 mb-2' },
                  errs[`question_${qIdx}_options`],
                )
              : null,
            h('div', { class: 'space-y-2' }, optionItems),
          ],
        )
      }

      // 카드 본체
      const cardContent = [
        // 헤더
        h(
          'div',
          {
            class:
              'flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-gray-700 bg-gray-50/80 dark:bg-gray-800/80 rounded-t-xl',
          },
          [
            h('div', { class: 'flex items-center gap-3' }, [
              props.isFollowUp
                ? h(CornerDownRight, {
                    class: 'w-4 h-4 text-indigo-400',
                  })
                : h(GripVertical, {
                    class: 'w-4 h-4 text-gray-400 cursor-grab',
                  }),
              props.topLevelIndex != null
                ? h(
                    'span',
                    {
                      class:
                        'inline-flex items-center justify-center w-7 h-7 rounded-full bg-indigo-100 dark:bg-indigo-900/40 text-indigo-700 dark:text-indigo-300 text-xs font-bold',
                    },
                    `${props.topLevelIndex + 1}`,
                  )
                : null,
              props.isFollowUp
                ? h(
                    'span',
                    {
                      class:
                        'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-300',
                    },
                    `꼬리질문`,
                  )
                : null,
              h(
                'span',
                {
                  class: [
                    'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium',
                    isChoiceType(q.type)
                      ? 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                      : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400',
                  ].join(' '),
                },
                QUESTION_TYPE_LABELS[q.type] || q.type,
              ),
              q.isRequired
                ? h(
                    'span',
                    {
                      class:
                        'inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300',
                    },
                    '필수',
                  )
                : null,
            ]),
            h('div', { class: 'flex items-center gap-1' }, [
              !props.isFollowUp
                ? h(
                    'button',
                    {
                      type: 'button',
                      class:
                        'p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-30 transition-colors',
                      disabled: props.topLevelIndex === 0,
                      title: '위로 이동',
                      onClick: () => emit('move', -1),
                    },
                    [h(ChevronUp, { class: 'w-4 h-4' })],
                  )
                : null,
              !props.isFollowUp
                ? h(
                    'button',
                    {
                      type: 'button',
                      class:
                        'p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-30 transition-colors',
                      disabled:
                        props.topLevelIndex ===
                        props.survey.questions.filter(
                          (qq) => !qq.parentOptionId,
                        ).length -
                          1,
                      title: '아래로 이동',
                      onClick: () => emit('move', 1),
                    },
                    [h(ChevronDown, { class: 'w-4 h-4' })],
                  )
                : null,
              h('div', {
                class: 'w-px h-5 bg-gray-200 dark:bg-gray-600 mx-1',
              }),
              h(
                'button',
                {
                  type: 'button',
                  class:
                    'p-1.5 rounded-md text-red-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors',
                  title: '삭제',
                  onClick: () => emit('remove'),
                },
                [h(Trash2, { class: 'w-4 h-4' })],
              ),
            ]),
          ],
        ),
        // 본문
        h('div', { class: 'p-4 space-y-4' }, [
          h('div', { class: 'grid grid-cols-1 md:grid-cols-2 gap-4' }, [
            h('div', null, [
              h(
                'label',
                {
                  class:
                    'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1',
                },
                ['질문 내용 ', h('span', { class: 'text-red-500' }, '*')],
              ),
              h('input', {
                value: q.questionText,
                onInput: (e) => {
                  q.questionText = e.target.value
                },
                type: 'text',
                placeholder: '질문 내용을 입력하세요',
                class: [
                  'block w-full rounded-lg px-3.5 py-2.5 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none',
                  errs[`question_${qIdx}`]
                    ? 'border-red-400 dark:border-red-500 bg-red-50 dark:bg-red-900/10'
                    : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200',
                ].join(' '),
              }),
              errs[`question_${qIdx}`]
                ? h(
                    'p',
                    { class: 'mt-1.5 text-sm text-red-500' },
                    errs[`question_${qIdx}`],
                  )
                : null,
            ]),
            h('div', null, [
              h(
                'label',
                {
                  class:
                    'block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1',
                },
                '질문 유형',
              ),
              h(
                'select',
                {
                  value: q.type,
                  onChange: (e) => {
                    q.type = e.target.value
                    emit('type-change')
                  },
                  class:
                    'block w-full rounded-lg px-3.5 py-2.5 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none',
                },
                Object.entries(QUESTION_TYPE_LABELS).map(([val, label]) =>
                  h('option', { value: val, key: val }, label),
                ),
              ),
            ]),
          ]),
          h('div', null, [
            h('label', { class: 'flex items-center gap-2 cursor-pointer' }, [
              h('input', {
                type: 'checkbox',
                checked: q.isRequired,
                onChange: (e) => {
                  q.isRequired = e.target.checked
                },
                class: 'rounded text-indigo-600 focus:ring-indigo-500/20',
              }),
              h(
                'span',
                { class: 'text-sm text-gray-700 dark:text-gray-300' },
                '필수 응답',
              ),
            ]),
          ]),
          renderOptions(),
        ]),
      ]

      return h(
        'div',
        {
          class: [
            'rounded-xl border transition-all duration-200',
            props.isFollowUp ? 'ml-6 mt-2' : '',
            errs[`question_${qIdx}`]
              ? 'border-red-300 dark:border-red-700 bg-red-50/50 dark:bg-red-900/10 shadow-red-100 dark:shadow-none shadow-sm'
              : props.isFollowUp
                ? 'border-indigo-200 dark:border-indigo-800 bg-indigo-50/30 dark:bg-indigo-900/10'
                : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 hover:shadow-md',
          ].join(' '),
        },
        cardContent,
      )
    }
  },
}

const props = defineProps({
  surveyId: {
    type: Number,
    default: null,
  },
  conventionId: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['cancel', 'saved'])

let nextTempKey = -1

const survey = ref({
  id: props.surveyId,
  title: '',
  description: '',
  isActive: true,
  startDate: null,
  endDate: null,
  conventionId: props.conventionId,
  questions: [],
})

const isEditing = computed(() => !!props.surveyId)
const isSaving = ref(false)
const isDirty = ref(false)
const errors = reactive({})

const { toastMessage, toastType, showToast } = useToast()

const topLevelQuestions = computed(() =>
  survey.value.questions.filter((q) => !q.parentOptionId),
)

function getTopLevelIndex(question) {
  return topLevelQuestions.value.indexOf(question)
}

// 미저장 경고
function onBeforeUnload(e) {
  if (isDirty.value) {
    e.preventDefault()
    e.returnValue = ''
  }
}

onMounted(() => {
  window.addEventListener('beforeunload', onBeforeUnload)
  fetchSurveyData()
})

onUnmounted(() => {
  window.removeEventListener('beforeunload', onBeforeUnload)
})

// 변경 감지
watch(
  survey,
  () => {
    isDirty.value = true
  },
  { deep: true },
)

watch(() => props.surveyId, fetchSurveyData)

function generateTempKey() {
  return nextTempKey--
}

async function fetchSurveyData() {
  if (!isEditing.value) {
    survey.value.questions = []
    isDirty.value = false
    return
  }
  try {
    const response = await api.get(`/surveys/${props.surveyId}`)
    const data = response.data
    data.startDate = formatDateTimeForInput(data.startDate)
    data.endDate = formatDateTimeForInput(data.endDate)
    // 각 질문/옵션에 _key 부여
    data.questions.forEach((q) => {
      q._key = q.id || generateTempKey()
      q.options.forEach((o) => {
        o._tempKey = o.id > 0 ? null : generateTempKey()
      })
    })
    survey.value = data
    isDirty.value = false
  } catch {
    showToast('설문 정보를 불러오는데 실패했습니다.', 'error')
  }
}

function addQuestion(parentOptionId = null) {
  const key = generateTempKey()
  survey.value.questions.push({
    _key: key,
    id: 0,
    questionText: '',
    type: 'SHORT_TEXT',
    isRequired: false,
    orderIndex: survey.value.questions.length,
    parentOptionId: parentOptionId,
    options: [],
  })
}

function addFollowUpQuestion({ optionId, afterIndex }) {
  const key = generateTempKey()
  const followUp = {
    _key: key,
    id: 0,
    questionText: '',
    type: 'SHORT_TEXT',
    isRequired: false,
    orderIndex: survey.value.questions.length,
    parentOptionId: optionId,
    options: [],
  }
  // 부모 질문 바로 뒤에 삽입
  survey.value.questions.splice(afterIndex + 1, 0, followUp)
}

function removeFollowUpQuestion(index) {
  survey.value.questions.splice(index, 1)
}

function removeQuestion(index) {
  const question = survey.value.questions[index]
  // 이 질문의 옵션에 연결된 꼬리질문도 삭제
  if (question.options) {
    const optionIds = question.options.map((o) =>
      o.id > 0 ? o.id : o._tempKey,
    )
    survey.value.questions = survey.value.questions.filter(
      (q, i) => i === index || !optionIds.includes(q.parentOptionId),
    )
  }
  // 본 질문 삭제
  const idx = survey.value.questions.indexOf(question)
  if (idx !== -1) survey.value.questions.splice(idx, 1)
}

function moveQuestion(index, direction) {
  const newIndex = index + direction
  if (newIndex < 0 || newIndex >= survey.value.questions.length) return
  const questions = survey.value.questions
  const temp = questions[index]
  questions[index] = questions[newIndex]
  questions[newIndex] = temp
  survey.value.questions = [...questions]
}

function addOption(questionIndex) {
  const tempKey = generateTempKey()
  survey.value.questions[questionIndex].options.push({
    id: 0,
    _tempKey: tempKey,
    optionText: '',
    orderIndex: survey.value.questions[questionIndex].options.length,
  })
}

function removeOption(questionIndex, optionIndex) {
  const option = survey.value.questions[questionIndex].options[optionIndex]
  const optionRef = option.id > 0 ? option.id : option._tempKey
  // 연결된 꼬리질문도 삭제
  survey.value.questions = survey.value.questions.filter(
    (q) => q.parentOptionId !== optionRef,
  )
  // 옵션 삭제 (index 재계산)
  const qIdx = survey.value.questions.findIndex(
    (q) =>
      q === survey.value.questions[questionIndex] ||
      q._key === survey.value.questions[questionIndex]?._key,
  )
  if (qIdx !== -1) {
    survey.value.questions[qIdx].options.splice(optionIndex, 1)
  }
}

function onQuestionTypeChange(question) {
  if (!isChoiceType(question.type)) {
    // 선택지에 연결된 꼬리질문도 삭제
    const optionIds = (question.options || []).map((o) =>
      o.id > 0 ? o.id : o._tempKey,
    )
    survey.value.questions = survey.value.questions.filter(
      (q) => !optionIds.includes(q.parentOptionId),
    )
    question.options = []
  }
}

function clearErrors() {
  Object.keys(errors).forEach((key) => delete errors[key])
}

function validateForm() {
  clearErrors()
  let valid = true

  if (!survey.value.title?.trim()) {
    errors.title = '설문 제목을 입력해주세요.'
    valid = false
  }

  if (
    survey.value.startDate &&
    survey.value.endDate &&
    new Date(survey.value.startDate) > new Date(survey.value.endDate)
  ) {
    errors.dateRange = '시작일이 종료일보다 늦을 수 없습니다.'
    valid = false
  }

  if (topLevelQuestions.value.length === 0) {
    errors.questions = '질문을 1개 이상 추가해주세요.'
    valid = false
  }

  survey.value.questions.forEach((q, qIndex) => {
    if (!q.questionText?.trim()) {
      errors[`question_${qIndex}`] = '질문 내용을 입력해주세요.'
      valid = false
    }

    if (isChoiceType(q.type)) {
      if (!q.options || q.options.length === 0) {
        errors[`question_${qIndex}_options`] = '선택지를 1개 이상 추가해주세요.'
        valid = false
      } else {
        q.options.forEach((o, oIndex) => {
          if (!o.optionText?.trim()) {
            errors[`question_${qIndex}_option_${oIndex}`] = true
            valid = false
          }
        })
      }
    }
  })

  return valid
}

async function saveSurvey() {
  if (!validateForm()) {
    showToast('입력값을 확인해주세요.', 'error')
    return
  }

  isSaving.value = true

  // orderIndex 재설정
  survey.value.questions.forEach((q, qIndex) => {
    q.orderIndex = qIndex
    if (q.options) {
      q.options.forEach((o, oIndex) => {
        o.orderIndex = oIndex
        // 새 옵션에 tempKey 전달
        if (!o.id || o.id === 0) {
          o.tempKey = o._tempKey
        }
      })
    }
  })

  const payload = {
    ...survey.value,
    startDate: survey.value.startDate || null,
    endDate: survey.value.endDate || null,
  }

  try {
    if (isEditing.value) {
      await api.put(`/surveys/${survey.value.id}`, payload)
      showToast('설문이 수정되었습니다.')
    } else {
      await api.post('/surveys', payload)
      showToast('설문이 생성되었습니다.')
    }
    isDirty.value = false
    emit('saved')
  } catch (err) {
    const msg = err.response?.data?.message || '설문 저장에 실패했습니다.'
    showToast(msg, 'error')
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
@keyframes shake {
  0%,
  100% {
    transform: translateX(0);
  }
  10%,
  30%,
  50%,
  70%,
  90% {
    transform: translateX(-2px);
  }
  20%,
  40%,
  60%,
  80% {
    transform: translateX(2px);
  }
}

.animate-shake {
  animation: shake 0.5s ease-in-out;
}
</style>
