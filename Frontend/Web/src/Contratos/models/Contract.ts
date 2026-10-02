import type { ContractStatus } from '@/Contratos/models/ContractStatus'

export interface Contract {
  contractId: number
  clientId: number
  clientName: string
  vehicleId: number
  vehicleLicensePlate: string
  vehicleBrand: string
  vehicleModel: string
  startDate: string
  endDate: string
  startMileage: number
  endMileage: number | null
  returnedAt: string | null
  cancelledAt: string | null
  status: ContractStatus
  createdAt: string
}
