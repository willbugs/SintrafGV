import type React from 'react';

export interface MenuItem {
  id: string;
  label: string;
  icon: React.ReactNode;
  path: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role?: string;
  phone?: string;
  lastLogin?: string;
  createdAt?: string;
}

export interface AuthContextType {
  user: User | null;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  loading: boolean;
  loginLoading: boolean;
  updateProfile?: (data: UpdateProfileData) => Promise<void>;
  changePassword?: (data: ChangePasswordData) => Promise<void>;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface UpdateProfileData {
  name: string;
  email: string;
  phone?: string;
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface Associado {
  id: string;
  nome: string;
  cpf: string;
  matriculaSindicato?: string;
  matriculaBancaria?: string;
  // Dados pessoais
  sexo?: string;
  estadoCivil?: string;
  dataNascimento?: string;
  naturalidade?: string;
  // Endereço
  cep?: string;
  endereco?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  // Dados bancários
  banco?: string;
  agencia?: string;
  codAgencia?: string;
  conta?: string;
  // Dados profissionais
  funcao?: string;
  ctps?: string;
  serie?: string;
  // Datas importantes
  dataAdmissao?: string;
  dataFiliacao?: string;
  dataDesligamento?: string;
  // Contato
  telefone?: string;
  celular?: string;
  email?: string;
  // Status
  ativo: boolean;
  aposentado: boolean;
  // Auditoria
  dataUltimaAtualizacao?: string;
  criadoEm: string;
}

export interface AssociadosListResponse {
  itens: Associado[];
  total: number;
}

// --- Eleições / Votações ---
export const TipoEleicao = { Enquete: 1, Eleicao: 2 } as const;
export const StatusEleicao = { Rascunho: 1, Aberta: 2, Encerrada: 3, Apurada: 4, Cancelada: 5 } as const;
export const TipoPergunta = { UnicoVoto: 1, MultiploVoto: 2 } as const;

export type TipoEleicaoVal = (typeof TipoEleicao)[keyof typeof TipoEleicao];
export type StatusEleicaoVal = (typeof StatusEleicao)[keyof typeof StatusEleicao];
export type TipoPerguntaVal = (typeof TipoPergunta)[keyof typeof TipoPergunta];

export interface OpcaoDto {
  id: string;
  perguntaId: string;
  ordem: number;
  texto: string;
  descricao?: string;
  foto?: string;
  associadoId?: string;
  associadoNome?: string;
}

export interface PerguntaDto {
  id: string;
  eleicaoId: string;
  ordem: number;
  texto: string;
  descricao?: string;
  tipo: TipoPerguntaVal;
  maxVotos?: number;
  permiteBranco: boolean;
  opcoes: OpcaoDto[];
}

export interface EleicaoDto {
  id: string;
  titulo: string;
  descricao?: string;
  arquivoAnexo?: string;
  inicioVotacao: string;
  fimVotacao: string;
  tipo: TipoEleicaoVal;
  status: StatusEleicaoVal;
  apenasAssociados: boolean;
  apenasAtivos: boolean;
  bancoId?: string;
  criadoEm: string;
  totalPerguntas: number;
  totalVotos: number;
  perguntas: PerguntaDto[];
}

export interface EleicaoResumoDto {
  id: string;
  titulo: string;
  tipo: TipoEleicaoVal;
  status: StatusEleicaoVal;
  inicioVotacao: string;
  fimVotacao: string;
  totalPerguntas: number;
  totalVotos: number;
}

export interface CreateOpcaoRequest {
  ordem: number;
  texto: string;
  descricao?: string;
  foto?: string;
  associadoId?: string;
}

export interface CreatePerguntaRequest {
  ordem: number;
  texto: string;
  descricao?: string;
  tipo: TipoPerguntaVal;
  maxVotos?: number;
  permiteBranco: boolean;
  opcoes: CreateOpcaoRequest[];
}

export interface CreateEleicaoRequest {
  titulo: string;
  descricao?: string;
  arquivoAnexo?: string;
  inicioVotacao: string;
  fimVotacao: string;
  tipo: TipoEleicaoVal;
  apenasAssociados: boolean;
  apenasAtivos: boolean;
  bancoId?: string;
  perguntas: CreatePerguntaRequest[];
}

export interface UpdateEleicaoRequest {
  titulo: string;
  descricao?: string;
  arquivoAnexo?: string;
  inicioVotacao: string;
  fimVotacao: string;
  tipo: TipoEleicaoVal;
  apenasAssociados: boolean;
  apenasAtivos: boolean;
  bancoId?: string;
}
