<template>
  <span :class="badgeClass">
    <span class="w-1.5 h-1.5 rounded-full" :class="dotClass"></span>
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, required: true }, // 'unloading' | 'pending' | 'settled'
})

const config = {
  unloading: { label: '卸貨中', badge: 'badge-unloading', dot: 'bg-red-400' },
  pending:   { label: '待結算', badge: 'badge-pending',   dot: 'bg-amber-400' },
  settled:   { label: '已結算', badge: 'badge-settled',   dot: 'bg-green-400' },
}

const badgeClass = computed(() => config[props.status]?.badge ?? 'badge bg-gray-500/20 text-gray-400')
const dotClass   = computed(() => config[props.status]?.dot   ?? 'bg-gray-400')
const label      = computed(() => config[props.status]?.label ?? props.status)
</script>
