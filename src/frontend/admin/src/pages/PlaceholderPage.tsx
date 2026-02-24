import React from 'react';
import { Box, Typography, Paper } from '@mui/material';

interface Props {
  title: string;
}

const PlaceholderPage: React.FC<Props> = ({ title }) => (
  <Box>
    <Typography variant="h4" gutterBottom fontWeight="bold">
      {title}
    </Typography>
    <Paper sx={{ p: 3 }}>
      <Typography color="text.secondary">Página em desenvolvimento.</Typography>
    </Paper>
  </Box>
);

export default PlaceholderPage;
