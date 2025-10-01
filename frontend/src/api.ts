
import axios from 'axios'
import toast from 'react-hot-toast'
import type { ValidationProblem } from './types'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5143'

export const api = axios.create({ baseURL, headers: { 'Content-Type': 'application/json' } })

api.interceptors.response.use(
  r => r,
  (error) => {
    const data: ValidationProblem | undefined = error.response?.data
    if (data?.errors) {
      const flat = Object.entries(data.errors).flatMap(([k, v]) => v.map(m => `${k}: ${m}`))
      if (flat.length) toast.error(flat.join('\n'))
    } else if (data?.detail || data?.title) {
      toast.error(`${data.title ?? 'Error'}${data.detail ? ' - ' + data.detail : ''}`)
    } else if (typeof error.response?.data === 'string') {
      toast.error(error.response.data)
    } else {
      toast.error(error.message ?? 'Request failed')
    }
    return Promise.reject(error)
  }
)

export async function getVehicles() { return (await api.get('/api/vehicles')).data }
export async function getVehicle(id: string) { return (await api.get(`/api/vehicles/${id}`)).data }
export async function createVehicle(payload: any) { return (await api.post('/api/vehicles', payload)).data }
export async function updateVehicle(id: string, payload: any) { return (await api.put(`/api/vehicles/${id}`, payload)).data }
export async function deleteVehicle(id: string) { return (await api.delete(`/api/vehicles/${id}`)).data }

export async function getTrackingPoints(vehicleId: string) {
  const { data } = await api.get(`/api/tracking/${vehicleId}`)

  // vira sempre um array
  const list =
    Array.isArray(data) ? data :
    Array.isArray(data?.items) ? data.items :
    Array.isArray(data?.data) ? data.data :
    Array.isArray(data?.points) ? data.points :
    (data ? [data] : [])

  return list
}

export async function uploadTrackingJson(items: any[]) {
  return (await api.post('/api/tracking/upload/', items, {
    validateStatus: (s) => (s >= 200 && s < 300) || s === 207
  })).data
}