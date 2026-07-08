import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import api, { type UserResponse } from '../lib/api'

interface AuthContextType {
  user: UserResponse | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (name: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType>({} as AuthContextType)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserResponse | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('myfood_token')
    if (!token) {
      setLoading(false)
      return
    }
    api
      .get<UserResponse>('/api/auth/me')
      .then((r) => setUser(r.data))
      .catch(() => localStorage.removeItem('myfood_token'))
      .finally(() => setLoading(false))
  }, [])

  async function login(email: string, password: string) {
    const r = await api.post('/api/auth/login', { Email: email, Password: password })
    localStorage.setItem('myfood_token', r.data.token)
    setUser(r.data.user)
  }

  async function register(name: string, email: string, password: string) {
    const r = await api.post('/api/auth/register', { Name: name, Email: email, Password: password })
    localStorage.setItem('myfood_token', r.data.token)
    setUser(r.data.user)
  }

  function logout() {
    localStorage.removeItem('myfood_token')
    setUser(null)
    location.href = '/login'
  }

  return <AuthContext.Provider value={{ user, loading, login, register, logout }}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  return useContext(AuthContext)
}
