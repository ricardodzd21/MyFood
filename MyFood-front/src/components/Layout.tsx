import { Link, useLocation } from 'react-router-dom'
import { UtensilsCrossed, LayoutGrid, Plus, Tags, LogOut, BarChart3 } from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'
import type { ReactNode } from 'react'

const nav = [
  { to: '/', label: 'Início', icon: BarChart3 },
  { to: '/catalogo', label: 'Catálogo', icon: LayoutGrid },
  { to: '/novo', label: 'Adicionar', icon: Plus },
  { to: '/categorias', label: 'Categorias', icon: Tags },
]

export default function Layout({ children }: { children: ReactNode }) {
  const { pathname } = useLocation()
  const { user, logout } = useAuth()
  const active = (to: string) => (to === '/' ? pathname === '/' : pathname.startsWith(to))

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-stone-900 text-white sticky top-0 z-20">
        <div className="max-w-6xl mx-auto px-4 h-16 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2 font-bold text-lg">
            <UtensilsCrossed className="text-amber-400" /> MyFood
          </Link>
          <nav className="hidden md:flex items-center gap-1">
            {nav.map((n) => (
              <Link
                key={n.to}
                to={n.to}
                className={`flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm transition-colors ${
                  active(n.to) ? 'bg-white/15 text-white' : 'text-stone-300 hover:bg-white/10'
                }`}
              >
                <n.icon size={16} /> {n.label}
              </Link>
            ))}
            <button onClick={logout} className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm text-stone-300 hover:bg-white/10 ml-2">
              <LogOut size={16} /> Sair
            </button>
          </nav>
          <span className="md:hidden text-sm text-stone-300">{user?.Name}</span>
        </div>
      </header>

      <main className="flex-1 max-w-6xl w-full mx-auto px-4 py-6 pb-24 md:pb-6">{children}</main>

      {/* Bottom nav mobile */}
      <nav className="md:hidden fixed bottom-0 inset-x-0 bg-stone-900 text-white border-t border-white/10 flex z-20">
        {nav.map((n) => (
          <Link key={n.to} to={n.to} className={`flex-1 flex flex-col items-center gap-0.5 py-2.5 text-xs ${active(n.to) ? 'text-amber-400' : 'text-stone-400'}`}>
            <n.icon size={20} /> {n.label}
          </Link>
        ))}
      </nav>
    </div>
  )
}
