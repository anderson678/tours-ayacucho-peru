import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  CalendarDaysIcon,
  ClockIcon,
  UsersIcon,
  CurrencyDollarIcon,
  MapPinIcon,
  ArrowRightIcon,
  ExclamationCircleIcon,
  CheckCircleIcon,
  XCircleIcon,
  ArrowPathIcon,
  ClockIcon as PendingIcon,
  ChatBubbleLeftRightIcon,
  PaperAirplaneIcon,
  StarIcon,
} from '@heroicons/react/24/outline'
import { StarIcon as StarSolidIcon } from '@heroicons/react/24/solid'

const STATUS_FILTERS = [
  { key: 'ALL', label: 'Todas' },
  { key: 'PENDIENTE_PAGO', label: 'Pendiente pago' },
  { key: 'CONFIRMADA', label: 'Confirmada' },
  { key: 'REPROGRAMADA', label: 'Reprogramada' },
  { key: 'COMPLETADA', label: 'Completada' },
  { key: 'CANCELADA', label: 'Cancelada' },
]

const statusBadge = (estado) => {
  const map = {
    PENDIENTE_PAGO: { cls: 'badge-warning', label: 'Pendiente pago', icon: PendingIcon },
    CONFIRMADA: { cls: 'badge-success', label: 'Confirmada', icon: CheckCircleIcon },
    REPROGRAMADA: { cls: 'badge-info', label: 'Reprogramada', icon: CalendarDaysIcon },
    COMPLETADA: { cls: 'badge-success', label: 'Completada', icon: CheckCircleIcon },
    CANCELADA: { cls: 'badge-danger', label: 'Cancelada', icon: XCircleIcon },
  }

  return map[estado] ?? { cls: 'badge-gray', label: estado ?? 'Sin estado', icon: ExclamationCircleIcon }
}

