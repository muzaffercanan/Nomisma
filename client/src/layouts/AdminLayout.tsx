import { Outlet } from 'react-router-dom'
import { LayoutDashboard, Users, Landmark, CreditCard, Settings } from 'lucide-react'
import { Sidebar } from '../components/Sidebar'

const navItems = [
  { to: '/admin/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
  { to: '/admin/customers', label: 'Müşteriler', icon: <Users size={18} /> },
  { to: '/admin/loans', label: 'Krediler', icon: <Landmark size={18} /> },
  { to: '/admin/payments', label: 'Ödemeler', icon: <CreditCard size={18} /> },
  { to: '/admin/settings', label: 'Ayarlar', icon: <Settings size={18} /> },
]

export function AdminLayout() {
  return (
    <div className="flex h-screen bg-gray-50 font-sans">
      <Sidebar navItems={navItems} role="Admin" />
      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>
    </div>
  )
}
