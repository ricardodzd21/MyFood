import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './contexts/AuthContext'
import Layout from './components/Layout'
import Login from './pages/Login'
import Register from './pages/Register'
import Home from './pages/Home'
import Catalog from './pages/Catalog'
import ItemDetail from './pages/ItemDetail'
import ItemForm from './pages/ItemForm'
import Categories from './pages/Categories'
import AdminUsers from './pages/AdminUsers'
import type { ReactNode } from 'react'

function Protected({ children, adminOnly }: { children: ReactNode; adminOnly?: boolean }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="min-h-screen flex items-center justify-center text-stone-400">Carregando…</div>
  if (!user) return <Navigate to="/login" replace />
  if (adminOnly && !user.IsAdmin) return <Navigate to="/" replace />
  return <Layout>{children}</Layout>
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/cadastro" element={<Register />} />
      <Route path="/" element={<Protected><Home /></Protected>} />
      <Route path="/catalogo" element={<Protected><Catalog /></Protected>} />
      <Route path="/item/:id" element={<Protected><ItemDetail /></Protected>} />
      <Route path="/novo" element={<Protected><ItemForm /></Protected>} />
      <Route path="/editar/:id" element={<Protected><ItemForm /></Protected>} />
      <Route path="/categorias" element={<Protected adminOnly><Categories /></Protected>} />
      <Route path="/usuarios" element={<Protected adminOnly><AdminUsers /></Protected>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
