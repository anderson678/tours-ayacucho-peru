import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import {
  ArrowRightOnRectangleIcon,
  ChartPieIcon,
  ClipboardDocumentListIcon,
  CubeIcon,
  DocumentChartBarIcon,
  HomeIcon,
  MapPinIcon,
  UserCircleIcon,
  UsersIcon,
} from '@heroicons/react/24/outline'

const navigationItems = [
  { key: 'dashboard', label: 'Panel', icon: ChartPieIcon },
  { key: 'reservas', label: 'Reservas', icon: ClipboardDocumentListIcon },
  { key: 'paquetes', label: 'Paquetes', icon: CubeIcon },
  { key: 'clientes', label: 'Clientes', icon: UsersIcon },
  { key: 'portada', label: 'Portada', icon: HomeIcon },
  { key: 'reportes', label: 'Reportes', icon: DocumentChartBarIcon },
]

const AdminLayout = ({ activeSection, onSectionChange, children }) => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const initials = (user?.nombre || 'A')
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

  const renderNavigation = (compact = false) => navigationItems.map(({ key, label, icon: Icon }) => {
    const isActive = activeSection === key

    return (
      <button
        key={key}
        type="button"
        onClick={() => onSectionChange(key)}
        className={`group flex items-center gap-3 rounded-xl font-semibold transition-all duration-200 ${
          compact
            ? `shrink-0 px-3 py-2 text-sm ${isActive ? 'bg-cyan-600 text-white shadow-lg shadow-cyan-950/30' : 'text-gray-300 hover:bg-gray-700 hover:text-white'}`
            : `w-full px-4 py-3 text-left ${isActive ? 'bg-cyan-600 text-white shadow-lg shadow-cyan-950/30' : 'text-gray-300 hover:bg-gray-700 hover:text-white'}`
        }`}
        aria-current={isActive ? 'page' : undefined}
      >
        <Icon className="h-5 w-5 shrink-0" />
        <span>{label}</span>
      </button>
    )
  })

  return (
    <div className="min-h-screen bg-dark-900 text-white lg:flex">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-[280px] flex-col border-r border-white/10 bg-gray-800/95 px-4 py-6 shadow-2xl backdrop-blur-xl lg:flex">
        <Link to="/" className="mb-10 flex items-center gap-3 px-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-teal-gradient shadow-teal">
            <MapPinIcon className="h-6 w-6 text-white" />
          </div>
          <div className="leading-tight">
            <p className="font-display text-base font-black tracking-wide text-white">TOURS AYACUCHO</p>
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-cyan-300">Administración</p>
          </div>
        </Link>

        <p className="mb-3 px-3 text-[11px] font-bold uppercase tracking-[0.16em] text-gray-500">Gestión</p>
        <nav className="space-y-2" aria-label="Navegación administrativa">
          {renderNavigation()}
        </nav>

        <div className="mt-auto border-t border-white/10 pt-5">
          <Link to="/perfil" className="mb-3 flex items-center gap-3 rounded-xl p-3 transition-all duration-200 hover:bg-gray-700">
            {user?.fotoUrl ? (
              <img src={user.fotoUrl} alt="Perfil del administrador" className="h-10 w-10 rounded-full object-cover" />
            ) : (
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-cyan-600 text-sm font-black text-white">
                {initials}
              </div>
            )}
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-bold text-white">{user?.nombre || 'Administrador'}</p>
              <p className="truncate text-xs text-gray-400">Administrador</p>
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
            <span className="font-display text-sm font-bold text-white">Panel administrativo</span>
          </div>
          <nav className="flex gap-2 overflow-x-auto pb-1" aria-label="Navegación administrativa móvil">
            {renderNavigation(true)}
          </nav>
        </div>
        <div className="mx-auto w-full max-w-[1600px] px-4 py-6 sm:px-6 lg:px-10 lg:py-10">
          {children}
        </div>
      </main>
    </div>
  )
}

export default AdminLayout
