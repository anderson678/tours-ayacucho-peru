import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import apiClient, { getApiErrorMessage } from '../api/apiClient'
import toast from 'react-hot-toast'
import {
  CurrencyDollarIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
  CalendarDaysIcon,
  UsersIcon,
  BanknotesIcon,
  DevicePhoneMobileIcon,
  ExclamationCircleIcon,
  DocumentTextIcon,
  PaperClipIcon,
} from '@heroicons/react/24/outline'

const METODOS_PAGO = [
  { value: 'TransferenciaBancaria', label: 'Transferencia Bancaria', icon: BanknotesIcon, color: 'text-emerald-400' },
  { value: 'DepositoCuenta', label: 'Deposito en Cuenta', icon: BanknotesIcon, color: 'text-gold-400' },
  { value: 'Yape', label: 'Yape', icon: DevicePhoneMobileIcon, color: 'text-purple-400' },
  { value: 'Plin', label: 'Plin', icon: DevicePhoneMobileIcon, color: 'text-blue-400' },
]

const formatMoney = (value) => `S/ ${Number(value ?? 0).toFixed(2)}`
const MAX_RECEIPT_SIZE = 2 * 1024 * 1024
const RECEIPT_TYPES = ['image/jpeg', 'image/png', 'application/pdf']

const parseReceipt = (receipt) => {
  if (!receipt?.comprobanteContenido) return null
  try {
    return JSON.parse(receipt.comprobanteContenido)
  } catch {
    return null
  }
}

