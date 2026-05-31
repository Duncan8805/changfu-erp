import axios from 'axios'
import router from '@/router'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
})

// ─── Request interceptor：自動帶 token ──────────────────────
api.interceptors.request.use(config => {
  const token = localStorage.getItem('changfu_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ─── Response interceptor：401 自動跳登入 ───────────────────
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('changfu_token')
      router.push({ name: 'Login' })
    }
    return Promise.reject(err)
  }
)

export default api
