# Backend SintrafGV

API .NET 8 em **Clean Architecture**: Domain, Application, Infrastructure, Api.

## Projetos

| Projeto | Responsabilidade |
|---------|-------------------|
| **SintrafGv.Domain** | Entidades (Associado, etc.), sem dependências externas |
| **SintrafGv.Application** | Serviços, DTOs, interfaces de repositório |
| **SintrafGv.Infrastructure** | EF Core, SQL Server, repositórios |
| **SintrafGv.Api** | Controllers, configuração, DI |

## Pré-requisitos

- .NET 8 SDK
- SQL Server (em dev: `docker-compose up -d` na pasta `docker` da raiz)

## Rodar

```bash
# Na raiz do repositório: subir SQL Server
cd docker && docker-compose up -d && cd ../..

# Aplicar migrações (cria o banco)
cd src/backend
dotnet ef database update --project src/SintrafGv.Infrastructure --startup-project src/SintrafGv.Api

# Executar a API
dotnet run --project src/SintrafGv.Api
```

API: `https://localhost:7xxx` ou `http://localhost:5xxx` (ver `launchSettings.json`). Swagger em `/swagger`.

## Endpoints (exemplo)

- `POST /api/auth/login` – login (body: `{ "email": "...", "password": "..." }`). Retorna `{ success, data: { token, user } }`.
- `GET/POST /api/associados` – listar (paginação) e criar associado
- `GET/PUT /api/associados/{id}` – obter e atualizar associado

**Usuário inicial (seed):** `admin@sintrafgv.com.br` / `Admin@123` (criado na primeira execução se não existir nenhum usuário).

Connection string em `SintrafGv.Api/appsettings.Development.json` (DefaultConnection). JWT em `appsettings` (seção Jwt).
