import { useState, useEffect, useMemo } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  CalendarDaysIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
  ClockIcon,
  UsersIcon,
  ExclamationTriangleIcon,
  ArrowPathIcon,
  ExclamationCircleIcon,
} from '@heroicons/react/24/outline'

const toDateInput = (date) => date.toISOString().slice(0, 10)

const getTomorrow = () => {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  return toDateInput(tomorrow)
}

const Reschedule = () => {
  const { reservaId } = useParams()
  const navigate = useNavigate()
  const [reservation, setReservation] = useState(null)
  const [resLoading, setResLoading] = useState(true)
  const [loading, setLoading] = useState(false)
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [nuevaFecha, setNuevaFecha] = useState(getTomorrow())
  const [now, setNow] = useState(new Date())

  useEffect(() => {
    const timer = window.setInterval(() => setNow(new Date()), 60000)
    return () => window.clearInterval(timer)
  }, [])

  useEffect(() => {
    const fetchReservation = async () => {
      try {
        const res = await apiClient.get(`/reservations/${reservaId}`)
        if (res.data.estado !== 'CONFIRMADA') {
          toast.error('Solo puedes reprogramar reservas confirmadas')
          navigate('/mis-reservas')
          return
        }
        setReservation(res.data)
      } catch (err) {
        toast.error(getApiErrorMessage(err, 'No se pudo cargar la reserva'))
        navigate('/mis-reservas')
      } finally {
        setResLoading(false)
      }
    }

    fetchReservation()
  }, [reservaId, navigate])

  const originalDate = useMemo(
    () => (reservation?.fechaInicio ? new Date(reservation.fechaInicio) : null),
    [reservation?.fechaInicio]
  )

  const hoursUntilStart = originalDate
    ? (originalDate.getTime() - now.getTime()) / 36e5
    : null
  const canRescheduleByWindow = hoursUntilStart === null || hoursUntilStart >= 12
  const remainingHoursLabel = hoursUntilStart === null
    ? 'N/A'
    : `${Math.max(0, hoursUntilStart).toFixed(1)} horas`

  const validate = () => {
    const errs = {}

    if (!canRescheduleByWindow) {
      errs.general = 'La reserva ya esta fuera de la ventana de reprogramacion de 12 horas'
    }

    if (!nuevaFecha) {
      errs.fecha = 'La nueva fecha es requerida'
    } else {
      const selectedDate = new Date(`${nuevaFecha}T12:00:00`)
      if (selectedDate <= now) {
        errs.fecha = 'La nueva fecha debe ser futura'
      }

      if (originalDate && nuevaFecha === toDateInput(originalDate)) {
        errs.fecha = 'La nueva fecha debe ser diferente a la fecha actual'
      }
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
      await apiClient.patch(`/reservations/${reservaId}/reschedule`, {
        nuevaFecha: new Date(`${nuevaFecha}T12:00:00`).toISOString(),
      })
      toast.success('Reserva reprogramada exitosamente')
      navigate('/mis-reservas')
    } catch (err) {
      const message = getApiErrorMessage(err, 'Error al reprogramar')
      setServerError(message)
      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  const handleDateChange = (event) => {
    setNuevaFecha(event.target.value)
    if (errors.fecha || errors.general) setErrors({})
    if (serverError) setServerError('')
  }

  if (resLoading) {
    return (
      <div className="page-wrapper flex items-center justify-center">
        <div className="spinner w-12 h-12" />
      </div>
    )
  }

  const fechaActual = originalDate
    ? originalDate.toLocaleDateString('es-PE', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      })
    : 'No especificada'

  return (
    <div className="page-wrapper">
      <div className="container-main max-w-2xl animate-fade-in">
        <button id="back-from-reschedule-btn" onClick={() => navigate(-1)} className="btn-ghost mb-6 -ml-2">
          <ArrowLeftIcon className="w-4 h-4" />
          Volver
        </button>

        <div className="mb-8">
          <div className="teal-divider mb-4" />
          <h1 className="font-display text-3xl font-bold text-white">Reprogramar Reserva</h1>
          <p className="text-gray-400 mt-2">Solicita una nueva fecha para tu reserva confirmada.</p>
        </div>

        <div className="glass-card p-8">
          <div className={`flex items-start gap-3 p-4 rounded-xl border mb-6 ${
            canRescheduleByWindow
              ? 'bg-amber-500/10 border-amber-500/20'
              : 'bg-red-500/10 border-red-500/30'
          }`}>
            <ExclamationTriangleIcon className={`w-5 h-5 flex-shrink-0 mt-0.5 ${canRescheduleByWindow ? 'text-amber-400' : 'text-red-400'}`} />
            <div>
              <p className={`font-medium text-sm ${canRescheduleByWindow ? 'text-amber-400' : 'text-red-300'}`}>
                Politica de reprogramacion
              </p>
              <p className="text-gray-400 text-xs mt-1">
                Puedes reprogramar una reserva confirmada solo si faltan al menos 12 horas para el inicio del tour.
                Tiempo restante: {remainingHoursLabel}.
              </p>
            </div>
          </div>

          {reservation && (
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
              <div className="p-4 rounded-xl bg-white/[0.03] border border-white/10 sm:col-span-2">
                <p className="text-gray-500 text-xs mb-1 flex items-center gap-1">
                  <CalendarDaysIcon className="w-3.5 h-3.5" />
                  Fecha actual
                </p>
                <p className="text-white text-sm font-medium capitalize">{fechaActual}</p>
              </div>
              <div className="p-4 rounded-xl bg-white/[0.03] border border-white/10">
                <p className="text-gray-500 text-xs mb-1 flex items-center gap-1">
                  <UsersIcon className="w-3.5 h-3.5" />
                  Asientos
                </p>
                <p className="text-white text-sm font-medium">{reservation.cantAsientos}</p>
              </div>
            </div>
          )}

          <form id="reschedule-form" onSubmit={handleSubmit} className="flex flex-col gap-6" noValidate>
            {(serverError || errors.general) && (
              <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                <p>{serverError || errors.general}</p>
              </div>
            )}

            <div>
              <label htmlFor="nuevaFecha" className="input-label">Nueva fecha de inicio</label>
              <div className="relative">
                <CalendarDaysIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-500" />
                <input
                  id="nuevaFecha"
                  name="nuevaFecha"
                  type="date"
                  min={getTomorrow()}
                  value={nuevaFecha}
                  onChange={handleDateChange}
                  disabled={!canRescheduleByWindow}
                  className={`input-field pl-10 ${errors.fecha ? 'border-red-500/60' : ''}`}
                  style={{ colorScheme: 'dark' }}
                />
              </div>
              {errors.fecha && <p className="input-error">{errors.fecha}</p>}
              <p className="text-gray-500 text-xs mt-1">La nueva fecha debe ser futura y distinta a la fecha actual.</p>
            </div>

            {nuevaFecha && (
              <div className="p-4 rounded-xl bg-primary-500/10 border border-primary-500/20">
                <p className="text-primary-400 font-medium text-sm flex items-center gap-2">
                  <ArrowPathIcon className="w-4 h-4" />
                  Nueva fecha seleccionada
                </p>
                <p className="text-white text-sm mt-1 capitalize">
                  {new Date(`${nuevaFecha}T12:00:00`).toLocaleDateString('es-PE', {
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </p>
              </div>
            )}

            <div className="flex flex-col sm:flex-row gap-3 pt-2">
              <button
                id="confirm-reschedule-btn"
                type="submit"
                disabled={loading || !canRescheduleByWindow}
                className="btn-primary flex-1"
              >
                {loading ? (
                  <>
                    <div className="spinner w-4 h-4" />
                    Reprogramando...
                  </>
                ) : (
                  <>
                    <CheckCircleIcon className="w-5 h-5" />
                    Confirmar Reprogramacion
                  </>
                )}
              </button>
              <button type="button" onClick={() => navigate(-1)} className="btn-secondary flex-1">
                Cancelar
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}

export default Reschedule
