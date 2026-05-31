import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/api'

export const useVehicleStore = defineStore('vehicles', () => {
  // ─── State ────────────────────────────────────────────────
  const vehicles        = ref([])   // Ticket[]
  const activeVehicleId = ref(null)
  const filterStatus    = ref('')   // '' | 'unloading' | 'pending' | 'settled'
  const loading         = ref(false)
  const error           = ref(null)

  // ─── Getters ──────────────────────────────────────────────
  const activeVehicle = computed(() =>
    vehicles.value.find(v => v.id === activeVehicleId.value) ?? null
  )

  const filteredVehicles = computed(() => {
    if (!filterStatus.value) return vehicles.value
    return vehicles.value.filter(v => v.status === filterStatus.value)
  })

  const pendingCount = computed(() =>
    vehicles.value.filter(v => v.status === 'pending').length
  )

  const unloadingCount = computed(() =>
    vehicles.value.filter(v => v.status === 'unloading').length
  )

  const settledVehicles = computed(() =>
    vehicles.value.filter(v => v.status === 'settled')
  )

  const activeVehicles = computed(() =>
    vehicles.value.filter(v => v.status !== 'settled')
  )

  // ─── Actions ──────────────────────────────────────────────
  async function fetchVehicles(params = {}) {
    loading.value = true
    error.value   = null
    try {
      const { data } = await api.get('/tickets', { params })
      vehicles.value = data
    } catch (e) {
      error.value = e.response?.data?.message || '載入傳票失敗'
    } finally {
      loading.value = false
    }
  }

  async function addVehicle(payload) {
    const { data } = await api.post('/tickets', payload)
    vehicles.value.unshift(data)
    activeVehicleId.value = data.id
    return data
  }

  async function updateTicket(id, payload) {
    const { data } = await api.put(`/tickets/${id}`, payload)
    _replaceInList(data)
    return data
  }

  async function updateStatus(id, status) {
    const { data } = await api.patch(`/tickets/${id}/status`, { status })
    _replaceInList(data)
    return data
  }

  async function settleTicket(id, payload) {
    const { data } = await api.post(`/tickets/${id}/settle`, payload)
    _replaceInList(data)
    return data
  }

  function setActiveVehicle(id) {
    activeVehicleId.value = id
  }

  function setFilterStatus(status) {
    filterStatus.value = status
  }

  // ─── Internal helpers ─────────────────────────────────────
  function _replaceInList(ticket) {
    const idx = vehicles.value.findIndex(v => v.id === ticket.id)
    if (idx !== -1) vehicles.value[idx] = ticket
  }

  return {
    vehicles, activeVehicleId, filterStatus, loading, error,
    activeVehicle, filteredVehicles, pendingCount, unloadingCount,
    settledVehicles, activeVehicles,
    fetchVehicles, addVehicle, updateTicket, updateStatus, settleTicket,
    setActiveVehicle, setFilterStatus,
  }
})
