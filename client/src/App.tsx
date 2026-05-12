import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import {
  Building2,
  CreditCard,
  FileText,
  Landmark,
  LogOut,
  Plus,
  RefreshCw,
  Save,
  Trash2,
  UserPlus,
} from 'lucide-react'
import { Navigate, Route, Routes, useNavigate } from 'react-router-dom'
import {
  api,
  formatDate,
  formatMoney,
  installmentStatusLabel,
  loanTypeLabel,
} from './api'
import type { Customer, CustomerSummary, Installment, Loan, Payment, Session } from './api'
import './App.css'

const sessionKey = 'nomisma.session'

type AppShellProps = {
  session: Session
  onLogout: () => void
  children: ReactNode
}

type CustomerFormState = {
  firstName: string
  lastName: string
  nationalId: string
  email: string
  phoneNumber: string
  address: string
  dateOfBirth: string
  password: string
}

type LoanFormState = {
  customerId: string
  type: 'Personal' | 'Education' | 'Vehicle'
  principalAmount: string
  profitRate: string
  termMonths: string
  startDate: string
}

const emptyCustomerForm: CustomerFormState = {
  firstName: '',
  lastName: '',
  nationalId: '',
  email: '',
  phoneNumber: '',
  address: '',
  dateOfBirth: '1990-01-01',
  password: 'Customer123!',
}

const emptyLoanForm: LoanFormState = {
  customerId: '',
  type: 'Personal',
  principalAmount: '25000',
  profitRate: '18',
  termMonths: '12',
  startDate: new Date().toISOString().slice(0, 10),
}

function App() {
  const [session, setSession] = useState<Session | null>(() => {
    const raw = localStorage.getItem(sessionKey)
    return raw ? (JSON.parse(raw) as Session) : null
  })

  const handleSession = (nextSession: Session) => {
    localStorage.setItem(sessionKey, JSON.stringify(nextSession))
    setSession(nextSession)
  }

  const logout = () => {
    localStorage.removeItem(sessionKey)
    setSession(null)
  }

  return (
    <Routes>
      <Route path="/login" element={<LoginPage session={session} onLogin={handleSession} />} />
      <Route
        path="/admin"
        element={
          <RequireRole session={session} role="Admin">
            <AppShell session={session!} onLogout={logout}>
              <AdminDashboard session={session!} />
            </AppShell>
          </RequireRole>
        }
      />
      <Route
        path="/customer"
        element={
          <RequireRole session={session} role="Customer">
            <AppShell session={session!} onLogout={logout}>
              <CustomerDashboard session={session!} />
            </AppShell>
          </RequireRole>
        }
      />
      <Route
        path="*"
        element={
          <Navigate to={session?.roles.includes('Admin') ? '/admin' : session ? '/customer' : '/login'} replace />
        }
      />
    </Routes>
  )
}

function RequireRole({ session, role, children }: { session: Session | null; role: 'Admin' | 'Customer'; children: ReactNode }) {
  if (!session) return <Navigate to="/login" replace />
  if (!session.roles.includes(role)) {
    return <Navigate to={session.roles.includes('Admin') ? '/admin' : '/customer'} replace />
  }
  return <>{children}</>
}

function LoginPage({ session, onLogin }: { session: Session | null; onLogin: (session: Session) => void }) {
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@nomisma.local')
  const [password, setPassword] = useState('Admin123!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (session?.roles.includes('Admin')) navigate('/admin', { replace: true })
    else if (session) navigate('/customer', { replace: true })
  }, [navigate, session])

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setLoading(true)
    setError('')
    try {
      const nextSession = await api.login(email, password)
      onLogin(nextSession)
      navigate(nextSession.roles.includes('Admin') ? '/admin' : '/customer', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giris basarisiz.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="login-screen">
      <section className="login-panel">
        <div className="brand-row">
          <span className="brand-mark">
            <Landmark size={24} aria-hidden="true" />
          </span>
          <div>
            <h1>Nomisma</h1>
            <p>Dijital kredi operasyonlari</p>
          </div>
        </div>

        <form className="form-grid" onSubmit={submit}>
          <label>
            Email
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" />
          </label>
          <label>
            Sifre
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" />
          </label>
          {error && <div className="alert error">{error}</div>}
          <button className="primary-action" type="submit" disabled={loading}>
            <Building2 size={18} aria-hidden="true" />
            {loading ? 'Giris yapiliyor' : 'Giris yap'}
          </button>
        </form>

        <div className="demo-logins">
          <button type="button" onClick={() => { setEmail('admin@nomisma.local'); setPassword('Admin123!') }}>
            Admin
          </button>
          <button type="button" onClick={() => { setEmail('customer@nomisma.local'); setPassword('Customer123!') }}>
            Musteri
          </button>
        </div>
      </section>
    </main>
  )
}

