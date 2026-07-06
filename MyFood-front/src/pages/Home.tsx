import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Star, Heart, Boxes, Sparkles } from 'lucide-react'
import api, { type Stats } from '../lib/api'
import ItemCard from '../components/ItemCard'

export default function Home() {
  const [stats, setStats] = useState<Stats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get<Stats>('/api/stats').then((r) => setStats(r.data)).finally(() => setLoading(false))
  }, [])

  if (loading) return <p className="text-stone-400">Carregando…</p>

  const cards = [
    { label: 'Itens', value: stats?.TotalItems ?? 0, icon: Boxes, color: 'text-stone-700' },
    { label: 'Favoritos', value: stats?.TotalFavorites ?? 0, icon: Heart, color: 'text-red-500' },
    { label: 'Categorias', value: stats?.TotalCategories ?? 0, icon: Sparkles, color: 'text-amber-500' },
    { label: 'Nota média', value: stats?.AverageRating ?? 0, icon: Star, color: 'text-amber-400' },
  ]

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Meu catálogo</h1>
          <p className="text-stone-500 text-sm">Tudo que você já provou, num lugar só.</p>
        </div>
        <Link to="/novo" className="hidden md:flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-white font-semibold rounded-lg px-4 py-2.5">
          <Plus size={18} /> Adicionar
        </Link>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {cards.map((c) => (
          <div key={c.label} className="bg-white rounded-xl border border-stone-100 p-4">
            <c.icon className={c.color} size={22} />
            <div className="text-2xl font-bold mt-2">{c.value}</div>
            <div className="text-stone-500 text-sm">{c.label}</div>
          </div>
        ))}
      </div>

      {stats && stats.ByCategory.length > 0 && (
        <div>
          <h2 className="font-semibold mb-3">Por categoria</h2>
          <div className="flex flex-wrap gap-2">
            {stats.ByCategory.map((c) => (
              <span key={c.Name} className="bg-white border border-stone-200 rounded-full px-3 py-1.5 text-sm">
                {c.Icon} {c.Name} <b className="text-stone-500">{c.Count}</b>
              </span>
            ))}
          </div>
        </div>
      )}

      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-semibold">Adicionados recentemente</h2>
          <Link to="/catalogo" className="text-amber-600 text-sm hover:underline">Ver tudo</Link>
        </div>
        {stats && stats.RecentItems.length > 0 ? (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {stats.RecentItems.map((i) => <ItemCard key={i.Id} item={i} />)}
          </div>
        ) : (
          <div className="bg-white border border-dashed border-stone-300 rounded-xl p-10 text-center text-stone-500">
            Nenhum item ainda. <Link to="/novo" className="text-amber-600 font-medium">Adicione o primeiro →</Link>
          </div>
        )}
      </div>
    </div>
  )
}
