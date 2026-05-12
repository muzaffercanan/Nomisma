import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useLocation } from 'react-router-dom'
import { CreditCard, RefreshCw } from 'lucide-react'
import { api, formatDate, formatMoney, loanTypeLabel } from '../../api'
import type { CustomerSummaryResponseDto, InstallmentResponseDto, LoanResponseDto, PaymentResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { StatusBadge } from '../../components/StatusBadge'

type CardForm = {
  cardHolderName: string
  cardNumber: string
  cvv: string
  expiryMonth: string
  expiryYear: string
}

const defaultCard: CardForm = {
  cardHolderName: 'Demo Customer',
  cardNumber: '4111111111111111',
  cvv: '123',
  expiryMonth: '12',
  expiryYear: '2030',
}

function isPayable(inst: InstallmentResponseDto, allUnpaid: InstallmentResponseDto[]) {
  return !allUnpaid.some(i => i.loanId === inst.loanId && i.installmentNumber < inst.installmentNumber)
}

export default function CustomerPaymentsPage() {
  const { session } = useSession()
  const location = useLocation()
  const preselectedId = (location.state as { installmentId?: string } | null)?.installmentId ?? ''

  const [summary, setSummary] = useState<CustomerSummaryResponseDto | null>(null)
  const [loans, setLoans] = useState<LoanResponseDto[]>([])
  const [payments, setPayments] = useState<PaymentResponseDto[]>([])
  const [target, setTarget] = useState<InstallmentResponseDto | null>(null)
  const [cardForm, setCardForm] = useState<CardForm>(defaultCard)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [paying, setPaying] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [s, l, p] = await Promise.all([
        api.mySummary(session.token),
        api.loans(session.token),
        api.payments(session.token),
      ])
      setSummary(s)
      setLoans(l)
      setPayments(p)
      if (preselectedId) {
        const found = s.unpaidInstallments.find(i => i.id === preselectedId)
        if (found) setTarget(found)
      }
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Veri alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token, preselectedId])

  useEffect(() => { load() }, [load])

  const submitPayment = async (e: FormEvent) => {
    e.preventDefault()
    if (!target) return
    setPaying(true)
    setError('')
    setNotice('')
    try {
      await api.createPayment(session.token, {
        installmentId: target.id,
        amount: target.amount,
        cardHolderName: cardForm.cardHolderName,
        cardNumber: cardForm.cardNumber,
        cvv: cardForm.cvv,
        expiryMonth: Number(cardForm.expiryMonth),
        expiryYear: Number(cardForm.expiryYear),
      })
      setNotice('Ödeme başarıyla alındı.')
      setTarget(null)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ödeme alınamadı.')
    } finally {
      setPaying(false)
    }
  }

  const unpaid = summary?.unpaidInstallments ?? []

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Ödemelerim</h1>
          <p className="text-sm text-gray-500 mt-0.5">{unpaid.length} bekleyen taksit</p>
        </div>
        <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
          <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          Yenile
        </button>
      </div>

      {error && <div className="alert-error">{error}</div>}
      {notice && <div className="alert-success">{notice}</div>}

      <div className="grid grid-cols-5 gap-6">
        {/* Bekleyen taksitler */}
        <div className="col-span-3 space-y-4">
          <h2 className="section-title">Bekleyen Taksitler</h2>
          {unpaid.length === 0 ? (
            <div className="card text-center text-sm text-gray-400 py-8">Bekleyen taksit yok. 🎉</div>
          ) : (
            <div className="card overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-100">
                    <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Kredi</th>
                    <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">No</th>
                    <th className="text-right py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
                    <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tarih</th>
                    <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
                    <th className="py-2" />
                  </tr>
                </thead>
                <tbody>
                  {unpaid.map(inst => {
                    const loan = loans.find(l => l.id === inst.loanId)
                    const payable = isPayable(inst, unpaid)
                    return (
                      <tr key={inst.id} className={`border-b border-gray-50 transition-colors ${target?.id === inst.id ? 'bg-teal-50' : 'hover:bg-gray-50/50'}`}>
                        <td className="py-3 pr-3 font-medium text-gray-900">
                          {loan ? loanTypeLabel(loan.type) + ' Kredisi' : '—'}
                        </td>
                        <td className="py-3 pr-3 text-gray-500">{inst.installmentNumber}</td>
                        <td className="py-3 pr-3 text-right font-semibold text-gray-900">{formatMoney(inst.amount)}</td>
                        <td className="py-3 pr-3 text-gray-500">{formatDate(inst.dueDate)}</td>
                        <td className="py-3 pr-3"><StatusBadge status={inst.status} /></td>
                        <td className="py-3 text-right">
                          {payable ? (
                            <button
                              type="button"
                              onClick={() => setTarget(inst)}
                              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                                target?.id === inst.id
                                  ? 'bg-brand-teal text-white'
                                  : inst.status === 'Overdue'
                                  ? 'bg-red-500 hover:bg-red-600 text-white'
                                  : 'bg-brand-navy hover:bg-brand-navyLight text-white'
                              }`}
                            >
                              {target?.id === inst.id ? 'Seçildi' : inst.status === 'Overdue' ? 'Hemen Öde' : 'Öde'}
                            </button>
                          ) : (
                            <span className="text-xs text-gray-400">Bekliyor</span>
                          )}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}

          {/* Payment history */}
          {payments.length > 0 && (
            <>
              <h2 className="section-title">Ödeme Geçmişi</h2>
              <div className="card overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-gray-100">
                      <th className="text-right py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Tarih</th>
                      <th className="text-left py-2 pr-3 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
                      <th className="text-left py-2 text-xs font-medium text-gray-500 uppercase tracking-wide">İşlem No</th>
                    </tr>
                  </thead>
                  <tbody>
                    {payments.slice(0, 20).map(p => (
                      <tr key={p.id} className="border-b border-gray-50 hover:bg-gray-50/50">
                        <td className="py-2.5 pr-3 text-right font-semibold text-gray-900">{formatMoney(p.amount)}</td>
                        <td className="py-2.5 pr-3 text-gray-500">{formatDate(p.paidAtUtc)}</td>
                        <td className="py-2.5 pr-3"><StatusBadge status={p.status} /></td>
                        <td className="py-2.5 text-xs text-gray-400 font-mono">{p.gatewayTransactionId?.slice(0, 12) ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>

        {/* Payment form */}
        <div className="col-span-2">
          <h2 className="section-title mb-3">Ödeme Yap</h2>
          {target ? (
            <div className="card space-y-4">
              <div className="bg-brand-navy rounded-xl p-4 text-white">
                <p className="text-white/60 text-xs">Ödenecek tutar</p>
                <p className="text-2xl font-bold mt-1">{formatMoney(target.amount)}</p>
                <p className="text-white/60 text-xs mt-2">
                  {target.installmentNumber}. taksit · {formatDate(target.dueDate)}
                </p>
              </div>

              <form onSubmit={submitPayment} className="space-y-3">
                <div>
                  <label className="label">Kart Sahibi</label>
                  <input className="input" value={cardForm.cardHolderName} onChange={e => setCardForm({ ...cardForm, cardHolderName: e.target.value })} />
                </div>
                <div>
                  <label className="label">Kart Numarası</label>
                  <input className="input" value={cardForm.cardNumber} onChange={e => setCardForm({ ...cardForm, cardNumber: e.target.value })} maxLength={16} />
                </div>
                <div className="grid grid-cols-3 gap-2">
                  <div>
                    <label className="label">CVV</label>
                    <input className="input" value={cardForm.cvv} onChange={e => setCardForm({ ...cardForm, cvv: e.target.value })} maxLength={4} />
                  </div>
                  <div>
                    <label className="label">Ay</label>
                    <input className="input" type="number" min={1} max={12} value={cardForm.expiryMonth} onChange={e => setCardForm({ ...cardForm, expiryMonth: e.target.value })} />
                  </div>
                  <div>
                    <label className="label">Yıl</label>
                    <input className="input" type="number" min={2026} value={cardForm.expiryYear} onChange={e => setCardForm({ ...cardForm, expiryYear: e.target.value })} />
                  </div>
                </div>

                <div className="flex gap-2 pt-1">
                  <button type="submit" className="btn-primary flex-1 justify-center" disabled={paying}>
                    <CreditCard size={16} />
                    {paying ? 'İşleniyor…' : 'Ödemeyi Tamamla'}
                  </button>
                  <button type="button" className="btn-secondary" onClick={() => setTarget(null)}>
                    Vazgeç
                  </button>
                </div>
              </form>
            </div>
          ) : (
            <div className="card flex flex-col items-center justify-center gap-3 h-48 text-center">
              <CreditCard size={32} className="text-gray-300" />
              <p className="text-sm text-gray-400">Ödemek istediğiniz taksiti sol tablodan seçin.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
