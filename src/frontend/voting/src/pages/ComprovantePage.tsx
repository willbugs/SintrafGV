import React, { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Button,
  Divider,
  Alert,
  Chip,
  Grid,
  Paper
} from '@mui/material'
import { 
  CheckCircle, 
  Print, 
  Share, 
  Home,
  QrCode,
  Security
} from '@mui/icons-material'
import { useAuth } from '../contexts/AuthContext'
import { api } from '../services/api'

interface ComprovanteVoto {
  id: string
  eleicaoTitulo: string
  dataHoraVoto: string
  hashVoto: string
  numeroComprovante: string
  associadoNome: string
  totalPerguntas: number
}

const ComprovantePage: React.FC = () => {
  const { votoId } = useParams<{ votoId: string }>()
  const navigate = useNavigate()
  const { associado } = useAuth()
  
  const [comprovante, setComprovante] = useState<ComprovanteVoto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (votoId) {
      carregarComprovante()
    }
  }, [votoId])

  const carregarComprovante = async () => {
    try {
      setLoading(true)
      const response = await api.get(`/api/eleicoes/comprovante/${votoId}`)
      const d = response.data
      setComprovante({
        id: d.id ?? d.Id ?? votoId ?? '',
        eleicaoTitulo: d.eleicaoTitulo ?? d.EleicaoTitulo ?? '',
        dataHoraVoto: d.dataHoraVoto ?? d.DataHoraVoto ?? '',
        hashVoto: d.hashVoto ?? d.HashVoto ?? '',
        numeroComprovante: d.numeroComprovante ?? d.NumeroComprovante ?? d.codigo ?? d.Codigo ?? '',
        associadoNome: d.associadoNome ?? d.AssociadoNome ?? associado?.nome ?? '',
        totalPerguntas: d.totalPerguntas ?? d.TotalPerguntas ?? 0
      })
    } catch (err) {
      setError('Erro ao carregar comprovante')
      console.error('Erro ao carregar comprovante:', err)
    } finally {
      setLoading(false)
    }
  }

  const handlePrint = () => {
    window.print()
  }

  const handleShare = async () => {
    if (navigator.share && comprovante) {
      try {
        await navigator.share({
          title: 'Comprovante de Votação - SintrafGV',
          text: `Votação confirmada na eleição: ${comprovante.eleicaoTitulo}\nComprovante: ${comprovante.numeroComprovante}`,
          url: window.location.href
        })
      } catch (err) {
        console.error('Erro ao compartilhar:', err)
      }
    } else {
      // Fallback: copiar para clipboard
      if (comprovante) {
        const texto = `Comprovante de Votação - SintrafGV
Eleição: ${comprovante.eleicaoTitulo}
Data/Hora: ${formatarDataHora(comprovante.dataHoraVoto)}
Comprovante: ${comprovante.numeroComprovante}
Hash: ${comprovante.hashVoto}`
        
        navigator.clipboard.writeText(texto).then(() => {
          alert('Comprovante copiado para a área de transferência!')
        })
      }
    }
  }

  const formatarDataHora = (dataString: string) => {
    const data = new Date(dataString)
    return data.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    })
  }

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <Typography>Gerando comprovante...</Typography>
      </Box>
    )
  }

  if (error || !comprovante) {
    return (
      <Container>
        <Alert severity="error" sx={{ mt: 3 }}>
          {error || 'Comprovante não encontrado'}
        </Alert>
        <Button onClick={() => navigate('/eleicoes')} sx={{ mt: 2 }}>
          Voltar às Eleições
        </Button>
      </Container>
    )
  }

  return (
    <Container maxWidth="md" sx={{ py: 3 }}>
      <Box className="printable">
        {/* Cabeçalho de Sucesso */}
        <Box textAlign="center" mb={4}>
          <CheckCircle color="success" sx={{ fontSize: 72, mb: 2 }} />
          <Typography variant="h4" component="h1" color="success.main" gutterBottom>
            Voto Confirmado!
          </Typography>
          <Typography variant="h6" color="text.secondary">
            Sua participação foi registrada com sucesso
          </Typography>
        </Box>

        {/* Comprovante Oficial */}
        <Card elevation={3} sx={{ mb: 4 }}>
          <CardContent sx={{ p: 4 }}>
            {/* Header do Comprovante */}
            <Box textAlign="center" mb={3}>
              <Typography variant="h5" component="h2" gutterBottom>
                🗳️ COMPROVANTE DE VOTAÇÃO
              </Typography>
              <Typography variant="subtitle1" color="primary" fontWeight="bold">
                SintrafGV - Sindicato dos Trabalhadores em Saúde
              </Typography>
              <Divider sx={{ my: 2 }} />
            </Box>

            {/* Dados da Votação */}
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="primary" gutterBottom>
                    📋 DADOS DA ELEIÇÃO
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Eleição:</strong> {comprovante.eleicaoTitulo}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Perguntas:</strong> {comprovante.totalPerguntas}
                  </Typography>
                </Paper>
              </Grid>

              <Grid item xs={12} md={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="primary" gutterBottom>
                    👤 DADOS DO VOTANTE
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Nome:</strong> {comprovante.associadoNome || associado?.nome || '—'}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>CPF:</strong> {associado?.cpf ?? '—'}
                  </Typography>
                </Paper>
              </Grid>

              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="primary" gutterBottom>
                    🕐 DADOS DA VOTAÇÃO
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Data/Hora:</strong> {formatarDataHora(comprovante.dataHoraVoto)}
                  </Typography>
                  <Typography variant="body2" gutterBottom>
                    <strong>Nº Comprovante:</strong> 
                    <Chip 
                      label={comprovante.numeroComprovante || '—'}
                      color="primary"
                      size="small"
                      sx={{ ml: 1, fontFamily: 'monospace' }}
                    />
                  </Typography>
                </Paper>
              </Grid>

              {comprovante.hashVoto && (
              <Grid item xs={12}>
                <Paper variant="outlined" sx={{ p: 2, bgcolor: 'grey.50' }}>
                  <Box display="flex" alignItems="center" mb={1}>
                    <Security color="primary" sx={{ mr: 1 }} />
                    <Typography variant="subtitle2" color="primary">
                      CÓDIGO DE INTEGRIDADE
                    </Typography>
                  </Box>
                  <Typography 
                    variant="caption" 
                    sx={{ 
                      fontFamily: 'monospace',
                      wordBreak: 'break-all',
                      fontSize: '0.7rem'
                    }}
                  >
                    {comprovante.hashVoto}
                  </Typography>
                  <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                    Este código garante a integridade e autenticidade do seu voto
                  </Typography>
                </Paper>
              </Grid>
              )}
            </Grid>

            {/* QR Code Placeholder */}
            <Box textAlign="center" mt={4}>
              <Paper variant="outlined" sx={{ p: 3, display: 'inline-block' }}>
                <QrCode sx={{ fontSize: 48, color: 'text.secondary' }} />
                <Typography variant="caption" display="block" color="text.secondary">
                  QR Code para verificação
                </Typography>
              </Paper>
            </Box>

            {/* Informações de Segurança */}
            <Alert severity="info" sx={{ mt: 3 }}>
              <Typography variant="body2">
                <strong>🔒 Informações Importantes:</strong>
                <br />
                • Este comprovante confirma apenas que você participou da votação
                <br />
                • Suas escolhas são mantidas em sigilo absoluto
                <br />
                • Guarde este comprovante para seus registros
                <br />
                • Em caso de dúvidas, entre em contato com a secretaria do sindicato
              </Typography>
            </Alert>
          </CardContent>
        </Card>

        {/* Ações (não imprimem) */}
        <Box className="no-print" display="flex" gap={2} justifyContent="center" flexWrap="wrap">
          <Button
            variant="outlined"
            startIcon={<Print />}
            onClick={handlePrint}
          >
            Imprimir
          </Button>
          
          <Button
            variant="outlined"
            startIcon={<Share />}
            onClick={handleShare}
          >
            Compartilhar
          </Button>
          
          <Button
            variant="contained"
            startIcon={<Home />}
            onClick={() => navigate('/eleicoes')}
          >
            Voltar às Eleições
          </Button>
        </Box>

        {/* Rodapé */}
        <Box textAlign="center" mt={4} className="no-print">
          <Typography variant="caption" color="text.secondary">
            SintrafGV - Sistema de Votação Eletrônica
            <br />
            Gerado em {formatarDataHora(new Date().toISOString())}
          </Typography>
        </Box>
      </Box>

      {/* CSS para impressão */}
      <style>
        {`
          @media print {
            .no-print {
              display: none !important;
            }
            
            .printable {
              max-width: 100% !important;
              margin: 0 !important;
              padding: 20px !important;
            }
            
            body {
              font-size: 12px !important;
            }
          }
        `}
      </style>
    </Container>
  )
}

export default ComprovantePage