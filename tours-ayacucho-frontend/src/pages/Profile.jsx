import { useEffect, useState } from 'react'
import { useAuth } from '../context/AuthContext'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  UserCircleIcon,
  PhoneIcon,
  EnvelopeIcon,
  PencilSquareIcon,
  CheckCircleIcon,
  ShieldCheckIcon,
  CalendarDaysIcon,
  KeyIcon,
  ExclamationCircleIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline'

const Profile = () => {
  const { user, updateUser } = useAuth()
  const [editing, setEditing] = useState(false)
  const [loading, setLoading] = useState(false)
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [formData, setFormData] = useState({
    nombre: user?.nombre ?? '',
    telefono: user?.telefono ?? '',
    fotoUrl: user?.fotoUrl ?? '',
  })

  useEffect(() => {
    const loadProfile = async () => {
      if (!user?.id) return

      try {
        const res = await apiClient.get(`/clients/${user.id}/profile`)
        updateUser({
          id: res.data.clienteId ?? user.id,
          nombre: res.data.nombre,
          correo: res.data.correo,
          telefono: res.data.telefono,
          fotoUrl: res.data.fotoUrl,
          rol: res.data.rol ?? user.rol,
        })
      } catch {
        // El perfil local se mantiene si la API no responde.
      }
    }

    loadProfile()
  }, [user?.id])

  useEffect(() => {
    if (!editing) {
      setFormData({
        nombre: user?.nombre ?? '',
        telefono: user?.telefono ?? '',
        fotoUrl: user?.fotoUrl ?? '',
      })
    }
  }, [editing, user?.nombre, user?.telefono, user?.fotoUrl])

  const validate = () => {
    const errs = {}
    const nombre = formData.nombre.trim()
    const telefono = formData.telefono.trim()
    const fotoUrl = formData.fotoUrl.trim()

    if (!nombre && !telefono && !fotoUrl) {
      errs.general = 'Debes ingresar al menos un dato para actualizar'
    }

    if (nombre.length > 150) {
      errs.nombre = 'El nombre no puede exceder 150 caracteres'
    }

    if (telefono && !/^\d{9,15}$/.test(telefono)) {
      errs.telefono = 'El telefono debe contener solo digitos, entre 9 y 15'
    }

    if (fotoUrl && !/^https?:\/\/.+/i.test(fotoUrl)) {
      errs.fotoUrl = 'La foto debe ser una URL valida que empiece con http o https'
    }

    return errs
  }

  const buildPayload = () => {
    const payload = {}
    const nombre = formData.nombre.trim()
    const telefono = formData.telefono.trim()
    const fotoUrl = formData.fotoUrl.trim()

    if (nombre) payload.nombre = nombre
    if (telefono) payload.telefono = telefono
    payload.fotoUrl = fotoUrl

    return payload
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
      const res = await apiClient.put(`/clients/${user.id}/profile`, buildPayload())
      updateUser({
        id: res.data.clienteId ?? user.id,
        nombre: res.data.nombre,
        correo: res.data.correo,
        telefono: res.data.telefono,
        fotoUrl: res.data.fotoUrl,
        rol: res.data.rol ?? user.rol,
      })
      toast.success('Perfil actualizado correctamente')
      setEditing(false)
    } catch (err) {
      const message = getApiErrorMessage(err, 'Error al actualizar perfil')
      setServerError(message)
      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    setFormData({ nombre: user?.nombre ?? '', telefono: user?.telefono ?? '', fotoUrl: user?.fotoUrl ?? '' })
    setErrors({})
    setServerError('')
    setEditing(false)
  }

  const handleChange = (event) => {
    const { name, value } = event.target
    setFormData((prev) => ({ ...prev, [name]: value }))
    if (errors[name] || errors.general) {
      setErrors((prev) => ({ ...prev, [name]: '', general: '' }))
    }
    if (serverError) setServerError('')
  }

  const initial = (user?.nombre ?? user?.correo ?? 'U').charAt(0).toUpperCase()
  const correo = user?.correo ?? 'usuario@email.com'
  const expiration = user?.expiraEn ? new Date(user.expiraEn) : null
  const avatarUrl = editing ? formData.fotoUrl.trim() : user?.fotoUrl

  return (
    <div className="page-wrapper">
      <div className="container-main max-w-4xl animate-fade-in">
        <div className="mb-8">
          <div className="teal-divider mb-4" />
          <h1 className="font-display text-3xl font-bold text-white">Mi Perfil</h1>
          <p className="text-gray-400 mt-2">Gestiona tu informacion personal</p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="flex flex-col gap-4">
            <div className="glass-card p-8 flex flex-col items-center text-center gap-4">
              <div className="relative">
                <div className="flex h-24 w-24 items-center justify-center overflow-hidden rounded-full bg-teal-gradient font-display text-3xl font-bold text-white shadow-teal-lg">
                  {avatarUrl ? (
                    <img src={avatarUrl} alt={user?.nombre ?? correo} className="h-full w-full object-cover" />
                  ) : (
                    initial
                  )}
                </div>
                <div className="absolute bottom-0 right-0 w-6 h-6 rounded-full bg-emerald-500 border-2 border-dark-900 flex items-center justify-center">
                  <CheckCircleIcon className="w-3.5 h-3.5 text-white" />
                </div>
              </div>

              <div>
                <h2 className="font-display font-bold text-white text-xl">
                  {user?.nombre ?? correo.split('@')[0]}
                </h2>
                <p className="text-gray-400 text-sm">{correo}</p>
              </div>

              <div className="flex items-center gap-2 px-3 py-1.5 rounded-full border border-primary-500/30 bg-primary-500/10">
                <ShieldCheckIcon className="w-3.5 h-3.5 text-primary-400" />
                <span className="text-primary-400 text-xs font-medium">{user?.rol ?? 'Cliente'}</span>
              </div>
            </div>

            <div className="glass-card p-5">
              <h3 className="font-display font-semibold text-white text-sm mb-4 uppercase tracking-wider">
                Informacion de cuenta
              </h3>
              <div className="flex flex-col gap-3">
                <div className="flex items-center gap-3 text-sm">
                  <EnvelopeIcon className="w-4 h-4 text-primary-400 flex-shrink-0" />
                  <div className="min-w-0">
                    <p className="text-gray-500 text-xs">Correo</p>
                    <p className="text-gray-300 break-all">{correo}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <CalendarDaysIcon className="w-4 h-4 text-primary-400 flex-shrink-0" />
                  <div>
                    <p className="text-gray-500 text-xs">Sesion expira</p>
                    <p className="text-gray-300 text-xs">
                      {expiration ? expiration.toLocaleString('es-PE') : 'N/A'}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <KeyIcon className="w-4 h-4 text-primary-400 flex-shrink-0" />
                  <div>
                    <p className="text-gray-500 text-xs">ID de cuenta</p>
                    <p className="text-gray-300 font-mono text-xs">{user?.id ? `${user.id.substring(0, 12)}...` : 'N/A'}</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 text-sm">
                  <UserCircleIcon className="w-4 h-4 text-primary-400 flex-shrink-0" />
                  <div className="min-w-0">
                    <p className="text-gray-500 text-xs">Foto de perfil</p>
                    <p className="text-gray-300 break-all">{user?.fotoUrl ? 'Configurada' : 'Sin foto personalizada'}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className="glass-card p-8">
              <div className="flex items-center justify-between gap-4 mb-6">
                <h2 className="font-display font-bold text-white text-xl">Informacion personal</h2>
                {!editing && (
                  <button id="edit-profile-btn" onClick={() => setEditing(true)} className="btn-secondary text-sm">
                    <PencilSquareIcon className="w-4 h-4" />
                    Editar
                  </button>
                )}
              </div>

              {editing ? (
                <form id="update-profile-form" onSubmit={handleSubmit} className="flex flex-col gap-5" noValidate>
                  {(serverError || errors.general) && (
                    <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                      <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                      <p>{serverError || errors.general}</p>
                    </div>
                  )}

                  <div>
                    <label htmlFor="profile-nombre" className="input-label">Nombre completo</label>
                    <div className="relative">
                      <UserCircleIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                      <input
                        id="profile-nombre"
                        name="nombre"
                        type="text"
                        value={formData.nombre}
                        onChange={handleChange}
                        className={`input-field pl-10 ${errors.nombre ? 'border-red-500/60' : ''}`}
                        placeholder="Nombre del cliente"
                      />
                    </div>
                    {errors.nombre && <p className="input-error">{errors.nombre}</p>}
                  </div>

                  <div>
                    <label htmlFor="profile-telefono" className="input-label">Telefono</label>
                    <div className="relative">
                      <PhoneIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                      <input
                        id="profile-telefono"
                        name="telefono"
                        type="tel"
                        inputMode="numeric"
                        value={formData.telefono}
                        onChange={handleChange}
                        className={`input-field pl-10 ${errors.telefono ? 'border-red-500/60' : ''}`}
                        placeholder="987654321"
                      />
                    </div>
                    {errors.telefono && <p className="input-error">{errors.telefono}</p>}
                  </div>

                  <div>
                    <label htmlFor="profile-fotoUrl" className="input-label">Foto de perfil URL</label>
                    <div className="relative">
                      <UserCircleIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                      <input
                        id="profile-fotoUrl"
                        name="fotoUrl"
                        type="url"
                        value={formData.fotoUrl}
                        onChange={handleChange}
                        className={`input-field pl-10 ${errors.fotoUrl ? 'border-red-500/60' : ''}`}
                        placeholder="https://..."
                      />
                    </div>
                    {errors.fotoUrl && <p className="input-error">{errors.fotoUrl}</p>}
                    <p className="text-gray-500 text-xs mt-1">Puedes usar la URL de una imagen cuadrada para una mejor presentacion.</p>
                  </div>

                  <div>
                    <label htmlFor="profile-correo" className="input-label">Correo electronico</label>
                    <div className="relative">
                      <EnvelopeIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                      <input
                        id="profile-correo"
                        type="email"
                        value={correo}
                        disabled
                        className="input-field pl-10 opacity-50 cursor-not-allowed"
                      />
                    </div>
                    <p className="text-gray-500 text-xs mt-1">El correo no puede ser modificado desde este formulario.</p>
                  </div>

                  <div className="flex flex-col sm:flex-row gap-3 pt-2">
                    <button id="save-profile-btn" type="submit" disabled={loading} className="btn-primary flex-1">
                      {loading ? (
                        <>
                          <div className="spinner w-4 h-4" />
                          Guardando...
                        </>
                      ) : (
                        <>
                          <CheckCircleIcon className="w-5 h-5" />
                          Guardar cambios
                        </>
                      )}
                    </button>
                    <button type="button" id="cancel-edit-btn" onClick={handleCancel} className="btn-secondary flex-1">
                      <XMarkIcon className="w-4 h-4" />
                      Cancelar
                    </button>
                  </div>
                </form>
              ) : (
                <div className="flex flex-col gap-5">
                  {[
                    { label: 'Nombre completo', value: user?.nombre ?? 'No especificado', icon: UserCircleIcon },
                    { label: 'Correo electronico', value: correo, icon: EnvelopeIcon },
                    { label: 'Telefono', value: user?.telefono ?? 'No especificado', icon: PhoneIcon },
                    { label: 'Foto de perfil', value: user?.fotoUrl ?? 'No especificado', icon: UserCircleIcon },
                  ].map(({ label, value, icon: Icon }) => (
                    <div key={label} className="p-4 rounded-xl bg-white/[0.03] border border-white/10">
                      <div className="flex items-center gap-2 mb-1">
                        <Icon className="w-4 h-4 text-primary-400" />
                        <span className="text-gray-500 text-xs font-medium uppercase tracking-wider">{label}</span>
                      </div>
                      <p className="text-white font-medium pl-6 break-words">{value}</p>
                    </div>
                  ))}

                  <div className="mt-2 p-4 rounded-xl bg-emerald-500/5 border border-emerald-500/20 flex items-center gap-3">
                    <ShieldCheckIcon className="w-5 h-5 text-emerald-400 flex-shrink-0" />
                    <div>
                      <p className="text-emerald-400 text-sm font-medium">Cuenta protegida</p>
                      <p className="text-gray-400 text-xs">El correo se conserva como identificador principal de la cuenta.</p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Profile
