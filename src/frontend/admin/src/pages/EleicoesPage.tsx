import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Collapse,
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
  TextField,
  InputLabel,
  Select,
  MenuItem,
  FormControl,
  InputAdornment,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions
} from '@mui/material';
import { Add, Edit, HowToVote, AttachFile, Assessment, Search, Clear, PlayArrow, Stop, CheckCircle, Cancel, HelpOutline, KeyboardArrowDown, KeyboardArrowUp } from '@mui/icons-material';
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

type DetalhesEleicao = {
  descricao?: string | null;
  inicioVotacao?: string;
  fimVotacao?: string;
  apenasAssociados?: boolean;
  apenasAtivos?: boolean;
  bancoNome?: string | null;
};

const EleicoesPage: React.FC = () => {
  const [itens, setItens] = useState<EleicaoResumoDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');
  const [filtroStatus, setFiltroStatus] = useState<string>('Todos');
  const [filtroTipo, setFiltroTipo] = useState<string>('Todos');
  const [alterandoStatusId, setAlterandoStatusId] = useState<string | null>(null);
  const [confirmacao, setConfirmacao] = useState<{ id: string; titulo: string; acao: string; novoStatus: number } | null>(null);
  const [expandidaId, setExpandidaId] = useState<string | null>(null);
  const [detalhesPorId, setDetalhesPorId] = useState<Record<string, DetalhesEleicao>>({});
  const [carregandoDetalhesId, setCarregandoDetalhesId] = useState<string | null>(null);
  const navigate = useNavigate();
  const toast = useToast();

  const abrirConfirmacao = (id: string, titulo: string, novoStatus: number) => {
    const acao =
      novoStatus === 2 ? 'Abrir votação' : novoStatus === 3 ? 'Encerrar votação' : novoStatus === 4 ? 'Apurar resultados' : 'Cancelar';
    setConfirmacao({ id, titulo, acao, novoStatus });
  };

  const fecharConfirmacao = () => setConfirmacao(null);

  const confirmarAlterarStatus = async () => {
    if (!confirmacao) return;
    const { id, acao, novoStatus } = confirmacao;
    setAlterandoStatusId(id);
    fecharConfirmacao();
    try {
      await eleicoesAPI.atualizarStatus(id, novoStatus);
      toast.success('Sucesso', `${acao} concluído.`);
      carregar();
    } catch {
      toast.error('Erro', `Não foi possível ${acao.toLowerCase()}.`);
    } finally {
      setAlterandoStatusId(null);
    }
  };

  const carregar = async () => {
    setLoading(true);
    try {
      const temFiltro = busca.trim() !== '' || filtroStatus !== 'Todos' || filtroTipo !== 'Todos';
      const data = temFiltro
        ? await eleicoesAPI.listar(1, 50, {
            busca: busca.trim(),
            status: filtroStatus !== 'Todos' ? parseInt(filtroStatus) : undefined,
            tipo: filtroTipo !== 'Todos' ? parseInt(filtroTipo) : undefined
          })
        : await eleicoesAPI.listar(1, 50);
      const raw = data.itens ?? [];
      setItens(
        raw.map((r: Record<string, unknown>) => ({
          id: r.id as string,
          titulo: (r.titulo ?? r.Titulo) as string,
          tipo: (r.tipo ?? r.Tipo) as number,
          status: (r.status ?? r.Status) as StatusEleicaoVal,
          arquivoAnexo: (r.arquivoAnexo ?? r.ArquivoAnexo) as string | null,
          bancoNome: (r.bancoNome ?? r.BancoNome) as string | null | undefined,
          inicioVotacao: (r.inicioVotacao ?? r.InicioVotacao) as string,
          fimVotacao: (r.fimVotacao ?? r.FimVotacao) as string,
          totalPerguntas: (r.totalPerguntas ?? r.TotalPerguntas) as number,
          totalVotos: (r.totalVotos ?? r.TotalVotos) as number,
        }))
      );
      setTotal(data.total ?? 0);
    } catch {
      toast.error('Erro', 'Erro ao carregar enquetes. Verifique se a API está rodando.');
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

  const formatDateTime = (s?: string) => {
    if (!s) return '—';
    try {
      return new Date(s).toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return s;
    }
  };

  const nomeArquivoAnexo = (arquivoAnexo?: string | null) => {
    if (!arquivoAnexo) return null;
    const partes = arquivoAnexo.split('/');
    return partes[partes.length - 1] || arquivoAnexo;
  };

  const toggleExpandir = async (id: string) => {
    if (expandidaId === id) {
      setExpandidaId(null);
      return;
    }

    setExpandidaId(id);
    if (detalhesPorId[id] || carregandoDetalhesId === id) return;

    try {
      setCarregandoDetalhesId(id);
      const r = await eleicoesAPI.obter(id);
      const detalhes: DetalhesEleicao = {
        descricao: (r.descricao ?? r.Descricao ?? '') as string,
        inicioVotacao: (r.inicioVotacao ?? r.InicioVotacao) as string,
        fimVotacao: (r.fimVotacao ?? r.FimVotacao) as string,
        apenasAssociados: (r.apenasAssociados ?? r.ApenasAssociados) as boolean,
        apenasAtivos: (r.apenasAtivos ?? r.ApenasAtivos) as boolean,
        bancoNome: (r.bancoNome ?? r.BancoNome) as string | null,
      };
      setDetalhesPorId((prev) => ({ ...prev, [id]: detalhes }));
    } catch {
      toast.error('Erro', 'Não foi possível carregar os detalhes da enquete.');
    } finally {
      setCarregandoDetalhesId(null);
    }
  };

  return (
    <Box sx={{ width: '100%' }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4">Enquetes</Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => navigate('/eleicoes/novo')}
        >
          Nova enquete
        </Button>
      </Box>

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField
          size="small"
          placeholder="Buscar por título..."
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && carregar()}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search fontSize="small" />
              </InputAdornment>
            ),
          }}
          sx={{ minWidth: 250 }}
        />
        
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Tipo</InputLabel>
          <Select
            value={filtroTipo}
            label="Tipo"
            onChange={(e) => setFiltroTipo(e.target.value)}
          >
            <MenuItem value="Todos">Todos</MenuItem>
            <MenuItem value="1">Enquete</MenuItem>
            <MenuItem value="2">Eleição</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={filtroStatus}
            label="Status"
            onChange={(e) => setFiltroStatus(e.target.value)}
          >
            <MenuItem value="Todos">Todos</MenuItem>
            <MenuItem value="1">Rascunho</MenuItem>
            <MenuItem value="2">Aberta</MenuItem>
            <MenuItem value="3">Encerrada</MenuItem>
            <MenuItem value="4">Apurada</MenuItem>
            <MenuItem value="5">Cancelada</MenuItem>
          </Select>
        </FormControl>

        <Button 
          variant="contained" 
          onClick={carregar}
          startIcon={<Search />}
        >
          Filtrar
        </Button>

        <Button
          variant="outlined"
          onClick={() => {
            setBusca('');
            setFiltroStatus('Todos');
            setFiltroTipo('Todos');
            carregar();
          }}
          startIcon={<Clear />}
        >
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
                  <TableCell sx={{ width: 44 }} />
                  <TableCell>Título</TableCell>
                  <TableCell>Tipo</TableCell>
                  <TableCell>Status</TableCell>
                      <TableCell sx={{ minWidth: 100 }}>Banco</TableCell>
                  <TableCell align="center">Anexo</TableCell>
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
                    <TableCell colSpan={11} align="center">
                      Nenhuma enquete cadastrada.
                    </TableCell>
                  </TableRow>
                ) : (
                  itens.map((e) => (
                    <React.Fragment key={e.id}>
                      <TableRow hover>
                        <TableCell>
                          <IconButton size="small" onClick={() => toggleExpandir(e.id)}>
                            {expandidaId === e.id ? <KeyboardArrowUp fontSize="small" /> : <KeyboardArrowDown fontSize="small" />}
                          </IconButton>
                        </TableCell>
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
                        <TableCell sx={{ minWidth: 100 }}>
                          {e.bancoNome ? (
                            <Chip label={e.bancoNome} size="small" variant="outlined" color="primary" />
                          ) : (
                            <Typography variant="body2" color="text.secondary">—</Typography>
                          )}
                        </TableCell>
                        <TableCell align="center">
                          {e.arquivoAnexo ? (
                            <Chip
                              size="small"
                              color="primary"
                              variant="outlined"
                              icon={<AttachFile />}
                              label="Com anexo"
                            />
                          ) : (
                            <Typography variant="body2" color="text.secondary">—</Typography>
                          )}
                        </TableCell>
                        <TableCell>{formatDate(e.inicioVotacao)}</TableCell>
                        <TableCell>{formatDate(e.fimVotacao)}</TableCell>
                        <TableCell align="center">{e.totalPerguntas}</TableCell>
                        <TableCell align="center">{e.totalVotos}</TableCell>
                        <TableCell align="right">
                        {/* Rascunho: Abrir votação ou Cancelar */}
                        {e.status === 1 && (
                          <>
                            <Tooltip title="Abrir votação">
                              <IconButton
                                onClick={() => abrirConfirmacao(e.id, e.titulo, 2)}
                                size="small"
                                color="primary"
                                disabled={alterandoStatusId === e.id}
                                sx={{ mr: 0.5 }}
                              >
                                <PlayArrow />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Cancelar enquete">
                              <IconButton
                                onClick={() => abrirConfirmacao(e.id, e.titulo, 5)}
                                size="small"
                                color="error"
                                disabled={alterandoStatusId === e.id}
                                sx={{ mr: 1 }}
                              >
                                <Cancel />
                              </IconButton>
                            </Tooltip>
                          </>
                        )}
                        {/* Aberta: Encerrar votação */}
                        {e.status === 2 && (
                          <Tooltip title="Encerrar votação">
                            <IconButton
                              onClick={() => abrirConfirmacao(e.id, e.titulo, 3)}
                              size="small"
                              color="warning"
                              disabled={alterandoStatusId === e.id}
                              sx={{ mr: 1 }}
                            >
                              <Stop />
                            </IconButton>
                          </Tooltip>
                        )}
                        {/* Encerrada: Apurar resultados */}
                        {e.status === 3 && (
                          <Tooltip title="Apurar resultados">
                            <IconButton
                              onClick={() => abrirConfirmacao(e.id, e.titulo, 4)}
                              size="small"
                              color="success"
                              disabled={alterandoStatusId === e.id}
                              sx={{ mr: 1 }}
                            >
                              <CheckCircle />
                            </IconButton>
                          </Tooltip>
                        )}
                        {/* Encerrada ou Apurada: Ver resultados */}
                        {(e.status === 3 || e.status === 4) && (
                          <Tooltip title="Ver resultados">
                            <IconButton
                              onClick={() => navigate(`/eleicoes/${e.id}/resultados`)}
                              size="small"
                              color="primary"
                              sx={{ mr: 1 }}
                            >
                              <Assessment />
                            </IconButton>
                          </Tooltip>
                        )}
                        {/* Editar: somente em Rascunho */}
                        {e.status === 1 && (
                          <Tooltip title="Editar enquete">
                            <IconButton
                              onClick={() => navigate(`/eleicoes/${e.id}`)}
                              size="small"
                              disabled={alterandoStatusId === e.id}
                            >
                              <Edit />
                            </IconButton>
                          </Tooltip>
                        )}
                        </TableCell>
                      </TableRow>
                      <TableRow>
                        <TableCell colSpan={11} sx={{ p: 0, borderBottom: expandidaId === e.id ? undefined : 'none' }}>
                          <Collapse in={expandidaId === e.id} timeout="auto" unmountOnExit>
                            <Box sx={{ px: 3, py: 2, bgcolor: 'grey.50' }}>
                              {carregandoDetalhesId === e.id && !detalhesPorId[e.id] ? (
                                <Typography variant="body2" color="text.secondary">Carregando detalhes...</Typography>
                              ) : (
                                <>
                                  <Typography variant="subtitle2" sx={{ mb: 1 }}>Detalhes da enquete</Typography>
                                  <Typography variant="body2" color="text.secondary" sx={{ mb: 0.75 }}>
                                    <strong>Descrição:</strong> {detalhesPorId[e.id]?.descricao?.trim() || 'Sem descrição'}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary" sx={{ mb: 0.75 }}>
                                    <strong>Período (data e hora):</strong> {formatDateTime(detalhesPorId[e.id]?.inicioVotacao ?? e.inicioVotacao)} até {formatDateTime(detalhesPorId[e.id]?.fimVotacao ?? e.fimVotacao)}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary" sx={{ mb: 0.75 }}>
                                    <strong>Acesso:</strong> {detalhesPorId[e.id]?.apenasAssociados === false ? 'Também não associados' : 'Apenas associados'} | {detalhesPorId[e.id]?.apenasAtivos === false ? 'Ativos e inativos' : 'Apenas ativos'}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary" sx={{ mb: 0.75 }}>
                                    <strong>Anexo:</strong> {e.arquivoAnexo ? `Sim (${nomeArquivoAnexo(e.arquivoAnexo)})` : 'Não'}
                                  </Typography>
                                  <Typography variant="body2" color="text.secondary">
                                    <strong>Banco restrito:</strong> {detalhesPorId[e.id]?.bancoNome || e.bancoNome || 'Todos os bancos'}
                                  </Typography>
                                </>
                              )}
                            </Box>
                          </Collapse>
                        </TableCell>
                      </TableRow>
                    </React.Fragment>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
          {total > 0 && (
            <Box sx={{ px: 3, py: 2 }}>
              <Typography variant="body2" color="text.secondary">
                Total: {total} enquete(s)
              </Typography>
            </Box>
          )}
        </Paper>
      )}

      <Dialog
        open={!!confirmacao}
        onClose={fecharConfirmacao}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: 2, boxShadow: 4 } }}
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 0 }}>
          <HelpOutline color="primary" />
          Confirmar ação
        </DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <DialogContentText sx={{ fontSize: '1rem' }}>
            {confirmacao && (
              <>
                Tem certeza que deseja <strong>{confirmacao.acao}</strong> na enquete &quot;{confirmacao.titulo}&quot;?
              </>
            )}
          </DialogContentText>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 0, gap: 1 }}>
          <Button onClick={fecharConfirmacao} variant="outlined">
            Cancelar
          </Button>
          <Button onClick={confirmarAlterarStatus} variant="contained" color="primary">
            Confirmar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EleicoesPage;
