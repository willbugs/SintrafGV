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
import { api } from '../services/api';

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

const RelatoriosVotacaoPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const tabFromUrl = parseInt(searchParams.get('tab') || '0', 10);
  const [tabValue, setTabValue] = useState(tabFromUrl);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [dadosParticipacao, setDadosParticipacao] = useState<any>(null);
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

  const carregarEnquetes = async () => {
    try {
      const response = await api.get('/api/eleicoes');
      setEnquetes(response.data.itens || []);
    } catch (err) {
      console.error('Erro ao carregar enquetes:', err);
    }
  };

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const construirFiltros = () => {
    const filtrosObj: any = {};
    if (filtros.enqueteId) filtrosObj.eleicaoId = filtros.enqueteId;
    if (filtros.dataInicio) filtrosObj.dataInicio = filtros.dataInicio;
    if (filtros.dataFim) filtrosObj.dataFim = filtros.dataFim;
    if (filtros.status) filtrosObj.status = filtros.status;
    if (filtros.tipo) filtrosObj.tipo = filtros.tipo;
    return filtrosObj;
  };

  const carregarRelatorioParticipacao = async () => {
    setLoading(true);
    setError(null);
    try {
      const dados = await relatorioService.obterRelatorioParticipacaoVotacao(construirFiltros());
      setDadosParticipacao(dados);
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
                onChange={(e) => setFiltros({ ...filtros, enqueteId: e.target.value })}
              >
                <MenuItem value="">Todas</MenuItem>
                {enquetes.map((enquete) => (
                  <MenuItem key={enquete.id} value={enquete.id}>
                    {enquete.titulo}
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
                <MenuItem value="2">Eleição</MenuItem>
              </Select>
            </FormControl>
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
              Análise do engajamento dos associados nas enquetes
            </Typography>
            <Button 
              variant="contained" 
              onClick={carregarRelatorioParticipacao}
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} /> : <Assessment />}
            >
              Gerar Relatório
            </Button>
          </Box>

          {dadosParticipacao && (
            <Box>
              <Grid container spacing={3} sx={{ mb: 3 }}>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosParticipacao.totalizadores?.TotalAssociados || 0}
                      </Typography>
                      <Typography variant="body2">Total de Associados</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosParticipacao.totalizadores?.TotalVotos || 0}
                      </Typography>
                      <Typography variant="body2">Total de Votos</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" color="primary">
                        {dadosParticipacao.totalizadores?.ParticipacaoMedia?.toFixed(2) || 0}%
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
                      <TableCell>Nome</TableCell>
                      <TableCell>CPF</TableCell>
                      <TableCell>Banco</TableCell>
                      <TableCell align="right">Enquetes Disponíveis</TableCell>
                      <TableCell align="right">Votos Realizados</TableCell>
                      <TableCell align="right">Participação (%)</TableCell>
                      <TableCell>Última Votação</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dadosParticipacao.dados?.map((item: any, index: number) => (
                      <TableRow key={index}>
                        <TableCell>{item.nome}</TableCell>
                        <TableCell>{item.cpf}</TableCell>
                        <TableCell>{item.nomeBanco}</TableCell>
                        <TableCell align="right">{item.totalEleicoesDisponiveis}</TableCell>
                        <TableCell align="right">{item.totalVotosRealizados}</TableCell>
                        <TableCell align="right">{item.percentualParticipacao?.toFixed(2)}%</TableCell>
                        <TableCell>
                          {item.ultimaVotacao ? new Date(item.ultimaVotacao).toLocaleDateString() : 'Nunca'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          )}
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Box sx={{ mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Resultados de Enquetes
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Detalhamento dos resultados por enquete
            </Typography>
            <Button 
              variant="contained" 
              onClick={carregarRelatorioResultados}
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} /> : <BarChart />}
            >
              Gerar Relatório
            </Button>
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
            <Button 
              variant="contained" 
              onClick={carregarRelatorioEngajamento}
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} /> : <TrendingUp />}
            >
              Gerar Relatório
            </Button>
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
