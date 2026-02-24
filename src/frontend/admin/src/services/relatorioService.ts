import api from './api';

export interface RelatorioRequest {
  tipoRelatorio: string;
  filtros: Record<string, any>;
  camposSelecionados?: string[];
  formatoExportacao?: 'html' | 'pdf' | 'excel' | 'csv';
  ordenacao?: string;
  ordenacaoDecrescente?: boolean;
  pagina?: number;
  tamanhoPagina?: number;
}

export interface RelatorioResponse<T> {
  dados: T[];
  metadata: RelatorioMetadata;
  totalizadores: Record<string, any>;
}

export interface RelatorioMetadata {
  titulo: string;
  subtitulo: string;
  dataGeracao: string;
  totalRegistros: number;
  paginaAtual?: number;
  totalPaginas?: number;
  filtrosAplicados: Record<string, any>;
  camposDisponiveis: CampoRelatorio[];
}

export interface CampoRelatorio {
  nome: string;
  titulo: string;
  tipo: 'string' | 'number' | 'date' | 'boolean';
  totalizavel: boolean;
  filtravel: boolean;
  ordenavel: boolean;
  formato?: string;
  mascara?: string;
}

export interface AssociadoRelatorio {
  id: string;
  nome: string;
  cpf: string;
  matriculaSindicato?: string;
  matriculaBancaria?: string;
  sexo?: string;
  estadoCivil?: string;
  endereco?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  naturalidade?: string;
  dataNascimento?: string;
  funcao?: string;
  dataAdmissao?: string;
  dataFiliacao?: string;
  dataDesligamento?: string;
  celular?: string;
  telefone?: string;
  email?: string;
  nomeBanco?: string;
  agencia?: string;
  conta?: string;
  ativo: boolean;
  associado: boolean;
  aposentado: boolean;
  motivo?: string;
  idade?: number;
  tempoServico?: number;
  tempoFiliacao?: number;
}

export interface DashboardKpi {
  totalAssociados: number;
  associadosAtivos: number;
  associadosInativos: number;
  novosMesAtual: number;
  desligadosMesAtual: number;
  enquetesAbertas: number;
  enquetesEncerradas: number;
  percentualCrescimento: number;
  graficoPorBanco: DashboardGrafico[];
  graficoPorIdade: DashboardGrafico[];
  graficoPorSexo: DashboardGrafico[];
  crescimentoMensal: DashboardGrafico[];
}

export interface DashboardGrafico {
  label: string;
  valor: number;
  cor?: string;
  metadata?: Record<string, any>;
}

class RelatorioService {
  // Dashboard
  async obterDashboardKpis(): Promise<DashboardKpi> {
    const response = await api.get('/relatorios/dashboard');
    return response.data;
  }

  // Relatórios de Associados
  async obterRelatorioAssociadosGeral(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/geral', request);
    return response.data;
  }

  async obterRelatorioAssociadosAtivos(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/ativos', request);
    return response.data;
  }

  async obterRelatorioAssociadosInativos(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/inativos', request);
    return response.data;
  }

  async obterRelatorioAniversariantes(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/aniversariantes', request);
    return response.data;
  }

  async obterRelatorioNovosAssociados(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/novos', request);
    return response.data;
  }

  async obterRelatorioPorSexo(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/por-sexo', request);
    return response.data;
  }

  async obterRelatorioPorBanco(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/por-banco', request);
    return response.data;
  }

  async obterRelatorioPorCidade(request: RelatorioRequest): Promise<RelatorioResponse<AssociadoRelatorio>> {
    const response = await api.post('/relatorios/associados/por-cidade', request);
    return response.data;
  }

  // Metadata
  async obterCamposDisponiveis(tipoRelatorio: string): Promise<CampoRelatorio[]> {
    const response = await api.get(`/relatorios/campos/${tipoRelatorio}`);
    return response.data;
  }

  async obterTiposRelatorio(): Promise<string[]> {
    const response = await api.get('/relatorios/tipos');
    return response.data;
  }

  // Exportação
  async exportarRelatorio(request: RelatorioRequest): Promise<Blob> {
    const response = await api.post('/relatorios/exportar', request, {
      responseType: 'blob'
    });
    return response.data;
  },

  // Métodos de exportação específicos para facilitar uso
  async exportarPdf(tipoRelatorio: string, filtros: any = {}): Promise<Blob> {
    return this.exportarRelatorio({
      tipoRelatorio,
      filtros,
      formatoExportacao: 'pdf'
    });
  },

  async exportarExcel(tipoRelatorio: string, filtros: any = {}): Promise<Blob> {
    return this.exportarRelatorio({
      tipoRelatorio,
      filtros,
      formatoExportacao: 'excel'
    });
  },

  async exportarCsv(tipoRelatorio: string, filtros: any = {}): Promise<Blob> {
    return this.exportarRelatorio({
      tipoRelatorio,
      filtros,
      formatoExportacao: 'csv'
    });
  }

  // Histórico
  async obterHistoricoRelatorios(limite = 10): Promise<any[]> {
    const response = await api.get(`/relatorios/historico?limite=${limite}`);
    return response.data;
  }
}

export default new RelatorioService();