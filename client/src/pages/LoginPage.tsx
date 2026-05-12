import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { Landmark, LogIn } from 'lucide-react'
import { api } from '../api'
import type { Session } from '../api'

type Props = {
  session: Session | null
  onLogin: (session: Session) => void
}

export default function LoginPage({ session, onLogin }: Props) {
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@nomisma.local')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (session?.roles.includes('Admin')) navigate('/admin/dashboard', { replace: true })
    else if (session) navigate('/customer/dashboard', { replace: true })
  }, [navigate, session])

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      const next = await api.login(email, password)
      onLogin(next)
      navigate(next.roles.includes('Admin') ? '/admin/dashboard' : '/customer/dashboard', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giriş başarısız.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-brand-navy via-brand-navyLight to-brand-tealDark flex items-center justify-center p-4">
      {/* Decorative background */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 left-1/4 w-64 h-64 rounded-full bg-white/5 blur-3xl" />
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 rounded-full bg-brand-teal/10 blur-3xl" />
      </div>

      <div className="relative w-full max-w-sm">
        {/* Card */}
        <div className="bg-white rounded-3xl shadow-card-lg p-8">
          {/* Logo */}
          <div className="flex flex-col items-center mb-8">
            <div className="w-14 h-14 rounded-2xl bg-brand-navy flex items-center justify-center mb-4">
              <Landmark size={28} className="text-brand-teal" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900">Nomisma</h1>
            <p className="text-sm text-gray-500 mt-1">Dijital Kredi Bankası</p>
          </div>

          {/* Form */}
          <form onSubmit={submit} className="space-y-4">
            <div>
              <label className="label">E-posta</label>
              <input
                className="input"
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                autoComplete="email"
              />
            </div>
            <div>
              <label className="label">Şifre</label>
              <input
                className="input"
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
                autoComplete="current-password"
              />
            </div>

            {error && <div className="alert-error">{error}</div>}

            <button className="btn-primary w-full justify-center py-2.5" type="submit" disabled={loading}>
              <LogIn size={18} />
              {loading ? 'Giriş yapılıyor…' : 'Giriş Yap'}
            </button>
          </form>

          {/* Demo buttons */}
          <div className="mt-6 pt-6 border-t border-gray-100">
            <p className="text-xs text-center text-gray-400 mb-3">Demo hesapları</p>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => { setEmail('admin@nomisma.local'); setPassword('Admin123!') }}
                className="flex-1 py-2 rounded-lg border border-gray-200 text-xs font-medium text-gray-600 hover:bg-gray-50 transition-colors"
              >
                Admin
              </button>
              <button
                type="button"
                onClick={() => { setEmail('customer@nomisma.local'); setPassword('Customer123!') }}
                className="flex-1 py-2 rounded-lg border border-gray-200 text-xs font-medium text-gray-600 hover:bg-gray-50 transition-colors"
              >
                Müşteri
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
