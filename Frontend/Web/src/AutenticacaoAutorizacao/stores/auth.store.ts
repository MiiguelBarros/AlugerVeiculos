import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { jwtDecode } from 'jwt-decode'
import type { AuthUser } from '@/AutenticacaoAutorizacao/models/AuthUser'
import type { Login } from '@/AutenticacaoAutorizacao/models/Login'
import type { Role } from '@/Shared/models/Role'
import { authService } from '@/AutenticacaoAutorizacao/services/auth.service'

const ClaimTypes = {
  nameIdentifier: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  name: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
  role: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
} as const

type TokenClaims = Record<string, string>

function decodeUser(token: string): AuthUser {
  const claims = jwtDecode<TokenClaims>(token)

  return {
    id: Number(claims[ClaimTypes.nameIdentifier]),
    name: claims[ClaimTypes.name] ?? '',
    role: claims[ClaimTypes.role] as Role,
  }
}

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const initialized = ref(false)
  let refreshPromise: Promise<boolean> | null = null

  const user = computed<AuthUser | null>(() => (accessToken.value ? decodeUser(accessToken.value) : null))
  const isAuthenticated = computed(() => user.value !== null)
  const isManager = computed(() => user.value?.role === 'Manager')

  function hasRole(roles: Role[]): boolean {
    return user.value !== null && roles.includes(user.value.role)
  }

  async function login(credentials: Login): Promise<void> {
    accessToken.value = await authService.login(credentials)
  }

  function refreshSession(): Promise<boolean> {
    refreshPromise ??= authService
      .refresh()
      .then((token) => {
        accessToken.value = token
        return true
      })
      .catch(() => {
        accessToken.value = null
        return false
      })
      .finally(() => {
        refreshPromise = null
      })

    return refreshPromise
  }

  async function initialize(): Promise<void> {
    if (initialized.value)
      return

    initialized.value = true
    await refreshSession()
  }

  async function logout(): Promise<void> {
    try {
      await authService.logout()
    } finally {
      accessToken.value = null
    }
  }

  return { accessToken, user, isAuthenticated, isManager, hasRole, login, refreshSession, initialize, logout }
})
