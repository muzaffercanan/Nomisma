import { Landmark, GraduationCap, Car } from 'lucide-react'
import type { LoanResponseDto } from '../api'
import { formatMoney } from '../api'
import { StatusBadge } from './StatusBadge'

const typeConfig = {
  Personal: { icon: Landmark, bg: 'bg-teal-100', color: 'text-teal-600', label: 'İhtiyaç Kredisi' },
  Education: { icon: GraduationCap, bg: 'bg-blue-100', color: 'text-blue-600', label: 'Eğitim Kredisi' },
  Vehicle: { icon: Car, bg: 'bg-purple-100', color: 'text-purple-600', label: 'Taşıt Kredisi' },
}

type Props = {
  loan: LoanResponseDto
  selected?: boolean
  onDetail?: () => void
}

export function LoanCard({ loan, selected, onDetail }: Props) {
  const cfg = typeConfig[loan.type]
  const Icon = cfg.icon
  const paidCount = loan.installments.filter(i => i.status === 'Paid').length
  const paidPct = loan.termMonths > 0 ? Math.round((paidCount / loan.termMonths) * 100) : 0
  const nextUnpaid = loan.installments.find(i => i.status !== 'Paid')

  return (
    <div className={`bg-white rounded-2xl shadow-card p-5 flex flex-col gap-4 transition-all ${selected ? 'ring-2 ring-brand-teal' : 'hover:shadow-card-lg'}`}>
      <div className="flex items-center justify-between">
        <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${cfg.bg}`}>
          <Icon size={20} className={cfg.color} />
        </div>
        <StatusBadge status={loan.status} />
      </div>

      <div>
        <p className="font-semibold text-gray-900">{cfg.label}</p>
        <p className="text-xl font-bold text-gray-900 mt-0.5">{formatMoney(loan.totalDebt)}</p>
        <p className="text-xs text-gray-500 mt-1">
          Toplam Borç · {paidCount} / {loan.termMonths} Ay
        </p>
      </div>

      <div>
        <div className="flex justify-between text-xs text-gray-500 mb-1">
          <span>İlerleme</span>
          <span>{paidPct}%</span>
        </div>
        <div className="w-full bg-gray-100 rounded-full h-1.5">
          <div
            className="bg-brand-teal rounded-full h-1.5 transition-all"
            style={{ width: `${paidPct}%` }}
          />
        </div>
      </div>

      {nextUnpaid && (
        <p className="text-xs text-gray-500">
          Sonraki Taksit <span className="font-semibold text-gray-900">{formatMoney(nextUnpaid.amount)}</span>
        </p>
      )}

      {onDetail && (
        <button
          type="button"
          onClick={onDetail}
          className={`w-full py-2 rounded-lg text-sm font-medium transition-colors ${selected ? 'bg-brand-teal text-white' : 'border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
        >
          Detay
        </button>
      )}
    </div>
  )
}
