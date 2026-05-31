<template>
  <div class="flex h-[calc(100vh-3.5rem)] overflow-hidden">
    <!-- Left: Vehicle Panel (fixed width) -->
    <aside class="w-72 xl:w-80 flex-shrink-0 border-r border-white/10 bg-gray-900/50">
      <VehiclePanel />
    </aside>

    <!-- Right: Ticket Form (flex-1, relative for absolute CheckoutBar) -->
    <div class="flex-1 flex flex-col relative overflow-hidden bg-gray-950">
      <TicketForm />
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'
import { useRiceTypeStore } from '@/stores/riceTypes'
import VehiclePanel from '@/components/weighing/VehiclePanel.vue'
import TicketForm from '@/components/weighing/TicketForm.vue'

const vehicleStore  = useVehicleStore()
const riceTypeStore = useRiceTypeStore()

onMounted(async () => {
  await Promise.all([
    vehicleStore.fetchVehicles(),
    riceTypeStore.fetchRiceTypes(),
  ])
})
</script>
