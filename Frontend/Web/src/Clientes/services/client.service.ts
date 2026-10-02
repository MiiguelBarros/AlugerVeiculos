import http from '@/Shared/services/http'
import type { ResponseDTO } from '@/Shared/models/ResponseDTO'
import type { Client } from '@/Clientes/models/Client'
import type { ClientFilter } from '@/Clientes/models/ClientFilter'
import type { ClientRequest } from '@/Clientes/models/ClientRequest'

export const clientService = {
  async getAll(filter: ClientFilter): Promise<Client[]> {
    const response = await http.get<ResponseDTO<Client[]>>('/clients', { params: filter })
    return response.data.data ?? []
  },

  async getById(clientId: number): Promise<Client> {
    const response = await http.get<ResponseDTO<Client>>(`/clients/${clientId}`)
    return response.data.data!
  },

  async create(request: ClientRequest): Promise<Client> {
    const response = await http.post<ResponseDTO<Client>>('/clients', request)
    return response.data.data!
  },

  async update(clientId: number, request: ClientRequest): Promise<Client> {
    const response = await http.put<ResponseDTO<Client>>(`/clients/${clientId}`, request)
    return response.data.data!
  },

  async deactivate(clientId: number): Promise<void> {
    await http.patch(`/clients/${clientId}/deactivate`)
  },

  async reactivate(clientId: number): Promise<void> {
    await http.patch(`/clients/${clientId}/reactivate`)
  },
}
