import React, { useState, useEffect, useRef } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  CircularProgress,
  IconButton,
  Chip,
  Avatar,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment,
} from '@mui/material';
import { Add, Edit, Delete, Person, Email, Search, Clear } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { usuariosAPI, configuracaoEmailAPI, type UsuarioListItem } from '../services/api';
import { useToast } from '../contexts/ToastContext';

const getPerfilColor = (role: string): 'primary' | 'default' | 'error' => {
  if (role === 'Admin') return 'error';
  return 'primary';
};

const getStatusColor = (ativo: boolean): 'success' | 'default' | 'error' => {
  return ativo ? 'success' : 'default';
};

const PERFIL_OPCOES = [
  { value: 'Todos', label: 'Todos' },
  { value: 'Admin', label: 'Administrador' },
  { value: 'Usuario', label: 'Usuário' },
] as const;

const STATUS_OPCOES = [
  { value: 'Todos', label: 'Todos' },
  { value: 'Ativo', label: 'Ativo' },
  { value: 'Inativo', label: 'Inativo' },
] as const;

const UsuariosPage: React.FC = () => {
  const [itens, setItens] = useState<UsuarioListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [emailHabilitado, setEmailHabilitado] = useState(false);
  const [reenviandoId, setReenviandoId] = useState<string | null>(null);
  const [busca, setBusca] = useState('');
  const [filtroPerfil, setFiltroPerfil] = useState<string>('Todos');
  const [filtroStatus, setFiltroStatus] = useState<string>('Todos');
  const navigate = useNavigate();
  const toast = useToast();
  const refBusca = useRef(busca);
  const refPerfil = useRef(filtroPerfil);
  const refStatus = useRef(filtroStatus);
  refBusca.current = busca;
  refPerfil.current = filtroPerfil;
  refStatus.current = filtroStatus;

  const carregarStatusEmail = async () => {
    try {
      const { habilitado } = await configuracaoEmailAPI.obterStatus();
      setEmailHabilitado(habilitado);
    } catch {
      setEmailHabilitado(false);
    }
  };

  const handleReenviarSenha = async (id: string) => {
    setReenviandoId(id);
    try {
      await usuariosAPI.reenviarSenha(id);
      toast.success('Sucesso', 'Nova senha gerada e enviada por e-mail.');
    } catch {
      toast.error('Erro', 'Falha ao reenviar senha. Verifique a configuração de e-mail.');
    } finally {
      setReenviandoId(null);
    }
  };

  const ativoFromStatus = (s: string): boolean | null =>
    s === 'Ativo' ? true : s === 'Inativo' ? false : null;

  const carregar = async (
    buscaAtual: string,
    perfilAtual: string,
    statusAtual: string
  ) => {
    setLoading(true);
    try {
      const a = ativoFromStatus(statusAtual);
      const temFiltro =
        buscaAtual.trim() !== '' || perfilAtual !== 'Todos' || a !== null;
      const data = temFiltro
        ? await usuariosAPI.listar(1, 50, {
            busca: buscaAtual.trim() || undefined,
            role: perfilAtual !== 'Todos' ? perfilAtual : undefined,
            ativo: a,
          })
        : await usuariosAPI.listar(1, 50);
      setItens(data.itens ?? []);
      setTotal(data.total ?? 0);
    } catch {
      toast.error('Erro', 'Erro ao carregar usuários. Verifique se a API está rodando.');
      setItens([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregar(refBusca.current, refPerfil.current, refStatus.current);
    carregarStatusEmail();
    // eslint-disable-next-line react-hooks/exhaustive-deps -- apenas no mount
  }, []);

  const handleBuscar = () => {
    carregar(refBusca.current, refPerfil.current, refStatus.current);
  };

  const handleLimpar = () => {
    setBusca('');
    setFiltroPerfil('Todos');
    setFiltroStatus('Todos');
    setTimeout(() => carregar('', 'Todos', 'Todos'), 0);
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4">
          Gestão de Usuários
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => navigate('/usuarios/novo')}
        >
          Novo Usuário
        </Button>
      </Box>

      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2, flexWrap: 'wrap' }}>
        <TextField
          size="small"
          placeholder="Buscar por nome ou e-mail..."
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && (e.preventDefault(), handleBuscar())}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search fontSize="small" />
              </InputAdornment>
            ),
          }}
          sx={{ minWidth: 280 }}
        />
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Perfil</InputLabel>
          <Select
            value={filtroPerfil}
            label="Perfil"
            onChange={(e) => setFiltroPerfil(e.target.value)}
          >
            {PERFIL_OPCOES.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={filtroStatus}
            label="Status"
            onChange={(e) => setFiltroStatus(e.target.value)}
          >
            {STATUS_OPCOES.map((opt) => (
              <MenuItem key={opt.value} value={opt.value}>
                {opt.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Button variant="contained" onClick={handleBuscar} startIcon={<Search />}>
          Buscar
        </Button>
        <Button variant="outlined" onClick={handleLimpar} startIcon={<Clear />}>
          Limpar
        </Button>
      </Box>

      {loading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', my: 5 }}>
          <CircularProgress />
        </Box>
      )}

      {!loading && (
        <Paper sx={{ overflow: 'hidden', width: '100%' }}>
          <TableContainer sx={{ px: 3, py: 2 }}>
            <Table size="medium" sx={{ '& .MuiTableCell-root': { py: 1.5 } }}>
              <TableHead>
                <TableRow>
                  <TableCell>Usuário</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Perfil</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.map((u) => (
                  <TableRow key={u.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Avatar sx={{ mr: 2 }}>
                          <Person />
                        </Avatar>
                        <Typography variant="body2">
                          {u.nome}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>{u.email}</TableCell>
                    <TableCell>
                      <Chip
                        label={u.role}
                        color={getPerfilColor(u.role)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={u.ativo ? 'Ativo' : 'Inativo'}
                        color={getStatusColor(u.ativo)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        onClick={() => navigate(`/usuarios/${u.id}`)}
                        size="small"
                        title="Editar usuário"
                      >
                        <Edit />
                      </IconButton>
                      {emailHabilitado && (
                        <IconButton
                          onClick={() => handleReenviarSenha(u.id)}
                          size="small"
                          title="Reenviar senha por e-mail"
                          disabled={reenviandoId === u.id}
                        >
                          <Email />
                        </IconButton>
                      )}
                      <IconButton
                        color="error"
                        size="small"
                        title="Excluir usuário"
                        disabled
                      >
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          {total > 0 && (
            <Box sx={{ px: 3, py: 2 }}>
              <Typography variant="body2" color="text.secondary">
                Total: {total} usuário(s)
              </Typography>
            </Box>
          )}
        </Paper>
      )}
    </Box>
  );
};

export default UsuariosPage;
