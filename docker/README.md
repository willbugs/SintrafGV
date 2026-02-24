# Docker – Desenvolvimento SintrafGV

Em **desenvolvimento** usamos apenas o **SQL Server** em Docker.

## Subir o SQL Server

Na pasta do projeto:

```bash
cd docker
docker-compose up -d
```

## Conexão (dev)

| Parâmetro | Valor |
|-----------|--------|
| Servidor | `localhost,1433` |
| Usuário | `sa` |
| Senha | `SintrafGv_Dev2025!` |

**Importante:** altere a senha no `docker-compose.yml` em ambientes compartilhados e não use essa senha em produção.

## Parar

```bash
docker-compose down
```

Os dados persistem no volume `sqlserver_data`. Para apagar tudo (incluindo dados):

```bash
docker-compose down -v
```
