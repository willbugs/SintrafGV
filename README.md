# SintrafGV – Plataforma Moderna

Modernização da plataforma do sindicato **SintrafGV**. Este repositório contém o projeto da nova aplicação e a documentação da base legada para migração.

---

## Visão geral

- **Sindicato:** **SintrafGV** (nome oficial da organização).
- **Objetivo:** substituir o sistema legado (.NET Framework, ASP.NET MVC, NHibernate) por uma plataforma moderna, mantendo os dados existentes e as funcionalidades essenciais.
- **Escopo:** **um único sindicato** — não haverá multi-empresa; a aplicação atende exclusivamente ao SintrafGV.
- **Banco de dados:** SQL Server. Em **desenvolvimento**, o banco roda em **Docker** (apenas SQL Server no container).
- **Dados legados:** o banco legado será preservado; a migração deve garantir **nenhuma perda de informações** (leitura do legado e/ou ETL para o novo modelo).

---

## Decisões de desenho

| Aspecto | Decisão |
|--------|--------|
| Multi-empresa | **Removido** — 1 sindicato apenas (SintrafGV) |
| Banco de dados | **SQL Server** (produção e dev) |
| Dev local | **SQL Server em Docker** (sem outros serviços no Docker em dev) |
| UI administrativo | Baseada no projeto **Bureau** (layout, controle de usuários e padrões visuais) |
| Frontends | Dois: **Administrativo** (painel interno) e **Votação** (enquetes para associados) |

---

## Referência de UI: Bureau

A interface do **painel administrativo** e o **controle de usuários** serão baseados no projeto Bureau (`D:\progs\Bureau`), de autoria do mesmo time:

- **Admin panel:** React 18 + Vite + **Material UI (MUI)** + React Router.
- **Layout:** `AdminLayout` com Drawer (sidebar), AppBar, menu e área de conteúdo.
- **Autenticação:** `AuthContext` com login (e-mail/senha), JWT, refresh de token e persistência em `localStorage`.
- **Páginas de referência:** Login, Dashboard, Usuários, perfil e demais telas do Bureau a serem adaptadas para o contexto do sindicato.

O código de UI/controle de usuários será **copiado/adaptado** do Bureau para este projeto (não multi-empresa).

---

## Estrutura do projeto

```
SintrafGv/
├── docs/
│   ├── BASE-LEGADA-SINTRAFGV.md   # Descritivo completo do legado
│   └── ROADMAP.md                 # Roadmap (backend → front admin → enquete)
├── base conhecimento legado/      # Código legado (referência e migração)
├── src/
│   ├── backend/                   # .NET 8, Clean Architecture
│   │   ├── SintrafGv.sln
│   │   └── src/
│   │       ├── SintrafGv.Api/           # Web API, controllers
│   │       ├── SintrafGv.Application/  # Serviços, DTOs, interfaces
│   │       ├── SintrafGv.Domain/        # Entidades (ex.: Associado)
│   │       └── SintrafGv.Infrastructure/ # EF Core, repositórios, SQL Server
│   └── frontend/
│       ├── admin/                 # Painel administrativo (React + Vite + MUI, UI Bureau)
│       └── votacao/               # Front de votação (enquetes) — após admin
├── docker/
│   ├── docker-compose.yml        # SQL Server em dev (projeto: sintrafgv)
│   └── README.md
└── README.md
```

---

## Frontends

### 1. Administrativo (`frontend/admin`)

- Público: gestores e funcionários do SintrafGV.
- Base visual e de autenticação: **Bureau** (layout, login, usuários, perfil).
- Funcionalidades (a migrar do legado): cadastro de associados (pessoas), bancos, parâmetros, relatórios, gestão de enquetes (criar/editar/encerrar), usuários e permissões (simplificado para um sindicato).

### 2. Votação (`frontend/votacao`)

- Público: **associados** (votantes).
- Função: acessar enquetes ativas, identificar-se (ex.: CPF + matrícula + data de nascimento, conforme legado) e registrar voto.
- Interface: fluxo simples e seguro (login do associado → listar enquetes abertas → votar → confirmação). Não reutiliza o layout pesado do administrativo.

---

## Banco de dados e Docker (dev)

- **Produção:** SQL Server em ambiente do SintrafGV (a definir).
- **Desenvolvimento:** SQL Server rodando em **Docker**; em dev usamos **somente** o container do SQL Server (sem API ou front no Docker por padrão).

Para subir apenas o SQL Server em dev:

```bash
cd docker
docker-compose up -d
```

Conexão típica em dev (ajustar em `appsettings` ou variáveis de ambiente):

- **Servidor:** `localhost,1433` (ou `host.docker.internal` se a API rodar fora do Docker).
- **Usuário:** `sa`
- **Senha:** definida no `docker-compose.yml` (trocar em ambiente real).

O banco legado (backup ou instância existente) deve ser acessível para **leitura** e para processos de migração/ETL; a nova aplicação pode usar um novo banco (ex.: `SintrafGv`) com schema migrado a partir do legado.

---

## Preservação dos dados legados

- **Não perder informações:** toda migração (ETL, scripts, importação) deve ser planejada para preservar os dados do banco legado.
- Estratégias possíveis:
  - Leitura direta do banco legado (só leitura) e nova aplicação escrevendo em novo banco.
  - ETL que copia/transforma dados do legado para o novo schema (uma vez ou incremental).
  - Backup completo do legado antes de qualquer operação destrutiva.
- Documentação do modelo e das regras legadas: **`docs/BASE-LEGADA-SINTRAFGV.md`**.

---

## Como rodar (dev)

1. **SQL Server:** `cd docker` e `docker-compose up -d`.
2. **Backend:** `cd src/backend` e `dotnet run --project src/SintrafGv.Api`. Aplicar migrações: `dotnet ef database update --project src/SintrafGv.Infrastructure --startup-project src/SintrafGv.Api`.
3. **Frontend admin:** `cd src/frontend/admin`, `npm install` e `npm run dev`. Login: `admin@sintrafgv.com.br` / `Admin@123`.

---

## Documentação

- **Roadmap (fases e entregas):** [docs/ROADMAP.md](docs/ROADMAP.md)
- **Base legada:** [docs/BASE-LEGADA-SINTRAFGV.md](docs/BASE-LEGADA-SINTRAFGV.md)
- **Bureau (referência de UI):** `D:\progs\Bureau\frontend\admin-panel`

---

*README inicial — projeto de modernização da plataforma SintrafGV.*
