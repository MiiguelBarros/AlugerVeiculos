export interface ResponseDTO<T> {
  success: boolean
  message: string
  data: T | null
  statusCode: number
}

export type ValidationErrors = Record<string, string[]>
