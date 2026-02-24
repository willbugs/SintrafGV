import React, { useState } from 'react';
import {
  Button,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  CircularProgress,
  Alert,
  Snackbar,
} from '@mui/material';
import {
  GetApp,
  PictureAsPdf,
  TableChart,
  Storage,
} from '@mui/icons-material';
import relatorioService, { RelatorioRequest } from '../../services/relatorioService';

interface ExportMenuProps {
  relatorioRequest: RelatorioRequest;
  disabled?: boolean;
  buttonVariant?: 'text' | 'outlined' | 'contained';
  buttonSize?: 'small' | 'medium' | 'large';
  children?: React.ReactNode;
}

const ExportMenu: React.FC<ExportMenuProps> = ({
  relatorioRequest,
  disabled = false,
  buttonVariant = 'outlined',
  buttonSize = 'medium',
  children
}) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleExport = async (formato: 'pdf' | 'excel' | 'csv') => {
    try {
      setLoading(true);
      setError(null);
      handleClose();

      const request = {
        ...relatorioRequest,
        formatoExportacao: formato as any
      };

      const blob = await relatorioService.exportarRelatorio(request);
      
      // Criar link para download
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      
      const timestamp = new Date().toISOString().split('T')[0];
      const extension = formato === 'excel' ? 'xlsx' : formato;
      link.download = `relatorio_${relatorioRequest.tipoRelatorio}_${timestamp}.${extension}`;
      
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
    } catch (err) {
      console.error('Erro ao exportar:', err);
      setError('Erro ao gerar arquivo. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  const formatosExportacao = [
    {
      formato: 'pdf' as const,
      label: 'Exportar PDF',
      icon: <PictureAsPdf />,
      description: 'Documento formatado para impressão'
    },
    {
      formato: 'excel' as const,
      label: 'Exportar Excel',
      icon: <TableChart />,
      description: 'Planilha para análise de dados'
    },
    {
      formato: 'csv' as const,
      label: 'Exportar CSV',
      icon: <Storage />,
      description: 'Dados brutos separados por vírgula'
    }
  ];

  return (
    <>
      <Button
        variant={buttonVariant}
        size={buttonSize}
        startIcon={loading ? <CircularProgress size={16} /> : <GetApp />}
        onClick={handleClick}
        disabled={disabled || loading}
        aria-controls={open ? 'export-menu' : undefined}
        aria-haspopup="true"
        aria-expanded={open ? 'true' : undefined}
      >
        {children || 'Exportar'}
      </Button>

      <Menu
        id="export-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        MenuListProps={{
          'aria-labelledby': 'export-button',
        }}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'left',
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'left',
        }}
      >
        {formatosExportacao.map((item) => (
          <MenuItem
            key={item.formato}
            onClick={() => handleExport(item.formato)}
            disabled={loading}
          >
            <ListItemIcon>
              {item.icon}
            </ListItemIcon>
            <ListItemText
              primary={item.label}
              secondary={item.description}
            />
          </MenuItem>
        ))}
      </Menu>

      <Snackbar
        open={!!error}
        autoHideDuration={6000}
        onClose={() => setError(null)}
      >
        <Alert onClose={() => setError(null)} severity="error" sx={{ width: '100%' }}>
          {error}
        </Alert>
      </Snackbar>
    </>
  );
};

export default ExportMenu;