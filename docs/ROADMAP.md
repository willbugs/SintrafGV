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

## Fase 1 – Backend (.NET) ✅ CONCLUÍDO

**Objetivo:** API REST com Clean Architecture, preparada para associados, autenticação e sistema de votações.

### 1.1 Estrutura da solução ✅

- **SintrafGv.Domain** – Entidades, value objects, exceções de domínio (sem dependências externas).
- **SintrafGv.Application** – Casos de uso, DTOs, interfaces de repositórios e serviços (depende só do Domain).
- **SintrafGv.Infrastructure** – EF Core, repositórios, acesso a SQL Server, serviços externos (depende de Application/Domain).
- **SintrafGv.Api** – Controllers, middleware, configuração (depende de Application e Infrastructure).

### 1.2 Entregas ✅

| # | Entrega | Status | Descrição |
|---|----------|--------|-----------|
| 1.1 | Solução e projetos | ✅ | Solução .NET 8, projetos Domain, Application, Infrastructure, Api criados. |
| 1.2 | Banco e EF Core | ✅ | SQL Server (Docker), DbContext, 8 migrations aplicadas. |
| 1.3 | Associados (CRUD) | ✅ | Entidade Associado (todos campos do legado), repositório, serviços e endpoints. |
| 1.4 | Autenticação | ✅ | JWT, login, refresh token, proteção de rotas implementados. |
| 1.5 | Usuários do sistema | ✅ | CRUD de usuários administrativos com roles. |
| 1.6 | Sistema de Votações | ✅ | **NOVO:** Entidades Eleicao, Pergunta, Opcao, Voto, VotoDetalhe + CRUD completo. |
| 1.7 | API de Resultados | ✅ | **NOVO:** Endpoints para contabilização de votos e obtenção de resultados. |
| 1.8 | API de Votação | ✅ | **NOVO:** Endpoint para submissão de votos com validações completas. |

### 1.3 Implementado ✅

1. ✅ Estrutura da solução + Docker (SQL Server rodando).  
2. ✅ Domain (Associado, Usuario, Eleicao, etc.) + Infrastructure (EF Core, repositórios).  
3. ✅ Application (serviços, DTOs) + Api (controllers).  
4. ✅ Autenticação (JWT) + Usuários.  
5. ✅ Sistema completo de votações/eleições (não previsto originalmente).
6. ✅ API de resultados e contabilização de votos.
7. ✅ API de votação com validações de segurança.
6. ✅ API de resultados e contabilização de votos.
7. ✅ API de votação com validações de segurança.

---

## Fase 2 – Frontend administrativo (React) ✅ CONCLUÍDO

**Objetivo:** Painel administrativo com UI do Bureau para gestão de associados, usuários e eleições.

### 2.1 Base de UI (Bureau) ✅

- **Layout:** AdminLayout com Drawer (sidebar), AppBar, menu.  
- **Autenticação:** AuthContext, login (e-mail/senha), JWT, refresh token.  
- **Componentes:** MUI (Material UI), tabelas, formulários, feedback (notistack).  

Referência: `D:\progs\Bureau\frontend\admin-panel`.

### 2.2 Estrutura do projeto (admin) ✅

- **src/components** – Layout, formulários reutilizáveis, tabelas.  
- **src/pages** – Associados, Usuários, Eleições (lista + formulário), Dashboard.  
- **src/services** – API (axios), auth, associados, usuários, eleições.  
- **src/contexts** – AuthContext, ToastContext.  
- **src/types** – Tipos TypeScript (Associado, Usuario, Eleicao, etc.).

### 2.3 Entregas ✅

| # | Entrega | Status | Descrição |
|---|----------|--------|-----------|
| 2.1 | Projeto base | ✅ | Vite + React + TypeScript + MUI + React Router implementados. |
| 2.2 | Layout e auth | ✅ | AdminLayout e AuthContext adaptados do Bureau; login funcional. |
| 2.3 | CRUD Associados | ✅ | Listagem, cadastro e edição de associados (todos campos do legado). |
| 2.4 | CRUD Usuários | ✅ | Gestão de usuários do sistema com roles. |
| 2.5 | CRUD Eleições | ✅ | **NOVO:** Gestão completa de eleições/enquetes com perguntas e opções. |
| 2.6 | Resultados de Eleições | ✅ | **NOVO:** Página de visualização de resultados com gráficos e estatísticas. |
| 2.7 | Anexos de Documentos | ✅ | **NOVO:** Upload e gerenciamento de documentos anexos às enquetes. |
| 2.8 | UI "Enquetes" | ✅ | **NOVO:** Terminologia atualizada e UI reorganizada em seções. |

