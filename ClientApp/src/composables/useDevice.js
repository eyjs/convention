
import { ref, onMounted } from 'vue';

/**
 * 터치 장치 여부를 감지하는 Vue Composable
 */
export function useDevice() {
  const isTouchDevice = ref(false);

  onMounted(() => {
    if (typeof window !== 'undefined') {
      // 'pointer: coarse'는 마우스처럼 정밀하지 않은 포인터(예: 손가락)를 감지하는 가장 현대적인 방법입니다.
      isTouchDevice.value = window.matchMedia('(pointer: coarse)').matches;
    }
  });

  return { isTouchDevice };
}
