import http from '@/Shared/services/http'
import type { ResponseDTO } from '@/Shared/models/ResponseDTO'
import type { CreateUser } from '@/Utilizadores/models/CreateUser'
import type { User } from '@/Utilizadores/models/User'

export const userService = {
  async getAll(): Promise<User[]> {
    const response = await http.get<ResponseDTO<User[]>>('/users')
    return response.data.data ?? []
  },

  async getById(userId: number): Promise<User> {
    const response = await http.get<ResponseDTO<User>>(`/users/${userId}`)
    return response.data.data!
  },

  async create(request: CreateUser): Promise<User> {
    const response = await http.post<ResponseDTO<User>>('/users', request)
    return response.data.data!
  },

  async deactivate(userId: number): Promise<void> {
    await http.patch(`/users/${userId}/deactivate`)
  },

  async reactivate(userId: number): Promise<void> {
    await http.patch(`/users/${userId}/reactivate`)
  },
}
