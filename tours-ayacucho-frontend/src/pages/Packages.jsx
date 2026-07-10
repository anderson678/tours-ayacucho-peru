import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import {
  ArrowRightIcon,
  ClockIcon,
  MagnifyingGlassIcon,
  MapPinIcon,
  SparklesIcon,
  UsersIcon,
} from '@heroicons/react/24/outline'
import { StarIcon as StarSolid } from '@heroicons/react/24/solid'

const formatDuration = (start, end) => {
  if (!start || !end) return 1
  return Math.max(1, Math.round((new Date(end) - new Date(start)) / 86400000) + 1)
}

const PackageCard = ({ pkg }) => {
  const duration = formatDuration(pkg.fechaInicio, pkg.fechaFin)

  return (
    <Link to={`/package/${pkg.paqueteId ?? pkg.id}`} className="group block">
      <article className="package-card h-full">
        <div className="relative h-52 overflow-hidden bg-gradient-to-br from-primary-900 to-dark-800">
          {pkg.imagenUrl ? (
            <img
              src={pkg.imagenUrl}
              alt={pkg.nombre}
              className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
              loading="lazy"
            />
          ) : (
            <div className="absolute inset-0 flex items-center justify-center">
              <MapPinIcon className="h-16 w-16 text-primary-700 opacity-30" />
            </div>
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-dark-900 via-dark-900/25 to-transparent" />
          <div className="absolute right-3 top-3 rounded-xl bg-teal-gradient px-3 py-1.5 font-display text-sm font-bold text-white shadow-teal">
            S/ {Number(pkg.precioUnitario ?? 0).toFixed(2)}
          </div>
          <div className="absolute bottom-3 left-3 flex items-center gap-1.5">
            <MapPinIcon className="h-3.5 w-3.5 text-primary-400" />
            <span className="text-xs font-medium text-white">{pkg.destino ?? 'Ayacucho'}</span>
          </div>
        </div>

        <div className="p-5">
          <h2 className="mb-2 font-display text-lg font-bold leading-tight text-white transition-colors group-hover:text-primary-400">
            {pkg.nombre}
          </h2>
          <p className="mb-4 line-clamp-2 text-sm leading-relaxed text-gray-400">
            {pkg.descripcion ?? 'Explora Ayacucho con este paquete turistico.'}
          </p>

          <div className="mb-4 flex flex-wrap items-center gap-4">
            <div className="flex items-center gap-1.5 text-xs text-gray-400">
              <ClockIcon className="h-3.5 w-3.5 text-primary-500" />
              <span>{duration} dia{duration !== 1 ? 's' : ''}</span>
            </div>
            <div className="flex items-center gap-1.5 text-xs text-gray-400">
              <UsersIcon className="h-3.5 w-3.5 text-primary-500" />
              <span>{pkg.asientosDisp ?? 0} cupos</span>
            </div>
            <div className="flex items-center gap-1">
              {[1, 2, 3, 4, 5].map((star) => (
                <StarSolid key={star} className={`h-3 w-3 ${star <= 4 ? 'text-gold-400' : 'text-gray-600'}`} />
              ))}
            </div>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-xs font-medium text-primary-400">Ver detalles</span>
            <div className="flex h-8 w-8 items-center justify-center rounded-lg border border-primary-500/30 bg-primary-500/10 transition-all duration-300 group-hover:border-primary-500 group-hover:bg-primary-500">
              <ArrowRightIcon className="h-4 w-4 text-primary-400 transition-colors group-hover:text-white" />
            </div>
          </div>
        </div>
      </article>
    </Link>
  )
}

const Packages = () => {
  const [packages, setPackages] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')

  useEffect(() => {
    const loadPackages = async () => {
      try {
        const response = await apiClient.get('/packages')
        setPackages(Array.isArray(response.data) ? response.data : [])
      } catch (err) {
        setError(getApiErrorMessage(err, 'No se pudieron cargar los paquetes turisticos.'))
      } finally {
        setLoading(false)
      }
    }

    loadPackages()
  }, [])

  const filteredPackages = packages.filter((pkg) => {
    const term = search.trim().toLowerCase()
    if (!term) return true

    return [pkg.nombre, pkg.destino, pkg.descripcion]
      .filter(Boolean)
      .some((value) => value.toLowerCase().includes(term))
  })

  return (
    <main className="page-wrapper">
      <div className="container-main animate-fade-in">
        <header className="mb-8 flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <div className="mb-3 inline-flex items-center gap-2 rounded-full border border-primary-400/30 bg-primary-500/10 px-4 py-2 text-sm font-semibold text-primary-300">
              <SparklesIcon className="h-4 w-4" />
              Catalogo completo
            </div>
            <h1 className="font-display text-4xl font-black text-white">Paquetes Turisticos</h1>
            <p className="mt-3 max-w-2xl text-gray-400">
              Explora todas las zonas turisticas disponibles. La portada muestra solo los destacados, aqui encuentras el catalogo completo.
            </p>
          </div>

          <div className="relative w-full lg:w-80">
            <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-500" />
            <input
              type="text"
              className="input-field pl-10"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Buscar por lugar o paquete..."
            />
          </div>
        </header>

        {loading ? (
          <div className="flex flex-col items-center justify-center gap-4 py-24">
            <div className="spinner h-12 w-12" />
            <p className="animate-pulse text-gray-400">Cargando paquetes...</p>
          </div>
        ) : error ? (
          <div className="glass-card flex flex-col items-center gap-4 p-10 text-center">
            <MapPinIcon className="h-14 w-14 text-red-400" />
            <p className="font-medium text-red-300">{error}</p>
          </div>
        ) : filteredPackages.length === 0 ? (
          <div className="glass-card flex flex-col items-center gap-4 p-10 text-center">
            <MapPinIcon className="h-14 w-14 text-gray-600" />
            <p className="font-medium text-gray-400">No hay paquetes que coincidan con tu busqueda.</p>
          </div>
        ) : (
          <>
            <div className="mb-5 flex items-center justify-between">
              <p className="text-sm text-gray-400">
                Mostrando <span className="font-semibold text-white">{filteredPackages.length}</span> de <span className="font-semibold text-white">{packages.length}</span> paquetes
              </p>
            </div>
            <section className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {filteredPackages.map((pkg) => (
                <PackageCard key={pkg.paqueteId ?? pkg.id} pkg={pkg} />
              ))}
            </section>
          </>
        )}
      </div>
    </main>
  )
}

export default Packages
