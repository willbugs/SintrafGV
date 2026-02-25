# Auditoria de Problemas Pendentes - SintrafGV Admin

**Data:** 25/02/2026
**Escopo:** Frontend Admin (React) + Backend (.NET)
**Última Atualização:** 25/02/2026 18:04 - ✅ TODAS CORREÇÕES APLICADAS

---

## ✅ STATUS FINAL - TODAS CORREÇÕES CONCLUÍDAS

Veja o arquivo `RESUMO-CORRECOES.md` para detalhes completos de cada correção aplicada.

**Resultado:**
- ✅ Frontend compila sem erros
- ✅ Backend compila sem erros (apenas warnings não-críticos)
- ✅ Todas as 6 categorias de problemas corrigidas
- ✅ 8 arquivos modificados
- ✅ ~600 linhas de código reescritas/corrigidas

---

## 1. PÁGINA QUEBRADA - Relatório Cartorial

**Arquivo:** `src/frontend/admin/src/pages/RelatorioCartorialPage.tsx`

| # | Linha | Problema | Detalhe |
|---|-------|----------|---------|
| 1.1 | 105 | BUG CRÍTICO | `response.data.filter(...)` — A API `/api/eleicoes` retorna `{ itens, total }`, não um array. Deveria ser `response.data.itens.filter(...)`. **A página inteira não carrega.** |
| 1.2 | 105 | STATUS INEXISTENTE | Filtra por `'Finalizada'` mas o enum real é `'Encerrada'` (StatusEleicao.Encerrada = 3). O status `'Finalizada'` não existe na entidade. |
| 1.3 | 49, 89, 339-352 | FUNCIONALIDADE REMOVIDA | Switch "Gerar Assinatura Digital" ainda presente na interface, apesar da decisão de NÃO utilizar certificados digitais. |
| 1.4 | 224 | TEXTO INCOERENTE | Menciona "assinatura digital para validação legal" no Alert informativo. |

---

## 2. PÁGINA MOCKADA - Perfil

**Arquivo:** `src/frontend/admin/src/pages/PerfilPage.tsx`

| # | Linha | Problema | Detalhe |
|---|-------|----------|---------|
| 2.1 | 76-81 | DADOS FAKE | "Últimas Ações" é uma lista hardcoded inventada (ex: "Login realizado", "Relatório de associados gerado", "Usuário criado: João Silva"). Não vem de nenhuma API. |
| 2.2 | 97 | DATA HARDCODED | `dataCriacao: '15/01/2024'` — valor fixo inventado. |
| 2.3 | 90 | COMENTÁRIO ADMITE MOCK | `// Simular dados do perfil baseado no usuário logado` |
| 2.4 | 117-118 | SALVAR NÃO FUNCIONA | `// Aqui seria a chamada para a API` / `// await api.put(...)` — Salvar perfil **não chama a API**, apenas altera o estado local do React. |
| 2.5 | 120 | COMENTÁRIO ADMITE MOCK | `// Simular sucesso` |
| 2.6 | 162-165 | ALTERAR SENHA NÃO FUNCIONA | `// Aqui seria a chamada para a API` / `// await api.post('/auth/alterar-senha'...)` — Alterar senha **não chama a API**, apenas finge que alterou. |

---

## 3. PÁGINA SEM FILTROS - Relatórios de Votação

**Arquivo:** `src/frontend/admin/src/pages/RelatoriosVotacaoPage.tsx`

| # | Linha | Problema | Detalhe |
|---|-------|----------|---------|
| 3.1 | — | SEM FILTRO DE ENQUETE | Não existe seletor para escolher uma enquete específica. O sistema gerencia VÁRIAS enquetes mas os relatórios tratam tudo como se fosse uma só. |
| 3.2 | — | SEM FILTRO DE PERÍODO | Não tem campos de data início/fim. O backend aceita `DataInicio` e `DataFim` mas o frontend nunca envia. |
| 3.3 | — | SEM FILTRO DE STATUS | Não filtra por Aberta, Encerrada, Apurada, etc. |
| 3.4 | — | SEM FILTRO DE TIPO | Não diferencia entre Enquete e Eleição. |
| 3.5 | 67, 80, 93 | FILTROS VAZIOS | Chama o serviço com `filtros: {}` vazio — nunca filtra nada. |
| 3.6 | 59 | ERRO DE COMPILAÇÃO | Variável `event` declarada mas nunca usada. Impede o `npm run build`. |

---

## 4. TERMINOLOGIA ERRADA - "Eleição" em vez de "Enquete"

O conceito central do sistema é **Enquete** (perguntas e respostas), que pode opcionalmente ser usada como Eleição. A terminologia nos relatórios está errada em vários lugares:

