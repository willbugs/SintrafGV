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
  Grid,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import { associadosAPI } from '../services/api';
import { useToast } from '../contexts/ToastContext';

const formatCpf = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 11);
  if (digits.length <= 3) return digits;
  if (digits.length <= 6) return `${digits.slice(0, 3)}.${digits.slice(3)}`;
  if (digits.length <= 9) return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`;
  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`;
};

const formatCep = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 8);
  if (digits.length <= 5) return digits;
  return `${digits.slice(0, 5)}-${digits.slice(5)}`;
};

const formatTelefone = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 10);
  if (digits.length <= 2) return digits.length ? `(${digits}` : '';
  if (digits.length <= 6) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
};

const formatCelular = (value: string): string => {
  const digits = value.replace(/\D/g, '').slice(0, 11);
  if (digits.length <= 2) return digits.length ? `(${digits}` : '';
  if (digits.length <= 7) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
};

interface ViaCepResponse {
  cep: string;
  logradouro: string;
  bairro: string;
  localidade: string;
  uf: string;
  erro?: boolean;
}

const buscarCep = async (cep: string): Promise<ViaCepResponse | null> => {
  const digits = cep.replace(/\D/g, '');
  if (digits.length !== 8) return null;
  
  try {
    const response = await fetch(`https://viacep.com.br/ws/${digits}/json/`);
    const data = await response.json();
    if (data.erro) return null;
    return data;
  } catch {
    return null;
  }
};

const AssociadoFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const toast = useToast();
  const isNew = !id;

  const [nome, setNome] = useState('');
  const [cpf, setCpf] = useState('');
  const [matriculaSindicato, setMatriculaSindicato] = useState('');
  const [matriculaBancaria, setMatriculaBancaria] = useState('');

  const [sexo, setSexo] = useState('');
  const [estadoCivil, setEstadoCivil] = useState('');
  const [dataNascimento, setDataNascimento] = useState('');
  const [naturalidade, setNaturalidade] = useState('');

  const [cep, setCep] = useState('');
  const [endereco, setEndereco] = useState('');
  const [complemento, setComplemento] = useState('');
  const [bairro, setBairro] = useState('');
  const [cidade, setCidade] = useState('');
  const [estado, setEstado] = useState('');

  const [banco, setBanco] = useState('');
  const [agencia, setAgencia] = useState('');
  const [codAgencia, setCodAgencia] = useState('');
  const [conta, setConta] = useState('');

  const [funcao, setFuncao] = useState('');
  const [ctps, setCtps] = useState('');
  const [serie, setSerie] = useState('');

  const [dataAdmissao, setDataAdmissao] = useState('');
  const [dataFiliacao, setDataFiliacao] = useState('');
  const [dataDesligamento, setDataDesligamento] = useState('');

  const [telefone, setTelefone] = useState('');
  const [celular, setCelular] = useState('');
  const [email, setEmail] = useState('');

  const [ativo, setAtivo] = useState(true);
  const [aposentado, setAposentado] = useState(false);

  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [buscandoCep, setBuscandoCep] = useState(false);

  useEffect(() => {
    if (!isNew && id) {
      setLoading(true);
      associadosAPI
        .obter(id)
        .then((a: Record<string, unknown>) => {
          setNome((a.nome as string) ?? '');
          setCpf(formatCpf((a.cpf as string) ?? ''));
          setMatriculaSindicato((a.matriculaSindicato as string) ?? '');
          setMatriculaBancaria((a.matriculaBancaria as string) ?? '');
          setSexo((a.sexo as string) ?? '');
          setEstadoCivil((a.estadoCivil as string) ?? '');
          setDataNascimento((a.dataNascimento as string)?.split('T')[0] ?? '');
          setNaturalidade((a.naturalidade as string) ?? '');
          setCep(formatCep((a.cep as string) ?? ''));
          setEndereco((a.endereco as string) ?? '');
          setComplemento((a.complemento as string) ?? '');
          setBairro((a.bairro as string) ?? '');
          setCidade((a.cidade as string) ?? '');
          setEstado((a.estado as string) ?? '');
          setBanco((a.banco as string) ?? '');
          setAgencia((a.agencia as string) ?? '');
          setCodAgencia((a.codAgencia as string) ?? '');
          setConta((a.conta as string) ?? '');
          setFuncao((a.funcao as string) ?? '');
          setCtps((a.ctps as string) ?? '');
          setSerie((a.serie as string) ?? '');
          setDataAdmissao((a.dataAdmissao as string)?.split('T')[0] ?? '');
          setDataFiliacao((a.dataFiliacao as string)?.split('T')[0] ?? '');
          setDataDesligamento((a.dataDesligamento as string)?.split('T')[0] ?? '');
          setTelefone(formatTelefone((a.telefone as string) ?? ''));
          setCelular(formatCelular((a.celular as string) ?? ''));
          setEmail((a.email as string) ?? '');
          setAtivo((a.ativo as boolean) ?? true);
          setAposentado((a.aposentado as boolean) ?? false);
        })
        .catch(() => toast.error('Erro', 'Associado não encontrado.'))
        .finally(() => setLoading(false));
    }
  }, [id, isNew, toast]);

  const handleCepBlur = async () => {
    const digits = cep.replace(/\D/g, '');
    if (digits.length !== 8) return;
    
    setBuscandoCep(true);
    const dados = await buscarCep(cep);
    setBuscandoCep(false);
    
    if (dados) {
      if (dados.logradouro) setEndereco(dados.logradouro);
      if (dados.bairro) setBairro(dados.bairro);
      if (dados.localidade) setCidade(dados.localidade);
      if (dados.uf) setEstado(dados.uf);
      toast.success('CEP encontrado', `${dados.localidade} - ${dados.uf}`);
    } else {
      toast.warning('CEP não encontrado', 'Verifique o CEP informado.');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!nome.trim() || !cpf.trim()) {
      toast.warning('Atenção', 'Nome e CPF são obrigatórios.');
      return;
    }

    setSaving(true);
    try {
      const payload = {
        nome: nome.trim(),
        cpf: cpf.replace(/\D/g, ''),
        matriculaSindicato: matriculaSindicato.trim() || null,
        matriculaBancaria: matriculaBancaria.trim() || null,
        sexo: sexo || null,
        estadoCivil: estadoCivil || null,
        dataNascimento: dataNascimento || null,
        naturalidade: naturalidade.trim() || null,
        cep: cep.replace(/\D/g, '') || null,
        endereco: endereco.trim() || null,
        complemento: complemento.trim() || null,
        bairro: bairro.trim() || null,
        cidade: cidade.trim() || null,
        estado: estado || null,
        banco: banco.trim() || null,
        agencia: agencia.trim() || null,
        codAgencia: codAgencia.trim() || null,
        conta: conta.trim() || null,
        funcao: funcao.trim() || null,
        ctps: ctps.trim() || null,
        serie: serie.trim() || null,
        dataAdmissao: dataAdmissao || null,
        dataFiliacao: dataFiliacao || null,
        dataDesligamento: dataDesligamento || null,
        telefone: telefone.replace(/\D/g, '') || null,
        celular: celular.replace(/\D/g, '') || null,
        email: email.trim() || null,
        ativo,
        aposentado,
      };

      if (isNew) {
        await associadosAPI.criar(payload);
        toast.success('Sucesso', 'Associado criado com sucesso.');
      } else if (id) {
        await associadosAPI.atualizar(id, payload);
        toast.success('Sucesso', 'Associado atualizado com sucesso.');
      }
      navigate('/associados');
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
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          {isNew ? 'Novo Associado' : 'Editar Associado'}
        </Typography>
      </Box>

      <Paper sx={{ p: 3 }}>
        <form onSubmit={handleSubmit}>
          <Typography variant="h6" gutterBottom>
            Dados Básicos
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Nome Completo"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                required
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="CPF"
                value={cpf}
                onChange={(e) => setCpf(formatCpf(e.target.value))}
                placeholder="000.000.000-00"
                required
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Matrícula Sindicato"
                value={matriculaSindicato}
                onChange={(e) => setMatriculaSindicato(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Matrícula Bancária"
                value={matriculaBancaria}
                onChange={(e) => setMatriculaBancaria(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Dados Pessoais
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Sexo</InputLabel>
                <Select value={sexo} label="Sexo" onChange={(e) => setSexo(e.target.value)}>
                  <MenuItem value="">-</MenuItem>
                  <MenuItem value="M">Masculino</MenuItem>
                  <MenuItem value="F">Feminino</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Estado Civil</InputLabel>
                <Select value={estadoCivil} label="Estado Civil" onChange={(e) => setEstadoCivil(e.target.value)}>
                  <MenuItem value="">-</MenuItem>
                  <MenuItem value="Solteiro">Solteiro(a)</MenuItem>
                  <MenuItem value="Casado">Casado(a)</MenuItem>
                  <MenuItem value="Divorciado">Divorciado(a)</MenuItem>
                  <MenuItem value="Viuvo">Viúvo(a)</MenuItem>
                  <MenuItem value="Uniao Estavel">União Estável</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Data de Nascimento"
                type="date"
                value={dataNascimento}
                onChange={(e) => setDataNascimento(e.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Naturalidade"
                value={naturalidade}
                onChange={(e) => setNaturalidade(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Endereço
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="CEP"
                value={cep}
                onChange={(e) => setCep(formatCep(e.target.value))}
                onBlur={handleCepBlur}
                placeholder="00000-000"
                helperText={buscandoCep ? "Buscando endereço..." : "Digite o CEP para buscar"}
                disabled={buscandoCep}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={5}>
              <TextField
                fullWidth
                label="Endereço"
                value={endereco}
                onChange={(e) => setEndereco(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Complemento"
                value={complemento}
                onChange={(e) => setComplemento(e.target.value)}
                placeholder="Apto, Bloco, Casa..."
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Bairro"
                value={bairro}
                onChange={(e) => setBairro(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Cidade"
                value={cidade}
                onChange={(e) => setCidade(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>UF</InputLabel>
                <Select value={estado} label="UF" onChange={(e) => setEstado(e.target.value)}>
                  <MenuItem value="">-</MenuItem>
                  {['AC','AL','AP','AM','BA','CE','DF','ES','GO','MA','MT','MS','MG','PA','PB','PR','PE','PI','RJ','RN','RS','RO','RR','SC','SP','SE','TO'].map(uf => (
                    <MenuItem key={uf} value={uf}>{uf}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Dados Bancários
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Banco"
                value={banco}
                onChange={(e) => setBanco(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Código Agência"
                value={codAgencia}
                onChange={(e) => setCodAgencia(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Agência"
                value={agencia}
                onChange={(e) => setAgencia(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                label="Conta"
                value={conta}
                onChange={(e) => setConta(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Dados Profissionais
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Função"
                value={funcao}
                onChange={(e) => setFuncao(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="CTPS"
                value={ctps}
                onChange={(e) => setCtps(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Série"
                value={serie}
                onChange={(e) => setSerie(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Datas Importantes
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Data de Admissão"
                type="date"
                value={dataAdmissao}
                onChange={(e) => setDataAdmissao(e.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Data de Filiação"
                type="date"
                value={dataFiliacao}
                onChange={(e) => setDataFiliacao(e.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Data de Desligamento"
                type="date"
                value={dataDesligamento}
                onChange={(e) => setDataDesligamento(e.target.value)}
                InputLabelProps={{ shrink: true }}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Contato
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Telefone"
                value={telefone}
                onChange={(e) => setTelefone(formatTelefone(e.target.value))}
                placeholder="(00) 0000-0000"
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Celular"
                value={celular}
                onChange={(e) => setCelular(formatCelular(e.target.value))}
                placeholder="(00) 00000-0000"
                sx={{ mb: 2 }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="E-mail"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                sx={{ mb: 2 }}
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="h6" gutterBottom>
            Status
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 3, mb: 2 }}>
            <FormControlLabel
              control={<Checkbox checked={ativo} onChange={(e) => setAtivo(e.target.checked)} />}
              label="Cadastro Ativo (pode votar)"
            />
            <FormControlLabel
              control={<Checkbox checked={aposentado} onChange={(e) => setAposentado(e.target.checked)} />}
              label="Aposentado"
            />
          </Box>

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 3 }}>
            <Button onClick={() => navigate('/associados')} disabled={saving}>
              Cancelar
            </Button>
            <Button type="submit" variant="contained" disabled={saving}>
              {saving ? <CircularProgress size={24} /> : isNew ? 'Criar Associado' : 'Salvar Alterações'}
            </Button>
          </Box>
        </form>
      </Paper>
    </Box>
  );
};

export default AssociadoFormPage;
