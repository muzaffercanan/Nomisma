import { useState } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import type { Session } from './api'
import { SessionContext } from './SessionContext'
import { RequireRole } from './components/RequireRole'
import { CustomerLayout } from './layouts/CustomerLayout'
import { AdminLayout } from './layouts/AdminLayout'
import LoginPage from './pages/LoginPage'
import CustomerDashboardPage from './pages/customer/DashboardPage'
import CustomerLoansPage from './pages/customer/LoansPage'
import CustomerPaymentsPage from './pages/customer/PaymentsPage'
import CustomerSettingsPage from './pages/customer/SettingsPage'
import CustomerSupportPage from './pages/customer/SupportPage'
import AdminDashboardPage from './pages/admin/DashboardPage'
import AdminCustomersPage from './pages/admin/CustomersPage'
import AdminLoansPage from './pages/admin/LoansPage'
import AdminPaymentsPage from './pages/admin/PaymentsPage'
import AdminSettingsPage from './pages/admin/SettingsPage'

const SESSION_KEY = 'nomisma.session'

export default function App() {
  const [session, setSession] = useState<Session | null>(() => {
    const raw = localStorage.getItem(SESSION_KEY)
    return raw ? (JSON.parse(raw) as Session) : null
  })

  const handleSession = (next: Session) => {
    localStorage.setItem(SESSION_KEY, JSON.stringify(next))
    setSession(next)
  }

  const logout = () => {
    localStorage.removeItem(SESSION_KEY)
    setSession(null)
  }

  return (
    <Routes>
      <Route path="/login" element={<LoginPage session={session} onLogin={handleSession} />} />

      <Route
        path="/customer"
        element={
          <RequireRole session={session} role="Customer">
            <SessionContext.Provider value={{ session: session!, logout }}>
              <CustomerLayout />
            </SessionContext.Provider>
          </RequireRole>
        }
      >
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<CustomerDashboardPage />} />
        <Route path="loans" element={<CustomerLoansPage />} />
        <Route path="payments" element={<CustomerPaymentsPage />} />
        <Route path="settings" element={<CustomerSettingsPage />} />
        <Route path="support" element={<CustomerSupportPage />} />
      </Route>

      <Route
        path="/admin"
        element={
          <RequireRole session={session} role="Admin">
            <SessionContext.Provider value={{ session: session!, logout }}>
              <AdminLayout />
            </SessionContext.Provider>
          </RequireRole>
        }
      >
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<AdminDashboardPage />} />
        <Route path="customers" element={<AdminCustomersPage />} />
        <Route path="loans" element={<AdminLoansPage />} />
        <Route path="payments" element={<AdminPaymentsPage />} />
        <Route path="settings" element={<AdminSettingsPage />} />
      </Route>

      <Route
        path="*"
        element={
          <Navigate
            to={
              session?.roles.includes('Admin')
                ? '/admin/dashboard'
                : session
                ? '/customer/dashboard'
                : '/login'
            }
            replace
          />
        }
      />
    </Routes>
  )
}
