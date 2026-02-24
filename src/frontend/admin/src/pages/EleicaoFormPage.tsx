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
import { Save, ArrowBack, Add, Delete, AttachFile, GetApp } from '@mui/icons-material';
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
  const [arquivoAnexo, setArquivoAnexo] = useState<string | null>(null);
  const [arquivoAnexoNome, setArquivoAnexoNome] = useState<string | null>(null);
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
          const anexo = (data.arquivoAnexo ?? data.ArquivoAnexo ?? null) as string | null;
          setArquivoAnexo(anexo);
          if (anexo) {
            // Extrai nome do arquivo da URL ou path
            const nomeArquivo = anexo.split('/').pop()?.split('\\').pop() || 'Documento anexo';
            setArquivoAnexoNome(nomeArquivo);
          }
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
        .catch(() => toast.error('Erro', 'Erro ao carregar enquete.'))
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
      opcoes: [{ ordem: 1, texto: '', descricao: '' }],
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

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Limitar tipos de arquivo (PDFs, DOCs, etc.)
    const allowedTypes = ['application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'];
    if (!allowedTypes.includes(file.type)) {
      toast.warning('Atenção', 'Apenas arquivos PDF, DOC e DOCX são permitidos.');
      return;
    }

    // Limitar tamanho (5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast.warning('Atenção', 'Arquivo deve ter no máximo 5MB.');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const base64 = e.target?.result as string;
      setArquivoAnexo(base64);
      setArquivoAnexoNome(file.name);
    };
    reader.readAsDataURL(file);
  };

  const removerArquivo = () => {
    setArquivoAnexo(null);
    setArquivoAnexoNome(null);
  };

  const downloadArquivo = () => {
    if (arquivoAnexo && arquivoAnexoNome) {
      const link = document.createElement('a');
      link.href = arquivoAnexo;
      link.download = arquivoAnexoNome;
      link.click();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) {
      toast.warning('Atenção', 'Informe o título da enquete.');
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
      if (p.opcoes.length < 1) {
        toast.warning('Atenção', `A pergunta "${p.texto}" precisa de pelo menos 1 opção.`);
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
        arquivoAnexo,
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
        toast.success('Sucesso', 'Enquete atualizada com sucesso.');
      } else {
        await eleicoesAPI.criar(payload);
        toast.success('Sucesso', 'Enquete criada com sucesso.');
      }
      navigate('/eleicoes');
    } catch {
      toast.error('Erro', 'Erro ao salvar enquete.');
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
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IconButton onClick={() => navigate('/eleicoes')} sx={{ mr: 1 }}>
          <ArrowBack />
        </IconButton>
        <Typography variant="h4">{isEdit ? 'Editar Enquete' : 'Nova Enquete'}</Typography>
      </Box>

      <Paper sx={{ p: 3 }}>
        <form onSubmit={handleSubmit}>
          <Typography variant="h6" gutterBottom>
            Dados Básicos
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={8}>
              <TextField
                label="Título"
                fullWidth
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                required
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <FormControl fullWidth sx={{ mb: 2 }}>
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
            <Grid item xs={12}>
              <TextField
                label="Descrição"
                fullWidth
                multiline
                rows={3}
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Regras de Acesso
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3, mb: 2 }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={apenasAssociados}
                  onChange={(e) => setApenasAssociados(e.target.checked)}
                />
              }
              label="Apenas associados podem votar"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={apenasAtivos}
                  onChange={(e) => setApenasAtivos(e.target.checked)}
                />
              }
              label="Apenas cadastros ativos podem votar"
            />
          </Box>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Documento Anexo (Opcional)
          </Typography>
          <Box sx={{ mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              <input
                accept=".pdf,.doc,.docx"
                style={{ display: 'none' }}
                id="arquivo-upload"
                type="file"
                onChange={handleFileUpload}
              />
              <label htmlFor="arquivo-upload">
                <Button
                  variant="outlined"
                  component="span"
                  startIcon={<AttachFile />}
                  size="medium"
                >
                  {arquivoAnexoNome ? 'Trocar arquivo' : 'Anexar arquivo'}
                </Button>
              </label>
              {arquivoAnexoNome && (
                <>
                  <Typography variant="body2" color="text.secondary">
                    {arquivoAnexoNome}
                  </Typography>
                  <IconButton
                    size="small"
                    color="primary"
                    onClick={downloadArquivo}
                    title="Baixar arquivo"
                  >
                    <GetApp />
                  </IconButton>
                  <IconButton
                    size="small"
                    color="error"
                    onClick={removerArquivo}
                    title="Remover arquivo"
                  >
                    <Delete />
                  </IconButton>
                </>
              )}
            </Box>
            <Typography variant="caption" color="text.secondary">
              Formatos aceitos: PDF, DOC, DOCX (até 5MB)
            </Typography>
          </Box>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Período de Votação
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField
                label="Início da Votação"
                type="datetime-local"
                fullWidth
                value={inicioVotacao}
                onChange={(e) => setInicioVotacao(e.target.value)}
                InputLabelProps={{ shrink: true }}
                required
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                label="Fim da Votação"
                type="datetime-local"
                fullWidth
                value={fimVotacao}
                onChange={(e) => setFimVotacao(e.target.value)}
                InputLabelProps={{ shrink: true }}
                required
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" gutterBottom>Perguntas</Typography>
            <Button startIcon={<Add />} onClick={addPergunta} variant="outlined" size="small">
              Adicionar Pergunta
            </Button>
          </Box>

          <Box sx={{ mb: 3 }}>
            {perguntas.length === 0 && (
              <Typography color="text.secondary" sx={{ fontStyle: 'italic', mb: 2 }}>
                Nenhuma pergunta adicionada. Clique em "Adicionar Pergunta" acima.
              </Typography>
            )}
            {perguntas.length > 0 && (
              <Typography variant="body2" color="primary" sx={{ mb: 2, p: 2, bgcolor: 'primary.50', borderRadius: 1 }}>
                💡 <strong>Flexibilidade total:</strong> Cada pergunta pode ter de 1 até quantas opções precisar (não há mais a limitação de apenas 2 opções do sistema anterior).
              </Typography>
            )}

            {perguntas.map((p, pIdx) => (
              <Card key={pIdx} sx={{ mb: 3, border: '1px solid #e0e0e0' }}>
                <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="subtitle1" fontWeight="bold">Pergunta {p.ordem}</Typography>
                  <IconButton onClick={() => removePergunta(pIdx)} color="error" size="small" title="Remover pergunta">
                    <Delete />
                  </IconButton>
                </Box>
                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        label="Texto da pergunta"
                        value={p.texto}
                        onChange={(e) => updatePergunta(pIdx, 'texto', e.target.value)}
                        required
                        sx={{ mb: 2 }}
                      />
                    </Grid>
                    <Grid item xs={12}>
                      <TextField
                        fullWidth
                        label="Descrição (opcional)"
                        multiline
                        rows={2}
                        value={p.descricao}
                        onChange={(e) => updatePergunta(pIdx, 'descricao', e.target.value)}
                        sx={{ mb: 2 }}
                      />
                    </Grid>
                    <Grid item xs={12} md={4}>
                      <FormControl fullWidth sx={{ mb: 2 }}>
                        <InputLabel>Tipo de Pergunta</InputLabel>
                        <Select
                          value={p.tipo}
                          label="Tipo de Pergunta"
                          onChange={(e) => updatePergunta(pIdx, 'tipo', e.target.value as TipoPerguntaVal)}
                        >
                          <MenuItem value={TipoPergunta.UnicoVoto}>Único voto</MenuItem>
                          <MenuItem value={TipoPergunta.MultiploVoto}>Múltiplos votos</MenuItem>
                        </Select>
                      </FormControl>
                    </Grid>
                    {p.tipo === TipoPergunta.MultiploVoto && (
                      <Grid item xs={12} md={4}>
                        <TextField
                          fullWidth
                          type="number"
                          label="Máximo de votos"
                          value={p.maxVotos ?? ''}
                          onChange={(e) => updatePergunta(pIdx, 'maxVotos', e.target.value ? parseInt(e.target.value) : null)}
                          inputProps={{ min: 1 }}
                          sx={{ mb: 2 }}
                        />
                      </Grid>
                    )}
                    <Grid item xs={12} md={4}>
                      <FormControlLabel
                        control={
                          <Checkbox
                            checked={p.permiteBranco}
                            onChange={(e) => updatePergunta(pIdx, 'permiteBranco', e.target.checked)}
                          />
                        }
                        label="Permitir voto em branco"
                      />
                    </Grid>
                  </Grid>

                  <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>
                    Opções de resposta:
                  </Typography>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                      {p.opcoes.length === 0 ? 'Nenhuma opção adicionada.' : `${p.opcoes.length} opção(ões) - adicione quantas precisar`}
                    </Typography>
                    <Button
                      size="small"
                      variant="outlined"
                      startIcon={<Add />}
                      onClick={() => addOpcao(pIdx)}
                    >
                      Adicionar opção
                    </Button>
                  </Box>
                  {p.opcoes.map((o, oIdx) => (
                    <Box key={oIdx} sx={{ border: '1px solid #ddd', borderRadius: 1, p: 2, mb: 2, backgroundColor: '#fafafa' }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                        <Typography variant="subtitle2">Opção {o.ordem}</Typography>
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => removeOpcao(pIdx, oIdx)}
                          disabled={p.opcoes.length <= 1}
                          title={p.opcoes.length <= 1 ? 'Deve ter pelo menos 1 opção' : 'Remover opção'}
                        >
                          <Delete />
                        </IconButton>
                      </Box>
                      <Grid container spacing={2}>
                        <Grid item xs={12} md={6}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Texto da opção"
                            value={o.texto}
                            onChange={(e) => updateOpcao(pIdx, oIdx, 'texto', e.target.value)}
                            required
                          />
                        </Grid>
                        <Grid item xs={12} md={6}>
                          <TextField
                            fullWidth
                            size="small"
                            label="Descrição (opcional)"
                            value={o.descricao}
                            onChange={(e) => updateOpcao(pIdx, oIdx, 'descricao', e.target.value)}
                          />
                        </Grid>
                      </Grid>
                    </Box>
                  ))}
                </CardContent>
              </Card>
            ))}
          </Box>

          <Divider sx={{ my: 3 }} />

          <Box sx={{ display: 'flex', gap: 2, pt: 2 }}>
            <Button
              type="submit"
              variant="contained"
              startIcon={<Save />}
              disabled={saving}
              size="large"
            >
              {saving ? <CircularProgress size={20} /> : 'Salvar'}
            </Button>
            <Button
              variant="outlined"
              onClick={() => navigate('/eleicoes')}
              disabled={saving}
              size="large"
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
