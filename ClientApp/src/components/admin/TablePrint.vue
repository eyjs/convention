<template>
    <div class="flex flex-col md:flex-row h-screen w-full overflow-hidden bg-gray-100 font-sans-kr">
        <!-- [좌측] 설정 패널 -->
        <aside class="no-print w-full md:w-96 bg-white border-r border-gray-200 flex flex-col h-full z-50 shadow-xl shrink-0">
            <div class="p-6 bg-slate-800 text-white shrink-0">
                <h1 class="text-xl font-bold flex items-center gap-2">
                    <span class="material-symbols-outlined">print</span>
                    명찰/안내문 인쇄기
                </h1>
                <p class="text-xs text-slate-300 mt-2"></p>
            </div>

            <div class="flex-1 overflow-y-auto p-6 space-y-8">
                <!-- 1. 참석자 업로드 -->
                <div class="space-y-3">
                    <div class="flex items-center justify-between">
                        <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wide">1. 참석자 업로드</h2>
                        <button @click="downloadTemplate"
                                class="text-xs text-blue-600 hover:underline font-medium flex items-center gap-1">
                            <span class="material-symbols-outlined text-[14px]">다운로드</span> 양식 다운로드
                        </button>
                    </div>
                    <div class="bg-blue-50 border border-blue-100 rounded-lg p-4">
                        <input type="file"
                               @change="handleExcelUpload"
                               accept=".xlsx, .xls"
                               class="block w-full text-xs text-gray-500 file:mr-2 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-xs file:font-semibold file:bg-blue-600 file:text-white hover:file:bg-blue-700 cursor-pointer" />
                        <p class="mt-2 text-[11px] text-blue-800">* 양식 파일(.xlsx)을 업로드하면 자동으로 데이터가 로드됩니다.</p>
                    </div>
                </div>

                <hr class="border-gray-100" />

                <!-- 2. 인쇄 모드 -->
                <div class="space-y-3">
                    <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wide">2. 인쇄 모드</h2>
                    <div class="flex flex-col gap-2">
                        <label class="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition has-[:checked]:border-blue-500 has-[:checked]:bg-blue-50">
                            <input type="radio"
                                   v-model="config.mode"
                                   value="table"
                                   class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500" />
                            <div class="ml-3">
                                <span class="block text-sm font-medium text-gray-900">테이블 안내문 (Table Sign)</span>
                                <span class="block text-xs text-gray-500">A4 용지 1장에 전체 좌석 표시</span>
                            </div>
                        </label>
                        <label class="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition has-[:checked]:border-blue-500 has-[:checked]:bg-blue-50">
                            <input type="radio"
                                   v-model="config.mode"
                                   value="individual"
                                   class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500" />
                            <div class="ml-3">
                                <span class="block text-sm font-medium text-gray-900">개별 명찰 (1인 1개)</span>
                                <span class="block text-xs text-gray-500">A4 1장에 4명씩 인쇄 (절취선 포함)</span>
                            </div>
                        </label>
                    </div>
                </div>

                <hr class="border-gray-100" />

                <!-- 3. 좌석 배치 설정 -->
                <div class="space-y-3 transition-opacity duration-300"
                     :class="{ 'opacity-30 pointer-events-none': config.mode === 'individual' }">
                    <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wide flex items-center gap-2">
                        3. 좌석 배치 설정
                    </h2>
                    <div class="grid grid-cols-1 gap-2">
                        <label class="flex items-center p-2 border rounded cursor-pointer hover:bg-gray-50 has-[:checked]:bg-blue-50 has-[:checked]:border-blue-500">
                            <input type="radio" v-model="config.seatLayout" value="auto" class="text-blue-600 focus:ring-blue-500" />
                            <span class="ml-2 text-xs font-medium">자동 (인원수에 맞춰 조절)</span>
                        </label>
                        <label class="flex items-center p-2 border rounded cursor-pointer hover:bg-gray-50 has-[:checked]:bg-blue-50 has-[:checked]:border-blue-500">
                            <input type="radio" v-model="config.seatLayout" value="4" class="text-blue-600 focus:ring-blue-500" />
                            <span class="ml-2 text-xs font-medium">4인용 배치 (2x2)</span>
                        </label>
                        <label class="flex items-center p-2 border rounded cursor-pointer hover:bg-gray-50 has-[:checked]:bg-blue-50 has-[:checked]:border-blue-500">
                            <input type="radio" v-model="config.seatLayout" value="6" class="text-blue-600 focus:ring-blue-500" />
                            <span class="ml-2 text-xs font-medium">6인용 배치 (3x2)</span>
                        </label>
                    </div>
                </div>

                <hr class="border-gray-100" />

                <!-- 4. 스타일 설정 -->
                <div class="space-y-4">
                    <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wide">4. 스타일 설정</h2>
                    <div>
                        <label class="block text-xs font-medium text-gray-500 mb-1">행사명</label>
                        <input type="text"
                               v-model="config.eventName"
                               class="w-full text-sm border-gray-300 rounded-md shadow-sm border p-2 focus:ring-2 focus:ring-slate-500 outline-none" />
                    </div>
                    <div>
                        <label class="block text-xs font-medium text-gray-500 mb-1">로고 URL (선택)</label>
                        <input type="text"
                               v-model="config.logoUrl"
                               placeholder="https://..."
                               class="w-full text-xs border-gray-300 rounded-md shadow-sm border p-2 focus:ring-2 focus:ring-slate-500 outline-none" />
                        <p class="mt-1 text-[10px] text-gray-400">권장 규격: <strong>가로 500px 이상 (배경없는 PNG)</strong></p>
                    </div>
                    <div>
                        <label class="block text-xs font-medium text-gray-500 mb-2">테마 색상</label>
                        <div class="flex gap-2">
                            <button v-for="(style, name) in themes"
                                    :key="name"
                                    @click="config.theme = name"
                                    class="w-6 h-6 rounded-full ring-1 ring-offset-1 ring-transparent hover:ring-gray-400"
                                    :class="[style.accent, { 'ring-gray-400': config.theme === name }]"></button>
                        </div>
                    </div>
                    <div>
                        <label class="block text-xs font-medium text-gray-500 mb-2">폰트 선택</label>
                        <div class="grid grid-cols-2 gap-2">
                            <button @click="config.font = 'font-sans-kr'"
                                    :class="{'ring-2 ring-blue-500 bg-blue-50': config.font === 'font-sans-kr'}"
                                    class="border rounded px-3 py-2 text-xs hover:bg-gray-50 font-sans-kr">
                                본고딕
                            </button>
                            <button @click="config.font = 'font-serif-kr'"
                                    :class="{'ring-2 ring-blue-500 bg-blue-50': config.font === 'font-serif-kr'}"
                                    class="border rounded px-3 py-2 text-xs hover:bg-gray-50 font-serif-kr">
                                나눔명조
                            </button>
                        </div>
                    </div>
                </div>

                <hr class="border-gray-100" />

                <!-- 5. 배경 이미지 설정 -->
                <div class="space-y-3">
                    <h2 class="text-sm font-bold text-gray-700 uppercase tracking-wide flex items-center gap-2">
                        5. 배경 이미지 설정
                    </h2>

                    <!-- 전체 배경 -->
                    <div class="bg-gray-50 border border-gray-200 rounded-lg p-3">
                        <label class="block text-xs font-semibold text-gray-600 mb-2">1. 전체 배경 (A4 용지)</label>
                        <input type="file"
                               @change="(e) => handleImageUpload(e, 'full')"
                               accept="image/*"
                               ref="fullBgInput"
                               class="block w-full text-xs text-gray-500 file:mr-2 file:py-1.5 file:px-3 file:rounded file:border-0 file:text-xs file:font-semibold file:bg-gray-600 file:text-white hover:file:bg-gray-700 cursor-pointer" />
                        <div class="flex flex-col gap-1 mt-2">
                            <span class="text-[10px] text-gray-500">용지 전체에 깔리는 배경 이미지입니다.</span>
                            <span class="text-[10px] text-blue-600 font-medium">권장 규격: 3508 x 2480 px (300dpi)</span>
                            <button @click="clearBg('full')" class="text-[10px] text-red-500 hover:underline self-end">삭제</button>
                        </div>
                    </div>

                    <!-- 카드 배경 -->
                    <div class="bg-gray-50 border border-gray-200 rounded-lg p-3">
                        <label class="block text-xs font-semibold text-gray-600 mb-2">2. 카드 배경 (박스 내부)</label>
                        <input type="file"
                               @change="(e) => handleImageUpload(e, 'card')"
                               accept="image/*"
                               ref="cardBgInput"
                               class="block w-full text-xs text-gray-500 file:mr-2 file:py-1.5 file:px-3 file:rounded file:border-0 file:text-xs file:font-semibold file:bg-gray-600 file:text-white hover:file:bg-gray-700 cursor-pointer" />
                        <div class="flex flex-col gap-1 mt-2">
                            <span class="text-[10px] text-gray-500">이름표/좌석표 박스 배경 이미지입니다.</span>
                            <span class="text-[10px] text-blue-600 font-medium">권장 규격: 1754 x 1240 px (300dpi)</span>
                            <button @click="clearBg('card')" class="text-[10px] text-red-500 hover:underline self-end">삭제</button>
                        </div>
                    </div>
                </div>
            </div>

            <div class="p-4 border-t border-gray-200 bg-gray-50 shrink-0 space-y-2">
                <div class="flex justify-between items-end text-xs text-gray-500 px-1">
                    <span>총 {{ displayPages.length }} 페이지</span>
                    <span>상태: {{ statusMessage }}</span>
                </div>
                <button @click="printPage"
                        class="w-full bg-slate-900 hover:bg-slate-800 text-white font-bold py-3 px-4 rounded-lg shadow-lg flex items-center justify-center gap-2 transition-all active:scale-95">
                    <span class="material-symbols-outlined">print</span>
                    인쇄하기
                </button>
            </div>
        </aside>

        <!-- [우측] 미리보기 영역 -->
        <main class="flex-1 overflow-auto bg-gray-200/80 p-8 md:p-12 relative flex justify-center">
            <div id="previewContainer" class="flex flex-col items-center gap-8 w-fit">
                <!-- A4 페이지 렌더링 -->
                <div v-for="(page, pageIndex) in displayPages"
                     :key="pageIndex"
                     class="page-sheet"
                     :class="[config.font, config.mode === 'individual' ? 'mode-individual' : 'mode-table']"
                     :style="pageStyle">
                    <!-- [MODE 1] 개별 명찰 모드 (4분할) -->
                    <template v-if="config.mode === 'individual'">
                        <div class="cut-line-vertical"></div>
                        <div class="cut-line-horizontal"></div>

                        <div v-for="(guest, idx) in page.items" :key="idx" class="card-cell" :style="cardStyle">
                            <template v-if="guest">
                                <!-- 상단 (접히는 부분) -->
                                <div class="tent-top grayscale opacity-60">
                                    <div class="text-[10px] font-bold text-gray-500 mb-1 tracking-widest uppercase">
                                        {{ config.eventName }}
                                    </div>
                                    <img v-if="config.logoUrl" :src="config.logoUrl" class="h-6 mb-2 object-contain opacity-80" />
                                    <div class="text-3xl font-bold mb-1 whitespace-nowrap" :class="currentTheme.text">
                                        {{ guest.name }}
                                    </div>
                                    <div class="text-sm text-gray-600 font-medium">{{ guest.title }}</div>
                                    <div v-if="guest.table"
                                         class="mt-2 text-[10px] px-2 py-0.5 border border-gray-400 rounded text-gray-500">
                                        Table {{ guest.table }}
                                    </div>
                                </div>
                                <div class="fold-line"></div>
                                <!-- 하단 (보여지는 부분) -->
                                <div class="tent-bottom">
                                    <div class="text-[10px] font-bold text-gray-400 mb-2 tracking-[0.25em] uppercase">
                                        {{ config.eventName }}
                                    </div>
                                    <img v-if="config.logoUrl" :src="config.logoUrl" class="h-6 mb-2 object-contain opacity-80" />
                                    <div class="text-5xl font-black mb-2 leading-tight whitespace-nowrap tracking-tight"
                                         :class="currentTheme.text">
                                        {{ guest.name }}
                                    </div>
                                    <div class="text-lg font-bold text-gray-700">{{ guest.title }}</div>
                                    <div v-if="guest.table"
                                         class="mt-4 text-xl font-bold border-2 rounded-lg px-4 py-1 shadow-sm bg-white/50 backdrop-blur-sm"
                                         :class="[currentTheme.text, currentTheme.border]">
                                        Table {{ guest.table }}
                                    </div>
                                </div>
                            </template>
                            <template v-else>
                                <span class="text-gray-200 text-sm select-none">빈 칸</span>
                            </template>
                        </div>
                    </template>

                    <!-- [MODE 2] 테이블 안내문 모드 (좌석 배치) -->
                    <template v-else>
                        <div class="table-header" :class="currentTheme.border">
                            <div class="flex flex-col">
                                <span class="text-xs font-bold text-gray-500 uppercase tracking-widest mb-1">{{ config.eventName }}</span>
                                <span class="text-5xl font-black" :class="currentTheme.text">{{ page.tableName }}</span>
                            </div>
                            <img v-if="config.logoUrl" :src="config.logoUrl" class="h-10 mb-2 object-contain" />
                        </div>

                        <div class="seat-chart-grid" :class="page.layoutClass">
                            <div v-for="(guest, idx) in page.items"
                                 :key="idx"
                                 class="seat-card transition"
                                 :class="[guest.isEmpty ? 'empty-seat' : `${currentTheme.border} hover:bg-gray-50`]"
                                 :style="guest.isEmpty ? '' : cardStyle">
                                <template v-if="!guest.isEmpty">
                                    <span class="seat-card-title">{{ guest.title }}</span>
                                    <span class="seat-card-name" :class="currentTheme.text">{{ guest.name }}</span>
                                </template>
                            </div>
                        </div>

                        <div class="mt-2 text-right w-full text-[10px] text-gray-400">총 {{ page.realCount }}명</div>
                    </template>
                </div>
            </div>

            <div class="no-print fixed bottom-6 left-1/2 md:left-[calc(50%+192px)] transform -translate-x-1/2 text-center pointer-events-none">
                <span class="bg-black/70 text-white text-xs px-4 py-2 rounded-full backdrop-blur-sm shadow-lg">
                    인쇄 시 A4 가로(landscape)에 자동 최적화됩니다.
                </span>
            </div>
        </main>
    </div>
