import http from '@/Shared/services/http'
import type { ResponseDTO } from '@/Shared/models/ResponseDTO'
import type { Login } from '@/AutenticacaoAutorizacao/models/Login'

export const authService = {
  async login(login: Login): Promise<string> {
    const response = await http.post<ResponseDTO<string>>('/auth/login', login)
    return response.data.data!
  },

  async refresh(): Promise<string> {
    const response = await http.post<ResponseDTO<string>>('/auth/refresh')
    return response.data.data!
  },

  async logout(): Promise<void> {
    await http.post('/auth/logout')
  },
}
