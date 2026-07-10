import { createContext, useContext, useState, useEffect, useCallback } from 'react'
import apiClient from '../api/apiClient'
import toast from 'react-hot-toast'

const AuthContext = createContext(null)

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const storedToken = localStorage.getItem('jwt_token')
    const storedUser = localStorage.getItem('user_data')

    if (storedToken && storedUser) {
      try {
        const parsedUser = JSON.parse(storedUser)
        if (parsedUser.expiraEn && new Date(parsedUser.expiraEn) <= new Date()) {
          localStorage.removeItem('jwt_token')
          localStorage.removeItem('user_data')
        } else {
          setToken(storedToken)
          setUser(parsedUser)
        }
      } catch {
        localStorage.removeItem('jwt_token')
        localStorage.removeItem('user_data')
      }
    }

    setLoading(false)
  }, [])

  useEffect(() => {
    if (!user?.expiraEn) return undefined

    const millisecondsUntilExpiration = new Date(user.expiraEn).getTime() - Date.now()
    if (millisecondsUntilExpiration <= 0) {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('user_data')
      setToken(null)
      setUser(null)
      return undefined
    }

    const timer = window.setTimeout(() => {
      localStorage.removeItem('jwt_token')
      localStorage.removeItem('user_data')
      setToken(null)
      setUser(null)
      toast.error('La sesion expiro. Inicia sesion nuevamente.')
    }, millisecondsUntilExpiration)

    return () => window.clearTimeout(timer)
  }, [user?.expiraEn])

  const login = useCallback(async (correo, password) => {
    const normalizedEmail = correo.trim().toLowerCase()
    const response = await apiClient.post('/auth/login', {
      correo: normalizedEmail,
      password,
    })
    const data = response.data
    const userData = {
      id: data.clienteId,
      nombre: data.nombre,
      correo: data.correo ?? normalizedEmail,
      telefono: data.telefono,
      fotoUrl: data.fotoUrl,
      rol: data.rol,
      expiraEn: data.expiraEn,
    }

    localStorage.setItem('jwt_token', data.token)
    localStorage.setItem('user_data', JSON.stringify(userData))
    setToken(data.token)
    setUser(userData)
    return userData
  }, [])

  const register = useCallback(async (nombre, correo, password, telefono) => {
    const response = await apiClient.post('/auth/register', {
      nombre,
      correo,
      password,
      telefono,
    })
    return response.data
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('jwt_token')
    localStorage.removeItem('user_data')
    setToken(null)
    setUser(null)
    toast.success('Sesion cerrada correctamente')
  }, [])

  const updateUser = useCallback((updatedData) => {
    const updated = { ...user, ...updatedData }
    localStorage.setItem('user_data', JSON.stringify(updated))
    setUser(updated)
  }, [user])

  const isAdmin = ['Administrador', 'Admin'].includes(user?.rol)
  const isAuthenticated = !!token && !!user

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        isAuthenticated,
        isAdmin,
        login,
        register,
        logout,
        updateUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

export default AuthContext
