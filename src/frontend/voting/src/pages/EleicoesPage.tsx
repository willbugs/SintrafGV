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
  Logout,
  AttachFile
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
  arquivoAnexo?: string | null
  bancoNome?: string | null
  apenasAssociados?: boolean
  apenasAtivos?: boolean
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
      const raw = response.data ?? []
      // API pode retornar PascalCase (PodeVotar, JaVotou) ou camelCase; /ativas = só abertas, então se não vier flag assume pode votar
      setEleicoes(raw.map((e: any) => ({
        id: e.id ?? e.Id,
        titulo: e.titulo ?? e.Titulo ?? '',
        descricao: e.descricao ?? e.Descricao ?? '',
        inicioVotacao: e.inicioVotacao ?? e.InicioVotacao ?? '',
        fimVotacao: e.fimVotacao ?? e.FimVotacao ?? '',
        status: (e.status ?? e.Status ?? 'Aberta') as Eleicao['status'],
        arquivoAnexo: e.arquivoAnexo ?? e.ArquivoAnexo ?? null,
        bancoNome: e.bancoNome ?? e.BancoNome ?? null,
        apenasAssociados: e.apenasAssociados ?? e.ApenasAssociados ?? true,
        apenasAtivos: e.apenasAtivos ?? e.ApenasAtivos ?? true,
        totalPerguntas: e.totalPerguntas ?? e.TotalPerguntas ?? 0,
        podeVotar: e.podeVotar ?? e.PodeVotar ?? true,
        jaVotou: e.jaVotou ?? e.JaVotou ?? false
      })))
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

  const formatDateTime = (s: string) => {
    const d = new Date(s)
    if (Number.isNaN(d.getTime())) return '—'
    return d.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
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
                      label={(eleicao.status as string | number) === 2 || eleicao.status === 'Aberta' ? 'Aberta' : String(eleicao.status)}
                      color={(eleicao.status as string | number) === 2 || eleicao.status === 'Aberta' ? 'success' : 'default'}
                      size="small"
                    />
                    <Box sx={{ mt: 1.5, display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
                      <Chip
                        size="small"
                        variant="outlined"
                        label={`${eleicao.totalPerguntas} pergunta(s)`}
                      />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={eleicao.arquivoAnexo ? 'Com anexo' : 'Sem anexo'}
                        icon={eleicao.arquivoAnexo ? <AttachFile /> : undefined}
                      />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={eleicao.bancoNome ? `Banco: ${eleicao.bancoNome}` : 'Banco: todos'}
                      />
                    </Box>
                    <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1.5 }}>
                      Início: {formatDateTime(eleicao.inicioVotacao)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary" display="block">
                      Fim: {formatDateTime(eleicao.fimVotacao)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 0.5 }}>
                      Acesso: {eleicao.apenasAssociados ? 'Apenas associados' : 'Associados e não associados'} | {eleicao.apenasAtivos ? 'Somente ativos' : 'Ativos e inativos'}
                    </Typography>
                    {!eleicao.podeVotar && !eleicao.jaVotou && (
                      <Alert severity="warning" sx={{ mt: 1.5, py: 0 }}>
                        Votação indisponível para seu perfil ou fora do período.
                      </Alert>
                    )}
                    {eleicao.jaVotou && (
                      <Alert severity="info" sx={{ mt: 1.5, py: 0 }}>
                        Seu voto já foi registrado nesta enquete.
                      </Alert>
                    )}
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