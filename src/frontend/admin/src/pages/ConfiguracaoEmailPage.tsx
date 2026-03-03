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
  FormControlLabel,
  Switch,
} from '@mui/material';
import { Email as EmailIcon, Send as SendIcon, Save as SaveIcon } from '@mui/icons-material';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';

interface ConfiguracaoEmail {
  id?: string;
  smtpHost: string;
  smtpPort: number;
  usarSsl: boolean;
  smtpUsuario: string;
  smtpSenha: string;
  emailRemetente: string;
  nomeRemetente: string;
  habilitado: boolean;
}

const ConfiguracaoEmailPage: React.FC = () => {
  const { showToast } = useToast();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testando, setTestando] = useState(false);
  const [emailTeste, setEmailTeste] = useState('');
  const [mensagemTeste, setMensagemTeste] = useState<{ tipo: 'success' | 'error' | 'info'; texto: string } | null>(null);
  const [config, setConfig] = useState<ConfiguracaoEmail>({
    smtpHost: '',
    smtpPort: 587,
    usarSsl: true,
    smtpUsuario: '',
    smtpSenha: '',
    emailRemetente: '',
    nomeRemetente: 'SintrafGV',
    habilitado: false,
  });

  useEffect(() => {
    carregar();
  }, []);

  const carregar = async () => {
    try {
      setLoading(true);
      const res = await api.get('/api/configuracao-email');
      const d = res.data;
      setConfig({
        smtpHost: d.smtpHost || '',
        smtpPort: d.smtpPort ?? 587,
        usarSsl: d.usarSsl ?? true,
        smtpUsuario: d.smtpUsuario || '',
        smtpSenha: d.smtpSenha || '',
        emailRemetente: d.emailRemetente || '',
        nomeRemetente: d.nomeRemetente || 'SintrafGV',
        habilitado: d.habilitado ?? false,
      });
    } catch {
      showToast('Erro ao carregar configuração de e-mail', 'error');
    } finally {
      setLoading(false);
    }
  };

  const salvar = async () => {
    try {
      setSaving(true);
      await api.post('/api/configuracao-email', config);
      showToast('Configuração salva com sucesso!', 'success');
    } catch {
      showToast('Erro ao salvar configuração', 'error');
    } finally {
      setSaving(false);
    }
  };

  const testar = async () => {
    setMensagemTeste(null);
    if (!emailTeste.trim()) {
      setMensagemTeste({ tipo: 'error', texto: 'Informe o e-mail para enviar o teste.' });
      return;
    }
    try {
      setTestando(true);
      const res = await api.post(`/api/configuracao-email/testar?destinatario=${encodeURIComponent(emailTeste.trim())}`, {}, { timeout: 90000 });
      const msg = res.data.mensagem ?? (res.data.sucesso ? 'Enviado.' : 'Falha.');
      setMensagemTeste({ tipo: res.data.sucesso ? 'success' : 'error', texto: msg });
    } catch (err: unknown) {
      const ax = err as { response?: { data?: { message?: string; mensagem?: string; detail?: string } }; message?: string };
      const msg = ax.response?.data?.mensagem ?? ax.response?.data?.message ?? ax.response?.data?.detail ?? ax.message ?? 'Erro ao testar envio. Verifique a conexão.';
      setMensagemTeste({ tipo: 'error', texto: msg });
    } finally {
      setTestando(false);
    }
  };

  const handleChange = (campo: keyof ConfiguracaoEmail) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement> | unknown) => {
    const ev = e as React.ChangeEvent<HTMLInputElement>;
    const value = ev.target.type === 'checkbox' ? (ev.target as HTMLInputElement).checked : ev.target.value;
    setConfig((prev) => ({ ...prev, [campo]: value }));
  };

  const handleSwitchChange = (campo: 'usarSsl' | 'habilitado') => (_e: unknown, checked: boolean) => {
    setConfig((prev) => ({ ...prev, [campo]: checked }));
  };

  const handleNumberChange = (campo: 'smtpPort') => (e: React.ChangeEvent<HTMLInputElement>) => {
    const v = parseInt(e.target.value, 10);
    if (!isNaN(v)) setConfig((prev) => ({ ...prev, [campo]: v }));
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4, display: 'flex', justifyContent: 'center' }}>
        <Typography>Carregando...</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper sx={{ p: 3 }}>
        <Box display="flex" alignItems="center" mb={3}>
          <EmailIcon sx={{ mr: 2, fontSize: 32, color: 'primary.main' }} />
          <Typography variant="h4" component="h1">
            Configuração de E-mail
          </Typography>
        </Box>

        <Alert severity="info" sx={{ mb: 3 }}>
          Configure o servidor SMTP para envio de e-mails ao cadastrar novos usuários. As credenciais serão enviadas por e-mail ao novo usuário.
        </Alert>

        <Card sx={{ mb: 3 }}>
          <CardHeader
            avatar={<EmailIcon color="primary" />}
            title="Servidor SMTP"
            subheader="Host, porta e autenticação"
          />
          <CardContent>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Servidor SMTP"
                  value={config.smtpHost}
                  onChange={handleChange('smtpHost')}
                  placeholder="smtp.gmail.com"
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  type="number"
                  label="Porta"
                  value={config.smtpPort}
                  onChange={handleNumberChange('smtpPort')}
                  inputProps={{ min: 1, max: 65535 }}
                />
              </Grid>
              <Grid item xs={12} md={3} sx={{ display: 'flex', alignItems: 'center' }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={config.usarSsl}
                      onChange={handleSwitchChange('usarSsl')}
                    />
                  }
                  label="Usar SSL/TLS"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Usuário SMTP"
                  value={config.smtpUsuario}
                  onChange={handleChange('smtpUsuario')}
                  placeholder="seu@email.com"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="password"
                  label="Senha SMTP"
                  value={config.smtpSenha}
                  onChange={handleChange('smtpSenha')}
                  placeholder="Senha ou senha de app"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  type="email"
                  label="E-mail remetente"
                  value={config.emailRemetente}
                  onChange={handleChange('emailRemetente')}
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Nome do remetente"
                  value={config.nomeRemetente}
                  onChange={handleChange('nomeRemetente')}
                  placeholder="SintrafGV"
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={config.habilitado}
                      onChange={handleSwitchChange('habilitado')}
                    />
                  }
                  label="Habilitar envio de e-mail ao cadastrar usuários"
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        <Box display="flex" flexWrap="wrap" gap={2} alignItems="center">
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={salvar}
            disabled={saving}
          >
            {saving ? 'Salvando...' : 'Salvar Configuração'}
          </Button>
          <TextField
            label="E-mail para teste"
            type="email"
            value={emailTeste}
            onChange={(e) => setEmailTeste(e.target.value)}
            placeholder="email@exemplo.com"
            size="small"
            sx={{ minWidth: 260 }}
          />
          <Button
            variant="outlined"
            startIcon={<SendIcon />}
            onClick={testar}
            disabled={testando || !config.smtpHost || !emailTeste.trim()}
          >
            {testando ? 'Enviando...' : 'Enviar E-mail de Teste'}
          </Button>
        </Box>

        {mensagemTeste && (
          <Alert
            severity={mensagemTeste.tipo}
            sx={{ mt: 2 }}
            onClose={() => setMensagemTeste(null)}
          >
            {mensagemTeste.texto}
          </Alert>
        )}
      </Paper>
    </Container>
  );
};

export default ConfiguracaoEmailPage;
