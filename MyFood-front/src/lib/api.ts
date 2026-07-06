import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('myfood_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  (r) => r,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('myfood_token')
      if (!location.pathname.startsWith('/login')) location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api

// ==================== Tipos ====================
export interface UserResponse {
  Id: string
  Name: string
  Email: string
  IsAdmin: boolean
}

export interface Subcategory {
  Id: string
  CategoryId: string
  Name: string
  Order: number
  ItemCount: number
}

export interface Category {
  Id: string
  Name: string
  Icon?: string
  Color?: string
  Order: number
  ItemCount: number
  Subcategories: Subcategory[]
  SuggestedAttributes: string[]
}

export interface Attribute {
  Name: string
  Value: string
}

export interface Photo {
  Id: string
  Url: string
  Order: number
  IsMain: boolean
}

export interface Item {
  Id: string
  Name: string
  CategoryId: string
  CategoryName: string
  CategoryIcon?: string
  SubcategoryId?: string
  SubcategoryName?: string
  Description?: string
  City?: string
  Establishment?: string
  Rating: number
  IsFavorite: boolean
  ConsumedAt?: string
  MainPhotoUrl?: string
  Photos: Photo[]
  Attributes: Attribute[]
  CreatedAt: string
}

export interface Stats {
  TotalItems: number
  TotalFavorites: number
  TotalCategories: number
  AverageRating: number
  ByCategory: { Name: string; Icon?: string; Count: number }[]
  RecentItems: Item[]
}

// Resultado da analise por IA (Gemini devolve campos em minúsculo)
export interface AiResult {
  name?: string
  category?: string
  subcategory?: string
  establishment?: string
  city?: string
  description?: string
  attributes?: { name: string; value: string }[]
}