function AppShell({ session, onLogout, children }: AppShellProps) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-row compact">
          <span className="brand-mark">
            <Landmark size={22} aria-hidden="true" />
          </span>
          <div>
            <strong>Nomisma</strong>
            <span>{session.roles.join(', ')}</span>
          </div>
        </div>
        <div className="sidebar-footer">
          <span>{session.email}</span>
          <button className="icon-button" type="button" onClick={onLogout} title="Cikis">
            <LogOut size={18} aria-hidden="true" />
          </button>
        </div>
      </aside>
      {children}
    </div>
  )
}

function AdminDashboard({ session }: { session: Session }) {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [loans, setLoans] = useState<Loan[]>([])
  const [payments, setPayments] = useState<Payment[]>([])
  const [selectedCustomerId, setSelectedCustomerId] = useState('')
  const [selectedLoanId, setSelectedLoanId] = useState('')
  const [summary, setSummary] = useState<CustomerSummary | null>(null)
  const [customerForm, setCustomerForm] = useState<CustomerFormState>(emptyCustomerForm)
  const [editingCustomerId, setEditingCustomerId] = useState<string | null>(null)
  const [loanForm, setLoanForm] = useState<LoanFormState>(emptyLoanForm)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')

  const selectedLoan = useMemo(() => loans.find((loan) => loan.id === selectedLoanId) ?? null, [loans, selectedLoanId])
  const selectedCustomer = useMemo(
    () => customers.find((customer) => customer.id === selectedCustomerId) ?? null,
    [customers, selectedCustomerId],
  )

  const loadAll = useCallback(async () => {
    const [nextCustomers, nextLoans, nextPayments] = await Promise.all([
      api.customers(session.token),
      api.loans(session.token),
      api.payments(session.token),
    ])
    setCustomers(nextCustomers)
    setLoans(nextLoans)
    setPayments(nextPayments)

    if (!selectedCustomerId && nextCustomers[0]) {
      setSelectedCustomerId(nextCustomers[0].id)
      setLoanForm((current) => ({ ...current, customerId: nextCustomers[0].id }))
    }
  }, [selectedCustomerId, session.token])

  useEffect(() => {
    loadAll().catch((err) => setError(err instanceof Error ? err.message : 'Veri alinamadi.'))
  }, [loadAll])

  useEffect(() => {
    if (!selectedCustomerId) {
      setSummary(null)
      return
    }

    api.customerSummary(session.token, selectedCustomerId)
      .then(setSummary)
      .catch((err) => setError(err instanceof Error ? err.message : 'Ozet alinamadi.'))
  }, [selectedCustomerId, session.token, loans, payments])

  const submitCustomer = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    try {
      if (editingCustomerId) {
        await api.updateCustomer(session.token, editingCustomerId, customerForm)
        setNotice('Musteri guncellendi.')
      } else {
        const created = await api.createCustomer(session.token, customerForm)
        setSelectedCustomerId(created.id)
        setLoanForm((current) => ({ ...current, customerId: created.id }))
        setNotice('Musteri olusturuldu.')
      }
      setCustomerForm(emptyCustomerForm)
      setEditingCustomerId(null)
      await loadAll()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Musteri kaydedilemedi.')
    }
  }

  const editCustomer = (customer: Customer) => {
    setEditingCustomerId(customer.id)
    setCustomerForm({
      firstName: customer.firstName,
      lastName: customer.lastName,
      nationalId: customer.nationalId,
      email: customer.email,
      phoneNumber: customer.phoneNumber,
      address: customer.address,
      dateOfBirth: customer.dateOfBirth,
      password: '',
    })
  }

  const deleteCustomer = async (id: string) => {
    setError('')
    setNotice('')
    try {
      await api.deleteCustomer(session.token, id)
      setNotice('Musteri silindi.')
      await loadAll()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Musteri silinemedi.')
    }
  }

  const submitLoan = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setNotice('')
    try {
      const created = await api.createLoan(session.token, {
        customerId: loanForm.customerId,
        type: loanForm.type,
        principalAmount: Number(loanForm.principalAmount),
        profitRate: Number(loanForm.profitRate),
        termMonths: Number(loanForm.termMonths),
        startDate: loanForm.startDate,
      })
      setSelectedLoanId(created.id)
      setNotice('Kredi olusturuldu.')
      await loadAll()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kredi olusturulamadi.')
    }
  }

  const closeLoan = async (loanId: string) => {
    setError('')
    setNotice('')
    try {
      await api.closeLoan(session.token, loanId)
      setNotice('Kredi kapatildi.')
      await loadAll()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kredi kapatilamadi.')
    }
  }

  return (
    <main className="workspace">
      <header className="topbar">
        <div>
          <h1>Operasyon Paneli</h1>
          <p>Kredi, taksit ve odeme kayitlari</p>
        </div>
        <button className="secondary-action" type="button" onClick={() => loadAll()}>
          <RefreshCw size={17} aria-hidden="true" />
          Yenile
        </button>
      </header>

      {(error || notice) && <div className={`alert ${error ? 'error' : 'success'}`}>{error || notice}</div>}

      <section className="metrics-grid">
        <Metric title="Musteri" value={customers.length.toString()} />
        <Metric title="Aktif kredi" value={loans.filter((loan) => loan.status === 'Active').length.toString()} />
        <Metric title="Kalan borc" value={formatMoney(loans.reduce((sum, loan) => sum + loan.remainingDebt, 0))} />
        <Metric title="Odeme" value={payments.length.toString()} />
      </section>

      <section className="two-column">
        <div className="panel">
          <PanelTitle icon={<UserPlus size={18} />} title={editingCustomerId ? 'Musteri Guncelle' : 'Musteri Olustur'} />
          <form className="form-grid dense" onSubmit={submitCustomer}>
            <div className="inline-fields">
              <label>Ad<input value={customerForm.firstName} onChange={(event) => setCustomerForm({ ...customerForm, firstName: event.target.value })} /></label>
              <label>Soyad<input value={customerForm.lastName} onChange={(event) => setCustomerForm({ ...customerForm, lastName: event.target.value })} /></label>
            </div>
            <label>Kimlik No<input value={customerForm.nationalId} disabled={!!editingCustomerId} onChange={(event) => setCustomerForm({ ...customerForm, nationalId: event.target.value })} /></label>
            <label>Email<input value={customerForm.email} type="email" onChange={(event) => setCustomerForm({ ...customerForm, email: event.target.value })} /></label>
            <div className="inline-fields">
              <label>Telefon<input value={customerForm.phoneNumber} onChange={(event) => setCustomerForm({ ...customerForm, phoneNumber: event.target.value })} /></label>
              <label>Dogum<input value={customerForm.dateOfBirth} type="date" onChange={(event) => setCustomerForm({ ...customerForm, dateOfBirth: event.target.value })} /></label>
            </div>
            <label>Adres<input value={customerForm.address} onChange={(event) => setCustomerForm({ ...customerForm, address: event.target.value })} /></label>
            {!editingCustomerId && <label>Sifre<input value={customerForm.password} type="password" onChange={(event) => setCustomerForm({ ...customerForm, password: event.target.value })} /></label>}
            <button className="primary-action" type="submit">
              <Save size={17} aria-hidden="true" />
              Kaydet
            </button>
          </form>
        </div>

        <div className="panel">
          <PanelTitle icon={<Landmark size={18} />} title="Kredi Olustur" />
          <form className="form-grid dense" onSubmit={submitLoan}>
            <label>
              Musteri
              <select value={loanForm.customerId} onChange={(event) => setLoanForm({ ...loanForm, customerId: event.target.value })}>
                <option value="">Seciniz</option>
                {customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>{customer.customerNumber} - {customer.fullName}</option>
                ))}
              </select>
            </label>
            <div className="inline-fields">
              <label>
                Tur
                <select value={loanForm.type} onChange={(event) => setLoanForm({ ...loanForm, type: event.target.value as LoanFormState['type'] })}>
                  <option value="Personal">Ihtiyac</option>
                  <option value="Education">Egitim</option>
                  <option value="Vehicle">Tasit</option>
                </select>
              </label>
              <label>Vade<input value={loanForm.termMonths} type="number" min="1" onChange={(event) => setLoanForm({ ...loanForm, termMonths: event.target.value })} /></label>
            </div>
            <div className="inline-fields">
              <label>Ana para<input value={loanForm.principalAmount} type="number" min="1" onChange={(event) => setLoanForm({ ...loanForm, principalAmount: event.target.value })} /></label>
              <label>Kar %<input value={loanForm.profitRate} type="number" min="0" step="0.01" onChange={(event) => setLoanForm({ ...loanForm, profitRate: event.target.value })} /></label>
            </div>
            <label>Baslangic<input value={loanForm.startDate} type="date" onChange={(event) => setLoanForm({ ...loanForm, startDate: event.target.value })} /></label>
            <button className="primary-action" type="submit">
              <Plus size={17} aria-hidden="true" />
              Kredi Ac
            </button>
          </form>
        </div>
      </section>

      <section className="data-grid">
        <div className="panel">
          <PanelTitle icon={<Building2 size={18} />} title="Musteriler" />
          <div className="list">
            {customers.map((customer) => (
              <button
                type="button"
                className={`list-row ${selectedCustomerId === customer.id ? 'selected' : ''}`}
                key={customer.id}
                onClick={() => {
                  setSelectedCustomerId(customer.id)
                  setLoanForm((current) => ({ ...current, customerId: customer.id }))
                }}
              >
                <span>
                  <strong>{customer.fullName}</strong>
                  <small>{customer.customerNumber} · {customer.email}</small>
                </span>
                <span className="row-actions">
                  <button type="button" onClick={(event) => { event.stopPropagation(); editCustomer(customer) }}>Duzenle</button>
                  <button type="button" className="danger" onClick={(event) => { event.stopPropagation(); deleteCustomer(customer.id) }} title="Sil">
                    <Trash2 size={15} aria-hidden="true" />
                  </button>
                </span>
              </button>
            ))}
          </div>
        </div>

        <div className="panel">
          <PanelTitle icon={<FileText size={18} />} title="Krediler" />
          <div className="list">
            {loans.filter((loan) => !selectedCustomerId || loan.customerId === selectedCustomerId).map((loan) => (
              <button
                type="button"
                className={`list-row ${selectedLoanId === loan.id ? 'selected' : ''}`}
                key={loan.id}
                onClick={() => setSelectedLoanId(loan.id)}
              >
                <span>
                  <strong>{loanTypeLabel(loan.type)} · {formatMoney(loan.totalDebt)}</strong>
                  <small>Skor {loan.creditScore} · Kalan {formatMoney(loan.remainingDebt)}</small>
                </span>
                <StatusBadge status={loan.status} />
              </button>
            ))}
          </div>
        </div>
      </section>

      <section className="two-column">
        <div className="panel">
          <PanelTitle icon={<CreditCard size={18} />} title="Musteri Ozeti" />
          {selectedCustomer && summary ? (
            <div className="summary-block">
              <h2>{selectedCustomer.fullName}</h2>
              <div className="metrics-grid compact-metrics">
                <Metric title="Toplam borc" value={formatMoney(summary.totalLoanDebt)} />
                <Metric title="Kalan ana para" value={formatMoney(summary.remainingPrincipal)} />
                <Metric title="Gecikmis" value={summary.overdueInstallmentCount.toString()} />
              </div>
              <p className="muted">{summary.unpaidInstallments.length} odenmemis, {summary.paidInstallments.length} odenmis taksit</p>
            </div>
          ) : (
            <p className="muted">Secili musteri yok.</p>
          )}
        </div>

        <div className="panel">
          <PanelTitle icon={<FileText size={18} />} title="Taksit Plani" />
          {selectedLoan ? (
            <>
              <div className="loan-toolbar">
                <span>{loanTypeLabel(selectedLoan.type)} · {formatMoney(selectedLoan.totalDebt)}</span>
                <button className="secondary-action" type="button" onClick={() => closeLoan(selectedLoan.id)}>Kapat</button>
              </div>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>No</th>
                      <th>Tutar</th>
                      <th>Son odeme</th>
                      <th>Durum</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedLoan.installments.map((installment) => (
                      <tr key={installment.id}>
                        <td>{installment.installmentNumber}</td>
                        <td>{formatMoney(installment.amount)}</td>
                        <td>{formatDate(installment.dueDate)}</td>
                        <td><StatusBadge status={installment.status} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <p className="muted">Secili kredi yok.</p>
          )}
        </div>
      </section>
    </main>
  )
}

