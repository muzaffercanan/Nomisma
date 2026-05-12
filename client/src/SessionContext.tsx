import { createContext, useContext } from 'react'
import type { Session } from './api'

type SessionContextType = {
  session: Session
  logout: () => void
}

export const SessionContext = createContext<SessionContextType | null>(null)

export function useSession(): SessionContextType {
  const ctx = useContext(SessionContext)
  if (!ctx) throw new Error('useSession must be used inside SessionContext.Provider')
  return ctx
}
