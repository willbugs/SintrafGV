import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  Grid,
  TextField,
  Button,
  Box,
  Alert,
  Card,
  CardContent,
  CardHeader,
  FormControlLabel,
  Switch,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions
} from '@mui/material';
import {
  Gavel as GavelIcon,
  Download as DownloadIcon,
  Security as SecurityIcon,
  Visibility as VisibilityIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
  Print as PrintIcon
} from '@mui/icons-material';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';

interface Eleicao {
  id: string;
  titulo: string;
  descricao: string;
  dataInicio: string;
  dataFim: string;
  status: string;
  totalVotos: number;
}

interface RelatorioCartorialRequest {
  eleicaoId: string;
  incluirDadosVotantes: boolean;
  incluirDadosTecnicos: boolean;
  cartorioDestino?: string;
  observacoes?: string;
}

interface RelatorioCartorial {
  dadosSindicato: {
    razaoSocial: string;
    cnpj: string;
    endereco: string;
    presidente: string;
  };
  dadosEleicao: {
    titulo: string;
    dataInicio: string;
    dataFim: string;
    pergunta: string;
  };
  resumo: {
    totalVotosComputados: number;
    percentualParticipacao: number;
    resultadoPorOpcao: Record<string, number>;
  };
  autenticacao: {
    numeroProtocolo: string;
    chaveValidacao: string;
    hashRelatorio: string;
    assinaturaDigitalRelatorio: string;
  };
}

const RelatorioCartorialPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(false);
  const [eleicoes, setEleicoes] = useState<Eleicao[]>([]);
  const [eleicaoSelecionada, setEleicaoSelecionada] = useState<string>('');
  const [relatorioRequest, setRelatorioRequest] = useState<RelatorioCartorialRequest>({
    eleicaoId: '',
    incluirDadosVotantes: true,
    incluirDadosTecnicos: true,
    cartorioDestino: '',
    observacoes: ''
  });
  const [relatorio, setRelatorio] = useState<RelatorioCartorial | null>(null);
  const [dialogPreview, setDialogPreview] = useState(false);
  const [integridadeValidada, setIntegridadeValidada] = useState<boolean | null>(null);

  useEffect(() => {
    carregarEleicoes();
  }, []);

  const carregarEleicoes = async () => {
    try {
      setLoading(true);
      const response = await api.get('/api/eleicoes');
      const eleicoes = response.data.itens || [];
      setEleicoes(eleicoes.filter((e: Eleicao) => e.status === 'Encerrada' || e.status === 'Apurada'));
    } catch (error) {
      console.error('Erro ao carregar enquetes:', error);
      showToast('Erro ao carregar enquetes', 'error');
    } finally {
      setLoading(false);
    }
  };

  const validarIntegridade = async (eleicaoId: string) => {
    try {
      setLoading(true);
      const response = await api.get(`/api/relatorio-cartorial/validar-integridade/${eleicaoId}`);
      setIntegridadeValidada(response.data.integridadeValida);
      
      if (response.data.integridadeValida) {
        showToast('Integridade dos dados validada com sucesso!', 'success');
      } else {
        showToast('Problemas de integridade detectados nos dados!', 'error');
      }
    } catch (error) {
      console.error('Erro ao validar integridade:', error);
      showToast('Erro ao validar integridade dos dados', 'error');
      setIntegridadeValidada(false);
    } finally {
      setLoading(false);
    }
  };

  const gerarRelatorio = async () => {
    if (!eleicaoSelecionada) {
      showToast('Selecione uma eleição', 'warning');
      return;
    }

    try {
      setLoading(true);
      
      const request = {
        ...relatorioRequest,
        eleicaoId: eleicaoSelecionada
      };

      const response = await api.post('/api/relatorio-cartorial/gerar', request);
      setRelatorio(response.data);
      setDialogPreview(true);
      showToast('Relatório gerado com sucesso!', 'success');
    } catch (error) {
      console.error('Erro ao gerar relatório:', error);
      showToast('Erro ao gerar relatório cartorial', 'error');
    } finally {
      setLoading(false);
    }
  };

  const baixarRelatorioPDF = async () => {
    if (!eleicaoSelecionada) {
      showToast('Selecione uma eleição', 'warning');
      return;
    }

    try {
      setLoading(true);
      
      const request = {
        ...relatorioRequest,
        eleicaoId: eleicaoSelecionada
      };

      const response = await api.post('/api/relatorio-cartorial/gerar-pdf', request, {
        responseType: 'blob'
      });

      const blob = new Blob([response.data], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = `relatorio_cartorial_${eleicaoSelecionada}_${new Date().toISOString().slice(0, 10)}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      
      showToast('Relatório PDF baixado com sucesso!', 'success');
    } catch (error) {
      console.error('Erro ao baixar PDF:', error);
      showToast('Erro ao baixar relatório em PDF', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleEleicaoChange = (eleicaoId: string) => {
    setEleicaoSelecionada(eleicaoId);
    setRelatorioRequest(prev => ({ ...prev, eleicaoId }));
    setIntegridadeValidada(null);
    setRelatorio(null);
    
    if (eleicaoId) {
      validarIntegridade(eleicaoId);
    }
  };

  const eleicaoAtual = eleicoes.find(e => e.id === eleicaoSelecionada);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper sx={{ p: 3 }}>
        <Box display="flex" alignItems="center" mb={3}>
          <GavelIcon sx={{ mr: 2, fontSize: 32, color: 'primary.main' }} />
          <Typography variant="h4" component="h1">
            Relatórios Cartoriais
          </Typography>
        </Box>

        <Alert severity="info" sx={{ mb: 3 }}>
          <Typography variant="body2">
            Gere relatórios oficiais de votação com dados detalhados de cada voto e timestamps precisos.
            Os relatórios incluem hash de validação para garantir integridade dos dados.
          </Typography>
        </Alert>

        <Grid container spacing={3}>
          {/* Seleção de Eleição */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                title="Selecionar Enquete"
                subheader="Escolha a enquete para gerar o relatório cartorial"
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={8}>
                    <TextField
                      select
                      fullWidth
                      label="Enquete"
                      value={eleicaoSelecionada}
                      onChange={(e) => handleEleicaoChange(e.target.value)}
                      SelectProps={{
                        native: true,
                      }}
                    >
                      <option value="">Selecione uma enquete...</option>
                      {eleicoes.map((eleicao) => (
                        <option key={eleicao.id} value={eleicao.id}>
                          {eleicao.titulo} - {new Date(eleicao.dataFim).toLocaleDateString()}
                        </option>
                      ))}
                    </TextField>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    {eleicaoSelecionada && (
                      <Box display="flex" alignItems="center" gap={1}>
                        <Chip
                          icon={integridadeValidada === true ? <CheckCircleIcon /> : 
                                integridadeValidada === false ? <ErrorIcon /> : <InfoIcon />}
                          label={integridadeValidada === true ? 'Íntegro' : 
                                 integridadeValidada === false ? 'Problemas' : 'Verificando...'}
                          color={integridadeValidada === true ? 'success' : 
                                 integridadeValidada === false ? 'error' : 'default'}
                          variant="outlined"
                        />
                      </Box>
                    )}
                  </Grid>
                </Grid>

                {eleicaoAtual && (
                  <Box mt={2} p={2} bgcolor="grey.50" borderRadius={1}>
                    <Typography variant="subtitle2" gutterBottom>
                      Detalhes da Enquete:
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Título:</strong> {eleicaoAtual.titulo}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Período:</strong> {new Date(eleicaoAtual.dataInicio).toLocaleDateString()} 
                      até {new Date(eleicaoAtual.dataFim).toLocaleDateString()}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Total de Votos:</strong> {eleicaoAtual.totalVotos}
                    </Typography>
                  </Box>
                )}
              </CardContent>
            </Card>
          </Grid>

          {/* Configurações do Relatório */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                avatar={<SecurityIcon color="primary" />}
                title="Configurações do Relatório"
                subheader="Defina o nível de detalhamento e segurança"
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={relatorioRequest.incluirDadosVotantes}
                          onChange={(e) => setRelatorioRequest(prev => ({
                            ...prev,
                            incluirDadosVotantes: e.target.checked
                          }))}
                        />
                      }
                      label="Incluir Dados dos Votantes"
                    />
                    <Typography variant="caption" display="block" color="text.secondary">
                      Nome, CPF e matrícula dos votantes
                    </Typography>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <FormControlLabel
                      control={
                        <Switch
                          checked={relatorioRequest.incluirDadosTecnicos}
                          onChange={(e) => setRelatorioRequest(prev => ({
                            ...prev,
                            incluirDadosTecnicos: e.target.checked
                          }))}
                        />
                      }
                      label="Incluir Dados Técnicos"
                    />
                    <Typography variant="caption" display="block" color="text.secondary">
                      IP, User Agent e dados do dispositivo
                    </Typography>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Cartório de Destino"
                      value={relatorioRequest.cartorioDestino || ''}
                      onChange={(e) => setRelatorioRequest(prev => ({
                        ...prev,
                        cartorioDestino: e.target.value
                      }))}
                      placeholder="Nome do cartório que receberá o relatório"
                    />
                  </Grid>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Observações"
                      value={relatorioRequest.observacoes || ''}
                      onChange={(e) => setRelatorioRequest(prev => ({
                        ...prev,
                        observacoes: e.target.value
                      }))}
                      multiline
                      rows={2}
                      placeholder="Observações adicionais para o relatório..."
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Ações */}
          <Grid item xs={12}>
            <Box display="flex" justifyContent="center" gap={2}>
              <Button
                variant="outlined"
                startIcon={<VisibilityIcon />}
                onClick={gerarRelatorio}
                disabled={!eleicaoSelecionada || loading || integridadeValidada === false}
              >
                Visualizar Relatório
              </Button>
              <Button
                variant="contained"
                startIcon={<DownloadIcon />}
                onClick={baixarRelatorioPDF}
                disabled={!eleicaoSelecionada || loading || integridadeValidada === false}
              >
                {loading ? 'Gerando...' : 'Baixar PDF'}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Dialog de Preview do Relatório */}
      <Dialog
        open={dialogPreview}
        onClose={() => setDialogPreview(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center">
            <GavelIcon sx={{ mr: 1 }} />
            Relatório Cartorial - Preview
          </Box>
        </DialogTitle>
        <DialogContent>
          {relatorio && (
            <Box>
              <Alert severity="success" sx={{ mb: 2 }}>
                Relatório gerado com sucesso! Protocolo: {relatorio.autenticacao.numeroProtocolo}
              </Alert>
              
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Dados do Sindicato</Typography>
                  <Typography variant="body2"><strong>Razão Social:</strong> {relatorio.dadosSindicato.razaoSocial}</Typography>
                  <Typography variant="body2"><strong>CNPJ:</strong> {relatorio.dadosSindicato.cnpj}</Typography>
                  <Typography variant="body2"><strong>Presidente:</strong> {relatorio.dadosSindicato.presidente}</Typography>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Dados da Eleição</Typography>
                  <Typography variant="body2"><strong>Título:</strong> {relatorio.dadosEleicao.titulo}</Typography>
                  <Typography variant="body2"><strong>Período:</strong> {new Date(relatorio.dadosEleicao.dataInicio).toLocaleDateString()} - {new Date(relatorio.dadosEleicao.dataFim).toLocaleDateString()}</Typography>
                  <Typography variant="body2"><strong>Pergunta:</strong> {relatorio.dadosEleicao.pergunta}</Typography>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>Resumo da Votação</Typography>
                  <Typography variant="body2"><strong>Total de Votos:</strong> {relatorio.resumo.totalVotosComputados}</Typography>
                  <Typography variant="body2"><strong>Participação:</strong> {relatorio.resumo.percentualParticipacao.toFixed(2)}%</Typography>
                  
                  <Box mt={1}>
                    <Typography variant="subtitle2">Resultados:</Typography>
                    {Object.entries(relatorio.resumo.resultadoPorOpcao).map(([opcao, votos]) => (
                      <Typography key={opcao} variant="body2">
                        • {opcao}: {votos} votos
                      </Typography>
                    ))}
                  </Box>
                </Grid>
                
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom>Dados de Autenticação</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                    <strong>Hash:</strong> {relatorio.autenticacao.hashRelatorio}
                  </Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                    <strong>Chave de Validação:</strong> {relatorio.autenticacao.chaveValidacao}
                  </Typography>
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogPreview(false)}>
            Fechar
          </Button>
          <Button
            variant="contained"
            startIcon={<PrintIcon />}
            onClick={baixarRelatorioPDF}
          >
            Baixar PDF
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default RelatorioCartorialPage;