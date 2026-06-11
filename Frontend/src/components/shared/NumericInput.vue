<template>
  <!-- Touch 裝置：顯示仿 input div，點擊開啟自訂鍵盤 -->
  <div
    v-if="isTouchDevice"
    role="button"
    tabindex="0"
    class="input-base flex items-center min-h-[44px] cursor-pointer select-none"
    :class="[inputClass, disabled ? 'opacity-50 pointer-events-none' : '', focused ? 'ring-2 ring-brand-500/60' : '']"
    @click="!disabled && openKeypad()"
  >
    <span v-if="modelValue" class="font-mono text-xl font-bold text-white">
      {{ modelValue }}
    </span>
    <span v-else class="text-gray-500">{{ placeholder }}</span>
  </div>

  <!-- 桌機：正常 input，可直接打字 -->
  <input
    v-else
    :value="modelValue || ''"
    type="tel"
    inputmode="numeric"
    pattern="[0-9]*"
    :placeholder="placeholder"
    :disabled="disabled"
    class="input-base text-xl font-mono font-bold"
    :class="inputClass"
    @focus="$event.target.select()"
    @input="$emit('update:modelValue', parseInt($event.target.value.replace(/\D/g, '')) || 0)"
    @change="$emit('change')"
  />
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useNumericKeypad } from '@/composables/useNumericKeypad'

const props = defineProps({
  modelValue:   { type: Number, default: 0 },
  label:        { type: String, default: '' },
  placeholder:  { type: String, default: '0' },
  disabled:     { type: Boolean, default: false },
  maxDigits:    { type: Number, default: 7 },
  inputClass:   { type: String, default: '' },
  /** (num: number, buf: string) => string — 鍵盤副標題（例如換算價格顯示） */
  subFormatter: { type: Function, default: null },
})

const emit = defineEmits(['update:modelValue', 'change'])

// ─── Touch 偵測 ────────────────────────────────────────────────
const isTouchDevice = ref(false)
onMounted(() => {
  isTouchDevice.value = window.matchMedia('(pointer: coarse)').matches
})

// ─── 鍵盤互動 ─────────────────────────────────────────────────
const kp = useNumericKeypad()
const focused = ref(false)

function openKeypad() {
  focused.value = true
  kp.open({
    label:        props.label,
    value:        props.modelValue,
    max:          props.maxDigits,
    subFormatter: props.subFormatter,
    onConfirm: (v) => {
      emit('update:modelValue', v)
      emit('change')
      focused.value = false
    },
  })
}
</script>
