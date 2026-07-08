import { useEffect, useState } from 'react'
import { Shield, Ban, CheckCircle2, Trash2, Boxes } from 'lucide-react'
import api, { type AdminUser } from '../lib/api'

export default function AdminUsers() {
  const [users, setUsers] = useState<AdminUser[]>([])
  const [loading, setLoading] = useState(true)

  function load() {
    setLoading(true)
    api.get<AdminUser[]>('/api/admin/users').then((r) => setUsers(r.data)).finally(() => setLoading(false))
  }
  useEffect(load, [])

  async function toggle(u: AdminUser) {
    try {
      await api.post(`/api/admin/users/${u.Id}/toggle-active`)
      load()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Erro')
    }
  }

  async function remove(u: AdminUser) {
    if (!confirm(`Excluir "${u.Name}" e todos os itens dele? Essa ação não volta.`)) return
    try {
      await api.delete(`/api/admin/users/${u.Id}`)
      load()
    } catch (e: any) {
      alert(e.response?.data?.message || 'Erro')
    }
  }

  return (
    <div className="max-w-3xl mx-auto space-y-5">
      <div>
        <h1 className="text-2xl font-bold">Usuários</h1>
        <p className="text-stone-500 text-sm">Gerencie quem usa a plataforma.</p>
      </div>

      {loading ? (
        <p className="text-stone-400">Carregando…</p>
      ) : (
        <div className="space-y-2">
          {users.map((u) => (
            <div key={u.Id} className={`bg-white border rounded-xl p-3 flex items-center gap-3 ${u.IsActive ? 'border-stone-100' : 'border-red-200 bg-red-50/40'}`}>
              <div className="w-10 h-10 rounded-full bg-stone-100 flex items-center justify-center font-semibold text-stone-500">
                {u.Name.charAt(0).toUpperCase()}
              </div>
              <div className="flex-1 min-w-0">
                <div className="font-semibold flex items-center gap-1.5">
                  {u.Name}
                  {u.IsAdmin && <span className="inline-flex items-center gap-1 text-xs bg-amber-100 text-amber-700 rounded-full px-2 py-0.5"><Shield size={11} /> admin</span>}
                  {!u.IsActive && <span className="text-xs bg-red-100 text-red-600 rounded-full px-2 py-0.5">bloqueado</span>}
                </div>
                <div className="text-sm text-stone-500 truncate">{u.Email}</div>
                <div className="text-xs text-stone-400 flex items-center gap-1"><Boxes size={12} /> {u.ItemCount} itens · desde {new Date(u.CreatedAt).toLocaleDateString('pt-BR')}</div>
              </div>
              {!u.IsAdmin && (
                <div className="flex items-center gap-1">
                  <button onClick={() => toggle(u)} title={u.IsActive ? 'Bloquear' : 'Desbloquear'} className={`p-2 rounded-lg ${u.IsActive ? 'text-stone-400 hover:text-red-500' : 'text-green-600 hover:bg-green-50'}`}>
                    {u.IsActive ? <Ban size={17} /> : <CheckCircle2 size={17} />}
                  </button>
                  <button onClick={() => remove(u)} title="Excluir" className="p-2 rounded-lg text-stone-400 hover:text-red-500">
                    <Trash2 size={17} />
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
