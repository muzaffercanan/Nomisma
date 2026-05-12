import { Outlet } from 'react-router-dom'
import { LayoutDashboard, Landmark, CreditCard, Settings, HelpCircle } from 'lucide-react'
import { Sidebar } from '../components/Sidebar'

const navItems = [
  { to: '/customer/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
  { to: '/customer/loans', label: 'Kredilerim', icon: <Landmark size={18} /> },
  { to: '/customer/payments', label: 'Ödemelerim', icon: <CreditCard size={18} /> },
  { to: '/customer/settings', label: 'Hesap Ayarları', icon: <Settings size={18} /> },
  { to: '/customer/support', label: 'Destek', icon: <HelpCircle size={18} /> },
]

export function CustomerLayout() {
  return (
    <div className="flex h-screen bg-gray-50 font-sans">
      <Sidebar navItems={navItems} role="Customer" />
      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  )
}
