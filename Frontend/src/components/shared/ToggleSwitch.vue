<template>
  <button
    :id="id"
    type="button"
    role="switch"
    :aria-checked="modelValue"
    class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent
           transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-offset-2
           focus:ring-offset-gray-900"
    :class="[
      modelValue ? activeClass : 'bg-gray-700',
      'focus:ring-brand-500'
    ]"
    @click="$emit('update:modelValue', !modelValue)"
  >
    <span class="sr-only">{{ label }}</span>
    <span
      class="pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0
             transition duration-200 ease-in-out"
      :class="modelValue ? 'translate-x-5' : 'translate-x-0'"
    />
  </button>
</template>

<script setup>
defineProps({
  modelValue: { type: Boolean, required: true },
  label:      { type: String,  default: 'toggle' },
  id:         { type: String,  default: 'toggle-switch' },
  activeClass:{ type: String,  default: 'bg-brand-600' },
})
defineEmits(['update:modelValue'])
</script>
