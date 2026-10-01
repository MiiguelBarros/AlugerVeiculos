import http from '@/Shared/services/http'
import type { ResponseDTO } from '@/Shared/models/ResponseDTO'
import type { Vehicle } from '@/Veiculos/models/Vehicle'
import type { VehicleFilter } from '@/Veiculos/models/VehicleFilter'
import type { VehicleRequest } from '@/Veiculos/models/VehicleRequest'

export const vehicleService = {
  async getAll(filter: VehicleFilter): Promise<Vehicle[]> {
    const response = await http.get<ResponseDTO<Vehicle[]>>('/vehicles', { params: filter })
    return response.data.data ?? []
  },

  async getById(vehicleId: number): Promise<Vehicle> {
    const response = await http.get<ResponseDTO<Vehicle>>(`/vehicles/${vehicleId}`)
    return response.data.data!
  },

  async create(request: VehicleRequest): Promise<Vehicle> {
    const response = await http.post<ResponseDTO<Vehicle>>('/vehicles', request)
    return response.data.data!
  },

  async update(vehicleId: number, request: VehicleRequest): Promise<Vehicle> {
    const response = await http.put<ResponseDTO<Vehicle>>(`/vehicles/${vehicleId}`, request)
    return response.data.data!
  },

  async deactivate(vehicleId: number): Promise<void> {
    await http.patch(`/vehicles/${vehicleId}/deactivate`)
  },

  async reactivate(vehicleId: number): Promise<void> {
    await http.patch(`/vehicles/${vehicleId}/reactivate`)
  },
}
