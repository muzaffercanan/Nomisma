type Props = { score: number; max?: number }

function scoreColor(score: number) {
  if (score >= 750) return '#10B981'
  if (score >= 600) return '#F59E0B'
  return '#EF4444'
}

function scoreLabel(score: number) {
  if (score >= 750) return 'İyi'
  if (score >= 600) return 'Orta'
  return 'Düşük'
}

export function CreditScoreGauge({ score, max = 900 }: Props) {
  const pct = Math.min(Math.max(score / max, 0), 1)
  const arcLen = 157
  const offset = arcLen * (1 - pct)
  const color = scoreColor(score)

  return (
    <div className="flex flex-col items-center">
      <div className="relative w-32 h-20">
        <svg viewBox="0 0 120 70" className="w-full h-full">
          <path
            d="M 10 65 A 50 50 0 0 1 110 65"
            fill="none"
            stroke="#E5E7EB"
            strokeWidth="10"
            strokeLinecap="round"
          />
          <path
            d="M 10 65 A 50 50 0 0 1 110 65"
            fill="none"
            stroke={color}
            strokeWidth="10"
            strokeLinecap="round"
            strokeDasharray={arcLen}
            strokeDashoffset={offset}
          />
        </svg>
        <div className="absolute inset-0 flex flex-col items-center justify-end pb-1">
          <span className="text-2xl font-bold text-gray-900 leading-none">{score}</span>
        </div>
      </div>
      <div className="flex items-center gap-2 mt-1">
        <span className="text-xs text-gray-400">/ {max}</span>
        <span className="text-xs font-semibold" style={{ color }}>{scoreLabel(score)}</span>
        <span className="text-xs text-gray-400">Mock Skor</span>
      </div>
    </div>
  )
}
