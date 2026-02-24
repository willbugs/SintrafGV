import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  TextField,
  Button,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Checkbox,
  FormControlLabel,
  Divider,
  IconButton,
  Card,
  CardContent,
  CircularProgress,
} from '@mui/material';
import { Save, ArrowBack, Add, Delete } from '@mui/icons-material';
import { eleicoesAPI } from '../services/api';
import { useToast } from '../contexts/ToastContext';
import { TipoEleicao, TipoPergunta, type TipoEleicaoVal, type TipoPerguntaVal } from '../types';

interface OpcaoForm {
  ordem: number;
  texto: string;
  descricao: string;
}

interface PerguntaForm {
  ordem: number;
  texto: string;
  descricao: string;
  tipo: TipoPerguntaVal;
  maxVotos: number | null;
  permiteBranco: boolean;
  opcoes: OpcaoForm[];
}

const EleicaoFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const isEdit = Boolean(id);

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  const [titulo, setTitulo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [tipo, setTipo] = useState<TipoEleicaoVal>(TipoEleicao.Enquete);
  const [inicioVotacao, setInicioVotacao] = useState('');
  const [fimVotacao, setFimVotacao] = useState('');
  const [apenasAssociados, setApenasAssociados] = useState(true);
  const [apenasAtivos, setApenasAtivos] = useState(true);
  const [perguntas, setPerguntas] = useState<PerguntaForm[]>([]);

  useEffect(() => {
    if (isEdit && id) {
      setLoading(true);
      eleicoesAPI.obter(id)
        .then((data: Record<string, unknown>) => {
          setTitulo((data.titulo ?? data.Titulo ?? '') as string);
          setDescricao((data.descricao ?? data.Descricao ?? '') as string);
          setTipo((data.tipo ?? data.Tipo ?? TipoEleicao.Enquete) as TipoEleicaoVal);
          const inicio = (data.inicioVotacao ?? data.InicioVotacao ?? '') as string;
          const fim = (data.fimVotacao ?? data.FimVotacao ?? '') as string;
          setInicioVotacao(inicio ? inicio.slice(0, 16) : '');
          setFimVotacao(fim ? fim.slice(0, 16) : '');
          setApenasAssociados((data.apenasAssociados ?? data.ApenasAssociados ?? true) as boolean);
          setApenasAtivos((data.apenasAtivos ?? data.ApenasAtivos ?? true) as boolean);
          const rawPerguntas = (data.perguntas ?? data.Perguntas ?? []) as Record<string, unknown>[];
          setPerguntas(rawPerguntas.map((p, idx) => ({
            ordem: (p.ordem ?? p.Ordem ?? idx + 1) as number,
            texto: (p.texto ?? p.Texto ?? '') as string,
            descricao: (p.descricao ?? p.Descricao ?? '') as string,
            tipo: (p.tipo ?? p.Tipo ?? TipoPergunta.UnicoVoto) as TipoPerguntaVal,
            maxVotos: (p.maxVotos ?? p.MaxVotos ?? null) as number | null,
            permiteBranco: (p.permiteBranco ?? p.PermiteBranco ?? true) as boolean,
            opcoes: ((p.opcoes ?? p.Opcoes ?? []) as Record<string, unknown>[]).map((o, oi) => ({
              ordem: (o.ordem ?? o.Ordem ?? oi + 1) as number,
              texto: (o.texto ?? o.Texto ?? '') as string,
              descricao: (o.descricao ?? o.Descricao ?? '') as string,
            })),
          })));
        })
        .catch(() => toast.error('Erro', 'Erro ao carregar eleição.'))
        .finally(() => setLoading(false));
    }
  }, [id, isEdit]);

  const addPergunta = () => {
    setPerguntas([...perguntas, {
      ordem: perguntas.length + 1,
      texto: '',
      descricao: '',
      tipo: TipoPergunta.UnicoVoto,
      maxVotos: null,
      permiteBranco: true,
      opcoes: [{ ordem: 1, texto: '', descricao: '' }, { ordem: 2, texto: '', descricao: '' }],
    }]);
  };

  const removePergunta = (idx: number) => {
    setPerguntas(perguntas.filter((_, i) => i !== idx).map((p, i) => ({ ...p, ordem: i + 1 })));
  };

  const updatePergunta = (idx: number, field: keyof PerguntaForm, value: unknown) => {
    setPerguntas(perguntas.map((p, i) => i === idx ? { ...p, [field]: value } : p));
  };

  const addOpcao = (pIdx: number) => {
    setPerguntas(perguntas.map((p, i) => i === pIdx ? {
      ...p,
      opcoes: [...p.opcoes, { ordem: p.opcoes.length + 1, texto: '', descricao: '' }]
    } : p));
  };

  const removeOpcao = (pIdx: number, oIdx: number) => {
    setPerguntas(perguntas.map((p, i) => i === pIdx ? {
      ...p,
      opcoes: p.opcoes.filter((_, oi) => oi !== oIdx).map((o, oi) => ({ ...o, ordem: oi + 1 }))
    } : p));
  };

  const updateOpcao = (pIdx: number, oIdx: number, field: keyof OpcaoForm, value: string | number) => {
    setPerguntas(perguntas.map((p, i) => i === pIdx ? {
      ...p,
      opcoes: p.opcoes.map((o, oi) => oi === oIdx ? { ...o, [field]: value } : o)
    } : p));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) {
      toast.warning('Atenção', 'Informe o título da eleição.');
      return;
    }
    if (!inicioVotacao || !fimVotacao) {
      toast.warning('Atenção', 'Informe as datas de início e fim.');
      return;
    }
    if (perguntas.length === 0) {
      toast.warning('Atenção', 'Adicione pelo menos uma pergunta.');
      return;
    }
    for (const p of perguntas) {
      if (!p.texto.trim()) {
        toast.warning('Atenção', 'Preencha o texto de todas as perguntas.');
        return;
      }
      if (p.opcoes.length < 2) {
        toast.warning('Atenção', `A pergunta "${p.texto}" precisa de pelo menos 2 opções.`);
        return;
      }
      for (const o of p.opcoes) {
        if (!o.texto.trim()) {
          toast.warning('Atenção', `Preencha o texto de todas as opções da pergunta "${p.texto}".`);
          return;
        }
      }
    }

    setSaving(true);
    try {
      const payload = {
        titulo,
        descricao,
        tipo,
        inicioVotacao: new Date(inicioVotacao).toISOString(),
        fimVotacao: new Date(fimVotacao).toISOString(),
        apenasAssociados,
        apenasAtivos,
        perguntas: perguntas.map(p => ({
          ordem: p.ordem,
          texto: p.texto,
          descricao: p.descricao,
          tipo: p.tipo,
          maxVotos: p.maxVotos,
          permiteBranco: p.permiteBranco,
          opcoes: p.opcoes.map(o => ({
            ordem: o.ordem,
            texto: o.texto,
            descricao: o.descricao,
          })),
        })),
      };
      if (isEdit && id) {
        await eleicoesAPI.atualizar(id, payload);
        toast.success('Sucesso', 'Eleição atualizada com sucesso.');
      } else {
        await eleicoesAPI.criar(payload);
        toast.success('Sucesso', 'Eleição criada com sucesso.');
      }
      navigate('/eleicoes');
    } catch {
      toast.error('Erro', 'Erro ao salvar eleição.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', my: 5 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%' }}>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IconButton onClick={() => navigate('/eleicoes')} sx={{ mr: 1 }}>
          <ArrowBack />
        </IconButton>
        <Typography variant="h4">{isEdit ? 'Editar Eleição' : 'Nova Eleição'}</Typography>
      </Box>

      <Paper sx={{ p: 4 }}>
        <form onSubmit={handleSubmit}>
          <Typography variant="h6" gutterBottom>Dados Básicos</Typography>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 8 }}>
              <TextField
                label="Título"
                fullWidth
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                required
              />
            </Grid>
            <Grid size={{ xs: 12, md: 4 }}>
              <FormControl fullWidth>
                <InputLabel>Tipo</InputLabel>
                <Select
                  value={tipo}
                  label="Tipo"
                  onChange={(e) => setTipo(e.target.value as TipoEleicaoVal)}
                >
                  <MenuItem value={TipoEleicao.Enquete}>Enquete</MenuItem>
                  <MenuItem value={TipoEleicao.Eleicao}>Eleição</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Descrição"
                fullWidth
                multiline
                rows={3}
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Início da Votação"
                type="datetime-local"
                fullWidth
                value={inicioVotacao}
                onChange={(e) => setInicioVotacao(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
                required
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Fim da Votação"
                type="datetime-local"
                fullWidth
                value={fimVotacao}
                onChange={(e) => setFimVotacao(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
                required
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <FormControlLabel
                control={<Checkbox checked={apenasAssociados} onChange={(e) => setApenasAssociados(e.target.checked)} />}
                label="Apenas associados podem votar"
              />
              <FormControlLabel
                control={<Checkbox checked={apenasAtivos} onChange={(e) => setApenasAtivos(e.target.checked)} />}
                label="Apenas cadastros ativos podem votar"
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 4 }} />

          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">Perguntas</Typography>
            <Button startIcon={<Add />} onClick={addPergunta} variant="outlined" size="small">
              Adicionar Pergunta
            </Button>
          </Box>

          {perguntas.length === 0 && (
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Nenhuma pergunta adicionada. Clique em "Adicionar Pergunta" acima.
            </Typography>
          )}

          {perguntas.map((p, pIdx) => (
            <Card key={pIdx} variant="outlined" sx={{ mb: 3 }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="subtitle1" fontWeight="bold">Pergunta {p.ordem}</Typography>
                  <IconButton onClick={() => removePergunta(pIdx)} color="error" size="small" title="Remover pergunta">
                    <Delete />
                  </IconButton>
                </Box>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, md: 8 }}>
                    <TextField
                      label="Texto da Pergunta"
                      fullWidth
                      value={p.texto}
                      onChange={(e) => updatePergunta(pIdx, 'texto', e.target.value)}
                      required
                    />
                  </Grid>
                  <Grid size={{ xs: 12, md: 4 }}>
                    <FormControl fullWidth>
                      <InputLabel>Tipo</InputLabel>
                      <Select
                        value={p.tipo}
                        label="Tipo"
                        onChange={(e) => updatePergunta(pIdx, 'tipo', e.target.value)}
                      >
                        <MenuItem value={TipoPergunta.UnicoVoto}>Único voto</MenuItem>
                        <MenuItem value={TipoPergunta.MultiploVoto}>Múltiplos votos</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid size={{ xs: 12 }}>
                    <FormControlLabel
                      control={<Checkbox checked={p.permiteBranco} onChange={(e) => updatePergunta(pIdx, 'permiteBranco', e.target.checked)} />}
                      label="Permitir voto em branco"
                    />
                  </Grid>
                </Grid>

                <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Opções de Resposta</Typography>
                {p.opcoes.map((o, oIdx) => (
                  <Box key={oIdx} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <Typography sx={{ minWidth: 30 }}>{o.ordem}.</Typography>
                    <TextField
                      size="small"
                      fullWidth
                      placeholder="Texto da opção"
                      value={o.texto}
                      onChange={(e) => updateOpcao(pIdx, oIdx, 'texto', e.target.value)}
                    />
                    <IconButton
                      onClick={() => removeOpcao(pIdx, oIdx)}
                      color="error"
                      size="small"
                      disabled={p.opcoes.length <= 2}
                      title="Remover opção"
                    >
                      <Delete fontSize="small" />
                    </IconButton>
                  </Box>
                ))}
                <Button size="small" startIcon={<Add />} onClick={() => addOpcao(pIdx)} sx={{ mt: 1 }}>
                  Adicionar Opção
                </Button>
              </CardContent>
            </Card>
          ))}

          <Divider sx={{ my: 4 }} />

          <Box sx={{ display: 'flex', gap: 2 }}>
            <Button
              type="submit"
              variant="contained"
              startIcon={<Save />}
              disabled={saving}
            >
              {saving ? 'Salvando...' : 'Salvar'}
            </Button>
            <Button
              variant="outlined"
              onClick={() => navigate('/eleicoes')}
              disabled={saving}
            >
              Cancelar
            </Button>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default EleicaoFormPage;
