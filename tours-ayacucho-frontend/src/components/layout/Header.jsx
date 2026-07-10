import { useState, useEffect } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { fetchSiteSettings, getSiteSettings } from '../../utils/siteSettings'
import {
  Bars3Icon,
  XMarkIcon,
  MapPinIcon,
  UserCircleIcon,
  ArrowRightOnRectangleIcon,
  ShieldCheckIcon,
  CalendarDaysIcon,
  HomeIcon,
  Squares2X2Icon,
} from '@heroicons/react/24/outline'

const Header = () => {
  const { isAuthenticated, isAdmin, user, logout } = useAuth()
  const navigate = useNavigate()
  const [siteSettings, setSiteSettings] = useState(getSiteSettings)
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [isScrolled, setIsScrolled] = useState(false)
  const [isProfileOpen, setIsProfileOpen] = useState(false)

  useEffect(() => {
    const handleScroll = () => setIsScrolled(window.scrollY > 20)
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

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

  const handleLogout = () => {
    logout()
    setIsProfileOpen(false)
    setIsMenuOpen(false)
    navigate('/login', { replace: true })
  }

  const navLinks = [
    ...(isAdmin
      ? [{ to: '/admin', label: 'Panel Admin', icon: ShieldCheckIcon }]
      : [{ to: '/', label: 'Inicio', icon: HomeIcon }]),
    ...(isAuthenticated && !isAdmin
      ? [
          { to: '/paquetes', label: 'Paquetes', icon: Squares2X2Icon },
          { to: '/mis-reservas', label: 'Mis Reservas', icon: CalendarDaysIcon },
          { to: '/perfil', label: 'Perfil', icon: UserCircleIcon },
        ]
      : []),
    ...(isAuthenticated && isAdmin
      ? [{ to: '/perfil', label: 'Perfil', icon: UserCircleIcon }]
      : []),
  ]

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
        isScrolled
          ? 'bg-dark-900/95 backdrop-blur-xl border-b border-white/10 shadow-lg shadow-black/20'
          : 'bg-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16 md:h-20">
          {/* Logo */}
          <Link
            to="/"
            className="flex items-center gap-2 group"
            onClick={() => setIsMenuOpen(false)}
          >
            <div className="flex h-9 w-9 items-center justify-center overflow-hidden rounded-xl bg-teal-gradient shadow-teal transition-all duration-300 group-hover:shadow-teal-lg">
              {siteSettings.logoUrl ? (
                <img src={siteSettings.logoUrl} alt={siteSettings.companyName} className="h-full w-full object-cover" />
              ) : (
                <MapPinIcon className="w-5 h-5 text-white" />
              )}
            </div>
            <div className="flex flex-col leading-none">
              <span className="font-display font-bold text-white text-sm tracking-wide">
                {siteSettings.companyName}
              </span>
              <span className="font-display font-bold text-xs tracking-widest gradient-text">
                {siteSettings.companySubtitle}
              </span>
            </div>
          </Link>

          {/* Desktop Nav */}
          <nav className="hidden md:flex items-center gap-1">
            {navLinks.map(({ to, label }) => (
              <NavLink
                key={to}
                to={to}
                end={to === '/'}
                className={({ isActive }) =>
                  `px-5 py-2.5 rounded-xl text-sm font-bold transition-all duration-200 ${
                    isActive
                      ? 'text-white bg-vivid-nav shadow-teal'
                      : 'text-gray-300 hover:text-white hover:bg-primary-500/15 hover:shadow-teal'
                  }`
                }
              >
                {label}
              </NavLink>
            ))}
          </nav>

          {/* Desktop Auth */}
          <div className="hidden md:flex items-center gap-3">
            {isAuthenticated ? (
              <div className="relative">
                <button
                  id="profile-menu-btn"
                  onClick={() => setIsProfileOpen(!isProfileOpen)}
                  className="flex items-center gap-2 px-3 py-2 rounded-xl border border-primary-400/20 bg-white/[0.04] hover:bg-primary-500/15 hover:border-primary-300/60 hover:shadow-teal transition-all duration-200 group"
                >
                  <div className="flex h-8 w-8 items-center justify-center overflow-hidden rounded-full bg-teal-gradient text-sm font-bold text-white shadow-teal">
                    {user?.fotoUrl ? (
                      <img src={user.fotoUrl} alt={user?.nombre ?? user?.correo ?? 'Usuario'} className="h-full w-full object-cover" />
                    ) : (
                      user?.correo?.charAt(0).toUpperCase() || 'U'
                    )}
                  </div>
                  <span className="text-gray-300 text-sm font-medium max-w-[120px] truncate">
                    {user?.correo?.split('@')[0]}
                  </span>
                  <svg
                    className={`w-4 h-4 text-gray-400 transition-transform duration-200 ${isProfileOpen ? 'rotate-180' : ''}`}
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>

                {isProfileOpen && (
                  <div className="absolute right-0 top-full z-50 mt-2 w-64 glass-card p-2 animate-slide-down border-primary-400/30 shadow-teal-lg">
                    {isAdmin && (
                      <Link
                        to="/admin"
                        className="flex items-center gap-3 px-4 py-3 rounded-xl text-gray-200 hover:text-white hover:bg-primary-500/15 transition-all duration-200 text-sm font-semibold"
                        onClick={() => setIsProfileOpen(false)}
                      >
                        <ShieldCheckIcon className="w-4 h-4 text-primary-400" />
                        Panel Admin
                      </Link>
                    )}
                    <Link
                      to="/perfil"
                      className="flex items-center gap-3 px-4 py-3 rounded-xl text-gray-200 hover:text-white hover:bg-primary-500/15 transition-all duration-200 text-sm font-semibold"
                      onClick={() => setIsProfileOpen(false)}
                    >
                      <UserCircleIcon className="w-4 h-4 text-primary-400" />
                      Mi Perfil
                    </Link>
                    {!isAdmin && (
                      <Link
                        to="/mis-reservas"
                        className="flex items-center gap-3 px-4 py-3 rounded-xl text-gray-200 hover:text-white hover:bg-primary-500/15 transition-all duration-200 text-sm font-semibold"
                        onClick={() => setIsProfileOpen(false)}
                      >
                        <CalendarDaysIcon className="w-4 h-4 text-primary-400" />
                        Mis Reservas
                      </Link>
                    )}
                    <div className="border-t border-white/10 my-1" />
                    <button
                      id="logout-btn"
                      type="button"
                      onClick={handleLogout}
                      className="flex w-full items-center gap-3 rounded-xl px-4 py-3 text-left text-sm font-bold text-white transition-all duration-200 bg-red-500/15 border border-red-400/25 hover:bg-red-500 hover:border-red-300 hover:shadow-[0_10px_30px_rgba(239,68,68,0.35)]"
                    >
                      <ArrowRightOnRectangleIcon className="w-4 h-4" />
                      Cerrar Sesion
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <>
                <Link to="/login" className="btn-ghost text-sm">
                  Iniciar Sesion
                </Link>
                <Link to="/register" className="btn-primary text-sm">
                  Registrarse
                </Link>
              </>
            )}
          </div>

          {/* Mobile Menu Button */}
          <button
            id="mobile-menu-btn"
            className="md:hidden p-2 rounded-lg text-gray-400 hover:text-white hover:bg-white/5 transition-all duration-200"
            onClick={() => setIsMenuOpen(!isMenuOpen)}
          >
            {isMenuOpen ? <XMarkIcon className="w-6 h-6" /> : <Bars3Icon className="w-6 h-6" />}
          </button>
        </div>

        {/* Mobile Menu */}
        {isMenuOpen && (
          <div className="md:hidden border-t border-white/10 bg-dark-900/[0.98] backdrop-blur-xl animate-slide-down pb-4">
            <nav className="flex flex-col gap-1 py-3">
              {navLinks.map(({ to, label, icon: Icon }) => (
                <NavLink
                  key={to}
                  to={to}
                  end={to === '/'}
                  className={({ isActive }) =>
                    `flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all duration-200 mx-2 ${
                      isActive
                        ? 'text-white bg-vivid-nav shadow-teal'
                        : 'text-gray-300 hover:text-white hover:bg-primary-500/15'
                    }`
                  }
                  onClick={() => setIsMenuOpen(false)}
                >
                  <Icon className="w-5 h-5" />
                  {label}
                </NavLink>
              ))}
            </nav>
            <div className="border-t border-white/10 pt-3 px-4 flex flex-col gap-2">
              {isAuthenticated ? (
                <button
                  onClick={() => { handleLogout(); setIsMenuOpen(false) }}
                  className="btn-danger w-full text-sm"
                >
                  <ArrowRightOnRectangleIcon className="w-4 h-4" />
                  Cerrar Sesion
                </button>
              ) : (
                <>
                  <Link to="/login" className="btn-secondary w-full text-sm text-center" onClick={() => setIsMenuOpen(false)}>
                    Iniciar Sesion
                  </Link>
                  <Link to="/register" className="btn-primary w-full text-sm text-center" onClick={() => setIsMenuOpen(false)}>
                    Registrarse
                  </Link>
                </>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Overlay to close profile dropdown */}
      {isProfileOpen && (
        <div
          className="fixed inset-0 z-30"
          onClick={() => setIsProfileOpen(false)}
        />
      )}
    </header>
  )
}

export default Header

