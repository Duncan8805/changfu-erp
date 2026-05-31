import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/api'

export const useRiceTypeStore = defineStore('riceTypes', () => {
  // ─── State ────────────────────────────────────────────────
  const riceTypes = ref([])
  const loading   = ref(false)
  const error     = ref(null)

  // ─── Actions ──────────────────────────────────────────────
  async function fetchRiceTypes() {
    loading.value = true
    error.value   = null
    try {
      const { data } = await api.get('/rice-types')
      riceTypes.value = data
    } catch (e) {
      error.value = e.response?.data?.message || '載入米種失敗'
    } finally {
      loading.value = false
    }
  }

  async function addRiceType(payload) {
    const { data } = await api.post('/rice-types', payload)
    riceTypes.value.push(data)
    return data
  }

  async function updateRiceType(id, payload) {
    const { data } = await api.put(`/rice-types/${id}`, payload)
    const idx = riceTypes.value.findIndex(r => r.id === id)
    if (idx !== -1) riceTypes.value[idx] = data
    return data
  }

  async function deleteRiceType(id) {
    await api.delete(`/rice-types/${id}`)
    riceTypes.value = riceTypes.value.filter(r => r.id !== id)
  }

  async function upsertPrice(riceTypeId, date, unitPrice) {
    const { data } = await api.post('/price-logs', {
      riceTypeId,
      priceDate: date,
      unitPrice,
    })
    // 更新 store 內的 todayPrice
    const item = riceTypes.value.find(r => r.id === riceTypeId)
    const today = new Date().toISOString().slice(0, 10)
    if (item && date === today) {
      item.todayPrice = data.unitPrice
    }
    return data
  }

  return {
    riceTypes, loading, error,
    fetchRiceTypes, addRiceType, updateRiceType, deleteRiceType, upsertPrice,
  }
})
