import type { ContractStatus } from '@/Contratos/models/ContractStatus'

export interface ContractFilter {
  status?: ContractStatus
  vehicleId?: number
  clientId?: number
}
