import type { RecordStatus } from '@/Shared/models/RecordStatus'

export interface Client {
  clientId: number
  fullName: string
  email: string
  phone: string
  driverLicenseNumber: string
  status: RecordStatus
  createdAt: string
}
