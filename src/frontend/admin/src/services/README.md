# Services

## api.ts
Instância axios configurada com:
- Base URL da API (SintrafGv.Api)
- Interceptors para JWT automático
- Tratamento de erros padronizado

## Serviços Implementados
- **authAPI** – Login JWT, refresh token, perfil
- **associadosService** – CRUD completo de associados
- **usuariosService** – Gestão de usuários do sistema
- **eleicoesService** – CRUD de eleições e votações
- **relatorioService** – Geração de relatórios diversos