### RelatoriosVotacaoPage.tsx
| Linha | Texto errado | Deveria ser |
|-------|-------------|-------------|
| 109 | "eleições e enquetes" | "enquetes e votações" |
| 146 | "associados nas eleições" | "associados nas enquetes" |
| 200 | "Eleições Disponíveis" | "Enquetes Disponíveis" |
| 230 | "Resultados de Eleições" | "Resultados de Enquetes" |
| 233 | "resultados por eleição" | "resultados por enquete" |
| 254 | "Total de Eleições" | "Total de Enquetes" |
| 290 | "Vencedor" (coluna) | Conceito de eleição, não se aplica a enquetes |
| 339 | "Eleições Analisadas" | "Enquetes Analisadas" |
| 369 | "Eleição" (coluna) | "Enquete" |

### DashboardPage.tsx
| Linha | Texto errado | Deveria ser |
|-------|-------------|-------------|
| 288 | "Eleições Ativas" | "Enquetes Ativas" |

### RelatoriosPage.tsx
| Linha | Texto errado | Deveria ser |
|-------|-------------|-------------|
| 121 | "nas eleições" | "nas enquetes" |
| 128 | "Resultados de Eleições" | "Resultados de Enquetes" |
| 129 | "resultados por eleição" | "resultados por enquete" |

### relatorioService.ts (interfaces)
| Linha | Nome errado | Deveria ser |
|-------|------------|-------------|
| 85 | `totalEleicoes` | `totalEnquetes` |
| 86 | `eleicoesAbertas` | `enquetesAbertas` |
| 87 | `eleicoesEncerradas` | `enquetesEncerradas` |
| 113 | `totalEleicoesDisponiveis` | `totalEnquetesDisponiveis` |
| 122 | `ResultadoEleicaoRelatorio` | `ResultadoEnqueteRelatorio` |
| 249 | `obterRelatorioResultadosEleicao` | `obterRelatorioResultadosEnquete` |

---

## 5. CÓDIGO COMENTADO - Relatório Visualizar

**Arquivo:** `src/frontend/admin/src/pages/RelatorioVisualizarPage.tsx`

| # | Linhas | Problema | Detalhe |
|---|--------|----------|---------|
| 5.1 | 152-177 | FUNÇÃO COMENTADA | `handleExportar` — toda a lógica de exportação PDF/Excel/CSV está comentada dentro de `/* */`. |
| 5.2 | 186-191 | FUNÇÃO COMENTADA | `aplicarFiltros` — lógica de aplicação de filtros está comentada dentro de `/* */`. |

---

## 6. BACKEND - Funcionalidades Vazias

**Arquivo:** `src/backend/src/SintrafGv.Application/Services/RelatorioServiceSimplificado.cs`

| # | Linha | Problema | Detalhe |
|---|-------|----------|---------|
| 6.1 | 428-431 | MOCK | `SalvarHistoricoRelatorioAsync` retorna `Task.CompletedTask` — não salva nada. |
| 6.2 | 433-436 | MOCK | `ObterHistoricoRelatoriosUsuarioAsync` retorna lista vazia `new List<dynamic>()` — nunca tem histórico. |

---

## 7. BACKEND - Assinatura Digital Residual

**Arquivo:** `src/backend/src/SintrafGv.Application/Services/RelatorioVotacaoCartorialService.cs`

| # | Linha | Problema | Detalhe |
|---|-------|----------|---------|
| 7.1 | 342-347 | ASSINATURA SIMULADA | `AssinarDigitalmenteRelatorioAsync` retorna string simulada `HASH_{hash}_{timestamp}` — não é assinatura digital real. |
| 7.2 | 443-445 | VALIDAÇÃO FALSA | `ValidarAssinaturaDigital` apenas verifica se string não é vazia — não valida nada. |
| 7.3 | 155 | FLAG ATIVA | Ainda aceita `request.GerarAssinaturaDigital` e executa a lógica simulada. |

---

## RESUMO GERAL

| Categoria | Quantidade |
|-----------|-----------|
| Páginas quebradas (não carregam) | 1 |
| Páginas inteiramente mockadas | 1 |
| Páginas sem filtros necessários | 1 |
| Funções comentadas/desabilitadas | 2 |
| Ocorrências de terminologia errada | 15+ |
| Funcionalidades backend vazias/mock | 2 |
| Assinatura digital residual (decisão: remover) | 3 |
| Erros de compilação | 1 |
| Status inexistente no filtro | 1 |

---

## DECISÕES JÁ TOMADAS PELO CLIENTE

1. **Assinatura Digital**: NÃO será usada. Remover todas as referências do frontend e simplificar no backend.
2. **QR Code no comprovante**: Ficará para próxima oportunidade.
3. **Terminologia**: O sistema é de **Enquetes** (perguntas e respostas), que podem opcionalmente ser usadas como Eleições.
