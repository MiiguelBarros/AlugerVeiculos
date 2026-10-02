import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { FuelType } from '@/Veiculos/models/FuelType'
import type { VehicleAvailability } from '@/Veiculos/models/VehicleAvailability'

export interface Vehicle {
  vehicleId: number
  brand: string
  model: string
  licensePlate: string
  year: number
  fuelType: FuelType
  status: RecordStatus
  availability: VehicleAvailability
  lastMileage: number | null
  createdAt: string
}
