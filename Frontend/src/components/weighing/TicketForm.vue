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
            <!-- Status advance button -->
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

        <!-- Rice type section -->
        <div class="glass p-4">
          <RiceTypeSection
            ref="riceRef"
            :rice-type-id="form.riceTypeId"
            :price-snapshot="ticket.priceSnapshot"
            :is-exception-prop="form.isException"
            :exception-reason-prop="form.exceptionReason"
            :is-settled="isSettled"
            @update:riceTypeId="form.riceTypeId = $event"
            @update:unitPrice="currentUnitPrice = $event"
            @update:isException="form.isException = $event"
            @update:exceptionReason="form.exceptionReason = $event"
          />
        </div>

        <!-- Note -->
        <div class="glass p-4">
          <label class="form-label" for="field-note">備註</label>
          <textarea
            id="field-note"
            v-model="form.note"
            class="input-base resize-none"
            rows="2"
            placeholder="備註事項（選填）"
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
        :is-exception="form.isException"
        :exception-reason="form.exceptionReason"
        :loading="settleLoading"
        @confirm="doSettle"
        @cancel="showConfirmSheet = false"
      />
    </div>
  </template>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'
import { useRiceTypeStore } from '@/stores/riceTypes'
import { useWeightCalc } from '@/composables/useWeightCalc'
import StatusBadge from '@/components/shared/StatusBadge.vue'
import WeightSection from './WeightSection.vue'
import RiceTypeSection from './RiceTypeSection.vue'
import CheckoutBar from './CheckoutBar.vue'
import SettleConfirmSheet from './SettleConfirmSheet.vue'

const store         = useVehicleStore()
const riceTypeStore = useRiceTypeStore()

const ticket    = computed(() => store.activeVehicle)
const isSettled = computed(() => ticket.value?.status === 'settled')

// ─── Form state ───────────────────────────────────────────────
const form = ref({
  vehicleNo: '', farmerName: '', village: '',
  grossWeightKg: 0, tareWeightKg: 0,
  riceTypeId: null, isException: false, exceptionReason: '', note: '',
})

const currentUnitPrice  = ref(null)
const showConfirmSheet  = ref(false)
const settleLoading     = ref(false)

// Template refs
const weightRef = ref(null)
const riceRef   = ref(null)

// ─── Sync form when active ticket changes ─────────────────────
watch(ticket, (t) => {
  if (!t) return
  form.value = {
    vehicleNo:      t.vehicleNo     ?? '',
    farmerName:     t.farmerName    ?? '',
    village:        t.village       ?? '',
    grossWeightKg:  t.grossWeightKg ?? 0,
    tareWeightKg:   t.tareWeightKg  ?? 0,
    riceTypeId:     t.riceTypeId    ?? null,
    isException:    t.isException   ?? false,
    exceptionReason: t.exceptionReason ?? '',
    note:           t.note          ?? '',
  }

  // 已結算：用快照單價；未結算：從 store 查今日牌價
  if (t.status === 'settled' && t.priceSnapshot) {
    currentUnitPrice.value = t.priceSnapshot
  } else if (t.riceTypeId) {
    const rt = riceTypeStore.riceTypes.find(r => r.id === t.riceTypeId)
    currentUnitPrice.value = rt?.todayPrice ?? null
  } else {
    currentUnitPrice.value = null
  }

  showConfirmSheet.value = false
}, { immediate: true })

// 若 riceTypes 比 ticket 晚載入（非同步），補設單價
watch(() => riceTypeStore.riceTypes, (types) => {
  if (!types.length) return
  const t = ticket.value
  if (!t || t.status === 'settled' || currentUnitPrice.value != null) return
  if (t.riceTypeId) {
    const rt = types.find(r => r.id === t.riceTypeId)
    if (rt?.todayPrice != null) currentUnitPrice.value = rt.todayPrice
  }
}, { deep: false })

// ─── Preview calculations (for CheckoutBar) ───────────────────
// 注意：useWeightCalc 吃的是 ref-like 物件（.value = 數字），
// 不能包成 computed(() => ({ value: X }))，那樣 .value 會是物件而非數字
const grossRef = computed(() => form.value.grossWeightKg)
const tareRef  = computed(() => form.value.tareWeightKg)
const priceRef = computed(() => currentUnitPrice.value ?? 0)

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
      vehicleNo:       form.value.vehicleNo,
      farmerName:      form.value.farmerName,
      village:         form.value.village,
      riceTypeId:      form.value.riceTypeId,
      grossWeightKg:   form.value.grossWeightKg,
      tareWeightKg:    form.value.tareWeightKg,
      isException:     form.value.isException,
      exceptionReason: form.value.exceptionReason,
      note:            form.value.note,
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

  // 1. Weight error check
  if (weightError.value) {
    alert('空重不能大於總重，請確認後再結算')
    return
  }

  // 2. Exception reason check
  if (form.value.isException && !riceRef.value?.validateReason()) {
    return
  }

  // 3. Show confirm sheet
  showConfirmSheet.value = true
}

// ─── Do settle ────────────────────────────────────────────────
async function doSettle() {
  if (!ticket.value) return
  settleLoading.value = true
  try {
    // Save latest form data first
    await store.updateTicket(ticket.value.id, {
      vehicleNo:       form.value.vehicleNo,
      farmerName:      form.value.farmerName,
      village:         form.value.village,
      grossWeightKg:   form.value.grossWeightKg,
      tareWeightKg:    form.value.tareWeightKg,
      riceTypeId:      form.value.riceTypeId,
      note:            form.value.note,
    })

    await store.settleTicket(ticket.value.id, {
      riceTypeId:      form.value.riceTypeId,
      isException:     form.value.isException,
      exceptionReason: form.value.exceptionReason,
      note:            form.value.note,
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
