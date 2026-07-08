import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { UtensilsCrossed } from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await register(name, email, password)
      navigate('/')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Falha ao criar conta')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-stone-900 px-4">
      <form onSubmit={submit} className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-8">
        <div className="flex flex-col items-center mb-6">
          <div className="w-14 h-14 rounded-2xl bg-stone-900 flex items-center justify-center mb-3">
            <UtensilsCrossed className="text-amber-400" size={28} />
          </div>
          <h1 className="text-2xl font-bold">Criar conta</h1>
          <p className="text-stone-500 text-sm">Seu catálogo pessoal de comidas e bebidas</p>
        </div>

        {error && <div className="bg-red-50 text-red-700 text-sm rounded-lg px-3 py-2 mb-4">{error}</div>}

        <label className="block text-sm font-medium mb-1">Nome</label>
        <input value={name} onChange={(e) => setName(e.target.value)} required className="w-full border border-stone-300 rounded-lg px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-amber-400" />

        <label className="block text-sm font-medium mb-1">Email</label>
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required className="w-full border border-stone-300 rounded-lg px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-amber-400" />

        <label className="block text-sm font-medium mb-1">Senha <span className="text-stone-400">(mín. 6)</span></label>
        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} className="w-full border border-stone-300 rounded-lg px-3 py-2 mb-6 focus:outline-none focus:ring-2 focus:ring-amber-400" />

        <button type="submit" disabled={loading} className="w-full bg-amber-500 hover:bg-amber-600 text-white font-semibold rounded-lg py-2.5 transition-colors disabled:opacity-60">
          {loading ? 'Criando…' : 'Criar conta'}
        </button>

        <p className="text-center text-sm text-stone-500 mt-4">
          Já tem conta? <Link to="/login" className="text-amber-600 font-medium">Entrar</Link>
        </p>
      </form>
    </div>
  )
}
