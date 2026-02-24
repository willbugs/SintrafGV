import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import { AuthProvider } from './contexts/AuthContext'
import LoginPage from './pages/LoginPage'
import EleicoesPage from './pages/EleicoesPage'
import VotacaoPage from './pages/VotacaoPage'
import ComprovantePage from './pages/ComprovantePage'
import { PrivateRoute } from './components/PrivateRoute'
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
              <Route path="/eleicoes" element={
                <PrivateRoute>
                  <EleicoesPage />
                </PrivateRoute>
              } />
              <Route path="/votacao/:eleicaoId" element={
                <PrivateRoute>
                  <VotacaoPage />
                </PrivateRoute>
              } />
              <Route path="/comprovante/:votoId" element={
                <PrivateRoute>
                  <ComprovantePage />
                </PrivateRoute>
              } />
              <Route path="/" element={<Navigate to="/eleicoes" replace />} />
            </Routes>
          </div>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  )
}

export default App