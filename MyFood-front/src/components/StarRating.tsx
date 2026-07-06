import { Star } from 'lucide-react'

interface Props {
  value: number
  onChange?: (v: number) => void
  size?: number
}

export default function StarRating({ value, onChange, size = 20 }: Props) {
  const interactive = !!onChange
  return (
    <div className="flex items-center gap-0.5">
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          disabled={!interactive}
          onClick={() => onChange?.(n === value ? 0 : n)}
          className={interactive ? 'cursor-pointer transition-transform hover:scale-110' : 'cursor-default'}
          aria-label={`${n} estrelas`}
        >
          <Star
            size={size}
            className={n <= value ? 'fill-amber-400 text-amber-400' : 'text-stone-300'}
          />
        </button>
      ))}
    </div>
  )
}
