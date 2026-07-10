import { useCallback, useEffect, useMemo, useState } from 'react'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import { useAuth } from '../context/AuthContext'
import { defaultSiteSettings, fetchSiteSettings, getSiteSettings, updateSiteSettings } from '../utils/siteSettings'
import toast from 'react-hot-toast'
import {
  ArrowDownTrayIcon,
  ArrowPathIcon,
  BanknotesIcon,
  CalendarDaysIcon,
  ChartBarIcon,
  CheckCircleIcon,
  ClipboardDocumentListIcon,
  DocumentChartBarIcon,
  ExclamationCircleIcon,
  ExclamationTriangleIcon,
  PhotoIcon,
  PencilSquareIcon,
  PlusIcon,
  ShieldCheckIcon,
  TrophyIcon,
  TrashIcon,
  UserMinusIcon,
  UserPlusIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline'

const emptyPackageForm = {
  nombre: '',
  destino: 'Ayacucho',
  descripcion: '',
  imagenUrl: '',
  precioUnitario: '',
  capacidadTotal: '',
  asientosDisp: '',
  fechaInicio: '',
  fechaFin: '',
  activo: true,
}

const REPORT_STATUS_OPTIONS = [
  { value: '', label: 'Todos los estados' },
  { value: 'PENDIENTE_PAGO', label: 'Pendiente pago' },
  { value: 'CONFIRMADA', label: 'Confirmada' },
  { value: 'REPROGRAMADA', label: 'Reprogramada' },
  { value: 'COMPLETADA', label: 'Completada' },
  { value: 'CANCELADA', label: 'Cancelada' },
]

const RESERVATION_STATUS_OPTIONS = REPORT_STATUS_OPTIONS

const toDateInput = (value) => {
  if (!value) return ''
  return new Date(value).toISOString().slice(0, 10)
}

const today = new Date()
const firstDay = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().slice(0, 10)
const todayValue = today.toISOString().slice(0, 10)

const formatShortDate = (value) => {
  if (!value) return 'N/A'
  return new Date(value).toLocaleDateString('es-PE', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

const formatCurrency = (value) => `S/ ${Number(value || 0).toFixed(2)}`

const getReservationBadgeClass = (estado) => {
  if (estado === 'CONFIRMADA' || estado === 'COMPLETADA') return 'badge-success'
  if (estado === 'PENDIENTE_PAGO' || estado === 'REPROGRAMADA') return 'badge-warning'
  if (estado === 'CANCELADA') return 'badge-danger'
  return 'badge-gray'
}

const adminLoadTargets = [
  {
    key: 'packages',
    label: 'paquetes',
    request: () => apiClient.get('/admin/packages'),
    fallback: 'No se pudieron cargar los paquetes.',
  },
  {
    key: 'clients',
    label: 'clientes',
    request: () => apiClient.get('/admin/clients'),
    fallback: 'No se pudieron cargar los clientes.',
  },
]

const StatCard = ({ label, value, icon: Icon, tone }) => (
  <div className="glass-card p-5">
    <div className="flex items-center justify-between">
      <div>
        <p className="text-gray-400 text-sm">{label}</p>
        <p className={`font-display text-3xl font-black mt-1 ${tone}`}>{value}</p>
      </div>
      <div className="w-11 h-11 rounded-xl bg-white/5 flex items-center justify-center">
        <Icon className={`w-6 h-6 ${tone}`} />
      </div>
    </div>
  </div>
)

const AdminDashboard = () => {
  const { user } = useAuth()
  const [activeTab, setActiveTab] = useState('reservas')
  const [packages, setPackages] = useState([])
  const [clients, setClients] = useState([])
  const [reservations, setReservations] = useState([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [sectionErrors, setSectionErrors] = useState({})
  const [packageFormError, setPackageFormError] = useState('')
  const [editingPackageId, setEditingPackageId] = useState(null)
  const [packageForm, setPackageForm] = useState(emptyPackageForm)
  const [siteForm, setSiteForm] = useState(getSiteSettings)
  const [reportLoading, setReportLoading] = useState(null)
  const [reportForm, setReportForm] = useState({
    from: firstDay,
    to: todayValue,
    format: 'pdf',
    estado: '',
    paqueteId: '',
  })
  const [reservationFilters, setReservationFilters] = useState({
    estado: '',
    paqueteId: '',
  })

  const loadData = useCallback(async () => {
    setLoading(true)
    setSectionErrors({})

    const reservationParams = {}
    if (reservationFilters.estado) reservationParams.estado = reservationFilters.estado
    if (reservationFilters.paqueteId) reservationParams.paqueteId = reservationFilters.paqueteId

    const targets = [
      ...adminLoadTargets,
      {
        key: 'reservations',
        label: 'reservas',
        request: () => apiClient.get('/admin/reservations', { params: reservationParams }),
        fallback: 'No se pudieron cargar las reservas.',
      },
    ]

    const results = await Promise.allSettled(targets.map((target) => target.request()))
    const nextErrors = {}

    results.forEach((result, index) => {
      const target = targets[index]
      if (result.status === 'fulfilled') {
        const data = Array.isArray(result.value.data) ? result.value.data : []
        if (target.key === 'packages') setPackages(data)
        if (target.key === 'clients') setClients(data)
        if (target.key === 'reservations') setReservations(data)
        return
      }

      nextErrors[target.key] = getApiErrorMessage(result.reason, target.fallback)
    })

    setSectionErrors(nextErrors)
    setLoading(false)

    if (Object.keys(nextErrors).length > 0) {
      toast.error('Algunas secciones del panel no se pudieron cargar.')
    }
  }, [reservationFilters.estado, reservationFilters.paqueteId])

  useEffect(() => {
    loadData()
  }, [loadData])

  useEffect(() => {
    fetchSiteSettings()
      .then(setSiteForm)
      .catch(() => {
        toast.error('No se pudo cargar la configuracion de portada desde la API.')
      })
  }, [])

  const stats = useMemo(() => {
    const activePackages = packages.filter((pkg) => pkg.activo !== false).length
    const activeClients = clients.filter((client) => client.estado === 'Activo').length
    const inactiveClients = clients.filter((client) => client.estado === 'Inactivo').length
    const availableSeats = packages.reduce((sum, pkg) => sum + (Number(pkg.asientosDisp) || 0), 0)
    const totalReservations = reservations.length
    const confirmedRevenue = reservations
      .filter((item) => item.estado !== 'PENDIENTE_PAGO' && item.estado !== 'CANCELADA')
      .reduce((sum, item) => sum + (Number(item.montoTotal) || 0), 0)
    const pendingReservations = reservations.filter((item) => item.estado === 'PENDIENTE_PAGO').length
    const reservationsByStatus = reservations.reduce((acc, item) => {
      acc[item.estado] = (acc[item.estado] ?? 0) + 1
      return acc
    }, {})
    const packagePerformance = Object.values(reservations.reduce((acc, item) => {
      const key = item.paqueteId
      if (!acc[key]) {
        acc[key] = {
          paqueteId: item.paqueteId,
          nombre: item.paqueteNombre,
          destino: item.paqueteDestino,
          reservas: 0,
          asientos: 0,
          monto: 0,
        }
      }
      acc[key].reservas += 1
      acc[key].asientos += Number(item.cantAsientos) || 0
      if (item.estado !== 'PENDIENTE_PAGO' && item.estado !== 'CANCELADA') {
        acc[key].monto += Number(item.montoTotal) || 0
      }
      return acc
    }, {})).sort((a, b) => b.asientos - a.asientos).slice(0, 3)
    const lowStockPackages = packages
      .filter((pkg) => pkg.activo !== false && Number(pkg.asientosDisp) <= Math.max(3, Math.ceil(Number(pkg.capacidadTotal || 0) * 0.2)))
      .sort((a, b) => Number(a.asientosDisp) - Number(b.asientosDisp))
      .slice(0, 4)

    return {
      activePackages,
      activeClients,
      inactiveClients,
      availableSeats,
      totalReservations,
      confirmedRevenue,
      pendingReservations,
      reservationsByStatus,
      packagePerformance,
      lowStockPackages,
    }
  }, [packages, clients, reservations])

  const hasSectionErrors = Object.keys(sectionErrors).length > 0

  const resetPackageForm = () => {
    setEditingPackageId(null)
    setPackageForm(emptyPackageForm)
    setPackageFormError('')
  }

  const handleSiteFieldChange = (event) => {
    const { name, value } = event.target
    setSiteForm((prev) => ({
      ...prev,
      [name]: value,
    }))
  }

  const handleHeroImageChange = (index, field, value) => {
    setSiteForm((prev) => ({
      ...prev,
      heroImages: prev.heroImages.map((item, itemIndex) => (
        itemIndex === index ? { ...item, [field]: value } : item
      )),
    }))
  }

  const handleSaveSiteSettings = async (event) => {
    event?.preventDefault()
    try {
      const updated = await updateSiteSettings(siteForm)
      setSiteForm(updated)
      toast.success('Portada actualizada correctamente')
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo guardar la portada'))
    }
  }

  const handleRestoreSiteSettings = async () => {
    try {
      const updated = await updateSiteSettings(defaultSiteSettings)
      setSiteForm(updated)
      toast.success('Portada restaurada')
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo restaurar la portada'))
    }
  }

  const startEditPackage = (pkg) => {
    setActiveTab('paquetes')
    setEditingPackageId(pkg.paqueteId)
    setPackageForm({
      nombre: pkg.nombre ?? '',
      destino: pkg.destino ?? 'Ayacucho',
      descripcion: pkg.descripcion ?? '',
      imagenUrl: pkg.imagenUrl ?? '',
      precioUnitario: pkg.precioUnitario ?? '',
      capacidadTotal: pkg.capacidadTotal ?? '',
      asientosDisp: pkg.asientosDisp ?? '',
      fechaInicio: toDateInput(pkg.fechaInicio),
      fechaFin: toDateInput(pkg.fechaFin),
      activo: pkg.activo !== false,
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handlePackageChange = (event) => {
    const { name, value, type, checked } = event.target
    setPackageForm((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
  }

  const buildPackagePayload = () => ({
    nombre: packageForm.nombre.trim(),
    destino: packageForm.destino.trim(),
    descripcion: packageForm.descripcion.trim() || null,
    imagenUrl: packageForm.imagenUrl.trim() || null,
    precioUnitario: Number(packageForm.precioUnitario),
    capacidadTotal: Number(packageForm.capacidadTotal),
    asientosDisp: Number(packageForm.asientosDisp),
    fechaInicio: packageForm.fechaInicio,
    fechaFin: packageForm.fechaFin,
    activo: packageForm.activo,
  })

  const validatePackageForm = () => {
    const price = Number(packageForm.precioUnitario)
    const capacity = Number(packageForm.capacidadTotal)
    const seats = Number(packageForm.asientosDisp)

    if (!packageForm.nombre.trim()) return 'El nombre del paquete es requerido.'
    if (!packageForm.destino.trim()) return 'El destino del paquete es requerido.'
    if (!Number.isFinite(price) || price <= 0) return 'El precio debe ser mayor a S/ 0.00.'
    if (!Number.isInteger(capacity) || capacity <= 0) return 'La capacidad total debe ser un entero positivo.'
    if (!Number.isInteger(seats) || seats < 0) return 'Los asientos disponibles no pueden ser negativos.'
    if (seats > capacity) return 'Los asientos disponibles no pueden exceder la capacidad total.'
    if (!packageForm.fechaInicio) return 'La fecha de inicio es requerida.'
    if (!packageForm.fechaFin) return 'La fecha de fin es requerida.'
    if (packageForm.fechaFin < packageForm.fechaInicio) return 'La fecha de fin debe ser posterior o igual a la fecha de inicio.'

    return ''
  }

  const handleSavePackage = async (event) => {
    event.preventDefault()
    const validationError = validatePackageForm()
    if (validationError) {
      setPackageFormError(validationError)
      toast.error(validationError)
      return
    }

    setSaving(true)
    setPackageFormError('')
    try {
      const payload = buildPackagePayload()
      if (editingPackageId) {
        await apiClient.put(`/admin/packages/${editingPackageId}`, payload)
        toast.success('Paquete actualizado correctamente')
      } else {
        await apiClient.post('/admin/packages', payload)
        toast.success('Paquete creado correctamente')
      }
      resetPackageForm()
      await loadData()
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo guardar el paquete'))
    } finally {
      setSaving(false)
    }
  }

  const handleDeactivatePackage = async (packageId) => {
    if (!window.confirm('Deseas desactivar este paquete turistico?')) return
    setSaving(true)
    try {
      await apiClient.delete(`/admin/packages/${packageId}`)
      toast.success('Paquete desactivado')
      await loadData()
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo desactivar el paquete'))
    } finally {
      setSaving(false)
    }
  }

  const handleClientStatus = async (clientId, estado) => {
    const action = estado === 'Activo' ? 'activar' : 'desactivar'
    if (!window.confirm(`Deseas ${action} esta cuenta de cliente?`)) return

    setSaving(true)
    try {
      await apiClient.patch(`/admin/clients/${clientId}/status`, { estado })
      toast.success(estado === 'Activo' ? 'Cliente activado' : 'Cliente desactivado')
      await loadData()
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo actualizar el estado del cliente'))
    } finally {
      setSaving(false)
    }
  }

  const downloadBlob = (blob, filename) => {
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  }

  const handleDownloadReport = async (type) => {
    if (!reportForm.from || !reportForm.to) {
      toast.error('Selecciona el rango de fechas del reporte')
      return
    }

    if (reportForm.from > reportForm.to) {
      toast.error('La fecha de inicio debe ser anterior o igual a la fecha de fin')
      return
    }

    setReportLoading(type)
    try {
      const params = {
        from: reportForm.from,
        to: reportForm.to,
        format: reportForm.format,
      }

      if (reportForm.estado) params.estado = reportForm.estado
      if (reportForm.paqueteId) params.paqueteId = reportForm.paqueteId

      const response = await apiClient.get(`/admin/reports/${type}`, {
        params,
        responseType: 'blob',
      })
      downloadBlob(response.data, `reporte-${type}.${reportForm.format}`)
      toast.success('Reporte descargado correctamente')
    } catch (err) {
      toast.error(getApiErrorMessage(err, 'No se pudo descargar el reporte'))
    } finally {
      setReportLoading(null)
    }
  }

  return (
    <div className="page-wrapper">
      <div className="container-main animate-fade-in">
        <header className="mb-8 flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <div className="flex items-center gap-3 mb-3">
              <div className="w-10 h-10 rounded-xl bg-teal-gradient flex items-center justify-center shadow-teal">
                <ShieldCheckIcon className="w-5 h-5 text-white" />
              </div>
              <div>
                <h1 className="font-display text-3xl font-bold text-white">Panel Administrativo</h1>
                <p className="text-gray-400 text-sm">
                  TOURS AYACUCHO PERU · {user?.correo}
                </p>
              </div>
            </div>
          </div>
          <button type="button" onClick={loadData} className="btn-secondary self-start lg:self-auto" disabled={loading}>
            <ArrowPathIcon className="w-4 h-4" />
            Actualizar
          </button>
        </header>

        {loading ? (
          <div className="flex flex-col items-center py-24 gap-4">
            <div className="spinner w-12 h-12" />
            <p className="text-gray-400 animate-pulse">Cargando panel...</p>
          </div>
        ) : (
          <>
            {hasSectionErrors && (
              <section className="mb-6 rounded-lg border border-amber-400/30 bg-amber-400/[0.07] p-4">
                <div className="flex items-start gap-3">
                  <ExclamationCircleIcon className="mt-0.5 h-5 w-5 flex-none text-amber-300" />
                  <div>
                    <p className="font-semibold text-amber-100">El panel cargo con observaciones</p>
                    <div className="mt-2 grid gap-1 text-sm text-amber-50/80">
                      {Object.entries(sectionErrors).map(([key, message]) => (
                        <p key={key}>{message}</p>
                      ))}
                    </div>
                  </div>
                </div>
              </section>
            )}

            <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
              <StatCard label="Reservas registradas" value={stats.totalReservations} icon={ClipboardDocumentListIcon} tone="text-primary-400" />
              <StatCard label="Ingresos confirmados" value={formatCurrency(stats.confirmedRevenue)} icon={BanknotesIcon} tone="text-emerald-400" />
              <StatCard label="Pendientes de pago" value={stats.pendingReservations} icon={ExclamationTriangleIcon} tone="text-amber-400" />
              <StatCard label="Asientos disponibles" value={stats.availableSeats} icon={ChartBarIcon} tone="text-blue-400" />
            </section>

            <section className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-8">
              <div className="glass-card p-5">
                <div className="mb-4 flex items-center gap-3">
                  <TrophyIcon className="h-5 w-5 text-gold-400" />
                  <h2 className="font-display text-lg font-bold text-white">Tours mas reservados</h2>
                </div>
                <div className="space-y-3">
                  {stats.packagePerformance.length === 0 ? (
                    <p className="text-sm text-gray-400">Aun no hay reservas registradas.</p>
                  ) : (
                    stats.packagePerformance.map((item, index) => (
                      <div key={item.paqueteId} className="rounded-xl border border-white/10 bg-white/[0.03] p-3">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <p className="text-xs font-bold text-primary-400">#{index + 1}</p>
                            <p className="text-sm font-semibold text-white">{item.nombre}</p>
                            <p className="text-xs text-gray-500">{item.destino}</p>
                          </div>
                          <div className="text-right">
                            <p className="text-sm font-bold text-white">{item.asientos} asientos</p>
                            <p className="text-xs text-emerald-400">{formatCurrency(item.monto)}</p>
                          </div>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>

              <div className="glass-card p-5">
                <div className="mb-4 flex items-center gap-3">
                  <ClipboardDocumentListIcon className="h-5 w-5 text-primary-400" />
                  <h2 className="font-display text-lg font-bold text-white">Estado de reservas</h2>
                </div>
                <div className="space-y-3">
                  {REPORT_STATUS_OPTIONS.filter((option) => option.value).map((option) => (
                    <div key={option.value} className="flex items-center justify-between rounded-xl border border-white/10 bg-white/[0.03] px-3 py-2">
                      <span className="text-sm text-gray-300">{option.label}</span>
                      <span className={getReservationBadgeClass(option.value)}>{stats.reservationsByStatus[option.value] ?? 0}</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="glass-card p-5">
                <div className="mb-4 flex items-center gap-3">
                  <ExclamationTriangleIcon className="h-5 w-5 text-amber-400" />
                  <h2 className="font-display text-lg font-bold text-white">Cupos criticos</h2>
                </div>
                <div className="space-y-3">
                  {stats.lowStockPackages.length === 0 ? (
                    <p className="text-sm text-gray-400">No hay paquetes activos con baja disponibilidad.</p>
                  ) : (
                    stats.lowStockPackages.map((pkg) => (
                      <button
                        key={pkg.paqueteId}
                        type="button"
                        onClick={() => startEditPackage(pkg)}
                        className="w-full rounded-xl border border-amber-400/20 bg-amber-400/[0.06] p-3 text-left transition hover:border-amber-300/50 hover:bg-amber-400/10"
                      >
                        <p className="text-sm font-semibold text-white">{pkg.nombre}</p>
                        <p className="text-xs text-amber-200">{pkg.asientosDisp} de {pkg.capacidadTotal} cupos disponibles</p>
                      </button>
                    ))
                  )}
                </div>
              </div>
            </section>

            <div className="mb-8 flex flex-wrap gap-2">
              {[
                ['reservas', 'Reservas'],
                ['paquetes', 'Paquetes'],
                ['clientes', 'Clientes'],
                ['portada', 'Portada'],
                ['reportes', 'Reportes'],
              ].map(([value, label]) => (
                <button
                  key={value}
                  type="button"
                  onClick={() => setActiveTab(value)}
                  className={activeTab === value ? 'btn-primary text-sm' : 'btn-secondary text-sm'}
                >
                  {label}
                </button>
              ))}
            </div>

            {activeTab === 'reservas' && (
              <section className="glass-card overflow-hidden">
                <div className="p-5 border-b border-white/10 flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
                  <div>
                    <div className="flex items-center gap-3">
                      <ClipboardDocumentListIcon className="w-6 h-6 text-primary-400" />
                      <h2 className="font-display text-xl font-bold text-white">Historial de reservas</h2>
                    </div>
                    <p className="text-sm text-gray-400 mt-1">Consulta quien reservo, que paquete eligio, su pago y el monto comprometido.</p>
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-[180px_260px_auto] gap-3">
                    <select
                      className="input-field"
                      value={reservationFilters.estado}
                      onChange={(e) => setReservationFilters((prev) => ({ ...prev, estado: e.target.value }))}
                      aria-label="Filtrar reservas por estado"
                    >
                      {RESERVATION_STATUS_OPTIONS.map((option) => (
                        <option key={option.value || 'all'} value={option.value}>{option.label}</option>
                      ))}
                    </select>
                    <select
                      className="input-field"
                      value={reservationFilters.paqueteId}
                      onChange={(e) => setReservationFilters((prev) => ({ ...prev, paqueteId: e.target.value }))}
                      aria-label="Filtrar reservas por paquete"
                    >
                      <option value="">Todos los paquetes</option>
                      {packages.map((pkg) => (
                        <option key={pkg.paqueteId} value={pkg.paqueteId}>{pkg.nombre}</option>
                      ))}
                    </select>
                    <button type="button" onClick={loadData} className="btn-primary" disabled={loading}>
                      <ArrowPathIcon className="w-4 h-4" />
                      Filtrar
                    </button>
                  </div>
                </div>

                {sectionErrors.reservations ? (
                  <div className="p-10 text-center">
                    <ExclamationCircleIcon className="mx-auto mb-4 h-12 w-12 text-red-400" />
                    <p className="font-medium text-red-300">{sectionErrors.reservations}</p>
                    <button type="button" onClick={loadData} className="btn-secondary mx-auto mt-5" disabled={loading}>
                      <ArrowPathIcon className="w-4 h-4" />
                      Reintentar
                    </button>
                  </div>
                ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-white/10">
                        <th className="text-left text-gray-400 px-5 py-4">Cliente</th>
                        <th className="text-left text-gray-400 px-4 py-4">Paquete</th>
                        <th className="text-left text-gray-400 px-4 py-4">Fecha tour</th>
                        <th className="text-center text-gray-400 px-4 py-4">Asientos</th>
                        <th className="text-center text-gray-400 px-4 py-4">Estado</th>
                        <th className="text-left text-gray-400 px-4 py-4">Pago</th>
                        <th className="text-right text-gray-400 px-4 py-4">Monto</th>
                        <th className="text-right text-gray-400 px-5 py-4">Acciones</th>
                      </tr>
                    </thead>
                    <tbody>
                      {reservations.length === 0 ? (
                        <tr>
                          <td colSpan="8" className="px-5 py-10 text-center text-gray-400">
                            Aun no hay reservas para los filtros seleccionados.
                          </td>
                        </tr>
                      ) : (
                        reservations.map((reservation) => (
                          <tr key={reservation.reservaId} className="border-b border-white/5 hover:bg-white/[0.03]">
                            <td className="px-5 py-4">
                              <p className="text-white font-medium">{reservation.clienteNombre}</p>
                              <p className="text-xs text-gray-400">{reservation.clienteCorreo}</p>
                              {reservation.clienteTelefono && <p className="text-xs text-gray-500">{reservation.clienteTelefono}</p>}
                            </td>
                            <td className="px-4 py-4">
                              <p className="text-gray-100 font-medium">{reservation.paqueteNombre}</p>
                              <p className="text-xs text-gray-400">{reservation.paqueteDestino}</p>
                            </td>
                            <td className="px-4 py-4 text-gray-300 whitespace-nowrap">
                              <div className="flex items-center gap-2">
                                <CalendarDaysIcon className="w-4 h-4 text-primary-400" />
                                {formatShortDate(reservation.fechaInicio)}
                              </div>
                              <p className="text-xs text-gray-500 mt-1">Creada: {formatShortDate(reservation.fechaCreacion)}</p>
                            </td>
                            <td className="px-4 py-4 text-center text-gray-300">{reservation.cantAsientos}</td>
                            <td className="px-4 py-4 text-center">
                              <span className={getReservationBadgeClass(reservation.estado)}>{reservation.estado}</span>
                            </td>
                            <td className="px-4 py-4 text-gray-300">
                              <p className="font-medium">{reservation.pagoEstado}</p>
                              {reservation.metodoPago && <p className="text-xs text-gray-500">{reservation.metodoPago} {reservation.numReferencia ? `- ${reservation.numReferencia}` : ''}</p>}
                              {reservation.comprobanteArchivoNombre && <p className="text-xs text-primary-400">Adjunto: {reservation.comprobanteArchivoNombre}</p>}
                            </td>
                            <td className="px-4 py-4 text-right text-primary-400 font-semibold">{formatCurrency(reservation.montoTotal)}</td>
                            <td className="px-5 py-4">
                              <div className="flex justify-end">
                                <button
                                  type="button"
                                  onClick={() => startEditPackage(packages.find((pkg) => pkg.paqueteId === reservation.paqueteId))}
                                  className="btn-ghost text-xs"
                                  disabled={!packages.some((pkg) => pkg.paqueteId === reservation.paqueteId)}
                                >
                                  <PencilSquareIcon className="w-4 h-4" />
                                  Editar paquete
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
                )}
              </section>
            )}

            {activeTab === 'paquetes' && (
              <section className="grid grid-cols-1 xl:grid-cols-[420px_1fr] gap-6">
                <form onSubmit={handleSavePackage} className="glass-card p-6 h-fit">
                  <div className="flex items-center justify-between mb-5">
                    <h2 className="font-display text-xl font-bold text-white">
                      {editingPackageId ? 'Editar paquete' : 'Nuevo paquete'}
                    </h2>
                    {editingPackageId && (
                      <button type="button" onClick={resetPackageForm} className="btn-ghost text-sm">
                        <XMarkIcon className="w-4 h-4" />
                        Cancelar
                      </button>
                    )}
                  </div>

                  <div className="space-y-4">
                    <div>
                      <label className="input-label" htmlFor="nombre">Nombre</label>
                      <input id="nombre" name="nombre" className="input-field" value={packageForm.nombre} onChange={handlePackageChange} required />
                    </div>
                    <div>
                      <label className="input-label" htmlFor="destino">Destino</label>
                      <input id="destino" name="destino" className="input-field" value={packageForm.destino} onChange={handlePackageChange} required />
                    </div>
                    <div>
                      <label className="input-label" htmlFor="descripcion">Itinerario y descripcion</label>
                      <textarea id="descripcion" name="descripcion" rows="4" className="input-field resize-none" value={packageForm.descripcion} onChange={handlePackageChange} placeholder="Dia 1: recorrido, actividades, alojamiento..." />
                    </div>
                    <div>
                      <label className="input-label" htmlFor="imagenUrl">Imagen URL</label>
                      <input id="imagenUrl" name="imagenUrl" type="url" className="input-field" value={packageForm.imagenUrl} onChange={handlePackageChange} placeholder="https://..." />
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                      <div>
                        <label className="input-label" htmlFor="precioUnitario">Precio S/</label>
                        <input id="precioUnitario" name="precioUnitario" type="number" min="0.01" step="0.01" className="input-field" value={packageForm.precioUnitario} onChange={handlePackageChange} required />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="capacidadTotal">Capacidad</label>
                        <input id="capacidadTotal" name="capacidadTotal" type="number" min="1" className="input-field" value={packageForm.capacidadTotal} onChange={handlePackageChange} required />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="asientosDisp">Disponibles</label>
                        <input id="asientosDisp" name="asientosDisp" type="number" min="0" className="input-field" value={packageForm.asientosDisp} onChange={handlePackageChange} required />
                      </div>
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                      <div>
                        <label className="input-label" htmlFor="fechaInicio">Fecha inicio</label>
                        <input id="fechaInicio" name="fechaInicio" type="date" className="input-field" value={packageForm.fechaInicio} onChange={handlePackageChange} required />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="fechaFin">Fecha fin</label>
                        <input id="fechaFin" name="fechaFin" type="date" className="input-field" value={packageForm.fechaFin} onChange={handlePackageChange} required />
                      </div>
                    </div>
                    {editingPackageId && (
                      <label className="flex items-center gap-3 text-sm text-gray-300">
                        <input type="checkbox" name="activo" checked={packageForm.activo} onChange={handlePackageChange} className="h-4 w-4 accent-teal-500" />
                        Paquete activo en catalogo publico
                      </label>
                    )}
                    {packageFormError && (
                      <div className="input-error">
                        <ExclamationCircleIcon className="w-4 h-4" />
                        {packageFormError}
                      </div>
                    )}
                    <button type="submit" className="btn-primary w-full" disabled={saving}>
                      {saving ? <div className="spinner w-4 h-4" /> : editingPackageId ? <CheckCircleIcon className="w-4 h-4" /> : <PlusIcon className="w-4 h-4" />}
                      {editingPackageId ? 'Guardar cambios' : 'Crear paquete'}
                    </button>
                  </div>
                </form>

                <div className="glass-card overflow-hidden">
                  <div className="p-5 border-b border-white/10 flex items-center justify-between">
                    <h2 className="font-display text-xl font-bold text-white">Paquetes turisticos</h2>
                    <span className="badge-info">{packages.length} registros</span>
                  </div>
                  {sectionErrors.packages ? (
                    <div className="p-10 text-center">
                      <ExclamationCircleIcon className="mx-auto mb-4 h-12 w-12 text-red-400" />
                      <p className="font-medium text-red-300">{sectionErrors.packages}</p>
                      <button type="button" onClick={loadData} className="btn-secondary mx-auto mt-5" disabled={loading}>
                        <ArrowPathIcon className="w-4 h-4" />
                        Reintentar
                      </button>
                    </div>
                  ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-white/10">
                          <th className="text-left text-gray-400 px-5 py-4">Nombre</th>
                          <th className="text-left text-gray-400 px-4 py-4">Destino</th>
                          <th className="text-right text-gray-400 px-4 py-4">Precio</th>
                          <th className="text-right text-gray-400 px-4 py-4">Asientos</th>
                          <th className="text-left text-gray-400 px-4 py-4">Fechas</th>
                          <th className="text-center text-gray-400 px-4 py-4">Estado</th>
                          <th className="text-right text-gray-400 px-5 py-4">Acciones</th>
                        </tr>
                      </thead>
                      <tbody>
                        {packages.map((pkg) => (
                          <tr key={pkg.paqueteId} className="border-b border-white/5 hover:bg-white/[0.03]">
                            <td className="px-5 py-4 text-white font-medium">{pkg.nombre}</td>
                            <td className="px-4 py-4 text-gray-300">{pkg.destino}</td>
                            <td className="px-4 py-4 text-right text-primary-400 font-semibold">S/ {Number(pkg.precioUnitario).toFixed(2)}</td>
                            <td className="px-4 py-4 text-right text-gray-300">{pkg.asientosDisp} / {pkg.capacidadTotal}</td>
                            <td className="px-4 py-4 text-gray-300">
                              <div className="flex items-center gap-2 whitespace-nowrap">
                                <CalendarDaysIcon className="w-4 h-4 text-primary-400" />
                                {formatShortDate(pkg.fechaInicio)} - {formatShortDate(pkg.fechaFin)}
                              </div>
                            </td>
                            <td className="px-4 py-4 text-center">{pkg.activo !== false ? <span className="badge-success">Activo</span> : <span className="badge-gray">Inactivo</span>}</td>
                            <td className="px-5 py-4">
                              <div className="flex justify-end gap-2">
                                <button type="button" onClick={() => startEditPackage(pkg)} className="btn-ghost text-xs">
                                  <PencilSquareIcon className="w-4 h-4" />
                                  Editar
                                </button>
                                {pkg.activo !== false && (
                                  <button type="button" onClick={() => handleDeactivatePackage(pkg.paqueteId)} className="btn-ghost text-xs text-red-400">
                                    <TrashIcon className="w-4 h-4" />
                                    Desactivar
                                  </button>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  )}
                </div>
              </section>
            )}

            {activeTab === 'clientes' && (
              <section className="glass-card overflow-hidden">
                <div className="p-5 border-b border-white/10 flex items-center justify-between">
                  <h2 className="font-display text-xl font-bold text-white">Gestion de cuentas de cliente</h2>
                  <span className="badge-info">{clients.length} clientes</span>
                </div>
                {sectionErrors.clients ? (
                  <div className="p-10 text-center">
                    <ExclamationCircleIcon className="mx-auto mb-4 h-12 w-12 text-red-400" />
                    <p className="font-medium text-red-300">{sectionErrors.clients}</p>
                    <button type="button" onClick={loadData} className="btn-secondary mx-auto mt-5" disabled={loading}>
                      <ArrowPathIcon className="w-4 h-4" />
                      Reintentar
                    </button>
                  </div>
                ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-white/10">
                        <th className="text-left text-gray-400 px-5 py-4">Nombre</th>
                        <th className="text-left text-gray-400 px-4 py-4">Correo</th>
                        <th className="text-center text-gray-400 px-4 py-4">Estado</th>
                        <th className="text-left text-gray-400 px-4 py-4">Registro</th>
                        <th className="text-right text-gray-400 px-5 py-4">Accion</th>
                      </tr>
                    </thead>
                    <tbody>
                      {clients.map((client) => (
                        <tr key={client.clienteId} className="border-b border-white/5 hover:bg-white/[0.03]">
                          <td className="px-5 py-4 text-white font-medium">{client.nombre}</td>
                          <td className="px-4 py-4 text-gray-300">{client.correo}</td>
                          <td className="px-4 py-4 text-center">
                            <span className={client.estado === 'Activo' ? 'badge-success' : 'badge-warning'}>{client.estado}</span>
                          </td>
                          <td className="px-4 py-4 text-gray-300">{client.fechaRegistro ? new Date(client.fechaRegistro).toLocaleDateString('es-PE') : 'N/A'}</td>
                          <td className="px-5 py-4">
                            <div className="flex justify-end">
                              {client.estado === 'Activo' ? (
                                <button type="button" onClick={() => handleClientStatus(client.clienteId, 'Inactivo')} className="btn-ghost text-xs text-amber-400" disabled={saving}>
                                  <UserMinusIcon className="w-4 h-4" />
                                  Desactivar
                                </button>
                              ) : (
                                <button type="button" onClick={() => handleClientStatus(client.clienteId, 'Activo')} className="btn-ghost text-xs text-emerald-400" disabled={saving}>
                                  <UserPlusIcon className="w-4 h-4" />
                                  Activar
                                </button>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                )}
              </section>
            )}

            {activeTab === 'portada' && (
              <section className="grid grid-cols-1 xl:grid-cols-[460px_1fr] gap-6">
                <form onSubmit={handleSaveSiteSettings} className="glass-card p-6 h-fit">
                  <div className="mb-5 flex items-center gap-3">
                    <PhotoIcon className="h-6 w-6 text-primary-400" />
                    <h2 className="font-display text-xl font-bold text-white">Editar portada</h2>
                  </div>

                  <div className="space-y-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                      <div>
                        <label className="input-label" htmlFor="companyName">Nombre corto</label>
                        <input id="companyName" name="companyName" className="input-field" value={siteForm.companyName} onChange={handleSiteFieldChange} />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="companySubtitle">Subtitulo marca</label>
                        <input id="companySubtitle" name="companySubtitle" className="input-field" value={siteForm.companySubtitle} onChange={handleSiteFieldChange} />
                      </div>
                    </div>

                    <div>
                      <label className="input-label" htmlFor="logoUrl">Logo URL</label>
                      <input id="logoUrl" name="logoUrl" type="url" className="input-field" value={siteForm.logoUrl} onChange={handleSiteFieldChange} placeholder="https://..." />
                    </div>

                    <div>
                      <label className="input-label" htmlFor="heroBadge">Etiqueta superior</label>
                      <input id="heroBadge" name="heroBadge" className="input-field" value={siteForm.heroBadge} onChange={handleSiteFieldChange} />
                    </div>

                    <div>
                      <label className="input-label" htmlFor="heroTitle">Titulo principal</label>
                      <input id="heroTitle" name="heroTitle" className="input-field" value={siteForm.heroTitle} onChange={handleSiteFieldChange} />
                    </div>

                    <div>
                      <label className="input-label" htmlFor="heroSubtitle">Descripcion</label>
                      <textarea id="heroSubtitle" name="heroSubtitle" rows="3" className="input-field resize-none" value={siteForm.heroSubtitle} onChange={handleSiteFieldChange} />
                    </div>

                    <div className="grid grid-cols-3 gap-3">
                      <div>
                        <label className="input-label" htmlFor="heroStatsTours">Tours</label>
                        <input id="heroStatsTours" name="heroStatsTours" className="input-field" value={siteForm.heroStatsTours} onChange={handleSiteFieldChange} />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="heroStatsTravelers">Viajeros</label>
                        <input id="heroStatsTravelers" name="heroStatsTravelers" className="input-field" value={siteForm.heroStatsTravelers} onChange={handleSiteFieldChange} />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="heroStatsRating">Rating</label>
                        <input id="heroStatsRating" name="heroStatsRating" className="input-field" value={siteForm.heroStatsRating} onChange={handleSiteFieldChange} />
                      </div>
                    </div>

                    <div className="flex flex-col sm:flex-row gap-3 pt-2">
                      <button type="submit" className="btn-primary flex-1">
                        <CheckCircleIcon className="w-4 h-4" />
                        Actualizar portada
                      </button>
                      <button type="button" onClick={handleRestoreSiteSettings} className="btn-secondary">
                        Restaurar
                      </button>
                    </div>
                  </div>
                </form>

                <div className="space-y-6">
                  <div className="glass-card overflow-hidden">
                    <div className="relative min-h-[420px] overflow-hidden">
                      {siteForm.heroImages[0]?.imageUrl ? (
                        <img src={siteForm.heroImages[0].imageUrl} alt={siteForm.heroImages[0].title} className="absolute inset-0 h-full w-full object-cover" />
                      ) : (
                        <div className="absolute inset-0 flex items-center justify-center bg-white/5">
                          <PhotoIcon className="h-14 w-14 text-gray-600" />
                        </div>
                      )}
                      <div className="absolute inset-0 bg-gradient-to-b from-dark-900/70 via-dark-900/45 to-dark-900/90" />
                      <div className="absolute inset-0 flex flex-col items-center justify-center px-8 text-center">
                        <p className="mb-4 inline-flex rounded-full border border-primary-400/30 bg-primary-500/10 px-4 py-2 text-sm font-semibold text-primary-300">
                          {siteForm.heroBadge}
                        </p>
                        <h3 className="max-w-3xl font-display text-4xl font-black text-white">{siteForm.heroTitle}</h3>
                        <p className="mt-4 max-w-2xl text-gray-200">{siteForm.heroSubtitle}</p>
                      </div>
                      {siteForm.heroImages[0]?.title && (
                        <div className="absolute bottom-4 left-4 rounded-full border border-white/20 bg-black/35 px-4 py-2 text-sm font-semibold text-white backdrop-blur-md">
                          {siteForm.heroImages[0].title}
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="glass-card p-5">
                    <div className="mb-4">
                      <h3 className="font-display text-xl font-bold text-white">Foto principal de fondo</h3>
                      <p className="mt-1 text-sm text-gray-400">Esta sera la unica imagen visible en la portada.</p>
                    </div>
                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                      <div>
                        <label className="input-label" htmlFor="heroImageTitle-0">Nombre de la zona</label>
                        <input
                          id="heroImageTitle-0"
                          className="input-field"
                          value={siteForm.heroImages[0]?.title ?? ''}
                          onChange={(event) => handleHeroImageChange(0, 'title', event.target.value)}
                        />
                      </div>
                      <div>
                        <label className="input-label" htmlFor="heroImageUrl-0">Foto URL</label>
                        <input
                          id="heroImageUrl-0"
                          type="url"
                          className="input-field"
                          value={siteForm.heroImages[0]?.imageUrl ?? ''}
                          onChange={(event) => handleHeroImageChange(0, 'imageUrl', event.target.value)}
                          placeholder="https://..."
                        />
                      </div>
                    </div>
                    <div className="mt-5 flex flex-col gap-3 sm:flex-row">
                      <button type="button" onClick={handleSaveSiteSettings} className="btn-primary flex-1">
                        <CheckCircleIcon className="w-4 h-4" />
                        Actualizar portada
                      </button>
                      <button type="button" onClick={handleRestoreSiteSettings} className="btn-secondary">
                        Restaurar
                      </button>
                    </div>
                  </div>
                </div>
              </section>
            )}

            {activeTab === 'reportes' && (
              <section className="glass-card p-6">
                <div className="flex items-center gap-3 mb-5">
                  <DocumentChartBarIcon className="w-6 h-6 text-primary-400" />
                  <h2 className="font-display text-xl font-bold text-white">Reportes de ventas y reservas</h2>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-5">
                  <div>
                    <label className="input-label" htmlFor="reportFrom">Desde</label>
                    <input id="reportFrom" type="date" className="input-field" value={reportForm.from} onChange={(e) => setReportForm((prev) => ({ ...prev, from: e.target.value }))} />
                  </div>
                  <div>
                    <label className="input-label" htmlFor="reportTo">Hasta</label>
                    <input id="reportTo" type="date" className="input-field" value={reportForm.to} onChange={(e) => setReportForm((prev) => ({ ...prev, to: e.target.value }))} />
                  </div>
                  <div>
                    <label className="input-label" htmlFor="reportFormat">Formato</label>
                    <select id="reportFormat" className="input-field" value={reportForm.format} onChange={(e) => setReportForm((prev) => ({ ...prev, format: e.target.value }))}>
                      <option value="pdf">PDF</option>
                      <option value="xlsx">Excel XLSX</option>
                    </select>
                  </div>
                  <div>
                    <label className="input-label" htmlFor="reportEstado">Estado</label>
                    <select id="reportEstado" className="input-field" value={reportForm.estado} onChange={(e) => setReportForm((prev) => ({ ...prev, estado: e.target.value }))}>
                      {REPORT_STATUS_OPTIONS.map((option) => (
                        <option key={option.value || 'all'} value={option.value}>{option.label}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="input-label" htmlFor="reportPaquete">Paquete</label>
                    <select id="reportPaquete" className="input-field" value={reportForm.paqueteId} onChange={(e) => setReportForm((prev) => ({ ...prev, paqueteId: e.target.value }))}>
                      <option value="">Todos los paquetes</option>
                      {packages.map((pkg) => (
                        <option key={pkg.paqueteId} value={pkg.paqueteId}>{pkg.nombre}</option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {[
                    ['sales', 'Reporte de ventas'],
                    ['reservations', 'Reporte de reservas'],
                  ].map(([type, label]) => (
                    <button key={type} type="button" onClick={() => handleDownloadReport(type)} disabled={reportLoading === type} className="btn-secondary justify-between">
                      <span>{label}</span>
                      {reportLoading === type ? <div className="spinner w-4 h-4" /> : <ArrowDownTrayIcon className="w-4 h-4" />}
                    </button>
                  ))}
                </div>
              </section>
            )}
          </>
        )}
      </div>
    </div>
  )
}

export default AdminDashboard
