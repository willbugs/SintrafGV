import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import { AuthProvider } from './contexts/AuthContext'
import LoginPage from './pages/LoginPage'
import CadastroPage from './pages/CadastroPage'
import EleicoesPage from './pages/EleicoesPage'
import VotacaoPage from './pages/VotacaoPage'
import ComprovantePage from './pages/ComprovantePage'
import { PrivateRoute } from './components/PrivateRoute'
import { RedirectEleicoesParaEnquetes } from './components/RedirectEleicoesParaEnquetes'
import './App.css'

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    fontFamily: [
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      'Roboto',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif',
    ].join(','),
  },
})

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <Router>
          <div className="App">
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/cadastro" element={<CadastroPage />} />
              <Route path="/eleicoes" element={<RedirectEleicoesParaEnquetes />} />
              <Route path="/enquetes" element={
                <PrivateRoute>
                  <EleicoesPage />
                </PrivateRoute>
              } />
              <Route path="/votacao/:enqueteId" element={
                <PrivateRoute>
                  <VotacaoPage />
                </PrivateRoute>
              } />
              <Route path="/comprovante/:votoId" element={
                <PrivateRoute>
                  <ComprovantePage />
                </PrivateRoute>
              } />
              <Route path="/" element={<Navigate to="/enquetes" replace />} />
            </Routes>
          </div>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  )
}

export default App