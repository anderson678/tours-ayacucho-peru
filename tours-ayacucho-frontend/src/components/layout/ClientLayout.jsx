import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import {
  ArrowRightOnRectangleIcon,
  CalendarDaysIcon,
  HomeIcon,
  MapPinIcon,
  Squares2X2Icon,
  UserCircleIcon,
} from '@heroicons/react/24/outline'

const navigationItems = [
  { key: 'inicio', label: 'Inicio', icon: HomeIcon, to: '/' },
  { key: 'paquetes', label: 'Paquetes', icon: Squares2X2Icon, to: '/paquetes' },
  { key: 'reservas', label: 'Mis reservas', icon: CalendarDaysIcon, to: '/mis-reservas' },
  { key: 'perfil', label: 'Mi perfil', icon: UserCircleIcon, to: '/perfil' },
]

const getActiveSection = (pathname) => {
  if (pathname.startsWith('/paquetes') || pathname.startsWith('/reservar')) return 'paquetes'
  if (pathname.startsWith('/mis-reservas') || pathname.startsWith('/pago') || pathname.startsWith('/reprogramar')) return 'reservas'
  if (pathname.startsWith('/perfil')) return 'perfil'
  return 'inicio'
}

const ClientLayout = ({ children }) => {
  const { user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const activeSection = getActiveSection(location.pathname)
  const initials = (user?.nombre || 'C')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((name) => name[0])
    .join('')
    .toUpperCase()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  const renderNavigation = (compact = false) => navigationItems.map(({ key, label, icon: Icon, to }) => {
    const isActive = activeSection === key

    return (
      <Link
        key={key}
        to={to}
        className={`group flex items-center gap-3 rounded-xl font-semibold transition-all duration-200 ${
          compact
            ? `shrink-0 px-3 py-2 text-sm ${isActive ? 'bg-cyan-600 text-white shadow-lg shadow-cyan-950/30' : 'text-gray-300 hover:bg-gray-700 hover:text-white'}`
            : `w-full px-4 py-3 ${isActive ? 'bg-cyan-600 text-white shadow-lg shadow-cyan-950/30' : 'text-gray-300 hover:bg-gray-700 hover:text-white'}`
        }`}
        aria-current={isActive ? 'page' : undefined}
      >
        <Icon className="h-5 w-5 shrink-0" />
        <span>{label}</span>
      </Link>
    )
  })

  return (
    <div className="client-layout min-h-screen bg-dark-900 text-white lg:flex">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-[280px] flex-col border-r border-white/10 bg-gray-800/95 px-4 py-6 shadow-2xl backdrop-blur-xl lg:flex">
        <Link to="/" className="mb-10 flex items-center gap-3 px-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-teal-gradient shadow-teal">
            <MapPinIcon className="h-6 w-6 text-white" />
          </div>
          <div className="leading-tight">
            <p className="font-display text-base font-black tracking-wide text-white">TOURS AYACUCHO</p>
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-cyan-300">Área de cliente</p>
          </div>
        </Link>

        <p className="mb-3 px-3 text-[11px] font-bold uppercase tracking-[0.16em] text-gray-500">Mi experiencia</p>
        <nav className="space-y-2" aria-label="Navegación de cliente">
          {renderNavigation()}
        </nav>

        <div className="mt-auto border-t border-white/10 pt-5">
          <Link to="/perfil" className="mb-3 flex items-center gap-3 rounded-xl p-3 transition-all duration-200 hover:bg-gray-700">
            {user?.fotoUrl ? (
              <img src={user.fotoUrl} alt="Mi perfil" className="h-10 w-10 rounded-full object-cover" />
            ) : (
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-cyan-600 text-sm font-black text-white">
                {initials}
              </div>
            )}
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-bold text-white">{user?.nombre || 'Cliente'}</p>
              <p className="truncate text-xs text-gray-400">Mi cuenta</p>
            </div>
            <UserCircleIcon className="h-5 w-5 text-gray-400" />
          </Link>
          <button type="button" onClick={handleLogout} className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold text-gray-400 transition-all duration-200 hover:bg-red-500/10 hover:text-red-300">
            <ArrowRightOnRectangleIcon className="h-5 w-5" />
            Cerrar sesión
          </button>
        </div>
      </aside>

      <main className="min-h-screen flex-1 lg:ml-[280px]">
        <div className="sticky top-0 z-30 border-b border-white/10 bg-dark-900/95 px-4 py-3 backdrop-blur-xl lg:hidden">
          <div className="mb-3 flex items-center gap-2">
            <MapPinIcon className="h-5 w-5 text-cyan-400" />
            <span className="font-display text-sm font-bold text-white">Área de cliente</span>
          </div>
          <nav className="flex gap-2 overflow-x-auto pb-1" aria-label="Navegación de cliente móvil">
            {renderNavigation(true)}
          </nav>
        </div>
        {children}
      </main>
    </div>
  )
}

export default ClientLayout
