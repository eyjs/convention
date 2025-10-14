<template>
  <div class="bulk-assign-container">
    <div class="header">
      <h1>🎯 참석자 속성 일괄 매핑</h1>
      <p class="subtitle">여러 참석자에게 속성을 한번에 설정할 수 있습니다</p>
    </div>

    <div class="template-section">
      <div class="form-group">
        <label>행사 선택</label>
        <select v-model="selectedConventionId" @change="loadAttributeDefinitions" class="form-control">
          <option :value="null">행사를 선택하세요</option>
          <option v-for="conv in conventions" :key="conv.id" :value="conv.id">{{ conv.title }}</option>
        </select>
      </div>
      <div v-if="attributeDefinitions.length > 0" class="attribute-definitions">
        <h3>설정 가능한 속성 목록</h3>
        <div class="attribute-chips">
          <span v-for="attr in attributeDefinitions" :key="attr.id" class="chip">
            {{ attr.attributeKey }}
            <span v-if="attr.isRequired" class="required-badge">필수</span>
          </span>
        </div>
      </div>
    </div>

    <div v-if="selectedConventionId" class="controls">
      <div class="controls-left">
        <button @click="toggleSelectAll" class="btn btn-secondary">{{ allSelected ? '전체 선택 해제' : '전체 선택' }}</button>
        <span class="selected-count">{{ selectedGuests.length }}명 선택됨</span>
      </div>
      <div class="controls-right">
        <input v-model="searchText" type="text" placeholder="이름, 회사, 부서로 검색..." class="search-input" />
        <button @click="openBulkEditModal" :disabled="selectedGuests.length === 0" class="btn btn-primary">선택한 참석자 속성 설정</button>
      </div>
    </div>

    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>참석자 목록을 불러오는 중...</p>
    </div>

    <div v-if="!loading && filteredGuests.length > 0" class="participant-grid">
      <div v-for="guest in filteredGuests" :key="guest.id" :class="['participant-card', { selected: isSelected(guest.id) }]" @click="toggleSelection(guest.id)">
        <div class="participant-header">
          <input type="checkbox" :checked="isSelected(guest.id)" @click.stop="toggleSelection(guest.id)" />
          <div class="participant-info">
            <div class="participant-name">{{ guest.guestName }}</div>
            <div class="participant-detail">{{ guest.corpName }} {{ guest.corpPart ? `/ ${guest.corpPart}` : '' }}</div>
            <div class="participant-contact">{{ guest.telephone }}</div>
          </div>
        </div>
        <div v-if="Object.keys(guest.currentAttributes).length > 0" class="current-attributes">
          <div class="attribute-label">현재 속성:</div>
          <div class="attribute-tags">
            <span v-for="(value, key) in guest.currentAttributes" :key="key" class="attribute-tag"><strong>{{ key }}:</strong> {{ value }}</span>
          </div>
        </div>
        <div v-else class="no-attributes">속성 미설정</div>
      </div>
    </div>

    <div v-if="!loading && filteredGuests.length === 0" class="empty-state">
      <p v-if="!selectedConventionId">행사를 선택해주세요</p>
      <p v-else-if="searchText">검색 결과가 없습니다</p>
      <p v-else>등록된 참석자가 없습니다</p>
    </div>

    <div v-if="showBulkEditModal" class="modal-overlay" @click="closeBulkEditModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>참석자 속성 일괄 설정</h2>
          <button @click="closeBulkEditModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <p class="info-text"><strong>{{ selectedGuests.length }}명</strong>의 참석자에게 속성을 설정합니다</p>
          <div class="attribute-form">
            <div v-for="definition in attributeDefinitions" :key="definition.id" class="form-group">
              <label>{{ definition.attributeKey }}<span v-if="definition.isRequired" class="required">*</span></label>
              <select v-if="definition.options && definition.options.length > 0" v-model="bulkAttributes[definition.attributeKey]" class="form-control">
                <option value="">선택하세요</option>
                <option v-for="option in definition.options" :key="option" :value="option">{{ option }}</option>
              </select>
              <input v-else v-model="bulkAttributes[definition.attributeKey]" type="text" class="form-control" :placeholder="`${definition.attributeKey}를 입력하세요`" />
            </div>
          </div>
          <div class="preview-section">
            <h3>미리보기</h3>
            <div class="preview-list">
              <div v-for="guestId in selectedGuests.slice(0, 5)" :key="guestId" class="preview-item">
                <strong>{{ getGuestName(guestId) }}</strong>
                <div class="preview-attributes">
                  <span v-for="(value, key) in bulkAttributes" :key="key" v-if="value" class="preview-tag">{{ key }}: {{ value }}</span>
                </div>
              </div>
              <div v-if="selectedGuests.length > 5" class="preview-more">... 외 {{ selectedGuests.length - 5 }}명</div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="closeBulkEditModal" class="btn btn-secondary">취소</button>
          <button @click="submitBulkAssign" class="btn btn-success" :disabled="submitting">{{ submitting ? '저장 중...' : '일괄 저장' }}</button>
        </div>
      </div>
    </div>

    <div v-if="toast.show" :class="['toast', toast.type]">{{ toast.message }}</div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'BulkAssignAttributes',
  data() {
    return {
      conventions: [],
      selectedConventionId: null,
      attributeDefinitions: [],
      guests: [],
      selectedGuests: [],
      searchText: '',
      bulkAttributes: {},
      showBulkEditModal: false,
      loading: false,
      submitting: false,
      toast: { show: false, message: '', type: 'success' }
    };
  },
  computed: {
    filteredGuests() {
      if (!this.searchText) return this.guests;
      const search = this.searchText.toLowerCase();
      return this.guests.filter(g => 
        g.guestName.toLowerCase().includes(search) ||
        (g.corpName && g.corpName.toLowerCase().includes(search)) ||
        (g.corpPart && g.corpPart.toLowerCase().includes(search))
      );
    },
    allSelected() {
      return this.filteredGuests.length > 0 && this.filteredGuests.every(g => this.selectedGuests.includes(g.id));
    }
  },
  async mounted() {
    await this.loadConventions();
  },
  methods: {
    async loadConventions() {
      try {
        const response = await axios.get('/api/conventions');
        this.conventions = response.data;
      } catch (error) {
        this.showToast('행사 목록을 불러오는데 실패했습니다', 'error');
      }
    },
    async loadAttributeDefinitions() {
      if (!this.selectedConventionId) return;
      try {
        const response = await axios.get(`/api/attribute/conventions/${this.selectedConventionId}/definitions`);
        this.attributeDefinitions = response.data.map(def => ({ ...def, options: def.options ? JSON.parse(def.options) : [] }));
        await this.loadGuests();
      } catch (error) {
        this.showToast('속성 정의를 불러오는데 실패했습니다', 'error');
      }
    },
    async loadGuests() {
      this.loading = true;
      try {
        const response = await axios.get(`/api/guest/participants-with-attributes?conventionId=${this.selectedConventionId}`);
        this.guests = response.data;
        this.selectedGuests = [];
      } catch (error) {
        this.showToast('참석자 목록을 불러오는데 실패했습니다', 'error');
      } finally {
        this.loading = false;
      }
    },
    toggleSelection(guestId) {
      const index = this.selectedGuests.indexOf(guestId);
      if (index > -1) this.selectedGuests.splice(index, 1);
      else this.selectedGuests.push(guestId);
    },
    isSelected(guestId) { return this.selectedGuests.includes(guestId); },
    toggleSelectAll() {
      if (this.allSelected) this.selectedGuests = [];
      else this.selectedGuests = this.filteredGuests.map(g => g.id);
    },
    openBulkEditModal() {
      if (this.selectedGuests.length === 0) {
        this.showToast('참석자를 선택해주세요', 'warning');
        return;
      }
      this.bulkAttributes = {};
      this.attributeDefinitions.forEach(def => { this.bulkAttributes[def.attributeKey] = ''; });
      this.showBulkEditModal = true;
    },
    closeBulkEditModal() {
      this.showBulkEditModal = false;
      this.bulkAttributes = {};
    },
    getGuestName(guestId) {
      const guest = this.guests.find(g => g.id === guestId);
      return guest ? guest.guestName : '';
    },
    async submitBulkAssign() {
      const requiredAttrs = this.attributeDefinitions.filter(def => def.isRequired);
      for (const attr of requiredAttrs) {
        if (!this.bulkAttributes[attr.attributeKey]) {
          this.showToast(`${attr.attributeKey}는 필수 항목입니다`, 'warning');
          return;
        }
      }
      const filteredAttributes = Object.entries(this.bulkAttributes).filter(([_, value]) => value !== '').reduce((acc, [key, value]) => { acc[key] = value; return acc; }, {});
      if (Object.keys(filteredAttributes).length === 0) {
        this.showToast('최소 하나의 속성을 입력해주세요', 'warning');
        return;
      }
      this.submitting = true;
      try {
        const payload = { conventionId: this.selectedConventionId, guestMappings: this.selectedGuests.map(guestId => ({ guestId, attributes: filteredAttributes })) };
        const response = await axios.post('/api/guest/bulk-assign-attributes', payload);
        if (response.data.success) {
          this.showToast(response.data.message, 'success');
          this.closeBulkEditModal();
          await this.loadGuests();
          this.selectedGuests = [];
        } else {
          this.showToast(response.data.message, 'error');
        }
      } catch (error) {
        this.showToast('속성 할당에 실패했습니다', 'error');
      } finally {
        this.submitting = false;
      }
    },
    showToast(message, type = 'success') {
      this.toast = { show: true, message, type };
      setTimeout(() => { this.toast.show = false; }, 3000);
    }
  }
};
</script>

