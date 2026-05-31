import { createRouter, createWebHashHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Lazy-loaded views
const LoginView    = () => import('@/views/LoginView.vue')
const AppLayout    = () => import('@/components/layout/AppLayout.vue')
const WeighingView = () => import('@/views/WeighingView.vue')
const DashboardView = () => import('@/views/DashboardView.vue')
const SettingsView = () => import('@/views/SettingsView.vue')

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { requiresGuest: true },
  },
  {
    path: '/',
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/weighing' },
      { path: 'weighing',  name: 'Weighing',  component: WeighingView },
      { path: 'dashboard', name: 'Dashboard', component: DashboardView },
      { path: 'settings',  name: 'Settings',  component: SettingsView },
    ],
  },
  // Catch-all
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHashHistory(),   // Hash mode：GitHub Pages 相容
  routes,
})

// ─── Navigation Guard ──────────────────────────────────────
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresGuest && auth.isLoggedIn) {
    return { path: '/weighing' }
  }
})

export default router
