<template>
  <div class="h-[calc(100vh-3.5rem)] overflow-y-auto">
    <div class="max-w-4xl mx-auto p-6 space-y-6">

      <!-- Page header -->
      <div>
        <h1 class="text-2xl font-bold text-white">系統設定</h1>
        <p class="text-sm text-gray-500 mt-0.5">管理米種主檔與每日牌價</p>
      </div>

      <!-- Tab navigation -->
      <div class="flex gap-1 p-1 bg-white/5 rounded-xl w-fit">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          class="px-5 py-2 rounded-lg text-sm font-medium transition-all duration-150"
          :class="activeTab === tab.id
            ? 'bg-brand-600/30 text-brand-300 ring-1 ring-brand-500/30'
            : 'text-gray-400 hover:text-gray-200 hover:bg-white/5'"
          @click="activeTab = tab.id"
        >
          {{ tab.label }}
        </button>
      </div>

      <!-- ── Tab: 米種管理 ───────────────────────────────────── -->
      <template v-if="activeTab === 'rice-types'">
        <div class="glass overflow-hidden">
          <!-- Header -->
          <div class="flex items-center justify-between px-5 py-4 border-b border-white/10">
            <h2 class="text-sm font-semibold text-gray-300">米種主檔</h2>
            <button id="add-rice-type-btn" class="btn-primary btn-sm" @click="openAddModal">
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
              </svg>
              新增米種
            </button>
          </div>

          <!-- Loading -->
          <div v-if="rtStore.loading" class="p-6 space-y-3">
            <div v-for="i in 4" :key="i" class="h-14 bg-white/5 rounded-lg animate-pulse"></div>
          </div>

          <!-- List -->
          <div v-else class="divide-y divide-white/5">
            <div
              v-for="rt in rtStore.riceTypes"
              :key="rt.id"
              class="flex items-center gap-4 px-5 py-3.5 hover:bg-white/3 transition-colors"
            >
              <!-- Status dot -->
              <div class="w-2 h-2 rounded-full flex-shrink-0"
                :class="rt.isActive ? 'bg-green-400' : 'bg-gray-600'"></div>

              <!-- Name (inline editable) -->
              <div class="flex-1 min-w-0">
                <template v-if="editing.id === rt.id">
                  <input
                    :id="`edit-rice-${rt.id}`"
                    v-model="editing.name"
                    type="text"
                    class="input-base py-1 text-sm"
                    placeholder="米種名稱"
                    @keydown.enter="saveEdit(rt)"
                    @keydown.esc="cancelEdit"
                    @blur="saveEdit(rt)"
                  />
                </template>
                <template v-else>
                  <p class="text-sm font-medium text-gray-200">{{ rt.name }}</p>
                  <p class="text-xs text-gray-600 mt-0.5">
                    {{ rt.isActive ? '啟用中' : '已停用' }}
                    <span v-if="rt.todayPrice != null" class="ml-2 text-brand-500">
                      今日 ${{ rt.todayPrice }} / 台斤
                    </span>
                    <span v-else class="ml-2 text-red-600">未設定今日牌價</span>
                  </p>
                </template>
              </div>

              <!-- Actions -->
              <div class="flex items-center gap-1.5 flex-shrink-0">
                <!-- Toggle active -->
                <ToggleSwitch
                  :id="`toggle-${rt.id}`"
                  :model-value="rt.isActive"
                  label="啟用"
                  @update:model-value="toggleActive(rt, $event)"
                />

                <!-- Edit -->
                <button
                  v-if="editing.id !== rt.id"
                  :id="`edit-btn-${rt.id}`"
                  class="btn-ghost p-1.5 text-gray-500 hover:text-brand-300"
                  title="編輯名稱"
                  @click="startEdit(rt)"
                >
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                  </svg>
                </button>
                <button v-else class="btn-ghost p-1.5 text-gray-500 hover:text-red-400" @click="cancelEdit">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>

                <!-- Delete -->
                <button
                  :id="`delete-btn-${rt.id}`"
                  class="btn-ghost p-1.5 text-gray-600 hover:text-red-400"
                  title="刪除米種"
                  @click="confirmDelete(rt)"
                >
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                  </svg>
                </button>
              </div>
            </div>

            <div v-if="rtStore.riceTypes.length === 0" class="py-12 text-center text-gray-600">
              <p class="text-sm">尚無米種，請新增</p>
            </div>
          </div>
        </div>

        <!-- Error banner -->
        <Transition name="slide-up">
          <div v-if="rtError" class="px-4 py-3 rounded-xl bg-red-500/10 border border-red-500/30 text-red-300 text-sm flex items-center gap-2">
            <svg class="w-4 h-4 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"/>
            </svg>
            {{ rtError }}
            <button class="ml-auto text-red-400 hover:text-red-300" @click="rtError = ''">✕</button>
          </div>
        </Transition>
      </template>

      <!-- ── Tab: 牌價設定 ───────────────────────────────────── -->
      <template v-if="activeTab === 'prices'">
        <div class="glass overflow-hidden">
          <div class="flex items-center justify-between px-5 py-4 border-b border-white/10">
            <div>
              <h2 class="text-sm font-semibold text-gray-300">每日牌價設定</h2>
              <p class="text-xs text-gray-600 mt-0.5">設定指定日期的米種收購單價（元/台斤）</p>
            </div>
            <!-- Date picker for price -->
            <div class="flex items-center gap-2 glass px-3 py-1.5">
              <svg class="w-3.5 h-3.5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
              </svg>
              <input
                id="price-date"
                v-model="priceDate"
                type="date"
                class="bg-transparent text-sm text-gray-200 focus:outline-none"
                @change="loadPriceLogs"
              />
            </div>
          </div>

          <!-- Price rows -->
          <div v-if="rtStore.loading" class="p-6 space-y-3">
            <div v-for="i in 4" :key="i" class="h-16 bg-white/5 rounded-lg animate-pulse"></div>
          </div>

          <div v-else class="divide-y divide-white/5">
            <div
              v-for="rt in rtStore.riceTypes.filter(r => r.isActive)"
              :key="rt.id"
              class="flex items-center gap-4 px-5 py-4"
            >
              <div class="flex-1">
                <p class="text-sm font-medium text-gray-200">{{ rt.name }}</p>
                <p class="text-xs text-gray-600 mt-0.5">
                  <span v-if="priceDate === today">今日牌價</span>
                  <span v-else>{{ priceDate }} 牌價</span>
                </p>
              </div>

              <!-- Price input -->
              <div class="flex items-center gap-2">
                <span class="text-gray-500 text-sm">$</span>
                <input
                  :id="`price-${rt.id}`"
                  v-model="priceInputs[rt.id]"
                  type="number"
                  inputmode="decimal"
                  step="0.1"
                  min="0"
                  placeholder="0.0"
                  class="input-base w-28 text-right font-mono"
                  @focus="$event.target.select()"
                />
                <span class="text-gray-500 text-xs">/ 台斤</span>
              </div>

              <button
                :id="`save-price-${rt.id}`"
                class="btn-primary btn-sm"
                :disabled="savingPrice[rt.id]"
                @click="savePrice(rt.id)"
              >
                <svg v-if="savingPrice[rt.id]" class="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                </svg>
                <svg v-else class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                </svg>
                儲存
              </button>

              <!-- Saved indicator -->
              <Transition name="fade">
                <svg v-if="savedIndicator[rt.id]" class="w-4 h-4 text-green-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </Transition>
            </div>

            <div v-if="rtStore.riceTypes.filter(r => r.isActive).length === 0"
              class="py-12 text-center text-gray-600 text-sm">
              無啟用米種，請至「米種管理」頁面新增並啟用
            </div>
          </div>
        </div>

        <!-- Price history -->
        <div class="glass overflow-hidden">
          <div class="px-5 py-4 border-b border-white/10">
            <h2 class="text-sm font-semibold text-gray-300">牌價歷史</h2>
          </div>
          <div v-if="priceLogs.length === 0" class="py-8 text-center text-gray-600 text-sm">
            {{ priceDate }} 尚無牌價紀錄
          </div>
          <table v-else class="w-full text-sm">
            <thead>
              <tr class="border-b border-white/10 text-left">
                <th class="px-5 py-2.5 text-xs font-medium text-gray-500">米種</th>
                <th class="px-5 py-2.5 text-xs font-medium text-gray-500 text-right">單價 (元/台斤)</th>
                <th class="px-5 py-2.5 text-xs font-medium text-gray-500">建立人</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in priceLogs" :key="log.id" class="border-b border-white/5">
                <td class="px-5 py-3 text-gray-200">{{ log.riceTypeName }}</td>
                <td class="px-5 py-3 font-mono text-right text-brand-300 font-semibold">${{ log.unitPrice }}</td>
                <td class="px-5 py-3 text-xs text-gray-500">{{ log.createdBy }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>

    </div>
  </div>

  <!-- Add rice type modal (inline) -->
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="showAddModal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        @click.self="showAddModal = false"
      >
        <Transition name="slide-up">
          <div v-if="showAddModal" class="glass w-full max-w-sm p-6 shadow-2xl">
            <h3 class="text-base font-semibold text-white mb-4">新增米種</h3>
            <div class="space-y-3">
              <div>
                <label class="form-label" for="new-rice-name">米種名稱</label>
                <input
                  id="new-rice-name"
                  ref="newRiceNameRef"
                  v-model="newRiceName"
                  type="text"
                  class="input-base"
                  placeholder="例：糯米 (乾)"
                  @keydown.enter="addRiceType"
                  @keydown.esc="showAddModal = false"
                />
              </div>
              <div class="flex gap-3 pt-1">
                <button class="btn-secondary flex-1" @click="showAddModal = false">取消</button>
                <button id="confirm-add-rice" class="btn-primary flex-1" :disabled="!newRiceName.trim()" @click="addRiceType">新增</button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>

  <!-- Delete confirm modal -->
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="deleteTarget"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        @click.self="deleteTarget = null"
      >
        <div class="glass w-full max-w-xs p-6 shadow-2xl">
          <h3 class="text-base font-semibold text-white mb-2">刪除米種</h3>
          <p class="text-sm text-gray-400 mb-4">
            確定要刪除「<span class="text-white font-medium">{{ deleteTarget?.name }}</span>」？<br/>
            <span class="text-xs text-gray-600">若已有關聯傳票，建議改為「停用」</span>
          </p>
          <div class="flex gap-3">
            <button class="btn-secondary flex-1" @click="deleteTarget = null">取消</button>
            <button id="confirm-delete-rice" class="btn-danger flex-1" @click="doDelete">確認刪除</button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { useRiceTypeStore } from '@/stores/riceTypes'
import ToggleSwitch from '@/components/shared/ToggleSwitch.vue'
import api from '@/api'

const rtStore = useRiceTypeStore()

const today      = new Date().toISOString().slice(0, 10)
const activeTab  = ref('rice-types')
const tabs = [
  { id: 'rice-types', label: '米種管理' },
  { id: 'prices',     label: '牌價設定' },
]

// ─── Rice type management ─────────────────────────────────────
const editing     = reactive({ id: null, name: '' })
const showAddModal = ref(false)
const newRiceName  = ref('')
const newRiceNameRef = ref(null)
const deleteTarget = ref(null)
const rtError      = ref('')

function openAddModal() {
  newRiceName.value = ''
  showAddModal.value = true
  nextTick(() => newRiceNameRef.value?.focus())
}

async function addRiceType() {
  if (!newRiceName.value.trim()) return
  try {
    await rtStore.addRiceType({ name: newRiceName.value.trim(), isActive: true })
    showAddModal.value = false
    newRiceName.value = ''
  } catch (e) {
    rtError.value = e.response?.data?.message || '新增失敗'
  }
}

function startEdit(rt) {
  editing.id   = rt.id
  editing.name = rt.name
  nextTick(() => document.getElementById(`edit-rice-${rt.id}`)?.focus())
}

function cancelEdit() {
  editing.id = null
  editing.name = ''
}

async function saveEdit(rt) {
  if (!editing.name.trim() || editing.id !== rt.id) { cancelEdit(); return }
  try {
    await rtStore.updateRiceType(rt.id, { name: editing.name.trim(), isActive: rt.isActive })
    cancelEdit()
  } catch (e) {
    rtError.value = e.response?.data?.message || '更新失敗'
    cancelEdit()
  }
}

async function toggleActive(rt, val) {
  try {
    await rtStore.updateRiceType(rt.id, { name: rt.name, isActive: val })
  } catch (e) {
    rtError.value = e.response?.data?.message || '更新失敗'
  }
}

function confirmDelete(rt) {
  deleteTarget.value = rt
}

async function doDelete() {
  if (!deleteTarget.value) return
  try {
    await rtStore.deleteRiceType(deleteTarget.value.id)
    deleteTarget.value = null
  } catch (e) {
    rtError.value = e.response?.data?.message || '刪除失敗'
    deleteTarget.value = null
  }
}

// ─── Price management ─────────────────────────────────────────
const priceDate    = ref(today)
const priceInputs  = reactive({})
const savingPrice  = reactive({})
const savedIndicator = reactive({})
const priceLogs    = ref([])

async function loadPriceLogs() {
  try {
    const { data } = await api.get('/price-logs', { params: { dateFrom: priceDate.value, dateTo: priceDate.value } })
    priceLogs.value = data

    // Pre-fill inputs from loaded logs
    data.forEach(log => {
      priceInputs[log.riceTypeId] = log.unitPrice
    })
  } catch (e) {
    console.error('Failed to load price logs:', e)
  }
}

async function savePrice(riceTypeId) {
  const val = parseFloat(priceInputs[riceTypeId])
  if (isNaN(val) || val <= 0) return

  savingPrice[riceTypeId] = true
  try {
    await rtStore.upsertPrice(riceTypeId, priceDate.value, val)
    await loadPriceLogs()

    // Show tick indicator for 2 seconds
    savedIndicator[riceTypeId] = true
    setTimeout(() => { savedIndicator[riceTypeId] = false }, 2000)
  } catch (e) {
    console.error('Failed to save price:', e)
  } finally {
    savingPrice[riceTypeId] = false
  }
}

onMounted(async () => {
  await rtStore.fetchRiceTypes()
  await loadPriceLogs()
})
</script>
