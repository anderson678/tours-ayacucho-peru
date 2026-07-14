import axios from 'axios'

const configuredApiUrl = import.meta.env.VITE_API_BASE_URL?.trim()

export const API_BASE_URL = (configuredApiUrl || 'https://tours-ayacucho-api.runasp.net')
  .replace(/\/+$/, '')

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

    const finalUrl = `${config.baseURL?.replace(/\/+$/, '')}/${config.url?.replace(/^\/+/, '')}`
    console.info('[API request]', config.method?.toUpperCase(), finalUrl)
    return config
  },
  (error) => Promise.reject(error)
)

apiClient.interceptors.response.use(
  (response) => {
    console.info('[API response]', response.status, response.config.url, response.data)
    return response
  },
  (error) => {
    const config = error.config
    const finalUrl = config
      ? `${config.baseURL?.replace(/\/+$/, '')}/${config.url?.replace(/^\/+/, '')}`
      : 'URL no disponible'

    console.error('[API error]', {
      url: finalUrl,
      status: error.response?.status ?? 'sin respuesta HTTP',
      body: error.response?.data ?? error.message,
      code: error.code,
    })

    // No destruimos la sesion por un 401 aislado. El panel administrativo carga
    // varias secciones en paralelo y una sola respuesta no autorizada no implica
    // que el token almacenado sea invalido. AuthContext controla la expiracion y
    // el cierre de sesion explicito.
    if (error.response?.status === 401) {
      console.warn('[API auth] La API rechazo esta solicitud; se conserva la sesion para diagnostico.', {
        url: finalUrl,
        response: error.response.data,
      })
    }
    return Promise.reject(error)
  }
)

export const getApiErrorMessage = (error, fallback = 'Ocurrio un error al procesar la solicitud') => {
  const data = error?.response?.data
  if (!data) {
    return error?.code === 'ERR_NETWORK'
      ? `No se pudo conectar con la API configurada en ${API_BASE_URL}.`
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
