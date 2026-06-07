<template>
  <div class="relative group">
    <button
      class="flex items-start gap-3 w-full text-left px-3 py-3 rounded-xl border-l-4 transition-all duration-150
             hover:bg-white/5 active:scale-[0.98]"
      :class="[
        borderClass,
        isActive
          ? 'bg-white/8 ring-1 ring-white/15'
          : 'bg-transparent'
      ]"
      @click="$emit('select', ticket.id)"
    >
      <!-- Left: info -->
      <div class="flex-1 min-w-0">
        <!-- Row 1: vehicle no + status -->
        <div class="flex items-center justify-between gap-2 mb-1">
          <span class="font-mono font-semibold text-sm text-white truncate">
            {{ ticket.vehicleNo || '—' }}
          </span>
          <StatusBadge :status="ticket.status" />
        </div>

        <!-- Row 2: farmer + village -->
        <p class="text-xs text-gray-400 truncate">
          {{ ticket.farmerName || '—' }}
          <span v-if="ticket.village" class="text-gray-600 mx-1">·</span>
          <span v-if="ticket.village" class="text-gray-500">{{ ticket.village }}</span>
        </p>

        <!-- Row 3: weight info (show if has data) -->
        <div v-if="ticket.grossWeightKg > 0" class="flex items-center gap-3 mt-1.5 text-[11px] text-gray-500">
          <span>總重 <span class="text-gray-300 font-mono">{{ ticket.grossWeightKg.toLocaleString() }}</span> kg</span>
          <span v-if="ticket.netWeightJin > 0">
            淨台斤 <span class="text-gray-300 font-mono">{{ ticket.netWeightJin.toLocaleString() }}</span>
          </span>
        </div>

        <!-- Row 4: ticket no + time -->
        <div class="flex items-center justify-between mt-1 text-[10px] text-gray-600">
          <span class="font-mono">{{ ticket.ticketNo }}</span>
          <span>{{ formatTime(ticket.createdAt) }}</span>
        </div>
      </div>
    </button>

    <!-- 刪單按鈕：hover 時顯示於右下角，已結算不顯示 -->
    <button
      v-if="ticket.status !== 'settled'"
      class="absolute bottom-2 right-2 w-6 h-6 rounded-md flex items-center justify-center
             opacity-0 group-hover:opacity-100 transition-opacity duration-150
             bg-red-500/20 hover:bg-red-500/50 text-red-400 hover:text-red-200"
      title="刪除此傳票"
      @click.stop="handleDelete"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="3 6 5 6 21 6"/>
        <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
        <path d="M10 11v6M14 11v6"/>
        <path d="M9 6V4h6v2"/>
      </svg>
    </button>

    <!-- 刪除確認 Dialog -->
    <div
      v-if="showConfirm"
      class="absolute inset-0 z-10 flex items-center justify-center rounded-xl bg-gray-900/95 backdrop-blur-sm"
      @click.stop
    >
      <div class="text-center px-3">
        <p class="text-xs text-gray-300 mb-3">確定刪除 <span class="text-white font-semibold">{{ ticket.vehicleNo }}</span>？</p>
        <div class="flex gap-2 justify-center">
          <button
            class="px-3 py-1 text-xs rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-200 transition-colors"
            @click="showConfirm = false"
          >取消</button>
          <button
            class="px-3 py-1 text-xs rounded-lg bg-red-600 hover:bg-red-500 text-white transition-colors"
            :disabled="deleting"
            @click="confirmDelete"
          >{{ deleting ? '刪除中…' : '確認刪除' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import StatusBadge from '@/components/shared/StatusBadge.vue'
import { useVehicleStore } from '@/stores/vehicles'

const props = defineProps({
  ticket:   { type: Object,  required: true },
  isActive: { type: Boolean, default: false },
})
defineEmits(['select'])

const store       = useVehicleStore()
const showConfirm = ref(false)
const deleting    = ref(false)

const borderClass = computed(() => ({
  unloading: 'border-l-red-500',
  pending:   'border-l-amber-400',
  settled:   'border-l-green-500',
})[props.ticket.status] ?? 'border-l-gray-600')

function formatTime(isoStr) {
  if (!isoStr) return ''
  try {
    return new Date(isoStr).toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit', hour12: false })
  } catch { return '' }
}

function handleDelete() {
  showConfirm.value = true
}

async function confirmDelete() {
  deleting.value = true
  try {
    await store.deleteVehicle(props.ticket.id)
  } catch (e) {
    alert(e.response?.data?.message || '刪除失敗')
    showConfirm.value = false
  } finally {
    deleting.value = false
  }
}
</script>