const formatDate = (value) => {
  if (!value) return 'No especificada'
  return new Date(value).toLocaleDateString('es-PE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

const ReviewForm = ({ reservation, packageLabel, onCreated, onCancel }) => {
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (event) => {
    event.preventDefault()
    const trimmedComment = comment.trim()

    if (rating < 1 || rating > 5) {
      setError('Selecciona una calificacion entre 1 y 5.')
      return
    }

    if (trimmedComment.length > 1000) {
      setError('El comentario no puede exceder los 1000 caracteres.')
      return
    }

    setSubmitting(true)
    setError('')

    try {
      await apiClient.post(`/packages/${reservation.paqueteId}/reviews`, {
        calificacion: rating,
        comentario: trimmedComment || null,
      })
      toast.success('Resena publicada correctamente')
      onCreated(reservation.paqueteId)
    } catch (err) {
      const message = getApiErrorMessage(err, 'No se pudo publicar la resena.')
      setError(message)
      toast.error(message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form id={`review-form-${reservation.reservaId}`} onSubmit={handleSubmit} className="mt-4 rounded-xl border border-primary-500/20 bg-primary-500/[0.04] p-4" noValidate>
      <div className="flex flex-col gap-4">
        <div>
          <label className="input-label">Calificacion para {packageLabel}</label>
          <div className="flex items-center gap-1" role="radiogroup" aria-label="Calificacion">
            {[1, 2, 3, 4, 5].map((value) => {
              const active = value <= rating
              const Icon = active ? StarSolidIcon : StarIcon
              return (
                <button
                  key={value}
                  id={`review-rating-${reservation.reservaId}-${value}`}
                  type="button"
                  role="radio"
                  aria-checked={rating === value}
                  title={`${value} estrella${value > 1 ? 's' : ''}`}
                  onClick={() => setRating(value)}
                  className="rounded-lg p-1 text-gold-400 transition hover:bg-gold-500/10 focus:outline-none focus:ring-2 focus:ring-gold-500/40"
                >
                  <Icon className="h-7 w-7" />
                </button>
              )
            })}
            <span className="ml-2 text-sm font-medium text-white">{rating}/5</span>
          </div>
        </div>

        <div>
          <label htmlFor={`review-comment-${reservation.reservaId}`} className="input-label">Comentario</label>
          <textarea
            id={`review-comment-${reservation.reservaId}`}
            rows="4"
            maxLength="1000"
            className="input-field resize-none"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            placeholder="Comparte tu experiencia del tour..."
          />
          <div className="mt-1 text-right text-xs text-gray-500">{comment.length}/1000</div>
        </div>

        {error && (
          <div className="input-error">
            <ExclamationCircleIcon className="h-4 w-4" />
            {error}
          </div>
        )}

        <div className="flex flex-wrap gap-2">
          <button type="submit" className="btn-primary px-4 py-2 text-sm" disabled={submitting}>
            {submitting ? <div className="spinner h-4 w-4" /> : <PaperAirplaneIcon className="h-4 w-4" />}
            Publicar resena
          </button>
          <button type="button" className="btn-ghost text-sm" onClick={onCancel} disabled={submitting}>
            Cancelar
          </button>
        </div>
      </div>
    </form>
  )
}

const ReservationCard = ({ reservation, reviewedPackageIds, onReviewCreated }) => {
  const [showReviewForm, setShowReviewForm] = useState(false)
  const badge = statusBadge(reservation.estado)
  const BadgeIcon = badge.icon
  const packageLabel = reservation.paqueteNombre ?? `Paquete ${reservation.paqueteId?.substring(0, 8) ?? ''}`
  const canReview = reservation.estado === 'COMPLETADA'
  const alreadyReviewed = reviewedPackageIds.has(reservation.paqueteId)

  return (
    <article id={`reservation-card-${reservation.reservaId}`} className="glass-card p-6 hover:border-primary-500/30 transition-all duration-300">
      <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <MapPinIcon className="w-4 h-4 text-primary-400" />
            <span className="text-gray-400 text-xs font-medium uppercase tracking-wider">
              ID: {reservation.reservaId?.substring(0, 8)}...
            </span>
          </div>
          <h3 className="font-display font-bold text-white text-lg">{packageLabel}</h3>
          {reservation.paqueteDestino && <p className="text-gray-400 text-sm">{reservation.paqueteDestino}</p>}
        </div>
        <div className={badge.cls}>
          <BadgeIcon className="w-3.5 h-3.5" />
          {badge.label}
        </div>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-5">
        <div className="p-3 rounded-xl bg-white/[0.03] border border-white/5">
          <CalendarDaysIcon className="w-4 h-4 text-primary-400 mb-1" />
          <div className="text-white text-xs font-medium">Fecha</div>
          <div className="text-gray-400 text-xs capitalize">{formatDate(reservation.fechaInicio)}</div>
        </div>
        <div className="p-3 rounded-xl bg-white/[0.03] border border-white/5">
          <UsersIcon className="w-4 h-4 text-primary-400 mb-1" />
          <div className="text-white text-xs font-medium">Asientos</div>
          <div className="text-gray-400 text-xs">{reservation.cantAsientos}</div>
        </div>
        <div className="p-3 rounded-xl bg-white/[0.03] border border-white/5">
          <CurrencyDollarIcon className="w-4 h-4 text-primary-400 mb-1" />
          <div className="text-white text-xs font-medium">Monto</div>
          <div className="text-primary-400 text-sm font-bold">S/ {Number(reservation.montoTotal ?? 0).toFixed(2)}</div>
        </div>
        <div className="p-3 rounded-xl bg-white/[0.03] border border-white/5">
          <ClockIcon className="w-4 h-4 text-primary-400 mb-1" />
          <div className="text-white text-xs font-medium">Estado</div>
          <div className="text-gray-400 text-xs">{badge.label}</div>
        </div>
      </div>

      <div className="flex flex-wrap gap-2 pt-4 border-t border-white/10">
        {reservation.estado === 'PENDIENTE_PAGO' && (
          <Link to={`/pago/${reservation.reservaId}`} id={`pay-reservation-btn-${reservation.reservaId}`} className="btn-primary text-sm px-4 py-2">
            <CurrencyDollarIcon className="w-4 h-4" />
            Pagar ahora
          </Link>
        )}
        {reservation.estado === 'CONFIRMADA' && (
          <Link to={`/reprogramar/${reservation.reservaId}`} id={`reschedule-reservation-btn-${reservation.reservaId}`} className="btn-secondary text-sm px-4 py-2">
            <CalendarDaysIcon className="w-4 h-4" />
            Reprogramar
          </Link>
        )}
        {canReview && !alreadyReviewed && (
          <button
            type="button"
            id={`open-review-btn-${reservation.reservaId}`}
            onClick={() => setShowReviewForm((current) => !current)}
            className="btn-secondary text-sm px-4 py-2"
          >
            <ChatBubbleLeftRightIcon className="w-4 h-4" />
            {showReviewForm ? 'Ocultar resena' : 'Comentar'}
          </button>
        )}
        {canReview && alreadyReviewed && (
          <div className="badge-info">
            <CheckCircleIcon className="w-3.5 h-3.5" />
            Resena enviada
          </div>
        )}
        <div className="text-gray-500 text-xs self-center ml-auto">Paquete: {packageLabel}</div>
      </div>

      {showReviewForm && canReview && !alreadyReviewed && (
        <ReviewForm
          reservation={reservation}
          packageLabel={packageLabel}
          onCreated={(packageId) => {
            onReviewCreated(packageId)
            setShowReviewForm(false)
          }}
          onCancel={() => setShowReviewForm(false)}
        />
      )}
    </article>
  )
}

const MyReservations = () => {
  const [reservations, setReservations] = useState([])
  const [statusCounts, setStatusCounts] = useState({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [filter, setFilter] = useState('ALL')
  const [reviewedPackageIds, setReviewedPackageIds] = useState(() => new Set())

  const loadReservations = useCallback(async (estado = filter) => {
    setLoading(true)
    setError(null)
    try {
      const params = estado === 'ALL' ? {} : { estado }
      const res = await apiClient.get('/reservations', { params })
      setReservations(Array.isArray(res.data) ? res.data : [])
    } catch (err) {
      setError(getApiErrorMessage(err, 'Error al cargar tus reservas. Intenta de nuevo.'))
    } finally {
      setLoading(false)
    }
  }, [filter])

  const loadCounts = useCallback(async () => {
    try {
      const res = await apiClient.get('/reservations')
      const allReservations = Array.isArray(res.data) ? res.data : []
      const counts = allReservations.reduce((acc, reservation) => {
        acc[reservation.estado] = (acc[reservation.estado] ?? 0) + 1
        return acc
      }, { ALL: allReservations.length })
      setStatusCounts(counts)
    } catch {
      setStatusCounts({})
    }
  }, [])

  useEffect(() => {
    loadReservations(filter)
    loadCounts()
  }, [filter, loadReservations, loadCounts])

  const handleRefresh = async () => {
    await Promise.all([loadReservations(filter), loadCounts()])
  }

  const handleReviewCreated = (packageId) => {
    setReviewedPackageIds((current) => {
      const next = new Set(current)
      next.add(packageId)
      return next
    })
  }

  return (
    <div className="page-wrapper">
      <div className="container-main animate-fade-in">
        <div className="mb-8">
          <div className="teal-divider mb-4" />
          <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
            <div>
              <h1 className="font-display text-3xl font-bold text-white">Mis Reservas</h1>
              <p className="text-gray-400 mt-2">
                Consulta tus reservas por estado y revisa fecha, asientos y monto total.
              </p>
            </div>
            <div className="flex flex-wrap gap-2">
              <button type="button" onClick={handleRefresh} className="btn-secondary text-sm">
                <ArrowPathIcon className="w-4 h-4" />
                Actualizar
              </button>
              <Link to="/" className="btn-primary text-sm">
                <MapPinIcon className="w-4 h-4" />
                Explorar tours
              </Link>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3 mb-8">
          {STATUS_FILTERS.map(({ key, label }) => {
            const active = filter === key
            return (
              <button
                key={key}
                id={`filter-${key.toLowerCase()}-btn`}
                type="button"
                onClick={() => setFilter(key)}
                className={`glass-card p-4 text-left ${active ? 'border-primary-500/50 bg-primary-500/10' : ''}`}
              >
                <div className={active ? 'text-primary-400 font-bold text-xl' : 'text-white font-bold text-xl'}>
                  {statusCounts[key] ?? 0}
                </div>
                <div className="text-gray-400 text-xs mt-1">{label}</div>
              </button>
            )
          })}
        </div>

        {loading ? (
          <div className="flex flex-col items-center py-24 gap-4">
            <div className="spinner w-12 h-12" />
            <p className="text-gray-400 animate-pulse">Cargando reservas...</p>
          </div>
        ) : error ? (
          <div className="flex flex-col items-center py-24 gap-4">
            <ExclamationCircleIcon className="w-16 h-16 text-red-400" />
            <p className="text-red-400">{error}</p>
            <button type="button" onClick={handleRefresh} className="btn-secondary text-sm">Reintentar</button>
          </div>
        ) : reservations.length === 0 ? (
          <div className="flex flex-col items-center py-24 gap-6 text-center">
            <div className="w-20 h-20 rounded-full bg-primary-500/10 border border-primary-500/20 flex items-center justify-center">
              <CalendarDaysIcon className="w-10 h-10 text-primary-400" />
            </div>
            <div>
              <p className="text-white font-medium text-lg">
                {filter === 'ALL'
                  ? 'No tienes reservas registradas'
                  : `No hay reservas con estado ${STATUS_FILTERS.find((item) => item.key === filter)?.label}`}
              </p>
              <p className="text-gray-400 text-sm mt-1">
                {filter === 'ALL' ? 'Explora nuestros tours y crea tu primera reserva' : 'Prueba con otro estado'}
              </p>
            </div>
            {filter === 'ALL' ? (
              <Link to="/" className="btn-primary">
                Ver paquetes turisticos
                <ArrowRightIcon className="w-4 h-4" />
              </Link>
            ) : (
              <button type="button" onClick={() => setFilter('ALL')} className="btn-secondary text-sm">
                Ver todas
              </button>
            )}
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            {reservations.map((reservation) => (
              <ReservationCard
                key={reservation.reservaId}
                reservation={reservation}
                reviewedPackageIds={reviewedPackageIds}
                onReviewCreated={handleReviewCreated}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

export default MyReservations
