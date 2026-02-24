# Referência de UI – Bureau (base de conhecimento)

Documento gerado a partir da investigação do Bureau (`D:\progs\Bureau\frontend\admin-panel`). O SintrafGV admin deve seguir estes padrões.

---

## 1. Stack e dependências

- **React** 18, **Vite**, **MUI** (Material UI) 5.16+, **React Router** 6.
- **notistack** ^3.0.2 – toasts (Snackbar).
- **react-hook-form**, **yup** – formulários e validação (em partes do Bureau).

---

## 2. App e tema (App.tsx)

- **Tema:** `createTheme` com:
  - `palette.primary.main`: #1976d2
  - `palette.secondary.main`: #dc004e
  - `palette.background.default`: #f5f5f5 (sem `paper` explícito)
  - `typography`: Roboto, h4 e h6 com `fontWeight: 600`
  - `components`: só `MuiDrawer.paper` (backgroundColor #fff, borderRight 1px #e0e0e0) e `MuiAppBar.root` (#1976d2).
- **Providers (ordem):** AuthProvider → ThemeProvider → CssBaseline → **ToastProvider** → **SnackbarProvider** (notistack: maxSnack 3, anchorOrigin top/right) → AppContent.
- **Rotas:** login e reset-password fora do layout; demais dentro de ProtectedRoute + AdminLayout com Outlet.

---

## 3. Layout (AdminLayout)

- **drawerWidth:** 280.
- **Sidebar:** Toolbar com título (Typography h6, fontWeight bold), Divider, List com ListItemButton; item selecionado: `backgroundColor: primary.main + '20'`, `borderRight: 3px solid primary.main`; ListItemIcon e ListItemText com cor primary quando selecionado.
- **AppBar:** position fixed, width `calc(100% - drawerWidth)` no md+, ml drawerWidth; Toolbar com ícone menu (mobile), Typography flexGrow 1, Avatar (32x32) + Menu (Perfil, Sair com ícones).
- **Main:** `component="main"`, `flexGrow: 1`, `p: 3`, `width: calc(100% - drawerWidth)`; sem `minHeight` nem `backgroundColor` no Box main.
- **Drawer:** temporary em mobile, permanent em md+; mesmo conteúdo nos dois.

---

## 4. Notificações e erros

- **Toast (notistack):** padrão para feedback de ação (sucesso, erro, aviso, info).  
  - **ToastContext:** `useSnackbar()` do notistack; `success(title, message)`, `error(title, message)`, `warning`, `info`; conteúdo com `<strong>{title}</strong><br />{message}`; autoHideDuration 5000; anchorOrigin top/right.
- **Alert (MUI):** usado para **erro em tela** (falha ao carregar, falha ao salvar) nas páginas de listagem/crud.
  - Ex.: UsuariosPage, EmpresasPage, DashboardPage: `{error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}`.
- **Login:** erro de credencial com `<Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>`.
- Algumas páginas (ex.: FaturamentoPage) usam `<Alert severity="error" ... onClose={() => setError(null)}>` (dismissível).
- **Conclusão:** Bureau usa **Alert** para erro persistente na página e **Toast** para feedback de ação (sucesso/erro de operação). Em UsuariosPage o Bureau usa Alert para erro; Toast é usado em outras telas para success/error de ações.

---

## 5. Páginas de listagem (ex.: UsuariosPage, EmpresasPage)

- **Estrutura:** Box → header (flex, space-between, mb: 3) com Typography variant="h4" (sem fontWeight bold no h4 do header) e Button "Novo X" (startIcon Add).
- **Loading:** Box com flex center, `my: 4`, CircularProgress.
- **Erro:** Alert severity="error", `sx={{ mb: 3 }}`.
- **Tabela:** Paper → TableContainer → Table (sem size="small" por padrão); TableHead com TableRow e TableCell; TableBody com map; linha com hover.
- **Coluna “Usuário” (nome):** Box flex alignItems center; **Avatar** (sx={{ mr: 2 }}) com ícone Person dentro; Typography variant="body2" com nome.
- **Perfil/Role:** Chip com função de cor (ex.: Administrador = error, Usuário Empresarial = primary).
- **Status:** Chip (Ativo = success, Inativo = default, Excluído = error).
- **Ações:** IconButton Editar, IconButton Delete (color="error"), size="small".
- **Formulário:** Bureau usa **Dialog** (modal) para novo/editar em UsuariosPage e EmpresasPage. No SintrafGV, por decisão do produto, o form de usuário é em **página** (/usuarios/novo, /usuarios/:id), não em modal.

---

## 6. Login (LoginPage)

- **Container:** Box full viewport, `background: linear-gradient(135deg, primary.main 0%, primary.dark 100%)`, overlay com radial-gradient (alpha white 0.1).
- **Layout:** duas colunas (md): esquerda branding/features (Fade), direita Card do form.
- **Card:** borderRadius 3, boxShadow '0 20px 40px rgba(0,0,0,0.1)', backdropFilter blur(10px), border 1px solid alpha(white, 0.1).
- **CardContent:** p: 4; ícone em círculo (primary), título, subtítulo; erro com Alert severity="error" borderRadius 2; TextField com sx `& .MuiOutlinedInput-root: { borderRadius: 2 }` (sem override de background).
- **Botão submit:** py 1.5, borderRadius 2, fontWeight bold, boxShadow com alpha primary.

---

## 7. Dashboard

- **Métricas:** Grid de Cards (CardContent com Typography overline, h4, trend opcional).
- **Erro de carregamento:** Alert severity="error" sx={{ mb: 3 }}.

---

## 8. Resumo para SintrafGV

| Aspecto              | Bureau                         | Ação no SintrafGV                    |
|----------------------|---------------------------------|--------------------------------------|
| Tema                 | palette + Drawer + AppBar       | Já alinhado; manter.                 |
| Toast                | notistack + ToastProvider       | Adicionar notistack + ToastProvider. |
| Erro em página       | Alert severity="error" mb: 3    | Manter Alert para erro de carga/save.|
| Listagem usuários    | Avatar + Chip cores + 2 ações   | Já alinhado.                         |
| Form usuário         | Dialog (modal)                  | Manter em página (sem modal).        |
| Login                | Gradiente + Card + Alert erro   | Já alinhado.                         |

---

*Documento de referência – Bureau como base de conhecimento para a UI do admin SintrafGV.*
