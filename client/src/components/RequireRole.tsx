import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import type { Session } from '../api'

type Props = {
  session: Session | null
  role: 'Admin' | 'Customer'
  children: ReactNode
}

export function RequireRole({ session, role, children }: Props) {
  if (!session) return <Navigate to="/login" replace />
  if (!session.roles.includes(role)) {
    return <Navigate to={session.roles.includes('Admin') ? '/admin/dashboard' : '/customer/dashboard'} replace />
  }
  return <>{children}</>
}
