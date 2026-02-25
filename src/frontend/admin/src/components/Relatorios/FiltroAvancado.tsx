import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Grid,
  IconButton,
  Divider,
} from '@mui/material';
import {
  Add,
  Delete,
  Clear,
  FilterList,
} from '@mui/icons-material';
// import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import type { CampoRelatorio } from '../../services/relatorioService';

export interface FiltroItem {
  campo: string;
  operador: string;
  valor: any;
  valorAte?: any;
  condicao: 'and' | 'or';
}

interface FiltroAvancadoProps {
  campos: CampoRelatorio[];
  filtros: FiltroItem[];
  onChange: (filtros: FiltroItem[]) => void;
  onLimpar?: () => void;
}

const operadoresPorTipo = {
  string: [
    { value: 'eq', label: 'Igual a' },
    { value: 'ne', label: 'Diferente de' },
    { value: 'contains', label: 'Contém' },
    { value: 'startswith', label: 'Inicia com' },
    { value: 'endswith', label: 'Termina com' },
  ],
  number: [
    { value: 'eq', label: 'Igual a' },
    { value: 'ne', label: 'Diferente de' },
    { value: 'gt', label: 'Maior que' },
    { value: 'lt', label: 'Menor que' },
    { value: 'gte', label: 'Maior ou igual' },
    { value: 'lte', label: 'Menor ou igual' },
    { value: 'between', label: 'Entre' },
  ],
  date: [
    { value: 'eq', label: 'Igual a' },
    { value: 'ne', label: 'Diferente de' },
    { value: 'gt', label: 'Posterior a' },
    { value: 'lt', label: 'Anterior a' },
    { value: 'gte', label: 'A partir de' },
    { value: 'lte', label: 'Até' },
    { value: 'between', label: 'Entre' },
  ],
  boolean: [
    { value: 'eq', label: 'Igual a' },
    { value: 'ne', label: 'Diferente de' },
  ],
};

