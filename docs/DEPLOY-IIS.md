# Deploy dos Frontends no IIS

## Admin e Voting

Os dois frontends (admin e voting) são SPAs React/Vite. Para rodar no IIS:

### 1. Build de produção

```powershell
# Admin
cd src/frontend/admin
npm run build

# Voting
cd src/frontend/voting
npm run build
```

O build gera a pasta `dist/` com os arquivos estáticos (otimizados para produção).

### 2. Configurar API URL (produção)

Antes do build, crie `.env.production` (ou defina a variável):

```env
VITE_API_URL=https://sua-api.com.br
```

Ou copie o exemplo:

```powershell
copy .env.production.example .env.production
# Edite .env.production com a URL correta da API
```

### 3. IIS (URL Rewrite)

O `web.config` já está em `public/` e é copiado para `dist/` no build. Ele configura o SPA fallback: rotas como `/associados`, `/login` etc. são redirecionadas para `index.html`.

**Requisito:** Módulo **URL Rewrite** do IIS instalado.

### 4. Publicar no IIS

1. Crie um **Site** ou **Aplicação** no IIS.
2. Defina o **caminho físico** apontando para a pasta `dist/` do build.
3. Para cada frontend:
   - **Admin:** `src/frontend/admin/dist`
   - **Voting:** `src/frontend/voting/dist`

### 5. Estrutura sugerida

| Site/Aplicação | Caminho físico | URL |
|----------------|----------------|-----|
| Admin | `...\admin\dist` | `/` ou `admin.sintrafgv.com.br` |
| Voting | `...\voting\dist` | `/` ou `votacao.sintrafgv.com.br` |

Se usar sub-aplicações no mesmo site (ex.: `/admin` e `/votacao`), será necessário ajustar o `base` no `vite.config.ts` e o `web.config`.

---

**Build = release:** O comando `npm run build` já gera o build de produção (minificado, otimizado). Não há "modo semente" — use sempre `npm run build` para deploy.
