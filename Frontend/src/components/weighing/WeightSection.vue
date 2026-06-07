<template>
  <div class="space-y-4">
    <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">重量資訊</h3>

    <!-- Gross / Tare inputs -->
    <div class="grid grid-cols-2 gap-3">
      <div>
        <label class="form-label" for="gross-weight">總重 (kg)</label>
        <input
          id="gross-weight"
          v-model="grossKgModel"
          type="number"
          inputmode="numeric"
          placeholder="0"
          min="0"
          step="0.1"
          class="input-base text-lg font-mono"
          :class="{ 'input-error': weightError }"
          :disabled="isSettled"
          @focus="$event.target.select()"
          @change="emitChange"
        />
      </div>

      <div>
        <label class="form-label" for="tare-weight">空重 (kg)</label>
        <input
          id="tare-weight"
          v-model="tareKgModel"
          type="number"
          inputmode="numeric"
          placeholder="0"
          min="0"
          step="0.1"
          class="input-base text-lg font-mono"
          :class="{ 'input-error': weightError }"
          :disabled="isSettled"
          @focus="$event.target.select()"
          @change="emitChange"
        />
      </div>
    </div>

    <!-- Weight error -->
    <Transition name="slide-up">
      <p v-if="weightError" class="text-red-400 text-xs flex items-center gap-1">
        <svg class="w-3.5 h-3.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
        </svg>
        空重不能大於總重
      </p>
    </Transition>

    <!-- Results -->
    <div class="grid grid-cols-3 gap-3">
      <div class="glass p-3 text-center">
        <p class="text-[10px] text-gray-500 mb-1">淨重 (kg)</p>
        <p class="text-lg font-mono font-bold" :class="netKg > 0 ? 'text-white' : 'text-gray-600'">
          {{ netKg > 0 ? netKg.toLocaleString() : '—' }}
        </p>
      </div>
      <div class="glass p-3 text-center">
        <p class="text-[10px] text-gray-500 mb-1">台斤</p>
        <p class="text-lg font-mono font-bold" :class="netJin > 0 ? 'text-brand-300' : 'text-gray-600'">
          {{ netJin > 0 ? netJin.toLocaleString() : '—' }}
        </p>
      </div>
      <div class="glass p-3 text-center">
        <p class="text-[10px] text-gray-500 mb-1">換算比</p>
        <p class="text-sm font-mono text-gray-400">÷ 0.6</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useWeightCalc } from '@/composables/useWeightCalc'

const props = defineProps({
  grossWeightKg: { type: Number, default: 0 },
  tareWeightKg:  { type: Number, default: 0 },
  unitPrice:     { type: Number, default: 0 },
  isSettled:     { type: Boolean, default: false },
})

const emit = defineEmits(['update:grossWeightKg', 'update:tareWeightKg'])

const grossKgModel = ref(String(props.grossWeightKg || ''))
const tareKgModel  = ref(String(props.tareWeightKg  || ''))

// Sync when parent (active ticket) changes
watch(() => props.grossWeightKg, v => { grossKgModel.value = v ? String(v) : '' })
watch(() => props.tareWeightKg,  v => { tareKgModel.value  = v ? String(v) : '' })

const { netKg, netJin, weightError } = useWeightCalc(grossKgModel, tareKgModel, ref(props.unitPrice))

function emitChange() {
  emit('update:grossWeightKg', parseFloat(grossKgModel.value) || 0)
  emit('update:tareWeightKg',  parseFloat(tareKgModel.value)  || 0)
}
</script>
