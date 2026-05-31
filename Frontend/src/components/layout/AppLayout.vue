<template>
  <div class="min-h-screen flex flex-col bg-gray-950">
    <AppHeader />
    <main class="flex-1 overflow-hidden">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import AppHeader from './AppHeader.vue'
import { RouterView } from 'vue-router'
</script>
