<template>
  <RouterView />
  <NumericKeypadSheet />
</template>

<script setup>
import { RouterView } from 'vue-router'
import NumericKeypadSheet from '@/components/shared/NumericKeypadSheet.vue'
</script>
