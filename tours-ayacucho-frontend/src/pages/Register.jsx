import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  UserIcon,
  EnvelopeIcon,
  LockClosedIcon,
  PhoneIcon,
  EyeIcon,
  EyeSlashIcon,
  MapPinIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
  ShieldCheckIcon,
} from '@heroicons/react/24/outline'

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const getPasswordCriteria = (password) => [
  {
    key: 'length',
    label: 'Minimo 8 caracteres',
    valid: password.length >= 8,
  },
  {
    key: 'uppercase',
    label: 'Al menos una letra mayuscula',
    valid: /[A-Z]/.test(password),
  },
  {
    key: 'digit',
    label: 'Al menos un digito',
    valid: /\d/.test(password),
  },
  {
    key: 'special',
    label: 'Al menos un caracter especial',
    valid: /[^A-Za-z0-9]/.test(password),
  },
]

const Register = () => {
  const { register, login } = useAuth()
  const navigate = useNavigate()

  const [formData, setFormData] = useState({
    nombre: '',
    correo: '',
    password: '',
    confirmPassword: '',
    telefono: '',
  })
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [loading, setLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)

  const passwordCriteria = getPasswordCriteria(formData.password)
  const isPasswordComplete = passwordCriteria.every((criterion) => criterion.valid)
  const passwordScore = passwordCriteria.filter((criterion) => criterion.valid).length
  const strengthLabels = ['Muy debil', 'Debil', 'Regular', 'Fuerte']
  const strengthColors = ['bg-red-500', 'bg-red-500', 'bg-amber-500', 'bg-emerald-500']

  const validate = () => {
    const errs = {}

    if (!formData.nombre.trim()) {
      errs.nombre = 'El nombre completo es requerido'
    } else if (formData.nombre.trim().length > 150) {
      errs.nombre = 'El nombre no puede exceder 150 caracteres'
    }

    if (!formData.correo.trim()) {
      errs.correo = 'El correo electronico es requerido'
    } else if (!emailRegex.test(formData.correo.trim())) {
      errs.correo = 'El correo electronico no tiene un formato valido'
    }

    if (!formData.password) {
      errs.password = 'La contrasena es requerida'
    } else if (!isPasswordComplete) {
      errs.password = 'La contrasena no cumple todos los criterios de seguridad'
    }

    if (!formData.confirmPassword) {
      errs.confirmPassword = 'Confirma tu contrasena'
    } else if (formData.password !== formData.confirmPassword) {
      errs.confirmPassword = 'Las contrasenas no coinciden'
    }

    if (!formData.telefono.trim()) {
      errs.telefono = 'El telefono es requerido'
    } else if (!/^\d{9,15}$/.test(formData.telefono.trim())) {
      errs.telefono = 'El telefono debe tener solo digitos, entre 9 y 15'
    }

    return errs
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
      const correo = formData.correo.trim().toLowerCase()
      const password = formData.password

      await register(
        formData.nombre.trim(),
        correo,
        password,
        formData.telefono.trim()
      )
      await login(correo, password)
      toast.success('Cuenta creada. Bienvenido a tu panel de cliente.')
      navigate('/mis-reservas', { replace: true })
    } catch (err) {
      const message = getApiErrorMessage(err, 'Error al registrar o iniciar sesion. Intenta de nuevo.')
      setServerError(message)
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

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-20 relative">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div
          className="absolute top-1/3 -left-40 w-80 h-80 rounded-full opacity-10 blur-3xl"
          style={{ background: 'radial-gradient(circle, #00bfbf, transparent)' }}
        />
        <div
          className="absolute bottom-1/3 -right-40 w-96 h-96 rounded-full opacity-8 blur-3xl"
          style={{ background: 'radial-gradient(circle, #f59e0b, transparent)' }}
        />
      </div>

      <div className="w-full max-w-lg relative z-10 animate-fade-in">
        <div className="flex flex-col items-center mb-8">
          <div className="flex items-center justify-center w-14 h-14 rounded-2xl bg-teal-gradient shadow-teal mb-4">
            <MapPinIcon className="w-7 h-7 text-white" />
          </div>
          <h1 className="font-display text-3xl font-bold text-white">Crear Cuenta</h1>
          <p className="text-gray-400 mt-2 text-center">
            Unete a <span className="text-primary-400 font-medium">Tours Ayacucho Peru</span>
          </p>
          <div className="mt-4 flex max-w-md items-center gap-3 rounded-2xl border border-primary-400/25 bg-primary-500/[0.08] p-4 text-left">
            <ShieldCheckIcon className="h-6 w-6 shrink-0 text-primary-300" />
            <p className="text-xs leading-relaxed text-gray-300">
              Este registro es para clientes. Las cuentas de administrador se crean internamente y se ingresan desde Iniciar Sesion.
            </p>
          </div>
        </div>

        <div className="glass-card p-8">
          <form id="register-form" onSubmit={handleSubmit} className="flex flex-col gap-5" noValidate>
            {serverError && (
              <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                <p>{serverError}</p>
              </div>
            )}

            <div>
              <label htmlFor="nombre" className="input-label">Nombre Completo</label>
              <div className="relative">
                <UserIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="nombre"
                  name="nombre"
                  type="text"
                  value={formData.nombre}
                  onChange={handleChange}
                  placeholder="Juan Perez"
                  className={`input-field pl-10 ${errors.nombre ? 'border-red-500/60' : ''}`}
                  autoComplete="name"
                />
              </div>
              {errors.nombre && <p className="input-error">{errors.nombre}</p>}
            </div>

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
                  placeholder="tu@correo.com"
                  className={`input-field pl-10 ${errors.correo ? 'border-red-500/60' : ''}`}
                  autoComplete="email"
                />
              </div>
              {errors.correo && <p className="input-error">{errors.correo}</p>}
            </div>

            <div>
              <label htmlFor="telefono" className="input-label">Telefono</label>
              <div className="relative">
                <PhoneIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="telefono"
                  name="telefono"
                  type="tel"
                  inputMode="numeric"
                  value={formData.telefono}
                  onChange={handleChange}
                  placeholder="987654321"
                  className={`input-field pl-10 ${errors.telefono ? 'border-red-500/60' : ''}`}
                  autoComplete="tel"
                />
              </div>
              {errors.telefono && <p className="input-error">{errors.telefono}</p>}
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
                  placeholder="Minimo 8 caracteres"
                  className={`input-field pl-10 pr-10 ${errors.password ? 'border-red-500/60' : ''}`}
                  autoComplete="new-password"
                />
                <button
                  type="button"
                  id="toggle-password-register-btn"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-primary-400 transition-colors"
                  aria-label={showPassword ? 'Ocultar contrasena' : 'Mostrar contrasena'}
                >
                  {showPassword ? <EyeSlashIcon className="w-5 h-5" /> : <EyeIcon className="w-5 h-5" />}
                </button>
              </div>

              {formData.password && (
                <div className="mt-3">
                  <div className="flex gap-1 mb-2">
                    {[0, 1, 2, 3].map((i) => (
                      <div
                        key={i}
                        className={`h-1 flex-1 rounded-full transition-all duration-300 ${
                          i < passwordScore ? strengthColors[Math.max(passwordScore - 1, 0)] : 'bg-white/10'
                        }`}
                      />
                    ))}
                  </div>
                  <p className="text-xs text-gray-400 mb-3">
                    Seguridad:{' '}
                    <span className={isPasswordComplete ? 'text-emerald-400 font-medium' : 'text-amber-400 font-medium'}>
                      {strengthLabels[Math.max(passwordScore - 1, 0)]}
                    </span>
                  </p>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    {passwordCriteria.map((criterion) => (
                      <div
                        key={criterion.key}
                        className={`flex items-center gap-2 text-xs ${criterion.valid ? 'text-emerald-400' : 'text-gray-500'}`}
                      >
                        <CheckCircleIcon className="w-3.5 h-3.5" />
                        <span>{criterion.label}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
              {errors.password && <p className="input-error">{errors.password}</p>}
            </div>

            <div>
              <label htmlFor="confirmPassword" className="input-label">Confirmar Contrasena</label>
              <div className="relative">
                <LockClosedIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type={showConfirm ? 'text' : 'password'}
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  placeholder="Repite tu contrasena"
                  className={`input-field pl-10 pr-10 ${errors.confirmPassword ? 'border-red-500/60' : ''}`}
                  autoComplete="new-password"
                />
                <button
                  type="button"
                  id="toggle-confirm-btn"
                  onClick={() => setShowConfirm(!showConfirm)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-primary-400 transition-colors"
                  aria-label={showConfirm ? 'Ocultar confirmacion' : 'Mostrar confirmacion'}
                >
                  {showConfirm ? <EyeSlashIcon className="w-5 h-5" /> : <EyeIcon className="w-5 h-5" />}
                </button>
              </div>
              {formData.confirmPassword && formData.password === formData.confirmPassword && (
                <p className="text-emerald-400 text-xs mt-1 flex items-center gap-1">
                  <CheckCircleIcon className="w-3 h-3" /> Contrasenas coinciden
                </p>
              )}
              {errors.confirmPassword && <p className="input-error">{errors.confirmPassword}</p>}
            </div>

            <button
              id="register-submit-btn"
              type="submit"
              disabled={loading || (formData.password && !isPasswordComplete)}
              className="btn-primary w-full mt-2"
            >
              {loading ? (
                <>
                  <div className="spinner w-4 h-4" />
                  Creando tu cuenta...
                </>
              ) : (
                <>
                  Crear Cuenta
                  <CheckCircleIcon className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          <div className="mt-6 text-center">
            <p className="text-gray-400 text-sm">
              Ya tienes cuenta?{' '}
              <Link to="/login" className="text-primary-400 hover:text-primary-300 font-medium transition-colors">
                Inicia sesion aqui
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Register
