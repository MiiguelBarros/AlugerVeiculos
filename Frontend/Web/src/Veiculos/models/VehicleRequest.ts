import type { FuelType } from '@/Veiculos/models/FuelType'

export interface VehicleRequest {
  brand: string
  model: string
  licensePlate: string
  year: number | null
  fuelType: FuelType | null
}
