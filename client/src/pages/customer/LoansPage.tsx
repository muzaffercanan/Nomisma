import { useCallback, useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { RefreshCw } from 'lucide-react'
import { api, formatDate, formatMoney } from '../../api'
import type { LoanResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { LoanCard } from '../../components/LoanCard'
import { StatusBadge } from '../../components/StatusBadge'

type Filter = 'all' | 'Active' | 'Closed'

export default function CustomerLoansPage() {
  const { session } = useSession()
  const location = useLocation()
  const [loans, setLoans] = useState<LoanResponseDto[]>([])
  const [selectedId, setSelectedId] = useState<string>((location.state as { loanId?: string } | null)?.loanId ?? '')
  const [filter, setFilter] = useState<Filter>('all')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.loans(session.token)
      setLoans(data)
      if (!selectedId && data[0]) setSelectedId(data[0].id)
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Veri alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token, selectedId])

  useEffect(() => { load() }, [load])

  const filtered = loans.filter(l => filter === 'all' || l.status === filter)
  const selected = loans.find(l => l.id === selectedId) ?? filtered[0] ?? null

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Kredilerim</h1>
          <p className="text-sm text-gray-500 mt-0.5">{loans.length} kredi kaydı</p>
        </div>
        <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
          <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          Yenile
        </button>
      </div>

      {error && <div className="alert-error">{error}</div>}

      {/* Filter tabs */}
      <div className="flex gap-2">
        {(['all', 'Active', 'Closed'] as Filter[]).map(f => (
          <button
            key={f}
            type="button"
            onClick={() => setFilter(f)}
            className={`px-4 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              filter === f ? 'bg-brand-navy text-white' : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'
            }`}
          >
            {f === 'all' ? 'Tümü' : f === 'Active' ? 'Aktif' : 'Kapalı'}
          </button>
        ))}
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Loan cards */}
        <div className="col-span-1 space-y-4">
          {filtered.length === 0 ? (
            <div className="card text-center text-sm text-gray-400">Kredi bulunamadı.</div>
          ) : (
            filtered.map(loan => (
              <LoanCard
                key={loan.id}
                loan={loan}
                selected={selected?.id === loan.id}
                onDetail={() => setSelectedId(loan.id)}
              />
            ))
          )}
        </div>

        {/* Installment plan */}
        <div className="col-span-2">
          {selected ? (
            <div className="card space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="section-title">{selected.type} Kredisi — Taksit Planı</h2>
                  <p className="text-sm text-gray-500">
                    {selected.termMonths} taksit · {formatMoney(selected.totalDebt)} toplam
                  </p>
                </div>
                <StatusBadge status={selected.status} />
              </div>

              <div className="grid grid-cols-3 gap-4 py-4 border-y border-gray-100">
                <div>
                  <p className="text-xs text-gray-500">Toplam Borç</p>
                  <p className="font-semibold text-gray-900">{formatMoney(selected.totalDebt)}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">Ödenen</p>
                  <p className="font-semibold text-green-600">{formatMoney(selected.paidAmount)}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">Kalan</p>
                  <p className="font-semibold text-gray-900">{formatMoney(selected.remainingDebt)}</p>
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-100">
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">No</th>
                      <th className="text-right py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
                      <th className="text-right py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Anapara</th>
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Son Ödeme</th>
                      <th className="text-left py-2 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selected.installments.map(inst => (
                      <tr key={inst.id} className="border-b border-gray-50 hover:bg-gray-50/50">
                        <td className="py-2.5 pr-3 text-gray-500">{inst.installmentNumber}</td>
                        <td className="py-2.5 pr-3 text-right font-medium text-gray-900">{formatMoney(inst.amount)}</td>
                        <td className="py-2.5 pr-3 text-right text-gray-500">{formatMoney(inst.principalAmount)}</td>
                        <td className="py-2.5 pr-3 text-gray-500">{formatDate(inst.dueDate)}</td>
                        <td className="py-2.5"><StatusBadge status={inst.status} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="card flex items-center justify-center h-48 text-sm text-gray-400">
              Taksit planını görmek için bir kredi seçin.
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
