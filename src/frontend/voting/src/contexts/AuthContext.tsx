import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
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
    const token = localStorage.getItem('voting-token')
    const savedAssociado = localStorage.getItem('voting-associado')
    if (token && savedAssociado) {
      try {
        setAssociado(JSON.parse(savedAssociado))
      } catch {
        localStorage.removeItem('voting-token')
        localStorage.removeItem('voting-associado')
      }
    }
    setIsLoading(false)
  }, [])

  const login = async (cpf: string, dataNascimento: string, matriculaBancaria: string): Promise<boolean> => {
    try {
      setIsLoading(true)
      
      // Remover formatação do CPF (apenas números)
      const cpfLimpo = cpf.replace(/\D/g, '')
      
      const response = await api.post('/api/auth/associado/login', {
        cpf: cpfLimpo,
        dataNascimento,
        matriculaBancaria
      })

      const { token, associado: dadosAssociado } = response.data

      localStorage.setItem('voting-token', token)
      localStorage.setItem('voting-associado', JSON.stringify(dadosAssociado))
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
    localStorage.removeItem('voting-associado')
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