import axios, { type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import router from '@/router'

interface RetriableRequestConfig extends InternalAxiosRequestConfig {
  retried?: boolean
}

const http = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

http.interceptors.request.use((config) => {
  const authStore = useAuthStore()

  if (authStore.accessToken)
    config.headers.Authorization = `Bearer ${authStore.accessToken}`

  return config
})

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const request = error.config as RetriableRequestConfig | undefined
    const isAuthRequest = request?.url?.startsWith('/auth/')

    if (error.response?.status !== 401 || !request || request.retried || isAuthRequest)
      return Promise.reject(error)

    request.retried = true

    const authStore = useAuthStore()
    const refreshed = await authStore.refreshSession()

    if (!refreshed) {
      await router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
      return Promise.reject(error)
    }

    return http(request)
  },
)

export default http
