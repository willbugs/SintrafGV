import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Card,
  CardContent,
  Grid,
  LinearProgress,
  IconButton,
  CircularProgress,
  Chip,
  Divider
} from '@mui/material';
import { ArrowBack, HowToVote, Person, Poll } from '@mui/icons-material';
import { eleicoesAPI } from '../services/api';
import { useToast } from '../contexts/ToastContext';

interface ResultadoOpcao {
  opcaoId: string;
  texto: string;
  foto?: string;
  totalVotos: number;
  percentual: number;
}

interface ResultadoPergunta {
  perguntaId: string;
  texto: string;
  totalVotos: number;
  votosBranco: number;
  opcoes: ResultadoOpcao[];
}

interface ResultadoEleicao {
  eleicaoId: string;
  titulo: string;
  totalVotantes: number;
  totalHabilitados: number;
  percentualParticipacao: number;
  perguntas: ResultadoPergunta[];
}

const ResultadosEleicaoPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const [loading, setLoading] = useState(true);
  const [resultados, setResultados] = useState<ResultadoEleicao | null>(null);

  useEffect(() => {
    if (!id) return;
    
    const carregar = async () => {
      setLoading(true);
      try {
        const data = await eleicoesAPI.obterResultados(id);
        setResultados(data);
      } catch (err: unknown) {
        const errorMsg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
        toast.error('Erro', errorMsg || 'Erro ao carregar resultados da enquete.');
        navigate('/eleicoes');
      } finally {
        setLoading(false);
      }
    };

    carregar();
  }, [id, toast, navigate]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!resultados) {
    return (
      <Box>
        <Typography variant="h5" color="error">
          Resultados não encontrados
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IconButton onClick={() => navigate('/eleicoes')} sx={{ mr: 1 }}>
          <ArrowBack />
        </IconButton>
        <Typography variant="h4">Resultados da Enquete</Typography>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <HowToVote color="primary" sx={{ fontSize: 32 }} />
          <Box>
            <Typography variant="h5">{resultados.titulo}</Typography>
            <Typography variant="body2" color="text.secondary">
              Resultados oficiais da enquete
            </Typography>
          </Box>
        </Box>

        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} md={4}>
            <Card sx={{ textAlign: 'center', bgcolor: 'primary.50' }}>
              <CardContent>
                <Person sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
                <Typography variant="h4" color="primary">
                  {resultados.totalVotantes}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Pessoas Votaram
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card sx={{ textAlign: 'center', bgcolor: 'secondary.50' }}>
              <CardContent>
                <Poll sx={{ fontSize: 40, color: 'secondary.main', mb: 1 }} />
                <Typography variant="h4" color="secondary">
                  {resultados.percentualParticipacao.toFixed(1)}%
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Participação
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card sx={{ textAlign: 'center', bgcolor: 'success.50' }}>
              <CardContent>
                <HowToVote sx={{ fontSize: 40, color: 'success.main', mb: 1 }} />
                <Typography variant="h4" color="success.main">
                  {resultados.perguntas.length}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Pergunta(s)
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Paper>

      {resultados.perguntas.map((pergunta, index) => (
        <Paper key={pergunta.perguntaId} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Pergunta {index + 1}: {pergunta.texto}
          </Typography>
          
          <Box sx={{ mb: 2 }}>
            <Typography variant="body2" color="text.secondary">
              Total de votos: {pergunta.totalVotos}
              {pergunta.votosBranco > 0 && (
                <Chip 
                  label={`${pergunta.votosBranco} voto(s) em branco`} 
                  size="small" 
                  sx={{ ml: 2 }} 
                />
              )}
            </Typography>
          </Box>

          <Divider sx={{ my: 2 }} />

          {pergunta.opcoes.map((opcao) => (
            <Box key={opcao.opcaoId} sx={{ mb: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="subtitle1" fontWeight="medium">
                  {opcao.texto}
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    {opcao.totalVotos} voto(s)
                  </Typography>
                  <Chip 
                    label={`${opcao.percentual.toFixed(1)}%`}
                    color="primary"
                    size="small"
                  />
                </Box>
              </Box>
              
              <LinearProgress
                variant="determinate"
                value={opcao.percentual}
                sx={{ 
                  height: 10, 
                  borderRadius: 5,
                  bgcolor: 'grey.200',
                  '& .MuiLinearProgress-bar': {
                    borderRadius: 5,
                    bgcolor: opcao.percentual > 50 ? 'success.main' : 'primary.main'
                  }
                }}
              />
            </Box>
          ))}

          {pergunta.votosBranco > 0 && (
            <Box sx={{ mt: 2, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Votos em Branco
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    {pergunta.votosBranco} voto(s)
                  </Typography>
                  <Chip 
                    label={`${pergunta.totalVotos > 0 ? ((pergunta.votosBranco / pergunta.totalVotos) * 100).toFixed(1) : 0}%`}
                    color="default"
                    size="small"
                  />
                </Box>
              </Box>
              
              <LinearProgress
                variant="determinate"
                value={pergunta.totalVotos > 0 ? (pergunta.votosBranco / pergunta.totalVotos) * 100 : 0}
                sx={{ 
                  height: 8, 
                  borderRadius: 4,
                  bgcolor: 'grey.300',
                  '& .MuiLinearProgress-bar': {
                    borderRadius: 4,
                    bgcolor: 'grey.500'
                  }
                }}
              />
            </Box>
          )}
        </Paper>
      ))}

      <Paper sx={{ p: 2, bgcolor: 'grey.50' }}>
        <Typography variant="caption" color="text.secondary" display="block">
          💡 Resultados atualizados automaticamente. Os votos são contabilizados de forma anônima e segura.
        </Typography>
        <Typography variant="caption" color="text.secondary">
          🔒 O sigilo do voto é garantido - não é possível identificar quem votou em que opção.
        </Typography>
      </Paper>
    </Box>
  );
};

export default ResultadosEleicaoPage;