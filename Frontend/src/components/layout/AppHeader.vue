<template>
  <header class="h-14 bg-gray-900/80 backdrop-blur-md border-b border-white/10 flex items-center px-4 gap-4 flex-shrink-0 z-30">
    <!-- Logo -->
    <div class="flex items-center gap-2.5 mr-2">
      <div class="w-8 h-8 bg-brand-600/20 rounded-lg border border-brand-500/30 flex items-center justify-center flex-shrink-0">
        <svg class="w-4 h-4 text-brand-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
            d="M3 6l3 1m0 0l-3 9a5.002 5.002 0 006.001 0M6 7l3 9M6 7l6-2m6 2l3-1m-3 1l-3 9a5.002 5.002 0 006.001 0M18 7l3 9m-3-9l-6-2m0-2v2m0 16V5m0 16H9m3 0h3" />
        </svg>
      </div>
      <span class="text-white font-semibold text-sm hidden sm:block">長富稻穀廠</span>
    </div>

    <!-- Navigation -->
    <nav class="flex items-center gap-1 flex-1">
      <RouterLink
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-all duration-150"
        :class="[
          $route.path.startsWith(item.to)
            ? 'bg-brand-600/20 text-brand-300 border border-brand-500/30'
            : 'text-gray-400 hover:text-gray-200 hover:bg-white/5'
        ]"
      >
        <component :is="item.icon" class="w-4 h-4" />
        <span class="hidden sm:inline">{{ item.label }}</span>
        <!-- Pending badge -->
        <span
          v-if="item.badge && pendingCount > 0"
          class="inline-flex items-center justify-center w-4 h-4 rounded-full bg-amber-500 text-white text-[10px] font-bold leading-none"
        >
          {{ pendingCount > 9 ? '9+' : pendingCount }}
        </span>
      </RouterLink>
    </nav>

    <!-- Right: User info + logout -->
    <div class="flex items-center gap-3 ml-auto">
      <!-- Date & time -->
      <div class="hidden md:flex flex-col items-end text-right">
        <span class="text-xs text-gray-400">{{ dateStr }}</span>
        <span class="text-sm font-mono text-gray-300">{{ timeStr }}</span>
      </div>

      <!-- User -->
      <div class="flex items-center gap-2">
        <div class="w-8 h-8 rounded-full bg-brand-600/30 border border-brand-500/40 flex items-center justify-center">
          <span class="text-brand-300 text-xs font-bold">{{ userInitial }}</span>
        </div>
        <div class="hidden sm:block">
          <p class="text-white text-xs font-medium leading-tight">{{ auth.username }}</p>
          <p class="text-gray-500 text-[10px] leading-tight capitalize">{{ auth.role }}</p>
        </div>
      </div>

      <!-- Logout -->
      <button
        id="logout-btn"
        class="btn-ghost text-gray-500 hover:text-red-400 p-2 rounded-lg"
        title="登出"
        @click="auth.logout()"
      >
        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
        </svg>
      </button>
    </div>
  </header>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, h } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useVehicleStore } from '@/stores/vehicles'

const auth    = useAuthStore()
const vehicle = useVehicleStore()
const route   = useRoute()

const pendingCount = computed(() => vehicle.pendingCount)
const userInitial  = computed(() => (auth.username?.[0] ?? '?').toUpperCase())

// ─── Clock ──────────────────────────────────────────────────
const now     = ref(new Date())
let clockTimer = null

onMounted(() => { clockTimer = setInterval(() => { now.value = new Date() }, 1000) })
onUnmounted(() => clearInterval(clockTimer))

const dateStr = computed(() =>
  now.value.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit', weekday: 'short' })
)
const timeStr = computed(() =>
  now.value.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false })
)

// ─── Icon components (inline SVG) ───────────────────────────
const ScaleIcon = { render: () => h('svg', { fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M3 6l3 1m0 0l-3 9a5.002 5.002 0 006.001 0M6 7l3 9M6 7l6-2m6 2l3-1m-3 1l-3 9a5.002 5.002 0 006.001 0M18 7l3 9m-3-9l-6-2m0-2v2m0 16V5m0 16H9m3 0h3' })
]) }

const ChartIcon = { render: () => h('svg', { fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' })
]) }

const CogIcon = { render: () => h('svg', { fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z' }),
  h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M15 12a3 3 0 11-6 0 3 3 0 016 0z' })
]) }

const navItems = [
  { to: '/weighing',  label: '過磅作業', icon: ScaleIcon, badge: true },
  { to: '/dashboard', label: '營運報表', icon: ChartIcon, badge: false },
  { to: '/settings',  label: '系統設定', icon: CogIcon,  badge: false },
]
</script>
