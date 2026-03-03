import React, { useState } from 'react'
import { Navigate } from 'react-router-dom'
import {
  Box,
  Paper,
  TextField,
  Button,
  Typography,
  Alert,
  IconButton,
  InputAdornment
} from '@mui/material'
import { Visibility, VisibilityOff, HowToVote } from '@mui/icons-material'
// Removido DatePicker para eliminar dependência date-fns problemática
import { useAuth } from '../contexts/AuthContext'

const LoginPage: React.FC = () => {
  const { login, isAuthenticated, isLoading } = useAuth()
  const [formData, setFormData] = useState({
    cpf: '',
    dataNascimento: '',
    matriculaBancaria: ''
  })
  const [showMatricula, setShowMatricula] = useState(false)
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Se já está autenticado, redirecionar para eleições
  if (isAuthenticated) {
    return <Navigate to="/eleicoes" replace />
  }

  const formatCPF = (value: string) => {
    const numbers = value.replace(/\D/g, '')
    return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
  }

  const handleCPFChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value
    const formattedCPF = formatCPF(value)
    if (formattedCPF.length <= 14) {
      setFormData(prev => ({ ...prev, cpf: formattedCPF }))
    }
  }

  // Máscara dd/mm/aaaa - usuário digita e formatamos
  const formatDataNascimento = (value: string) => {
    const numbers = value.replace(/\D/g, '')
    if (numbers.length <= 2) return numbers
    if (numbers.length <= 4) return `${numbers.slice(0, 2)}/${numbers.slice(2)}`
    return `${numbers.slice(0, 2)}/${numbers.slice(2, 4)}/${numbers.slice(4, 8)}`
  }

  const handleDataNascimentoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value
    const formatted = formatDataNascimento(value)
    if (formatted.length <= 10) {
      setFormData(prev => ({ ...prev, dataNascimento: formatted }))
    }
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    
    if (!formData.cpf || !formData.dataNascimento || !formData.matriculaBancaria) {
      setError('Todos os campos são obrigatórios')
      return
    }

    setIsSubmitting(true)

    try {
      const dataNascimentoFormatada = formData.dataNascimento
      
      const success = await login(
        formData.cpf,
        dataNascimentoFormatada,
        formData.matriculaBancaria
      )

      if (!success) {
        setError('Dados inválidos. Verifique CPF, data de nascimento e matrícula bancária.')
      }
    } catch (err) {
      setError('Erro no sistema. Tente novamente mais tarde.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
      <Box
        sx={{
          minHeight: '100vh',
          width: '100%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: '#f5f5f5',
          py: 3,
          px: 2,
          boxSizing: 'border-box',
        }}
      >
        <Paper
          elevation={3}
          sx={{
            p: 4,
            width: '100%',
            maxWidth: 400,
            flexShrink: 0,
          }}
        >
            {/* Header */}
            <Box textAlign="center" mb={3}>
              <HowToVote color="primary" sx={{ fontSize: 48, mb: 1 }} />
              <Typography variant="h4" component="h1" gutterBottom>
                SintrafGV
              </Typography>
              <Typography variant="subtitle1" color="text.secondary">
                Sistema de Votação
              </Typography>
            </Box>

            {/* Erro */}
            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}

            {/* Formulário */}
            <Box component="form" onSubmit={handleSubmit}>
              {/* CPF - pattern valida formato XXX.XXX.XXX-XX (11 dígitos formatados) */}
              <TextField
                label="CPF"
                value={formData.cpf}
                onChange={handleCPFChange}
                placeholder="000.000.000-00"
                fullWidth
                margin="normal"
                required
                inputProps={{
                  inputMode: 'numeric',
                  maxLength: 14,
                  pattern: '[0-9]{3}\\.[0-9]{3}\\.[0-9]{3}-[0-9]{2}',
                  title: 'Digite o CPF no formato 000.000.000-00'
                }}
              />

              {/* Data de Nascimento - type=text com dd/mm/aaaa (Brasil); type=date exigia YYYY-MM-DD e gerava "faça corresponder ao formato pedido" quando usuário digitava dd/mm/yyyy */}
              <TextField
                fullWidth
                margin="normal"
                label="Data de Nascimento"
                type="text"
                value={formData.dataNascimento}
                onChange={handleDataNascimentoChange}
                placeholder="dd/mm/aaaa"
                required
                inputProps={{
                  inputMode: 'numeric',
                  maxLength: 10,
                  pattern: '(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/[0-9]{4}',
                  title: 'Digite a data no formato dd/mm/aaaa'
                }}
              />

              {/* Matrícula Bancária */}
              <TextField
                label="Matrícula Bancária"
                type={showMatricula ? 'text' : 'password'}
                value={formData.matriculaBancaria}
                onChange={(e) => setFormData({ ...formData, matriculaBancaria: e.target.value })}
                fullWidth
                margin="normal"
                required
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowMatricula(!showMatricula)}
                        edge="end"
                      >
                        {showMatricula ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />

              {/* Botão de Login */}
              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={isSubmitting || isLoading}
                sx={{ mt: 3 }}
              >
                {isSubmitting ? 'Entrando...' : 'Entrar'}
              </Button>
            </Box>

            {/* Informações de Segurança */}
            <Box mt={3}>
              <Typography variant="caption" color="text.secondary" align="center" display="block">
                🔒 Seus dados são protegidos e utilizados apenas para autenticação
              </Typography>
            </Box>
          </Paper>
      </Box>
  )
}

export default LoginPage