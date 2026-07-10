import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import apiClient from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import {
  MapPinIcon,
  ClockIcon,
  UsersIcon,
  ArrowLeftIcon,
  CalendarDaysIcon,
  CheckCircleIcon,
  ShieldCheckIcon,
} from '@heroicons/react/24/outline'
import { StarIcon as StarSolid } from '@heroicons/react/24/solid'

const PackageDetail = () => {
  const { id } = useParams()
  const navigate = useNavigate()
  const { isAuthenticated, isAdmin } = useAuth()
  const [pkg, setPkg] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetch = async () => {
      try {
        const res = await apiClient.get(`/packages/${id}`)
        setPkg(res.data)
      } catch (err) {
        setError(err.response?.status === 404 ? 'Paquete no encontrado' : 'Error al cargar el paquete')
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [id])

  if (loading) {
    return (
      <div className="page-wrapper flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="spinner w-12 h-12" />
          <p className="text-gray-400 animate-pulse">Cargando paquete...</p>
        </div>
      </div>
    )
  }

  if (error || !pkg) {
    return (
      <div className="page-wrapper flex flex-col items-center justify-center gap-6">
        <div className="w-20 h-20 rounded-full bg-red-500/10 flex items-center justify-center">
          <MapPinIcon className="w-10 h-10 text-red-400" />
        </div>
        <p className="text-red-400 font-medium text-lg">{error || 'Paquete no encontrado'}</p>
        <Link to="/" className="btn-secondary">
          <ArrowLeftIcon className="w-4 h-4" />
          Volver al catálogo
        </Link>
      </div>
    )
  }

  const precio = pkg.precioUnitario ?? 0
  const capacidad = pkg.asientosDisp ?? 0
  const canReserve = capacidad > 0 && pkg.activo !== false
  const fechaInicio = pkg.fechaInicio ? new Date(pkg.fechaInicio) : null
  const fechaFin = pkg.fechaFin ? new Date(pkg.fechaFin) : null
  const duracion = fechaInicio && fechaFin
    ? Math.max(1, Math.round((fechaFin - fechaInicio) / 86400000) + 1)
    : 1
  const nombre = pkg.nombre ?? 'Paquete Turístico'
  const descripcion = pkg.descripcion ?? ''
  const destino = pkg.destino ?? 'Ayacucho, Perú'
  const imagenUrl = pkg.imagenUrl
  const incluye = pkg.incluye ?? ''

  const includeItems = incluye
    ? incluye.split(/[,\n]/).map(s => s.trim()).filter(Boolean)
    : ['Guía turístico experto', 'Transporte incluido', 'Seguro de viaje', 'Hospedaje']

  return (
    <div className="page-wrapper">
      <div className="container-main animate-fade-in">
        {/* Back button */}
        <button
          id="back-to-catalog-btn"
          onClick={() => navigate(-1)}
          className="btn-ghost mb-6 -ml-2"
        >
          <ArrowLeftIcon className="w-4 h-4" />
          Volver al catálogo
        </button>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Content */}
          <div className="lg:col-span-2 flex flex-col gap-6">
            {/* Hero Image area */}
            <div className="glass-card overflow-hidden">
              <div className="relative h-64 md:h-80 bg-gradient-to-br from-primary-900/50 to-dark-800 flex items-center justify-center overflow-hidden">
                {imagenUrl ? (
                  <img
                    src={imagenUrl}
                    alt={nombre}
                    className="absolute inset-0 h-full w-full object-cover"
                  />
                ) : (
                  <MapPinIcon className="w-24 h-24 text-primary-700/30" />
                )}
                <div className="absolute inset-0 bg-gradient-to-t from-dark-900/90 via-dark-900/25 to-transparent" />
                <div className="absolute bottom-6 left-6 right-6">
                  <div className="flex items-center gap-2 mb-2">
                    <MapPinIcon className="w-4 h-4 text-primary-400" />
                    <span className="text-primary-400 text-sm font-medium">{destino}</span>
                  </div>
                  <h1 className="font-display text-3xl md:text-4xl font-bold text-white">{nombre}</h1>
                </div>
              </div>

              <div className="p-6">
                {/* Quick stats */}
                <div className="grid grid-cols-3 gap-4 mb-6">
                  <div className="text-center p-3 rounded-xl bg-primary-500/5 border border-primary-500/10">
                    <ClockIcon className="w-5 h-5 text-primary-400 mx-auto mb-1" />
                    <div className="text-white font-semibold">{duracion}</div>
                    <div className="text-gray-500 text-xs">Día{duracion !== 1 ? 's' : ''}</div>
                  </div>
                  <div className="text-center p-3 rounded-xl bg-primary-500/5 border border-primary-500/10">
                    <UsersIcon className="w-5 h-5 text-primary-400 mx-auto mb-1" />
                    <div className="text-white font-semibold">{capacidad}</div>
                    <div className="text-gray-500 text-xs">Cupos</div>
                  </div>
                  <div className="text-center p-3 rounded-xl bg-gold-500/5 border border-gold-500/10">
                    <div className="flex justify-center gap-0.5 mb-1">
                      {[1,2,3,4,5].map(s => (
                        <StarSolid key={s} className={`w-3 h-3 ${s <= 4 ? 'text-gold-400' : 'text-gray-600'}`} />
                      ))}
                    </div>
                    <div className="text-white font-semibold">4.8</div>
                    <div className="text-gray-500 text-xs">Rating</div>
                  </div>
                </div>

                {/* Description */}
                <div>
                  <h2 className="font-display font-bold text-white text-xl mb-3">Descripción</h2>
                  <p className="text-gray-300 leading-relaxed">{descripcion}</p>
                </div>
              </div>
            </div>

            {/* What's Included */}
            <div className="glass-card p-6">
              <h2 className="font-display font-bold text-white text-xl mb-4 flex items-center gap-2">
                <CheckCircleIcon className="w-6 h-6 text-primary-400" />
                ¿Qué incluye?
              </h2>
              <ul className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {includeItems.map((item, i) => (
                  <li key={i} className="flex items-center gap-2.5 text-gray-300 text-sm">
                    <div className="flex-shrink-0 w-5 h-5 rounded-full bg-emerald-500/20 border border-emerald-500/30 flex items-center justify-center">
                      <CheckCircleIcon className="w-3.5 h-3.5 text-emerald-400" />
                    </div>
                    {item}
                  </li>
                ))}
              </ul>
            </div>
          </div>

          {/* Booking Sidebar */}
          <div className="lg:col-span-1">
            <div className="glass-card p-6 sticky top-24">
              {/* Price */}
              <div className="flex items-baseline gap-1 mb-1">
                <span className="font-display text-4xl font-black gradient-text">
                  S/ {Number(precio).toFixed(2)}
                </span>
              </div>
              <p className="text-gray-400 text-sm mb-6">por persona</p>

              {/* Rating */}
              <div className="flex items-center gap-2 mb-6 p-3 rounded-xl bg-gold-500/5 border border-gold-500/10">
                <div className="flex gap-0.5">
                  {[1,2,3,4,5].map(s => (
                    <StarSolid key={s} className={`w-4 h-4 ${s <= 4 ? 'text-gold-400' : 'text-gray-600'}`} />
                  ))}
                </div>
                <span className="text-white font-medium text-sm">4.8</span>
                <span className="text-gray-400 text-xs">(127 reseñas)</span>
              </div>

              {/* Info rows */}
              <div className="flex flex-col gap-3 mb-6">
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-400 flex items-center gap-1.5">
                    <ClockIcon className="w-4 h-4 text-primary-500" /> Duración
                  </span>
                  <span className="text-white font-medium">{duracion} día{duracion !== 1 ? 's' : ''}</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-400 flex items-center gap-1.5">
                    <UsersIcon className="w-4 h-4 text-primary-500" /> Disponibles
                  </span>
                  <span className={`font-medium ${capacidad > 5 ? 'text-emerald-400' : capacidad > 0 ? 'text-amber-400' : 'text-red-400'}`}>
                    {capacidad} cupos
                  </span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-400 flex items-center gap-1.5">
                    <MapPinIcon className="w-4 h-4 text-primary-500" /> Destino
                  </span>
                  <span className="text-white font-medium">{destino}</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-400 flex items-center gap-1.5">
                    <CalendarDaysIcon className="w-4 h-4 text-primary-500" /> Inicio
                  </span>
                  <span className="text-white font-medium">
                    {fechaInicio ? fechaInicio.toLocaleDateString('es-PE') : 'N/A'}
                  </span>
                </div>
              </div>

              {/* CTA */}
              {isAuthenticated && isAdmin ? (
                <Link
                  to="/admin"
                  className="btn-secondary w-full text-center"
                >
                  <ShieldCheckIcon className="w-5 h-5" />
                  Gestionar en Admin
                </Link>
              ) : isAuthenticated ? (
                canReserve ? (
                  <Link
                    to={`/reservar/${pkg.paqueteId ?? pkg.id}`}
                    id="reserve-package-btn"
                    className="btn-primary w-full"
                  >
                    <CalendarDaysIcon className="w-5 h-5" />
                    Reservar Ahora
                  </Link>
                ) : (
                  <button type="button" id="reserve-package-btn" className="btn-secondary w-full" disabled>
                    Sin disponibilidad
                  </button>
                )
              ) : (
                <div className="flex flex-col gap-3">
                  <Link to="/login" className="btn-primary w-full text-center">
                    Iniciar Sesión para Reservar
                  </Link>
                  <p className="text-center text-gray-500 text-xs">
                    ¿No tienes cuenta?{' '}
                    <Link to="/register" className="text-primary-400 hover:text-primary-300">
                      Regístrate gratis
                    </Link>
                  </p>
                </div>
              )}

              {/* Trust badges */}
              <div className="mt-4 pt-4 border-t border-white/10 flex flex-col gap-2">
                {['Cancelación gratuita 48h antes', 'Pago 100% seguro', 'Confirmación inmediata'].map(txt => (
                  <div key={txt} className="flex items-center gap-2 text-gray-400 text-xs">
                    <CheckCircleIcon className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0" />
                    {txt}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default PackageDetail


