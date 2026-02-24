import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { api } from '../services/api'

interface Associado {
  id: string
  nome: string
  cpf: string
  email?: string
  ativo: boolean
}

interface AuthContextType {
  associado: Associado | null
  isLoading: boolean
  login: (cpf: string, dataNascimento: string, matriculaBancaria: string) => Promise<boolean>
  logout: () => void
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [associado, setAssociado] = useState<Associado | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Verificar se há token armazenado ao inicializar
    const token = localStorage.getItem('voting-token')
    if (token) {
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`
      // TODO: Verificar se token é válido
      setIsLoading(false)
    } else {
      setIsLoading(false)
    }
  }, [])

  const login = async (cpf: string, dataNascimento: string, matriculaBancaria: string): Promise<boolean> => {
    try {
      setIsLoading(true)
      
      // Remover formatação do CPF (apenas números)
      const cpfLimpo = cpf.replace(/\D/g, '')
      
      const response = await api.post('/auth/associado/login', {
        cpf: cpfLimpo,
        dataNascimento,
        matriculaBancaria
      })

      const { token, associado: dadosAssociado } = response.data

      // Armazenar token
      localStorage.setItem('voting-token', token)
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`

      // Armazenar dados do associado
      setAssociado(dadosAssociado)

      return true
    } catch (error) {
      console.error('Erro no login:', error)
      return false
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    localStorage.removeItem('voting-token')
    delete api.defaults.headers.common['Authorization']
    setAssociado(null)
  }

  const isAuthenticated = !!associado

  const value: AuthContextType = {
    associado,
    isLoading,
    login,
    logout,
    isAuthenticated
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}