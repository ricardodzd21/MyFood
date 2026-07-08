import { useEffect, useState } from 'react'
import { X, ChevronLeft, ChevronRight } from 'lucide-react'

interface Props {
  images: string[]
  index?: number
  onClose: () => void
}

export default function Lightbox({ images, index = 0, onClose }: Props) {
  const [i, setI] = useState(index)

  const go = (delta: number) => setI((v) => (v + delta + images.length) % images.length)

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose()
      if (e.key === 'ArrowLeft') go(-1)
      if (e.key === 'ArrowRight') go(1)
    }
    window.addEventListener('keydown', onKey)
    document.body.style.overflow = 'hidden'
    return () => {
      window.removeEventListener('keydown', onKey)
      document.body.style.overflow = ''
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [images.length])

  if (!images.length) return null

  return (
    <div className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center select-none" onClick={onClose}>
      <button onClick={onClose} className="absolute top-4 right-4 text-white/80 hover:text-white p-2" aria-label="Fechar">
        <X size={28} />
      </button>

      {images.length > 1 && (
        <>
          <button onClick={(e) => { e.stopPropagation(); go(-1) }} className="absolute left-2 md:left-6 text-white/70 hover:text-white p-2" aria-label="Anterior">
            <ChevronLeft size={40} />
          </button>
          <button onClick={(e) => { e.stopPropagation(); go(1) }} className="absolute right-2 md:right-6 text-white/70 hover:text-white p-2" aria-label="Próxima">
            <ChevronRight size={40} />
          </button>
        </>
      )}

      <img
        src={images[i]}
        alt=""
        onClick={(e) => e.stopPropagation()}
        className="max-h-[88vh] max-w-[92vw] object-contain rounded-lg"
      />

      {images.length > 1 && (
        <div className="absolute bottom-5 left-1/2 -translate-x-1/2 flex items-center gap-3">
          <span className="text-white/80 text-sm bg-black/40 rounded-full px-3 py-1">{i + 1} / {images.length}</span>
          <div className="flex gap-1.5">
            {images.map((_, idx) => (
              <button
                key={idx}
                onClick={(e) => { e.stopPropagation(); setI(idx) }}
                className={`w-2 h-2 rounded-full ${idx === i ? 'bg-white' : 'bg-white/40'}`}
                aria-label={`Foto ${idx + 1}`}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
