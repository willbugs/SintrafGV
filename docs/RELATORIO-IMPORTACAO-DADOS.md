# Relatório: Importação de Dados do Banco Legado

**Banco origem:** Sintrafgv (produção)  
**Banco destino:** Sintraf_GV (nova plataforma)  
**Restrição:** Apenas leitura no legado — nenhuma alteração no banco de produção.

---

## 1. Mapeamento de Entidades

| Legado (Sintrafgv) | Novo (Sintraf_GV) | Complexidade |
|--------------------|-------------------|--------------|
| PESSOAS | Associados | Média |
| BANCOS | (referência → string em Associados) | Baixa |
| EMPRESAS | ConfiguracoesSindicato | Média (parcial) |
| USUARIOS | Usuarios | Alta (senha) |
| ENQUETES | Eleicoes + Perguntas + Opcoes | Alta |
| PESSOASENQUETES | Votos + VotosDetalhes | Muito alta |
| MENUS, SUBMENUS, TELAS, CAMPOS, RELATORIOS | — | Não migrar (modelo estático) |
| PARAMETROS, ERROS, LOGENQUETELOGIN | — | Opcional |

---

## 2. Detalhamento por Entidade

### 2.1 PESSOAS → Associados

| Legado | Novo | Observação |
|--------|------|------------|
| Sguid | Id | Converter string → Guid (`CONVERT(uniqueidentifier, Sguid)`) |
| NOME | Nome | Direto |
| CPF | Cpf | Direto |
| MATRICULASINDICATO | MatriculaSindicato | Direto |
| MATRICULABANCARIA | MatriculaBancaria | Direto |
| BANCO (FK) | Banco | JOIN com BANCOS para obter NOME |
| SEXO, ESTADOCIVIL, DATNASCIMENTO, NATURALIDADE | Idem | Direto |
| ENDERECO, BAIRRO, CIDADE, ESTADO | Idem | Direto |
| CODAGENCIA, AGENCIA, CONTA | CodAgencia, Agencia, Conta | Direto |
| FUNCAO, CTPS, SERIE | Idem | Direto |
| DATADMISSAO, DATFILIACAO, DATDESLIGAMENTO | DataAdmissao, DataFiliacao, DataDesligamento | Direto |
| CELULAR, TELEFONE, EMAIL | Idem | Direto |
| ATIVO | Ativo | Direto |
| APOSENTADO | Aposentado | Direto |
| DTULTIMAATUALIZACAO | DataUltimaAtualizacao | Direto |
| — | CriadoEm | Usar `GETUTCDATE()` ou DTULTIMAATUALIZACAO |

**Pontos de atenção:** CEP e Complemento não existem no legado; preencher NULL.

---

### 2.2 USUARIOS → Usuarios

| Legado | Novo | Observação |
|--------|------|------------|
| Sguid | Id | Converter para Guid |
| NOMEUSUARIO | Nome | Direto |
| LOGINUSUARIO | Email | Se não for e-mail, usar `LOGINUSUARIO + @sintrafgv.local` |
| SENHA | SenhaHash | **Crítico:** legado usa texto plano; novo exige hash (BCrypt/ASP.NET Identity) |
| TIPOUSUARIO (A/E/P) | Role | Mapear: A→Admin, E→User, P→User |
| — | Ativo | Default true |
| — | CriadoEm | GETUTCDATE() |

**Recomendação:** Migração de usuários deve ser feita em **código C#** para aplicar hash corretamente.

---

### 2.3 EMPRESAS → ConfiguracoesSindicato

| Legado | Novo | Observação |
|--------|------|------------|
| EMITENTE_RAZAOSOCIAL | RazaoSocial | Direto |
| EMITENTE_FANTASIA | NomeFantasia | Direto |
| EMITENTE_CNPJ | CNPJ | Direto |
| EMITENTE_IE | InscricaoEstadual | Direto |
| EMITENTE_UF | UF | Direto |
| — | Endereco, Numero, Bairro, Cidade, CEP, etc. | Legado não tem; preencher vazio ou manual |

**Observação:** EMPRESAS tem dados limitados. ConfiguracoesSindicato exige mais campos. Migração parcial + complemento manual.

---

### 2.4 ENQUETES → Eleicoes + Perguntas + Opcoes

**Estrutura legada:** Uma enquete = uma pergunta com duas opções (RESPOSTA01, RESPOSTA02 como texto, ex: "SIM"/"NÃO").

**Estrutura nova:** Eleicao → Perguntas (1:N) → Opcoes (1:N).

| Legado | Novo |
|--------|------|
| Sguid | Eleicao.Id |
| TITULO | Eleicao.Titulo |
| DESCRICAO | Eleicao.Descricao |
| ARQUIVOANEXO | Eleicao.ArquivoAnexo |
| DATA + HORAINICIO | Eleicao.InicioVotacao |
| DATARESULTADO + HORAFINAL | Eleicao.FimVotacao |
| ASSOCIADO | Eleicao.ApenasAssociados |
| ATIVO | Eleicao.Status (Aberta/Encerrada) |
| PERGUNTA | Pergunta.Texto (1 pergunta por enquete) |
| RESPOSTA01 | Opcao[0].Texto |
| RESPOSTA02 | Opcao[1].Texto |

