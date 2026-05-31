<template>
  <div class="flex flex-col h-full">
    <!-- Top: Add button + filter -->
    <div class="flex-shrink-0 border-b border-white/10">
      <div class="p-3">
        <button
          id="add-vehicle-btn"
          class="btn-primary w-full gap-2"
          @click="showModal = true"
        >
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          新增進場車輛
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
</script>
