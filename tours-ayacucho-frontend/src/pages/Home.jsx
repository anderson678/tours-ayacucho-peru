import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import { fetchSiteSettings, getSiteSettings } from '../utils/siteSettings'
import {
  MapPinIcon,
  ClockIcon,
  UsersIcon,
  ArrowRightIcon,
  SparklesIcon,
  GlobeAltIcon,
  ShieldCheckIcon,
  HeartIcon,
  FireIcon,
  TagIcon,
} from '@heroicons/react/24/outline'
import { StarIcon as StarSolid } from '@heroicons/react/24/solid'

// â”€â”€â”€ PackageCard â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const PackageCard = ({ pkg, badge }) => {
  const precio = pkg.precioUnitario ?? 0
  const capacidad = pkg.asientosDisp ?? 0
  const fechaInicio = pkg.fechaInicio ? new Date(pkg.fechaInicio) : null
  const fechaFin = pkg.fechaFin ? new Date(pkg.fechaFin) : null
  const duracion = fechaInicio && fechaFin
    ? Math.max(1, Math.round((fechaFin - fechaInicio) / 86400000) + 1)
    : 1
  const nombre = pkg.nombre ?? 'Paquete Turistico'
  const descripcion = pkg.descripcion ?? 'Explora Ayacucho con este increible paquete.'
  const destino = pkg.destino ?? 'Ayacucho, Peru'
  const imagenUrl = pkg.imagenUrl

  return (
    <Link to={`/package/${pkg.paqueteId ?? pkg.id}`} id={`package-card-${pkg.paqueteId ?? pkg.id}`}>
      <article className="package-card group">
        {/* Image area */}
        <div className="relative h-48 overflow-hidden bg-gradient-to-br from-primary-900 to-dark-800">
          {imagenUrl ? (
            <img
              src={imagenUrl}
              alt={nombre}
              className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
              loading="lazy"
            />
          ) : (
            <div className="absolute inset-0 flex items-center justify-center">
              <MapPinIcon className="w-16 h-16 text-primary-700 opacity-30" />
            </div>
          )}
          {/* Gradient overlay */}
          <div className="absolute inset-0 bg-gradient-to-t from-dark-900 via-dark-900/20 to-transparent" />
          {/* Price Badge */}
          <div className="absolute top-3 right-3 flex flex-col items-end gap-2">
            {badge && (
              <div className="inline-flex items-center gap-1.5 rounded-full border border-amber-300/40 bg-amber-400/90 px-3 py-1 text-xs font-bold text-dark-900 shadow-lg">
                <TagIcon className="h-3.5 w-3.5" />
                {badge}
              </div>
            )}
            <div className="px-3 py-1.5 rounded-xl font-display font-bold text-white text-sm"
              style={{ background: 'linear-gradient(135deg, #00bfbf, #0d9488)', boxShadow: '0 4px 15px rgba(0,191,191,0.4)' }}>
              S/ {Number(precio).toFixed(2)}
            </div>
          </div>
          {/* Destination tag */}
          <div className="absolute bottom-3 left-3 flex items-center gap-1.5">
            <MapPinIcon className="w-3.5 h-3.5 text-primary-400" />
            <span className="text-white text-xs font-medium">{destino}</span>
          </div>
        </div>

        {/* Content */}
        <div className="p-5">
          <h2 className="font-display font-bold text-white text-lg leading-tight mb-2 group-hover:text-primary-400 transition-colors duration-200 line-clamp-1">
            {nombre}
          </h2>
          <p className="text-gray-400 text-sm leading-relaxed line-clamp-2 mb-4">
            {descripcion}
          </p>

          {/* Stats */}
          <div className="flex items-center gap-4 mb-4">
            <div className="flex items-center gap-1.5 text-gray-400 text-xs">
              <ClockIcon className="w-3.5 h-3.5 text-primary-500" />
              <span>{duracion} dia{duracion !== 1 ? 's' : ''}</span>
            </div>
            <div className="flex items-center gap-1.5 text-gray-400 text-xs">
              <UsersIcon className="w-3.5 h-3.5 text-primary-500" />
              <span>{capacidad} cupos</span>
            </div>
            <div className="flex items-center gap-1.5 text-xs">
              {[1, 2, 3, 4, 5].map(s => (
                <StarSolid key={s} className={`w-3 h-3 ${s <= 4 ? 'text-gold-400' : 'text-gray-600'}`} />
              ))}
            </div>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-primary-400 text-xs font-medium">Ver detalles</span>
            <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-primary-500/10 border border-primary-500/30
                            group-hover:bg-primary-500 group-hover:border-primary-500 transition-all duration-300">
              <ArrowRightIcon className="w-4 h-4 text-primary-400 group-hover:text-white transition-colors duration-300" />
            </div>
          </div>
        </div>
      </article>
    </Link>
  )
}

