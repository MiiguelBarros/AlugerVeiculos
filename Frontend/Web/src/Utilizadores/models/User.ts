import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { Role } from '@/Shared/models/Role'

export interface User {
  userId: number
  name: string
  email: string
  role: Role
  status: RecordStatus
  createdAt: string
}
