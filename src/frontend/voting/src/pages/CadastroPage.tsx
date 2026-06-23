import React, { useState } from 'react'
import { Link as RouterLink, Navigate, useNavigate } from 'react-router-dom'
import {
  Box,
  Paper,
  TextField,
  Button,
  Typography,
  Alert,
  FormControlLabel,
  Checkbox,
} from '@mui/material'
import { HowToVote } from '@mui/icons-material'
import { useAuth } from '../contexts/AuthContext'
import { api } from '../services/api'

const formatCPF = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 11)
  if (digits.length <= 3) return digits
  if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`
  if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`
  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`
}

const formatDataNascimento = (value: string): string => {
  const numbers = value.replace(/\D/g, '')
  if (numbers.length <= 2) return numbers
  if (numbers.length <= 4) return `${numbers.slice(0, 2)}/${numbers.slice(2)}`
  return `${numbers.slice(0, 2)}/${numbers.slice(2, 4)}/${numbers.slice(4, 8)}`
}

const formatCelular = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 11)
  if (digits.length <= 2) return digits.length ? `(${digits}` : ''
  if (digits.length <= 7) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`
}

const CadastroPage: React.FC = () => {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()

  const [nome, setNome] = useState('')
  const [cpf, setCpf] = useState('')
  const [dataNascimento, setDataNascimento] = useState('')
  const [matriculaBancaria, setMatriculaBancaria] = useState('')
  const [celular, setCelular] = useState('')
  const [email, setEmail] = useState('')
  const [banco, setBanco] = useState('')
  const [aceiteTermos, setAceiteTermos] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  if (isAuthenticated) {
    return <Navigate to="/enquetes" replace />
  }

  const verificarMatriculaExistente = async (matricula: string) => {
    const limpa = matricula.trim()
    if (!limpa) return
    try {
      const response = await api.get('/api/auth/associado/existe-matricula', {
        params: { matricula: limpa },
      })
      if (response.data?.existe) {
        setError('Matrícula bancária já cadastrada. Use a tela de login para votar.')
      }
    } catch {
      // Ignora falha de verificação; cadastro validará no submit
    }
  }

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')
    setSuccess('')

    if (!aceiteTermos) {
      setError('É necessário aceitar os termos de uso.')
      return
    }

    setIsSubmitting(true)
    try {
      await api.post('/api/auth/associado/cadastro', {
        nome: nome.trim(),
        cpf: cpf.replace(/\D/g, ''),
        dataNascimento,
        matriculaBancaria: matriculaBancaria.trim(),
        celular: celular.replace(/\D/g, ''),
        email: email.trim(),
        banco: banco.trim(),
        aceiteTermos: true,
      })
      setSuccess('Cadastro realizado com sucesso. Redirecionando para o login...')
      setTimeout(() => navigate('/login'), 2000)
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } }
      setError(axiosErr.response?.data?.message ?? 'Erro ao cadastrar. Tente novamente.')
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
      <Paper elevation={3} sx={{ p: 4, width: '100%', maxWidth: 480, flexShrink: 0 }}>
        <Box textAlign="center" mb={3}>
          <HowToVote color="primary" sx={{ fontSize: 48, mb: 1 }} />
          <Typography variant="h5" component="h1" gutterBottom>
            Cadastro para Votação
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Preencha seus dados para participar das enquetes
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        {success && (
          <Alert severity="success" sx={{ mb: 2 }}>
            {success}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            label="Matrícula Bancária"
            value={matriculaBancaria}
            onChange={(e) => setMatriculaBancaria(e.target.value)}
            onBlur={() => verificarMatriculaExistente(matriculaBancaria)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Nome Completo"
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Data de Nascimento"
            value={dataNascimento}
            onChange={(e) => setDataNascimento(formatDataNascimento(e.target.value))}
            placeholder="dd/mm/aaaa"
            fullWidth
            margin="normal"
            required
            inputProps={{ maxLength: 10 }}
          />
          <TextField
            label="CPF"
            value={cpf}
            onChange={(e) => setCpf(formatCPF(e.target.value))}
            placeholder="000.000.000-00"
            fullWidth
            margin="normal"
            required
            inputProps={{ maxLength: 14 }}
          />
          <TextField
            label="Celular"
            value={celular}
            onChange={(e) => setCelular(formatCelular(e.target.value))}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="E-mail"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <TextField
            label="Instituição Bancária"
            value={banco}
            onChange={(e) => setBanco(e.target.value)}
            fullWidth
            margin="normal"
            required
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={aceiteTermos}
                onChange={(e) => setAceiteTermos(e.target.checked)}
              />
            }
            label="Estou de acordo com os termos de uso"
            sx={{ mt: 1 }}
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            size="large"
            disabled={isSubmitting}
            sx={{ mt: 2 }}
          >
            {isSubmitting ? 'Cadastrando...' : 'Cadastrar'}
          </Button>
          <Button
            component={RouterLink}
            to="/login"
            fullWidth
            variant="outlined"
            size="large"
            sx={{ mt: 1.5 }}
          >
            Já sou cadastrado — entrar para votar
          </Button>
        </Box>

        <Box mt={2} textAlign="center">
          <Typography variant="caption" color="text.secondary">
            Após o cadastro, faça login com CPF, data de nascimento e matrícula bancária.
          </Typography>
        </Box>
      </Paper>
    </Box>
  )
}

export default CadastroPage
