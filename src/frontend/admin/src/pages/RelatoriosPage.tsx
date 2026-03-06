import React from 'react';
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
  const categorias = Array.from(new Set(tiposRelatorios.map(r => r.categoria)));

  const handleAbrirRelatorio = (tipo: TipoRelatorio) => {
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
        navigate(`/relatorios/visualizar?tipo=${tipo.id}`);
        break;
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
    </Box>
  );
};

export default RelatoriosPage;