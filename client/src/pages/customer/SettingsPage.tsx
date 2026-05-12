import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Save } from 'lucide-react'
import { api } from '../../api'
import type { CustomerResponseDto } from '../../api'
import { useSession } from '../../SessionContext'

type Form = {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  address: string
  dateOfBirth: string
}

export default function CustomerSettingsPage() {
  const { session } = useSession()
  const [profile, setProfile] = useState<CustomerResponseDto | null>(null)
  const [form, setForm] = useState<Form | null>(null)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    if (!session.customerId) return
    try {
      const data = await api.customer(session.token, session.customerId)
      setProfile(data)
      setForm({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        phoneNumber: data.phoneNumber,
        address: data.address,
        dateOfBirth: data.dateOfBirth,
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Profil alınamadı.')
    }
  }, [session.token, session.customerId])

  useEffect(() => { load() }, [load])

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    if (!form || !session.customerId) return
    setSaving(true)
    setError('')
    setNotice('')
    try {
      await api.updateCustomer(session.token, session.customerId, form)
      setNotice('Profil güncellendi.')
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Güncelleme başarısız.')
    } finally {
      setSaving(false)
    }
  }

  if (!form) return (
    <div className="max-w-2xl mx-auto px-6 py-8">
      <h1 className="page-title">Hesap Ayarları</h1>
      {error && <div className="alert-error mt-4">{error}</div>}
    </div>
  )

  return (
    <div className="max-w-2xl mx-auto px-6 py-8 space-y-6">
      <div>
        <h1 className="page-title">Hesap Ayarları</h1>
        <p className="text-sm text-gray-500 mt-0.5">Profil bilgilerinizi güncelleyin</p>
      </div>

      {error && <div className="alert-error">{error}</div>}
      {notice && <div className="alert-success">{notice}</div>}

      {profile && (
        <div className="card flex items-center gap-4">
          <div className="w-14 h-14 rounded-full bg-brand-teal flex items-center justify-center flex-shrink-0">
            <span className="text-white text-lg font-bold">
              {profile.firstName[0]}{profile.lastName[0]}
            </span>
          </div>
          <div>
            <p className="font-semibold text-gray-900">{profile.fullName}</p>
            <p className="text-sm text-gray-500">Müşteri No: {profile.customerNumber}</p>
            <p className="text-sm text-gray-500">TC: {profile.nationalId}</p>
          </div>
        </div>
      )}

      <form onSubmit={submit} className="card space-y-4">
        <h2 className="section-title">Kişisel Bilgiler</h2>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">Ad</label>
            <input className="input" value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} />
          </div>
          <div>
            <label className="label">Soyad</label>
            <input className="input" value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} />
          </div>
        </div>
        <div>
          <label className="label">E-posta</label>
          <input className="input" type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="label">Telefon</label>
            <input className="input" value={form.phoneNumber} onChange={e => setForm({ ...form, phoneNumber: e.target.value })} />
          </div>
          <div>
            <label className="label">Doğum Tarihi</label>
            <input className="input" type="date" value={form.dateOfBirth} onChange={e => setForm({ ...form, dateOfBirth: e.target.value })} />
          </div>
        </div>
        <div>
          <label className="label">Adres</label>
          <input className="input" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
        </div>

        <div className="pt-2 flex justify-end">
          <button type="submit" className="btn-primary" disabled={saving}>
            <Save size={16} />
            {saving ? 'Kaydediliyor…' : 'Değişiklikleri Kaydet'}
          </button>
        </div>
      </form>
    </div>
  )
}
