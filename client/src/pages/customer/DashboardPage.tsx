import { useCallback, useEffect, useState } from 'react'
import { RefreshCw } from 'lucide-react'
import { api, formatMoney } from '../../api'
import type { CustomerSummaryResponseDto, LoanResponseDto } from '../../api'
import { useSession } from '../../SessionContext'
import { MetricCard } from '../../components/MetricCard'
import { LoanCard } from '../../components/LoanCard'
import { CreditScoreGauge } from '../../components/CreditScoreGauge'
import { GreetingHero } from '../../components/GreetingHero'
import { UpcomingPaymentsTable } from '../../components/UpcomingPaymentsTable'
import { PaymentTimeline } from '../../components/PaymentTimeline'
import { useNavigate } from 'react-router-dom'

export default function CustomerDashboardPage() {
  const { session } = useSession()
  const navigate = useNavigate()
  const [summary, setSummary] = useState<CustomerSummaryResponseDto | null>(null)
  const [loans, setLoans] = useState<LoanResponseDto[]>([])
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const [s, l] = await Promise.all([
        api.mySummary(session.token),
        api.loans(session.token),
      ])
      setSummary(s)
      setLoans(l)
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Veri alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token])

  useEffect(() => { load() }, [load])

  const activeLoans = loans.filter(l => l.status === 'Active')
  const avgScore = activeLoans.length > 0
    ? Math.round(activeLoans.reduce((s, l) => s + l.creditScore, 0) / activeLoans.length)
    : 0

  const overdueSubtitle = summary
    ? summary.unpaidInstallments
        .filter(i => i.status === 'Overdue')
        .slice(0, 2)
        .map((i, idx) => `No ${idx + 1}: ${formatMoney(i.amount)} (${new Date(i.dueDate).toLocaleDateString('tr-TR')})`)
        .join('\n')
    : undefined

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="text-sm text-gray-500 mt-0.5">Finansal durumunuza genel bakış</p>
        </div>
        <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
          <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          Yenile
        </button>
      </div>

      {error && <div className="alert-error">{error}</div>}

      {/* Top row: greeting + credit score */}
      <div className="grid grid-cols-3 gap-6">
        <div className="col-span-2">
          <GreetingHero name={summary?.fullName ?? session.email} />
        </div>
        <div className="card flex flex-col items-center justify-center gap-2">
          <p className="text-sm font-medium text-gray-600">Kredi Skoru</p>
          {avgScore > 0 ? (
            <CreditScoreGauge score={avgScore} />
          ) : (
            <p className="text-sm text-gray-400">Aktif kredi yok</p>
          )}
        </div>
      </div>

      {/* Finansal Genel Bakış */}
      <div>
        <h2 className="section-title mb-3">Finansal Genel Bakış</h2>
        <div className="grid grid-cols-3 gap-4">
          <MetricCard
            title="Toplam Borç"
            value={formatMoney(summary?.totalLoanDebt ?? 0)}
            variant="mint"
          />
          <MetricCard
            title="Kalan Anapara"
            value={formatMoney(summary?.remainingPrincipal ?? 0)}
            variant="aqua"
          />
          <MetricCard
            title="Gecikmiş Taksitler"
            value={(summary?.overdueInstallmentCount ?? 0).toString()}
            subtitle={overdueSubtitle}
            variant="peach"
            alert={(summary?.overdueInstallmentCount ?? 0) > 0}
          />
        </div>
      </div>

      {/* Bottom: active loans + upcoming payments */}
      <div className="grid grid-cols-5 gap-6">
        {/* Active loans */}
        <div className="col-span-2">
          <h2 className="section-title mb-3">Aktif Kredilerim</h2>
          {activeLoans.length === 0 ? (
            <div className="card text-center text-sm text-gray-400">Aktif kredi bulunamadı.</div>
          ) : (
            <div className="space-y-4">
              {activeLoans.map(loan => (
                <LoanCard
                  key={loan.id}
                  loan={loan}
                  onDetail={() => navigate('/customer/loans', { state: { loanId: loan.id } })}
                />
              ))}
            </div>
          )}
        </div>

        {/* Upcoming payments */}
        <div className="col-span-3">
          <h2 className="section-title mb-3">Yaklaşan Ödemeler</h2>
          <div className="card space-y-4">
            <UpcomingPaymentsTable
              installments={summary?.unpaidInstallments ?? []}
              loans={loans}
            />
            {(summary?.unpaidInstallments?.length ?? 0) > 0 && (
              <div className="pt-2 border-t border-gray-100">
                <PaymentTimeline installments={summary!.unpaidInstallments} />
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