const FiltroAvancado: React.FC<FiltroAvancadoProps> = ({
  campos,
  filtros,
  onChange,
  onLimpar,
}) => {
  const [aberto, setAberto] = useState(false);

  const adicionarFiltro = () => {
    const novoFiltro: FiltroItem = {
      campo: campos[0]?.nome || '',
      operador: 'eq',
      valor: '',
      condicao: 'and',
    };
    onChange([...filtros, novoFiltro]);
  };

  const removerFiltro = (index: number) => {
    const novosFiltros = filtros.filter((_, i) => i !== index);
    onChange(novosFiltros);
  };

  const atualizarFiltro = (index: number, campo: keyof FiltroItem, valor: any) => {
    const novosFiltros = [...filtros];
    novosFiltros[index] = { ...novosFiltros[index], [campo]: valor };

    // Se mudou o campo, resetar operador e valor
    if (campo === 'campo') {
      const tipoCampo = campos.find(c => c.nome === valor)?.tipo || 'string';
      const primeiroOperador = operadoresPorTipo[tipoCampo][0]?.value;
      novosFiltros[index].operador = primeiroOperador;
      novosFiltros[index].valor = '';
      novosFiltros[index].valorAte = undefined;
    }

    onChange(novosFiltros);
  };

  const limparFiltros = () => {
    onChange([]);
    if (onLimpar) onLimpar();
  };

  const obterCampo = (nomeCampo: string) => {
    return campos.find(c => c.nome === nomeCampo);
  };

  const obterOperadores = (tipoCampo: string) => {
    return operadoresPorTipo[tipoCampo as keyof typeof operadoresPorTipo] || operadoresPorTipo.string;
  };

  const renderizarCampoValor = (filtro: FiltroItem, index: number) => {
    const campo = obterCampo(filtro.campo);
    if (!campo) return null;

    const isRange = filtro.operador === 'between';

    switch (campo.tipo) {
      case 'date':
        return (
          <Grid container spacing={1}>
            <Grid item xs={isRange ? 6 : 12}>
              <TextField
                label="Valor"
                type="date"
                size="small"
                fullWidth
                value={filtro.valor || ''}
                onChange={(e) => atualizarFiltro(index, 'valor', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            {isRange && (
              <Grid item xs={6}>
                <TextField
                  label="Até"
                  type="date"
                  size="small"
                  fullWidth
                  value={filtro.valorAte || ''}
                  onChange={(e) => atualizarFiltro(index, 'valorAte', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
            )}
          </Grid>
        );

      case 'boolean':
        return (
          <FormControl size="small" fullWidth>
            <InputLabel>Valor</InputLabel>
            <Select
              value={filtro.valor}
              onChange={(e) => atualizarFiltro(index, 'valor', e.target.value)}
              label="Valor"
            >
              <MenuItem value="true">Sim</MenuItem>
              <MenuItem value="false">Não</MenuItem>
            </Select>
          </FormControl>
        );

      case 'number':
        return (
          <Grid container spacing={1}>
            <Grid item xs={isRange ? 6 : 12}>
              <TextField
                size="small"
                fullWidth
                label="Valor"
                type="number"
                value={filtro.valor}
                onChange={(e) => atualizarFiltro(index, 'valor', Number(e.target.value))}
              />
            </Grid>
            {isRange && (
              <Grid item xs={6}>
                <TextField
                  size="small"
                  fullWidth
                  label="Até"
                  type="number"
                  value={filtro.valorAte || ''}
                  onChange={(e) => atualizarFiltro(index, 'valorAte', Number(e.target.value))}
                />
              </Grid>
            )}
          </Grid>
        );

      default:
        return (
          <TextField
            size="small"
            fullWidth
            label="Valor"
            value={filtro.valor}
            onChange={(e) => atualizarFiltro(index, 'valor', e.target.value)}
          />
        );
    }
  };

  if (!aberto) {
    return (
      <Box display="flex" alignItems="center" gap={2} mb={2}>
        <Button
          variant="outlined"
          startIcon={<FilterList />}
          onClick={() => setAberto(true)}
        >
          Filtros Avançados
        </Button>
        
        {filtros.length > 0 && (
          <Box display="flex" alignItems="center" gap={1}>
            <Typography variant="body2" color="text.secondary">
              {filtros.length} filtro{filtros.length > 1 ? 's' : ''} aplicado{filtros.length > 1 ? 's' : ''}
            </Typography>
            <Button size="small" onClick={limparFiltros}>
              Limpar
            </Button>
          </Box>
        )}
      </Box>
    );
  }

  return (
    <Paper sx={{ p: 2, mb: 2 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h6">Filtros Avançados</Typography>
        <Box>
          <Button
            size="small"
            onClick={limparFiltros}
            startIcon={<Clear />}
            sx={{ mr: 1 }}
          >
            Limpar
          </Button>
          <Button
            size="small"
            onClick={() => setAberto(false)}
          >
            Fechar
          </Button>
        </Box>
      </Box>

      <Divider sx={{ mb: 2 }} />

      {filtros.length === 0 && (
        <Box textAlign="center" py={2}>
          <Typography variant="body2" color="text.secondary" mb={2}>
            Nenhum filtro definido
          </Typography>
        </Box>
      )}

      {filtros.map((filtro, index) => {
        const campo = obterCampo(filtro.campo);
        const operadores = campo ? obterOperadores(campo.tipo) : [];

        return (
          <Box key={index} mb={2}>
            {index > 0 && (
              <Box display="flex" alignItems="center" mb={1}>
                <FormControl size="small" sx={{ minWidth: 80 }}>
                  <Select
                    value={filtro.condicao}
                    onChange={(e) => atualizarFiltro(index, 'condicao', e.target.value)}
                  >
                    <MenuItem value="and">E</MenuItem>
                    <MenuItem value="or">OU</MenuItem>
                  </Select>
                </FormControl>
              </Box>
            )}

            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} sm={3}>
                <FormControl size="small" fullWidth>
                  <InputLabel>Campo</InputLabel>
                  <Select
                    value={filtro.campo}
                    onChange={(e) => atualizarFiltro(index, 'campo', e.target.value)}
                    label="Campo"
                  >
                    {campos.filter(c => c.filtravel).map((campo) => (
                      <MenuItem key={campo.nome} value={campo.nome}>
                        {campo.titulo}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={3}>
                <FormControl size="small" fullWidth>
                  <InputLabel>Operador</InputLabel>
                  <Select
                    value={filtro.operador}
                    onChange={(e) => atualizarFiltro(index, 'operador', e.target.value)}
                    label="Operador"
                  >
                    {operadores.map((op) => (
                      <MenuItem key={op.value} value={op.value}>
                        {op.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={5}>
                {renderizarCampoValor(filtro, index)}
              </Grid>

              <Grid item xs={12} sm={1}>
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => removerFiltro(index)}
                >
                  <Delete />
                </IconButton>
              </Grid>
            </Grid>
          </Box>
        );
      })}

      <Button
        startIcon={<Add />}
        onClick={adicionarFiltro}
        disabled={campos.filter(c => c.filtravel).length === 0}
      >
        Adicionar Filtro
      </Button>
    </Paper>
  );
};

export default FiltroAvancado;