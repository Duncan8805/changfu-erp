<template>
  <button
    class="flex items-start gap-3 w-full text-left px-3 py-3 rounded-xl border-l-4 transition-all duration-150
           hover:bg-white/5 active:scale-[0.98] relative group"
    :class="[
      borderClass,
      isActive
        ? 'bg-white/8 ring-1 ring-white/15'
        : 'bg-transparent'
    ]"
    @click="$emit('select', ticket.id)"
  >
    <!-- Status border color indicator -->
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
</template>

<script setup>
import { computed } from 'vue'
import StatusBadge from '@/components/shared/StatusBadge.vue'

const props = defineProps({
  ticket:   { type: Object,  required: true },
  isActive: { type: Boolean, default: false },
})
defineEmits(['select'])

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
</script>
