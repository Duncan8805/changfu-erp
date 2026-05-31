import { computed } from 'vue'

/**
 * useWeightCalc — 即時重量計算 composable（前端預覽用，最終數字以後端為準）
 * @param {Ref<string|number>} grossKg  - 總重 (ref)
 * @param {Ref<string|number>} tareKg   - 空重 (ref)
 * @param {Ref<string|number>} unitPrice - 單價 (ref)
 */
export function useWeightCalc(grossKg, tareKg, unitPrice) {
  const netKg = computed(() => {
    const g = parseFloat(grossKg.value) || 0
    const t = parseFloat(tareKg.value) || 0
    return Math.max(0, g - t)
  })

  const netJin = computed(() => Math.round(netKg.value / 0.6))

  const totalAmount = computed(() =>
    Math.round(netJin.value * (parseFloat(unitPrice.value) || 0))
  )

  const weightError = computed(() => {
    const g = parseFloat(grossKg.value) || 0
    const t = parseFloat(tareKg.value) || 0
    return t > 0 && g > 0 && t > g
  })

  return { netKg, netJin, totalAmount, weightError }
}
