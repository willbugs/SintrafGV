import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  Grid,
  TextField,
  Button,
  Box,
  Alert,
  Card,
  CardContent,
  CardHeader,
  IconButton,
  Tooltip
} from '@mui/material';
import {
  Save as SaveIcon,
  Business as BusinessIcon,
  LocationOn as LocationIcon,
  Person as PersonIcon,
  Security as SecurityIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';

interface ConfiguracaoSindicato {
  id?: string;
  razaoSocial: string;
  nomeFantasia: string;
  cnpj: string;
  inscricaoEstadual: string;
  endereco: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  telefone: string;
  celular: string;
  email: string;
  website: string;
  presidente: string;
  cpfPresidente: string;
  secretario: string;
  cpfSecretario: string;
  textoAutenticacao: string;
  cartorioResponsavel: string;
  enderecoCartorio: string;
}

const ConfiguracaoSindicatoPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(false);
  const [configuracao, setConfiguracao] = useState<ConfiguracaoSindicato>({
    razaoSocial: '',
    nomeFantasia: '',
    cnpj: '',
    inscricaoEstadual: '',
    endereco: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    uf: '',
    cep: '',
    telefone: '',
    celular: '',
    email: '',
    website: '',
    presidente: '',
    cpfPresidente: '',
    secretario: '',
    cpfSecretario: '',
    textoAutenticacao: '',
    cartorioResponsavel: '',
    enderecoCartorio: ''
  });

  useEffect(() => {
    carregarConfiguracao();
  }, []);

  const carregarConfiguracao = async () => {
    try {
      setLoading(true);
      const response = await api.get('/api/configuracao-sindicato');
      
      // Converter campos null para string vazia para evitar warnings do React
      const data = response.data;
      const configLimpa = {
        razaoSocial: data.razaoSocial || '',
        nomeFantasia: data.nomeFantasia || '',
        cnpj: data.cnpj || '',
        inscricaoEstadual: data.inscricaoEstadual || '',
        endereco: data.endereco || '',
        numero: data.numero || '',
        complemento: data.complemento || '',
        bairro: data.bairro || '',
        cidade: data.cidade || '',
        uf: data.uf || '',
        cep: data.cep || '',
        telefone: data.telefone || '',
        celular: data.celular || '',
        email: data.email || '',
        website: data.website || '',
        presidente: data.presidente || '',
        cpfPresidente: data.cpfPresidente || '',
        secretario: data.secretario || '',
        cpfSecretario: data.cpfSecretario || '',
        textoAutenticacao: data.textoAutenticacao || '',
        cartorioResponsavel: data.cartorioResponsavel || '',
        enderecoCartorio: data.enderecoCartorio || ''
      };
      
      setConfiguracao(configLimpa);
    } catch (error: unknown) {
      const axiosError = error as { response?: { status: number } };
      if (axiosError.response?.status === 404) {
        showToast('Configuração não encontrada. Preencha os dados para criar.', 'info');
      } else {
        console.error('Erro ao carregar configuração:', error);
        showToast('Erro ao carregar configuração do sindicato', 'error');
      }
    } finally {
      setLoading(false);
    }
  };

  const salvarConfiguracao = async () => {
    try {
      setLoading(true);
      const response = await api.post('/api/configuracao-sindicato', configuracao);
      
      // Converter campos null para string vazia
      const data = response.data;
      const configLimpa = {
        razaoSocial: data.razaoSocial || '',
        nomeFantasia: data.nomeFantasia || '',
        cnpj: data.cnpj || '',
        inscricaoEstadual: data.inscricaoEstadual || '',
        endereco: data.endereco || '',
        numero: data.numero || '',
        complemento: data.complemento || '',
        bairro: data.bairro || '',
        cidade: data.cidade || '',
        uf: data.uf || '',
        cep: data.cep || '',
        telefone: data.telefone || '',
        celular: data.celular || '',
        email: data.email || '',
        website: data.website || '',
        presidente: data.presidente || '',
        cpfPresidente: data.cpfPresidente || '',
        secretario: data.secretario || '',
        cpfSecretario: data.cpfSecretario || '',
        textoAutenticacao: data.textoAutenticacao || '',
        cartorioResponsavel: data.cartorioResponsavel || '',
        enderecoCartorio: data.enderecoCartorio || ''
      };
      
      setConfiguracao(configLimpa);
      showToast('Configuração salva com sucesso!', 'success');
    } catch (error) {
      console.error('Erro ao salvar configuração:', error);
      showToast('Erro ao salvar configuração', 'error');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: keyof ConfiguracaoSindicato) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setConfiguracao(prev => ({
      ...prev,
      [field]: event.target.value
    }));
  };

  const formatCNPJ = (value: string | null | undefined) => {
    if (!value) return '';
    const numbers = value.replace(/\D/g, '');
    return numbers.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
  };

  const formatCPF = (value: string | null | undefined) => {
    if (!value) return '';
    const numbers = value.replace(/\D/g, '');
    return numbers.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  };

  const formatCEP = (value: string | null | undefined) => {
    if (!value) return '';
    const numbers = value.replace(/\D/g, '');
    return numbers.replace(/(\d{5})(\d{3})/, '$1-$2');
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper sx={{ p: 3 }}>
        <Box display="flex" alignItems="center" mb={3}>
          <BusinessIcon sx={{ mr: 2, fontSize: 32, color: 'primary.main' }} />
          <Typography variant="h4" component="h1">
            Configuração do Sindicato
          </Typography>
        </Box>

        <Alert severity="info" sx={{ mb: 3 }}>
          <Typography variant="body2">
            Estas informações são utilizadas nos relatórios oficiais de votação para autenticação cartorial.
            Certifique-se de que todos os dados estão corretos e atualizados.
          </Typography>
        </Alert>

        <Grid container spacing={3}>
          {/* Dados da Empresa */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                avatar={<BusinessIcon color="primary" />}
                title="Dados da Empresa"
                subheader="Informações básicas do sindicato"
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Razão Social"
                      value={configuracao.razaoSocial}
                      onChange={handleChange('razaoSocial')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Nome Fantasia"
                      value={configuracao.nomeFantasia}
                      onChange={handleChange('nomeFantasia')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="CNPJ"
                      value={formatCNPJ(configuracao.cnpj)}
                      onChange={(e) => setConfiguracao(prev => ({ ...prev, cnpj: e.target.value.replace(/\D/g, '') }))}
                      inputProps={{ maxLength: 18 }}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Inscrição Estadual"
                      value={configuracao.inscricaoEstadual}
                      onChange={handleChange('inscricaoEstadual')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Telefone"
                      value={configuracao.telefone}
                      onChange={handleChange('telefone')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Celular"
                      value={configuracao.celular}
                      onChange={handleChange('celular')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="E-mail"
                      type="email"
                      value={configuracao.email}
                      onChange={handleChange('email')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Website"
                      value={configuracao.website}
                      onChange={handleChange('website')}
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Endereço */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                avatar={<LocationIcon color="primary" />}
                title="Endereço"
                subheader="Endereço completo do sindicato"
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={8}>
                    <TextField
                      fullWidth
                      label="Endereço"
                      value={configuracao.endereco}
                      onChange={handleChange('endereco')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={2}>
                    <TextField
                      fullWidth
                      label="Número"
                      value={configuracao.numero}
                      onChange={handleChange('numero')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={2}>
                    <TextField
                      fullWidth
                      label="Complemento"
                      value={configuracao.complemento}
                      onChange={handleChange('complemento')}
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Bairro"
                      value={configuracao.bairro}
                      onChange={handleChange('bairro')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <TextField
                      fullWidth
                      label="Cidade"
                      value={configuracao.cidade}
                      onChange={handleChange('cidade')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={2}>
                    <TextField
                      fullWidth
                      label="UF"
                      value={configuracao.uf}
                      onChange={handleChange('uf')}
                      inputProps={{ maxLength: 2 }}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={2}>
                    <TextField
                      fullWidth
                      label="CEP"
                      value={formatCEP(configuracao.cep)}
                      onChange={(e) => setConfiguracao(prev => ({ ...prev, cep: e.target.value.replace(/\D/g, '') }))}
                      inputProps={{ maxLength: 9 }}
                      required
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Dirigentes */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                avatar={<PersonIcon color="primary" />}
                title="Dirigentes"
                subheader="Presidente e secretário do sindicato"
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Nome do Presidente"
                      value={configuracao.presidente}
                      onChange={handleChange('presidente')}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="CPF do Presidente"
                      value={formatCPF(configuracao.cpfPresidente)}
                      onChange={(e) => setConfiguracao(prev => ({ ...prev, cpfPresidente: e.target.value.replace(/\D/g, '') }))}
                      inputProps={{ maxLength: 14 }}
                      required
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Nome do Secretário"
                      value={configuracao.secretario}
                      onChange={handleChange('secretario')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="CPF do Secretário"
                      value={formatCPF(configuracao.cpfSecretario)}
                      onChange={(e) => setConfiguracao(prev => ({ ...prev, cpfSecretario: e.target.value.replace(/\D/g, '') }))}
                      inputProps={{ maxLength: 14 }}
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Autenticação Cartorial */}
          <Grid item xs={12}>
            <Card>
              <CardHeader
                avatar={<SecurityIcon color="primary" />}
                title="Autenticação Cartorial"
                subheader="Dados para validação de relatórios pelo cartório"
                action={
                  <Tooltip title="Estas informações são incluídas nos relatórios oficiais de votação">
                    <IconButton>
                      <InfoIcon />
                    </IconButton>
                  </Tooltip>
                }
              />
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Texto de Autenticação"
                      value={configuracao.textoAutenticacao}
                      onChange={handleChange('textoAutenticacao')}
                      multiline
                      rows={3}
                      placeholder="Este relatório contém os dados oficiais da votação/eleição realizada pelo sindicato..."
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Cartório Responsável"
                      value={configuracao.cartorioResponsavel}
                      onChange={handleChange('cartorioResponsavel')}
                    />
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Endereço do Cartório"
                      value={configuracao.enderecoCartorio}
                      onChange={handleChange('enderecoCartorio')}
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>

          {/* Botões de Ação */}
          <Grid item xs={12}>
            <Box display="flex" justifyContent="flex-end" gap={2}>
              <Button
                variant="outlined"
                onClick={carregarConfiguracao}
                disabled={loading}
              >
                Cancelar
              </Button>
              <Button
                variant="contained"
                startIcon={<SaveIcon />}
                onClick={salvarConfiguracao}
                disabled={loading}
              >
                {loading ? 'Salvando...' : 'Salvar Configuração'}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Paper>
    </Container>
  );
};

export default ConfiguracaoSindicatoPage;