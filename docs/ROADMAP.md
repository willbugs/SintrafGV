# Roadmap – Plataforma SintrafGV

Roadmap de desenvolvimento da nova plataforma: backend (.NET), frontend administrativo (React, UI Bureau) e, em seguida, app de enquetes.

---

## Stack

| Camada | Tecnologia |
|--------|------------|
| **Backend** | .NET 8+, ASP.NET Core Web API, Clean Architecture, SQL Server |
| **Frontend Admin** | React 18, Vite, TypeScript, Material UI (MUI) — UI baseada no Bureau |
| **Frontend Enquete** | React + Vite (após conclusão do admin) |
| **Banco** | SQL Server (Docker em dev) |

---

## Princípios

- **Backend:** Clean Code, divisão clara de responsabilidades (Domain, Application, Infrastructure, API).
- **Frontend Admin:** Seguir o padrão visual e de componentes do Bureau (layout, autenticação, usuários).
- **Escopo inicial:** Cadastro de associados (clientes), relatórios e consultas para gestão dos associados do sindicato. **App de enquete** será desenvolvido **após** o backend e o front admin estarem em uso.

---

## Fase 1 – Backend (.NET)

**Objetivo:** API REST com Clean Architecture, preparada para associados, autenticação e relatórios/consultas.

### 1.1 Estrutura da solução

- **SintrafGv.Domain** – Entidades, value objects, exceções de domínio (sem dependências externas).
- **SintrafGv.Application** – Casos de uso, DTOs, interfaces de repositórios e serviços (depende só do Domain).
- **SintrafGv.Infrastructure** – EF Core, repositórios, acesso a SQL Server, serviços externos (depende de Application/Domain).
- **SintrafGv.Api** – Controllers, middleware, configuração (depende de Application e Infrastructure).

### 1.2 Entregas

| # | Entrega | Descrição |
|---|----------|-----------|
| 1.1 | Solução e projetos | Criar solução .NET 8, projetos Domain, Application, Infrastructure, Api. |
| 1.2 | Banco e EF Core | Connection string para SQL Server (Docker), DbContext, migrações. |
| 1.3 | Associados (CRUD) | Entidade Associado (alinhada ao legado CPessoas), repositório, serviços e endpoints. |
| 1.4 | Autenticação | JWT, login, refresh token, proteção de rotas (base para o Bureau no front). |
| 1.5 | Usuários do sistema | Cadastro de usuários administrativos (roles/permissões simplificadas). |
| 1.6 | Relatórios e consultas | Endpoints para listagens, filtros e exportação (relatórios sobre associados). |

### 1.3 Ordem sugerida

1. Estrutura da solução + Docker (SQL Server já disponível).  
2. Domain (Associado, Usuario, etc.) + Infrastructure (EF Core, repositórios).  
3. Application (serviços, DTOs) + Api (controllers de Associados).  
4. Autenticação (JWT) + Usuários.  
5. Relatórios e consultas (endpoints e filtros).

---

## Fase 2 – Frontend administrativo (React)

**Objetivo:** Painel administrativo com UI do Bureau para gestão de associados, relatórios e consultas.

### 2.1 Base de UI (Bureau)

- **Layout:** AdminLayout com Drawer (sidebar), AppBar, menu.  
- **Autenticação:** AuthContext, login (e-mail/senha), JWT, refresh token.  
- **Componentes:** MUI (Material UI), tabelas, formulários, feedback (toast/snackbar).  

Referência: `D:\progs\Bureau\frontend\admin-panel`.

### 2.2 Estrutura do projeto (admin)

- **src/components** – Layout, formulários reutilizáveis, tabelas.  
- **src/pages** – Associados (lista, cadastro, edição), Dashboard, Relatórios, Consultas, Usuários, Perfil.  
- **src/services** – API (axios), auth, associados, relatórios.  
- **src/contexts** – AuthContext (e outros se necessário).  
- **src/types** – Tipos TypeScript (Associado, Usuario, etc.).

### 2.3 Entregas

| # | Entrega | Descrição |
|---|----------|-----------|
| 2.1 | Projeto base | Vite + React + TypeScript + MUI + React Router. |
| 2.2 | Layout e auth | Copiar/adaptar AdminLayout e AuthContext do Bureau; tela de login. |
| 2.3 | CRUD Associados | Listagem, filtros, cadastro e edição de associados (consumindo a API). |
| 2.4 | Relatórios | Telas de relatórios e consultas sobre associados (gráficos/tabelas/export). |
| 2.5 | Usuários e perfil | Gestão de usuários do sistema e tela de perfil (como no Bureau). |

### 2.4 Ordem sugerida

1. Criar projeto React (Vite + TS), instalar MUI e Router.  
2. Adaptar Layout e Auth do Bureau; conectar à API de autenticação.  
3. Páginas de Associados (lista + formulário).  
4. Relatórios e consultas.  
5. Usuários e perfil.

---

## Fase 3 – App de enquete

**Objetivo:** Aplicação de votação para associados (enquetes), após o backend e o front admin estarem em uso.

- **Quando:** Iniciar **após** Fase 1 e Fase 2 estáveis (backend + painel admin com associados, relatórios e consultas).  
- **Escopo:** Frontend de votação (React), endpoints de enquetes e votos, identificação do associado (ex.: CPF + matrícula + data nascimento), registro de voto e log.  
- Detalhamento (entregas e ordem) será definido em um documento específico ou em atualização deste roadmap.

---

## Resumo visual

```
Fase 1: Backend (.NET)
  → Estrutura Clean (Domain, Application, Infrastructure, Api)
  → SQL Server + EF Core
  → Associados (CRUD)
  → Autenticação JWT + Usuários
  → Relatórios e consultas (endpoints)

Fase 2: Frontend Admin (React)
  → UI Bureau (Layout + Auth)
  → CRUD Associados
  → Relatórios e consultas (telas)
  → Usuários e perfil

Fase 3: App Enquete (após 1 e 2)
  → Backend: enquetes e votos
  → Front: votação para associados
```

---

## Documentos relacionados

- [BASE-LEGADA-SINTRAFGV.md](BASE-LEGADA-SINTRAFGV.md) – Modelo e regras do sistema legado.  
- [README.md](../README.md) – Visão geral do repositório e decisões de desenho.
