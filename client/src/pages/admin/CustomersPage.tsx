import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { RefreshCw, Save, Trash2, UserPlus, Users } from 'lucide-react'
import { api, formatDate } from '../../api'
import type { CustomerResponseDto } from '../../api'
import { useSession } from '../../SessionContext'

type Form = {
  firstName: string
  lastName: string
  nationalId: string
  email: string
  phoneNumber: string
  address: string
  dateOfBirth: string
  password: string
}

const empty: Form = {
  firstName: '', lastName: '', nationalId: '', email: '',
  phoneNumber: '', address: '', dateOfBirth: '1990-01-01', password: 'Customer123!',
}

export default function AdminCustomersPage() {
  const { session } = useSession()
  const [customers, setCustomers] = useState<CustomerResponseDto[]>([])
  const [selected, setSelected] = useState<CustomerResponseDto | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<Form>(empty)
  const [showForm, setShowForm] = useState(false)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.customers(session.token)
      setCustomers(data)
      setError('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Müşteriler alınamadı.')
    } finally {
      setLoading(false)
    }
  }, [session.token])

  useEffect(() => { load() }, [load])

  const startEdit = (c: CustomerResponseDto) => {
    setEditingId(c.id)
    setForm({ firstName: c.firstName, lastName: c.lastName, nationalId: c.nationalId,
      email: c.email, phoneNumber: c.phoneNumber, address: c.address,
      dateOfBirth: c.dateOfBirth, password: '' })
    setShowForm(true)
    setSelected(c)
  }

  const cancelForm = () => {
    setEditingId(null)
    setForm(empty)
    setShowForm(false)
  }

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError(''); setNotice('')
    try {
      if (editingId) {
        await api.updateCustomer(session.token, editingId, form)
        setNotice('Müşteri güncellendi.')
      } else {
        const created = await api.createCustomer(session.token, form)
        setSelected(created)
        setNotice('Müşteri oluşturuldu.')
      }
      cancelForm()
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'İşlem başarısız.')
    }
  }

  const del = async (id: string) => {
    if (!confirm('Bu müşteriyi silmek istediğinizden emin misiniz?')) return
    setError(''); setNotice('')
    try {
      await api.deleteCustomer(session.token, id)
      setNotice('Müşteri silindi.')
      if (selected?.id === id) setSelected(null)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Silme başarısız.')
    }
  }

  return (
    <div className="max-w-7xl mx-auto px-6 py-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="page-title">Müşteriler</h1>
          <p className="text-sm text-gray-500 mt-0.5">{customers.length} müşteri kayıtlı</p>
        </div>
        <div className="flex gap-2">
          <button type="button" onClick={load} className="btn-secondary" disabled={loading}>
            <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
            Yenile
          </button>
          <button type="button" onClick={() => { cancelForm(); setShowForm(true) }} className="btn-primary">
            <UserPlus size={16} />
            Yeni Müşteri
          </button>
        </div>
      </div>

      {error && <div className="alert-error">{error}</div>}
      {notice && <div className="alert-success">{notice}</div>}

      <div className="grid grid-cols-5 gap-6">
        {/* Customer list */}
        <div className={showForm ? 'col-span-3' : 'col-span-5'}>
          <div className="card overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Ad Soyad</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Müşteri No</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">E-posta</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Telefon</th>
                  <th className="text-left py-2 pr-4 text-xs font-medium text-gray-500 uppercase tracking-wide">Kayıt Tarihi</th>
                  <th className="py-2" />
                </tr>
              </thead>
              <tbody>
                {customers.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="py-8 text-center text-gray-400">
                      <Users size={32} className="mx-auto mb-2 text-gray-200" />
                      Müşteri bulunamadı.
                    </td>
                  </tr>
                ) : customers.map(c => (
                  <tr
                    key={c.id}
                    className={`border-b border-gray-50 transition-colors cursor-pointer ${selected?.id === c.id ? 'bg-teal-50' : 'hover:bg-gray-50/50'}`}
                    onClick={() => setSelected(c)}
                  >
                    <td className="py-3 pr-4 font-medium text-gray-900">{c.fullName}</td>
                    <td className="py-3 pr-4 text-gray-500 font-mono text-xs">{c.customerNumber}</td>
                    <td className="py-3 pr-4 text-gray-500">{c.email}</td>
                    <td className="py-3 pr-4 text-gray-500">{c.phoneNumber}</td>
                    <td className="py-3 pr-4 text-gray-500">{formatDate(c.createdAtUtc)}</td>
                    <td className="py-3 text-right">
                      <div className="flex justify-end gap-1">
                        <button type="button" onClick={e => { e.stopPropagation(); startEdit(c) }} className="btn-ghost py-1 px-2 text-xs">Düzenle</button>
                        <button type="button" onClick={e => { e.stopPropagation(); del(c.id) }} className="btn-danger py-1 px-2">
                          <Trash2 size={13} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Form panel */}
        {showForm && (
          <div className="col-span-2">
            <div className="card space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="section-title">{editingId ? 'Müşteri Düzenle' : 'Yeni Müşteri'}</h2>
                <button type="button" onClick={cancelForm} className="btn-ghost text-xs px-2 py-1">İptal</button>
              </div>
              <form onSubmit={submit} className="space-y-3">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="label">Ad</label>
                    <input className="input" value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} required />
                  </div>
                  <div>
                    <label className="label">Soyad</label>
                    <input className="input" value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} required />
                  </div>
                </div>
                <div>
                  <label className="label">TC Kimlik No</label>
                  <input className="input" value={form.nationalId} disabled={!!editingId} onChange={e => setForm({ ...form, nationalId: e.target.value })} required={!editingId} />
                </div>
                <div>
                  <label className="label">E-posta</label>
                  <input className="input" type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} required />
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="label">Telefon</label>
                    <input className="input" value={form.phoneNumber} onChange={e => setForm({ ...form, phoneNumber: e.target.value })} required />
                  </div>
                  <div>
                    <label className="label">Doğum Tarihi</label>
                    <input className="input" type="date" value={form.dateOfBirth} onChange={e => setForm({ ...form, dateOfBirth: e.target.value })} required />
                  </div>
                </div>
                <div>
                  <label className="label">Adres</label>
                  <input className="input" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} required />
                </div>
                {!editingId && (
                  <div>
                    <label className="label">Şifre</label>
                    <input className="input" type="password" value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} required />
                  </div>
                )}
                <button type="submit" className="btn-primary w-full justify-center">
                  <Save size={16} />
                  {editingId ? 'Güncelle' : 'Oluştur'}
                </button>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
