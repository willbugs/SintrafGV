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
