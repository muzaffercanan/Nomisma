import { useCallback, useEffect, useState } from 'react'
import { RefreshCw, AlertTriangle } from 'lucide-react'
import { api, formatMoney } from '../../api'
import type { CustomerResponseDto, LoanResponseDto, PaymentResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { MetricCard } from '../../components/MetricCard'
import { StatusBadge } from '../../components/StatusBadge'
import { useNavigate } from 'react-router-dom'

export default function AdminDashboardPage() {
  const { session } = useSession()
  const navigate = useNavigate()
  const [customers, setCustomers] = useState<CustomerResponseDto[]>([])
  const [loans, setLoans] = useState<LoanResponseDto[]>([])
  const [payments, setPayments] = useState<PaymentResponseDto[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [c, l, p] = await Promise.all([
        api.customers(session.token),
        api.loans(session.token),
        api.payments(session.token),
      ])
      setCustomers(c)
      setLoans(l)
      setPayments(p)
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Veri alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token])

  useEffect(() => { load() }, [load])

  const activeLoans = loans.filter(l => l.status === 'Active')
  const totalRemaining = loans.reduce((s, l) => s + l.remainingDebt, 0)
  const overdueLoans = loans.filter(l => l.installments.some(i => i.status === 'Overdue'))

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Operasyon Paneli</h1>
          <p className="text-sm text-gray-500 mt-0.5">Tüm kredi ve ödeme operasyonları</p>
        </div>
        <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
          <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          Yenile
        </button>
      </div>

      {error && <div className="alert-error">{error}</div>}

      {/* Metrics */}
      <div className="grid grid-cols-4 gap-4">
        <MetricCard title="Müşteri" value={customers.length.toString()} variant="blue" />
        <MetricCard title="Aktif Kredi" value={activeLoans.length.toString()} variant="mint" />
        <MetricCard title="Kalan Borç" value={formatMoney(totalRemaining)} variant="aqua" />
        <MetricCard title="Ödeme" value={payments.length.toString()} variant="lavender" />
      </div>

      {/* Overdue loans alert */}
      {overdueLoans.length > 0 && (
        <div>
          <div className="flex items-center gap-2 mb-3">
            <AlertTriangle size={18} className="text-red-500" />
            <h2 className="section-title text-red-600">Gecikmiş Ödemeli Krediler ({overdueLoans.length})</h2>
          </div>
          <div className="card overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Müşteri</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Kredi Türü</th>
                  <th className="text-right py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Kalan Borç</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
                  <th className="py-2" />
                </tr>
              </thead>
              <tbody>
                {overdueLoans.slice(0, 10).map(loan => {
                  const customer = customers.find(c => c.id === loan.customerId)
                  return (
                    <tr key={loan.id} className="border-b border-gray-50 hover:bg-red-50/30 transition-colors">
                      <td className="py-2.5 pr-4 font-medium text-gray-900">{customer?.fullName ?? '—'}</td>
                      <td className="py-2.5 pr-4 text-gray-500">{loan.type}</td>
                      <td className="py-2.5 pr-4 text-right font-semibold text-gray-900">{formatMoney(loan.remainingDebt)}</td>
                      <td className="py-2.5 pr-4"><StatusBadge status="Overdue" /></td>
                      <td className="py-2.5 text-right">
                        <button
                          type="button"
                          onClick={() => navigate('/admin/loans')}
                          className="text-xs text-brand-teal hover:underline font-medium"
                        >
                          Detay
                        </button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Recent loans */}
      <div>
        <h2 className="section-title mb-3">Son Krediler</h2>
        <div className="card overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-100">
                <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Müşteri</th>
                <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Tür</th>
                <th className="text-right py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Toplam Borç</th>
                <th className="text-right py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Kalan</th>
                <th className="text-center py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Skor</th>
                <th className="text-left py-2 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
              </tr>
            </thead>
            <tbody>
              {loans.slice(0, 8).map(loan => {
                const customer = customers.find(c => c.id === loan.customerId)
                return (
                  <tr key={loan.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                    <td className="py-2.5 pr-4 font-medium text-gray-900">{customer?.fullName ?? '—'}</td>
                    <td className="py-2.5 pr-4 text-gray-500">{loan.type}</td>
                    <td className="py-2.5 pr-4 text-right font-semibold text-gray-900">{formatMoney(loan.totalDebt)}</td>
                    <td className="py-2.5 pr-4 text-right text-gray-500">{formatMoney(loan.remainingDebt)}</td>
                    <td className="py-2.5 pr-4 text-center">
                      <span className="text-xs font-semibold text-gray-700">{loan.creditScore}</span>
                    </td>
                    <td className="py-2.5"><StatusBadge status={loan.status} /></td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
