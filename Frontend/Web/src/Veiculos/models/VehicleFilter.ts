import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { VehicleAvailability } from '@/Veiculos/models/VehicleAvailability'

export interface VehicleFilter {
  status?: RecordStatus
  availability?: VehicleAvailability
}
