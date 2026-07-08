import { useEffect, useState } from 'react'
import { Plus, Pencil, Trash2, X, ChevronDown, ChevronRight } from 'lucide-react'
import api, { type Category } from '../lib/api'

export default function Categories() {
  const [categories, setCategories] = useState<Category[]>([])
  const [expanded, setExpanded] = useState<string | null>(null)
  const [editing, setEditing] = useState<Category | null>(null)
  const [creating, setCreating] = useState(false)

  function load() {
    api.get<Category[]>('/api/categories').then((r) => setCategories(r.data))
  }
  useEffect(load, [])

  async function delCat(c: Category) {
    if (!confirm(`Excluir categoria "${c.Name}"?`)) return
    try {
      await api.delete(`/api/categories/${c.Id}`)
      load()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Erro ao excluir')
    }
  }

  return (
    <div className="max-w-2xl mx-auto space-y-5">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Categorias</h1>
        <button onClick={() => setCreating(true)} className="flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-white rounded-lg px-4 py-2 text-sm font-semibold">
          <Plus size={16} /> Nova categoria
        </button>
      </div>

      <div className="space-y-2">
        {categories.map((c) => (
          <div key={c.Id} className="bg-white border border-stone-100 rounded-xl">
            <div className="flex items-center gap-2 p-3">
              <button onClick={() => setExpanded(expanded === c.Id ? null : c.Id)} className="text-stone-400">
                {expanded === c.Id ? <ChevronDown size={18} /> : <ChevronRight size={18} />}
              </button>
              <span className="text-xl">{c.Icon}</span>
              <div className="flex-1">
                <div className="font-semibold">{c.Name}</div>
                <div className="text-xs text-stone-400">{c.ItemCount} itens · {c.Subcategories.length} subcategorias</div>
              </div>
              <button onClick={() => setEditing(c)} className="text-stone-400 hover:text-stone-700 p-1.5"><Pencil size={16} /></button>
              <button onClick={() => delCat(c)} className="text-stone-400 hover:text-red-500 p-1.5"><Trash2 size={16} /></button>
            </div>
            {expanded === c.Id && <SubcategoryPanel category={c} onChange={load} />}
          </div>
        ))}
      </div>

      {(creating || editing) && (
        <CategoryModal category={editing} onClose={() => { setCreating(false); setEditing(null) }} onSaved={() => { setCreating(false); setEditing(null); load() }} />
      )}
    </div>
  )
}

function SubcategoryPanel({ category, onChange }: { category: Category; onChange: () => void }) {
  const [name, setName] = useState('')

  async function add() {
    if (!name.trim()) return
    await api.post('/api/subcategories', { CategoryId: category.Id, Name: name.trim(), Order: category.Subcategories.length })
    setName('')
    onChange()
  }
  async function del(id: string) {
    await api.delete(`/api/subcategories/${id}`)
    onChange()
  }

  return (
    <div className="border-t border-stone-100 p-3 pl-10 space-y-2">
      {category.Subcategories.map((s) => (
        <div key={s.Id} className="flex items-center justify-between text-sm">
          <span>{s.Name} <span className="text-stone-400">({s.ItemCount})</span></span>
          <button onClick={() => del(s.Id)} className="text-stone-400 hover:text-red-500"><X size={14} /></button>
        </div>
      ))}
      <div className="flex gap-2 pt-1">
        <input value={name} onChange={(e) => setName(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && add()} placeholder="Nova subcategoria" className="flex-1 border border-stone-300 rounded-lg px-3 py-1.5 text-sm" />
        <button onClick={add} className="bg-stone-900 text-white rounded-lg px-3 text-sm">Add</button>
      </div>
    </div>
  )
}

function CategoryModal({ category, onClose, onSaved }: { category: Category | null; onClose: () => void; onSaved: () => void }) {
  const [name, setName] = useState(category?.Name ?? '')
  const [icon, setIcon] = useState(category?.Icon ?? '')
  const [color, setColor] = useState(category?.Color ?? '#7f1d1d')
  const [attrs, setAttrs] = useState<string[]>(category?.SuggestedAttributes ?? [])
  const [hasVenue, setHasVenue] = useState(category?.HasVenueRating ?? false)
  const [saving, setSaving] = useState(false)

  async function save() {
    if (!name.trim()) return
    setSaving(true)
    const payload = { Name: name.trim(), Icon: icon, Color: color, Order: category?.Order ?? 99, HasVenueRating: hasVenue, SuggestedAttributes: attrs.filter((a) => a.trim()) }
    try {
      if (category) await api.put(`/api/categories/${category.Id}`, payload)
      else await api.post('/api/categories', payload)
      onSaved()
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-30 px-4" onClick={onClose}>
      <div className="bg-white rounded-2xl w-full max-w-md p-6" onClick={(e) => e.stopPropagation()}>
        <h2 className="text-lg font-bold mb-4">{category ? 'Editar' : 'Nova'} categoria</h2>

        <div className="grid grid-cols-3 gap-3 mb-3">
          <div className="col-span-2">
            <label className="block text-sm font-medium mb-1">Nome</label>
            <input value={name} onChange={(e) => setName(e.target.value)} className="w-full border border-stone-300 rounded-lg px-3 py-2" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Ícone</label>
            <input value={icon} onChange={(e) => setIcon(e.target.value)} placeholder="🍷" className="w-full border border-stone-300 rounded-lg px-3 py-2 text-center" />
          </div>
        </div>

        <label className="flex items-start gap-2 mb-4 cursor-pointer select-none">
          <input type="checkbox" checked={hasVenue} onChange={(e) => setHasVenue(e.target.checked)} className="w-4 h-4 mt-0.5 accent-amber-500" />
          <span className="text-sm">
            Avaliação de local
            <span className="block text-xs text-stone-400">Mostra notas de limpeza, atendimento e ambiente (para comidas/estabelecimentos). Deixe desmarcado para bebidas.</span>
          </span>
        </label>

        <div className="mb-4">
          <label className="block text-sm font-medium mb-1">Atributos sugeridos</label>
          <p className="text-xs text-stone-400 mb-2">Aparecem automaticamente ao criar um item desta categoria.</p>
          <div className="space-y-2">
            {attrs.map((a, idx) => (
              <div key={idx} className="flex gap-2">
                <input value={a} onChange={(e) => setAttrs((x) => x.map((v, i) => (i === idx ? e.target.value : v)))} className="flex-1 border border-stone-300 rounded-lg px-3 py-1.5 text-sm" />
                <button onClick={() => setAttrs((x) => x.filter((_, i) => i !== idx))} className="text-stone-400 hover:text-red-500 px-2"><X size={16} /></button>
              </div>
            ))}
            <button onClick={() => setAttrs((x) => [...x, ''])} className="flex items-center gap-1 text-amber-600 text-sm"><Plus size={14} /> Adicionar atributo</button>
          </div>
        </div>

        <div className="flex gap-2 justify-end">
          <button onClick={onClose} className="border border-stone-300 rounded-lg px-4 py-2 text-sm">Cancelar</button>
          <button onClick={save} disabled={saving} className="bg-amber-500 hover:bg-amber-600 text-white rounded-lg px-4 py-2 text-sm font-semibold disabled:opacity-60">
            {saving ? 'Salvando…' : 'Salvar'}
          </button>
        </div>
      </div>
    </div>
  )
}
