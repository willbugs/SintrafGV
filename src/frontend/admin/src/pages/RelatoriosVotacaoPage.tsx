import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Button,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { Assessment, BarChart, TrendingUp, FilterList } from '@mui/icons-material';
import relatorioService from '../services/relatorioService';
import { enquetesAPI } from '../services/api';
import ExportMenu from '../components/Relatorios/ExportMenu';

const STATUS_ENQUETE: Record<number, string> = {
  1: 'Rascunho',
  2: 'Aberta',
  3: 'Encerrada',
  4: 'Apurada',
  5: 'Cancelada',
};

function statusEnqueteLabel(status: number): string {
  return STATUS_ENQUETE[status] ?? '—';
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

interface Enquete {
  id: string;
  titulo: string;
  tipo: number;
  status: number;
}

function lerTotalizador(totalizadores: Record<string, unknown> | undefined, ...chaves: string[]): number | null {
  if (!totalizadores) return null;
  for (const chave of chaves) {
    const valor = totalizadores[chave];
    if (valor !== undefined && valor !== null) return Number(valor);
  }
  return null;
}

function linhasParticipacao(dadosParticipacao: any): Record<string, unknown>[] {
  const raw = dadosParticipacao?.dados ?? dadosParticipacao?.Dados;
  return Array.isArray(raw) ? raw : [];
}

function extrairResultadoPorOpcao(dadosParticipacao: any): { ordem: string[]; valores: Record<string, number> } {
  const tot = (dadosParticipacao?.totalizadores ?? dadosParticipacao?.Totalizadores ?? {}) as Record<string, unknown>;
  const raw = tot.resultadoPorOpcao ?? tot.ResultadoPorOpcao;
  const valores: Record<string, number> = {};

  if (raw && typeof raw === 'object') {
    for (const [chave, valor] of Object.entries(raw as Record<string, unknown>)) {
      valores[chave] = Number(valor) || 0;
    }
  }

  const ordemRaw = tot.ordemTotalizadores ?? tot.OrdemTotalizadores;
  let ordem: string[] = Array.isArray(ordemRaw)
    ? ordemRaw.map((x) => String(x))
    : Object.keys(valores);

  // Compatibilidade com API antiga (Sim/Não/Branco fixos)
  if (ordem.length === 0 && Object.keys(valores).length === 0) {
    const sim = Number(tot.votosSim ?? tot.VotosSim ?? 0);
    const nao = Number(tot.votosNao ?? tot.VotosNao ?? 0);
    const branco = Number(tot.votosBranco ?? tot.VotosBranco ?? 0);
    if (sim || nao || branco) {
      ordem = ['SIM', 'NÃO', 'Branco'];
      valores.SIM = sim;
      valores.NÃO = nao;
      valores.Branco = branco;
    }
  }

  return { ordem, valores };
}

function calcularTotaisParticipacao(dadosParticipacao: any) {
  const linhas = linhasParticipacao(dadosParticipacao);
  const tot = dadosParticipacao?.totalizadores as Record<string, unknown> | undefined;
  const { ordem: ordemOpcoes, valores: resultadoPorOpcao } = extrairResultadoPorOpcao(dadosParticipacao);

  const totalVotantes = lerTotalizador(tot, 'totalAssociados', 'TotalAssociados') ?? linhas.length;

  return { totalVotantes, ordemOpcoes, resultadoPorOpcao, linhas };
}

const RelatoriosVotacaoPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const tabFromUrl = Math.min(2, Math.max(0, parseInt(searchParams.get('tab') || '0', 10) || 0));
  const [tabValue, setTabValue] = useState(tabFromUrl);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [dadosParticipacao, setDadosParticipacao] = useState<any>(null);
  const [filtrosParticipacaoExport, setFiltrosParticipacaoExport] = useState<Record<string, string> | null>(null);
  const [dadosResultados, setDadosResultados] = useState<any>(null);
  const [dadosEngajamento, setDadosEngajamento] = useState<any>(null);
  const [enquetes, setEnquetes] = useState<Enquete[]>([]);
  
  const [filtros, setFiltros] = useState({
    enqueteId: '',
    dataInicio: '',
    dataFim: '',
    status: '',
    tipo: '',
  });

  useEffect(() => {
    carregarEnquetes();
    // Atualizar aba se mudou na URL
    setTabValue(tabFromUrl);
  }, [tabFromUrl]);

  const carregarEnquetes = async (filtrosParaListagem?: { dataInicio?: string; dataFim?: string; status?: string }) => {
    try {
      const f = filtrosParaListagem ?? filtros;
      const response = await enquetesAPI.listar(1, 500, {
        dataInicio: f.dataInicio || undefined,
        dataFim: f.dataFim || undefined,
        status: f.status ? Number(f.status) : undefined,
      });
      const raw = response?.itens ?? [];
      const itens: Enquete[] = raw.map((e: Record<string, unknown>) => ({
        id: String(e.id ?? e.Id ?? ''),
        titulo: String(e.titulo ?? e.Titulo ?? ''),
        tipo: Number(e.tipo ?? e.Tipo ?? 0),
        status: Number(e.status ?? e.Status ?? 0),
      }));
      setEnquetes(itens);
      if (filtros.enqueteId && !itens.some((e) => e.id === filtros.enqueteId)) {
        setFiltros((prev) => ({ ...prev, enqueteId: '' }));
      }
    } catch (err) {
      console.error('Erro ao carregar enquetes:', err);
      setEnquetes([]);
    }
  };

  const aplicarFiltrosDropdown = () => {
    carregarEnquetes();
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
    setSearchParams({ tab: String(newValue) }, { replace: true });
  };

  const construirFiltros = () => {
    const filtrosObj: any = {};
    if (filtros.enqueteId) filtrosObj.enqueteId = filtros.enqueteId;
    if (filtros.dataInicio) filtrosObj.dataInicio = filtros.dataInicio;
    if (filtros.dataFim) filtrosObj.dataFim = filtros.dataFim;
    if (filtros.status) filtrosObj.status = filtros.status;
    if (filtros.tipo) filtrosObj.tipo = filtros.tipo;
    return filtrosObj;
  };

  const carregarRelatorioParticipacao = async () => {
    if (!filtros.enqueteId) {
      setError('Selecione uma enquete para gerar o relatório de participação.');
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const filtrosRelatorio = construirFiltros();
      const dados = await relatorioService.obterRelatorioParticipacaoVotacao(filtrosRelatorio);
      setDadosParticipacao(dados);
      setFiltrosParticipacaoExport(filtrosRelatorio);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar relatório de participação');
    } finally {
      setLoading(false);
    }
  };

  const carregarRelatorioResultados = async () => {
    setLoading(true);
    setError(null);
    try {
      const dados = await relatorioService.obterRelatorioResultadosEleicao(construirFiltros());
      setDadosResultados(dados);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar relatório de resultados');
    } finally {
      setLoading(false);
    }
  };

  const carregarRelatorioEngajamento = async () => {
    setLoading(true);
    setError(null);
    try {
      const dados = await relatorioService.obterRelatorioEngajamentoVotacao(construirFiltros());
      setDadosEngajamento(dados);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erro ao carregar relatório de engajamento');
    } finally {
      setLoading(false);
    }
  };

  const limparFiltros = () => {
    setFiltros({
      enqueteId: '',
      dataInicio: '',
      dataFim: '',
      status: '',
      tipo: '',
    });
    setDadosParticipacao(null);
    setFiltrosParticipacaoExport(null);
    carregarEnquetes({ dataInicio: '', dataFim: '', status: '' });
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Relatórios de Votação
        </Typography>
        <Typography variant="subtitle1" color="text.secondary">
          Análises e métricas das enquetes e votações
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Filtros */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box display="flex" alignItems="center" mb={2}>
          <FilterList sx={{ mr: 1 }} />
          <Typography variant="h6">Filtros</Typography>
        </Box>
        <Grid container spacing={2}>
          <Grid item xs={12} md={3}>
            <FormControl fullWidth>
              <InputLabel>Enquete</InputLabel>
              <Select
                value={filtros.enqueteId}
                label="Enquete"
                onChange={(e) => {
                  setFiltros({ ...filtros, enqueteId: e.target.value });
                  setDadosParticipacao(null);
                  setFiltrosParticipacaoExport(null);
                }}
              >
                <MenuItem value="">Todas</MenuItem>
                {enquetes.map((enquete) => (
                  <MenuItem key={enquete.id} value={enquete.id}>
                    {enquete.titulo} – {statusEnqueteLabel(enquete.status)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <TextField
              fullWidth
              label="Data Início"
              type="date"
              value={filtros.dataInicio}
              onChange={(e) => setFiltros({ ...filtros, dataInicio: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <TextField
              fullWidth
              label="Data Fim"
              type="date"
              value={filtros.dataFim}
              onChange={(e) => setFiltros({ ...filtros, dataFim: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={filtros.status}
                label="Status"
                onChange={(e) => setFiltros({ ...filtros, status: e.target.value })}
              >
                <MenuItem value="">Todos</MenuItem>
                <MenuItem value="1">Rascunho</MenuItem>
                <MenuItem value="2">Aberta</MenuItem>
                <MenuItem value="3">Encerrada</MenuItem>
                <MenuItem value="4">Apurada</MenuItem>
                <MenuItem value="5">Cancelada</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={2}>
            <FormControl fullWidth>
              <InputLabel>Tipo</InputLabel>
              <Select
                value={filtros.tipo}
                label="Tipo"
                onChange={(e) => setFiltros({ ...filtros, tipo: e.target.value })}
              >
                <MenuItem value="">Todos</MenuItem>
                <MenuItem value="1">Enquete</MenuItem>
                <MenuItem value="2">Assembleia</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={1}>
            <Button
              fullWidth
              variant="contained"
              onClick={aplicarFiltrosDropdown}
              sx={{ height: '56px' }}
            >
              Aplicar filtros
            </Button>
          </Grid>
          <Grid item xs={12} md={1}>
            <Button
              fullWidth
              variant="outlined"
              onClick={limparFiltros}
              sx={{ height: '56px' }}
            >
              Limpar
            </Button>
          </Grid>
        </Grid>
      </Paper>

      <Card>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={handleTabChange} aria-label="relatórios de votação">
            <Tab 
              label="Participação" 
              icon={<Assessment />} 
              iconPosition="start"
            />
            <Tab 
              label="Resultados" 
              icon={<BarChart />} 
              iconPosition="start"
            />
            <Tab 
              label="Engajamento" 
              icon={<TrendingUp />} 
              iconPosition="start"
            />
          </Tabs>
        </Box>

        <TabPanel value={tabValue} index={0}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Participação em Votações
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Lista quem participou (sem expor a escolha individual). Totais por opção aparecem nos cards.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button 
                variant="contained" 
                onClick={carregarRelatorioParticipacao}
                disabled={loading}
                startIcon={loading ? <CircularProgress size={20} /> : <Assessment />}
              >
                Gerar Relatório
              </Button>
              <ExportMenu
                relatorioRequest={{
                  tipoRelatorio: 'participacao-votacao',
                  filtros: filtrosParticipacaoExport ?? {},
                  formatoExportacao: 'html',
                }}
                disabled={!dadosParticipacao || !filtrosParticipacaoExport}
                buttonVariant="outlined"
              />
            </Box>
          </Box>

          {dadosParticipacao && (() => {
            const { totalVotantes, ordemOpcoes, resultadoPorOpcao, linhas } = calcularTotaisParticipacao(dadosParticipacao);
            return (
            <Box>
              <Grid container spacing={3} sx={{ mb: 3 }}>
                <Grid item xs={12} md={3}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {totalVotantes}
                      </Typography>
                      <Typography variant="body2">Total de Votantes</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                {ordemOpcoes.map((opcao) => (
                  <Grid item xs={12} md={3} key={opcao}>
                    <Card variant="outlined">
                      <CardContent>
                        <Typography variant="h6" color="text.primary">
                          {resultadoPorOpcao[opcao] ?? 0}
                        </Typography>
                        <Typography variant="body2">{opcao}</Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>

              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Nome</TableCell>
                      <TableCell>CPF</TableCell>
                      <TableCell>Banco</TableCell>
                      <TableCell>Data/Hora Votação</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {linhas.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={4} align="center">
                          Nenhum votante encontrado. Gere o relatório novamente.
                        </TableCell>
                      </TableRow>
                    ) : (
                    linhas.map((item: any, index: number) => (
                      <TableRow key={index}>
                        <TableCell>{item.nome ?? item.Nome}</TableCell>
                        <TableCell>{item.cpf ?? item.Cpf}</TableCell>
                        <TableCell>{item.nomeBanco ?? item.NomeBanco}</TableCell>
                        <TableCell>
                          {(item.ultimaVotacao ?? item.UltimaVotacao)
                            ? new Date(item.ultimaVotacao ?? item.UltimaVotacao).toLocaleString('pt-BR', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit',
                              })
                            : 'Nunca'}
                        </TableCell>
                      </TableRow>
                    ))
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
            );
          })()}
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Resultados de Enquetes
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Detalhamento dos resultados por enquete
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button 
                variant="contained" 
                onClick={carregarRelatorioResultados}
                disabled={loading}
                startIcon={loading ? <CircularProgress size={20} /> : <BarChart />}
              >
                Gerar Relatório
              </Button>
              <ExportMenu
                relatorioRequest={{
                  tipoRelatorio: 'resultados-enquete',
                  filtros: construirFiltros(),
                  formatoExportacao: 'html',
                }}
                buttonVariant="outlined"
              />
            </Box>
          </Box>

          {dadosResultados && (
            <Box>
              <Grid container spacing={3} sx={{ mb: 3 }}>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosResultados.totalizadores?.TotalEleicoes || 0}
                      </Typography>
                      <Typography variant="body2">Total de Enquetes</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosResultados.totalizadores?.TotalVotosComputados || 0}
                      </Typography>
                      <Typography variant="body2">Votos Computados</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosResultados.totalizadores?.ParticipacaoMedia?.toFixed(2) || 0}%
                      </Typography>
                      <Typography variant="body2">Participação Média</Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Título</TableCell>
                      <TableCell>Data Início</TableCell>
                      <TableCell>Data Fim</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="right">Total Votos</TableCell>
                      <TableCell align="right">Participação (%)</TableCell>
                      <TableCell>Opção Mais Votada</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dadosResultados.dados?.map((item: any, index: number) => (
                      <TableRow key={index}>
                        <TableCell>{item.titulo}</TableCell>
                        <TableCell>{new Date(item.dataInicio).toLocaleDateString()}</TableCell>
                        <TableCell>{new Date(item.dataFim).toLocaleDateString()}</TableCell>
                        <TableCell>{item.status}</TableCell>
                        <TableCell align="right">{item.totalVotos}</TableCell>
                        <TableCell align="right">{item.percentualParticipacao?.toFixed(2)}%</TableCell>
                        <TableCell>{item.vencedor}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Engajamento em Votações
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Métricas de participação por período
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button 
                variant="contained" 
                onClick={carregarRelatorioEngajamento}
                disabled={loading}
                startIcon={loading ? <CircularProgress size={20} /> : <TrendingUp />}
              >
                Gerar Relatório
              </Button>
              <ExportMenu
                relatorioRequest={{
                  tipoRelatorio: 'engajamento-votacao',
                  filtros: construirFiltros(),
                  formatoExportacao: 'html',
                }}
                buttonVariant="outlined"
              />
            </Box>
          </Box>

          {dadosEngajamento && (
            <Box>
              <Grid container spacing={3} sx={{ mb: 3 }}>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosEngajamento.totalizadores?.TotalEleicoes || 0}
                      </Typography>
                      <Typography variant="body2">Enquetes Analisadas</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosEngajamento.totalizadores?.EngajamentoMedio?.toFixed(2) || 0}%
                      </Typography>
                      <Typography variant="body2">Engajamento Médio</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosEngajamento.totalizadores?.TotalVotosAnalisados || 0}
                      </Typography>
                      <Typography variant="body2">Votos Analisados</Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Enquete</TableCell>
                      <TableCell>Data Início</TableCell>
                      <TableCell>Data Fim</TableCell>
                      <TableCell align="right">Votos Computados</TableCell>
                      <TableCell align="right">Participação (%)</TableCell>
                      <TableCell align="right">Votos/Dia</TableCell>
                      <TableCell>Status</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dadosEngajamento.dados?.map((item: any, index: number) => (
                      <TableRow key={index}>
                        <TableCell>{item.tituloEleicao}</TableCell>
                        <TableCell>{new Date(item.dataInicio).toLocaleDateString()}</TableCell>
                        <TableCell>{new Date(item.dataFim).toLocaleDateString()}</TableCell>
                        <TableCell align="right">{item.totalVotosComputados}</TableCell>
                        <TableCell align="right">{item.percentualParticipacao?.toFixed(2)}%</TableCell>
                        <TableCell align="right">{item.votosPorDia}</TableCell>
                        <TableCell>{item.statusEleicao}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}
        </TabPanel>
      </Card>
    </Container>
  );
};

export default RelatoriosVotacaoPage;
