<template>
  <Teleport to="body">
    <Transition name="fade">
      <div
        v-if="modelValue"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        @click.self="close"
      >
        <Transition name="slide-up">
          <div
            v-if="modelValue"
            class="glass w-full max-w-sm shadow-2xl shadow-black/60 p-6"
            @keydown.esc="close"
          >
            <!-- Header -->
            <div class="flex items-center justify-between mb-5">
              <h3 class="text-base font-semibold text-white flex items-center gap-2">
                <svg class="w-5 h-5 text-brand-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M12 4v16m8-8H4"/>
                </svg>
                新增進場車輛
              </h3>
              <button class="btn-ghost p-1.5 rounded-lg" @click="close">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>

            <!-- Error -->
            <Transition name="slide-up">
              <div v-if="error" class="mb-4 px-3 py-2 rounded-lg bg-red-500/10 border border-red-500/30 text-red-300 text-sm">
                {{ error }}
              </div>
            </Transition>

            <!-- Form -->
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div>
                <label class="form-label" for="new-vehicle-no">車號 <span class="text-red-400">*</span></label>
                <input
                  id="new-vehicle-no"
                  ref="vehicleNoRef"
                  v-model="form.vehicleNo"
                  type="text"
                  class="input-base uppercase"
                  placeholder="例：AB-1234"
                  required
                  autocomplete="off"
                  @input="form.vehicleNo = form.vehicleNo.toUpperCase()"
                />
              </div>

              <div>
                <label class="form-label" for="new-farmer-name">農民姓名</label>
                <input
                  id="new-farmer-name"
                  v-model="form.farmerName"
                  type="text"
                  class="input-base"
                  placeholder="例：林大牛"
                  autocomplete="off"
                />
              </div>

              <div>
                <label class="form-label" for="new-village">村別 / 行號</label>
                <input
                  id="new-village"
                  v-model="form.village"
                  type="text"
                  class="input-base"
                  placeholder="例：松梅里"
                  autocomplete="off"
                />
              </div>

              <div>
                <label class="form-label" for="new-gross-weight">總重 (kg) <span class="text-red-400">*</span></label>
                <NumericInput
                  :model-value="form.grossWeightKg"
                  label="總重 (kg)"
                  placeholder="0"
                  :max-digits="6"
                  @update:model-value="form.grossWeightKg = $event"
                />
              </div>

              <!-- Actions -->
              <div class="flex gap-3 pt-2">
                <button type="button" class="btn-secondary flex-1" @click="close">取消</button>
                <button
                  id="add-vehicle-submit"
                  type="submit"
                  class="btn-primary flex-1"
                  :disabled="loading"
                >
                  <svg v-if="loading" class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                  </svg>
                  {{ loading ? '建立中...' : '確認進場' }}
                </button>
              </div>
            </form>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import { useVehicleStore } from '@/stores/vehicles'
import NumericInput from '@/components/shared/NumericInput.vue'

const props = defineProps({
  modelValue: { type: Boolean, required: true },
})
const emit = defineEmits(['update:modelValue'])

const vehicleStore = useVehicleStore()
const vehicleNoRef = ref(null)

const form = ref({ vehicleNo: '', farmerName: '', village: '', grossWeightKg: '' })
const loading = ref(false)
const error   = ref('')

// Focus first input when modal opens
watch(() => props.modelValue, async (val) => {
  if (val) {
    form.value = { vehicleNo: '', farmerName: '', village: '', grossWeightKg: '' }
    error.value = ''
    await nextTick()
    vehicleNoRef.value?.focus()
  }
})

function close() {
  if (!loading.value) emit('update:modelValue', false)
}

async function handleSubmit() {
  if (!form.value.vehicleNo.trim()) return
  const gross = parseFloat(form.value.grossWeightKg) || 0
  if (gross <= 0) {
    error.value = '請輸入總重'
    return
  }

  loading.value = true
  error.value   = ''
  try {
    await vehicleStore.addVehicle({
      vehicleNo:    form.value.vehicleNo.trim(),
      farmerName:   form.value.farmerName.trim(),
      village:      form.value.village.trim(),
      grossWeightKg: gross,
    })
    emit('update:modelValue', false)
  } catch (e) {
    error.value = e.response?.data?.message || '建立失敗，請稍後再試'
  } finally {
    loading.value = false
  }
}
</script>
