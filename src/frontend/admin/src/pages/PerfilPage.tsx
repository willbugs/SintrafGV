import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  TextField,
  Button,
  Avatar,
  Divider,
  Alert,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import {
  Person,
  Email,
  Lock,
  History,
  Edit,
  Save,
  Cancel,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';

interface PerfilData {
  id: string;
  nome: string;
  email: string;
  role: string;
  dataCriacao?: string;
  ultimoAcesso?: string;
}

interface AlterarSenhaData {
  senhaAtual: string;
  novaSenha: string;
  confirmarSenha: string;
}

const PerfilPage: React.FC = () => {
  const { user } = useAuth();
  const { success, error } = useToast();
  const [perfil, setPerfil] = useState<PerfilData | null>(null);
  const [editando, setEditando] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [dialogSenha, setDialogSenha] = useState(false);
  const [mostrarSenhas, setMostrarSenhas] = useState({
    atual: false,
    nova: false,
    confirmar: false,
  });

  const [dadosEdicao, setDadosEdicao] = useState({
    nome: '',
    email: '',
  });

  const [dadosSenha, setDadosSenha] = useState<AlterarSenhaData>({
    senhaAtual: '',
    novaSenha: '',
    confirmarSenha: '',
  });

  const [ultimasAcoes, setUltimasAcoes] = useState<Array<{acao: string; data: string}>>([]);

  useEffect(() => {
    carregarPerfil();
    carregarUltimasAcoes();
  }, []);

  const carregarUltimasAcoes = async () => {
    try {
      if (user) {
        const response = await api.get(`/api/usuarios/${user.id}/historico-acoes?limite=4`);
        setUltimasAcoes(response.data.map((item: any) => ({
          acao: item.acao,
          data: new Date(item.data).toLocaleString('pt-BR')
        })));
      }
    } catch (err) {
      // Se falhar, deixa vazio sem erro
      setUltimasAcoes([]);
    }
  };

  const carregarPerfil = async () => {
    try {
      setCarregando(true);
      if (user) {
        // Buscar dados reais do perfil da API
        const response = await api.get(`/api/usuarios/${user.id}`);
        const perfilData: PerfilData = {
          id: response.data.id,
          nome: response.data.nome,
          email: response.data.email,
          role: response.data.role,
          dataCriacao: new Date(response.data.criadoEm).toLocaleDateString('pt-BR'),
          ultimoAcesso: new Date().toLocaleString(),
        };
        setPerfil(perfilData);
        setDadosEdicao({
          nome: perfilData.nome,
          email: perfilData.email,
        });
      }
    } catch (err) {
      error('Erro', 'Erro ao carregar perfil');
    } finally {
      setCarregando(false);
    }
  };

  const handleSalvarPerfil = async () => {
    try {
      setCarregando(true);
      
      await api.put(`/api/usuarios/${perfil?.id}`, {
        nome: dadosEdicao.nome,
        email: dadosEdicao.email
      });
      
      if (perfil) {
        setPerfil({
          ...perfil,
          nome: dadosEdicao.nome,
          email: dadosEdicao.email,
        });
      }
      
      setEditando(false);
      success('Sucesso', 'Perfil atualizado com sucesso');
    } catch (err) {
      error('Erro', 'Erro ao atualizar perfil');
    } finally {
      setCarregando(false);
    }
  };

  const handleCancelarEdicao = () => {
    if (perfil) {
      setDadosEdicao({
        nome: perfil.nome,
        email: perfil.email,
      });
    }
    setEditando(false);
  };

  const handleAlterarSenha = async () => {
    if (dadosSenha.novaSenha !== dadosSenha.confirmarSenha) {
      error('Erro', 'Nova senha e confirmação não coincidem');
      return;
    }

    if (dadosSenha.novaSenha.length < 6) {
      error('Erro', 'Nova senha deve ter pelo menos 6 caracteres');
      return;
    }

    try {
      setCarregando(true);
      
      await api.post('/api/auth/alterar-senha', {
        senhaAtual: dadosSenha.senhaAtual,
        novaSenha: dadosSenha.novaSenha,
      });
      
      setDialogSenha(false);
      setDadosSenha({
        senhaAtual: '',
        novaSenha: '',
        confirmarSenha: '',
      });
      success('Sucesso', 'Senha alterada com sucesso');
    } catch (err) {
      error('Erro', 'Erro ao alterar senha. Verifique a senha atual.');
    } finally {
      setCarregando(false);
    }
  };

  const toggleMostrarSenha = (campo: keyof typeof mostrarSenhas) => {
    setMostrarSenhas(prev => ({
      ...prev,
      [campo]: !prev[campo],
    }));
  };

  if (!perfil) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <Typography>Carregando perfil...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom fontWeight="bold">
        Meu Perfil
      </Typography>

      <Grid container spacing={3}>
        {/* Informações Principais */}
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">Informações Pessoais</Typography>
              {!editando ? (
                <Button
                  startIcon={<Edit />}
                  onClick={() => setEditando(true)}
                  variant="outlined"
                  size="small"
                >
                  Editar
                </Button>
              ) : (
                <Box>
                  <Button
                    startIcon={<Save />}
                    onClick={handleSalvarPerfil}
                    variant="contained"
                    size="small"
                    sx={{ mr: 1 }}
                    disabled={carregando}
                  >
                    Salvar
                  </Button>
                  <Button
                    startIcon={<Cancel />}
                    onClick={handleCancelarEdicao}
                    variant="outlined"
                    size="small"
                  >
                    Cancelar
                  </Button>
                </Box>
              )}
            </Box>

            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Nome"
                  value={dadosEdicao.nome}
                  onChange={(e) => setDadosEdicao(prev => ({ ...prev, nome: e.target.value }))}
                  disabled={!editando}
                  InputProps={{
                    startAdornment: <Person sx={{ mr: 1, color: 'text.secondary' }} />,
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="E-mail"
                  type="email"
                  value={dadosEdicao.email}
                  onChange={(e) => setDadosEdicao(prev => ({ ...prev, email: e.target.value }))}
                  disabled={!editando}
                  InputProps={{
                    startAdornment: <Email sx={{ mr: 1, color: 'text.secondary' }} />,
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Perfil"
                  value={perfil.role}
                  disabled
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Data de Criação"
                  value={perfil.dataCriacao}
                  disabled
                />
              </Grid>
            </Grid>

            <Divider sx={{ my: 3 }} />

            <Box>
              <Typography variant="h6" gutterBottom>
                Segurança
              </Typography>
              <Button
                startIcon={<Lock />}
                onClick={() => setDialogSenha(true)}
                variant="outlined"
              >
                Alterar Senha
              </Button>
            </Box>
          </Paper>
        </Grid>

        {/* Sidebar */}
        <Grid item xs={12} md={4}>
          {/* Avatar e Info Rápida */}
          <Card sx={{ mb: 3 }}>
            <CardContent sx={{ textAlign: 'center' }}>
              <Avatar
                sx={{ 
                  width: 80, 
                  height: 80, 
                  mx: 'auto', 
                  mb: 2,
                  bgcolor: 'primary.main',
                  fontSize: '2rem'
                }}
              >
                {perfil.nome.charAt(0).toUpperCase()}
              </Avatar>
              <Typography variant="h6">{perfil.nome}</Typography>
              <Typography variant="body2" color="text.secondary">
                {perfil.role}
              </Typography>
              <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                Último acesso: {perfil.ultimoAcesso}
              </Typography>
            </CardContent>
          </Card>

          {/* Últimas Ações */}
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                <History sx={{ mr: 1, verticalAlign: 'middle' }} />
                Últimas Ações
              </Typography>
              {ultimasAcoes.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  Nenhuma ação recente registrada
                </Typography>
              ) : (
                <List dense>
                  {ultimasAcoes.map((acao, index) => (
                    <ListItem key={index} divider={index < ultimasAcoes.length - 1}>
                      <ListItemText
                        primary={acao.acao}
                        secondary={acao.data}
                        primaryTypographyProps={{ variant: 'body2' }}
                        secondaryTypographyProps={{ variant: 'caption' }}
                      />
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Dialog Alterar Senha */}
      <Dialog open={dialogSenha} onClose={() => setDialogSenha(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Alterar Senha</DialogTitle>
        <DialogContent>
          <Alert severity="info" sx={{ mb: 2 }}>
            A nova senha deve ter pelo menos 6 caracteres.
          </Alert>
          
          <TextField
            fullWidth
            label="Senha Atual"
            type={mostrarSenhas.atual ? 'text' : 'password'}
            value={dadosSenha.senhaAtual}
            onChange={(e) => setDadosSenha(prev => ({ ...prev, senhaAtual: e.target.value }))}
            sx={{ mb: 2 }}
            InputProps={{
              endAdornment: (
                <IconButton onClick={() => toggleMostrarSenha('atual')} edge="end">
                  {mostrarSenhas.atual ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              ),
            }}
          />
          
          <TextField
            fullWidth
            label="Nova Senha"
            type={mostrarSenhas.nova ? 'text' : 'password'}
            value={dadosSenha.novaSenha}
            onChange={(e) => setDadosSenha(prev => ({ ...prev, novaSenha: e.target.value }))}
            sx={{ mb: 2 }}
            InputProps={{
              endAdornment: (
                <IconButton onClick={() => toggleMostrarSenha('nova')} edge="end">
                  {mostrarSenhas.nova ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              ),
            }}
          />
          
          <TextField
            fullWidth
            label="Confirmar Nova Senha"
            type={mostrarSenhas.confirmar ? 'text' : 'password'}
            value={dadosSenha.confirmarSenha}
            onChange={(e) => setDadosSenha(prev => ({ ...prev, confirmarSenha: e.target.value }))}
            InputProps={{
              endAdornment: (
                <IconButton onClick={() => toggleMostrarSenha('confirmar')} edge="end">
                  {mostrarSenhas.confirmar ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              ),
            }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogSenha(false)}>
            Cancelar
          </Button>
          <Button 
            onClick={handleAlterarSenha} 
            variant="contained"
            disabled={carregando || !dadosSenha.senhaAtual || !dadosSenha.novaSenha || !dadosSenha.confirmarSenha}
          >
            Alterar Senha
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default PerfilPage;