// â”€â”€â”€ Home Page â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const testimonials = [
  {
    name: 'María G.',
    initials: 'MG',
    comment: 'Una experiencia inolvidable. El tour a Millpu fue espectacular.',
    photo: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=160&q=80',
  },
  {
    name: 'Carlos R.',
    initials: 'CR',
    comment: 'Excelente atención y organización. Tours únicos y equipo profesional.',
    photo: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=160&q=80',
  },
  {
    name: 'Laura M.',
    initials: 'LM',
    comment: 'Ayacucho es mágico y este tour lo demostró. Totalmente recomendado.',
    photo: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=160&q=80',
  },
]

const Home = () => {
  const { isAuthenticated } = useAuth()
  const [siteSettings, setSiteSettings] = useState(getSiteSettings)
  const [packages, setPackages] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const handleSettingsUpdate = () => setSiteSettings(getSiteSettings())
    window.addEventListener('site-settings-updated', handleSettingsUpdate)
    window.addEventListener('storage', handleSettingsUpdate)
    fetchSiteSettings()
      .then(setSiteSettings)
      .catch(() => setSiteSettings(getSiteSettings()))
    return () => {
      window.removeEventListener('site-settings-updated', handleSettingsUpdate)
      window.removeEventListener('storage', handleSettingsUpdate)
    }
  }, [])

  useEffect(() => {
    const fetchPackages = async () => {
      try {
        const res = await apiClient.get('/packages')
        setPackages(Array.isArray(res.data) ? res.data : [])
      } catch (err) {
        setError(getApiErrorMessage(err, 'No se pudo cargar el catálogo. Verifica la conexión con el servidor.'))
      } finally {
        setLoading(false)
      }
    }
    fetchPackages()
  }, [])

  const featuredPackages = packages
    .map((pkg) => {
      const name = (pkg.nombre ?? '').toLowerCase()
      const price = Number(pkg.precioUnitario ?? 0)
      let score = 0

      if (name.includes('millpu')) score += 90
      if (name.includes('experiencia')) score += 82
      if (name.includes('wari') || name.includes('quinua')) score += 78
      if (name.includes('vilcashuaman') || name.includes('inca')) score += 72
      if (name.includes('city') || name.includes('colonial')) score += 68
      if (price > 0 && price <= 150) score += 18
      if ((pkg.asientosDisp ?? 0) > 0) score += 10

      return { ...pkg, featuredScore: score }
    })
    .sort((a, b) => b.featuredScore - a.featuredScore)
    .slice(0, 4)

  const featuredBadges = ['Promocion especial', 'Imperdible', 'Mas atractivo', 'Salida destacada']
  const heroBackground = siteSettings.heroImages?.[0]

  return (
    <main>
      {/* â”€â”€â”€ HERO SECTION â”€â”€â”€ */}
      <section className="relative min-h-screen flex items-center justify-center overflow-hidden">
        <div className="absolute inset-0 overflow-hidden">
          {heroBackground?.imageUrl ? (
            <img
              src={heroBackground.imageUrl}
              alt={heroBackground.title || siteSettings.heroTitle}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="h-full w-full bg-hero-gradient" />
          )}
        </div>
        <div className="absolute inset-0 bg-gradient-to-b from-dark-900/75 via-dark-900/45 to-dark-900/95" />
        <div className="absolute inset-0 bg-gradient-to-r from-dark-900/80 via-dark-900/35 to-primary-900/45" />
        {heroBackground?.title && (
          <div className="absolute bottom-8 left-6 hidden rounded-full border border-white/20 bg-black/35 px-4 py-2 text-sm font-semibold text-white backdrop-blur-md md:block">
            {heroBackground.title}
          </div>
        )}

        {/* Content */}
        <div className="relative z-10 max-w-5xl mx-auto px-4 text-center animate-fade-in">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full border border-primary-500/30 bg-primary-500/10 text-primary-400 text-sm font-medium mb-8">
            <SparklesIcon className="w-4 h-4" />
            {siteSettings.heroBadge}
          </div>

          <h1 className="font-display text-5xl sm:text-6xl md:text-7xl font-black text-white mb-6 leading-tight">
            {siteSettings.heroTitle}
          </h1>

          <p className="text-gray-300 text-lg md:text-xl max-w-2xl mx-auto mb-10 leading-relaxed">
            {siteSettings.heroSubtitle}
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a href="#catalogo" className="btn-primary text-base px-8 py-4">
              <MapPinIcon className="w-5 h-5" />
              Ver 4 lugares destacados
            </a>
            {!isAuthenticated && (
              <Link to="/register" className="btn-secondary text-base px-8 py-4">
                Crear Cuenta Gratis
              </Link>
            )}
          </div>

          {/* Stats */}
          <div className="grid grid-cols-3 gap-6 mt-16 max-w-sm mx-auto">
            {[
              { value: siteSettings.heroStatsTours, label: 'Tours' },
              { value: siteSettings.heroStatsTravelers, label: 'Viajeros' },
              { value: siteSettings.heroStatsRating, label: 'Rating' },
            ].map(({ value, label }) => (
              <div key={label} className="text-center">
                <div className="font-display text-2xl font-bold gradient-text">{value}</div>
                <div className="text-gray-500 text-xs mt-1">{label}</div>
              </div>
            ))}
          </div>
        </div>

        {/* Scroll indicator */}
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 animate-bounce">
          <div className="w-5 h-8 rounded-full border-2 border-primary-500/50 flex items-start justify-center p-1">
            <div className="w-1 h-2 rounded-full bg-primary-500 animate-pulse" />
          </div>
        </div>
      </section>

      {/* â”€â”€â”€ FEATURES â”€â”€â”€ */}
      <section className="py-16 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="mb-10 max-w-2xl">
            <div className="teal-divider mb-4" />
            <p className="mb-3 text-sm font-bold uppercase tracking-[0.18em] text-primary-400">Viaja con confianza</p>
            <h2 className="section-title">Todo listo para tu próxima aventura</h2>
            <p className="section-subtitle">Atención cercana, conocimiento local y reservas diseñadas para que disfrutes Ayacucho sin preocupaciones.</p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[
              {
                icon: ShieldCheckIcon,
                title: 'Reservas Seguras',
                desc: 'Pago seguro y garantia de servicio en todos nuestros tours.',
              },
              {
                icon: GlobeAltIcon,
                title: 'Guias Expertos',
                desc: 'Conocedores profundos de la historia y cultura de Ayacucho.',
              },
              {
                icon: HeartIcon,
                title: 'Experiencias Unicas',
                desc: 'Itinerarios personalizados para vivir Huamanga de verdad.',
              },
            ].map(({ icon: Icon, title, desc }) => (
              <div key={title} className="glass-card p-6 flex gap-4 hover:border-primary-500/30 transition-all duration-300">
                <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-primary-500/10 border border-primary-500/20 flex items-center justify-center">
                  <Icon className="w-6 h-6 text-primary-400" />
                </div>
                <div>
                  <h3 className="font-display font-semibold text-white mb-1">{title}</h3>
                  <p className="text-gray-400 text-sm leading-relaxed">{desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* â”€â”€â”€ CATALOG SECTION â”€â”€â”€ */}
      <section id="catalogo" className="py-16 px-4">
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-10">
            <div>
              <div className="teal-divider mb-4" />
              <div className="mb-3 inline-flex items-center gap-2 rounded-full border border-amber-400/30 bg-amber-400/10 px-4 py-2 text-sm font-semibold text-amber-300">
                <FireIcon className="h-4 w-4" />
                Promociones y lugares imperdibles
              </div>
              <h2 className="section-title">4 Lugares Turisticos Destacados</h2>
              <p className="section-subtitle">
                Una seleccion corta con los tours mas llamativos para reservar rapido desde la portada.
              </p>
            </div>
            <div className="rounded-2xl border border-primary-400/20 bg-primary-500/10 px-5 py-4 text-sm text-primary-100">
              <p className="font-display text-xl font-bold text-white">Solo 4 opciones</p>
              <p className="text-gray-400">La portada muestra lo mas atractivo y promocional.</p>
              {isAuthenticated && (
                <Link to="/paquetes" className="mt-3 inline-flex items-center gap-2 text-sm font-bold text-primary-300 hover:text-white">
                  Ver todos los paquetes
                  <ArrowRightIcon className="h-4 w-4" />
                </Link>
              )}
            </div>
          </div>

          {/* Content */}
          {loading ? (
            <div className="flex flex-col items-center justify-center py-24 gap-4">
              <div className="spinner w-12 h-12" />
              <p className="text-gray-400 animate-pulse">Cargando paquetes turisticos...</p>
            </div>
          ) : error ? (
            <div className="flex flex-col items-center justify-center py-24 gap-4">
              <div className="w-16 h-16 rounded-full bg-red-500/10 flex items-center justify-center">
                <SparklesIcon className="w-8 h-8 text-red-400" />
              </div>
              <p className="text-red-400 font-medium">{error}</p>
              <button onClick={() => window.location.reload()} className="btn-secondary text-sm">
                Reintentar
              </button>
            </div>
          ) : featuredPackages.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-24 gap-4">
              <MapPinIcon className="w-16 h-16 text-gray-700" />
              <p className="text-gray-400 font-medium">
                No hay paquetes disponibles en este momento
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {featuredPackages.map((pkg, index) => (
                <PackageCard key={pkg.paqueteId ?? pkg.id} pkg={pkg} badge={featuredBadges[index]} />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* â”€â”€â”€ CTA SECTION â”€â”€â”€ */}
      <section className="px-4 py-20">
        <div className="mx-auto grid max-w-6xl grid-cols-1 items-center gap-10 lg:grid-cols-[1.1fr_0.9fr]">
          <div>
            <div className="teal-divider mb-4" />
            <p className="mb-3 text-sm font-bold uppercase tracking-[0.18em] text-primary-400">Nuestra esencia</p>
            <h2 className="section-title">Sobre Nosotros</h2>
            <p className="section-subtitle max-w-2xl text-base leading-8">
              Somos TOURS AYACUCHO PERÚ, una agencia de viajes con más de 10 años de experiencia en la organización de tours culturales, históricos y de naturaleza en la región de Ayacucho. Nuestra misión es brindar experiencias auténticas que conecten a los viajeros con la riqueza cultural, histórica y natural de Huamanga, mientras contribuimos al desarrollo sostenible de las comunidades locales.
            </p>
          </div>
          <div className="glass-card relative overflow-hidden p-7">
            <div className="absolute -right-10 -top-10 h-36 w-36 rounded-full bg-primary-500/10 blur-2xl" />
            <div className="relative">
              <div className="mb-6 flex h-14 w-14 items-center justify-center rounded-2xl border border-primary-400/30 bg-primary-500/10">
                <GlobeAltIcon className="h-7 w-7 text-primary-300" />
              </div>
              <h3 className="font-display text-2xl font-bold text-white">Viajes que dejan huella</h3>
              <p className="mt-3 text-sm leading-relaxed text-gray-400">Creamos recorridos responsables, cercanos y memorables junto a quienes conocen Ayacucho mejor que nadie.</p>
              <div className="mt-6 grid grid-cols-2 gap-3 border-t border-white/10 pt-5">
                <div className="rounded-xl bg-white/[0.03] p-3">
                  <p className="font-display text-xl font-black text-primary-300">10+ años</p>
                  <p className="mt-1 text-xs text-gray-400">de experiencia</p>
                </div>
                <div className="rounded-xl bg-white/[0.03] p-3">
                  <p className="font-display text-xl font-black text-amber-300">Local</p>
                  <p className="mt-1 text-xs text-gray-400">compromiso sostenible</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="px-4 py-20">
        <div className="mx-auto max-w-6xl">
          <div className="mx-auto mb-12 max-w-2xl text-center">
            <div className="teal-divider mx-auto mb-4" />
            <p className="mb-3 text-sm font-bold uppercase tracking-[0.18em] text-primary-400">Experiencias reales</p>
            <h2 className="section-title">Lo que dicen nuestros viajeros</h2>
            <p className="section-subtitle">Historias de personas que ya descubrieron la magia de Ayacucho con nosotros.</p>
          </div>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
            {testimonials.map((testimonial) => (
              <article key={testimonial.name} className="glass-card flex h-full flex-col p-6 transition-all duration-300 hover:-translate-y-1 hover:border-primary-400/40 hover:shadow-card-hover">
                <div className="mb-5 flex items-center justify-between gap-4">
                  <div className="flex items-center gap-3">
                    <div className="relative flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-full bg-primary-600 text-sm font-black text-white">
                      <span>{testimonial.initials}</span>
                      <img
                        src={testimonial.photo}
                        alt={`Foto de ${testimonial.name}`}
                        className="absolute inset-0 h-full w-full object-cover"
                        loading="lazy"
                        onError={(event) => { event.currentTarget.style.display = 'none' }}
                      />
                    </div>
                    <div>
                      <h3 className="font-display font-bold text-white">{testimonial.name}</h3>
                      <p className="text-xs text-gray-400">Viajero verificado</p>
                    </div>
                  </div>
                  <div className="flex gap-0.5" aria-label="Calificación de 5 estrellas">
                    {[1, 2, 3, 4, 5].map((star) => <StarSolid key={star} className="h-4 w-4 text-gold-400" />)}
                  </div>
                </div>
                <p className="mt-auto text-sm leading-relaxed text-gray-300">&ldquo;{testimonial.comment}&rdquo;</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      {!isAuthenticated && (
        <section className="py-16 px-4">
          <div className="max-w-3xl mx-auto">
            <div className="glass-card p-10 text-center relative overflow-hidden">
              <div className="absolute inset-0 opacity-5 pointer-events-none"
                style={{ background: 'radial-gradient(circle at 50% 50%, #00bfbf, transparent)' }} />
              <div className="relative z-10">
                <SparklesIcon className="w-12 h-12 text-primary-400 mx-auto mb-4" />
                <h2 className="font-display text-3xl font-bold text-white mb-4">
                  Listo para tu proxima aventura?
                </h2>
                <p className="text-gray-400 mb-8">
                  Registrate gratis y accede a todos nuestros paquetes, gestiona tus reservas y mucho mas.
                </p>
                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                  <Link to="/register" className="btn-primary">
                    Comenzar Ahora
                    <ArrowRightIcon className="w-4 h-4" />
                  </Link>
                  <Link to="/login" className="btn-secondary">
                    Ya tengo cuenta
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </section>
      )}
    </main>
  )
}

export default Home