**Lógica:** Para cada ENQUETE criar 1 Eleicao, 1 Pergunta, 2 Opcoes.

---

### 2.5 PESSOASENQUETES → Votos + VotosDetalhes

**Estrutura legada:** Uma linha = um voto. RESPOSTA01=1 e RESPOSTA02=0 → votou na opção 1; RESPOSTA01=0 e RESPOSTA02=1 → votou na opção 2.

**Estrutura nova:** 
- **Voto:** AssociadoId, EleicaoId, DataHoraVoto, HashVoto, RespostaCriptografada, ChaveCriptografia, TimestampPreciso (obrigatórios).
- **VotoDetalhe:** PerguntaId, OpcaoId (ou null se branco), DataHora, VotoBranco.

**Problema:** O novo modelo exige campos de autenticação cartorial (HashVoto, RespostaCriptografada, etc.) que o legado não possui. Para votos históricos, é necessário usar valores placeholder (ex.: hash fictício, timestamp da migração).

**Lógica de migração:**
1. Para cada PESSOASENQUETES: obter PESSOA (→ AssociadoId), ENQUETE (→ EleicaoId).
2. Criar Voto com valores placeholder nos campos cartoriais.
3. Obter PerguntaId (única pergunta da Eleicao migrada).
4. Se RESPOSTA01=1 → OpcaoId = primeira Opcao da pergunta.
5. Se RESPOSTA02=1 → OpcaoId = segunda Opcao da pergunta.
6. Criar VotoDetalhe correspondente.

---

## 3. Melhor Abordagem para Importação

### Recomendação: **Console Application em C# (.NET 8)**

| Critério | SQL puro | Console C# |
|----------|----------|------------|
| Hash de senha (Usuarios) | ❌ Não suporta | ✅ Sim |
| Conversão Sguid → Guid | ⚠️ Possível | ✅ Simples |
| Lógica ENQUETES → Eleicoes | ❌ Complexo | ✅ Clara |
| PESSOASENQUETES → Votos | ❌ Muito complexo | ✅ Controlável |
| Validações (CPF, etc.) | ❌ Limitado | ✅ Completo |
| Rollback / transações | ⚠️ Manual | ✅ EF Core |
| Log de erros | ❌ Limitado | ✅ Detalhado |

### Estrutura sugerida do projeto de importação

```
src/
  ImportacaoLegado/
    ImportacaoLegado.csproj          # Console app
    Program.cs
    Services/
      AssociadoImportService.cs      # PESSOAS → Associados
      UsuarioImportService.cs        # USUARIOS → Usuarios (com hash)
      EmpresaImportService.cs       # EMPRESAS → ConfiguracoesSindicato
      EnqueteImportService.cs       # ENQUETES → Eleicoes+Perguntas+Opcoes
      VotoImportService.cs          # PESSOASENQUETES → Votos+VotosDetalhes
    Mappings/
      LegadoToNovoMapping.cs         # Dicionários Sguid legado → Guid novo
```

### Ordem de execução

1. **Associados** (PESSOAS) — sem dependências no novo schema.
2. **Usuarios** (USUARIOS) — sem dependências.
3. **ConfiguracoesSindicato** (EMPRESAS) — opcional.
4. **Eleicoes + Perguntas + Opcoes** (ENQUETES).
5. **Votos + VotosDetalhes** (PESSOASENQUETES) — depende de Associados e Eleicoes.

### Conexões

- **Legado (leitura):** `Data Source=127.0.0.1;Initial Catalog=Sintrafgv;User Id=Durval;Password=Lspxmw01oz;TrustServerCertificate=True;`
- **Novo (leitura/escrita):** `Data Source=127.0.0.1;Initial Catalog=Sintraf_GV;User Id=Durval;Password=Lspxmw01oz;TrustServerCertificate=True;`

---

## 4. Alternativa: SQL com limitações

Se preferir **apenas SQL** (sem C#):

- **Associados:** Possível com `INSERT...SELECT` e `CONVERT(uniqueidentifier, Sguid)`.
- **Usuarios:** Não recomendado — senhas em texto plano no novo banco.
- **Eleicoes/Perguntas/Opcoes:** Possível, porém com scripts longos e complexos.
- **Votos:** Muito complexo — campos obrigatórios sem equivalente no legado.

---

## 5. Resumo

| Abordagem | Quando usar |
|-----------|-------------|
| **Console C#** | Recomendado — cobre todos os casos e mantém integridade. |
| **SQL puro** | Apenas Associados e, com cuidado, Eleicoes/Perguntas/Opcoes. |
| **Híbrido** | SQL para Associados; C# para Usuarios, Enquetes e Votos. |

**Próximo passo sugerido:** Criar o projeto `ImportacaoLegado` e implementar os serviços na ordem indicada.

---

*Relatório gerado com base em BASE-LEGADA-SINTRAFGV.md e schema do novo backend.*
