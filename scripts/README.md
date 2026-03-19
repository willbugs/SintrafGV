# Scripts de banco de dados

## Sintraf_GV (nova plataforma)

**Banco legado:** `Sintrafgv` (produção)  
**Banco novo:** `Sintraf_GV` (nova plataforma)

### Executar o script

```powershell
# Via sqlcmd (ajuste Server, User, Password conforme seu ambiente)
sqlcmd -S 127.0.0.1 -U Durval -P Lspxmw01oz -i CreateSintraf_GV.sql

# Ou via SQL Server Management Studio: abra o arquivo e execute (F5)
```

### Connection string

Após criar o banco, configure a API em `appsettings.json` ou `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=127.0.0.1;Initial Catalog=Sintraf_GV;Integrated Security=false;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;User=Durval;Password=Lspxmw01oz"
  }
}
```

### Importação dos dados

Os dados do banco legado **Sintrafgv** devem ser migrados para **Sintraf_GV** via ETL ou scripts de importação. O schema do novo banco é diferente (Associados, Usuarios, Eleicoes etc. vs PESSOAS, USUARIOS, ENQUETES, etc.). Consulte `docs/BASE-LEGADA-SINTRAFGV.md` para o mapeamento das entidades.

---

## Validação de votação (restrição por banco BRADESCO)

O script **`validar-votacao-bradesco.ps1`** confere se a votação restrita a um banco funciona corretamente:

1. **Associado BRADESCO:** vê a enquete em "ativas" e consegue votar (ou recebe 409 se já votou).
2. **Associado de outro banco:** não vê a enquete em "ativas" e, ao tentar votar direto, recebe 400 com mensagem de restrição ao banco.

### Como usar

1. No admin, crie uma enquete com **Banco (opcional)** = **BRADESCO** e anote o **Id** da enquete, o **Id** de uma pergunta e o **Id** de uma opção.
2. Edite o script e preencha no topo:
   - `$base` (ex.: `https://api.sintrafgv.com.br`)
   - `$eleicaoId`, `$perguntaId`, `$opcaoId`
   - `$associadoBradesco`: CPF, data de nascimento e matrícula bancária de um associado cujo cadastro tem Banco = BRADESCO.
   - `$associadoOutroBanco`: mesmo campos de um associado de outro banco.
3. Opcional: se existir `votantes.json` na raiz do projeto e cada item tiver campo `banco`, o script pode usar o primeiro com banco "BRADESCO" e o primeiro com outro banco (preenchendo os dois associados automaticamente).
4. Execute: `.\scripts\validar-votacao-bradesco.ps1`
