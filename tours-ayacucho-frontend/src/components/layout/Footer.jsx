import { Link } from 'react-router-dom'
import { MapPinIcon, PhoneIcon, EnvelopeIcon, GlobeAltIcon } from '@heroicons/react/24/outline'
import { useAuth } from '../../context/AuthContext'

const Footer = () => {
  const { isAuthenticated, isAdmin } = useAuth()
  const currentYear = new Date().getFullYear()
  const navigationLinks = isAdmin
    ? [
        { to: '/admin', label: 'Panel Admin' },
        { to: '/perfil', label: 'Mi Perfil' },
      ]
    : [
        { to: '/', label: 'Inicio' },
        ...(!isAuthenticated
          ? [
              { to: '/register', label: 'Registrarse' },
              { to: '/login', label: 'Iniciar Sesion' },
            ]
          : [
              { to: '/mis-reservas', label: 'Mis Reservas' },
              { to: '/perfil', label: 'Mi Perfil' },
            ]),
      ]

  return (
    <footer className="border-t border-white/10 mt-auto">
      <div
        className="py-12 px-4"
        style={{
          background: 'linear-gradient(180deg, transparent, rgba(0, 191, 191, 0.03))',
        }}
      >
        <div className="max-w-7xl mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-10">
            {/* Brand */}
            <div className="flex flex-col gap-4">
              <div className="flex items-center gap-2">
                <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-teal-gradient shadow-teal">
                  <MapPinIcon className="w-5 h-5 text-white" />
                </div>
                <div className="flex flex-col leading-none">
                  <span className="font-display font-bold text-white text-sm tracking-wide">TOURS</span>
                  <span className="font-display font-bold text-xs tracking-widest gradient-text">AYACUCHO PERÚ</span>
                </div>
              </div>
              <p className="text-gray-400 text-sm leading-relaxed max-w-xs">
                Tu puerta de entrada a la maravilla cultural e histórica de Ayacucho. 
                Vivencia Huamanga con tours únicos y memorables.
              </p>
              <div className="flex gap-3 mt-2">
                {['facebook', 'instagram', 'twitter'].map((social) => (
                  <a
                    key={social}
                    href="#"
                    aria-label={social}
                    className="w-8 h-8 rounded-lg bg-white/5 border border-white/10 flex items-center justify-center
                               text-gray-400 hover:text-primary-400 hover:border-primary-500/50 hover:bg-primary-500/10
                               transition-all duration-200"
                  >
                    <GlobeAltIcon className="w-4 h-4" />
                  </a>
                ))}
              </div>
            </div>

            {/* Links */}
            <div>
              <h3 className="font-display font-semibold text-white mb-4 text-sm uppercase tracking-wider">Navegación</h3>
              <ul className="flex flex-col gap-2">
                {navigationLinks.map(({ to, label }) => (
                  <li key={to}>
                    <Link
                      to={to}
                      className="text-gray-400 hover:text-primary-400 text-sm transition-colors duration-200 flex items-center gap-2"
                    >
                      <span className="w-1 h-1 rounded-full bg-primary-500/50" />
                      {label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Contact */}
            <div>
              <h3 className="font-display font-semibold text-white mb-4 text-sm uppercase tracking-wider">Contacto</h3>
              <ul className="flex flex-col gap-3">
                <li className="flex items-start gap-3 text-sm text-gray-400">
                  <MapPinIcon className="w-4 h-4 text-primary-400 mt-0.5 shrink-0" />
                  <span>Jr. 28 de Julio 123, Ayacucho, Perú</span>
                </li>
                <li className="flex items-center gap-3 text-sm text-gray-400">
                  <PhoneIcon className="w-4 h-4 text-primary-400 shrink-0" />
                  <span>+51 966 123 456</span>
                </li>
                <li className="flex items-center gap-3 text-sm text-gray-400">
                  <EnvelopeIcon className="w-4 h-4 text-primary-400 shrink-0" />
                  <span>contacto@toursayacucho.pe</span>
                </li>
              </ul>

              <div className="mt-6 p-3 rounded-xl bg-primary-500/10 border border-primary-500/20">
                <p className="text-xs text-primary-400 font-medium">🕐 Horario de atención</p>
                <p className="text-xs text-gray-400 mt-1">Lun - Sáb: 8:00 AM – 7:00 PM</p>
                <p className="text-xs text-gray-400">Dom: 9:00 AM – 2:00 PM</p>
              </div>
            </div>
          </div>

          {/* Bottom Bar */}
          <div className="border-t border-white/10 mt-10 pt-6 flex flex-col md:flex-row justify-between items-center gap-3">
            <p className="text-gray-500 text-xs">
              © {currentYear} Tours Ayacucho Perú. Todos los derechos reservados.
            </p>
            <div className="flex gap-4">
              <a href="#" className="text-gray-500 hover:text-gray-400 text-xs transition-colors duration-200">
                Política de Privacidad
              </a>
              <a href="#" className="text-gray-500 hover:text-gray-400 text-xs transition-colors duration-200">
                Términos de Servicio
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  )
}

export default Footer

