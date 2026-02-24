import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  CircularProgress,
  FormControlLabel,
  Checkbox,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import { usuariosAPI } from '../services/api';
import { useToast } from '../contexts/ToastContext';

const UsuarioFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const isNew = id === 'novo' || !id;

  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [role, setRole] = useState('Admin');
  const [ativo, setAtivo] = useState(true);
  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isNew && id) {
      setLoading(true);
      usuariosAPI
        .obter(id)
        .then((u: { nome: string; email: string; role: string; ativo: boolean }) => {
          setNome(u.nome);
          setEmail(u.email);
          setRole(u.role ?? 'Admin');
          setAtivo(u.ativo ?? true);
        })
        .catch(() => toast.error('Erro', 'Usuário não encontrado.'))
        .finally(() => setLoading(false));
    }
  }, [id, isNew, toast]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!nome.trim() || !email.trim()) {
      toast.warning('Atenção', 'Nome e e-mail são obrigatórios.');
      return;
    }
    if (isNew && !senha.trim()) {
      toast.warning('Atenção', 'Senha é obrigatória para novo usuário.');
      return;
    }
    setSaving(true);
    try {
      if (isNew) {
        await usuariosAPI.criar({ nome: nome.trim(), email: email.trim(), senha, role });
        toast.success('Sucesso', 'Usuário criado com sucesso.');
      } else if (id) {
        await usuariosAPI.atualizar(id, { nome: nome.trim(), email: email.trim(), role, ativo });
        toast.success('Sucesso', 'Usuário atualizado com sucesso.');
      }
      navigate('/usuarios');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error('Erro', msg || 'Erro ao salvar.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%' }}>
      <Typography variant="h4" fontWeight="bold" sx={{ mb: 1 }}>
        {isNew ? 'Novo Usuário' : 'Editar Usuário'}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
        Usuários acessam o painel com e-mail e senha. Sem matrícula nem dados de associado.
      </Typography>

      <Paper sx={{ p: 4, maxWidth: 600 }}>
        <form onSubmit={handleSubmit}>
          <TextField
            fullWidth
            label="Nome"
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            sx={{ mb: 3 }}
            required
          />
          <TextField
            fullWidth
            label="E-mail"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            sx={{ mb: 3 }}
            required
          />
          {isNew && (
            <TextField
              fullWidth
              label="Senha"
              type="password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              sx={{ mb: 3 }}
              required
              autoComplete="new-password"
            />
          )}
          <FormControl fullWidth sx={{ mb: 3 }}>
            <InputLabel>Perfil</InputLabel>
            <Select value={role} label="Perfil" onChange={(e) => setRole(e.target.value)}>
              <MenuItem value="Admin">Administrador</MenuItem>
              <MenuItem value="Usuario">Usuário</MenuItem>
            </Select>
          </FormControl>
          {!isNew && (
            <FormControlLabel
              control={<Checkbox checked={ativo} onChange={(e) => setAtivo(e.target.checked)} />}
              label="Ativo"
              sx={{ mb: 2, display: 'block' }}
            />
          )}
          <Box sx={{ display: 'flex', gap: 2, mt: 4 }}>
            <Button type="button" variant="outlined" onClick={() => navigate('/usuarios')} disabled={saving}>
              Cancelar
            </Button>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? <CircularProgress size={24} /> : 'Salvar'}
            </Button>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default UsuarioFormPage;
