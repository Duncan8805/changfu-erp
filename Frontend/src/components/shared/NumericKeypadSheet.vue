<template>
  <Teleport to="body">
    <Transition name="kp-overlay">
      <div
        v-if="kp.isOpen.value"
        class="fixed inset-0 z-[300] flex flex-col justify-end"
      >
        <!-- 半透明遮罩 -->
        <div
          class="absolute inset-0 bg-black/60 backdrop-blur-sm"
          @click="kp.handleCancel()"
        />

        <!-- 鍵盤面板 -->
        <Transition name="kp-sheet">
          <div
            v-if="kp.isOpen.value"
            class="relative bg-gray-950 border-t border-white/10 rounded-t-3xl shadow-2xl pb-safe"
          >
            <!-- Handle bar -->
            <div class="flex justify-center pt-3 pb-1">
              <div class="w-10 h-1 rounded-full bg-white/25" />
            </div>

            <!-- Header：標籤 + 目前數值 -->
            <div class="flex items-center justify-between px-6 py-3">
              <span class="text-sm font-medium text-gray-400">{{ kp.currentLabel.value }}</span>
              <div class="text-right">
                <div
                  class="font-mono font-bold text-white transition-all"
                  :class="kp.displayValue.value ? 'text-4xl' : 'text-2xl text-gray-600'"
                >
                  {{ kp.displayValue.value || '0' }}
                </div>
                <div v-if="kp.subDisplay.value" class="text-sm text-brand-300 font-mono mt-0.5">
                  {{ kp.subDisplay.value }}
                </div>
              </div>
            </div>

            <!-- 分隔線 -->
            <div class="h-px bg-white/8 mx-4 mb-3" />

            <!-- 數字按鍵 3×4 格 -->
            <div class="grid grid-cols-3 gap-2.5 px-4 pb-5">
              <button
                v-for="key in keyLayout"
                :key="key.label"
                class="h-16 rounded-2xl font-semibold select-none
                       transition-all duration-75 active:scale-95 active:brightness-125"
                :class="key.cls"
                @click="kp.pressKey(key.value)"
              >
                <span class="text-2xl">{{ key.label }}</span>
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { useNumericKeypad } from '@/composables/useNumericKeypad'

const kp = useNumericKeypad()

const base    = 'bg-white/8 text-white hover:bg-white/12'
const delCls  = 'bg-red-950/60 text-red-400 hover:bg-red-900/50'
const okCls   = 'bg-green-700/40 text-green-300 hover:bg-green-700/60'

const keyLayout = [
  { label: '7', value: '7', cls: base },
  { label: '8', value: '8', cls: base },
  { label: '9', value: '9', cls: base },
  { label: '4', value: '4', cls: base },
  { label: '5', value: '5', cls: base },
  { label: '6', value: '6', cls: base },
  { label: '1', value: '1', cls: base },
  { label: '2', value: '2', cls: base },
  { label: '3', value: '3', cls: base },
  { label: '⌫',  value: 'del',     cls: delCls },
  { label: '0',  value: '0',       cls: base   },
  { label: '✓',  value: 'confirm', cls: okCls  },
]
</script>

<style>
/* 遮罩 fade */
.kp-overlay-enter-active,
.kp-overlay-leave-active { transition: opacity 0.2s ease; }
.kp-overlay-enter-from,
.kp-overlay-leave-to    { opacity: 0; }

/* 面板 slide-up */
.kp-sheet-enter-active  { transition: transform 0.25s cubic-bezier(0.32, 0.72, 0, 1); }
.kp-sheet-leave-active  { transition: transform 0.2s ease-in; }
.kp-sheet-enter-from,
.kp-sheet-leave-to      { transform: translateY(100%); }
</style>
