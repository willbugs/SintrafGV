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
import { Add, Edit, Person } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { associadosAPI } from '../services/api';
import type { Associado } from '../types';
import { useToast } from '../contexts/ToastContext';

const AssociadosPage: React.FC = () => {
  const [itens, setItens] = useState<Associado[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const toast = useToast();

  const carregar = async () => {
    setLoading(true);
    try {
      const data = await associadosAPI.listar(1, 50);
      setItens(data.itens ?? []);
      setTotal(data.total ?? 0);
    } catch {
      toast.error('Erro', 'Erro ao carregar associados. Verifique se a API está rodando.');
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
          Gestão de Associados
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => navigate('/associados/novo')}
        >
          Novo Associado
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
                  <TableCell>Associado</TableCell>
                  <TableCell>CPF</TableCell>
                  <TableCell>E-mail</TableCell>
                  <TableCell>Celular</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      Nenhum associado cadastrado.
                    </TableCell>
                  </TableRow>
                ) : (
                  itens.map((a) => (
                    <TableRow key={a.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          <Avatar sx={{ mr: 2 }}>
                            <Person />
                          </Avatar>
                          <Typography variant="body2">
                            {a.nome}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>{a.cpf}</TableCell>
                      <TableCell>{a.email ?? '—'}</TableCell>
                      <TableCell>{a.celular ?? '—'}</TableCell>
                      <TableCell>
                        <Chip
                          label={a.ativo ? 'Ativo' : 'Inativo'}
                          color={a.ativo ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          onClick={() => navigate(`/associados/${a.id}`)}
                          size="small"
                          title="Editar associado"
                        >
                          <Edit />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
          {total > 0 && (
            <Box sx={{ px: 3, py: 2 }}>
              <Typography variant="body2" color="text.secondary">
                Total: {total} associado(s)
              </Typography>
            </Box>
          )}
        </Paper>
      )}
    </Box>
  );
};

export default AssociadosPage;
