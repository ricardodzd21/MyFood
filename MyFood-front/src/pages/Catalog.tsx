import { useEffect, useState } from 'react'
import { Search, Heart } from 'lucide-react'
import api, { type Item, type Category } from '../lib/api'
import ItemCard from '../components/ItemCard'

export default function Catalog() {
  const [items, setItems] = useState<Item[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)

  const [q, setQ] = useState('')
  const [category, setCategory] = useState('')
  const [subcategory, setSubcategory] = useState('')
  const [favorite, setFavorite] = useState(false)
  const [sort, setSort] = useState('newest')

  useEffect(() => {
    api.get<Category[]>('/api/categories').then((r) => setCategories(r.data))
  }, [])

  useEffect(() => {
    const params: Record<string, string> = { sort }
    if (q) params.q = q
    if (category) params.category = category
    if (subcategory) params.subcategory = subcategory
    if (favorite) params.favorite = 'true'
    setLoading(true)
    const t = setTimeout(() => {
      api.get<Item[]>('/api/items', { params }).then((r) => setItems(r.data)).finally(() => setLoading(false))
    }, 250)
    return () => clearTimeout(t)
  }, [q, category, subcategory, favorite, sort])

  const subs = categories.find((c) => c.Id === category)?.Subcategories ?? []

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold">Catálogo</h1>

      {/* Filtros */}
      <div className="bg-white rounded-xl border border-stone-100 p-3 space-y-3">
        <div className="relative">
          <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar por nome ou descrição…"
            className="w-full pl-10 pr-3 py-2 border border-stone-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-amber-400"
          />
        </div>
        <div className="flex flex-wrap gap-2">
          <select value={category} onChange={(e) => { setCategory(e.target.value); setSubcategory('') }} className="border border-stone-300 rounded-lg px-3 py-2 text-sm">
            <option value="">Todas categorias</option>
            {categories.map((c) => <option key={c.Id} value={c.Id}>{c.Icon} {c.Name}</option>)}
          </select>
          <select value={subcategory} onChange={(e) => setSubcategory(e.target.value)} disabled={!category} className="border border-stone-300 rounded-lg px-3 py-2 text-sm disabled:opacity-50">
            <option value="">Todas subcategorias</option>
            {subs.map((s) => <option key={s.Id} value={s.Id}>{s.Name}</option>)}
          </select>
          <select value={sort} onChange={(e) => setSort(e.target.value)} className="border border-stone-300 rounded-lg px-3 py-2 text-sm">
            <option value="newest">Mais recentes</option>
            <option value="oldest">Mais antigos</option>
            <option value="rating">Melhor nota</option>
            <option value="name">Nome (A-Z)</option>
          </select>
          <button
            onClick={() => setFavorite((f) => !f)}
            className={`flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm border ${favorite ? 'bg-red-50 border-red-200 text-red-600' : 'border-stone-300 text-stone-600'}`}
          >
            <Heart size={15} className={favorite ? 'fill-red-500 text-red-500' : ''} /> Favoritos
          </button>
        </div>
      </div>

      {loading ? (
        <p className="text-stone-400">Carregando…</p>
      ) : items.length === 0 ? (
        <div className="bg-white border border-dashed border-stone-300 rounded-xl p-10 text-center text-stone-500">Nenhum item encontrado.</div>
      ) : (
        <>
          <p className="text-sm text-stone-500">{items.length} item(ns)</p>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {items.map((i) => <ItemCard key={i.Id} item={i} />)}
          </div>
        </>
      )}
    </div>
  )
}
