<template>
  <!-- No ticket selected -->
  <div v-if="!ticket" class="flex-1 flex flex-col items-center justify-center text-center p-8 text-gray-600">
    <svg class="w-16 h-16 mb-4 opacity-20" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
        d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
    </svg>
    <p class="text-sm font-medium text-gray-500">請從左側選擇車輛</p>
    <p class="text-xs text-gray-700 mt-1">或點擊「新增進場車輛」開始作業</p>
  </div>

  <!-- Ticket form -->
  <template v-else>
    <!-- Scrollable content area -->
    <div class="flex-1 overflow-y-auto pb-[140px]">
      <div class="p-5 space-y-6">
        <!-- Header: ticket no + status -->
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs text-gray-500 font-mono">{{ ticket.ticketNo }}</p>
            <h2 class="text-xl font-bold text-white mt-0.5">
              {{ ticket.vehicleNo || '車號未填' }}
            </h2>
            <p class="text-sm text-gray-400">
              {{ ticket.farmerName || '農民姓名未填' }}
              <span v-if="ticket.village" class="text-gray-600 mx-1">·</span>
              <span v-if="ticket.village" class="text-gray-500">{{ ticket.village }}</span>
            </p>
          </div>
          <div class="flex items-center gap-2">
            <StatusBadge :status="ticket.status" />
            <button
              v-if="ticket.status === 'unloading'"
              class="btn-secondary btn-sm"
              title="標記為待結算"
              @click="advanceStatus"
            >
              卸貨完成
            </button>
          </div>
        </div>

        <!-- Basic info (editable) -->
        <div class="glass p-4 space-y-3">
          <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">基本資訊</h3>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="form-label" for="field-vehicle-no">車號</label>
              <input id="field-vehicle-no" v-model="form.vehicleNo" type="text" class="input-base uppercase"
                placeholder="AB-1234" :disabled="isSettled"
                @input="form.vehicleNo = form.vehicleNo.toUpperCase()" />
            </div>
            <div>
              <label class="form-label" for="field-farmer">農民姓名</label>
              <input id="field-farmer" v-model="form.farmerName" type="text" class="input-base"
                placeholder="姓名" :disabled="isSettled" />
            </div>
          </div>
          <div>
            <label class="form-label" for="field-village">村別 / 行號</label>
            <input id="field-village" v-model="form.village" type="text" class="input-base"
              placeholder="例：松梅里" :disabled="isSettled" />
          </div>
        </div>

        <!-- Weight section -->
        <div class="glass p-4">
          <WeightSection
            ref="weightRef"
            v-model:grossWeightKg="form.grossWeightKg"
            v-model:tareWeightKg="form.tareWeightKg"
            :unit-price="currentUnitPrice"
            :is-settled="isSettled"
          />
        </div>

        <!-- 單價區塊（取代原本米種區） -->
        <div class="glass p-4 space-y-3">
          <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">單價</h3>

          <div v-if="isSettled">
            <!-- 已結算：顯示快照 -->
            <div class="flex items-center gap-2">
              <span class="text-2xl font-mono font-bold text-brand-300">
                $ {{ ticket.priceSnapshot }}
              </span>
              <span class="text-sm text-gray-500">/ 台斤</span>
            </div>
          </div>
          <div v-else>
            <!-- 未結算：手動輸入，4位數自動換算 -->
            <div class="flex items-end gap-3">
              <div class="flex-1">
                <label class="form-label" for="price-raw-input">單價（元/台斤）</label>
                <input
                  id="price-raw-input"
                  :value="priceRaw || ''"
                  type="tel"
                  inputmode="numeric"
                  pattern="[0-9]*"
                  placeholder="1105"
                  class="input-base text-lg font-mono"
                  :disabled="isSettled"
                  @focus="$event.target.select()"
                  @input="priceRaw = parseInt($event.target.value.replace(/\D/g,'')) || 0"
                />
              </div>
              <div class="glass px-4 py-3 text-center flex-shrink-0 min-w-[100px]">
                <p class="text-[10px] text-gray-500 mb-0.5">換算後單價</p>
                <p class="text-xl font-mono font-bold" :class="currentUnitPrice > 0 ? 'text-brand-300' : 'text-gray-600'">
                  $ {{ currentUnitPrice > 0 ? currentUnitPrice.toFixed(2) : '—' }}
                </p>
                <p class="text-[10px] text-gray-500">/ 台斤</p>
              </div>
            </div>
                <p class="text-[10px] text-gray-500 mt-1">輸入4位數自動換算，例：1105 → $11.05</p>
          </div>
        </div>

        <!-- Note -->
        <div class="glass p-4 space-y-2">
          <label class="form-label" for="field-note">備註</label>

          <!-- 快速備註 chips -->
          <div v-if="notePresets.length > 0 && !isSettled" class="flex flex-wrap gap-1.5">
            <button
              v-for="p in notePresets"
              :key="p.id"
              type="button"
              class="px-2.5 py-1 text-xs rounded-full border border-white/15 bg-white/5
                     hover:bg-white/10 hover:border-white/30 text-gray-300 transition-colors"
              @click="appendNote(p.content)"
            >
              {{ p.content }}
            </button>
          </div>

          <textarea
            id="field-note"
            v-model="form.note"
            class="input-base resize-none"
            rows="2"
            placeholder="備註事項（選填，可點選上方快速選項）"
            :disabled="isSettled"
          />
        </div>

        <!-- Settled result (read-only) -->
        <div v-if="isSettled" class="glass p-4 border border-green-500/20 bg-green-500/5">
          <p class="text-xs text-green-400 font-semibold uppercase tracking-wider mb-3">結算明細</p>
          <div class="grid grid-cols-3 gap-3 text-sm">
            <div>
              <p class="text-xs text-gray-500">單價快照</p>
              <p class="font-mono text-white">${{ ticket.priceSnapshot }} / 台斤</p>
            </div>
            <div>
              <p class="text-xs text-gray-500">台斤</p>
              <p class="font-mono text-brand-300">{{ ticket.netWeightJin?.toLocaleString() }}</p>
            </div>
            <div>
              <p class="text-xs text-gray-500">結算金額</p>
              <p class="font-mono font-bold text-green-400">${{ ticket.totalAmount?.toLocaleString() }}</p>
            </div>
          </div>
          <p class="text-xs text-gray-600 mt-2">
            結算時間：{{ fmtSettledAt(ticket.settledAt) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Fixed bottom: CheckoutBar + SettleConfirmSheet -->
    <div class="absolute bottom-0 left-0 right-0">
      <CheckoutBar
        :total-amount="previewTotal"
        :net-jin="previewJin"
        :unit-price="currentUnitPrice ?? 0"
        :loading="settleLoading"
        :is-settled="isSettled"
        @save-draft="saveDraft"
        @settle-click="onSettleClick"
      />

      <SettleConfirmSheet
        :show="showConfirmSheet"
        :ticket="previewTicket"
        :unit-price="currentUnitPrice ?? 0"
        :total-amount="previewTotal"
        :is-exception="false"
        :exception-reason="''"
        :loading="settleLoading"
        @confirm="doSettle"
        @cancel="showConfirmSheet = false"
      />
    </div>
  </template>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'
import { useWeightCalc } from '@/composables/useWeightCalc'
import api from '@/api'
import StatusBadge from '@/components/shared/StatusBadge.vue'
import WeightSection from './WeightSection.vue'
import CheckoutBar from './CheckoutBar.vue'
import SettleConfirmSheet from './SettleConfirmSheet.vue'

const store = useVehicleStore()

const ticket    = computed(() => store.activeVehicle)
const isSettled = computed(() => ticket.value?.status === 'settled')

// ─── Form state ───────────────────────────────────────────────
const form = ref({
  vehicleNo: '', farmerName: '', village: '',
  grossWeightKg: 0, tareWeightKg: 0, note: '',
})

// ─── 單價：4位原始輸入 → 除以100得實際單價 ────────────────────
// e.g., priceRaw = 1105 → currentUnitPrice = 11.05
const priceRaw = ref(0)
const currentUnitPrice = computed(() => {
  const v = Number(priceRaw.value) || 0
  return Math.round(v) / 100
})

const showConfirmSheet = ref(false)
const settleLoading    = ref(false)

const weightRef = ref(null)

// ─── Sync form when active ticket changes ─────────────────────
watch(ticket, (t) => {
  if (!t) return
  form.value = {
    vehicleNo:    t.vehicleNo    ?? '',
    farmerName:   t.farmerName   ?? '',
    village:      t.village      ?? '',
    grossWeightKg: t.grossWeightKg ?? 0,
    tareWeightKg:  t.tareWeightKg  ?? 0,
    note:          t.note          ?? '',
  }

  // 已結算：從快照恢復（priceSnapshot * 100 → raw）
  if (t.status === 'settled' && t.priceSnapshot) {
    priceRaw.value = Math.round(t.priceSnapshot * 100)
  } else {
    priceRaw.value = 0
  }

  showConfirmSheet.value = false
}, { immediate: true })

// ─── Note presets ─────────────────────────────────────────────
const notePresets = ref([])
onMounted(async () => {
  try {
    const { data } = await api.get('/note-presets')
    notePresets.value = data
  } catch { /* 靜默失敗 */ }
})

function appendNote(text) {
  const cur = form.value.note?.trim() || ''
  form.value.note = cur ? `${cur}，${text}` : text
}

const grossRef = computed(() => form.value.grossWeightKg)
const tareRef  = computed(() => form.value.tareWeightKg)
const priceRef = computed(() => currentUnitPrice.value)

const { netJin: previewJin, totalAmount: previewTotal, weightError } = useWeightCalc(grossRef, tareRef, priceRef)

const previewTicket = computed(() => ({
  ...ticket.value,
  ...form.value,
  netWeightKg:  Math.max(0, form.value.grossWeightKg - form.value.tareWeightKg),
  netWeightJin: previewJin.value,
}))

// ─── Save draft ───────────────────────────────────────────────
async function saveDraft() {
  if (!ticket.value) return
  try {
    await store.updateTicket(ticket.value.id, {
      vehicleNo:     form.value.vehicleNo,
      farmerName:    form.value.farmerName,
      village:       form.value.village,
      grossWeightKg: form.value.grossWeightKg,
      tareWeightKg:  form.value.tareWeightKg,
      note:          form.value.note,
    })
  } catch (e) {
    console.error('Save draft failed:', e)
  }
}

// ─── Advance status ───────────────────────────────────────────
async function advanceStatus() {
  if (!ticket.value) return
  await saveDraft()
  await store.updateStatus(ticket.value.id, 'pending')
}

// ─── Settle click (pre-validation) ───────────────────────────
function onSettleClick() {
  if (!ticket.value) return

  if (weightError.value) {
    alert('空重不能大於總重，請確認後再結算')
    return
  }

  if (currentUnitPrice.value <= 0) {
    alert('請輸入單價後再結算')
    return
  }

  showConfirmSheet.value = true
}

// ─── Do settle ────────────────────────────────────────────────
async function doSettle() {
  if (!ticket.value) return
  settleLoading.value = true
  try {
    await store.updateTicket(ticket.value.id, {
      vehicleNo:     form.value.vehicleNo,
      farmerName:    form.value.farmerName,
      village:       form.value.village,
      grossWeightKg: form.value.grossWeightKg,
      tareWeightKg:  form.value.tareWeightKg,
      note:          form.value.note,
    })

    await store.settleTicket(ticket.value.id, {
      priceOverride: currentUnitPrice.value,   // 直接傳換算後的單價（例：11.05）
      isException:   false,
      note:          form.value.note,
    })

    showConfirmSheet.value = false
  } catch (e) {
    alert(e.response?.data?.message || '結算失敗，請稍後再試')
  } finally {
    settleLoading.value = false
  }
}

function fmtSettledAt(iso) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString('zh-TW', {
      year: 'numeric', month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit', hour12: false,
    })
  } catch { return iso }
}
</script>