</template>

<script setup>
    import { ref, reactive, computed } from 'vue';
    import { uploadAPI } from '@/services/api';

    const props = defineProps({
        conventionId: {
            type: Number,
            required: true,
        },
    });

    // ----------------------------------------------------------------------------
    // 1. 상태 및 설정
    // ----------------------------------------------------------------------------
    const config = reactive({
        mode: 'table', // 'individual' | 'table'
        seatLayout: 'auto', // 'auto' | '4' | '6'
        eventName: '2025 글로벌 컨퍼런스',
        logoUrl: '',
        theme: 'indigo',
        font: 'font-sans-kr',
    });

    const guestsData = ref([
        // { name: '홍길동', title: '대표이사', table: 'VIP Table' },
    ]);

    const bgImages = reactive({
        full: null,
        card: null,
    });

    const themes = {
        indigo: { text: 'text-indigo-900', border: 'border-indigo-900', bg: 'bg-indigo-50', accent: 'bg-indigo-900' },
        red: { text: 'text-red-900', border: 'border-red-900', bg: 'bg-red-50', accent: 'bg-red-900' },
        green: { text: 'text-emerald-900', border: 'border-emerald-900', bg: 'bg-emerald-50', accent: 'bg-emerald-900' },
        black: { text: 'text-gray-900', border: 'border-gray-900', bg: 'bg-gray-50', accent: 'bg-gray-900' },
        gold: { text: 'text-yellow-700', border: 'border-yellow-700', bg: 'bg-yellow-50', accent: 'bg-yellow-700' },
    };

    // ----------------------------------------------------------------------------
    // 2. Computed Properties (계산된 속성)
    // ----------------------------------------------------------------------------
    const currentTheme = computed(() => themes[config.theme]);

    const statusMessage = computed(() => {
        if (config.mode === 'individual') return `참석자 ${guestsData.value.length}명`;
        const tableCount = new Set(guestsData.value.map((g) => g.table || 'No Table')).size;
        return `테이블 ${tableCount}개`;
    });

    const pageStyle = computed(() => {
        return bgImages.full ? { backgroundImage: `url('${bgImages.full}')` } : {};
    });

    const cardStyle = computed(() => {
        return bgImages.card ? { backgroundImage: `url('${bgImages.card}')` } : {};
    });

    const displayPages = computed(() => {
        // [MODE 1] 개별 명찰 (4명 분할)
        if (config.mode === 'individual') {
            const pages = [];
            const itemsPerPage = 4;
            for (let i = 0; i < guestsData.value.length; i += itemsPerPage) {
                const chunk = guestsData.value.slice(i, i + itemsPerPage);
                // 4칸 채우기
                while (chunk.length < 4) chunk.push(null);
                pages.push({ items: chunk });
            }
            return pages;
        }

        // [MODE 2] 테이블 안내문 (테이블별 그룹화)
        else {
            const tableGroups = {};
            guestsData.value.forEach((guest) => {
                const tName = guest.table || 'No Table';
                if (!tableGroups[tName]) tableGroups[tName] = [];
                tableGroups[tName].push(guest);
            });

            const sortedTables = Object.keys(tableGroups).sort();

            return sortedTables.map((tableName) => {
                let members = [...tableGroups[tableName]];
                const realCount = members.length;
                let targetCount = members.length;
                let layoutClass = '';

                // 레이아웃 결정
                if (config.seatLayout === '4') {
                    targetCount = 4;
                    layoutClass = 'grid-cols-2 grid-rows-2';
                } else if (config.seatLayout === '6') {
                    targetCount = 6;
                    layoutClass = 'grid-cols-3 grid-rows-2';
                } else {
                    // Auto
                    if (members.length <= 2) {
                        layoutClass = 'grid-cols-2 grid-rows-1';
                        targetCount = Math.max(members.length, 1);
                    } else if (members.length <= 4) {
                        layoutClass = 'grid-cols-2 grid-rows-2';
                        targetCount = 4;
                    } else {
                        layoutClass = 'grid-cols-3 grid-rows-2';
                        targetCount = Math.max(6, members.length);
                    }
                }

                // 빈 박스 채우기
                while (members.length < targetCount) {
                    members.push({ isEmpty: true });
                }

                return {
                    tableName,
                    realCount,
                    layoutClass,
                    items: members,
                };
            });
        }
    });

    // ----------------------------------------------------------------------------
    // 3. 핸들러 함수
    // ----------------------------------------------------------------------------
    const handleExcelUpload = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        if (!props.conventionId) {
            alert('행사 ID가 유효하지 않습니다.');
            return;
        }

        try {
            const response = await uploadAPI.uploadNameTags(props.conventionId, file);
            if (response.data.success) {
                guestsData.value = response.data.data;
                alert(`성공: ${response.data.successCount}명의 데이터를 불러왔습니다.`);
            } else {
                const errorMsg = response.data.errors?.join('\n') || '알 수 없는 오류';
                alert(`오류가 발생했습니다:\n${errorMsg}`);
            }
        } catch (error) {
            console.error('Failed to upload name tags:', error);
            const errorMsg = error.response?.data?.error || error.message || '서버 통신 중 오류가 발생했습니다.';
            alert(`오류: ${errorMsg}`);
        } finally {
            e.target.value = ''; // Reset file input
        }
    };

    const downloadTemplate = () => {
        // Public 폴더에 있는 실제 샘플 파일 경로로 수정
        const link = document.createElement('a');
        link.href = '/sample/명찰_일괄등록_양식.xlsx';
        link.setAttribute('download', '명찰_일괄등록_양식.xlsx');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const handleImageUpload = (e, type) => {
        const file = e.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (evt) => {
            bgImages[type] = evt.target.result;
        };
        reader.readAsDataURL(file);
    };

    const clearBg = (type) => {
        bgImages[type] = null;
        if (type === 'full') {
            const fullBgInput = document.querySelector('input[type="file"][accept="image/*"]:first-of-type');
            if(fullBgInput) fullBgInput.value = '';
        } else {
            const cardBgInput = document.querySelector('input[type="file"][accept="image/*"]:last-of-type');
            if(cardBgInput) cardBgInput.value = '';
        }
    };

    const printPage = () => {
        window.print();
    };
</script>

<style scoped>
/* A4 Landscape Layout for Printing */
@media print {
    .page-sheet {
        width: 297mm;
        height: 210mm;
        margin: 0;
        box-shadow: none;
        border: none;
    }
}

/* Base Page Sheet for Screen */
.page-sheet {
    width: 297mm;
    height: 210mm;
    background-color: white;
    box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1);
    border: 1px solid #e5e7eb;
    overflow: hidden;
    position: relative;
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}

