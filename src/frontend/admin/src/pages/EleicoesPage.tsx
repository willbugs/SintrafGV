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
} from '@mui/material';
import { Add, Edit, HowToVote } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { eleicoesAPI } from '../services/api';
import type { EleicaoResumoDto, StatusEleicaoVal } from '../types';
import { useToast } from '../contexts/ToastContext';

const statusLabel: Record<StatusEleicaoVal, string> = {
  1: 'Rascunho',
  2: 'Aberta',
  3: 'Encerrada',
  4: 'Apurada',
  5: 'Cancelada',
};

const statusColor: Record<StatusEleicaoVal, 'default' | 'primary' | 'success' | 'warning' | 'error'> = {
  1: 'default',
  2: 'primary',
  3: 'warning',
  4: 'success',
  5: 'error',
};

const tipoLabel: Record<number, string> = {
  1: 'Enquete',
  2: 'Eleição',
};

const EleicoesPage: React.FC = () => {
  const [itens, setItens] = useState<EleicaoResumoDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const toast = useToast();

  const carregar = async () => {
    setLoading(true);
    try {
      const data = await eleicoesAPI.listar(1, 50);
      const raw = data.itens ?? [];
      setItens(
        raw.map((r: Record<string, unknown>) => ({
          id: r.id as string,
          titulo: (r.titulo ?? r.Titulo) as string,
          tipo: (r.tipo ?? r.Tipo) as number,
          status: (r.status ?? r.Status) as StatusEleicaoVal,
          inicioVotacao: (r.inicioVotacao ?? r.InicioVotacao) as string,
          fimVotacao: (r.fimVotacao ?? r.FimVotacao) as string,
          totalPerguntas: (r.totalPerguntas ?? r.TotalPerguntas) as number,
          totalVotos: (r.totalVotos ?? r.TotalVotos) as number,
        }))
      );
      setTotal(data.total ?? 0);
    } catch {
      toast.error('Erro', 'Erro ao carregar eleições. Verifique se a API está rodando.');
      setItens([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregar();
  }, []);

  const formatDate = (s: string) => {
    if (!s) return '—';
    try {
      return new Date(s).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    } catch {
      return s;
    }
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4">Eleições e Enquetes</Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => navigate('/eleicoes/novo')}
        >
          Nova eleição
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
                  <TableCell>Título</TableCell>
                  <TableCell>Tipo</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Início</TableCell>
                  <TableCell>Fim</TableCell>
                  <TableCell align="center">Perguntas</TableCell>
                  <TableCell align="center">Votos</TableCell>
                  <TableCell align="right">Ações</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {itens.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      Nenhuma eleição cadastrada.
                    </TableCell>
                  </TableRow>
                ) : (
                  itens.map((e) => (
                    <TableRow key={e.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          <HowToVote sx={{ mr: 1, color: 'action.active' }} />
                          <Typography variant="body2">{e.titulo}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell>{tipoLabel[e.tipo] ?? e.tipo}</TableCell>
                      <TableCell>
                        <Chip
                          label={statusLabel[e.status] ?? '—'}
                          color={statusColor[e.status] ?? 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{formatDate(e.inicioVotacao)}</TableCell>
                      <TableCell>{formatDate(e.fimVotacao)}</TableCell>
                      <TableCell align="center">{e.totalPerguntas}</TableCell>
                      <TableCell align="center">{e.totalVotos}</TableCell>
                      <TableCell align="right">
                        <IconButton
                          onClick={() => navigate(`/eleicoes/${e.id}`)}
                          size="small"
                          title="Editar eleição"
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
                Total: {total} eleição(ões)
              </Typography>
            </Box>
          )}
        </Paper>
      )}
    </Box>
  );
};

export default EleicoesPage;
