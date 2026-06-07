<template>
  <!-- 外層：固定寬度、overflow-hidden，只露出卡片部分 -->
  <div class="relative overflow-hidden rounded-xl">

    <!-- 滑動容器：卡片 + 刪除區並排 -->
    <div
      class="flex"
      :style="{
        transform: `translateX(${slideX}px)`,
        transition: isDragging ? 'none' : 'transform 0.25s cubic-bezier(0.25,0.46,0.45,0.94)',
        width: `calc(100% + ${REVEAL_WIDTH}px)`,
      }"
    >
      <!-- ① 卡片本體 -->
      <div
        class="border-l-4 rounded-xl cursor-pointer transition-colors duration-150 hover:bg-white/5"
        :class="[borderClass, isActive ? 'bg-white/8 ring-1 ring-white/15' : 'bg-gray-900']"
        :style="{ width: `calc(100% - ${REVEAL_WIDTH}px)`, flexShrink: 0 }"
        @click="onCardClick"
        @touchstart.passive="onTouchStart"
        @touchmove.prevent="onTouchMove"
        @touchend="onTouchEnd"
        @mousedown="onMouseDown"
      >
        <div class="flex items-start gap-3 px-3 py-3">
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

            <!-- Row 3: weight info -->
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
        </div>
      </div>

      <!-- ② 刪除區（在卡片右側，初始隱藏在 overflow 外） -->
      <div
        v-if="ticket.status !== 'settled'"
        class="flex flex-col items-center justify-center bg-red-600 hover:bg-red-500
               active:bg-red-700 cursor-pointer transition-colors rounded-r-xl"
        :style="{ width: `${REVEAL_WIDTH}px`, flexShrink: 0 }"
        @click="handleDelete"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-white mb-0.5"
             viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="3 6 5 6 21 6"/>
          <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
          <path d="M10 11v6M14 11v6"/>
          <path d="M9 6V4h6v2"/>
        </svg>
        <span class="text-white text-[11px] font-medium">刪除</span>
      </div>

      <!-- 已結算：刪除區佔位但空白，避免 flex 跑版 -->
      <div
        v-else
        :style="{ width: `${REVEAL_WIDTH}px`, flexShrink: 0 }"
      />
    </div>

    <!-- 刪除確認 overlay -->
    <div
      v-if="showConfirm"
      class="absolute inset-0 z-20 flex items-center justify-center rounded-xl
             bg-gray-900/95 backdrop-blur-sm"
      @click.stop
    >
      <div class="text-center px-3">
        <p class="text-xs text-gray-300 mb-3">
          確定刪除 <span class="text-white font-semibold">{{ ticket.vehicleNo }}</span>？
        </p>
        <div class="flex gap-2 justify-center">
          <button
            class="px-3 py-1 text-xs rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-200 transition-colors"
            @click="cancelDelete"
          >取消</button>
          <button
            class="px-3 py-1 text-xs rounded-lg bg-red-600 hover:bg-red-500 text-white transition-colors"
            :disabled="deleting"
            @click="confirmDelete"
          >{{ deleting ? '刪除中…' : '確認刪除' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import StatusBadge from '@/components/shared/StatusBadge.vue'
import { useVehicleStore } from '@/stores/vehicles'

const props = defineProps({
  ticket:   { type: Object,  required: true },
  isActive: { type: Boolean, default: false },
})
const emit = defineEmits(['select'])

const store       = useVehicleStore()
const showConfirm = ref(false)
const deleting    = ref(false)

// ─── 滑動常數 ────────────────────────────────────────────────────
const SNAP_THRESHOLD = 40   // 超過 40px 才 snap 開
const REVEAL_WIDTH   = 80   // 刪除區寬度

const slideX    = ref(0)
const isDragging = ref(false)

let startX     = 0
let startSlide = 0

// ─── Touch ──────────────────────────────────────────────────────
function onTouchStart(e) {
  if (props.ticket.status === 'settled') return
  startX      = e.touches[0].clientX
  startSlide  = slideX.value
  isDragging.value = true
}
function onTouchMove(e) {
  if (!isDragging.value) return
  applySlide(startSlide + (e.touches[0].clientX - startX))
}
function onTouchEnd() {
  isDragging.value = false
  snapOrReset()
}

// ─── Mouse（桌面左滑）───────────────────────────────────────────
function onMouseDown(e) {
  if (props.ticket.status === 'settled') return
  startX      = e.clientX
  startSlide  = slideX.value
  isDragging.value = true
  window.addEventListener('mousemove', onMouseMove)
  window.addEventListener('mouseup',   onMouseUp)
}
function onMouseMove(e) {
  if (!isDragging.value) return
  applySlide(startSlide + (e.clientX - startX))
}
function onMouseUp() {
  isDragging.value = false
  window.removeEventListener('mousemove', onMouseMove)
  window.removeEventListener('mouseup',   onMouseUp)
  snapOrReset()
}

function applySlide(x) {
  slideX.value = Math.max(-REVEAL_WIDTH, Math.min(0, x))
}

function snapOrReset() {
  slideX.value = slideX.value < -SNAP_THRESHOLD ? -REVEAL_WIDTH : 0
}

// 點擊卡片：已滑開就收回；未滑開就選取
function onCardClick() {
  if (Math.abs(slideX.value) > 4) {
    slideX.value = 0
    return
  }
  emit('select', props.ticket.id)
}

// ─── 刪除 ────────────────────────────────────────────────────────
function handleDelete() {
  showConfirm.value = true
}
function cancelDelete() {
  showConfirm.value = false
  slideX.value      = 0
}
async function confirmDelete() {
  deleting.value = true
  try {
    await store.deleteVehicle(props.ticket.id)
  } catch (e) {
    alert(e.response?.data?.message || '刪除失敗')
    showConfirm.value = false
    slideX.value      = 0
  } finally {
    deleting.value = false
  }
}

// ─── Helpers ────────────────────────────────────────────────────
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
