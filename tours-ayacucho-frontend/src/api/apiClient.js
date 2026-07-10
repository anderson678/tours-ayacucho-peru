import axios from 'axios'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5150'

const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
})

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('user_data')
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export const getApiErrorMessage = (error, fallback = 'Ocurrio un error al procesar la solicitud') => {
  const data = error?.response?.data
  if (!data) {
    return error?.code === 'ERR_NETWORK'
      ? 'No se pudo conectar con la API. Verifica que el backend este activo en http://localhost:5150.'
      : fallback
  }

  if (typeof data === 'string') return data
  if (data.mensaje) return data.mensaje
  if (Array.isArray(data.detalle) && data.detalle.length > 0) return data.detalle.join(' ')
  if (data.detail) return data.detail
  if (data.title) return data.title

  if (data.errors) {
    const values = Object.values(data.errors).flat()
    if (values.length > 0) return values.join(' ')
  }

  return fallback
}

export default apiClient
