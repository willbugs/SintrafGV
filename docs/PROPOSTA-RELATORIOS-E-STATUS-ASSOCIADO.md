# Proposta: Status do associado, filtros ativo/inativo e novos relatórios

**Objetivo:** Alinhar cadastro (baixa, desfilado, aposentado, demitido), garantir filtro ativos/inativos em todos os relatórios de associados e atender levantamento “quem era associado em um período”.

---

## 1. Situação do associado (baixa, desfilou, aposentou, demitido)

### Hoje
- **Ativo** (bool): cadastro ativo no sistema (pode votar, receber comunicações).
- **Aposentado** (bool): status funcional (na ativa x aposentado do banco).
- **DataDesligamento** (DateTime?): data do desligamento.
- **Motivo** (string): motivo de desligamento ou observação (texto livre).

Não existe um campo único que classifique o tipo de saída (desligado, desfilado, aposentado, demitido). Isso fica implícito em **Ativo** + **DataDesligamento** + **Motivo** (e opcionalmente **Aposentado**).

### Proposta (escolher uma)

- **Opção A – Manter como está:**  
  Documentar que “baixa” = `Ativo = false`; o **Motivo** deve ser preenchido com texto padrão (ex.: “Desfilado”, “Aposentado”, “Demitido”, “Óbito”). Relatórios continuam usando **Ativo** e, quando precisar, filtram por **Motivo** (ou exibem **Motivo** na coluna).

- **Opção B – Campo “Tipo de baixa”:**  
  Adicionar enum/campo (ex.: **TipoBaixa** ou **SituacaoCadastro**) com valores como: `Ativo`, `Desligado`, `Desfilado`, `Aposentado`, `Demitido`, `Outro`.  
  Migração: criar coluna, preencher a partir de **Ativo** + **Motivo** (ou **Aposentado**), e opcionalmente manter **Motivo** para detalhe.  
  Relatórios e filtros passam a usar esse campo para “situação” e para filtro ativos/inativos (agrupando como “ativos” só o valor `Ativo`).

Recomendação inicial: **Opção A** (sem mudança de modelo), com convenção de preenchimento do **Motivo** e, na tela de relatórios, filtro **Situação: Todos | Ativos | Inativos** usando apenas o campo **Ativo**.

---

## 2. Relatório de aniversariantes (ativos e inativos / filtro)

### Hoje
- Backend: busca **todos** os associados (`ListarAsync(..., false)`) e filtra por mês/dia de **DataNascimento**. Não aplica filtro por ativo/inativo.
- Frontend: só tem filtro de **mês** e **dia**; não há “Ativos / Inativos / Todos”.

### Proposta
- **Backend:** Em `ObterRelatorioAniversariantesAsync`, ler do `request.Filtros` um parâmetro **situacao** (ou **apenasAtivos**): `"todos"` | `"ativos"` | `"inativos"`.
  - Se `"ativos"`: filtrar `associados.Where(a => a.Ativo)` antes de filtrar por aniversário.
  - Se `"inativos"`: filtrar `associados.Where(a => !a.Ativo)` antes de filtrar por aniversário.
  - Se `"todos"` ou ausente: manter comportamento atual.
- **Frontend:** Na tela do relatório de aniversariantes, adicionar um select **Situação:** Todos | Ativos | Inativos e enviar no `filtros` da requisição.

Assim o relatório de aniversariantes passa a ter filtro explícito ativos/inativos.

---

## 3. Relatório por banco (e demais) – filtro ativos/inativos

### Hoje
- **Por banco, por cidade, por sexo:** Backend traz **todos** os associados e aplica só o filtro específico (banco, cidade, sexo). O `AplicarFiltrosAssociados` até suporta filtro por **ativo** (campo `ativo`), mas a **UI não expõe** esse filtro nesses relatórios.
- **Aniversariantes** e **Novos associados:** idem – não há filtro “Ativos / Inativos” na tela.

