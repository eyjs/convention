<template>
  <BaseModal :is-open="isOpen" @close="close" max-width="3xl">
    <template #header>
      <h2 class="text-2xl font-bold text-center w-full text-gray-900">
        개인정보처리방침
      </h2>
    </template>
    <template #body>
      <div class="space-y-4 leading-relaxed text-sm md:text-base">
        <p>
          본 개인정보처리방침은 아이에프에이(주)(이하 ‘당사’) 여행 대상자의
          개인정보를 보호하는 것을 주된 목적으로 합니다.
        </p>

        <p>
          당사는 고객님의 개인정보보호를 매우 중요시하며, 『개인정보보호법』,
          『정보통신망이용촉진 및 정보보호에 관한 법률』,
          『개인정보보호지침』을 준수합니다.
        </p>

        <p>
          본 방침은 고객님께서 제공하신 개인정보가 어떠한 용도와 방식으로
          이용되며, 보호를 위해 어떠한 조치가 취해지는지를 안내하기 위한
          것입니다.
        </p>

        <p>
          개인정보처리방침 개정 시에는 웹사이트
          <a
            href="https://www.ifa.co.kr/iFATour"
            target="_blank"
            class="text-blue-600 hover:underline"
            >https://www.ifa.co.kr/iFATour</a
          >
          을 통해 공지합니다.
        </p>

        <hr class="my-4" />

        <h3 class="font-semibold text-lg text-gray-800">
          1. 개인정보의 수집 및 이용 목적
        </h3>
        <ul class="list-disc pl-6 space-y-1">
          <li>
            여행 예약 및 서비스 제공, 요금 결제, 보험가입 등을 위한 최소한의
            필수정보 수집
          </li>
          <li>
            고객 의견 수렴, 불만처리, 고지사항 전달 등 서비스 품질 향상을 위한
            마케팅 활동
          </li>
        </ul>

        <h3 class="font-semibold text-lg text-gray-800 mt-6">
          2. 개인정보 보유 및 이용기간
        </h3>
        <ul class="list-disc pl-6 space-y-1">
          <li>계약 또는 청약철회 기록: 5년</li>
          <li>대금결제 및 재화 공급기록: 5년</li>
          <li>소비자 불만 또는 분쟁처리 기록: 3년</li>
        </ul>

        <h3 class="font-semibold text-lg text-gray-800 mt-6">
          3. 개인정보 보호 책임자
        </h3>
        <div class="bg-gray-50 border rounded-lg p-4 text-sm">
          <p><strong>담당자:</strong> 정유진 매니저</p>
          <p><strong>부서:</strong> 인사팀</p>
          <p>
            <strong>전화:</strong>
            <a href="tel:02-6203-3636" class="text-blue-600 hover:underline"
              >02-6203-3636</a
            >
          </p>
          <p>
            <strong>Email:</strong>
            <a
              href="mailto:thx2019@nate.com"
              class="text-blue-600 hover:underline"
              >thx2019@nate.com</a
            >
          </p>
        </div>

        <p class="text-xs text-gray-500 mt-6">
          ※ 자세한 내용은 공식 홈페이지에서 전체 개인정보처리방침 전문을
          확인할 수 있습니다.
        </p>
      </div>
    </template>
  </BaseModal>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'
import BaseModal from '@/components/common/BaseModal.vue'

defineProps({
  isOpen: Boolean,
})

const emit = defineEmits(['close'])
const close = () => emit('close')
</script>
