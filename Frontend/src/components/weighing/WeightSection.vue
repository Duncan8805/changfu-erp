<template>
  <div class="space-y-2">
    <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">重量資訊</h3>

    <div class="flex gap-3">
      <!-- ① 左：輸入欄位 -->
      <div class="flex flex-col gap-2 w-[48%]">
        <div>
          <label class="form-label">總重 (kg)</label>
          <NumericInput
            :model-value="grossKg"
            label="總重 (kg)"
            placeholder="0"
            :disabled="isSettled"
            :input-class="weightError ? 'input-error' : ''"
            @update:model-value="grossKg = $event"
            @change="emitChange"
          />
        </div>

        <div>
          <label class="form-label">空重 (kg)</label>
          <NumericInput
            :model-value="tareKg"
            label="空重 (kg)"
            placeholder="0"
            :disabled="isSettled"
            :input-class="weightError ? 'input-error' : ''"
            @update:model-value="tareKg = $event"
            @change="emitChange"
          />
        </div>

        <!-- 錯誤提示 -->
        <Transition name="slide-up">
          <p v-if="weightError" class="text-red-400 text-xs flex items-center gap-1">
            <svg class="w-3.5 h-3.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
            </svg>
            空重不能大於總重
          </p>
        </Transition>
      </div>

      <!-- ② 右：計算結果顯示 -->
      <div class="flex flex-col gap-2 flex-1">
        <div class="glass p-3 flex-1 flex flex-col justify-center">
          <p class="text-[10px] text-gray-500 mb-1">淨重 (kg)</p>
          <p class="text-xl font-mono font-bold" :class="netKg > 0 ? 'text-white' : 'text-gray-600'">
            {{ netKg > 0 ? netKg.toLocaleString() : '—' }}
          </p>
        </div>

        <div class="glass p-3 flex-1 flex flex-col justify-center">
          <p class="text-[10px] text-gray-500 mb-1">台斤</p>
          <p class="text-xl font-mono font-bold" :class="netJin > 0 ? 'text-brand-300' : 'text-gray-600'">
            {{ netJin > 0 ? netJin.toLocaleString() : '—' }}
          </p>
        </div>

        <div class="glass p-3 flex-1 flex flex-col justify-center">
          <p class="text-[10px] text-gray-500 mb-1">換算比</p>
          <p class="text-sm font-mono text-gray-400">÷ 0.6</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useWeightCalc } from '@/composables/useWeightCalc'
import NumericInput from '@/components/shared/NumericInput.vue'

const props = defineProps({
  grossWeightKg: { type: Number, default: 0 },
  tareWeightKg:  { type: Number, default: 0 },
  unitPrice:     { type: Number, default: 0 },
  isSettled:     { type: Boolean, default: false },
})

const emit = defineEmits(['update:grossWeightKg', 'update:tareWeightKg'])

// 內部 number state（與外部 prop 雙向同步）
const grossKg = ref(props.grossWeightKg || 0)
const tareKg  = ref(props.tareWeightKg  || 0)

watch(() => props.grossWeightKg, v => { grossKg.value = v || 0 })
watch(() => props.tareWeightKg,  v => { tareKg.value  = v || 0 })

const unitPriceRef = computed(() => props.unitPrice)
const { netKg, netJin, weightError } = useWeightCalc(grossKg, tareKg, unitPriceRef)

function emitChange() {
  emit('update:grossWeightKg', grossKg.value)
  emit('update:tareWeightKg',  tareKg.value)
}
</script>
