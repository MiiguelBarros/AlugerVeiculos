import { isAxiosError } from 'axios'
import type { ResponseDTO, ValidationErrors } from '@/Shared/models/ResponseDTO'

const fallbackMessage = 'Não foi possível comunicar com o servidor. Tente novamente.'

function getResponse(error: unknown): ResponseDTO<unknown> | null {
  if (!isAxiosError<ResponseDTO<unknown>>(error) || !error.response?.data)
    return null

  return error.response.data
}

export function getErrorMessage(error: unknown): string {
  return getResponse(error)?.message || fallbackMessage
}

export function getValidationErrors(error: unknown): ValidationErrors {
  const response = getResponse(error)

  if (response?.statusCode !== 400 || !response.data || typeof response.data !== 'object')
    return {}

  return response.data as ValidationErrors
}
