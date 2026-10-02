import type { Role } from '@/Shared/models/Role'

export interface AuthUser {
  id: number
  name: string
  role: Role
}
