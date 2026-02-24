import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  Button,
  Chip,
  Stack,
} from '@mui/material';
import {
  TrendingUp,
  TrendingDown,
  People,
  PersonAdd,
  PersonOff,
  Assessment,
  HowToVote,
} from '@mui/icons-material';
import { Line, Bar, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
} from 'chart.js';
import relatorioService, { DashboardKpi } from '../services/relatorioService';

// Registrar componentes do Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement
);

interface KpiCardProps {
  titulo: string;
  valor: number;
  icone: React.ReactElement;
  cor: string;
  crescimento?: number;
  subtitle?: string;
}

const KpiCard: React.FC<KpiCardProps> = ({ titulo, valor, icone, cor, crescimento, subtitle }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box display="flex" justifyContent="space-between" alignItems="flex-start">
        <Box>
          <Typography color="text.secondary" gutterBottom variant="body2">
            {titulo}
          </Typography>
          <Typography variant="h4" fontWeight="bold">
            {valor.toLocaleString('pt-BR')}
          </Typography>
          {subtitle && (
            <Typography variant="body2" color="text.secondary">
              {subtitle}
            </Typography>
          )}
          {crescimento !== undefined && (
            <Box display="flex" alignItems="center" mt={1}>
              {crescimento >= 0 ? (
                <TrendingUp sx={{ color: 'success.main', mr: 0.5 }} fontSize="small" />
              ) : (
                <TrendingDown sx={{ color: 'error.main', mr: 0.5 }} fontSize="small" />
              )}
              <Typography
                variant="body2"
                color={crescimento >= 0 ? 'success.main' : 'error.main'}
                fontWeight="medium"
              >
                {crescimento > 0 ? '+' : ''}{crescimento.toFixed(1)}%
              </Typography>
            </Box>
          )}
        </Box>
        <Box
          sx={{
            bgcolor: cor,
            color: 'white',
            borderRadius: '50%',
            p: 1.5,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {icone}
        </Box>
      </Box>
    </CardContent>
  </Card>
);

const DashboardPage: React.FC = () => {
  const [kpis, setKpis] = useState<DashboardKpi | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const carregarDados = async () => {
      try {
        setLoading(true);
        const dados = await relatorioService.obterDashboardKpis();
        setKpis(dados);
        setError(null);
      } catch (err) {
        console.error('Erro ao carregar dashboard:', err);
        setError('Erro ao carregar dados do dashboard');
      } finally {
        setLoading(false);
      }
    };

    carregarDados();
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  if (!kpis) {
    return (
      <Alert severity="info">
        Nenhum dado disponível para exibir.
      </Alert>
    );
  }

  // Configurações dos gráficos
  const graficoSexoConfig = {
    data: {
      labels: kpis.graficoPorSexo.map(item => item.label),
      datasets: [
        {
          data: kpis.graficoPorSexo.map(item => item.valor),
          backgroundColor: kpis.graficoPorSexo.map(item => item.cor || '#2196F3'),
          borderWidth: 0,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom' as const,
        },
      },
    },
  };

  const graficoBancoConfig = {
    data: {
      labels: kpis.graficoPorBanco.slice(0, 8).map(item => item.label),
      datasets: [
        {
          label: 'Associados',
          data: kpis.graficoPorBanco.slice(0, 8).map(item => item.valor),
          backgroundColor: '#2196F3',
          borderRadius: 4,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            precision: 0,
          },
        },
      },
    },
  };

  const graficoCrescimentoConfig = {
    data: {
      labels: kpis.crescimentoMensal.map(item => item.label),
      datasets: [
        {
          label: 'Novos Associados',
          data: kpis.crescimentoMensal.map(item => item.valor),
          borderColor: '#4CAF50',
          backgroundColor: '#4CAF50',
          tension: 0.4,
          fill: false,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            precision: 0,
          },
        },
      },
    },
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" gutterBottom fontWeight="bold">
          Dashboard
        </Typography>
        <Button variant="outlined" href="/relatorios">
          Ver Relatórios
        </Button>
      </Box>

      {/* KPIs Principais */}
      <Grid container spacing={3} mb={4}>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            titulo="Total de Associados"
            valor={kpis.totalAssociados}
            icone={<People />}
            cor="#2196F3"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            titulo="Associados Ativos"
            valor={kpis.associadosAtivos}
            icone={<People />}
            cor="#4CAF50"
            subtitle={`${((kpis.associadosAtivos / kpis.totalAssociados) * 100).toFixed(1)}% do total`}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            titulo="Novos no Mês"
            valor={kpis.novosMesAtual}
            icone={<PersonAdd />}
            cor="#FF9800"
            crescimento={kpis.percentualCrescimento}
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <KpiCard
            titulo="Enquetes Ativas"
            valor={kpis.enquetesAbertas}
            icone={<HowToVote />}
            cor="#9C27B0"
            subtitle={`${kpis.enquetesEncerradas} encerradas`}
          />
        </Grid>
      </Grid>

      {/* Segunda linha de KPIs */}
      <Grid container spacing={3} mb={4}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Associados Inativos
              </Typography>
              <Typography variant="h5" fontWeight="bold">
                {kpis.associadosInativos.toLocaleString('pt-BR')}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {((kpis.associadosInativos / kpis.totalAssociados) * 100).toFixed(1)}% do total
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Desligamentos no Mês
              </Typography>
              <Typography variant="h5" fontWeight="bold" color="error">
                {kpis.desligadosMesAtual.toLocaleString('pt-BR')}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Requer atenção
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Taxa de Retenção
              </Typography>
              <Typography variant="h5" fontWeight="bold" color="success.main">
                {((kpis.associadosAtivos / kpis.totalAssociados) * 100).toFixed(1)}%
              </Typography>
              <Chip
                label="Excelente"
                size="small"
                color="success"
                sx={{ mt: 1 }}
              />
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="text.secondary" gutterBottom variant="body2">
                Crescimento Líquido
              </Typography>
              <Typography variant="h5" fontWeight="bold">
                +{(kpis.novosMesAtual - kpis.desligadosMesAtual).toLocaleString('pt-BR')}
              </Typography>
              <Typography variant="body2" color="success.main">
                Novos - Desligamentos
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Gráficos */}
      <Grid container spacing={3}>
        {/* Distribuição por Sexo */}
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3, height: 350 }}>
            <Typography variant="h6" gutterBottom>
              Distribuição por Sexo
            </Typography>
            <Box height={250}>
              <Doughnut {...graficoSexoConfig} />
            </Box>
          </Paper>
        </Grid>

        {/* Top Bancos */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3, height: 350 }}>
            <Typography variant="h6" gutterBottom>
              Principais Instituições Bancárias
            </Typography>
            <Box height={250}>
              <Bar {...graficoBancoConfig} />
            </Box>
          </Paper>
        </Grid>

        {/* Crescimento Mensal */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3, height: 350 }}>
            <Typography variant="h6" gutterBottom>
              Evolução de Novos Associados (Últimos 12 meses)
            </Typography>
            <Box height={250}>
              <Line {...graficoCrescimentoConfig} />
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Resumo Textual */}
      <Paper sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Resumo Executivo
        </Typography>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Typography variant="body2" component="span">
            O SintrafGV possui atualmente
          </Typography>
          <Chip label={`${kpis.totalAssociados.toLocaleString('pt-BR')} associados`} size="small" color="primary" />
          <Typography variant="body2" component="span">
            sendo
          </Typography>
          <Chip label={`${kpis.associadosAtivos.toLocaleString('pt-BR')} ativos`} size="small" color="success" />
          <Typography variant="body2" component="span">
            ({((kpis.associadosAtivos / kpis.totalAssociados) * 100).toFixed(1)}%).
          </Typography>
          <Typography variant="body2" component="span">
            No mês atual foram
          </Typography>
          <Chip label={`${kpis.novosMesAtual} novos associados`} size="small" color="info" />
          <Typography variant="body2" component="span">
            e
          </Typography>
          <Chip label={`${kpis.desligadosMesAtual} desligamentos`} size="small" color="warning" />
          <Typography variant="body2" component="span">
            resultando em crescimento líquido de
          </Typography>
          <Chip 
            label={`+${kpis.novosMesAtual - kpis.desligadosMesAtual} associados`} 
            size="small" 
            color={kpis.novosMesAtual - kpis.desligadosMesAtual >= 0 ? "success" : "error"} 
          />
        </Stack>
      </Paper>
    </Box>
  );
};

export default DashboardPage;