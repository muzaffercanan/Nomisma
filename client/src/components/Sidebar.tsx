import type { ReactNode } from 'react'
import { NavLink } from 'react-router-dom'
import { LogOut, Landmark } from 'lucide-react'
import { useSession } from '../SessionContext'

type NavItem = {
  to: string
  label: string
  icon: ReactNode
}

type Props = {
  navItems: NavItem[]
  role: 'Admin' | 'Customer'
}

export function Sidebar({ navItems, role }: Props) {
  const { session, logout } = useSession()
  const initials = session.email.slice(0, 2).toUpperCase()
  const subtitle = role === 'Admin' ? 'Operasyon Paneli' : 'Müşteri Paneli'

  return (
    <aside className="w-64 flex-shrink-0 bg-brand-navy flex flex-col h-screen sticky top-0">
      {/* Brand */}
      <div className="px-6 py-5 border-b border-white/10">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-brand-teal flex items-center justify-center flex-shrink-0">
            <Landmark size={20} className="text-white" />
          </div>
          <div className="min-w-0">
            <p className="text-white font-bold text-sm leading-tight">Nomisma</p>
            <p className="text-white/50 text-xs leading-tight">{subtitle}</p>
          </div>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        {navItems.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-brand-navyLight text-white'
                  : 'text-white/60 hover:bg-white/10 hover:text-white'
              }`
            }
          >
            <span className="flex-shrink-0">{item.icon}</span>
            {item.label}
          </NavLink>
        ))}
      </nav>

      {/* User footer */}
      <div className="px-4 py-4 border-t border-white/10">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-brand-teal flex items-center justify-center flex-shrink-0">
            <span className="text-white text-xs font-bold">{initials}</span>
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-white text-sm font-medium truncate">{session.email}</p>
            <p className="text-white/40 text-xs">{role}</p>
          </div>
          <button
            type="button"
            onClick={logout}
            title="Çıkış"
            className="flex-shrink-0 p-1.5 rounded-lg text-white/40 hover:text-white hover:bg-white/10 transition-colors"
          >
            <LogOut size={16} />
          </button>
        </div>
      </div>
    </aside>
  )
}
