<template>
  <div class="space-y-4">
    <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider">米種 / 單價</h3>

    <!-- Exception toggle -->
    <div class="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/10">
      <div>
        <p class="text-sm font-medium" :class="isException ? 'text-red-300' : 'text-gray-200'">
          例外單價
        </p>
        <p class="text-xs text-gray-500">開啟後米種按鈕停用，需手動輸入單價</p>
      </div>
      <ToggleSwitch
        id="exception-toggle"
        v-model="isException"
        :active-class="'bg-red-600'"
        label="例外單價"
        :disabled="isSettled"
        @update:model-value="onExceptionChange"
      />
    </div>

    <!-- Rice type quick select (hidden when exception) -->
    <Transition name="fade">
      <div v-if="!isException">
        <p class="form-label mb-2">米種快選</p>
        <div class="grid grid-cols-2 gap-2">
          <button
            v-for="rt in activeRiceTypes"
            :key="rt.id"
            type="button"
            class="flex flex-col items-start p-3 rounded-xl border transition-all duration-150 text-left"
            :class="[
              selectedRiceTypeId === rt.id
                ? 'bg-brand-600/25 border-brand-500/60 ring-1 ring-brand-500/40'
                : 'bg-white/5 border-white/10 hover:bg-white/10 hover:border-white/20',
              isSettled ? 'opacity-60 pointer-events-none' : ''
            ]"
            :disabled="isSettled"
            @click="selectRiceType(rt)"
          >
            <span class="text-sm font-medium" :class="selectedRiceTypeId === rt.id ? 'text-brand-300' : 'text-gray-200'">
              {{ rt.name }}
            </span>
            <span class="text-xs mt-0.5" :class="selectedRiceTypeId === rt.id ? 'text-brand-400' : 'text-gray-500'">
              {{ rt.todayPrice != null ? `$ ${rt.todayPrice} / 台斤` : '未設定牌價' }}
            </span>
          </button>
        </div>
        <p v-if="activeRiceTypes.length === 0" class="text-xs text-gray-600 text-center py-3">
          尚無啟用的米種，請至系統設定新增
        </p>
      </div>
    </Transition>

    <!-- Exception fields -->
    <Transition name="fade">
      <div v-if="isException" class="space-y-3 p-3 rounded-xl bg-red-500/5 border border-red-500/20">
        <!-- Exception reason -->
        <div ref="reasonRef">
          <label class="form-label text-red-400" for="exception-reason">
            例外原因 <span class="text-red-500">*</span>
          </label>
          <div class="grid grid-cols-3 gap-2">
            <button
              v-for="r in exceptionReasons"
              :key="r"
              type="button"
              class="px-2 py-1.5 rounded-lg text-xs border transition-all duration-150 text-center"
              :class="[
                exceptionReason === r
                  ? 'bg-red-600/30 border-red-500/60 text-red-200'
                  : 'bg-white/5 border-white/10 text-gray-400 hover:border-red-500/30 hover:text-red-300',
                reasonError ? 'border-red-500/50' : ''
              ]"
              @click="exceptionReason = r; reasonError = false"
            >
              {{ r }}
            </button>
          </div>
          <Transition name="slide-up">
            <p v-if="reasonError" class="text-red-400 text-xs mt-1.5 flex items-center gap-1">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
              </svg>
              請選擇例外原因
            </p>
          </Transition>
        </div>

        <!-- Custom unit price input when exception -->
        <div>
          <label class="form-label text-red-400" for="exception-price">自訂單價 (元/台斤)</label>
          <input
            id="exception-price"
            v-model="exceptionPrice"
            type="number"
            inputmode="decimal"
            step="0.1"
            min="0"
            placeholder="輸入單價"
            class="input-base input-error font-mono"
            :disabled="isSettled"
            @focus="$event.target.select()"
          />
        </div>
      </div>
    </Transition>

    <!-- Current price display -->
    <div v-if="currentPrice != null" class="flex items-center justify-between px-3 py-2 rounded-lg bg-brand-600/10 border border-brand-600/20">
      <span class="text-xs text-gray-400">結算單價</span>
      <span class="text-base font-mono font-semibold text-brand-300">${{ currentPrice }} / 台斤</span>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRiceTypeStore } from '@/stores/riceTypes'
import ToggleSwitch from '@/components/shared/ToggleSwitch.vue'

const props = defineProps({
  riceTypeId:        { type: Number, default: null },
  priceSnapshot:     { type: Number, default: null },
  isExceptionProp:   { type: Boolean, default: false },
  exceptionReasonProp: { type: String, default: '' },
  isSettled:         { type: Boolean, default: false },
})

const emit = defineEmits([
  'update:riceTypeId',
  'update:unitPrice',
  'update:isException',
  'update:exceptionReason',
])

const riceTypeStore = useRiceTypeStore()

const selectedRiceTypeId = ref(props.riceTypeId)
const isException        = ref(props.isExceptionProp)
const exceptionReason    = ref(props.exceptionReasonProp)
const exceptionPrice     = ref('')
const reasonError        = ref(false)
const reasonRef          = ref(null)

const exceptionReasons = ['太青', '含水高', '雜質', '蟲害', '摻沙', '其他']

const activeRiceTypes = computed(() =>
  riceTypeStore.riceTypes.filter(r => r.isActive)
)

const currentPrice = computed(() => {
  if (props.isSettled && props.priceSnapshot) return props.priceSnapshot
  if (isException.value) return parseFloat(exceptionPrice.value) || null
  const rt = activeRiceTypes.value.find(r => r.id === selectedRiceTypeId.value)
  return rt?.todayPrice ?? null
})

// Sync from parent when active ticket changes
watch(() => props.riceTypeId, v => { selectedRiceTypeId.value = v })
watch(() => props.isExceptionProp, v => { isException.value = v })
watch(() => props.exceptionReasonProp, v => { exceptionReason.value = v })

// Emit price whenever it changes
watch(currentPrice, v => emit('update:unitPrice', v))

function selectRiceType(rt) {
  selectedRiceTypeId.value = rt.id
  emit('update:riceTypeId', rt.id)
  emit('update:unitPrice', rt.todayPrice)
}

function onExceptionChange(val) {
  isException.value = val
  if (!val) {
    exceptionReason.value = ''
    exceptionPrice.value  = ''
    reasonError.value     = false
  }
  emit('update:isException', val)
}

watch(exceptionReason, v => emit('update:exceptionReason', v))

// Expose for parent validation
function validateReason() {
  if (isException.value && !exceptionReason.value) {
    reasonError.value = true
    reasonRef.value?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    return false
  }
  return true
}

defineExpose({ validateReason, isException, exceptionReason, exceptionPrice, selectedRiceTypeId, currentPrice })
</script>
