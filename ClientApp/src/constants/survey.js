export const QUESTION_TYPES = {
  SHORT_TEXT: 'SHORT_TEXT',
  LONG_TEXT: 'LONG_TEXT',
  SINGLE_CHOICE: 'SINGLE_CHOICE',
  MULTIPLE_CHOICE: 'MULTIPLE_CHOICE',
}

export const QUESTION_TYPE_LABELS = {
  SHORT_TEXT: '단답형',
  LONG_TEXT: '장문형',
  SINGLE_CHOICE: '단일 선택',
  MULTIPLE_CHOICE: '다중 선택',
}

export const SURVEY_TYPES = {
  GENERAL: 'GENERAL',
}

export const SURVEY_TYPE_LABELS = {
  GENERAL: '일반 설문',
}

export function getQuestionTypeLabel(type) {
  return QUESTION_TYPE_LABELS[type] || type
}

export function isChoiceType(type) {
  return (
    type === QUESTION_TYPES.SINGLE_CHOICE ||
    type === QUESTION_TYPES.MULTIPLE_CHOICE
  )
}
