import { Settings } from 'lucide-react'
import { useSession } from '../../SessionContext'

export default function AdminSettingsPage() {
  const { session } = useSession()

  return (
    <div className="max-w-2xl mx-auto px-6 py-8 space-y-6">
      <div>
        <h1 className="page-title">Ayarlar</h1>
        <p className="text-sm text-gray-500 mt-0.5">Sistem ayarları</p>
      </div>

      <div className="card flex flex-col items-center justify-center gap-4 py-16 text-center">
        <div className="w-16 h-16 rounded-2xl bg-gray-100 flex items-center justify-center">
          <Settings size={28} className="text-gray-400" />
        </div>
        <div>
          <p className="font-semibold text-gray-700">Admin Paneli</p>
          <p className="text-sm text-gray-400 mt-1">{session.email}</p>
        </div>
        <p className="text-sm text-gray-400 max-w-xs">
          Sistem ayarları şu an yapım aşamasında. Yakında daha fazla seçenek sunulacak.
        </p>
      </div>
    </div>
  )
}
