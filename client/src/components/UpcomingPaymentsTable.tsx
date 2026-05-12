import { useNavigate } from 'react-router-dom'
import type { InstallmentResponseDto, LoanResponseDto } from '../api'
import { formatDate, formatMoney, loanTypeLabel } from '../api'
import { StatusBadge } from './StatusBadge'

type Props = {
  installments: InstallmentResponseDto[]
  loans: LoanResponseDto[]
}

function isPayable(inst: InstallmentResponseDto, allUnpaid: InstallmentResponseDto[]) {
  return !allUnpaid.some(
    i => i.loanId === inst.loanId && i.installmentNumber < inst.installmentNumber
  )
}

export function UpcomingPaymentsTable({ installments, loans }: Props) {
  const navigate = useNavigate()
  const upcoming = installments.slice(0, 8)

  if (upcoming.length === 0) {
    return <p className="text-sm text-gray-400 py-4 text-center">Bekleyen ödeme yok.</p>
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-gray-100">
            <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Kredi</th>
            <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">No</th>
            <th className="text-right py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Tutar</th>
            <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Tarih</th>
            <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Durum</th>
            <th className="py-2" />
          </tr>
        </thead>
        <tbody>
          {upcoming.map(inst => {
            const loan = loans.find(l => l.id === inst.loanId)
            const payable = isPayable(inst, installments)
            const isOverdue = inst.status === 'Overdue'

            return (
              <tr key={inst.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                <td className="py-3 pr-4 font-medium text-gray-900">
                  {loan ? loanTypeLabel(loan.type) + ' Kredisi' : '—'}
                </td>
                <td className="py-3 pr-4 text-gray-500">{inst.installmentNumber}</td>
                <td className="py-3 pr-4 text-right font-semibold text-gray-900">{formatMoney(inst.amount)}</td>
                <td className="py-3 pr-4 text-gray-500">{formatDate(inst.dueDate)}</td>
                <td className="py-3 pr-4">
                  <StatusBadge status={inst.status} />
                </td>
                <td className="py-3 text-right">
                  {payable && (
                    <button
                      type="button"
                      onClick={() => navigate('/customer/payments', { state: { installmentId: inst.id } })}
                      className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                        isOverdue
                          ? 'bg-red-500 hover:bg-red-600 text-white'
                          : 'bg-brand-navy hover:bg-brand-navyLight text-white'
                      }`}
                    >
                      {isOverdue ? 'Hemen Öde' : 'Öde'}
                    </button>
                  )}
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}
