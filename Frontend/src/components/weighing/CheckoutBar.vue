<template>
  <div class="bg-gray-900/90 backdrop-blur-sm border-t border-white/10 px-5 py-4">
    <!-- Summary row -->
    <div class="flex items-end justify-between mb-3">
      <div>
        <p class="text-xs text-gray-500 mb-0.5">應付總金額</p>
        <p
          class="text-3xl font-bold font-mono transition-all duration-200"
          :class="totalAmount > 0 ? 'text-green-400' : 'text-gray-600'"
        >
          {{ totalAmount > 0 ? `$ ${totalAmount.toLocaleString()}` : '$ —' }}
        </p>
        <p v-if="netJin > 0 && unitPrice > 0" class="text-xs text-gray-500 mt-0.5">
          {{ netJin.toLocaleString() }} 台斤 × ${{ unitPrice }}
        </p>
      </div>

      <!-- Action buttons -->
      <div class="flex gap-2">
        <button
          id="save-draft-btn"
          class="btn-secondary"
          :disabled="loading || isSettled"
          @click="$emit('save-draft')"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4"/>
          </svg>
          暫存
        </button>

        <button
          id="settle-btn"
          class="btn-primary btn-lg px-6 gap-2"
          :disabled="loading || isSettled"
          :class="{ 'opacity-50 cursor-not-allowed': isSettled }"
          @click="$emit('settle-click')"
        >
          <svg v-if="loading" class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
          </svg>
          <svg v-else class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          {{ isSettled ? '已結算' : '結算完成' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  totalAmount: { type: Number, default: 0 },
  netJin:      { type: Number, default: 0 },
  unitPrice:   { type: Number, default: 0 },
  loading:     { type: Boolean, default: false },
  isSettled:   { type: Boolean, default: false },
})
defineEmits(['save-draft', 'settle-click'])
</script>
