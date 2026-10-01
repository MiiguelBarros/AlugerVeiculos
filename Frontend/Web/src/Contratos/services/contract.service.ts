import http from '@/Shared/services/http'
import type { ResponseDTO } from '@/Shared/models/ResponseDTO'
import type { Contract } from '@/Contratos/models/Contract'
import type { ContractFilter } from '@/Contratos/models/ContractFilter'
import type { CreateContract } from '@/Contratos/models/CreateContract'
import type { ReturnContract } from '@/Contratos/models/ReturnContract'

export const contractService = {
  async getAll(filter: ContractFilter): Promise<Contract[]> {
    const response = await http.get<ResponseDTO<Contract[]>>('/contracts', { params: filter })
    return response.data.data ?? []
  },

  async getById(contractId: number): Promise<Contract> {
    const response = await http.get<ResponseDTO<Contract>>(`/contracts/${contractId}`)
    return response.data.data!
  },

  async create(request: CreateContract): Promise<Contract> {
    const response = await http.post<ResponseDTO<Contract>>('/contracts', request)
    return response.data.data!
  },

  async returnContract(contractId: number, request: ReturnContract): Promise<Contract> {
    const response = await http.patch<ResponseDTO<Contract>>(`/contracts/${contractId}/return`, request)
    return response.data.data!
  },

  async cancel(contractId: number): Promise<Contract> {
    const response = await http.patch<ResponseDTO<Contract>>(`/contracts/${contractId}/cancel`)
    return response.data.data!
  },
}
