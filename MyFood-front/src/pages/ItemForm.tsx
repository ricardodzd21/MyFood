import { useEffect, useRef, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Plus, X, Star as StarIcon, Sparkles, ImagePlus, Loader2 } from 'lucide-react'
import api, { type Category, type Item, type Attribute, type AiResult } from '../lib/api'
import StarRating from '../components/StarRating'

export default function ItemForm() {
  const { id } = useParams()
  const editing = !!id
  const navigate = useNavigate()
  const fileRef = useRef<HTMLInputElement>(null)
  const aiRef = useRef<HTMLInputElement>(null)

  const [categories, setCategories] = useState<Category[]>([])
  const [aiEnabled, setAiEnabled] = useState(false)
  const [aiLoading, setAiLoading] = useState(false)
  const [aiMsg, setAiMsg] = useState('')
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)

  const [name, setName] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [subcategoryId, setSubcategoryId] = useState('')
  const [description, setDescription] = useState('')
  const [rating, setRating] = useState(0)
  const [isFavorite, setIsFavorite] = useState(false)
  const [consumedAt, setConsumedAt] = useState('')
  const [photos, setPhotos] = useState<string[]>([])
  const [mainPhoto, setMainPhoto] = useState(0)
  const [attributes, setAttributes] = useState<Attribute[]>([])

  const category = categories.find((c) => c.Id === categoryId)

  useEffect(() => {
    api.get<Category[]>('/api/categories').then((r) => setCategories(r.data))
    api.get('/api/ai/status').then((r) => setAiEnabled(r.data.enabled)).catch(() => {})
    if (editing) {
      api.get<Item>(`/api/items/${id}`).then((r) => {
        const i = r.data
        setName(i.Name); setCategoryId(i.CategoryId); setSubcategoryId(i.SubcategoryId ?? '')
        setDescription(i.Description ?? ''); setRating(i.Rating); setIsFavorite(i.IsFavorite)
        setConsumedAt(i.ConsumedAt ? i.ConsumedAt.slice(0, 10) : '')
        setPhotos(i.Photos.map((p) => p.Url)); setMainPhoto(Math.max(0, i.Photos.findIndex((p) => p.IsMain)))
        setAttributes(i.Attributes.length ? i.Attributes : [])
      })
    }
  }, [id])

  // Ao escolher categoria (sem editar), pre-carrega atributos sugeridos vazios
  useEffect(() => {
    if (editing || !category) return
    if (attributes.length === 0 && category.SuggestedAttributes.length) {
      setAttributes(category.SuggestedAttributes.map((n) => ({ Name: n, Value: '' })))
    }
  }, [categoryId])

  async function uploadFiles(files: FileList) {
    const remaining = 3 - photos.length
    const toUpload = Array.from(files).slice(0, remaining)
    if (toUpload.length === 0) return
    setUploading(true)
    try {
      const urls: string[] = []
      for (const f of toUpload) {
        const fd = new FormData()
        fd.append('file', f)
        const r = await api.post('/api/upload', fd)
        urls.push(r.data.url)
      }
      setPhotos((p) => [...p, ...urls])
    } finally {
      setUploading(false)
    }
  }

  async function analyzeWithAi(file: File) {
    setAiLoading(true)
    setAiMsg('Analisando a foto…')
    try {
      const fd = new FormData()
      fd.append('file', file)
      const r = await api.post<AiResult>('/api/ai/analyze', fd)
      const ai = r.data
      if (ai.name) setName(ai.name)
      if (ai.description) setDescription(ai.description)
      // Casar categoria pelo nome
      if (ai.category) {
        const match = categories.find((c) => c.Name.toLowerCase() === ai.category!.toLowerCase())
        if (match) {
          setCategoryId(match.Id)
          if (ai.subcategory) {
            const sub = match.Subcategories.find((s) => s.Name.toLowerCase() === ai.subcategory!.toLowerCase())
            if (sub) setSubcategoryId(sub.Id)
          }
        } else {
          setAiMsg(`Sugestão de categoria: "${ai.category}" (crie em Categorias, se quiser). Demais campos preenchidos.`)
        }
      }
      if (ai.attributes?.length) {
        setAttributes(ai.attributes.filter((a) => a.Name && a.Value))
      }
      // Usa a mesma foto como imagem do item
      await uploadFiles(dataTransferOf(file))
      if (!aiMsg) setAiMsg('Preenchido pela IA! Revise antes de salvar. ✨')
      setTimeout(() => setAiMsg(''), 6000)
    } catch (err: any) {
      setAiMsg(err.response?.data?.message || 'Não foi possível analisar a imagem.')
    } finally {
      setAiLoading(false)
    }
  }

  function dataTransferOf(file: File): FileList {
    const dt = new DataTransfer()
    dt.items.add(file)
    return dt.files
  }

  function setAttr(idx: number, patch: Partial<Attribute>) {
    setAttributes((a) => a.map((x, i) => (i === idx ? { ...x, ...patch } : x)))
  }

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    if (!name || !categoryId) return
    setSaving(true)
    const payload = {
      Name: name,
      CategoryId: categoryId,
      SubcategoryId: subcategoryId || null,
      Description: description || null,
      Rating: rating,
      IsFavorite: isFavorite,
      ConsumedAt: consumedAt ? new Date(consumedAt).toISOString() : null,
      PhotoUrls: photos,
      MainPhotoIndex: mainPhoto,
      Attributes: attributes.filter((a) => a.Name && a.Value),
    }
    try {
      if (editing) await api.put(`/api/items/${id}`, payload)
      else await api.post('/api/items', payload)
      navigate('/catalogo')
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={submit} className="max-w-2xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold">{editing ? 'Editar item' : 'Adicionar item'}</h1>

      {/* IA */}
      {aiEnabled && (
        <div className="bg-gradient-to-r from-amber-50 to-red-50 border border-amber-200 rounded-xl p-4">
          <div className="flex items-start gap-3">
            <Sparkles className="text-amber-500 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="font-medium text-stone-800">Preencher com IA</p>
              <p className="text-sm text-stone-600">Tire uma foto do prato ou do rótulo e a IA preenche os campos pra você revisar.</p>
              <button
                type="button"
                onClick={() => aiRef.current?.click()}
                disabled={aiLoading}
                className="mt-2 inline-flex items-center gap-2 bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold rounded-lg px-4 py-2 disabled:opacity-60"
              >
                {aiLoading ? <Loader2 size={16} className="animate-spin" /> : <Sparkles size={16} />} Analisar foto
              </button>
              {aiMsg && <p className="text-sm text-amber-700 mt-2">{aiMsg}</p>}
            </div>
          </div>
          <input ref={aiRef} type="file" accept="image/*" capture="environment" hidden onChange={(e) => e.target.files?.[0] && analyzeWithAi(e.target.files[0])} />
        </div>
      )}

      {/* Fotos */}
      <div>
        <label className="block text-sm font-medium mb-2">Fotos <span className="text-stone-400">(máx. 3)</span></label>
        <div className="flex flex-wrap gap-3">
          {photos.map((url, idx) => (
            <div key={idx} className="relative w-24 h-24 rounded-lg overflow-hidden group">
              <img src={url} alt="" className="w-full h-full object-cover" />
              <button type="button" onClick={() => { setMainPhoto(idx) }} className={`absolute bottom-1 left-1 rounded-full p-1 ${idx === mainPhoto ? 'bg-amber-500 text-white' : 'bg-white/80 text-stone-500'}`} title="Foto principal">
                <StarIcon size={12} className={idx === mainPhoto ? 'fill-white' : ''} />
              </button>
              <button type="button" onClick={() => { setPhotos((p) => p.filter((_, i) => i !== idx)); if (mainPhoto >= idx) setMainPhoto(0) }} className="absolute top-1 right-1 bg-black/60 text-white rounded-full p-1">
                <X size={12} />
              </button>
            </div>
          ))}
          {photos.length < 3 && (
            <button type="button" onClick={() => fileRef.current?.click()} disabled={uploading} className="w-24 h-24 rounded-lg border-2 border-dashed border-stone-300 flex flex-col items-center justify-center text-stone-400 hover:border-amber-400 hover:text-amber-500">
              {uploading ? <Loader2 size={20} className="animate-spin" /> : <ImagePlus size={20} />}
              <span className="text-xs mt-1">Adicionar</span>
            </button>
          )}
        </div>
        <input ref={fileRef} type="file" accept="image/*" multiple hidden onChange={(e) => e.target.files && uploadFiles(e.target.files)} />
      </div>

      {/* Nome */}
      <div>
        <label className="block text-sm font-medium mb-1">Nome *</label>
        <input value={name} onChange={(e) => setName(e.target.value)} required className="w-full border border-stone-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-amber-400" placeholder="Ex: Vinho Casillero del Diablo Reserva" />
      </div>

      {/* Categoria + Subcategoria */}
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium mb-1">Categoria *</label>
          <select value={categoryId} onChange={(e) => { setCategoryId(e.target.value); setSubcategoryId('') }} required className="w-full border border-stone-300 rounded-lg px-3 py-2">
            <option value="">Selecione…</option>
            {categories.map((c) => <option key={c.Id} value={c.Id}>{c.Icon} {c.Name}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">Subcategoria</label>
          <select value={subcategoryId} onChange={(e) => setSubcategoryId(e.target.value)} disabled={!category} className="w-full border border-stone-300 rounded-lg px-3 py-2 disabled:opacity-50">
            <option value="">—</option>
            {category?.Subcategories.map((s) => <option key={s.Id} value={s.Id}>{s.Name}</option>)}
          </select>
        </div>
      </div>

      {/* Nota + favorito + data */}
      <div className="flex flex-wrap items-end gap-6">
        <div>
          <label className="block text-sm font-medium mb-1">Minha nota</label>
          <StarRating value={rating} onChange={setRating} size={26} />
        </div>
        <label className="flex items-center gap-2 text-sm cursor-pointer select-none pb-1">
          <input type="checkbox" checked={isFavorite} onChange={(e) => setIsFavorite(e.target.checked)} className="w-4 h-4 accent-red-500" />
          Favorito
        </label>
        <div>
          <label className="block text-sm font-medium mb-1">Consumido em</label>
          <input type="date" value={consumedAt} onChange={(e) => setConsumedAt(e.target.value)} className="border border-stone-300 rounded-lg px-3 py-2 text-sm" />
        </div>
      </div>

      {/* Atributos flexíveis */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <label className="text-sm font-medium">Informações {category && <span className="text-stone-400">(sugeridas para {category.Name})</span>}</label>
          <button type="button" onClick={() => setAttributes((a) => [...a, { Name: '', Value: '' }])} className="flex items-center gap-1 text-amber-600 text-sm">
            <Plus size={14} /> Campo
          </button>
        </div>
        <div className="space-y-2">
          {attributes.map((a, idx) => (
            <div key={idx} className="flex gap-2">
              <input value={a.Name} onChange={(e) => setAttr(idx, { Name: e.target.value })} placeholder="Ex: Origem" className="w-1/3 border border-stone-300 rounded-lg px-3 py-2 text-sm" />
              <input value={a.Value} onChange={(e) => setAttr(idx, { Value: e.target.value })} placeholder="Ex: Chile" className="flex-1 border border-stone-300 rounded-lg px-3 py-2 text-sm" />
              <button type="button" onClick={() => setAttributes((att) => att.filter((_, i) => i !== idx))} className="text-stone-400 hover:text-red-500 px-2">
                <X size={16} />
              </button>
            </div>
          ))}
          {attributes.length === 0 && <p className="text-sm text-stone-400">Nenhum campo. Escolha uma categoria ou adicione manualmente.</p>}
        </div>
      </div>

      {/* Descrição */}
      <div>
        <label className="block text-sm font-medium mb-1">Descrição <span className="text-stone-400">(opcional)</span></label>
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} className="w-full border border-stone-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-amber-400" placeholder="Suas impressões…" />
      </div>

      <div className="flex gap-2">
        <button type="submit" disabled={saving} className="bg-amber-500 hover:bg-amber-600 text-white font-semibold rounded-lg px-6 py-2.5 disabled:opacity-60">
          {saving ? 'Salvando…' : editing ? 'Salvar alterações' : 'Adicionar ao catálogo'}
        </button>
        <button type="button" onClick={() => navigate(-1)} className="border border-stone-300 rounded-lg px-6 py-2.5 text-stone-600">Cancelar</button>
      </div>
    </form>
  )
}
