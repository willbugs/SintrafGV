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
} from '@mui/material';
import {
  ArrowBack,
  GetApp,
  Print,
  Refresh,
  FilterList,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material';
import relatorioService, {
  RelatorioRequest,
  RelatorioResponse,
  AssociadoRelatorio,
  CampoRelatorio,
} from '../services/relatorioService';
import FiltroAvancado, { FiltroItem } from '../components/Relatorios/FiltroAvancado';
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
  const [filtros, setFiltros] = useState<FiltroItem[]>([]);
  const [mostrarFiltros, setMostrarFiltros] = useState(false);
  const [colunasVisiveis, setColunasVisiveis] = useState<ColunasVisibilidade>({});
  const [pagina, setPagina] = useState(0);
  const [tamanhoPagina, setTamanhoPagina] = useState(25);

  const tipoRelatorio = searchParams.get('tipo') || 'associados-geral';
  const formato = searchParams.get('formato') || 'html';

  useEffect(() => {
    carregarRelatorio();
    carregarCampos();
  }, [tipoRelatorio, pagina, tamanhoPagina]);

  const carregarCampos = async () => {
    try {
      const camposDisponiveis = await relatorioService.obterCamposDisponiveis('associados-geral');
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
        default:
          resultado = await relatorioService.obterRelatorioAssociadosGeral(request);
      }

      setDados(resultado);
    } catch (err) {
      console.error('Erro ao carregar relatório:', err);
      setError('Erro ao carregar relatório. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  const construirFiltros = () => {
    const filtrosObj: Record<string, any> = {};
    
    // Filtros da URL
    for (const [key, value] of searchParams.entries()) {
      if (key !== 'tipo' && key !== 'formato') {
        filtrosObj[key] = value;
      }
    }

    // Filtros avançados
    filtros.forEach((filtro) => {
      filtrosObj[filtro.campo] = filtro.valor;
    });

    return filtrosObj;
  };

  const handleExportar = async (formatoExport: 'pdf' | 'excel' | 'csv') => {
    try {
      const request: RelatorioRequest = {
        tipoRelatorio,
        filtros: construirFiltros(),
        formatoExportacao: formatoExport,
      };

      const blob = await relatorioService.exportarRelatorio(request);
      
      // Criar link para download
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `relatorio_${tipoRelatorio}_${new Date().toISOString().split('T')[0]}.${formatoExport}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Erro ao exportar:', err);
      alert('Funcionalidade de exportação será implementada em breve');
    }
  };

  const toggleColunaVisibilidade = (nomeCampo: string) => {
    setColunasVisiveis(prev => ({
      ...prev,
      [nomeCampo]: !prev[nomeCampo]
    }));
  };

  const aplicarFiltros = () => {
    setPagina(0);
    carregarRelatorio();
  };

  const limparFiltros = () => {
    setFiltros([]);
    setPagina(0);
    carregarRelatorio();
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

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

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

  if (!dados) {
    return (
      <Alert severity="info">
        Nenhum dado encontrado para os filtros especificados.
      </Alert>
    );
  }

  const camposVisiveis = campos.filter(campo => colunasVisiveis[campo.nome]);

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
            {dados.metadata.titulo}
          </Typography>
          {dados.metadata.subtitulo && (
            <Typography variant="subtitle1" color="text.secondary">
              {dados.metadata.subtitulo}
            </Typography>
          )}
        </Box>
        
        <Stack direction="row" spacing={1}>
          <Tooltip title="Atualizar dados">
            <IconButton onClick={carregarRelatorio}>
              <Refresh />
            </IconButton>
          </Tooltip>
          <Button
            variant="outlined"
            startIcon={<FilterList />}
            onClick={() => setMostrarFiltros(!mostrarFiltros)}
          >
            Filtros
          </Button>
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
                {dados.metadata.totalRegistros.toLocaleString('pt-BR')}
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
                {dados.dados.length.toLocaleString('pt-BR')}
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
                {new Date(dados.metadata.dataGeracao).toLocaleString('pt-BR')}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                {Object.entries(dados.totalizadores).map(([chave, valor]) => (
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

      {/* Filtros Avançados */}
      {mostrarFiltros && (
        <FiltroAvancado
          campos={campos}
          filtros={filtros}
          onChange={setFiltros}
          onLimpar={limparFiltros}
        />
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

      {/* Tabela de Dados */}
      <Paper sx={{ width: '100%', overflow: 'hidden' }}>
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
              {dados.dados.map((registro, index) => (
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

        {/* Paginação */}
        <TablePagination
          component="div"
          count={dados.metadata.totalRegistros}
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
      </Paper>
    </Box>
  );
};

export default RelatorioVisualizarPage;