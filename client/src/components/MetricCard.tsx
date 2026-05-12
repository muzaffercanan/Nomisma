import type { ReactNode } from 'react'
import { AlertTriangle } from 'lucide-react'

type Variant = 'mint' | 'aqua' | 'peach' | 'lavender' | 'blue' | 'default'

const variantClasses: Record<Variant, string> = {
  mint: 'bg-card-mint',
  aqua: 'bg-card-aqua',
  peach: 'bg-card-peach',
  lavender: 'bg-card-lavender',
  blue: 'bg-card-blue',
  default: 'bg-white',
}

type Props = {
  title: string
  value: string
  subtitle?: string
  icon?: ReactNode
  variant?: Variant
  alert?: boolean
}

export function MetricCard({ title, value, subtitle, icon, variant = 'default', alert }: Props) {
  return (
    <div className={`rounded-2xl shadow-card p-6 flex flex-col gap-3 ${variantClasses[variant]}`}>
      <div className="flex items-start justify-between">
        <span className="text-sm font-medium text-gray-600">{title}</span>
        <div className="flex items-center gap-1">
          {alert && <AlertTriangle size={16} className="text-red-500" />}
          {icon && <span className="text-gray-400">{icon}</span>}
        </div>
      </div>
      <div>
        <p className="text-2xl font-bold text-gray-900 leading-tight">{value}</p>
        {subtitle && <p className="text-xs text-gray-500 mt-1">{subtitle}</p>}
      </div>
    </div>
  )
}
