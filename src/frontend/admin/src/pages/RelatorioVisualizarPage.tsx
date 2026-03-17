import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  CircularProgress,
  Alert,
  Chip,
  Stack,
  TablePagination,
  IconButton,
  Tooltip,
  Card,
  CardContent,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Autocomplete,
} from '@mui/material';
import {
  ArrowBack,
  Refresh,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material';
import relatorioService from '../services/relatorioService';
import type {
  RelatorioRequest,
  RelatorioResponse,
  AssociadoRelatorio,
  CampoRelatorio,
} from '../services/relatorioService';
import ExportMenu from '../components/Relatorios/ExportMenu';

interface ColunasVisibilidade {
  [key: string]: boolean;
}

const RelatorioVisualizarPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const [dados, setDados] = useState<RelatorioResponse<AssociadoRelatorio> | null>(null);
  const [campos, setCampos] = useState<CampoRelatorio[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [colunasVisiveis, setColunasVisiveis] = useState<ColunasVisibilidade>({});
  const [pagina, setPagina] = useState(0);
  const [tamanhoPagina, setTamanhoPagina] = useState(25);
  /** Aniversariantes: mês (1–12) e dia opcional */
  const [mesAniversariantes, setMesAniversariantes] = useState(() => new Date().getMonth() + 1);
  const [diaAniversariantes, setDiaAniversariantes] = useState<number | null>(null);
  /** Por cidade: cidade selecionada ('' = todas) */
  const [cidadePorCidade, setCidadePorCidade] = useState('');
  /** Por banco: banco selecionado ('' = todos) */
  const [bancoPorBanco, setBancoPorBanco] = useState('');
  /** Por sexo: M, F ou '' (todos) */
  const [sexoPorSexo, setSexoPorSexo] = useState('');
  /** Novos associados: período */
  const [dataInicioNovos, setDataInicioNovos] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 1);
    return d.toISOString().slice(0, 10);
  });
  const [dataFimNovos, setDataFimNovos] = useState(() => new Date().toISOString().slice(0, 10));
  /** Situação (Todos / Ativos / Inativos) – usado em todos os relatórios de associados */
  const [situacaoFilter, setSituacaoFilter] = useState<'todos' | 'ativos' | 'inativos'>('todos');
  /** Associados em período: data início e fim */
  const [dataInicioEmPeriodo, setDataInicioEmPeriodo] = useState(() => {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 1);
    return d.toISOString().slice(0, 10);
  });
  const [dataFimEmPeriodo, setDataFimEmPeriodo] = useState(() => new Date().toISOString().slice(0, 10));
  /** Opções para dropdowns (por cidade / por banco) */
  const [opcoesCidades, setOpcoesCidades] = useState<string[]>([]);
  const [opcoesBancos, setOpcoesBancos] = useState<string[]>([]);
  const [loadingCidades, setLoadingCidades] = useState(false);
  const [loadingBancos, setLoadingBancos] = useState(false);
  const [erroOpcoesCidades, setErroOpcoesCidades] = useState<string | null>(null);
  const [erroOpcoesBancos, setErroOpcoesBancos] = useState<string | null>(null);

  const tipoRelatorio = searchParams.get('tipo') || 'associados-geral';

  useEffect(() => {
    carregarRelatorio();
  }, [tipoRelatorio, pagina, tamanhoPagina, mesAniversariantes, diaAniversariantes, cidadePorCidade, bancoPorBanco, sexoPorSexo, dataInicioNovos, dataFimNovos, situacaoFilter, dataInicioEmPeriodo, dataFimEmPeriodo]);

  useEffect(() => {
    carregarCampos();
  }, [tipoRelatorio]);

  useEffect(() => {
    if (tipoRelatorio === 'por-cidade') {
      setErroOpcoesCidades(null);
      setLoadingCidades(true);
      relatorioService
        .obterCidadesParaFiltro()
        .then((lista) => setOpcoesCidades(Array.isArray(lista) ? lista : []))
        .catch(() => {
          setOpcoesCidades([]);
          setErroOpcoesCidades('Não foi possível carregar a lista. Você pode digitar o nome da cidade abaixo.');
        })
        .finally(() => setLoadingCidades(false));
    }
  }, [tipoRelatorio]);

  useEffect(() => {
    if (tipoRelatorio === 'por-banco') {
      setErroOpcoesBancos(null);
      setLoadingBancos(true);
      relatorioService
        .obterBancosParaFiltro()
        .then((lista) => setOpcoesBancos(Array.isArray(lista) ? lista : []))
        .catch(() => {
          setOpcoesBancos([]);
          setErroOpcoesBancos('Não foi possível carregar a lista. Você pode digitar o nome do banco abaixo.');
        })
        .finally(() => setLoadingBancos(false));
    }
  }, [tipoRelatorio]);

  const carregarCampos = async () => {
    try {
      const camposDisponiveis = await relatorioService.obterCamposDisponiveis(tipoRelatorio);
      setCampos(camposDisponiveis);
      
      // Inicializar colunas visíveis (primeiras 8 colunas visíveis por padrão)
      const visibilidade: ColunasVisibilidade = {};
      camposDisponiveis.forEach((campo, index) => {
        visibilidade[campo.nome] = index < 8;
      });
      setColunasVisiveis(visibilidade);
    } catch (err) {
      console.error('Erro ao carregar campos:', err);
    }
  };

  const carregarRelatorio = async () => {
    try {
      setLoading(true);
      setError(null);

      const request: RelatorioRequest = {
        tipoRelatorio,
        filtros: construirFiltros(),
        formatoExportacao: 'html',
        pagina: pagina + 1,
        tamanhoPagina,
      };

      let resultado: RelatorioResponse<AssociadoRelatorio>;

      switch (tipoRelatorio) {
        case 'associados-ativos':
          resultado = await relatorioService.obterRelatorioAssociadosAtivos(request);
          break;
        case 'associados-inativos':
          resultado = await relatorioService.obterRelatorioAssociadosInativos(request);
          break;
        case 'aniversariantes':
          resultado = await relatorioService.obterRelatorioAniversariantes(request);
          break;
        case 'novos-associados':
          resultado = await relatorioService.obterRelatorioNovosAssociados(request);
          break;
        case 'por-sexo':
          resultado = await relatorioService.obterRelatorioPorSexo(request);
          break;
        case 'por-banco':
          resultado = await relatorioService.obterRelatorioPorBanco(request);
          break;
        case 'por-cidade':
          resultado = await relatorioService.obterRelatorioPorCidade(request);
          break;
        case 'em-periodo':
          resultado = await relatorioService.obterRelatorioAssociadosEmPeriodo(request);
          break;
        default:
          resultado = await relatorioService.obterRelatorioAssociadosGeral(request);
      }

      setDados(resultado);

      // Fallback: preencher opções de cidade/banco a partir dos dados do relatório quando a API retornar vazio
      if (tipoRelatorio === 'por-cidade' && resultado?.dados?.length) {
        const cidadesDoRelatorio = [
          ...new Set(
            resultado.dados
              .map((r) => (r as AssociadoRelatorio).cidade)
              .filter((c): c is string => c != null && String(c).trim() !== '')
              .map((c) => String(c).trim())
          ),
        ];
        if (cidadesDoRelatorio.length) {
          setOpcoesCidades((prev) => {
            const unicas = new Set([...prev, ...cidadesDoRelatorio]);
            return Array.from(unicas).sort((a, b) => a.localeCompare(b, 'pt-BR'));
          });
        }
      }
      if (tipoRelatorio === 'por-banco' && resultado?.dados?.length) {
        const bancosDoRelatorio = [
          ...new Set(
            resultado.dados
              .map((r) => (r as AssociadoRelatorio).nomeBanco)
              .filter((b): b is string => b != null && String(b).trim() !== '')
              .map((b) => String(b).trim())
          ),
        ];
        if (bancosDoRelatorio.length) {
          setOpcoesBancos((prev) => {
            const unicos = new Set([...prev, ...bancosDoRelatorio]);
            return Array.from(unicos).sort((a, b) => a.localeCompare(b, 'pt-BR'));
          });
        }
      }
    } catch (err) {
      console.error('Erro ao carregar relatório:', err);
      setError('Erro ao carregar relatório. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  /** Tipos de relatório que usam filtro Situação (Todos/Ativos/Inativos). Ativos/Inativos não entram pois já são fixos. */
  const relatoriosComSituacao = [
    'associados-geral', 'aniversariantes', 'novos-associados', 'por-sexo', 'por-banco', 'por-cidade', 'em-periodo'
  ];

  /** Monta o objeto de filtros enviado à API (só filtros específicos do relatório). */
  const construirFiltros = (): Record<string, unknown> => {
    const filtrosObj: Record<string, unknown> = {};
    if (relatoriosComSituacao.includes(tipoRelatorio)) {
      filtrosObj.situacao = situacaoFilter;
    }
    if (tipoRelatorio === 'aniversariantes') {
      filtrosObj.mes = mesAniversariantes;
      if (diaAniversariantes != null) filtrosObj.dia = diaAniversariantes;
    } else if (tipoRelatorio === 'por-cidade' && cidadePorCidade) {
      filtrosObj.cidade = cidadePorCidade;
    } else if (tipoRelatorio === 'por-banco' && bancoPorBanco) {
      filtrosObj.nomeBanco = bancoPorBanco;
    } else if (tipoRelatorio === 'por-sexo' && sexoPorSexo) {
      filtrosObj.sexo = sexoPorSexo;
    } else if (tipoRelatorio === 'novos-associados') {
      filtrosObj.dataInicio = dataInicioNovos;
      filtrosObj.dataFim = dataFimNovos;
    } else if (tipoRelatorio === 'em-periodo') {
      filtrosObj.dataInicio = dataInicioEmPeriodo;
      filtrosObj.dataFim = dataFimEmPeriodo;
    }
    return filtrosObj;
  };

  const toggleColunaVisibilidade = (nomeCampo: string) => {
    setColunasVisiveis(prev => ({
      ...prev,
      [nomeCampo]: !prev[nomeCampo]
    }));
  };

  const formatarValor = (valor: any, campo: CampoRelatorio) => {
    if (valor === null || valor === undefined) return '-';

    switch (campo.tipo) {
      case 'date':
        return new Date(valor).toLocaleDateString('pt-BR');
      case 'boolean':
        return valor ? 'Sim' : 'Não';
      case 'number':
        return valor.toLocaleString('pt-BR');
      default:
        return valor.toString();
    }
  };

  const aplicarMascara = (valor: string, mascara?: string) => {
    if (!mascara || !valor) return valor;
    
    // Implementação básica de máscaras
    if (mascara === '000.000.000-00') {
      return valor.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
    if (mascara === '(00) 00000-0000') {
      return valor.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    }
    
    return valor;
  };

  if (error) {
    return (
      <Box>
        <Button
          startIcon={<ArrowBack />}
          onClick={() => navigate('/relatorios')}
          sx={{ mb: 2 }}
        >
          Voltar aos Relatórios
        </Button>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  const camposVisiveis = campos.filter(campo => colunasVisiveis[campo.nome]);
  const titulo = dados?.metadata?.titulo ?? 'Relatório';
  const subtitulo = dados?.metadata?.subtitulo;
  const totalRegistros = dados?.metadata?.totalRegistros ?? 0;
  const dadosLength = dados?.dados?.length ?? 0;
  const dataGeracao = dados?.metadata?.dataGeracao;
  const totalizadores = dados?.totalizadores ?? {};

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Button
            startIcon={<ArrowBack />}
            onClick={() => navigate('/relatorios')}
            sx={{ mb: 1 }}
          >
            Voltar aos Relatórios
          </Button>
          <Typography variant="h4" fontWeight="bold">
            {titulo}
          </Typography>
          {subtitulo && (
            <Typography variant="subtitle1" color="text.secondary">
              {subtitulo}
            </Typography>
          )}
        </Box>
        <Stack direction="row" spacing={1}>
          <Tooltip title="Atualizar dados">
            <IconButton onClick={carregarRelatorio}>
              <Refresh />
            </IconButton>
          </Tooltip>
          <ExportMenu
            relatorioRequest={{
              tipoRelatorio,
              filtros: construirFiltros(),
              formatoExportacao: 'html',
            }}
          />
        </Stack>
      </Box>

      {/* Informações do Relatório */}
      <Grid container spacing={2} mb={3}>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Total de Registros
              </Typography>
              <Typography variant="h5" fontWeight="bold">
                {totalRegistros.toLocaleString('pt-BR')}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Registros Exibidos
              </Typography>
              <Typography variant="h5" fontWeight="bold">
                {dadosLength.toLocaleString('pt-BR')}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Data de Geração
              </Typography>
              <Typography variant="h6">
                {dataGeracao ? new Date(dataGeracao).toLocaleString('pt-BR') : '-'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                {Object.entries(totalizadores).map(([chave, valor]) => (
                  <Chip
                    key={chave}
                    label={`${chave}: ${valor}`}
                    size="small"
                    variant="outlined"
                  />
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filtro Situação (Todos / Ativos / Inativos) – todos os relatórios de associados */}
      {relatoriosComSituacao.includes(tipoRelatorio) && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Situação
          </Typography>
          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel>Situação</InputLabel>
            <Select
              value={situacaoFilter}
              label="Situação"
              onChange={(e) => { setSituacaoFilter(e.target.value as 'todos' | 'ativos' | 'inativos'); setPagina(0); }}
            >
              <MenuItem value="todos">Todos</MenuItem>
              <MenuItem value="ativos">Ativos</MenuItem>
              <MenuItem value="inativos">Inativos</MenuItem>
            </Select>
          </FormControl>
        </Paper>
      )}

      {/* Filtro específico: Associados em período (data início / fim) */}
      {tipoRelatorio === 'em-periodo' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Período em que a pessoa era associada (DataFiliação e DataDesligamento)
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
            <TextField
              size="small"
              label="Data início"
              type="date"
              value={dataInicioEmPeriodo}
              onChange={(e) => { setDataInicioEmPeriodo(e.target.value); setPagina(0); }}
              InputLabelProps={{ shrink: true }}
              sx={{ minWidth: 160 }}
            />
            <TextField
              size="small"
              label="Data fim"
              type="date"
              value={dataFimEmPeriodo}
              onChange={(e) => { setDataFimEmPeriodo(e.target.value); setPagina(0); }}
              InputLabelProps={{ shrink: true }}
              sx={{ minWidth: 160 }}
            />
          </Stack>
        </Paper>
      )}

      {/* Filtro específico: Aniversariantes (Mês + Dia) */}
      {tipoRelatorio === 'aniversariantes' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Aniversariantes: escolha o mês e, se quiser, o dia
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
            <FormControl size="small" sx={{ minWidth: 160 }}>
              <InputLabel>Mês</InputLabel>
              <Select
                value={mesAniversariantes}
                label="Mês"
                onChange={(e) => { setMesAniversariantes(Number(e.target.value)); setPagina(0); }}
              >
                {[
                  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
                  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
                ].map((nome, i) => (
                  <MenuItem key={i} value={i + 1}>{nome}</MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Dia (opcional)</InputLabel>
              <Select
                value={diaAniversariantes ?? ''}
                label="Dia (opcional)"
                onChange={(e) => {
                  const v = e.target.value;
                  setDiaAniversariantes(v === '' ? null : Number(v));
                  setPagina(0);
                }}
              >
                <MenuItem value="">Todos</MenuItem>
                {Array.from({ length: 31 }, (_, i) => i + 1).map((d) => (
                  <MenuItem key={d} value={d}>{d}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </Paper>
      )}

      {/* Filtro específico: Por cidade (Autocomplete = digitar para buscar) */}
      {tipoRelatorio === 'por-cidade' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Filtrar por cidade: digite para buscar ou escolha na lista
          </Typography>
          <Autocomplete
            size="small"
            options={opcoesCidades}
            value={cidadePorCidade || null}
            onChange={(_, newValue) => {
              setCidadePorCidade(typeof newValue === 'string' ? newValue : newValue ?? '');
              setPagina(0);
            }}
            getOptionLabel={(option) => (option && String(option).trim()) ? String(option) : 'Todas as cidades'}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Cidade"
                placeholder="Todas ou digite/escolha uma cidade"
                helperText={erroOpcoesCidades ?? undefined}
                error={!!erroOpcoesCidades}
                InputProps={{
                  ...params.InputProps,
                  endAdornment: loadingCidades ? (
                    <>
                      <CircularProgress size={20} sx={{ mr: 1 }} />
                      {params.InputProps.endAdornment}
                    </>
                  ) : (
                    params.InputProps.endAdornment
                  ),
                }}
              />
            )}
            sx={{ minWidth: 280, maxWidth: 400 }}
            freeSolo
            loading={loadingCidades}
          />
        </Paper>
      )}

      {/* Filtro específico: Por banco */}
      {tipoRelatorio === 'por-banco' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Filtrar por banco: digite para buscar ou escolha na lista
          </Typography>
          <Autocomplete
            size="small"
            options={opcoesBancos}
            value={bancoPorBanco || null}
            onChange={(_, newValue) => {
              setBancoPorBanco(typeof newValue === 'string' ? newValue : newValue ?? '');
              setPagina(0);
            }}
            getOptionLabel={(option) => (option && String(option).trim()) ? String(option) : 'Todos os bancos'}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Banco"
                placeholder="Todos ou digite/escolha um banco"
                helperText={erroOpcoesBancos ?? undefined}
                error={!!erroOpcoesBancos}
                InputProps={{
                  ...params.InputProps,
                  endAdornment: loadingBancos ? (
                    <>
                      <CircularProgress size={20} sx={{ mr: 1 }} />
                      {params.InputProps.endAdornment}
                    </>
                  ) : (
                    params.InputProps.endAdornment
                  ),
                }}
              />
            )}
            sx={{ minWidth: 280, maxWidth: 400 }}
            freeSolo
            loading={loadingBancos}
          />
        </Paper>
      )}

      {/* Filtro específico: Por sexo */}
      {tipoRelatorio === 'por-sexo' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Filtrar por sexo
          </Typography>
          <FormControl size="small" sx={{ minWidth: 160 }}>
            <InputLabel>Sexo</InputLabel>
            <Select
              value={sexoPorSexo}
              label="Sexo"
              onChange={(e) => { setSexoPorSexo(e.target.value); setPagina(0); }}
            >
              <MenuItem value="">Todos</MenuItem>
              <MenuItem value="M">Masculino</MenuItem>
              <MenuItem value="F">Feminino</MenuItem>
            </Select>
          </FormControl>
        </Paper>
      )}

      {/* Filtro específico: Novos associados (período) */}
      {tipoRelatorio === 'novos-associados' && (
        <Paper sx={{ p: 2, mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Período de filiação
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
            <TextField
              size="small"
              label="Data início"
              type="date"
              value={dataInicioNovos}
              onChange={(e) => { setDataInicioNovos(e.target.value); setPagina(0); }}
              InputLabelProps={{ shrink: true }}
              sx={{ minWidth: 160 }}
            />
            <TextField
              size="small"
              label="Data fim"
              type="date"
              value={dataFimNovos}
              onChange={(e) => { setDataFimNovos(e.target.value); setPagina(0); }}
              InputLabelProps={{ shrink: true }}
              sx={{ minWidth: 160 }}
            />
          </Stack>
        </Paper>
      )}

      {/* Controle de Colunas */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Colunas Visíveis:
        </Typography>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          {campos.map((campo) => (
            <Chip
              key={campo.nome}
              label={campo.titulo}
              variant={colunasVisiveis[campo.nome] ? "filled" : "outlined"}
              onClick={() => toggleColunaVisibilidade(campo.nome)}
              icon={colunasVisiveis[campo.nome] ? <Visibility /> : <VisibilityOff />}
              size="small"
            />
          ))}
        </Stack>
      </Paper>

      {/* Tabela de Dados — loading apenas nesta área (overlay) */}
      <Paper sx={{ width: '100%', overflow: 'hidden', position: 'relative', minHeight: 320 }}>
        {loading && (
          <Box
            sx={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              bottom: 0,
              minHeight: 280,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: 'rgba(255,255,255,0.85)',
              zIndex: 10,
            }}
          >
            <CircularProgress />
          </Box>
        )}
        <TableContainer sx={{ maxHeight: 600 }}>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {camposVisiveis.map((campo) => (
                  <TableCell key={campo.nome}>
                    {campo.titulo}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {!dados && !loading && (
                <TableRow>
                  <TableCell colSpan={camposVisiveis.length} align="center">
                    Nenhum dado encontrado para os filtros especificados.
                  </TableCell>
                </TableRow>
              )}
              {dados?.dados?.map((registro, index) => (
                <TableRow key={registro.id || index} hover>
                  {camposVisiveis.map((campo) => {
                    const valor = (registro as any)[campo.nome];
                    let valorFormatado = formatarValor(valor, campo);
                    if (campo.mascara && valor) {
                      valorFormatado = aplicarMascara(valor.toString(), campo.mascara);
                    }
                    return (
                      <TableCell key={campo.nome}>
                        {campo.tipo === 'boolean' ? (
                          <Chip
                            label={valorFormatado}
                            color={valor ? 'success' : 'default'}
                            size="small"
                          />
                        ) : (
                          valorFormatado
                        )}
                      </TableCell>
                    );
                  })}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
        {dados && (
          <TablePagination
            component="div"
            count={totalRegistros}
            page={pagina}
            onPageChange={(_, newPage) => setPagina(newPage)}
            rowsPerPage={tamanhoPagina}
            onRowsPerPageChange={(event) => {
              setTamanhoPagina(parseInt(event.target.value, 10));
              setPagina(0);
            }}
            rowsPerPageOptions={[10, 25, 50, 100]}
            labelRowsPerPage="Registros por página:"
            labelDisplayedRows={({ from, to, count }) =>
              `${from}-${to} de ${count !== -1 ? count : `mais de ${to}`}`
            }
          />
        )}
      </Paper>
    </Box>
  );
};

export default RelatorioVisualizarPage;