type PaymentFormState = {
  cardHolderName: string
  cardNumber: string
  cvv: string
  expiryMonth: string
  expiryYear: string
}

const defaultPaymentForm: PaymentFormState = {
  cardHolderName: 'Demo Customer',
  cardNumber: '4111111111111111',
  cvv: '123',
  expiryMonth: '12',
  expiryYear: '2030',
}

function CustomerDashboard({ session }: { session: Session }) {
  const [summary, setSummary] = useState<CustomerSummary | null>(null)
  const [loans, setLoans] = useState<Loan[]>([])
  const [selectedLoanId, setSelectedLoanId] = useState('')
  const [paymentTarget, setPaymentTarget] = useState<Installment | null>(null)
  const [paymentForm, setPaymentForm] = useState<PaymentFormState>(defaultPaymentForm)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')

  const selectedLoan = useMemo(() => loans.find((loan) => loan.id === selectedLoanId) ?? loans[0] ?? null, [loans, selectedLoanId])

  const loadCustomerData = useCallback(async () => {
    const [nextSummary, nextLoans] = await Promise.all([
      api.mySummary(session.token),
      api.loans(session.token),
    ])
    setSummary(nextSummary)
    setLoans(nextLoans)
    if (!selectedLoanId && nextLoans[0]) setSelectedLoanId(nextLoans[0].id)
  }, [selectedLoanId, session.token])

  useEffect(() => {
    loadCustomerData().catch((err) => setError(err instanceof Error ? err.message : 'Veri alinamadi.'))
  }, [loadCustomerData])

  const submitPayment = async (event: FormEvent) => {
    event.preventDefault()
    if (!paymentTarget) return
    setError('')
    setNotice('')
    try {
      await api.createPayment(session.token, {
        installmentId: paymentTarget.id,
        amount: paymentTarget.amount,
        cardHolderName: paymentForm.cardHolderName,
        cardNumber: paymentForm.cardNumber,
        cvv: paymentForm.cvv,
        expiryMonth: Number(paymentForm.expiryMonth),
        expiryYear: Number(paymentForm.expiryYear),
      })
      setNotice('Odeme alindi.')
      setPaymentTarget(null)
      await loadCustomerData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Odeme alinamadi.')
    }
  }

  return (
    <main className="workspace">
      <header className="topbar">
        <div>
          <h1>Musteri Paneli</h1>
          <p>{summary?.fullName ?? session.email}</p>
        </div>
        <button className="secondary-action" type="button" onClick={() => loadCustomerData()}>
          <RefreshCw size={17} aria-hidden="true" />
          Yenile
        </button>
      </header>

      {(error || notice) && <div className={`alert ${error ? 'error' : 'success'}`}>{error || notice}</div>}

      <section className="metrics-grid">
        <Metric title="Toplam borc" value={formatMoney(summary?.totalLoanDebt ?? 0)} />
        <Metric title="Kalan ana para" value={formatMoney(summary?.remainingPrincipal ?? 0)} />
        <Metric title="Kalan borc" value={formatMoney(summary?.remainingDebt ?? 0)} />
        <Metric title="Gecikmis" value={(summary?.overdueInstallmentCount ?? 0).toString()} />
      </section>

      <section className="data-grid">
        <div className="panel">
          <PanelTitle icon={<Landmark size={18} />} title="Kredilerim" />
          <div className="list">
            {loans.map((loan) => (
              <button
                type="button"
                className={`list-row ${selectedLoan?.id === loan.id ? 'selected' : ''}`}
                key={loan.id}
                onClick={() => setSelectedLoanId(loan.id)}
              >
                <span>
                  <strong>{loanTypeLabel(loan.type)} · {formatMoney(loan.totalDebt)}</strong>
                  <small>Kalan {formatMoney(loan.remainingDebt)} · Skor {loan.creditScore}</small>
                </span>
                <StatusBadge status={loan.status} />
              </button>
            ))}
          </div>
        </div>

        <div className="panel">
          <PanelTitle icon={<CreditCard size={18} />} title="Odeme" />
          {paymentTarget ? (
            <form className="form-grid dense" onSubmit={submitPayment}>
              <div className="payment-target">
                <strong>{formatMoney(paymentTarget.amount)}</strong>
                <span>{paymentTarget.installmentNumber}. taksit · {formatDate(paymentTarget.dueDate)}</span>
              </div>
              <label>Kart sahibi<input value={paymentForm.cardHolderName} onChange={(event) => setPaymentForm({ ...paymentForm, cardHolderName: event.target.value })} /></label>
              <label>Kart no<input value={paymentForm.cardNumber} onChange={(event) => setPaymentForm({ ...paymentForm, cardNumber: event.target.value })} /></label>
              <div className="inline-fields">
                <label>CVV<input value={paymentForm.cvv} onChange={(event) => setPaymentForm({ ...paymentForm, cvv: event.target.value })} /></label>
                <label>Ay<input value={paymentForm.expiryMonth} type="number" min="1" max="12" onChange={(event) => setPaymentForm({ ...paymentForm, expiryMonth: event.target.value })} /></label>
              </div>
              <label>Yil<input value={paymentForm.expiryYear} type="number" min="2026" onChange={(event) => setPaymentForm({ ...paymentForm, expiryYear: event.target.value })} /></label>
              <div className="button-row">
                <button className="primary-action" type="submit">
                  <CreditCard size={17} aria-hidden="true" />
                  Ode
                </button>
                <button className="secondary-action" type="button" onClick={() => setPaymentTarget(null)}>Vazgec</button>
              </div>
            </form>
          ) : (
            <p className="muted">{summary?.unpaidInstallments.length ?? 0} odenmemis taksit</p>
          )}
        </div>
      </section>

      <section className="panel">
        <PanelTitle icon={<FileText size={18} />} title="Taksitler" />
        {selectedLoan ? (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>No</th>
                  <th>Tutar</th>
                  <th>Ana para</th>
                  <th>Son odeme</th>
                  <th>Durum</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {selectedLoan.installments.map((installment) => (
                  <tr key={installment.id}>
                    <td>{installment.installmentNumber}</td>
                    <td>{formatMoney(installment.amount)}</td>
                    <td>{formatMoney(installment.principalAmount)}</td>
                    <td>{formatDate(installment.dueDate)}</td>
                    <td><StatusBadge status={installment.status} /></td>
                    <td>
                      {installment.status !== 'Paid' && (
                        <button className="secondary-action slim" type="button" onClick={() => setPaymentTarget(installment)}>
                          <CreditCard size={15} aria-hidden="true" />
                          Ode
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="muted">Kredi kaydi yok.</p>
        )}
      </section>
    </main>
  )
}

function Metric({ title, value }: { title: string; value: string }) {
  return (
    <div className="metric">
      <span>{title}</span>
      <strong>{value}</strong>
    </div>
  )
}

function PanelTitle({ icon, title }: { icon: ReactNode; title: string }) {
  return (
    <div className="panel-title">
      <span>{icon}</span>
      <h2>{title}</h2>
    </div>
  )
}

function StatusBadge({ status }: { status: string }) {
  return <span className={`status ${status.toLowerCase()}`}>{status in statusLabels ? statusLabels[status] : installmentStatusLabel(status as never)}</span>
}

const statusLabels: Record<string, string> = {
  Active: 'Aktif',
  Closed: 'Kapatildi',
  Paid: 'Odendi',
  Unpaid: 'Odenmedi',
  Overdue: 'Gecikmis',
}

export default App