<style scoped>
.bulk-assign-container{max-width:1600px;margin:0 auto;padding:20px;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif}
.header{margin-bottom:30px;text-align:center}
.header h1{color:#2c3e50;font-size:32px;margin-bottom:10px}
.subtitle{color:#7f8c8d;font-size:16px}
.template-section{background:#f8f9fa;padding:25px;border-radius:12px;margin-bottom:25px;box-shadow:0 2px 4px rgba(0,0,0,0.05)}
.form-group{margin-bottom:20px}
.form-group label{display:block;font-weight:600;margin-bottom:8px;color:#34495e;font-size:14px}
.form-control{width:100%;padding:12px 15px;border:2px solid #dce1e6;border-radius:8px;font-size:15px;transition:all 0.3s;background:white}
.form-control:focus{outline:none;border-color:#3498db;box-shadow:0 0 0 3px rgba(52,152,219,0.1)}
.attribute-definitions{margin-top:20px;padding-top:20px;border-top:2px solid #e0e0e0}
.attribute-definitions h3{font-size:16px;color:#555;margin-bottom:15px}
.attribute-chips{display:flex;flex-wrap:wrap;gap:10px}
.chip{background:white;border:2px solid #3498db;color:#3498db;padding:8px 16px;border-radius:20px;font-size:14px;font-weight:500;display:inline-flex;align-items:center;gap:5px}
.required-badge{background:#e74c3c;color:white;padding:2px 8px;border-radius:10px;font-size:11px;font-weight:600}
.controls{display:flex;justify-content:space-between;align-items:center;background:#ecf0f1;padding:15px 20px;border-radius:10px;margin-bottom:20px;flex-wrap:wrap;gap:15px}
.controls-left,.controls-right{display:flex;align-items:center;gap:12px;flex-wrap:wrap}
.selected-count{background:#3498db;color:white;padding:8px 16px;border-radius:20px;font-weight:600;font-size:14px}
.search-input{padding:10px 15px;border:2px solid #dce1e6;border-radius:8px;font-size:14px;min-width:250px;transition:border-color 0.3s}
.search-input:focus{outline:none;border-color:#3498db}
.btn{padding:10px 20px;border:none;border-radius:8px;cursor:pointer;font-size:14px;font-weight:600;transition:all 0.3s;display:inline-flex;align-items:center;gap:5px}
.btn:hover{transform:translateY(-2px);box-shadow:0 4px 8px rgba(0,0,0,0.15)}
.btn:disabled{opacity:0.5;cursor:not-allowed;transform:none}
.btn-primary{background:#3498db;color:white}
.btn-primary:hover:not(:disabled){background:#2980b9}
.btn-success{background:#27ae60;color:white}
.btn-success:hover:not(:disabled){background:#229954}
.btn-secondary{background:#95a5a6;color:white}
.btn-secondary:hover:not(:disabled){background:#7f8c8d}
.loading{text-align:center;padding:60px 20px;color:#7f8c8d}
.spinner{border:4px solid #f3f3f3;border-top:4px solid #3498db;border-radius:50%;width:50px;height:50px;animation:spin 1s linear infinite;margin:0 auto 20px}
@keyframes spin{0%{transform:rotate(0deg)}100%{transform:rotate(360deg)}}
.participant-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(350px,1fr));gap:20px;max-height:800px;overflow-y:auto;padding:15px;background:#fafbfc;border-radius:10px}
.participant-card{background:white;border:2px solid #e1e8ed;border-radius:12px;padding:20px;cursor:pointer;transition:all 0.3s;position:relative}
.participant-card:hover{box-shadow:0 6px 16px rgba(0,0,0,0.1);transform:translateY(-3px)}
.participant-card.selected{border-color:#3498db;background:#e7f3ff;box-shadow:0 4px 12px rgba(52,152,219,0.2)}
.participant-header{display:flex;align-items:flex-start;gap:12px;margin-bottom:15px}
.participant-header input[type="checkbox"]{width:20px;height:20px;cursor:pointer;margin-top:3px;flex-shrink:0}
.participant-info{flex:1}
.participant-name{font-size:18px;font-weight:700;color:#2c3e50;margin-bottom:6px}
.participant-detail{font-size:14px;color:#7f8c8d;margin-bottom:4px}
.participant-contact{font-size:13px;color:#95a5a6}
.current-attributes{background:#f8f9fa;padding:12px;border-radius:8px;margin-top:12px}
.attribute-label{font-size:12px;font-weight:600;color:#7f8c8d;margin-bottom:8px;text-transform:uppercase;letter-spacing:0.5px}
.attribute-tags{display:flex;flex-wrap:wrap;gap:8px}
.attribute-tag{background:white;border:1px solid #ddd;padding:6px 12px;border-radius:6px;font-size:13px;color:#555}
.attribute-tag strong{color:#3498db;margin-right:4px}
.no-attributes{text-align:center;padding:15px;color:#bdc3c7;font-size:13px;font-style:italic;background:#f8f9fa;border-radius:8px;margin-top:12px}
.empty-state{text-align:center;padding:80px 20px;color:#95a5a6;font-size:16px}
.modal-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.6);display:flex;align-items:center;justify-content:center;z-index:9999;padding:20px}
.modal-content{background:white;border-radius:16px;max-width:800px;width:100%;max-height:90vh;overflow-y:auto;box-shadow:0 10px 40px rgba(0,0,0,0.2)}
.modal-header{display:flex;justify-content:space-between;align-items:center;padding:25px 30px;border-bottom:2px solid #ecf0f1}
.modal-header h2{font-size:24px;color:#2c3e50;margin:0}
.close-btn{background:none;border:none;font-size:32px;color:#95a5a6;cursor:pointer;line-height:1;transition:color 0.3s}
.close-btn:hover{color:#e74c3c}
.modal-body{padding:30px}
.info-text{background:#e7f3ff;border-left:4px solid #3498db;padding:15px;border-radius:6px;margin-bottom:25px;color:#2c3e50}
.info-text strong{color:#3498db;font-size:18px}
.attribute-form{margin-bottom:30px}
.required{color:#e74c3c;margin-left:4px}
.preview-section{background:#f8f9fa;padding:20px;border-radius:10px;margin-top:25px}
.preview-section h3{font-size:16px;color:#555;margin-bottom:15px}
.preview-list{display:flex;flex-direction:column;gap:12px}
.preview-item{background:white;padding:15px;border-radius:8px;border:1px solid #e0e0e0}
.preview-item strong{display:block;color:#2c3e50;margin-bottom:8px;font-size:15px}
.preview-attributes{display:flex;flex-wrap:wrap;gap:8px}
.preview-tag{background:#e7f3ff;color:#3498db;padding:6px 12px;border-radius:6px;font-size:13px;font-weight:500}
.preview-more{text-align:center;color:#7f8c8d;font-style:italic;padding:10px}
.modal-footer{display:flex;justify-content:flex-end;gap:12px;padding:20px 30px;border-top:2px solid #ecf0f1;background:#f8f9fa}
.toast{position:fixed;top:20px;right:20px;padding:16px 24px;border-radius:8px;color:white;font-weight:600;box-shadow:0 4px 12px rgba(0,0,0,0.15);z-index:10000;animation:slideIn 0.3s ease-out;max-width:400px}
@keyframes slideIn{from{transform:translateX(400px);opacity:0}to{transform:translateX(0);opacity:1}}
.toast.success{background:#27ae60}
.toast.error{background:#e74c3c}
.toast.warning{background:#f39c12}
@media (max-width:768px){
.controls{flex-direction:column;align-items:stretch}
.controls-left,.controls-right{width:100%;justify-content:center}
.search-input{width:100%;min-width:unset}
.participant-grid{grid-template-columns:1fr}
.modal-content{margin:10px}
}
</style>
