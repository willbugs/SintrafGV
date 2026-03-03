# Testar Email - SintrafGV

Script para testar envio de e-mail **fora da API**, usando os dados da tabela `ConfiguracoesEmail`.

- **Timeout**: 60 segundos (evita o erro 30000ms da API)
- **Sem dependência** da API ou IIS

## Uso

```powershell
cd d:\progs\Sintrafgv\src\backend\src\TestarEmail

# Usar connection string padrão (desenvolvimento)
dotnet run -- seu-email@exemplo.com

# Ou definir connection string via variável de ambiente
$env:ConnectionStrings__DefaultConnection = "Data Source=...;Initial Catalog=Sintraf_GV;..."
dotnet run -- seu-email@exemplo.com
```

## Connection string

Se não definir `ConnectionStrings__DefaultConnection`, o script usa a string padrão de desenvolvimento. Para produção, defina a variável antes de rodar.
