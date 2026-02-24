import React from 'react';
import { Box, Typography, Paper, Grid } from '@mui/material';

const DashboardPage: React.FC = () => {
  return (
    <Box>
      <Typography variant="h4" gutterBottom fontWeight="bold">
        Dashboard
      </Typography>
      <Grid container spacing={2}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Associados ativos</Typography>
            <Typography variant="h4">—</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Total de associados</Typography>
            <Typography variant="h4">—</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography color="text.secondary">Enquetes ativas</Typography>
            <Typography variant="h4">—</Typography>
          </Paper>
        </Grid>
      </Grid>
      <Paper sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Bem-vindo ao SintrafGV
        </Typography>
        <Typography color="text.secondary">
          Use o menu para acessar Associados, Relatórios e Configurações. Os dados do dashboard serão
          integrados em seguida.
        </Typography>
      </Paper>
    </Box>
  );
};

export default DashboardPage;
