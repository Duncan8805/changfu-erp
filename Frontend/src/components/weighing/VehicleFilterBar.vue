<template>
  <div class="flex gap-1.5 p-2">
    <button
      v-for="f in filters"
      :key="f.value"
      class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all duration-150 flex-1 justify-center"
      :class="[
        currentFilter === f.value
          ? 'bg-brand-600/30 text-brand-300 ring-1 ring-brand-500/40'
          : 'text-gray-400 hover:text-gray-200 hover:bg-white/5'
      ]"
      @click="$emit('update:filter', f.value)"
    >
      {{ f.label }}
      <span
        v-if="f.count > 0"
        class="inline-flex items-center justify-center min-w-[1.1rem] h-4 px-1 rounded-full text-[10px] font-bold"
        :class="f.countClass"
      >{{ f.count }}</span>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'

const props = defineProps({
  currentFilter: { type: String, default: '' },
})
defineEmits(['update:filter'])

const store = useVehicleStore()

const filters = computed(() => [
  {
    value: '',
    label: '全部',
    count: store.vehicles.length,
    countClass: 'bg-gray-600 text-gray-200',
  },
  {
    value: 'unloading',
    label: '卸貨中',
    count: store.unloadingCount,
    countClass: 'bg-red-500/30 text-red-300',
  },
  {
    value: 'pending',
    label: '待結算',
    count: store.pendingCount,
    countClass: 'bg-amber-500/30 text-amber-300',
  },
])
</script>
