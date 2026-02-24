import React, { useState, useEffect } from 'react';
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
} from '@mui/material';
import { Add, Edit, Delete, Person } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { usuariosAPI, type UsuarioListItem } from '../services/api';
import { useToast } from '../contexts/ToastContext';

const getPerfilColor = (role: string): 'primary' | 'default' | 'error' => {
  if (role === 'Admin') return 'error';
  return 'primary';
};

const getStatusColor = (ativo: boolean): 'success' | 'default' | 'error' => {
  return ativo ? 'success' : 'default';
};

const UsuariosPage: React.FC = () => {
  const [itens, setItens] = useState<UsuarioListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const toast = useToast();

  const carregar = async () => {
    setLoading(true);
    try {
      const data = await usuariosAPI.listar(1, 50);
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
    carregar();
  }, []);

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
