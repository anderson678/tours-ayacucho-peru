import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  MapPinIcon,
  CalendarDaysIcon,
  UsersIcon,
  CurrencyDollarIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationCircleIcon,
} from '@heroicons/react/24/outline'

const toDateInput = (value) => {
  if (!value) return ''
  return new Date(value).toISOString().slice(0, 10)
}

const CreateReservation = () => {
  const { packageId } = useParams()
  const navigate = useNavigate()
  const [pkg, setPkg] = useState(null)
  const [pkgLoading, setPkgLoading] = useState(true)
  const [loading, setLoading] = useState(false)
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [formData, setFormData] = useState({
    cantAsientos: 1,
    fechaInicio: '',
  })

  useEffect(() => {
    const fetchPkg = async () => {
      try {
        const res = await apiClient.get(`/packages/${packageId}`)
        setPkg(res.data)
        setFormData((prev) => ({
          ...prev,
          fechaInicio: toDateInput(res.data?.fechaInicio),
        }))
      } catch (err) {
        toast.error(getApiErrorMessage(err, 'No se pudo cargar el paquete'))
        navigate('/')
      } finally {
        setPkgLoading(false)
      }
    }

    fetchPkg()
  }, [packageId, navigate])

  const validate = () => {
    const errs = {}
    const seats = Number(formData.cantAsientos)
    const availableSeats = Number(pkg?.asientosDisp ?? 0)

    if (!pkg?.activo && pkg?.activo !== undefined) {
      errs.general = 'Este paquete no se encuentra activo'
    }

    if (availableSeats <= 0) {
      errs.cantAsientos = 'No hay asientos disponibles para este paquete'
    } else if (!Number.isInteger(seats) || seats < 1) {
      errs.cantAsientos = 'La cantidad de asientos debe ser al menos 1'
    } else if (seats > availableSeats) {
      errs.cantAsientos = `Solo quedan ${availableSeats} asientos disponibles`
    }

    if (!formData.fechaInicio) {
      errs.fechaInicio = 'La fecha de inicio del tour es requerida'
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
      const payload = {
        paqueteId: packageId,
        cantAsientos: Number(formData.cantAsientos),
        fechaInicio: formData.fechaInicio,
      }
      const response = await apiClient.post('/reservations', payload)
      toast.success('Reserva creada con estado PENDIENTE_PAGO')
      navigate('/mis-reservas', {
        state: { createdReservationId: response.data?.reservaId },
      })
    } catch (err) {
      const message = getApiErrorMessage(err, 'Error al crear la reserva')
      setServerError(message)
      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  const handleSeatsChange = (event) => {
    setFormData((prev) => ({ ...prev, cantAsientos: event.target.value }))
    if (errors.cantAsientos || errors.general) {
      setErrors((prev) => ({ ...prev, cantAsientos: '', general: '' }))
    }
    if (serverError) setServerError('')
  }

  const precio = Number(pkg?.precioUnitario ?? 0)
  const seats = Number(formData.cantAsientos || 1)
  const total = precio * seats
  const fechaInicioPaquete = pkg?.fechaInicio ? new Date(pkg.fechaInicio) : null
  const fechaFinPaquete = pkg?.fechaFin ? new Date(pkg.fechaFin) : null
  const duracion = fechaInicioPaquete && fechaFinPaquete
    ? Math.max(1, Math.round((fechaFinPaquete - fechaInicioPaquete) / 86400000) + 1)
    : 1
  const availableSeats = Number(pkg?.asientosDisp ?? 0)
  const canReserve = availableSeats > 0 && pkg?.activo !== false

  if (pkgLoading) {
    return (
      <div className="page-wrapper flex items-center justify-center">
        <div className="spinner w-12 h-12" />
      </div>
    )
  }

  return (
    <div className="page-wrapper">
      <div className="container-main max-w-4xl animate-fade-in">
        <button id="back-from-reservation-btn" onClick={() => navigate(-1)} className="btn-ghost mb-6 -ml-2">
          <ArrowLeftIcon className="w-4 h-4" />
          Volver
        </button>

        <div className="mb-8">
          <div className="teal-divider mb-4" />
          <h1 className="font-display text-3xl font-bold text-white">Crear Reserva</h1>
          <p className="text-gray-400 mt-2">Selecciona la cantidad de asientos para confirmar tu reserva.</p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-5 gap-8">
          <div className="lg:col-span-3">
            <div className="glass-card p-8">
              <form id="create-reservation-form" onSubmit={handleSubmit} className="flex flex-col gap-6" noValidate>
                {(serverError || errors.general) && (
                  <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                    <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                    <p>{serverError || errors.general}</p>
                  </div>
                )}

                {pkg && (
                  <div className="p-4 rounded-xl bg-primary-500/5 border border-primary-500/20">
                    <div className="flex items-center gap-2 mb-1">
                      <MapPinIcon className="w-4 h-4 text-primary-400" />
                      <span className="text-primary-400 text-xs font-medium">PAQUETE SELECCIONADO</span>
                    </div>
                    <p className="text-white font-semibold">{pkg.nombre}</p>
                    <p className="text-gray-400 text-sm">{pkg.destino ?? 'Ayacucho, Peru'}</p>
                  </div>
                )}

                <div>
                  <label htmlFor="fechaInicio" className="input-label">Fecha de inicio del tour</label>
                  <div className="relative">
                    <CalendarDaysIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                    <input
                      id="fechaInicio"
                      name="fechaInicio"
                      type="date"
                      value={formData.fechaInicio}
                      readOnly
                      className={`input-field pl-10 opacity-75 cursor-not-allowed ${errors.fechaInicio ? 'border-red-500/60' : ''}`}
                      style={{ colorScheme: 'dark' }}
                    />
                  </div>
                  <p className="text-gray-500 text-xs mt-1">La fecha corresponde a la salida publicada del paquete.</p>
                  {errors.fechaInicio && <p className="input-error">{errors.fechaInicio}</p>}
                </div>

                <div>
                  <label htmlFor="cantAsientos" className="input-label">Numero de asientos</label>
                  <div className="relative">
                    <UsersIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                    <input
                      id="cantAsientos"
                      name="cantAsientos"
                      type="number"
                      min={1}
                      max={availableSeats || 1}
                      value={formData.cantAsientos}
                      onChange={handleSeatsChange}
                      disabled={!canReserve}
                      className={`input-field pl-10 ${errors.cantAsientos ? 'border-red-500/60' : ''}`}
                    />
                  </div>
                  {errors.cantAsientos && <p className="input-error">{errors.cantAsientos}</p>}
                  <p className="text-gray-500 text-xs mt-1">
                    Disponibilidad actual: {availableSeats} asiento{availableSeats !== 1 ? 's' : ''}
                  </p>
                </div>

                <button
                  id="confirm-reservation-btn"
                  type="submit"
                  disabled={loading || !canReserve}
                  className="btn-primary w-full mt-2"
                >
                  {loading ? (
                    <>
                      <div className="spinner w-4 h-4" />
                      Creando reserva...
                    </>
                  ) : (
                    <>
                      <CheckCircleIcon className="w-5 h-5" />
                      Confirmar Reserva
                    </>
                  )}
                </button>

                {!canReserve && (
                  <p className="text-center text-amber-400 text-sm">
                    Este paquete no tiene disponibilidad para nuevas reservas.
                  </p>
                )}
              </form>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className="glass-card p-6 sticky top-24">
              <h2 className="font-display font-bold text-white text-lg mb-5 flex items-center gap-2">
                <CurrencyDollarIcon className="w-5 h-5 text-primary-400" />
                Resumen
              </h2>

              <div className="flex flex-col gap-3 mb-5">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-400">Precio por persona</span>
                  <span className="text-white font-medium">S/ {precio.toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-400">Asientos solicitados</span>
                  <span className="text-white font-medium">x {seats || 0}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-400 flex items-center gap-1">
                    <ClockIcon className="w-3.5 h-3.5" />
                    Duracion
                  </span>
                  <span className="text-white font-medium">{duracion} dia{duracion !== 1 ? 's' : ''}</span>
                </div>
                <div className="border-t border-white/10 pt-3 flex justify-between">
                  <span className="text-white font-bold text-lg">Total</span>
                  <span className="font-display text-2xl font-bold gradient-text">S/ {total.toFixed(2)}</span>
                </div>
              </div>

              <div className="flex flex-col gap-2 pt-4 border-t border-white/10">
                {[
                  'La reserva se crea con estado PENDIENTE_PAGO',
                  'Los asientos se descuentan al confirmar',
                  'La disponibilidad se valida en tiempo real',
                ].map((txt) => (
                  <div key={txt} className="flex items-center gap-2 text-gray-400 text-xs">
                    <CheckCircleIcon className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0" />
                    {txt}
                  </div>
                ))}
              </div>

              <Link to={`/package/${packageId}`} className="btn-secondary w-full mt-5 text-sm">
                Ver detalle del paquete
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default CreateReservation
