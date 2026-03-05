# Relatório de Investigação: Relatórios de Votação

## Resumo

Investigação sobre a estrutura de relatórios no menu, nos cards e o apontamento dos 3 relatórios de votação.

---

## 1. Estrutura do Menu (AdminLayout.tsx)

O menu **Relatórios** possui **3 subopções**:

| Item | Rota | Descrição |
|------|------|-----------|
| Relatórios Gerais | `/relatorios` | Página central com todos os cards |
| Relatórios de Votação | `/relatorios/votacao` | Página com abas (Participação, Resultados, Engajamento) |
| Relatórios Cartoriais | `/relatorios/cartorial` | Relatório oficial para cartório |

---

## 2. Estrutura dos Cards (RelatoriosPage.tsx)

Na página **Relatórios Gerais** (`/relatorios`), os cards são exibidos por categoria. Na categoria **Votações** há **4 cards**:

| Card | ID | Navegação ao clicar |
|------|-----|---------------------|
| Participação em Votações | `participacao-votacao` | `/relatorios/votacao?tab=0` |
| Resultados de Enquetes | `resultados-eleicao` | `/relatorios/votacao?tab=1` |
| Engajamento em Votações | `engajamento-votacao` | `/relatorios/votacao?tab=2` |
| Relatório Cartorial | `cartorial` | `/relatorios/cartorial` |

---

## 3. Página de Relatórios de Votação (RelatoriosVotacaoPage.tsx)

A rota `/relatorios/votacao` exibe **3 abas**:

| Aba | Índice | Endpoint API | Conteúdo |
|-----|--------|--------------|----------|
| Participação | tab=0 | `POST /api/relatorios/participacao-votacao` | Associados com participação em votações |
| Resultados | tab=1 | `POST /api/relatorios/resultados-eleicao` | Resultados por enquete/eleição |
| Engajamento | tab=2 | `POST /api/relatorios/engajamento-votacao` | Métricas de engajamento por período |

---

## 4. Backend – Endpoints Distintos

O backend possui **3 endpoints separados** que retornam dados diferentes:

- **participacao-votacao**: `ParticipacaoVotacaoDto` (associados, votos, percentual)
- **resultados-eleicao**: `ResultadoEleicaoDto` (eleições, candidatos, vencedor)
- **engajamento-votacao**: `EngajamentoVotacaoDto` (métricas, votos/dia, pico)

---

## 5. Problema Identificado

### Falta de sincronização URL ↔ aba

1. **Ao clicar em um card**: a navegação usa `?tab=0`, `?tab=1` ou `?tab=2` corretamente.
2. **Ao trocar de aba manualmente**: o `handleTabChange` só atualiza o estado local; a **URL não é alterada**.
3. **Efeito**: se o usuário trocar de aba e depois voltar ou recarregar, a aba exibida pode não corresponder à URL.

### Possível causa do “todos apontam para o mesmo”

- Se a leitura de `searchParams.get('tab')` falhar ou o parâmetro não for propagado corretamente em algum fluxo, o valor padrão `0` seria usado.
- Nesse caso, os 3 cards levariam à mesma aba (Participação, tab=0).

---

## 6. Correções Recomendadas

1. **Sincronizar URL ao trocar de aba**: ao clicar em uma aba, atualizar a URL com `?tab=N` usando `setSearchParams` ou `navigate`.
2. **Garantir leitura do parâmetro `tab`**: validar que `tab` está entre 0 e 2; caso contrário, usar 0.
3. **Testar fluxo completo**: clicar em cada card e conferir se a aba correta é exibida.

---

## 7. Arquivos Relevantes

- `src/frontend/admin/src/components/Layout/AdminLayout.tsx` – menu
- `src/frontend/admin/src/pages/RelatoriosPage.tsx` – cards e `handleAbrirRelatorio`
- `src/frontend/admin/src/pages/RelatoriosVotacaoPage.tsx` – abas e parâmetro `tab`
- `src/backend/src/SintrafGv.Api/Controllers/RelatoriosController.cs` – endpoints
- `src/backend/src/SintrafGv.Application/Services/RelatorioServiceSimplificado.cs` – lógica dos relatórios
