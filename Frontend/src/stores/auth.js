import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/api'
import router from '@/router'

export const useAuthStore = defineStore('auth', () => {
  // ─── State ────────────────────────────────────────────────
  const token    = ref(localStorage.getItem('changfu_token') || null)
  const username = ref(localStorage.getItem('changfu_username') || null)
  const role     = ref(localStorage.getItem('changfu_role') || null)

  // ─── Getters ──────────────────────────────────────────────
  const isLoggedIn = computed(() => !!token.value)
  const isAdmin    = computed(() => role.value === 'admin')

  // ─── Actions ──────────────────────────────────────────────
  async function login(usernameVal, password) {
    const { data } = await api.post('/auth/login', {
      username: usernameVal,
      password,
    })

    token.value    = data.token
    username.value = data.username
    role.value     = data.role

    localStorage.setItem('changfu_token',    data.token)
    localStorage.setItem('changfu_username', data.username)
    localStorage.setItem('changfu_role',     data.role)
  }

  function logout() {
    token.value    = null
    username.value = null
    role.value     = null

    localStorage.removeItem('changfu_token')
    localStorage.removeItem('changfu_username')
    localStorage.removeItem('changfu_role')

    router.push({ name: 'Login' })
  }

  return { token, username, role, isLoggedIn, isAdmin, login, logout }
})
