# SintrafGV – Painel Administrativo

React 18 + Vite + TypeScript + **Material UI (MUI)**. UI baseada no **Bureau**.

## Estrutura

- `src/components/Layout/AdminLayout.tsx` – Sidebar, AppBar, menu (SintrafGV)
- `src/contexts/AuthContext.tsx` – Login mock (qualquer e-mail/senha); trocar por API quando existir
- `src/pages` – Login, Dashboard, Associados (lista da API), placeholders (Usuários, Relatórios, Configurações, Perfil)
- `src/services/api.ts` – axios, authAPI, associadosAPI (base URL: `VITE_API_URL` ou `http://localhost:5066`)
- `src/types/index.ts` – User, Associado, MenuItem, etc.

## Rodar

```bash
npm install
npm run dev
```

**API:** o backend deve estar em `http://localhost:5066` (ou defina `.env` com `VITE_API_URL=`).

**Login:** use o usuário criado pelo backend: `admin@sintrafgv.com.br` / `Admin@123`.

## Build

```bash
npm run build
```
