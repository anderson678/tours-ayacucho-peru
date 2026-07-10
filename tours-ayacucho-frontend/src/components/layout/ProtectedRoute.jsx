import { Navigate, Link, useLocation } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { ShieldExclamationIcon } from '@heroicons/react/24/outline'

const ProtectedRoute = ({ children, requireAdmin = false, requireClient = false }) => {
  const { isAuthenticated, isAdmin, loading } = useAuth()
  const location = useLocation()

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="flex flex-col items-center gap-4">
          <div className="spinner w-12 h-12" />
          <p className="text-gray-400 font-medium animate-pulse">Verificando sesion...</p>
        </div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  if (requireAdmin && !isAdmin) {
    return (
      <div className="page-wrapper flex items-center justify-center">
        <div className="glass-card p-10 max-w-md text-center">
          <div className="w-16 h-16 rounded-full bg-red-500/10 border border-red-500/30 flex items-center justify-center mx-auto mb-5">
            <ShieldExclamationIcon className="w-9 h-9 text-red-400" />
          </div>
          <h1 className="font-display text-2xl font-bold text-white mb-2">Acceso denegado</h1>
          <p className="text-gray-400 mb-6">
            Esta seccion esta restringida a usuarios con rol Administrador.
          </p>
          <Link to="/" className="btn-secondary">
            Volver al inicio
          </Link>
        </div>
      </div>
    )
  }

  if (requireClient && isAdmin) {
    return <Navigate to="/admin" replace />
  }

  return children
}

export default ProtectedRoute
