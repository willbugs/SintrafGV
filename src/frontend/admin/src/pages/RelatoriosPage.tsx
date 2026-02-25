import React, { useState } from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Button,
  IconButton,
  Chip,
  Stack,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Divider,
} from '@mui/material';
import {
  Assessment,
  People,
  Cake,
  PersonAdd,
  Group,
  LocationOn,
  Business,
  GetApp,
  Visibility,
  Close,
  HowToVote,
  TrendingUp,
  Gavel,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

interface TipoRelatorio {
  id: string;
  titulo: string;
  descricao: string;
  icone: React.ReactElement;
  categoria: string;
  cor: string;
}

const tiposRelatorios: TipoRelatorio[] = [
  {
    id: 'associados-geral',
    titulo: 'Relatório Geral',
    descricao: 'Lista completa de associados com filtros personalizáveis',
    icone: <People />,
    categoria: 'Associados',
    cor: '#2196F3'
  },
  {
    id: 'associados-ativos',
    titulo: 'Associados Ativos',
    descricao: 'Apenas associados em situação ativa',
    icone: <Group />,
    categoria: 'Associados',
    cor: '#4CAF50'
  },
  {
    id: 'associados-inativos',
    titulo: 'Associados Inativos',
    descricao: 'Associados desligados com motivos',
    icone: <People />,
    categoria: 'Associados',
    cor: '#FF9800'
  },
  {
    id: 'aniversariantes',
    titulo: 'Aniversariantes',
    descricao: 'Associados por mês de aniversário',
    icone: <Cake />,
    categoria: 'Associados',
    cor: '#E91E63'
  },
  {
    id: 'novos-associados',
    titulo: 'Novos Associados',
    descricao: 'Associados filiados em período específico',
    icone: <PersonAdd />,
    categoria: 'Associados',
    cor: '#9C27B0'
  },
  {
    id: 'por-banco',
    titulo: 'Por Banco',
    descricao: 'Distribuição por instituição bancária',
    icone: <Business />,
    categoria: 'Demográficos',
    cor: '#607D8B'
  },
  {
    id: 'por-cidade',
    titulo: 'Por Cidade',
    descricao: 'Distribuição geográfica por cidade',
    icone: <LocationOn />,
    categoria: 'Demográficos',
    cor: '#795548'
  },
  {
    id: 'por-sexo',
    titulo: 'Por Sexo',
    descricao: 'Distribuição por gênero',
    icone: <Assessment />,
    categoria: 'Demográficos',
    cor: '#FF5722'
  },
  // Relatórios de Votações/Eleições
  {
    id: 'participacao-votacao',
    titulo: 'Participação em Votações',
    descricao: 'Análise de engajamento dos associados nas enquetes',
    icone: <HowToVote />,
    categoria: 'Votações',
    cor: '#2196F3'
  },
  {
    id: 'resultados-eleicao',
    titulo: 'Resultados de Enquetes',
    descricao: 'Detalhamento dos resultados por enquete e pergunta',
    icone: <Assessment />,
    categoria: 'Votações',
    cor: '#4CAF50'
  },
  {
    id: 'engajamento-votacao',
    titulo: 'Engajamento em Votações',
    descricao: 'Métricas de participação e engajamento por período',
    icone: <TrendingUp />,
    categoria: 'Votações',
    cor: '#FF9800'
  },
  {
    id: 'cartorial',
    titulo: 'Relatório Cartorial',
    descricao: 'Relatório oficial para autenticação em cartório',
    icone: <Gavel />,
    categoria: 'Votações',
    cor: '#9C27B0'
  },
];

const RelatoriosPage: React.FC = () => {
  const navigate = useNavigate();
  const [dialogAberto, setDialogAberto] = useState(false);
  const [relatorioSelecionado, setRelatorioSelecionado] = useState<TipoRelatorio | null>(null);
  const [filtros, setFiltros] = useState({
    mes: new Date().getMonth() + 1,
    sexo: '',
    dataInicio: '',
    dataFim: '',
    formato: 'html' as 'html' | 'pdf' | 'excel' | 'csv',
    incluirInativos: false,
  });

  const categorias = Array.from(new Set(tiposRelatorios.map(r => r.categoria)));

  const handleAbrirRelatorio = (tipo: TipoRelatorio) => {
    // Navegar diretamente para páginas específicas baseadas no tipo
    switch (tipo.id) {
      case 'participacao-votacao':
        navigate('/relatorios/votacao?tab=0');
        break;
      case 'resultados-eleicao':
        navigate('/relatorios/votacao?tab=1');
        break;
      case 'engajamento-votacao':
        navigate('/relatorios/votacao?tab=2');
        break;
      case 'cartorial':
        navigate('/relatorios/cartorial');
        break;
      default:
        // Para relatórios genéricos, usar o dialog
        setRelatorioSelecionado(tipo);
        setDialogAberto(true);
        break;
    }
  };

  const handleFecharDialog = () => {
    setDialogAberto(false);
    setRelatorioSelecionado(null);
  };

  const handleExecutarRelatorio = () => {
    if (!relatorioSelecionado) return;

    // Preparar parâmetros do relatório
    const params = new URLSearchParams({
      tipo: relatorioSelecionado.id,
      formato: filtros.formato,
      incluirInativos: filtros.incluirInativos.toString(),
    });

    // Adicionar filtros específicos baseados no tipo de relatório
    if (relatorioSelecionado.id === 'aniversariantes') {
      params.append('mes', filtros.mes.toString());
    }
    
    if (relatorioSelecionado.id === 'por-sexo' && filtros.sexo) {
      params.append('sexo', filtros.sexo);
    }
    
    if (relatorioSelecionado.id === 'novos-associados' && filtros.dataInicio && filtros.dataFim) {
      params.append('dataInicio', filtros.dataInicio);
      params.append('dataFim', filtros.dataFim);
    }

    // Navegar para visualização do relatório
    navigate(`/relatorios/visualizar?${params.toString()}`);
    handleFecharDialog();
  };

  const renderFiltrosEspecificos = () => {
    if (!relatorioSelecionado) return null;

    switch (relatorioSelecionado.id) {
      case 'aniversariantes':
        return (
          <FormControl fullWidth margin="normal">
            <InputLabel>Mês</InputLabel>
            <Select
              value={filtros.mes}
              onChange={(e) => setFiltros({ ...filtros, mes: Number(e.target.value) })}
              label="Mês"
            >
              {Array.from({ length: 12 }, (_, i) => (
                <MenuItem key={i + 1} value={i + 1}>
                  {new Date(0, i).toLocaleString('pt-BR', { month: 'long' })}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );

      case 'por-sexo':
        return (
          <FormControl fullWidth margin="normal">
            <InputLabel>Sexo</InputLabel>
            <Select
              value={filtros.sexo}
              onChange={(e) => setFiltros({ ...filtros, sexo: e.target.value })}
              label="Sexo"
            >
              <MenuItem value="M">Masculino</MenuItem>
              <MenuItem value="F">Feminino</MenuItem>
            </Select>
          </FormControl>
        );

      case 'novos-associados':
        return (
          <>
            <TextField
              fullWidth
              margin="normal"
              label="Data Início"
              type="date"
              value={filtros.dataInicio}
              onChange={(e) => setFiltros({ ...filtros, dataInicio: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              fullWidth
              margin="normal"
              label="Data Fim"
              type="date"
              value={filtros.dataFim}
              onChange={(e) => setFiltros({ ...filtros, dataFim: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </>
        );

      default:
        return null;
    }
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" fontWeight="bold">
          Relatórios
        </Typography>
        <Button
          variant="outlined"
          startIcon={<Assessment />}
          onClick={() => navigate('/dashboard')}
        >
          Ver Dashboard
        </Button>
      </Box>

      {categorias.map((categoria) => (
        <Box key={categoria} mb={4}>
          <Typography variant="h6" gutterBottom fontWeight="600" color="primary">
            {categoria}
          </Typography>
          <Grid container spacing={2}>
            {tiposRelatorios
              .filter((tipo) => tipo.categoria === categoria)
              .map((tipo) => (
                <Grid item xs={12} sm={6} md={4} key={tipo.id}>
                  <Card
                    sx={{
                      height: '100%',
                      transition: 'transform 0.2s, box-shadow 0.2s',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: 4,
                      },
                    }}
                  >
                    <CardContent>
                      <Box display="flex" alignItems="center" mb={2}>
                        <Box
                          sx={{
                            bgcolor: tipo.cor,
                            color: 'white',
                            borderRadius: '50%',
                            p: 1,
                            mr: 2,
                            minWidth: 40,
                            height: 40,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                          }}
                        >
                          {tipo.icone}
                        </Box>
                        <Typography variant="h6" fontWeight="600">
                          {tipo.titulo}
                        </Typography>
                      </Box>
                      <Typography variant="body2" color="text.secondary" mb={2}>
                        {tipo.descricao}
                      </Typography>
                      <Chip
                        label={categoria}
                        size="small"
                        sx={{ bgcolor: tipo.cor, color: 'white' }}
                      />
                    </CardContent>
                    <CardActions>
                      <Button
                        size="small"
                        startIcon={<Visibility />}
                        onClick={() => handleAbrirRelatorio(tipo)}
                      >
                        Gerar Relatório
                      </Button>
                      <IconButton 
                        size="small" 
                        onClick={(e) => {
                          e.stopPropagation();
                          handleAbrirRelatorio(tipo);
                        }}
                        title="Exportar relatório"
                      >
                        <GetApp />
                      </IconButton>
                    </CardActions>
                  </Card>
                </Grid>
              ))}
          </Grid>
        </Box>
      ))}

      {/* Dialog de Configuração do Relatório */}
      <Dialog 
        open={dialogAberto} 
        onClose={handleFecharDialog}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">
              Configurar Relatório: {relatorioSelecionado?.titulo}
            </Typography>
            <IconButton onClick={handleFecharDialog} size="small">
              <Close />
            </IconButton>
          </Box>
        </DialogTitle>
        
        <DialogContent>
          <Typography variant="body2" color="text.secondary" mb={3}>
            {relatorioSelecionado?.descricao}
          </Typography>

          <Divider sx={{ mb: 2 }} />

          {/* Formato de Saída */}
          <FormControl fullWidth margin="normal">
            <InputLabel>Formato</InputLabel>
            <Select
              value={filtros.formato}
              onChange={(e) => setFiltros({ ...filtros, formato: e.target.value as any })}
              label="Formato"
            >
              <MenuItem value="html">Visualizar na Tela</MenuItem>
              <MenuItem value="pdf">PDF</MenuItem>
              <MenuItem value="excel">Excel</MenuItem>
              <MenuItem value="csv">CSV</MenuItem>
            </Select>
          </FormControl>

          {/* Filtros Específicos */}
          {renderFiltrosEspecificos()}

          {/* Opções Gerais */}
          <Stack spacing={2} mt={2}>
            <FormControlLabel
              control={
                <Switch
                  checked={filtros.incluirInativos}
                  onChange={(e) => setFiltros({ ...filtros, incluirInativos: e.target.checked })}
                />
              }
              label="Incluir associados inativos"
            />
          </Stack>
        </DialogContent>

        <DialogActions>
          <Button onClick={handleFecharDialog}>Cancelar</Button>
          <Button
            variant="contained"
            onClick={handleExecutarRelatorio}
            startIcon={<Assessment />}
          >
            Gerar Relatório
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default RelatoriosPage;