import { useCallback, useEffect, useState } from 'react'
import { RefreshCw, CreditCard } from 'lucide-react'
import { api, formatDate, formatMoney } from '../../api'
import type { PaymentResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { StatusBadge } from '../../components/StatusBadge'

export default function AdminPaymentsPage() {
  const { session } = useSession()
  const [payments, setPayments] = useState<PaymentResponseDto[]>([])
  const [search, setSearch] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.payments(session.token)
      setPayments(data)
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ödemeler alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token])

  useEffect(() => { load() }, [load])

  const filtered = search
    ? payments.filter(p =>
        p.gatewayTransactionId?.toLowerCase().includes(search.toLowerCase()) ||
        p.id.toLowerCase().includes(search.toLowerCase())
      )
    : payments

  const totalCompleted = payments.filter(p => p.status === 'Completed').reduce((s, p) => s + p.amount, 0)

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Ödemeler</h1>
          <p className="text-sm text-gray-500 mt-0.5">{payments.length} ödeme kaydı · Toplam {formatMoney(totalCompleted)}</p>
        </div>
        <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
          <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          Yenile
        </button>
      </div>

      {error && <div className="alert-error">{error}</div>}

      <div className="flex items-center gap-3">
        <input
          className="input w-72"
          placeholder="İşlem no ile ara…"
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        {search && (
          <button type="button" className="btn-ghost text-xs" onClick={() => setSearch('')}>Temizle</button>
        )}
      </div>

      <div className="card overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100">
              <th className="text-right py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
              <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Tarih</th>
              <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
              <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Gateway</th>
              <th className="text-left py-2 text-xs font-medium text-gray-500 uppercase tracking-wide">İşlem No</th>
            </tr>
          </thead>
          <tbody>
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={5} className="py-8 text-center text-gray-400">
                  <CreditCard size={32} className="mx-auto mb-2 text-gray-200" />
                  Ödeme bulunamadı.
                </td>
              </tr>
            ) : filtered.map(p => (
              <tr key={p.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-3 pr-4 text-right font-semibold text-gray-900">{formatMoney(p.amount)}</td>
                <td className="py-3 pr-4 text-gray-500">{formatDate(p.paidAtUtc)}</td>
                <td className="py-3 pr-4"><StatusBadge status={p.status} /></td>
                <td className="py-3 pr-4"><StatusBadge status={p.gatewayStatus} /></td>
                <td className="py-3 text-xs text-gray-400 font-mono">{p.gatewayTransactionId ?? '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
