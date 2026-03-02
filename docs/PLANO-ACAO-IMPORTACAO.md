# Plano de Ação – Importação de Dados

**Escopo:** PESSOAS, BANCOS, EMPRESAS  
**Foco:** Associados, ConfiguracoesSindicato — máximo de dados, sem perda.  
**Fora do escopo:** Usuarios, Enquetes.

---

## 1. Entidades e Fontes

| Legado | Novo | Observação |
|--------|------|------------|
| PESSOAS | Associados | Migração completa |
| BANCOS | Associados.Banco (via JOIN) | NOME + NUMERO preservados |
| EMPRESAS | ConfiguracoesSindicato | Máximo de dados disponíveis |

---

## 2. Mapeamento Detalhado

### 2.1 PESSOAS → Associados (sem perda de dados)

| Legado (PESSOAS) | Novo (Associados) | Estratégia |
|------------------|-------------------|------------|
| Sguid | Id | `CONVERT(uniqueidentifier, Sguid)` |
| NOME | Nome | Direto |
| CPF | Cpf | Direto (normalizar: apenas dígitos) |
| MATRICULASINDICATO | MatriculaSindicato | Direto |
| MATRICULABANCARIA | MatriculaBancaria | Direto |
| SEXO | Sexo | Direto |
| ESTADOCIVIL | EstadoCivil | Direto |
| DATNASCIMENTO | DataNascimento | Direto |
| NATURALIDADE | Naturalidade | Direto |
| ENDERECO | Endereco | Direto |
| BAIRRO | Bairro | Direto |
| CIDADE | Cidade | Direto |
| ESTADO | Estado | Direto |
| CODAGENCIA | CodAgencia | Direto |
| AGENCIA | Agencia | Direto |
| CONTA | Conta | Direto |
| BANCO (FK) | Banco | JOIN BANCOS → `NOME (NUMERO)` para preservar ambos |
| FUNCAO | Funcao | Direto |
| CTPS | Ctps | Direto |
| SERIE | Serie | Direto |
| DATADMISSAO | DataAdmissao | Direto |
| DATFILIACAO | DataFiliacao | Direto |
| DATDESLIGAMENTO | DataDesligamento | Direto |
| CELULAR | Celular | Direto |
| TELEFONE | Telefone | Direto |
| EMAIL | Email | Direto |
| ATIVO | Ativo | Direto |
| APOSENTADO | Aposentado | Direto |
| DTULTIMAATUALIZACAO | DataUltimaAtualizacao | Direto |
| — | CriadoEm | `COALESCE(DTULTIMAATUALIZACAO, GETUTCDATE())` |
| CEP | Cep | Legado não tem; NULL |
| COMPLEMENTO | Complemento | Legado não tem; NULL |
| CARTEIRINHA | — | **Sem coluna no novo** → ver item 4 |
| BASE | — | **Sem coluna no novo** → ver item 4 |
| MOTIVO | — | **Sem coluna no novo** → ver item 4 |
| ASSOCIADO, DEACORDO, NOVO | — | Simplificados em Ativo; não migrar |

### 2.2 BANCOS → Associados.Banco

O novo schema não tem tabela BANCOS. O dado é armazenado em `Associados.Banco`.

**Estratégia:** Para cada PESSOA, fazer JOIN com BANCOS e preencher:

```
Associados.Banco = BANCOS.NOME + ' (' + BANCOS.NUMERO + ')'
```

Exemplo: `"Banco do Brasil (001)"` — preserva nome e número.

Se `PESSOAS.BANCO` for NULL, usar `Associados.Banco = NULL`.

### 2.3 EMPRESAS → ConfiguracoesSindicato

| Legado (EMPRESAS) | Novo (ConfiguracoesSindicato) | Estratégia |
|-------------------|------------------------------|------------|
| EMITENTE_RAZAOSOCIAL | RazaoSocial | Direto |
| EMITENTE_FANTASIA | NomeFantasia | Direto; se NULL → NOMEREDUZIDO |
| EMITENTE_CNPJ | CNPJ | Direto (normalizar: apenas dígitos) |
| EMITENTE_IE | InscricaoEstadual | Direto |
| EMITENTE_UF | UF | Direto |
| NOMEREDUZIDO | — | Usar como NomeFantasia se EMITENTE_FANTASIA for NULL |
| Sguid | Id | `CONVERT(uniqueidentifier, Sguid)` |

**Campos obrigatórios sem equivalente no legado:**

| Campo | Valor padrão |
|-------|--------------|
| Endereco | `'A preencher'` |
| Numero | `'S/N'` |
| Bairro | `'A preencher'` |
| Cidade | `'A preencher'` |
| CEP | `'00000000'` |
| Presidente | `'A preencher'` |
| CPFPresidente | `'00000000000000'` |
| Secretario | NULL |
| CPFSecretario | NULL |
| Complemento | NULL |
| Telefone, Celular, Email, Website | NULL |
| TextoAutenticacao, CartorioResponsavel, EnderecoCartorio | NULL |
| CriadoEm | GETUTCDATE() |

---

## 3. Ordem de Execução

1. **ConfiguracoesSindicato** (EMPRESAS) — sem dependências.
2. **Associados** (PESSOAS + JOIN BANCOS) — sem dependências no novo schema.

---

## 4. Campos Legado sem Coluna no Novo (PESSOAS)

| Campo | Situação | Proposta |
|-------|----------|----------|
| CARTEIRINHA | Sem coluna em Associados | Incluir coluna `Carteirinha` (nvarchar) via migration |
| BASE | Sem coluna em Associados | Incluir coluna `Base` (nvarchar) via migration |
| MOTIVO | Sem coluna em Associados | Incluir coluna `Motivo` (nvarchar) via migration |

**Decisão:** Para “não perder nada”, adicionar as três colunas em Associados antes da importação.

---

## 5. Implementação Proposta

### Opção A: Console Application C# (recomendada)

- Duas connection strings (legado leitura, novo escrita).
- Dapper ou ADO para leitura do legado.
- EF Core para escrita no novo.
- Validações (CPF, conversão de Guid).
- Log de erros e contagem de registros.

### Opção B: Script SQL (INSERT...SELECT)

- Possível para PESSOAS e EMPRESAS.
- Exige conversão de Sguid e JOIN com BANCOS.
- Sem validações complexas.

### Opção C: Híbrido

- SQL para ConfiguracoesSindicato (mais simples).
- C# para Associados (mais controle e validação).

---

## 6. Checklist Pré-Importação

- [ ] Banco Sintraf_GV criado e acessível.
- [ ] Colunas Carteirinha, Base, Motivo em Associados (se optar por preservar).
- [ ] Connection string do legado testada (somente leitura).
- [ ] Backup do banco Sintraf_GV antes da primeira importação.

---

## 7. Resumo do Plano

| Etapa | Ação |
|-------|------|
| 1 | Adicionar colunas Carteirinha, Base, Motivo em Associados (migration) |
| 2 | Criar projeto/tool de importação (Console C# ou SQL) |
| 3 | Migrar EMPRESAS → ConfiguracoesSindicato |
| 4 | Migrar PESSOAS (com JOIN BANCOS) → Associados |
| 5 | Validar contagens e amostras de dados |

---

*Aguardando confirmação para iniciar implementação.*
