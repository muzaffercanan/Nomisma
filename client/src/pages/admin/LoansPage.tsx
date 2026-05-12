import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { RefreshCw, Plus, Landmark } from 'lucide-react'
import { api, formatDate, formatMoney, loanTypeLabel } from '../../api'
import type { CustomerResponseDto, LoanResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { StatusBadge } from '../../components/StatusBadge'

type LoanForm = {
  customerId: string
  type: 'Personal' | 'Education' | 'Vehicle'
  principalAmount: string
  profitRate: string
  termMonths: string
  startDate: string
}

const emptyForm: LoanForm = {
  customerId: '', type: 'Personal',
  principalAmount: '25000', profitRate: '18',
  termMonths: '12', startDate: new Date().toISOString().slice(0, 10),
}

export default function AdminLoansPage() {
  const { session } = useSession()
  const [customers, setCustomers] = useState<CustomerResponseDto[]>([])
  const [loans, setLoans] = useState<LoanResponseDto[]>([])
  const [filterCustomerId, setFilterCustomerId] = useState('')
  const [selectedLoan, setSelectedLoan] = useState<LoanResponseDto | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState<LoanForm>(emptyForm)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [c, l] = await Promise.all([
        api.customers(session.token),
        api.loans(session.token),
      ])
      setCustomers(c)
      setLoans(l)
      if (!form.customerId && c[0]) setForm(f => ({ ...f, customerId: c[0].id }))
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Veri alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token])

  useEffect(() => { load() }, [load])

  const createLoan = async (e: FormEvent) => {
    e.preventDefault()
    setError(''); setNotice('')
    try {
      const created = await api.createLoan(session.token, {
        customerId: form.customerId,
        type: form.type,
        principalAmount: Number(form.principalAmount),
        profitRate: Number(form.profitRate),
        termMonths: Number(form.termMonths),
        startDate: form.startDate,
      })
      setSelectedLoan(created)
      setNotice('Kredi oluşturuldu.')
      setShowForm(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kredi oluşturulamadı.')
    }
  }

  const closeLoan = async (id: string) => {
    if (!confirm('Bu krediyi kapatmak istediğinizden emin misiniz?')) return
    setError(''); setNotice('')
    try {
      await api.closeLoan(session.token, id)
      setNotice('Kredi kapatıldı.')
      setSelectedLoan(null)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kredi kapatılamadı.')
    }
  }

  const filtered = filterCustomerId ? loans.filter(l => l.customerId === filterCustomerId) : loans

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Krediler</h1>
          <p className="text-sm text-gray-500 mt-0.5">{loans.length} kredi kaydı</p>
        </div>
        <div className="flex gap-2">
          <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
            <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
            Yenile
          </button>
          <button type="button" onClick={() => setShowForm(v => !v)} className="btn-primary">
            <Plus size={16} />
            Yeni Kredi
          </button>
        </div>
      </div>

      {error && <div className="alert-error">{error}</div>}
      {notice && <div className="alert-success">{notice}</div>}

      {/* Create form */}
      {showForm && (
        <div className="card">
          <h2 className="section-title mb-4">Kredi Oluştur</h2>
          <form onSubmit={createLoan} className="grid grid-cols-3 gap-4">
            <div>
              <label className="label">Müşteri</label>
              <select className="input" value={form.customerId} onChange={e => setForm({ ...form, customerId: e.target.value })} required>
                <option value="">Seçiniz</option>
                {customers.map(c => (
                  <option key={c.id} value={c.id}>{c.customerNumber} — {c.fullName}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="label">Kredi Türü</label>
              <select className="input" value={form.type} onChange={e => setForm({ ...form, type: e.target.value as LoanForm['type'] })}>
                <option value="Personal">İhtiyaç</option>
                <option value="Education">Eğitim</option>
                <option value="Vehicle">Taşıt</option>
              </select>
            </div>
            <div>
              <label className="label">Vade (Ay)</label>
              <input className="input" type="number" min={1} value={form.termMonths} onChange={e => setForm({ ...form, termMonths: e.target.value })} required />
            </div>
            <div>
              <label className="label">Anapara (₺)</label>
              <input className="input" type="number" min={1} value={form.principalAmount} onChange={e => setForm({ ...form, principalAmount: e.target.value })} required />
            </div>
            <div>
              <label className="label">Kâr Oranı (%)</label>
              <input className="input" type="number" min={0} step={0.01} value={form.profitRate} onChange={e => setForm({ ...form, profitRate: e.target.value })} required />
            </div>
            <div>
              <label className="label">Başlangıç</label>
              <input className="input" type="date" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} required />
            </div>
            <div className="col-span-3 flex justify-end gap-2">
              <button type="button" className="btn-secondary" onClick={() => setShowForm(false)}>İptal</button>
              <button type="submit" className="btn-primary">
                <Plus size={16} />
                Kredi Aç
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Filter */}
      <div className="flex items-center gap-3">
        <label className="text-sm font-medium text-gray-700">Müşteri filtresi:</label>
        <select
          className="input w-64"
          value={filterCustomerId}
          onChange={e => { setFilterCustomerId(e.target.value); setSelectedLoan(null) }}
        >
          <option value="">Tüm müşteriler</option>
          {customers.map(c => (
            <option key={c.id} value={c.id}>{c.customerNumber} — {c.fullName}</option>
          ))}
        </select>
        {filterCustomerId && (
          <button type="button" className="btn-ghost text-xs" onClick={() => setFilterCustomerId('')}>Temizle</button>
        )}
      </div>

      <div className="grid grid-cols-5 gap-6">
        {/* Loan list */}
        <div className="col-span-2">
          <div className="space-y-2">
            {filtered.length === 0 ? (
              <div className="card text-center text-sm text-gray-400 py-8">
                <Landmark size={32} className="mx-auto mb-2 text-gray-200" />
                Kredi bulunamadı.
              </div>
            ) : filtered.map(loan => {
              const customer = customers.find(c => c.id === loan.customerId)
              return (
                <div
                  key={loan.id}
                  className={`card cursor-pointer transition-all ${selectedLoan?.id === loan.id ? 'ring-2 ring-brand-teal' : 'hover:shadow-card-lg'}`}
                  onClick={() => setSelectedLoan(loan)}
                >
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="font-semibold text-gray-900">{loanTypeLabel(loan.type)} Kredisi</p>
                      <p className="text-xs text-gray-500 mt-0.5">{customer?.fullName ?? '—'}</p>
                    </div>
                    <StatusBadge status={loan.status} />
                  </div>
                  <div className="flex justify-between mt-3 text-sm">
                    <span className="text-gray-500">Toplam</span>
                    <span className="font-semibold text-gray-900">{formatMoney(loan.totalDebt)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Kalan</span>
                    <span className="font-medium text-gray-700">{formatMoney(loan.remainingDebt)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">Skor</span>
                    <span className="font-medium text-gray-700">{loan.creditScore}</span>
                  </div>
                </div>
              )
            })}
          </div>
        </div>

        {/* Installment plan */}
        <div className="col-span-3">
          {selectedLoan ? (
            <div className="card space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="section-title">{loanTypeLabel(selectedLoan.type)} Kredisi — Taksit Planı</h2>
                  <p className="text-sm text-gray-500">{selectedLoan.termMonths} taksit · {formatMoney(selectedLoan.totalDebt)}</p>
                </div>
                {selectedLoan.status === 'Active' && (
                  <button type="button" className="btn-danger" onClick={() => closeLoan(selectedLoan.id)}>
                    Krediyi Kapat
                  </button>
                )}
              </div>
              <div className="grid grid-cols-3 gap-4 py-4 border-y border-gray-100">
                <div>
                  <p className="text-xs text-gray-500">Toplam Borç</p>
                  <p className="font-semibold text-gray-900">{formatMoney(selectedLoan.totalDebt)}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">Ödenen</p>
                  <p className="font-semibold text-green-600">{formatMoney(selectedLoan.paidAmount)}</p>
                </div>
                <div>
                  <p className="text-xs text-gray-500">Kalan</p>
                  <p className="font-semibold text-gray-900">{formatMoney(selectedLoan.remainingDebt)}</p>
                </div>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-100">
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">No</th>
                      <th className="text-right py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Son Ödeme</th>
                      <th className="text-left py-2 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedLoan.installments.map(inst => (
                      <tr key={inst.id} className="border-b border-gray-50 hover:bg-gray-50/50">
                        <td className="py-2.5 pr-3 text-gray-500">{inst.installmentNumber}</td>
                        <td className="py-2.5 pr-3 text-right font-medium text-gray-900">{formatMoney(inst.amount)}</td>
                        <td className="py-2.5 pr-3 text-gray-500">{formatDate(inst.dueDate)}</td>
                        <td className="py-2.5"><StatusBadge status={inst.status} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="card flex flex-col items-center justify-center gap-3 h-64 text-center">
              <Landmark size={36} className="text-gray-200" />
              <p className="text-sm text-gray-400">Taksit planını görmek için bir kredi seçin.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
