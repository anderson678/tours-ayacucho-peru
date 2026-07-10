import { Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import Header from './components/layout/Header'
import Footer from './components/layout/Footer'
import ProtectedRoute from './components/layout/ProtectedRoute'

// Pages
import Home from './pages/Home'
import Packages from './pages/Packages'
import Login from './pages/Login'
import Register from './pages/Register'
import PackageDetail from './pages/PackageDetail'
import CreateReservation from './pages/CreateReservation'
import MyReservations from './pages/MyReservations'
import Payment from './pages/Payment'
import Reschedule from './pages/Reschedule'
import Profile from './pages/Profile'
import AdminDashboard from './pages/AdminDashboard'

function App() {
  return (
    <div className="min-h-screen flex flex-col">
      {/* Toast notifications */}
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            background: 'rgba(15, 25, 35, 0.95)',
            color: '#fff',
            border: '1px solid rgba(0, 191, 191, 0.2)',
            borderRadius: '12px',
            backdropFilter: 'blur(20px)',
            fontSize: '14px',
            fontFamily: 'Inter, system-ui, sans-serif',
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.5)',
          },
          success: {
            iconTheme: { primary: '#00bfbf', secondary: '#fff' },
            style: {
              border: '1px solid rgba(0, 191, 191, 0.3)',
            },
          },
          error: {
            iconTheme: { primary: '#ef4444', secondary: '#fff' },
            style: {
              border: '1px solid rgba(239, 68, 68, 0.3)',
            },
          },
        }}
      />

      {/* Navigation */}
      <Header />

      {/* Main content */}
      <div className="flex-1">
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/package/:id" element={<PackageDetail />} />
          <Route
            path="/paquetes"
            element={
              <ProtectedRoute requireClient>
                <Packages />
              </ProtectedRoute>
            }
          />

          {/* Protected routes â€“ authenticated users */}
          <Route
            path="/reservar/:packageId"
            element={
              <ProtectedRoute requireClient>
                <CreateReservation />
              </ProtectedRoute>
            }
          />
          <Route
            path="/mis-reservas"
            element={
              <ProtectedRoute requireClient>
                <MyReservations />
              </ProtectedRoute>
            }
          />
          <Route
            path="/pago/:reservaId"
            element={
              <ProtectedRoute requireClient>
                <Payment />
              </ProtectedRoute>
            }
          />
          <Route
            path="/reprogramar/:reservaId"
            element={
              <ProtectedRoute requireClient>
                <Reschedule />
              </ProtectedRoute>
            }
          />
          <Route
            path="/perfil"
            element={
              <ProtectedRoute>
                <Profile />
              </ProtectedRoute>
            }
          />

          {/* Admin route */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute requireAdmin>
                <AdminDashboard />
              </ProtectedRoute>
            }
          />

          {/* Catch-all â†’ Home */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>

      {/* Footer */}
      <Footer />
    </div>
  )
}

export default App
