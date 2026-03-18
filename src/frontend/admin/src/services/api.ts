import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5066';

export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('sintrafgv_token');
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const isLoginRequest = error.config?.url?.includes('/api/auth/login');
      if (!isLoginRequest) {
        localStorage.removeItem('sintrafgv_token');
        localStorage.removeItem('sintrafgv_user');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export const authAPI = {
  login: async (credentials: { email: string; password: string }) => {
    const response = await api.post('/api/auth/login', credentials);
    return response.data;
  },
  refreshToken: async () => {
    const response = await api.post('/api/auth/refresh-token');
    return { success: true, data: response.data, message: 'Token renovado' };
  },
  updateProfile: async (data: { name: string; email: string; phone?: string }) => {
    const response = await api.put('/api/auth/profile', data);
    return response.data;
  },
  changePassword: async (data: { currentPassword: string; newPassword: string }) => {
    const response = await api.post('/api/auth/change-password', data);
    return response.data;
  },
};

export interface UsuarioListItem {
  id: string;
  nome: string;
  email: string;
  role: string;
  ativo: boolean;
  criadoEm: string;
}

export const usuariosAPI = {
  listar: async (
    pagina = 1,
    porPagina = 20,
    filtros?: { busca?: string; role?: string; ativo?: boolean | null }
  ) => {
    const q = new URLSearchParams();
    q.set('pagina', String(pagina));
    q.set('porPagina', String(porPagina));
    q.set('_', String(Date.now()));
    if (filtros?.busca?.trim()) q.set('busca', filtros.busca.trim());
    if (filtros?.role?.trim()) q.set('role', filtros.role.trim());
    if (filtros?.ativo !== undefined && filtros?.ativo !== null)
      q.set('ativo', String(filtros.ativo));
    const response = await api.get(`/api/usuarios?${q.toString()}`);
    return response.data;
  },
  obter: async (id: string) => {
    const response = await api.get(`/api/usuarios/${id}`);
    return response.data;
  },
  criar: async (data: { nome: string; email: string; senha?: string; role: string }) => {
    const response = await api.post('/api/usuarios', {
      nome: data.nome,
      email: data.email,
      senha: data.senha ?? '',
      role: data.role,
    });
    return response.data;
  },
  atualizar: async (id: string, data: { nome: string; email: string; role: string; ativo: boolean }) => {
    const response = await api.put(`/api/usuarios/${id}`, {
      nome: data.nome,
      email: data.email,
      role: data.role,
      ativo: data.ativo,
    });
    return response.data;
  },
  reenviarSenha: async (id: string) => {
    const response = await api.post(`/api/usuarios/${id}/reenviar-senha`);
    return response.data;
  },
};

export const configuracaoEmailAPI = {
  obterStatus: async (): Promise<{ habilitado: boolean }> => {
    const response = await api.get('/api/configuracao-email/status');
    return response.data;
  },
};

export const associadosAPI = {
  listar: async (
    pagina = 1,
    porPagina = 20,
    apenasAtivosOrFiltros?: boolean | { busca?: string; status?: 'Todos' | 'Ativo' | 'Inativo' }
  ) => {
    const q = new URLSearchParams();
    q.set('pagina', String(pagina));
    q.set('porPagina', String(porPagina));
    q.set('_', String(Date.now()));
    if (typeof apenasAtivosOrFiltros === 'object' && apenasAtivosOrFiltros !== null) {
      const b = apenasAtivosOrFiltros.busca?.trim();
      if (b) q.set('busca', b);
      const s = apenasAtivosOrFiltros.status;
      if (s && s !== 'Todos') q.set('status', s);
    } else if (apenasAtivosOrFiltros === true) {
      q.set('apenasAtivos', 'true');
    }
    const response = await api.get(`/api/associados?${q.toString()}`);
    return response.data;
  },
  obter: async (id: string) => {
    const response = await api.get(`/api/associados/${id}`);
    return response.data;
  },
  criar: async (data: Record<string, unknown>) => {
    const response = await api.post('/api/associados', data);
    return response.data;
  },
  atualizar: async (id: string, data: Record<string, unknown>) => {
    await api.put(`/api/associados/${id}`, data);
  },
};

export const eleicoesAPI = {
  listar: async (pagina = 1, porPagina = 20, filtros?: { busca?: string; status?: number; tipo?: number }) => {
    const params = new URLSearchParams();
    params.set('pagina', String(pagina));
    params.set('porPagina', String(porPagina));
    if (filtros?.busca) params.set('busca', filtros.busca);
    if (filtros?.status != null) params.set('status', String(filtros.status));
    if (filtros?.tipo != null) params.set('tipo', String(filtros.tipo));
    const response = await api.get(`/api/eleicoes?${params.toString()}`);
    return response.data;
  },
  obter: async (id: string) => {
    const response = await api.get(`/api/eleicoes/${id}`);
    return response.data;
  },
  criar: async (data: Record<string, unknown>) => {
    const response = await api.post('/api/eleicoes', data);
    return response.data;
  },
  atualizar: async (id: string, data: Record<string, unknown>) => {
    await api.put(`/api/eleicoes/${id}`, data);
  },
  atualizarStatus: async (id: string, status: number) => {
    await api.patch(`/api/eleicoes/${id}/status`, { status });
  },
  obterResultados: async (id: string) => {
    const response = await api.get(`/api/eleicoes/${id}/resultados`);
    return response.data;
  },
  votar: async (id: string, respostas: Record<string, unknown>) => {
    const response = await api.post(`/api/eleicoes/${id}/votar`, respostas);
    return response.data;
  },
};
