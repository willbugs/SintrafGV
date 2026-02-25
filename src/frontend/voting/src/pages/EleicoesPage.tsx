import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  CardActions,
  Button,
  Chip,
  Grid,
  AppBar,
  Toolbar,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Alert
} from '@mui/material'
import { 
  HowToVote, 
  AccountCircle, 
  Logout
} from '@mui/icons-material'
import { useAuth } from '../contexts/AuthContext'
import { api } from '../services/api'

interface Eleicao {
  id: string
  titulo: string
  descricao: string
  inicioVotacao: string
  fimVotacao: string
  status: 'Rascunho' | 'Aberta' | 'Encerrada' | 'Apurada' | 'Cancelada'
  podeVotar: boolean
  jaVotou: boolean
  totalPerguntas: number
}

const EleicoesPage: React.FC = () => {
  const { associado, logout } = useAuth()
  const navigate = useNavigate()
  const [eleicoes, setEleicoes] = useState<Eleicao[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  useEffect(() => {
    carregarEleicoes()
  }, [])

  const carregarEleicoes = async () => {
    try {
      setLoading(true)
      const response = await api.get('/api/eleicoes/ativas')
      setEleicoes(response.data)
    } catch (err) {
      setError('Erro ao carregar eleições disponíveis')
      console.error('Erro ao carregar eleições:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleVotar = (eleicaoId: string) => {
    navigate(`/votacao/${eleicaoId}`)
  }

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
  }

  const handleLogout = () => {
    handleMenuClose()
    logout()
    navigate('/login')
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <Typography>Carregando eleições...</Typography>
      </Box>
    )
  }

  return (
    <Box>
      <AppBar position="sticky">
        <Toolbar>
          <HowToVote sx={{ mr: 2 }} />
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            SintrafGV - Eleições
          </Typography>
          
          <Box display="flex" alignItems="center">
            <Typography variant="body2" sx={{ mr: 1, display: { xs: 'none', sm: 'block' } }}>
              {associado?.nome}
            </Typography>
            <IconButton color="inherit" onClick={handleMenuOpen}>
              <Avatar sx={{ width: 32, height: 32 }}>
                <AccountCircle />
              </Avatar>
            </IconButton>
          </Box>

          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
          >
            <MenuItem onClick={handleLogout}>
              <Logout sx={{ mr: 1 }} fontSize="small" />
              Sair
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 3 }}>
        <Box mb={3}>
          <Typography variant="h4" component="h1" gutterBottom>
            Bem-vindo, {associado?.nome?.split(' ')[0]}!
          </Typography>
          <Typography variant="subtitle1" color="text.secondary">
            Selecione uma eleição para participar da votação
          </Typography>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {eleicoes.length === 0 ? (
          <Alert severity="info">
            Não há eleições disponíveis no momento.
          </Alert>
        ) : (
          <Grid container spacing={3}>
            {eleicoes.map((eleicao) => (
              <Grid item xs={12} md={6} lg={4} key={eleicao.id}>
                <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Typography variant="h6" component="h2" gutterBottom>
                      {eleicao.titulo}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" paragraph>
                      {eleicao.descricao}
                    </Typography>
                    <Chip
                      label={eleicao.status}
                      color={eleicao.status === 'Aberta' ? 'success' : 'default'}
                      size="small"
                    />
                  </CardContent>
                  <CardActions>
                    <Button
                      variant={eleicao.podeVotar && !eleicao.jaVotou ? 'contained' : 'outlined'}
                      fullWidth
                      disabled={!eleicao.podeVotar || eleicao.jaVotou}
                      onClick={() => handleVotar(eleicao.id)}
                    >
                      {eleicao.jaVotou ? 'Já Votou' : eleicao.podeVotar ? 'Votar Agora' : 'Não Disponível'}
                    </Button>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Container>
    </Box>
  )
}

export default EleicoesPage