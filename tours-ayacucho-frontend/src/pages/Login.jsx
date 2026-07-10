import { useEffect, useState } from 'react'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  EnvelopeIcon,
  LockClosedIcon,
  EyeIcon,
  EyeSlashIcon,
  MapPinIcon,
  ArrowRightIcon,
  ExclamationCircleIcon,
  ClockIcon,
  ShieldCheckIcon,
  UserIcon,
} from '@heroicons/react/24/outline'

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const Login = () => {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [formData, setFormData] = useState({ correo: '', password: '' })
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [blockedUntil, setBlockedUntil] = useState(null)
  const [remainingSeconds, setRemainingSeconds] = useState(0)
  const [loading, setLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const [accessMode, setAccessMode] = useState('cliente')

  useEffect(() => {
    if (!blockedUntil) return undefined

    const updateRemaining = () => {
      const seconds = Math.max(0, Math.ceil((blockedUntil.getTime() - Date.now()) / 1000))
      setRemainingSeconds(seconds)
      if (seconds === 0) setBlockedUntil(null)
    }

    updateRemaining()
    const timer = window.setInterval(updateRemaining, 1000)
    return () => window.clearInterval(timer)
  }, [blockedUntil])

  const validate = () => {
    const errs = {}

    if (!formData.correo.trim()) {
      errs.correo = 'El correo electronico es requerido'
    } else if (!emailRegex.test(formData.correo.trim())) {
      errs.correo = 'El correo electronico no tiene un formato valido'
    }

    if (!formData.password) {
      errs.password = 'La contrasena es requerida'
    }

    return errs
  }

  const formatRemaining = (seconds) => {
    const minutes = Math.floor(seconds / 60)
    const rest = seconds % 60
    return `${minutes}:${String(rest).padStart(2, '0')}`
  }

  const getDestination = (userData) => {
    const requestedPath = location.state?.from?.pathname
    const userIsAdmin = userData.rol === 'Administrador' || userData.rol === 'Admin'
    const clientOnlyPaths = ['/mis-reservas', '/reservar', '/pago', '/reprogramar']

    if (userIsAdmin) return '/admin'
    if (requestedPath?.startsWith('/admin')) return '/mis-reservas'
    if (requestedPath && requestedPath !== '/login' && clientOnlyPaths.some((path) => requestedPath.startsWith(path))) {
      return requestedPath
    }

    return '/mis-reservas'
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setServerError('')

    const errs = validate()
    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }

    setErrors({})
    setLoading(true)
    try {
      const userData = await login(formData.correo.trim().toLowerCase(), formData.password)
      setBlockedUntil(null)
      toast.success('Inicio de sesion correcto')
      navigate(getDestination(userData), { replace: true })
    } catch (err) {
      const message = getApiErrorMessage(err, 'Credenciales incorrectas.')
      setServerError(message)

      if (err?.response?.status === 429) {
        const minutes = Number(err.response.data?.minutosRestantes || 15)
        setBlockedUntil(new Date(Date.now() + minutes * 60 * 1000))
      }

      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData((prev) => ({ ...prev, [name]: value }))
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: '' }))
    if (serverError) setServerError('')
  }

  const isBlocked = remainingSeconds > 0

  const selectAccessMode = (mode) => {
    setAccessMode(mode)
    setErrors({})
    setServerError('')

    if (mode === 'admin') {
      setFormData({
        correo: 'admin@toursayacuchoperu.com',
        password: 'Admin123@',
      })
      return
    }

    setFormData((prev) => {
      if (prev.correo === 'admin@toursayacuchoperu.com') {
        return { correo: '', password: '' }
      }
      return prev
    })
  }

  const isAdminMode = accessMode === 'admin'

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-20 relative">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div
          className="absolute top-1/4 -left-40 w-80 h-80 rounded-full opacity-10 blur-3xl"
          style={{ background: 'radial-gradient(circle, #00bfbf, transparent)' }}
        />
        <div
          className="absolute bottom-1/4 -right-40 w-80 h-80 rounded-full opacity-10 blur-3xl"
          style={{ background: 'radial-gradient(circle, #0d9488, transparent)' }}
        />
      </div>

      <div className="w-full max-w-md relative z-10 animate-fade-in">
        <div className="flex flex-col items-center mb-8">
          <div className="flex items-center justify-center w-14 h-14 rounded-2xl bg-teal-gradient shadow-teal mb-4">
            <MapPinIcon className="w-7 h-7 text-white" />
          </div>
          <h1 className="font-display text-3xl font-bold text-white">
            {isAdminMode ? 'Acceso Administrador' : 'Acceso Cliente'}
          </h1>
          <p className="text-gray-400 mt-2 text-center">
            {isAdminMode
              ? 'Gestiona paquetes, clientes, reservas y reportes.'
              : 'Reserva tours y administra tus viajes en Ayacucho.'}
          </p>
        </div>

        <div className="glass-card p-8">
          <div className="mb-6 grid grid-cols-1 gap-3 sm:grid-cols-2">
            <button
              type="button"
              onClick={() => selectAccessMode('cliente')}
              className={`rounded-2xl border p-4 text-left transition-all duration-200 ${
                !isAdminMode
                  ? 'border-primary-300 bg-primary-500/20 shadow-teal'
                  : 'border-white/10 bg-white/[0.03] hover:border-primary-400/60 hover:bg-primary-500/10'
              }`}
            >
              <UserIcon className={!isAdminMode ? 'mb-2 h-6 w-6 text-primary-200' : 'mb-2 h-6 w-6 text-gray-400'} />
              <p className="font-display text-sm font-bold text-white">Cliente</p>
              <p className="mt-1 text-xs text-gray-400">Reservas y perfil</p>
            </button>
            <button
              type="button"
              onClick={() => selectAccessMode('admin')}
              className={`rounded-2xl border p-4 text-left transition-all duration-200 ${
                isAdminMode
                  ? 'border-amber-300 bg-amber-400/20 shadow-[0_10px_32px_rgba(245,158,11,0.24)]'
                  : 'border-white/10 bg-white/[0.03] hover:border-amber-400/60 hover:bg-amber-400/10'
              }`}
            >
              <ShieldCheckIcon className={isAdminMode ? 'mb-2 h-6 w-6 text-amber-200' : 'mb-2 h-6 w-6 text-gray-400'} />
              <p className="font-display text-sm font-bold text-white">Administrador</p>
              <p className="mt-1 text-xs text-gray-400">Panel de gestion</p>
            </button>
          </div>

          {isAdminMode && (
            <div className="mb-5 rounded-2xl border border-amber-400/25 bg-amber-400/[0.08] p-4">
              <div className="flex items-start gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gold-gradient shadow-[0_8px_24px_rgba(245,158,11,0.28)]">
                  <ShieldCheckIcon className="h-5 w-5 text-white" />
                </div>
                <div className="flex-1">
                  <p className="font-display text-sm font-bold text-white">Cuenta administrativa seleccionada</p>
                  <p className="mt-1 text-xs text-amber-100/80">
                    El registro publico es solo para clientes. El administrador se crea en la base de datos.
                  </p>
                </div>
              </div>
            </div>
          )}

          <form id="login-form" onSubmit={handleSubmit} className="flex flex-col gap-5" noValidate>
            {serverError && (
              <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                <p>{serverError}</p>
              </div>
            )}

            {isBlocked && (
              <div className="rounded-xl border border-amber-500/30 bg-amber-500/10 p-4 text-sm text-amber-200 flex items-center gap-3">
                <ClockIcon className="w-5 h-5 shrink-0" />
                <p>Cuenta bloqueada temporalmente. Tiempo restante: {formatRemaining(remainingSeconds)}</p>
              </div>
            )}

            <div>
              <label htmlFor="correo" className="input-label">Correo Electronico</label>
              <div className="relative">
                <EnvelopeIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="correo"
                  name="correo"
                  type="email"
                  value={formData.correo}
                  onChange={handleChange}
                  placeholder={isAdminMode ? 'admin@toursayacuchoperu.com' : 'tu@correo.com'}
                  className={`input-field pl-10 ${errors.correo ? 'border-red-500/60' : ''}`}
                  autoComplete="email"
                />
              </div>
              {errors.correo && <p className="input-error">{errors.correo}</p>}
            </div>

            <div>
              <label htmlFor="password" className="input-label">Contrasena</label>
              <div className="relative">
                <LockClosedIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={handleChange}
                  placeholder={isAdminMode ? 'Clave de administrador' : 'Ingresa tu contrasena'}
                  className={`input-field pl-10 pr-10 ${errors.password ? 'border-red-500/60' : ''}`}
                  autoComplete="current-password"
                />
                <button
                  type="button"
                  id="toggle-password-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-primary-400 transition-colors"
                  aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
                >
                  {showPassword ? <EyeSlashIcon className="w-5 h-5" /> : <EyeIcon className="w-5 h-5" />}
                </button>
              </div>
              {errors.password && <p className="input-error">{errors.password}</p>}
            </div>

            <button
              id="login-submit-btn"
              type="submit"
              disabled={loading || isBlocked}
              className="btn-primary w-full mt-2"
            >
              {loading ? (
                <>
                  <div className="spinner w-4 h-4" />
                  Iniciando sesion...
                </>
              ) : (
                <>
                  {isAdminMode ? 'Entrar al Panel Admin' : 'Iniciar Sesion'}
                  {isAdminMode ? <ShieldCheckIcon className="w-4 h-4" /> : <ArrowRightIcon className="w-4 h-4" />}
                </>
              )}
            </button>
          </form>

          <div className={isAdminMode ? 'mt-6 hidden text-center' : 'mt-6 text-center'}>
            <p className="text-gray-400 text-sm">
              No tienes cuenta?{' '}
              <Link to="/register" className="text-primary-400 hover:text-primary-300 font-medium transition-colors">
                Registrate gratis
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Login