### Proposta
- **Backend:** Em **todos** os relatórios de associados (geral, aniversariantes, novos, por-sexo, por-banco, por-cidade):
  - Ler dos filtros um parâmetro único, por exemplo **situacao**: `"todos"` | `"ativos"` | `"inativos"`.
  - Na lista de associados obtida do repositório, **antes** de aplicar filtros específicos (mês, banco, cidade, etc.), aplicar:
    - `"ativos"` → manter só `Ativo == true`
    - `"inativos"` → manter só `Ativo == false`
    - `"todos"` ou ausente → não filtrar por ativo.
- **Frontend:** Em **todas** as telas de visualização desses relatórios, incluir um filtro comum **Situação:** Todos | Ativos | Inativos (por exemplo no topo da área de filtros) e enviar em `filtros.situacao` (ou `filtros.apenasAtivos` / `filtros.ativo` conforme padrão adotado).

Com isso, **por banco**, **por cidade**, **por sexo**, **aniversariantes**, **novos associados** e **geral** passam a ter sempre o filtro ativos/inativos.

---

## 4. Relatório “para montar ação” / levantamento “quem era associado em um período”

### Hoje
- Não existe relatório que responda: “quem era associado no período [dataInicio, dataFim]?” usando **DataFiliacao** e **DataDesligamento**.

### Proposta
- **Novo relatório:** “Associados em período” (ou “Levantamento por período” / “Base em período”).
  - **Filtros:**
    - **Data início** e **Data fim** do período.
    - **Situação na data fim (opcional):** Todos | Ativos na data fim | Inativos na data fim (para cruzar com “quem ainda era ativo no fim do período”).
  - **Regra de negócio:**  
    Considerar “era associado em [dataInicio, dataFim]” quando:
    - **DataFiliacao** ≤ dataFim **e**
    - (**DataDesligamento** é null **ou** DataDesligamento ≥ dataInicio).

  Ou seja: filiou antes do fim do período e não tinha desligado antes do início (ou desligou dentro do período).

- **Backend:** Novo método no serviço de relatório (ex.: `ObterRelatorioAssociadosEmPeriodoAsync`) e novo endpoint (ex.: `POST /api/relatorios/associados/em-periodo`).
- **Frontend:** Novo tipo de relatório na lista (ex.: “Associados em período” / “Levantamento por período”) com tela que tenha:
  - Data início / Data fim
  - Situação: Todos | Ativos | Inativos (onde “Ativos” = ativo na data fim, “Inativos” = inativo na data fim).

Esse relatório atende “montar ação” e “quem era associado em um certo período”.

---

## 5. Resumo dos ajustes propostos

| Item | O quê | Onde |
|------|--------|------|
| Status (baixa/desfilado/aposentado/demitido) | Definir: manter texto em **Motivo** (Opção A) ou criar campo **TipoBaixa** (Opção B). | Modelo + doc + tela de cadastro (se B). |
| Aniversariantes | Filtro **Situação: Todos / Ativos / Inativos**. | Backend `ObterRelatorioAniversariantesAsync` + frontend RelatorioVisualizarPage (aniversariantes). |
| Por banco, por cidade, por sexo, novos, geral | Filtro **Situação: Todos / Ativos / Inativos** em todos. | Backend: todos os métodos de relatório de associados. Frontend: filtro único em RelatorioVisualizarPage para esses tipos. |
| Relatório “em período” | Novo relatório “Associados em período” com data início/fim e regra DataFiliacao/DataDesligamento. | Backend: novo método + endpoint. Frontend: novo card + tela com datas e situação. |

---

## 6. Ordem sugerida de implementação

1. **Filtro ativos/inativos em todos os relatórios de associados** (backend + frontend), incluindo aniversariantes e por banco.
2. **Novo relatório “Associados em período”** (backend + frontend).
3. **Definição e eventual implementação** de status/tipo de baixa (Opção A ou B), se aprovado.

---

**Aguardando sua aprovação para seguir com a implementação conforme esta proposta.**
