const labels: Record<string, string> = {
  Active: 'Aktif',
  Closed: 'Kapatıldı',
  Paid: 'Ödendi',
  Unpaid: 'Ödenmedi',
  Overdue: 'Gecikmiş',
  Completed: 'Tamamlandı',
  Failed: 'Başarısız',
  NotSent: 'Gönderilmedi',
  Approved: 'Onaylandı',
  Declined: 'Reddedildi',
}

const styles: Record<string, string> = {
  Active: 'bg-teal-100 text-teal-700',
  Closed: 'bg-gray-100 text-gray-500',
  Paid: 'bg-green-100 text-green-700',
  Unpaid: 'bg-amber-100 text-amber-700',
  Overdue: 'bg-red-100 text-red-700',
  Completed: 'bg-green-100 text-green-700',
  Failed: 'bg-red-100 text-red-700',
  NotSent: 'bg-gray-100 text-gray-500',
  Approved: 'bg-green-100 text-green-700',
  Declined: 'bg-red-100 text-red-700',
}

type Props = { status: string }

export function StatusBadge({ status }: Props) {
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${styles[status] ?? 'bg-gray-100 text-gray-600'}`}>
      {labels[status] ?? status}
    </span>
  )
}
