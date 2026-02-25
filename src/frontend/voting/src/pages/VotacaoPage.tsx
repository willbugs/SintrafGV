import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Card,
  CardContent,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormControl,
  FormLabel,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stepper,
  Step,
  StepLabel,
  Chip
} from '@mui/material';
import {
  HowToVote as VoteIcon,
  CheckCircle as CheckIcon,
  Warning as WarningIcon
} from '@mui/icons-material';
import { api } from '../services/api';

interface Candidato {
  id: string;
  nome: string;
  numero: string;
  partido?: string;
  foto?: string;
  proposta?: string;
}

interface Pergunta {
  id: string;
  titulo: string;
  descricao?: string;
  tipo: 'MULTIPLA_ESCOLHA' | 'UNICA_ESCOLHA';
  obrigatoria: boolean;
  candidatos: Candidato[];
}

interface Eleicao {
  id: string;
  titulo: string;
  descricao: string;
  inicioVotacao: string;
  fimVotacao: string;
  status: string;
  perguntas: Pergunta[];
}

interface VotoResposta {
  perguntaId: string;
  candidatoId: string;
}

const VotacaoPage: React.FC = () => {
  const { eleicaoId } = useParams<{ eleicaoId: string }>();
  const navigate = useNavigate();
  
  const [eleicao, setEleicao] = useState<Eleicao | null>(null);
  const [loading, setLoading] = useState(true);
  const [votando, setVotando] = useState(false);
  const [respostas, setRespostas] = useState<VotoResposta[]>([]);
  const [etapaAtual, setEtapaAtual] = useState(0);
  const [showConfirmacao, setShowConfirmacao] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    if (eleicaoId) {
      carregarEleicao();
    }
  }, [eleicaoId]);

  const carregarEleicao = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/api/eleicoes/${eleicaoId}`);
      setEleicao(response.data);
      
      // Verificar se a eleição está ativa
      if (response.data.status !== 'Ativa') {
        setErro('Esta eleição não está disponível para votação.');
      }
    } catch (error) {
      console.error('Erro ao carregar eleição:', error);
      setErro('Erro ao carregar dados da eleição.');
    } finally {
      setLoading(false);
    }
  };

  const handleRespostaChange = (perguntaId: string, candidatoId: string) => {
    setRespostas(prev => {
      const novasRespostas = prev.filter(r => r.perguntaId !== perguntaId);
      return [...novasRespostas, { perguntaId, candidatoId }];
    });
  };

  const proximaEtapa = () => {
    if (!eleicao) return;
    
    const perguntaAtual = eleicao.perguntas[etapaAtual];
    const resposta = respostas.find(r => r.perguntaId === perguntaAtual.id);
    
    if (perguntaAtual.obrigatoria && !resposta) {
      setErro('Esta pergunta é obrigatória. Selecione uma opção.');
      return;
    }
    
    setErro(null);
    
    if (etapaAtual < eleicao.perguntas.length - 1) {
      setEtapaAtual(prev => prev + 1);
    } else {
      setShowConfirmacao(true);
    }
  };

  const etapaAnterior = () => {
    if (etapaAtual > 0) {
      setEtapaAtual(prev => prev - 1);
      setErro(null);
    }
  };

  const confirmarVoto = async () => {
    if (!eleicao || !eleicaoId) return;
    
    try {
      setVotando(true);
      
      // Verificar se todas as perguntas obrigatórias foram respondidas
      const perguntasObrigatorias = eleicao.perguntas.filter(p => p.obrigatoria);
      const respostasObrigatorias = perguntasObrigatorias.filter(p => 
        respostas.some(r => r.perguntaId === p.id)
      );
      
      if (respostasObrigatorias.length !== perguntasObrigatorias.length) {
        setErro('Todas as perguntas obrigatórias devem ser respondidas.');
        return;
      }
      
      // Preparar dados do voto
      const dadosVoto = {
        eleicaoId,
        respostas: respostas.reduce((acc, resposta) => {
          acc[resposta.perguntaId] = resposta.candidatoId;
          return acc;
        }, {} as Record<string, string>)
      };
      
      const response = await api.post(`/api/eleicoes/${eleicaoId}/votar`, dadosVoto);
      
      // Redirecionar para página de comprovante
      navigate(`/comprovante/${response.data.votoId}`);
      
    } catch (error: any) {
      console.error('Erro ao votar:', error);
      if (error.response?.status === 409) {
        setErro('Você já votou nesta eleição.');
      } else if (error.response?.status === 400) {
        setErro('Dados do voto inválidos. Verifique suas respostas.');
      } else {
        setErro('Erro ao registrar voto. Tente novamente.');
      }
    } finally {
      setVotando(false);
      setShowConfirmacao(false);
    }
  };

  if (loading) {
    return (
      <Container maxWidth="md" sx={{ py: 4, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Container>
    );
  }

  if (erro && !eleicao) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error" sx={{ mb: 2 }}>
          {erro}
        </Alert>
        <Button variant="outlined" onClick={() => navigate('/eleicoes')}>
          Voltar às Eleições
        </Button>
      </Container>
    );
  }

  if (!eleicao) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="warning">
          Eleição não encontrada.
        </Alert>
        <Button variant="outlined" onClick={() => navigate('/eleicoes')} sx={{ mt: 2 }}>
          Voltar às Eleições
        </Button>
      </Container>
    );
  }

  const perguntaAtual = eleicao.perguntas[etapaAtual];
  const respostaAtual = respostas.find(r => r.perguntaId === perguntaAtual?.id);

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        {/* Cabeçalho */}
        <Box sx={{ mb: 4, textAlign: 'center' }}>
          <VoteIcon sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
          <Typography variant="h4" component="h1" gutterBottom>
            {eleicao.titulo}
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
            {eleicao.descricao}
          </Typography>
          <Chip 
            label={`Pergunta ${etapaAtual + 1} de ${eleicao.perguntas.length}`}
            color="primary"
            variant="outlined"
          />
        </Box>

        {/* Stepper */}
        <Stepper activeStep={etapaAtual} sx={{ mb: 4 }}>
          {eleicao.perguntas.map((pergunta) => (
            <Step key={pergunta.id}>
              <StepLabel>
                {pergunta.titulo.length > 20 
                  ? `${pergunta.titulo.substring(0, 20)}...` 
                  : pergunta.titulo
                }
              </StepLabel>
            </Step>
          ))}
        </Stepper>

        {/* Pergunta Atual */}
        {perguntaAtual && (
          <Card sx={{ mb: 4 }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6" component="h2" sx={{ flexGrow: 1 }}>
                  {perguntaAtual.titulo}
                </Typography>
                {perguntaAtual.obrigatoria && (
                  <Chip label="Obrigatória" color="error" size="small" />
                )}
              </Box>
              
              {perguntaAtual.descricao && (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                  {perguntaAtual.descricao}
                </Typography>
              )}

              <FormControl component="fieldset" fullWidth>
                <FormLabel component="legend" sx={{ mb: 2 }}>
                  Selecione sua opção:
                </FormLabel>
                <RadioGroup
                  value={respostaAtual?.candidatoId || ''}
                  onChange={(e) => handleRespostaChange(perguntaAtual.id, e.target.value)}
                >
                  {perguntaAtual.candidatos.map((candidato) => (
                    <Card 
                      key={candidato.id} 
                      variant="outlined" 
                      sx={{ 
                        mb: 2, 
                        cursor: 'pointer',
                        '&:hover': { bgcolor: 'action.hover' },
                        bgcolor: respostaAtual?.candidatoId === candidato.id ? 'action.selected' : 'transparent'
                      }}
                      onClick={() => handleRespostaChange(perguntaAtual.id, candidato.id)}
                    >
                      <CardContent sx={{ py: 2 }}>
                        <FormControlLabel
                          value={candidato.id}
                          control={<Radio />}
                          label={
                            <Box>
                              <Typography variant="h6">
                                {candidato.numero} - {candidato.nome}
                              </Typography>
                              {candidato.partido && (
                                <Typography variant="body2" color="text.secondary">
                                  {candidato.partido}
                                </Typography>
                              )}
                              {candidato.proposta && (
                                <Typography variant="body2" sx={{ mt: 1 }}>
                                  {candidato.proposta}
                                </Typography>
                              )}
                            </Box>
                          }
                          sx={{ width: '100%', m: 0 }}
                        />
                      </CardContent>
                    </Card>
                  ))}
                </RadioGroup>
              </FormControl>
            </CardContent>
          </Card>
        )}

        {/* Erro */}
        {erro && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {erro}
          </Alert>
        )}

        {/* Botões de Navegação */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
          <Button
            variant="outlined"
            onClick={etapaAnterior}
            disabled={etapaAtual === 0}
          >
            Anterior
          </Button>
          
          <Box sx={{ display: 'flex', gap: 2 }}>
            <Button
              variant="outlined"
              onClick={() => navigate('/eleicoes')}
            >
              Cancelar
            </Button>
            
            <Button
              variant="contained"
              onClick={proximaEtapa}
              endIcon={etapaAtual === eleicao.perguntas.length - 1 ? <CheckIcon /> : undefined}
            >
              {etapaAtual === eleicao.perguntas.length - 1 ? 'Finalizar' : 'Próxima'}
            </Button>
          </Box>
        </Box>
      </Paper>

      {/* Dialog de Confirmação */}
      <Dialog open={showConfirmacao} onClose={() => setShowConfirmacao(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <WarningIcon color="warning" />
            Confirmar Voto
          </Box>
        </DialogTitle>
        <DialogContent>
          <Alert severity="info" sx={{ mb: 3 }}>
            <Typography variant="body2">
              <strong>Atenção:</strong> Após confirmar, seu voto não poderá ser alterado.
            </Typography>
          </Alert>
          
          <Typography variant="h6" gutterBottom>
            Resumo do seu voto:
          </Typography>
          
          {eleicao.perguntas.map((pergunta) => {
            const resposta = respostas.find(r => r.perguntaId === pergunta.id);
            const candidato = resposta ? pergunta.candidatos.find(c => c.id === resposta.candidatoId) : null;
            
            return (
              <Box key={pergunta.id} sx={{ mb: 2, p: 2, bgcolor: 'grey.50', borderRadius: 1 }}>
                <Typography variant="subtitle2" gutterBottom>
                  {pergunta.titulo}
                </Typography>
                <Typography variant="body2" color="primary.main">
                  {candidato ? `${candidato.numero} - ${candidato.nome}` : 'Não respondida'}
                </Typography>
              </Box>
            );
          })}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowConfirmacao(false)}>
            Revisar
          </Button>
          <Button 
            onClick={confirmarVoto} 
            variant="contained" 
            disabled={votando}
            startIcon={votando ? <CircularProgress size={20} /> : <CheckIcon />}
          >
            {votando ? 'Confirmando...' : 'Confirmar Voto'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default VotacaoPage;