const Payment = () => {
  const { reservaId } = useParams()
  const navigate = useNavigate()
  const [reservation, setReservation] = useState(null)
  const [receipt, setReceipt] = useState(null)
  const [paymentResult, setPaymentResult] = useState(null)
  const [resLoading, setResLoading] = useState(true)
  const [loading, setLoading] = useState(false)
  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [receiptFile, setReceiptFile] = useState(null)
  const [formData, setFormData] = useState({
    metodoPago: 'TransferenciaBancaria',
    numReferencia: '',
    monto: '',
    comprobanteArchivoNombre: '',
    comprobanteArchivoTipo: '',
    comprobanteArchivoBase64: '',
  })

  useEffect(() => {
    const fetchReservation = async () => {
      try {
        const res = await apiClient.get(`/reservations/${reservaId}`)
        setReservation(res.data)
        setFormData((prev) => ({ ...prev, monto: Number(res.data.montoTotal ?? 0).toFixed(2) }))
      } catch (err) {
        toast.error(getApiErrorMessage(err, 'No se pudo cargar la reserva'))
        navigate('/mis-reservas')
      } finally {
        setResLoading(false)
      }
    }

    fetchReservation()
  }, [reservaId, navigate])

  const expectedAmount = Number(reservation?.montoTotal ?? 0)
  const amount = Number(formData.monto)
  const amountDiff = Math.abs(amount - expectedAmount)
  const isPendingPayment = reservation?.estado === 'PENDIENTE_PAGO'

  const validate = () => {
    const errs = {}

    if (!isPendingPayment) {
      errs.general = 'Solo se puede registrar pago para reservas en estado PENDIENTE_PAGO'
    }

    if (!METODOS_PAGO.some((method) => method.value === formData.metodoPago)) {
      errs.metodoPago = 'Selecciona un metodo de pago valido'
    }

    if (!formData.numReferencia.trim()) {
      errs.numReferencia = 'El numero de referencia es requerido'
    } else if (formData.numReferencia.trim().length > 100) {
      errs.numReferencia = 'El numero de referencia no puede exceder 100 caracteres'
    }

    if (!formData.monto || Number.isNaN(amount) || amount <= 0) {
      errs.monto = 'El monto debe ser mayor a 0'
    } else if (amountDiff > 0.01) {
      errs.monto = `El monto debe coincidir con ${formatMoney(expectedAmount)}`
    }

    if (!formData.comprobanteArchivoBase64) {
      errs.comprobante = 'Adjunta el comprobante de pago en JPG, PNG o PDF'
    }

    return errs
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setServerError('')

    const errs = validate()
    if (Object.keys(errs).length > 0) {
      setErrors(errs)
      return
    }

    setErrors({})
    setLoading(true)
    try {
      const response = await apiClient.post('/payments', {
        reservaId,
        monto: amount,
        metodoPago: formData.metodoPago,
        numReferencia: formData.numReferencia.trim(),
        comprobanteArchivoNombre: formData.comprobanteArchivoNombre,
        comprobanteArchivoTipo: formData.comprobanteArchivoTipo,
        comprobanteArchivoBase64: formData.comprobanteArchivoBase64,
      })

      setPaymentResult(response.data)
      toast.success('Pago registrado. La reserva fue confirmada.')

      const receiptResponse = await apiClient.get(`/payments/${response.data.pagoId}/receipt`)
      setReceipt(receiptResponse.data)
    } catch (err) {
      const message = getApiErrorMessage(err, 'Error al registrar el pago')
      setServerError(message)
      toast.error(message)
    } finally {
      setLoading(false)
    }
  }

  const handleChange = (event) => {
    const { name, value } = event.target
    setFormData((prev) => ({ ...prev, [name]: value }))
    if (errors[name] || errors.general) {
      setErrors((prev) => ({ ...prev, [name]: '', general: '' }))
    }
    if (serverError) setServerError('')
  }

  const handleReceiptFile = (event) => {
    const file = event.target.files?.[0]
    setReceiptFile(null)
    setFormData((prev) => ({
      ...prev,
      comprobanteArchivoNombre: '',
      comprobanteArchivoTipo: '',
      comprobanteArchivoBase64: '',
    }))

    if (errors.comprobante || errors.general) {
      setErrors((prev) => ({ ...prev, comprobante: '', general: '' }))
    }
    if (serverError) setServerError('')
    if (!file) return

    if (!RECEIPT_TYPES.includes(file.type)) {
      setErrors((prev) => ({ ...prev, comprobante: 'Solo se aceptan archivos JPG, PNG o PDF.' }))
      return
    }

    if (file.size > MAX_RECEIPT_SIZE) {
      setErrors((prev) => ({ ...prev, comprobante: 'El comprobante no puede superar 2 MB.' }))
      return
    }

    const reader = new FileReader()
    reader.onload = () => {
      const result = String(reader.result || '')
      const base64 = result.includes(',') ? result.split(',')[1] : result
      setReceiptFile(file)
      setFormData((prev) => ({
        ...prev,
        comprobanteArchivoNombre: file.name,
        comprobanteArchivoTipo: file.type,
        comprobanteArchivoBase64: base64,
      }))
    }
    reader.onerror = () => {
      setErrors((prev) => ({ ...prev, comprobante: 'No se pudo leer el archivo seleccionado.' }))
    }
    reader.readAsDataURL(file)
  }

  const receiptContent = parseReceipt(receipt)

  if (resLoading) {
    return (
      <div className="page-wrapper flex items-center justify-center">
        <div className="spinner w-12 h-12" />
      </div>
    )
  }

  if (paymentResult) {
    return (
      <div className="page-wrapper">
        <div className="container-main max-w-3xl animate-fade-in">
          <div className="glass-card p-8">
            <div className="flex flex-col items-center text-center gap-4 mb-8">
              <div className="w-16 h-16 rounded-full bg-emerald-500/20 border border-emerald-500/30 flex items-center justify-center">
                <CheckCircleIcon className="w-9 h-9 text-emerald-400" />
              </div>
              <div>
                <h1 className="font-display text-3xl font-bold text-white">Pago Registrado</h1>
                <p className="text-gray-400 mt-2">Tu reserva ahora esta confirmada. El comprobante fue generado y enviado al correo registrado.</p>
              </div>
            </div>

            <div className="rounded-xl border border-white/10 bg-white/[0.03] p-5 mb-6">
              <h2 className="font-display text-xl font-bold text-white mb-4 flex items-center gap-2">
                <DocumentTextIcon className="w-5 h-5 text-primary-400" />
                Comprobante Digital
              </h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
                <div>
                  <p className="text-gray-500 text-xs">Reserva</p>
                  <p className="text-white font-mono text-xs break-all">{receiptContent?.reservaId ?? paymentResult.reservaId}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Pago</p>
                  <p className="text-white font-mono text-xs break-all">{paymentResult.pagoId}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Paquete</p>
                  <p className="text-white">{receiptContent?.paquete ?? reservation?.paqueteNombre ?? 'N/A'}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Cliente</p>
                  <p className="text-white">{receiptContent?.cliente ?? 'Cliente autenticado'}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Monto</p>
                  <p className="text-primary-400 font-bold">{formatMoney(receiptContent?.monto ?? paymentResult.monto)}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Metodo</p>
                  <p className="text-white">{receiptContent?.metodoPago ?? formData.metodoPago}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Referencia</p>
                  <p className="text-white">{receiptContent?.numReferencia ?? formData.numReferencia}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Adjunto</p>
                  <p className="text-white">{receiptContent?.comprobanteAdjunto?.nombre ?? formData.comprobanteArchivoNombre ?? 'No adjuntado'}</p>
                </div>
                <div>
                  <p className="text-gray-500 text-xs">Fecha y hora</p>
                  <p className="text-white">{receiptContent?.fechaHora ? new Date(receiptContent.fechaHora).toLocaleString('es-PE') : new Date(paymentResult.fechaPago).toLocaleString('es-PE')}</p>
                </div>
              </div>
            </div>

            <div className="flex flex-col sm:flex-row gap-3">
              <Link to="/mis-reservas" className="btn-primary flex-1">
                Ver mis reservas
              </Link>
              <Link to="/" className="btn-secondary flex-1">
                Explorar paquetes
              </Link>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="page-wrapper">
      <div className="container-main max-w-4xl animate-fade-in">
        <button id="back-from-payment-btn" onClick={() => navigate(-1)} className="btn-ghost mb-6 -ml-2">
          <ArrowLeftIcon className="w-4 h-4" />
          Volver
        </button>

        <div className="mb-8">
          <div className="teal-divider mb-4" />
          <h1 className="font-display text-3xl font-bold text-white">Registrar Pago</h1>
          <p className="text-gray-400 mt-2">Registra el pago para confirmar tu reserva.</p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-5 gap-8">
          <div className="lg:col-span-3">
            <div className="glass-card p-8">
              <form id="payment-form" onSubmit={handleSubmit} className="flex flex-col gap-6" noValidate>
                {(serverError || errors.general) && (
                  <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-300 flex gap-3">
                    <ExclamationCircleIcon className="w-5 h-5 shrink-0" />
                    <p>{serverError || errors.general}</p>
                  </div>
                )}

                <div>
                  <label className="input-label mb-3 block">Metodo de pago</label>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    {METODOS_PAGO.map(({ value, label, icon: Icon, color }) => (
                      <button
                        key={value}
                        type="button"
                        id={`payment-method-${value.toLowerCase()}`}
                        onClick={() => {
                          setFormData((prev) => ({ ...prev, metodoPago: value }))
                          if (errors.metodoPago) setErrors((prev) => ({ ...prev, metodoPago: '' }))
                        }}
                        className={`flex items-center gap-3 p-3 rounded-xl border transition-all duration-200 text-left ${
                          formData.metodoPago === value
                            ? 'border-primary-500 bg-primary-500/10 shadow-teal'
                            : 'border-white/10 bg-white/[0.03] hover:border-white/20'
                        }`}
                      >
                        <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${formData.metodoPago === value ? 'bg-primary-500/20' : 'bg-white/5'}`}>
                          <Icon className={`w-4 h-4 ${formData.metodoPago === value ? 'text-primary-400' : color}`} />
                        </div>
                        <span className={`text-sm font-medium ${formData.metodoPago === value ? 'text-primary-400' : 'text-gray-300'}`}>
                          {label}
                        </span>
                        {formData.metodoPago === value && <CheckCircleIcon className="w-4 h-4 text-primary-400 ml-auto flex-shrink-0" />}
                      </button>
                    ))}
                  </div>
                  {errors.metodoPago && <p className="input-error">{errors.metodoPago}</p>}
                </div>

                <div>
                  <label htmlFor="numReferencia" className="input-label">Numero de referencia</label>
                  <input
                    id="numReferencia"
                    name="numReferencia"
                    type="text"
                    value={formData.numReferencia}
                    onChange={handleChange}
                    placeholder="Ej: OP-2026-123456"
                    className={`input-field ${errors.numReferencia ? 'border-red-500/60' : ''}`}
                    maxLength={100}
                  />
                  {errors.numReferencia && <p className="input-error">{errors.numReferencia}</p>}
                  <p className="text-gray-500 text-xs mt-1">Ingresa el codigo de operacion del banco, Yape o Plin.</p>
                </div>

                <div>
                  <label htmlFor="monto" className="input-label">Monto pagado</label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 font-medium text-sm">S/</span>
                    <input
                      id="monto"
                      name="monto"
                      type="number"
                      step="0.01"
                      value={formData.monto}
                      onChange={handleChange}
                      className={`input-field pl-10 ${errors.monto ? 'border-red-500/60' : ''}`}
                    />
                  </div>
                  {errors.monto && <p className="input-error">{errors.monto}</p>}
                  <p className={amountDiff <= 0.01 ? 'text-gray-500 text-xs mt-1' : 'text-amber-400 text-xs mt-1'}>
                    Monto esperado: {formatMoney(expectedAmount)}. Tolerancia permitida: S/ 0.01.
                  </p>
                </div>

                <div>
                  <label htmlFor="comprobantePago" className="input-label">Comprobante de pago</label>
                  <label
                    htmlFor="comprobantePago"
                    className={`flex cursor-pointer items-center gap-3 rounded-xl border p-4 transition-all duration-200 ${
                      errors.comprobante
                        ? 'border-red-500/60 bg-red-500/10'
                        : receiptFile
                          ? 'border-emerald-500/40 bg-emerald-500/10'
                          : 'border-white/10 bg-white/[0.03] hover:border-primary-400/50'
                    }`}
                  >
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-white/5">
                      <PaperClipIcon className={receiptFile ? 'h-5 w-5 text-emerald-400' : 'h-5 w-5 text-primary-400'} />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-semibold text-white">
                        {receiptFile ? receiptFile.name : 'Adjuntar comprobante'}
                      </p>
                      <p className="text-xs text-gray-500">JPG, PNG o PDF. Maximo 2 MB.</p>
                    </div>
                    {receiptFile && <CheckCircleIcon className="h-5 w-5 text-emerald-400" />}
                  </label>
                  <input
                    id="comprobantePago"
                    name="comprobantePago"
                    type="file"
                    accept=".jpg,.jpeg,.png,.pdf,image/jpeg,image/png,application/pdf"
                    onChange={handleReceiptFile}
                    className="sr-only"
                  />
                  {errors.comprobante && <p className="input-error">{errors.comprobante}</p>}
                </div>

                <button id="confirm-payment-btn" type="submit" disabled={loading || !isPendingPayment} className="btn-primary w-full">
                  {loading ? (
                    <>
                      <div className="spinner w-4 h-4" />
                      Registrando pago...
                    </>
                  ) : (
                    <>
                      <CurrencyDollarIcon className="w-5 h-5" />
                      Confirmar Pago
                    </>
                  )}
                </button>
              </form>
            </div>
          </div>

          <div className="lg:col-span-2">
            <div className="glass-card p-6 sticky top-24">
              <h2 className="font-display font-bold text-white text-lg mb-5 flex items-center gap-2">
                <CalendarDaysIcon className="w-5 h-5 text-primary-400" />
                Detalle de Reserva
              </h2>

              {reservation && (
                <div className="flex flex-col gap-3 mb-5">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-400">Estado</span>
                    <span className={isPendingPayment ? 'badge-warning' : 'badge-success'}>{reservation.estado}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-400">ID Reserva</span>
                    <span className="text-white font-mono text-xs">{reservation.reservaId?.substring(0, 12)}...</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-400">Paquete</span>
                    <span className="text-white font-medium text-right">{reservation.paqueteNombre ?? 'Tour'}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-400 flex items-center gap-1">
                      <UsersIcon className="w-3.5 h-3.5" /> Asientos
                    </span>
                    <span className="text-white font-medium">{reservation.cantAsientos}</span>
                  </div>
                  <div className="border-t border-white/10 pt-3 flex justify-between">
                    <span className="text-white font-bold text-lg">Total</span>
                    <span className="font-display text-2xl font-bold gradient-text">{formatMoney(reservation.montoTotal)}</span>
                  </div>
                </div>
              )}

              <div className="p-3 rounded-xl bg-emerald-500/5 border border-emerald-500/20 text-xs text-emerald-400 flex items-start gap-2">
                <CheckCircleIcon className="w-4 h-4 flex-shrink-0 mt-0.5" />
                <span>Al registrar el pago, la reserva cambia a CONFIRMADA y se genera un comprobante digital.</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Payment
