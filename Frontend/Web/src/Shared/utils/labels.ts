import type { ContractStatus } from '@/Contratos/models/ContractStatus'
import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { Role } from '@/Shared/models/Role'
import type { TagSeverity } from '@/Shared/models/TagSeverity'
import type { FuelType } from '@/Veiculos/models/FuelType'
import type { VehicleAvailability } from '@/Veiculos/models/VehicleAvailability'

export const roleLabels: Record<Role, string> = {
  Manager: 'Gestor',
  Employee: 'Funcionário',
}

export const recordStatusLabels: Record<RecordStatus, string> = {
  Active: 'Ativo',
  Inactive: 'Inativo',
}

export const recordStatusSeverities: Record<RecordStatus, TagSeverity> = {
  Active: 'success',
  Inactive: 'secondary',
}

export const fuelTypeLabels: Record<FuelType, string> = {
  Gasoline: 'Gasolina',
  Diesel: 'Gasóleo',
  Hybrid: 'Híbrido',
  Electric: 'Elétrico',
}

export const vehicleAvailabilityLabels: Record<VehicleAvailability, string> = {
  Available: 'Disponível',
  Rented: 'Alugado',
  Reserved: 'Reservado',
}

export const vehicleAvailabilitySeverities: Record<VehicleAvailability, TagSeverity> = {
  Available: 'success',
  Rented: 'info',
  Reserved: 'warn',
}

export const contractStatusLabels: Record<ContractStatus, string> = {
  Scheduled: 'Agendado',
  Active: 'Ativo',
  Overdue: 'Em atraso',
  Completed: 'Concluído',
  Cancelled: 'Cancelado',
}

export const contractStatusSeverities: Record<ContractStatus, TagSeverity> = {
  Scheduled: 'secondary',
  Active: 'info',
  Overdue: 'danger',
  Completed: 'success',
  Cancelled: 'secondary',
}

export function toOptions<T extends string>(labels: Record<T, string>): { label: string; value: T }[] {
  return (Object.keys(labels) as T[]).map((value) => ({ label: labels[value], value }))
}