/* Individual Mode: 4 cards per page */
.mode-individual {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    grid-template-rows: repeat(2, 1fr);
}

.card-cell {
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}

.fold-line {
    position: absolute;
    width: 100%;
    height: 1px;
    background-image: linear-gradient(to right, #9ca3af 33%, transparent 0%);
    background-size: 8px 1px;
    background-repeat: repeat-x;
    top: 50%;
    left: 0;
    transform: translateY(-50%);
}

.cut-line-vertical {
    position: absolute;
    width: 1px;
    height: 100%;
    background-image: linear-gradient(to bottom, #9ca3af 33%, transparent 0%);
    background-size: 1px 8px;
    background-repeat: repeat-y;
    top: 0;
    left: 50%;
    transform: translateX(-50%);
}
.cut-line-horizontal {
    position: absolute;
    width: 100%;
    height: 1px;
    background-image: linear-gradient(to right, #9ca3af 33%, transparent 0%);
    background-size: 8px 1px;
    background-repeat: repeat-x;
    top: 50%;
    left: 0;
    transform: translateY(-50%);
}


.tent-top, .tent-bottom {
    width: 100%;
    height: 50%;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    padding: 20px;
    box-sizing: border-box;
}

.tent-top {
    transform: rotate(180deg);
}


/* Table Mode */
.mode-table {
    display: flex;
    flex-direction: column;
    padding: 40px;
    box-sizing: border-box;
    align-items: center;
}
.table-header {
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom-width: 4px;
    padding-bottom: 16px;
    margin-bottom: 24px;
}
.seat-chart-grid {
    display: grid;
    gap: 16px;
    width: 100%;
    flex-grow: 1;
}

.seat-card {
    border-width: 2px;
    border-radius: 0.5rem;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    padding: 8px;
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}
.empty-seat {
    background-color: #f3f4f6;
    border: 2px dashed #d1d5db;
}

.seat-card-title {
    font-size: 0.875rem;
    color: #4b5563;
}
.seat-card-name {
    font-size: 1.5rem; /* 24px */
    font-weight: 800;
}


/* No-Print Utility */
@media print {
    .no-print {
        display: none;
    }
}
</style>