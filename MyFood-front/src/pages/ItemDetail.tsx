import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { Heart, Pencil, Trash2, ArrowLeft, Calendar, MapPin } from 'lucide-react'
import api, { type Item } from '../lib/api'
import StarRating from '../components/StarRating'

export default function ItemDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [item, setItem] = useState<Item | null>(null)
  const [loading, setLoading] = useState(true)
  const [activePhoto, setActivePhoto] = useState(0)

  useEffect(() => {
    api.get<Item>(`/api/items/${id}`).then((r) => setItem(r.data)).finally(() => setLoading(false))
  }, [id])

  async function toggleFav() {
    if (!item) return
    const r = await api.post(`/api/items/${item.Id}/toggle-favorite`)
    setItem({ ...item, IsFavorite: r.data.IsFavorite })
  }

  async function remove() {
    if (!item || !confirm(`Excluir "${item.Name}"?`)) return
    await api.delete(`/api/items/${item.Id}`)
    navigate('/catalogo')
  }

  if (loading) return <p className="text-stone-400">Carregando…</p>
  if (!item) return <p className="text-stone-400">Item não encontrado.</p>

  return (
    <div className="max-w-3xl mx-auto space-y-5">
      <button onClick={() => navigate(-1)} className="flex items-center gap-1 text-stone-500 hover:text-stone-800 text-sm">
        <ArrowLeft size={16} /> Voltar
      </button>

      <div className="grid md:grid-cols-2 gap-6">
        {/* Fotos */}
        <div>
          <div className="aspect-square bg-stone-100 rounded-xl overflow-hidden flex items-center justify-center text-6xl">
            {item.Photos.length > 0 ? (
              <img src={item.Photos[activePhoto]?.Url} alt={item.Name} className="w-full h-full object-cover" />
            ) : (
              item.CategoryIcon || '🍽️'
            )}
          </div>
          {item.Photos.length > 1 && (
            <div className="flex gap-2 mt-2">
              {item.Photos.map((p, idx) => (
                <button key={p.Id} onClick={() => setActivePhoto(idx)} className={`w-16 h-16 rounded-lg overflow-hidden border-2 ${idx === activePhoto ? 'border-amber-500' : 'border-transparent'}`}>
                  <img src={p.Url} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Info */}
        <div>
          <div className="flex items-center gap-2 text-sm text-stone-500 mb-1">
            <span>{item.CategoryIcon} {item.CategoryName}</span>
            {item.SubcategoryName && <span>· {item.SubcategoryName}</span>}
          </div>
          <h1 className="text-2xl font-bold">{item.Name}</h1>

          <div className="flex items-center gap-3 mt-3">
            <StarRating value={item.Rating} size={22} />
            <button onClick={toggleFav} className={`flex items-center gap-1.5 text-sm rounded-full px-3 py-1.5 border ${item.IsFavorite ? 'bg-red-50 border-red-200 text-red-600' : 'border-stone-300 text-stone-600'}`}>
              <Heart size={15} className={item.IsFavorite ? 'fill-red-500 text-red-500' : ''} /> {item.IsFavorite ? 'Favorito' : 'Favoritar'}
            </button>
          </div>

          {(item.Establishment || item.City) && (
            <p className="flex items-center gap-1.5 text-sm text-stone-500 mt-3">
              <MapPin size={14} /> {[item.Establishment, item.City].filter(Boolean).join(' · ')}
            </p>
          )}

          {item.ConsumedAt && (
            <p className="flex items-center gap-1.5 text-sm text-stone-500 mt-1">
              <Calendar size={14} /> Consumido em {new Date(item.ConsumedAt).toLocaleDateString('pt-BR')}
            </p>
          )}

          {item.Description && <p className="mt-3 text-stone-700 whitespace-pre-line">{item.Description}</p>}

          {item.Attributes.length > 0 && (
            <div className="mt-4 border border-stone-200 rounded-xl divide-y divide-stone-100">
              {item.Attributes.map((a) => (
                <div key={a.Name} className="flex justify-between px-3 py-2 text-sm">
                  <span className="text-stone-500">{a.Name}</span>
                  <span className="font-medium text-stone-800">{a.Value}</span>
                </div>
              ))}
            </div>
          )}

          <div className="flex gap-2 mt-6">
            <Link to={`/editar/${item.Id}`} className="flex items-center gap-1.5 bg-stone-900 text-white rounded-lg px-4 py-2 text-sm">
              <Pencil size={15} /> Editar
            </Link>
            <button onClick={remove} className="flex items-center gap-1.5 border border-red-200 text-red-600 rounded-lg px-4 py-2 text-sm hover:bg-red-50">
              <Trash2 size={15} /> Excluir
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
