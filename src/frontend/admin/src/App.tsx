import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { SnackbarProvider } from 'notistack';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { Box, CircularProgress } from '@mui/material';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import AdminLayout from './components/Layout/AdminLayout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import AssociadosPage from './pages/AssociadosPage';
import UsuariosPage from './pages/UsuariosPage';
import UsuarioFormPage from './pages/UsuarioFormPage';
import AssociadoFormPage from './pages/AssociadoFormPage';
import EleicoesPage from './pages/EleicoesPage';
import EleicaoFormPage from './pages/EleicaoFormPage';
import ResultadosEleicaoPage from './pages/ResultadosEleicaoPage';
import RelatoriosPage from './pages/RelatoriosPage';
import RelatorioVisualizarPage from './pages/RelatorioVisualizarPage';
import ConfiguracaoSindicatoPage from './pages/ConfiguracaoSindicatoPage';
import RelatorioCartorialPage from './pages/RelatorioCartorialPage';
import RelatoriosVotacaoPage from './pages/RelatoriosVotacaoPage';
import PerfilPage from './pages/PerfilPage';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, loading } = useAuth();
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }
  return user ? <>{children}</> : <Navigate to="/login" replace />;
};

// Tema baseado no Bureau. OutlinedInput com fundo branco explícito
// porque MUI 5 + React 19 deixa o background.default (#f5f5f5) vazar nos inputs.
// No Bureau (React 18) isso não acontece.
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f5f5f5',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
    },
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiDrawer: {
      styleOverrides: {
        paper: {
          backgroundColor: '#fff',
          borderRight: '1px solid #e0e0e0',
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#1976d2',
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          backgroundColor: '#fff',
        },
        input: {
          '&:-webkit-autofill': {
            WebkitBoxShadow: '0 0 0 100px #fff inset',
          },
        },
      },
    },
  },
});

const AppContent: React.FC = () => (
  <Router
    future={{
      v7_startTransition: true,
      v7_relativeSplatPath: true
    }}
  >
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <AdminLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="associados" element={<Outlet />}>
          <Route index element={<AssociadosPage />} />
          <Route path="novo" element={<AssociadoFormPage />} />
          <Route path=":id" element={<AssociadoFormPage />} />
        </Route>
        <Route path="usuarios" element={<Outlet />}>
          <Route index element={<UsuariosPage />} />
          <Route path="novo" element={<UsuarioFormPage />} />
          <Route path=":id" element={<UsuarioFormPage />} />
        </Route>
        <Route path="eleicoes" element={<Outlet />}>
          <Route index element={<EleicoesPage />} />
          <Route path="novo" element={<EleicaoFormPage />} />
          <Route path=":id" element={<EleicaoFormPage />} />
          <Route path=":id/resultados" element={<ResultadosEleicaoPage />} />
        </Route>
        <Route path="relatorios" element={<Outlet />}>
          <Route index element={<RelatoriosPage />} />
          <Route path="visualizar" element={<RelatorioVisualizarPage />} />
          <Route path="cartorial" element={<RelatorioCartorialPage />} />
          <Route path="votacao" element={<RelatoriosVotacaoPage />} />
        </Route>
        <Route path="configuracoes" element={<Outlet />}>
          <Route index element={<ConfiguracaoSindicatoPage />} />
          <Route path="sindicato" element={<ConfiguracaoSindicatoPage />} />
        </Route>
        <Route path="perfil" element={<PerfilPage />} />
      </Route>
    </Routes>
  </Router>
);

function App() {
  return (
    <AuthProvider>
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <SnackbarProvider
            maxSnack={3}
            anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
          >
            <ToastProvider>
              <AppContent />
            </ToastProvider>
          </SnackbarProvider>
        </ThemeProvider>
      </LocalizationProvider>
    </AuthProvider>
  );
}

export default App;