### 2.4 Implementado ✅

1. ✅ Projeto React (Vite + TS + MUI + Router).  
2. ✅ Layout e Auth do Bureau adaptados; API conectada.  
3. ✅ Páginas de Associados (lista + formulário completo).  
4. ✅ Páginas de Usuários (lista + formulário).  
5. ✅ Sistema de Eleições/Enquetes (não previsto originalmente).
6. ✅ Página de resultados com gráficos e estatísticas detalhadas.
7. ✅ Sistema de anexos de documentos (PDF, DOC, DOCX).
8. ✅ UI reorganizada com terminologia "Enquetes" e seções estruturadas.

---

## Fase 3 – Frontend de Votação (Associados) 📋 EM ANDAMENTO

**Objetivo:** Aplicação pública de votação para associados votarem em eleições/enquetes.

### 3.1 Entregas Pendentes

| # | Entrega | Status | Descrição |
|---|----------|--------|-----------|
| 3.1 | Projeto Base | 📋 | React + Vite + TypeScript + MUI para interface pública. |
| 3.2 | Login de Associado | 📋 | Autenticação por CPF + Data Nascimento (API já implementada). |
| 3.3 | Lista de Eleições | 📋 | Mostrar eleições disponíveis para o associado votar. |
| 3.4 | Fluxo de Votação | 📋 | Wizard passo a passo (pergunta por pergunta). |
| 3.5 | Confirmação e Comprovante | 📋 | Resumo final e comprovante de voto. |
| 3.6 | Visualização de Resultados | 📋 | Resultados públicos após apuração (reutilizar componente admin). |

**Status:** ✅ Backend de votações **100% implementado** (Fase 1) + ✅ Admin de enquetes **100% implementado** (Fase 2). 📋 Falta apenas o **frontend público** para associados.

---

## Fase 4 – PWA + Mobile 🚀 FUTURO

**Objetivo:** Transformar o frontend de votação em PWA com recursos nativos móveis.

### 4.1 Entregas Futuras

| # | Entrega | Status | Descrição |
|---|----------|--------|-----------|
| 4.1 | PWA Configuration | 🚀 | Service Worker, manifest, instalação como app. |
| 4.2 | Mobile Wrappers | 🚀 | WebView nativo para Android e iOS. |
| 4.3 | Biometria | 🚀 | Integração com biometria do dispositivo para votação. |
| 4.4 | Push Notifications | 🚀 | Avisos de eleições abertas e resultados. |
| 4.5 | Recursos Avançados | 🚀 | Validação facial, geolocalização, câmera. |

---

## Status Atual

```
✅ Fase 1: Backend (.NET) — CONCLUÍDO
  ✅ Estrutura Clean (Domain, Application, Infrastructure, Api)
  ✅ SQL Server + EF Core + 8 migrations
  ✅ Associados (CRUD completo)
  ✅ Autenticação JWT + Usuários + Refresh Token
  ✅ Sistema de Votações (Eleicao, Pergunta, Opcao, Voto, VotoDetalhe)
  ✅ API de Resultados e Contabilização
  ✅ API de Votação com Validações de Segurança

✅ Fase 2: Frontend Admin (React) — CONCLUÍDO
  ✅ UI Bureau (Layout + Auth)
  ✅ CRUD Associados (todos campos)
  ✅ CRUD Usuários
  ✅ CRUD Eleições/Enquetes (com perguntas e opções)
  ✅ Visualização de Resultados (gráficos + estatísticas)
  ✅ Sistema de Anexos de Documentos
  ✅ UI "Enquetes" com seções organizadas

📋 Fase 3: Frontend de Votação — PENDENTE
  📋 Login de Associado
  📋 Interface de votação
  📋 Comprovante e resultados

🚀 Fase 4: PWA + Mobile — FUTURO
  🚀 Progressive Web App
  🚀 Wrappers Android/iOS
  🚀 Biometria e recursos nativos
```

**Próximo:** ⚡ Implementar **frontend de votação** para associados (última etapa antes do MVP funcional).

---

## Documentos relacionados

- [BASE-LEGADA-SINTRAFGV.md](BASE-LEGADA-SINTRAFGV.md) – Modelo e regras do sistema legado.  
- [SISTEMA-VOTACOES.md](SISTEMA-VOTACOES.md) – Arquitetura e roadmap detalhado do sistema de votações.  
- [README.md](../README.md) – Visão geral do repositório e decisões de desenho.
