import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Heart, ImageOff } from 'lucide-react'
import StarRating from './StarRating'
import Lightbox from './Lightbox'
import type { Item } from '../lib/api'

export default function ItemCard({ item }: { item: Item }) {
  const [lightbox, setLightbox] = useState(false)
  const photos = item.Photos.map((p) => p.Url)
  const mainIndex = Math.max(0, item.Photos.findIndex((p) => p.IsMain))

  return (
    <>
      <Link to={`/item/${item.Id}`} className="group bg-white rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow border border-stone-100 block">
        <div
          className={`aspect-[4/3] bg-stone-100 relative overflow-hidden ${photos.length ? 'cursor-zoom-in' : ''}`}
          onClick={(e) => {
            if (photos.length) {
              e.preventDefault()
              e.stopPropagation()
              setLightbox(true)
            }
          }}
        >
          {item.MainPhotoUrl ? (
            <img src={item.MainPhotoUrl} alt={item.Name} className="w-full h-full object-cover group-hover:scale-105 transition-transform" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-stone-300 text-4xl">
              {item.CategoryIcon || <ImageOff />}
            </div>
          )}
          {item.IsFavorite && (
            <span className="absolute top-2 right-2 bg-white/90 rounded-full p-1.5">
              <Heart size={14} className="fill-red-500 text-red-500" />
            </span>
          )}
        </div>
        <div className="p-3">
          <div className="flex items-center gap-1 text-xs text-stone-500 mb-1">
            <span>{item.CategoryIcon}</span>
            <span>{item.CategoryName}</span>
            {item.SubcategoryName && <span>· {item.SubcategoryName}</span>}
          </div>
          <h3 className="font-semibold text-stone-800 line-clamp-1">{item.Name}</h3>
          {(item.Establishment || item.City) && (
            <p className="text-xs text-stone-400 line-clamp-1 mt-0.5">{[item.Establishment, item.City].filter(Boolean).join(' · ')}</p>
          )}
          <div className="mt-1.5">
            <StarRating value={item.Rating} size={14} />
          </div>
        </div>
      </Link>

      {lightbox && <Lightbox images={photos} index={mainIndex} onClose={() => setLightbox(false)} />}
    </>
  )
}
