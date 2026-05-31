<template>
  <Transition name="slide-up">
    <div
      v-if="show"
      class="absolute inset-x-0 bottom-0 z-20 bg-gray-900/95 backdrop-blur-md border-t border-white/10
             shadow-2xl shadow-black/60 rounded-t-2xl"
    >
      <!-- Handle bar -->
      <div class="flex justify-center pt-3 pb-1">
        <div class="w-10 h-1 rounded-full bg-white/20"></div>
      </div>

      <div class="px-6 pb-6 pt-2">
        <!-- Title -->
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-base font-semibold text-white flex items-center gap-2">
            <svg class="w-5 h-5 text-brand-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            確認結算
          </h3>
          <button class="btn-ghost p-1.5" @click="$emit('cancel')">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Summary table -->
        <div class="space-y-3 mb-5">
          <div class="grid grid-cols-2 gap-3">
            <SummaryRow label="車號" :value="ticket.vehicleNo || '—'" mono />
            <SummaryRow label="農民姓名" :value="ticket.farmerName || '—'" />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <SummaryRow label="米種" :value="riceTypeName" />
            <SummaryRow label="村別" :value="ticket.village || '—'" />
          </div>

          <div class="border-t border-white/10 pt-3 grid grid-cols-3 gap-3">
            <SummaryRow label="淨重 (kg)" :value="fmtNum(ticket.netWeightKg)" mono />
            <SummaryRow label="台斤" :value="fmtNum(ticket.netWeightJin)" mono highlight />
            <SummaryRow label="單價 (元/台斤)" :value="`$ ${unitPrice}`" mono />
          </div>

          <!-- Total -->
          <div class="flex items-center justify-between px-4 py-3 rounded-xl bg-green-500/10 border border-green-500/20">
            <span class="text-sm text-gray-300">應付總金額</span>
            <span class="text-2xl font-bold font-mono text-green-400">
              $ {{ fmtNum(totalAmount) }}
            </span>
          </div>

          <!-- Exception note -->
          <div v-if="isException" class="px-3 py-2 rounded-lg bg-red-500/10 border border-red-500/20 text-xs text-red-300 flex items-center gap-2">
            <svg class="w-4 h-4 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
            </svg>
            例外原因：{{ exceptionReason }}
          </div>
        </div>

        <!-- Buttons -->
        <div class="flex gap-3">
          <button class="btn-secondary flex-1" :disabled="loading" @click="$emit('cancel')">取消</button>
          <button
            id="confirm-settle-btn"
            class="btn-primary flex-1 btn-lg"
            :disabled="loading"
            @click="$emit('confirm')"
          >
            <svg v-if="loading" class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
            </svg>
            <svg v-else class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
            </svg>
            確認結算
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { computed, h } from 'vue'
import { useRiceTypeStore } from '@/stores/riceTypes'

// Internal SummaryRow micro-component
const SummaryRow = {
  props: { label: String, value: String, mono: Boolean, highlight: Boolean },
  setup(p) {
    return () => h('div', { class: 'glass p-2.5' }, [
      h('p', { class: 'text-[10px] text-gray-500 mb-0.5' }, p.label),
      h('p', {
        class: [
          'text-sm font-medium',
          p.mono      ? 'font-mono' : '',
          p.highlight ? 'text-brand-300' : 'text-gray-100',
        ].filter(Boolean).join(' ')
      }, p.value),
    ])
  }
}

const props = defineProps({
  show:            { type: Boolean, required: true },
  ticket:          { type: Object,  required: true },
  unitPrice:       { type: Number,  default: 0 },
  totalAmount:     { type: Number,  default: 0 },
  isException:     { type: Boolean, default: false },
  exceptionReason: { type: String,  default: '' },
  loading:         { type: Boolean, default: false },
})
defineEmits(['confirm', 'cancel'])

const riceTypeStore = useRiceTypeStore()

const riceTypeName = computed(() => {
  const rt = riceTypeStore.riceTypes.find(r => r.id === props.ticket.riceTypeId)
  return rt?.name ?? '—'
})

function fmtNum(n) {
  const v = parseFloat(n)
  return isNaN(v) ? '—' : v.toLocaleString()
}
</script>
