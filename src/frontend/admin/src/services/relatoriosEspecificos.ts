import { api } from './api';

// === TYPES PARA RELATÓRIOS ESPECÍFICOS ===

export interface InadimplenciaDto {
  associadoId: string;
  nome: string;
  cpf: string;
  matriculaSindicato: string;
  matriculaBancaria: string;
  nomeBanco: string;
  mesesAtraso: number;
  ultimoPagamento?: string;
  valorDevido: number;
  telefone: string;
  celular: string;
  email: string;
  dataFiliacao: string;
  statusAssociado: string;
}

export interface MovimentacaoMensalDto {
  ano: number;
  mes: number;
  mesNome: string;
  novasFiliacao: number;
  desligamentos: number;
  saldoMovimentacao: number;
  totalAtivosFinalizados: number;
  percentualCrescimento: number;
  novosFiliados: NovoFiliadoDto[];
  detalhesDesligamentos: DesligamentoDto[];
}

export interface NovoFiliadoDto {
  nome: string;
  cpf: string;
  dataFiliacao: string;
  nomeBanco: string;
  funcao: string;
}

export interface DesligamentoDto {
  nome: string;
  cpf: string;
  dataDesligamento: string;
  motivoDesligamento: string;
  nomeBanco: string;
  tempoFiliacao: string;
}

export interface ParticipacaoVotacaoDto {
  associadoId: string;
  nome: string;
  cpf: string;
  matriculaSindicato: string;
  nomeBanco: string;
  funcao: string;
  totalEleicoesDisponiveis: number;
  totalVotosRealizados: number;
  percentualParticipacao: number;
  ultimaVotacao?: string;
  ultimaEleicaoTitulo: string;
  statusAssociado: string;
  dataFiliacao: string;
}

export interface FaixaEtariaDto {
  faixaEtaria: string;
  idadeMinima: number;
  idadeMaxima: number;
  totalAssociados: number;
  associadosAtivos: number;
  associadosInativos: number;
  percentualTotal: number;
  idadeMedia: number;
  detalhes: AssociadoFaixaEtariaDto[];
}

export interface AssociadoFaixaEtariaDto {
  nome: string;
  cpf: string;
  dataNascimento: string;
  idade: number;
  nomeBanco: string;
  funcao: string;
  ativo: boolean;
  dataFiliacao: string;
}

export interface AposentadoPensionistaDto {
  associadoId: string;
  nome: string;
  cpf: string;
  matriculaSindicato: string;
  matriculaBancaria: string;
  nomeBanco: string;
  tipoBeneficio: 'Aposentado' | 'Pensionista' | 'AposentadoPensionista' | 'Ativo';
  tipoBeneficioDescricao: string;
  dataAposentadoria?: string;
  dataPensao?: string;
  ativo: boolean;
  statusAssociado: string;
  dataFiliacao: string;
  dataDesligamento?: string;
  idadeAtual: number;
  tempoContribuicao: string;
  telefone: string;
  email: string;
}

export interface RelatorioRequest {
  tipoRelatorio: string;
  skip?: number;
  take?: number;
  filtros?: Record<string, any>;
  ordenacao?: {
    campo: string;
    direcao: 'asc' | 'desc';
  };
  formatoExportacao?: 'pdf' | 'excel' | 'csv' | 'html';
}

export interface RelatorioResponse<T> {
  dados: T[];
  totalizadores: Record<string, any>;
  metadata: {
    titulo: string;
    subtitulo: string;
    tipoRelatorio: string;
    dataGeracao: string;
    totalRegistros: number;
    filtros: Record<string, any>;
  };
}

// === API CLIENT PARA RELATÓRIOS ESPECÍFICOS ===

const relatoriosEspecificosAPI = {
  // Relatório de Inadimplência
  async obterInadimplencia(request: RelatorioRequest): Promise<RelatorioResponse<InadimplenciaDto>> {
    const response = await api.post('/relatorios/inadimplencia', request);
    return response.data;
  },

  // Relatório de Movimentação Mensal
  async obterMovimentacaoMensal(request: RelatorioRequest): Promise<RelatorioResponse<MovimentacaoMensalDto>> {
    const response = await api.post('/relatorios/movimentacao-mensal', request);
    return response.data;
  },

  // Relatório de Participação em Votações
  async obterParticipacaoVotacao(request: RelatorioRequest): Promise<RelatorioResponse<ParticipacaoVotacaoDto>> {
    const response = await api.post('/relatorios/participacao-votacao', request);
    return response.data;
  },

  // Relatório de Faixa Etária
  async obterFaixaEtaria(request: RelatorioRequest): Promise<RelatorioResponse<FaixaEtariaDto>> {
    const response = await api.post('/relatorios/faixa-etaria', request);
    return response.data;
  },

  // Relatório de Aposentados e Pensionistas
  async obterAposentadosPensionistas(request: RelatorioRequest): Promise<RelatorioResponse<AposentadoPensionistaDto>> {
    const response = await api.post('/relatorios/aposentados-pensionistas', request);
    return response.data;
  },

  // Exportação de relatórios específicos
  async exportarRelatorio(request: RelatorioRequest): Promise<Blob> {
    const response = await api.post('/relatorios/exportar', request, {
      responseType: 'blob'
    });
    return response.data;
  }
};

export default relatoriosEspecificosAPI;