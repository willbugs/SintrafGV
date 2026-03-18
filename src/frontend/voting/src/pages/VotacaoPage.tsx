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

  // Mapeia o formato da API (texto, opcoes) para o formato do componente (titulo, candidatos)
  const mapEleicaoFromApi = (data: any): Eleicao => {
    const rawPerguntas = data.perguntas ?? data.Perguntas ?? [];
    const perguntasArray = Array.isArray(rawPerguntas) ? rawPerguntas : [];
    return {
      id: data.id ?? data.Id ?? '',
      titulo: data.titulo ?? data.Titulo ?? '',
      descricao: data.descricao ?? data.Descricao ?? '',
      inicioVotacao: data.inicioVotacao ?? data.InicioVotacao ?? '',
      fimVotacao: data.fimVotacao ?? data.FimVotacao ?? '',
      status: data.status ?? data.Status ?? '',
      perguntas: perguntasArray.map((p: any, pi: number) => {
        const rawOpcoes = p.opcoes ?? p.Opcoes ?? [];
        const opcoesArray = Array.isArray(rawOpcoes) ? rawOpcoes : [];
        return {
          id: String(p.id ?? p.Id ?? `p-${pi}`),
          titulo: p.texto ?? p.Texto ?? p.titulo ?? '',
          descricao: p.descricao ?? p.Descricao,
          tipo: (p.tipo === 2 || p.tipo === 'MultiploVoto') ? 'MULTIPLA_ESCOLHA' : 'UNICA_ESCOLHA',
          obrigatoria: true,
          candidatos: opcoesArray.map((o: any, oi: number) => ({
            id: String(o.id ?? o.Id ?? `o-${pi}-${oi}`),
            nome: o.texto ?? o.Texto ?? o.nome ?? `Opção ${oi + 1}`,
            numero: String(o.ordem ?? o.Ordem ?? oi + 1),
            partido: o.descricao ?? o.Descricao,
            foto: o.foto ?? o.Foto,
            proposta: o.descricao ?? o.Descricao
          }))
        };
      })
    };
  };

  const carregarEleicao = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/api/eleicoes/${eleicaoId}`);
      setEleicao(mapEleicaoFromApi(response.data));
      
      // Verificar se a eleição está aberta (API pode retornar status 2 = Aberta ou string 'Aberta')
      const status = response.data.status ?? response.data.Status;
      const aberta = status === 'Aberta' || status === 2;
      if (!aberta) {
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
    const lista = eleicao?.perguntas ?? [];
    if (lista.length === 0) return;
    const atual = lista[etapaAtual];
    if (!atual) return;

    const resposta = respostas.find(r => r.perguntaId === atual.id);
    if (atual.obrigatoria && !resposta) {
      setErro('Esta pergunta é obrigatória. Selecione uma opção.');
      return;
    }

    setErro(null);
    if (etapaAtual < lista.length - 1) {
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
      
      // Preparar dados do voto (API espera respostas: [{ perguntaId, opcaoId, votoBranco }])
      const dadosVoto = {
        respostas: respostas.map(r => ({
          perguntaId: r.perguntaId,
          opcaoId: r.candidatoId,
          votoBranco: false
        }))
      };
      
      const response = await api.post(`/api/eleicoes/${eleicaoId}/votar`, dadosVoto);
      
      // Redirecionar para página de comprovante (API retorna votoId)
      const votoId = response.data.votoId ?? response.data.id;
      navigate(`/comprovante/${votoId}`);
      
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

  const perguntas = eleicao.perguntas ?? [];
  if (perguntas.length === 0) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="warning">
          Esta eleição não possui perguntas disponíveis para votação.
        </Alert>
        <Button variant="outlined" onClick={() => navigate('/eleicoes')} sx={{ mt: 2 }}>
          Voltar às Eleições
        </Button>
      </Container>
    );
  }

  const perguntaAtual = perguntas[etapaAtual];
  const respostaAtual = respostas.find(r => r.perguntaId === perguntaAtual?.id);

  if (!perguntaAtual) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="warning">
          Pergunta não encontrada.
        </Alert>
        <Button variant="outlined" onClick={() => navigate('/eleicoes')} sx={{ mt: 2 }}>
          Voltar às Eleições
        </Button>
      </Container>
    );
  }

  const candidatos = perguntaAtual.candidatos ?? [];
  if (candidatos.length === 0) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography variant="h5" gutterBottom>{eleicao.titulo}</Typography>
          <Typography variant="h6" color="text.secondary" sx={{ mt: 2 }}>{perguntaAtual.titulo}</Typography>
          <Alert severity="info" sx={{ mt: 3 }}>
            Nenhuma opção disponível para esta pergunta.
          </Alert>
          <Button variant="outlined" onClick={() => navigate('/eleicoes')} sx={{ mt: 3 }}>
            Voltar às Eleições
          </Button>
        </Paper>
      </Container>
    );
  }

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
            label={`Pergunta ${etapaAtual + 1} de ${perguntas.length}`}
            color="primary"
            variant="outlined"
          />
        </Box>

        {/* Stepper */}
        <Stepper activeStep={etapaAtual} sx={{ mb: 4 }}>
          {perguntas.map((pergunta) => (
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
                  {candidatos.map((candidato) => (
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
              endIcon={etapaAtual === perguntas.length - 1 ? <CheckIcon /> : undefined}
            >
              {etapaAtual === perguntas.length - 1 ? 'Finalizar' : 'Próxima'}
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
          
          {perguntas.map((pergunta) => {
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