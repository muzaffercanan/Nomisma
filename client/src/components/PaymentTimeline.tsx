import type { InstallmentResponseDto } from '../api'
import { formatDate } from '../api'

type Props = { installments: InstallmentResponseDto[]; max?: number }

export function PaymentTimeline({ installments, max = 5 }: Props) {
  const items = installments.slice(0, max)
  if (items.length === 0) return null

  return (
    <div className="relative flex items-center gap-0 overflow-x-auto py-2">
      {items.map((inst, idx) => {
        const isLast = idx === items.length - 1
        const isOverdue = inst.status === 'Overdue'
        const isFirst = idx === 0

        return (
          <div key={inst.id} className="flex items-center flex-shrink-0">
            <div className="flex flex-col items-center">
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center border-2 transition-colors text-xs font-bold ${
                  isFirst
                    ? 'bg-brand-navy border-brand-navy text-white'
                    : isOverdue
                    ? 'bg-red-500 border-red-500 text-white'
                    : 'bg-white border-gray-300 text-gray-500'
                }`}
              >
                {idx + 1}
              </div>
              <span className="text-xs text-gray-400 mt-1 whitespace-nowrap">{formatDate(inst.dueDate)}</span>
            </div>
            {!isLast && (
              <div className="w-12 h-0.5 bg-gray-200 mx-1 mb-5 flex-shrink-0" />
            )}
          </div>
        )
      })}
    </div>
  )
}
