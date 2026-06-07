<template>
  <div class="flex flex-col h-full">
    <!-- Top: Add button + filter -->
    <div class="flex-shrink-0 border-b border-white/10">
      <div class="p-3 flex gap-2">
        <button
          id="add-vehicle-btn"
          class="btn-primary flex-1 gap-2"
          @click="showModal = true"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          新增進場車輛
        </button>

        <!-- 垃圾桶按鈕 -->
        <button
          class="btn-ghost px-3 py-2 relative"
          title="已刪除的傳票（7天內可恢復）"
          @click="openTrash"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-gray-400" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6"/>
            <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
            <path d="M10 11v6M14 11v6"/>
            <path d="M9 6V4h6v2"/>
          </svg>
        </button>
      </div>
      <VehicleFilterBar :current-filter="filterStatus" @update:filter="store.setFilterStatus" />
    </div>

    <!-- Scrollable list -->
    <div class="flex-1 overflow-y-auto">

      <!-- Active vehicles (unloading + pending) -->
      <div class="p-2 space-y-1">
        <TransitionGroup name="slide-up">
          <template v-for="ticket in activeList" :key="ticket.id">
            <VehicleCard
              :ticket="ticket"
              :is-active="ticket.id === store.activeVehicleId"
              @select="store.setActiveVehicle"
            />
          </template>
        </TransitionGroup>

        <!-- Empty state -->
        <div v-if="activeList.length === 0" class="text-center py-10 text-gray-600">
          <svg class="w-10 h-10 mx-auto mb-2 opacity-30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
              d="M9 17a2 2 0 11-4 0 2 2 0 014 0zM19 17a2 2 0 11-4 0 2 2 0 014 0z"/>
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
              d="M13 16V6a1 1 0 00-1-1H4a1 1 0 00-1 1v10l2 2h1m6-1h1l6-6v-3l-4-2h-3v5l-2 2"/>
          </svg>
          <p class="text-xs">目前無車輛</p>
        </div>
      </div>

      <!-- Settled section (collapsible) -->
      <details v-if="settledList.length > 0" class="border-t border-white/10">
        <summary class="flex items-center justify-between px-4 py-2.5 cursor-pointer
                        text-xs font-medium text-gray-500 hover:text-gray-300 hover:bg-white/5
                        select-none list-none transition-colors">
          <span class="flex items-center gap-1.5">
            <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
            </svg>
            今日已完成
          </span>
          <span class="bg-green-500/20 text-green-400 px-1.5 py-0.5 rounded-full text-[10px]">
            {{ settledList.length }} 台
          </span>
        </summary>
        <div class="p-2 space-y-1 bg-black/10">
          <VehicleCard
            v-for="ticket in settledList"
            :key="ticket.id"
            :ticket="ticket"
            :is-active="ticket.id === store.activeVehicleId"
            @select="store.setActiveVehicle"
          />
        </div>
      </details>
    </div>

    <!-- Add vehicle modal -->
    <AddVehicleModal v-model="showModal" />

    <!-- 垃圾桶 Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="showTrash"
          class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
          @click.self="showTrash = false"
        >
          <div class="glass w-full max-w-sm shadow-2xl shadow-black/60 max-h-[80vh] flex flex-col">
            <!-- Header -->
            <div class="flex items-center justify-between p-4 border-b border-white/10 flex-shrink-0">
              <h3 class="text-base font-semibold text-white flex items-center gap-2">
                <svg class="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <polyline points="3 6 5 6 21 6"/>
                  <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                </svg>
                已刪除傳票
              </h3>
              <div class="flex items-center gap-2">
                <span class="text-xs text-gray-500">7天內可恢復</span>
                <button class="btn-ghost p-1.5 rounded-lg" @click="showTrash = false">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Body -->
            <div class="flex-1 overflow-y-auto p-3 space-y-2">
              <div v-if="trashLoading" class="py-8 text-center text-gray-500 text-sm">載入中…</div>

              <div v-else-if="trashedTickets.length === 0" class="py-8 text-center text-gray-600">
                <svg class="w-10 h-10 mx-auto mb-2 opacity-30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <polyline points="3 6 5 6 21 6"/>
                  <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                </svg>
                <p class="text-xs">垃圾桶是空的</p>
              </div>

              <div
                v-for="t in trashedTickets"
                :key="t.id"
                class="flex items-center justify-between glass p-3 rounded-xl"
              >
                <div class="min-w-0">
                  <p class="font-mono font-semibold text-sm text-white">{{ t.vehicleNo || '—' }}</p>
                  <p class="text-xs text-gray-400 truncate">{{ t.farmerName || '—' }}</p>
                  <p class="text-[10px] text-gray-600 font-mono">{{ t.ticketNo }}</p>
                </div>
                <button
                  class="btn-secondary btn-sm flex-shrink-0 ml-3"
                  :disabled="restoring === t.id"
                  @click="doRestore(t.id)"
                >
                  {{ restoring === t.id ? '恢復中…' : '恢復' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'
import VehicleCard from './VehicleCard.vue'
import VehicleFilterBar from './VehicleFilterBar.vue'
import AddVehicleModal from '@/components/shared/AddVehicleModal.vue'

const store = useVehicleStore()
const showModal = ref(false)

const filterStatus = computed(() => store.filterStatus)

const activeList = computed(() => {
  const filter = store.filterStatus
  if (!filter) return store.activeVehicles
  return store.vehicles.filter(v => v.status === filter)
})

const settledList = computed(() => {
  if (store.filterStatus && store.filterStatus !== 'settled') return []
  return store.settledVehicles
})

// ─── Trash ────────────────────────────────────────────────────
const showTrash     = ref(false)
const trashLoading  = ref(false)
const trashedTickets = ref([])
const restoring     = ref(null)

async function openTrash() {
  showTrash.value    = true
  trashLoading.value = true
  try {
    trashedTickets.value = await store.fetchTrashed()
  } finally {
    trashLoading.value = false
  }
}

async function doRestore(id) {
  restoring.value = id
  try {
    await store.restoreVehicle(id)
    trashedTickets.value = trashedTickets.value.filter(t => t.id !== id)
  } catch (e) {
    alert(e.response?.data?.message || '恢復失敗')
  } finally {
    restoring.value = null
  }
}
</script>
