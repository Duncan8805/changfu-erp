import { ref, computed, shallowRef } from 'vue'

// ─── 全域單例狀態（跨元件共用）─────────────────────────────────
const isOpen       = ref(false)
const currentLabel = ref('')
const buffer       = ref('')          // 使用者目前輸入的數字字串
const maxDigits    = ref(7)
const _onConfirm   = shallowRef(null)
const _subFmt      = shallowRef(null) // (num, buf) => 副標題字串

// 主顯示：預設直接顯示 buffer
const displayValue = computed(() => buffer.value || '')

// 副顯示：例如單價 1105 → "→ $11.05 / 台斤"
const subDisplay = computed(() => {
  if (!_subFmt.value) return ''
  const num = parseInt(buffer.value) || 0
  return _subFmt.value(num, buffer.value)
})

export function useNumericKeypad() {
  /**
   * 開啟鍵盤
   * @param {object} options
   * @param {string}   options.label        - 欄位標籤
   * @param {number}   options.value        - 初始值（整數）
   * @param {number}   [options.max=7]      - 最大位數
   * @param {function} options.onConfirm    - 確認後回呼 (value: number) => void
   * @param {function} [options.subFormatter] - 副標題格式化 (num, buf) => string
   */
  function open({ label, value = 0, max = 7, onConfirm, subFormatter }) {
    currentLabel.value = label
    buffer.value       = value > 0 ? String(Math.round(value)) : ''
    maxDigits.value    = max
    _onConfirm.value   = onConfirm
    _subFmt.value      = subFormatter ?? null
    isOpen.value       = true
  }

  function close() { isOpen.value = false }

  function handleConfirm() {
    _onConfirm.value?.(parseInt(buffer.value) || 0)
    close()
  }

  function handleCancel() { close() }

  /** 按下鍵盤按鍵：數字字串 / 'del' / 'confirm' */
  function pressKey(key) {
    if (key === 'del') {
      buffer.value = buffer.value.slice(0, -1)
    } else if (key === 'confirm') {
      handleConfirm()
    } else {
      // 防止前導零
      if (buffer.value === '' && key === '0') return
      if (buffer.value.length >= maxDigits.value) return
      buffer.value += key
    }
  }

  return {
    isOpen, currentLabel, buffer, displayValue, subDisplay,
    open, close, handleConfirm, handleCancel, pressKey,
  }
}
