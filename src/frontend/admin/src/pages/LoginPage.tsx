import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  CircularProgress,
  Container,
  FormControlLabel,
  Checkbox,
  Stack,
  IconButton,
  InputAdornment,
  Fade,
  useTheme,
  alpha,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  LockOutlined,
  People,
  Assessment,
  GroupWork,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const { login, loginLoading, user } = useAuth();
  const navigate = useNavigate();
  const theme = useTheme();
  const toast = useToast();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login({ email, password });
      if (rememberMe) {
        localStorage.setItem('rememberEmail', email);
      } else {
        localStorage.removeItem('rememberEmail');
      }
      toast.success('Bem-vindo!', 'Login realizado com sucesso.');
      navigate('/dashboard');
    } catch {
      toast.error('Erro', 'Credenciais inválidas. Verifique seus dados e tente novamente.');
    }
  };

  React.useEffect(() => {
    const savedEmail = localStorage.getItem('rememberEmail');
    if (savedEmail) {
      setEmail(savedEmail);
      setRememberMe(true);
    }
  }, []);

  React.useEffect(() => {
    if (user) navigate('/dashboard');
  }, [user, navigate]);

  const features = [
    {
      icon: <People color="primary" />,
      title: 'Gestão de Associados',
      description: 'Cadastro e consulta de associados do sindicato',
    },
    {
      icon: <Assessment color="primary" />,
      title: 'Relatórios',
      description: 'Consultas e relatórios sobre associados',
    },
    {
      icon: <GroupWork color="primary" />,
      title: 'SintrafGV',
      description: 'Plataforma do sindicato',
    },
  ];

  return (
    <Box
      sx={{
        minHeight: '100vh',
        width: '100%',
        background: `linear-gradient(135deg, ${theme.palette.primary.main} 0%, ${theme.palette.primary.dark} 100%)`,
        display: 'flex',
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        overflow: 'auto',
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundImage: `radial-gradient(circle at 20% 50%, ${alpha(theme.palette.common.white, 0.1)} 0%, transparent 50%)`,
          zIndex: 0,
        }}
      />

      <Container maxWidth="lg" sx={{ position: 'relative', zIndex: 1, flex: 1, display: 'flex' }}>
        <Box
          sx={{
            minHeight: '100vh',
            width: '100%',
            display: 'flex',
            alignItems: 'center',
            py: 4,
          }}
        >
          <Box sx={{ display: 'flex', width: '100%', gap: 4, alignItems: 'center', flexWrap: 'wrap' }}>
            {/* Lado esquerdo - Marca e destaques */}
            <Box
              sx={{
                flex: '1 1 300px',
                color: 'white',
                display: { xs: 'none', md: 'block' },
                minWidth: 0,
              }}
            >
              <Fade in timeout={1000}>
                <Box>
                  <Box sx={{ mb: 4 }}>
                    <People sx={{ fontSize: 48, mb: 2 }} />
                    <Typography variant="h3" component="h1" fontWeight="bold" gutterBottom>
                      SintrafGV
                    </Typography>
                    <Typography variant="h6" sx={{ opacity: 0.9, mb: 4 }}>
                      Painel Administrativo do Sindicato
                    </Typography>
                    <Typography variant="body1" sx={{ opacity: 0.8, lineHeight: 1.6 }}>
                      Acesse o sistema para gerenciar associados, consultar relatórios e
                      administrar as atividades do sindicato.
                    </Typography>
                  </Box>

                  <Stack spacing={3}>
                    {features.map((feature, index) => (
                      <Fade in timeout={1500 + index * 200} key={index}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          {feature.icon}
                          <Box>
                            <Typography variant="subtitle1" fontWeight="medium">
                              {feature.title}
                            </Typography>
                            <Typography variant="body2" sx={{ opacity: 0.8 }}>
                              {feature.description}
                            </Typography>
                          </Box>
                        </Box>
                      </Fade>
                    ))}
                  </Stack>
                </Box>
              </Fade>
            </Box>

            {/* Lado direito - Formulário de login */}
            <Box sx={{ flex: { xs: '1 1 100%', md: '0 0 420px' }, maxWidth: 480, minWidth: 0 }}>
              <Fade in timeout={800}>
                <Card
                  sx={{
                    borderRadius: 3,
                    boxShadow: '0 20px 40px rgba(0,0,0,0.1)',
                    backgroundColor: '#fff',
                    border: `1px solid ${alpha(theme.palette.common.white, 0.1)}`,
                  }}
                >
                  <CardContent sx={{ p: 4 }}>
                    <Box sx={{ textAlign: 'center', mb: 4, color: '#212121' }}>
                      <Box
                        sx={{
                          width: 64,
                          height: 64,
                          borderRadius: '50%',
                          backgroundColor: theme.palette.primary.main,
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          margin: '0 auto 16px',
                          boxShadow: `0 8px 24px ${alpha(theme.palette.primary.main, 0.3)}`,
                        }}
                      >
                        <LockOutlined sx={{ color: 'white', fontSize: 28 }} />
                      </Box>

                      <Typography variant="h5" component="h2" fontWeight="bold" gutterBottom sx={{ color: '#212121' }}>
                        Acesso ao Sistema
                      </Typography>
                      <Typography variant="body2" sx={{ color: '#666' }}>
                        Entre com suas credenciais para continuar
                      </Typography>
                    </Box>

                    <Box component="form" onSubmit={handleSubmit} sx={{ color: '#212121' }}>
                      <TextField
                        fullWidth
                        label="E-mail"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        margin="normal"
                        required
                        autoFocus
                        placeholder="seu@email.com"
                        InputLabelProps={{ sx: { color: '#424242' } }}
                        inputProps={{ style: { color: '#212121' } }}
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: '#fff',
                            color: '#212121',
                            '& .MuiOutlinedInput-input': { color: '#212121' },
                            '& fieldset': { borderColor: 'rgba(0,0,0,0.23)' },
                          },
                          '& .MuiInputLabel-root': { color: '#424242' },
                          '& .MuiInputLabel-root.Mui-focused': { color: theme.palette.primary.main },
                        }}
                      />

                      <TextField
                        fullWidth
                        label="Senha"
                        type={showPassword ? 'text' : 'password'}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        margin="normal"
                        required
                        placeholder="Digite sua senha"
                        InputLabelProps={{ sx: { color: '#424242' } }}
                        inputProps={{ style: { color: '#212121' } }}
                        InputProps={{
                          endAdornment: (
                            <InputAdornment position="end">
                              <IconButton
                                onClick={() => setShowPassword(!showPassword)}
                                edge="end"
                                size="small"
                                aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
                              >
                                {showPassword ? <VisibilityOff /> : <Visibility />}
                              </IconButton>
                            </InputAdornment>
                          ),
                        }}
                        sx={{
                          '& .MuiOutlinedInput-root': {
                            borderRadius: 2,
                            backgroundColor: '#fff',
                            color: '#212121',
                            '& .MuiOutlinedInput-input': { color: '#212121' },
                            '& fieldset': { borderColor: 'rgba(0,0,0,0.23)' },
                          },
                          '& .MuiInputLabel-root': { color: '#424242' },
                          '& .MuiInputLabel-root.Mui-focused': { color: theme.palette.primary.main },
                        }}
                      />

                      <Box
                        sx={{
                          display: 'flex',
                          alignItems: 'center',
                          mt: 2,
                          mb: 3,
                        }}
                      >
                        <FormControlLabel
                          control={
                            <Checkbox
                              checked={rememberMe}
                              onChange={(e) => setRememberMe(e.target.checked)}
                              color="primary"
                            />
                          }
                          label={<Typography variant="body2" sx={{ color: '#212121' }}>Lembrar-me</Typography>}
                        />
                      </Box>

                      <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        size="large"
                        disabled={loginLoading}
                        sx={{
                          py: 1.5,
                          borderRadius: 2,
                          fontWeight: 'bold',
                          fontSize: '1rem',
                          boxShadow: `0 8px 24px ${alpha(theme.palette.primary.main, 0.3)}`,
                          '&:hover': {
                            boxShadow: `0 12px 32px ${alpha(theme.palette.primary.main, 0.4)}`,
                          },
                        }}
                      >
                        {loginLoading ? (
                          <CircularProgress size={24} color="inherit" />
                        ) : (
                          'Entrar no Sistema'
                        )}
                      </Button>
                    </Box>
                  </CardContent>
                </Card>
              </Fade>
            </Box>
          </Box>
        </Box>
      </Container>
    </Box>
  );
};

export default LoginPage;
