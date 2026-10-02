import type { Role } from '@/Shared/models/Role'

export interface CreateUser {
  name: string
  email: string
  password: string
  confirmPassword: string
  role: Role | null
}
