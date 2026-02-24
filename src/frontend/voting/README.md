# SintrafGV - PWA de Votação

Progressive Web App (PWA) para votação dos associados do SintrafGV.

## 🎯 Funcionalidades

- **Login Triplo Seguro**: CPF + Data nascimento + Matrícula bancária
- **Interface Responsiva**: Otimizada para dispositivos móveis
- **PWA Completo**: Instalável como app nativo
- **Fluxo de Votação Intuitivo**: Wizard passo a passo
- **Comprovante Digital**: Com QR code e hash de segurança
- **Offline Support**: Funcionalidade básica sem conexão

## 🚀 Tecnologias

- **React 18** com TypeScript
- **Vite** para build e desenvolvimento
- **Material UI (MUI)** para interface
- **React Router** para navegação
- **Axios** para API calls
- **Vite PWA Plugin** para funcionalidades PWA

## 📱 Recursos PWA

- ✅ **Service Worker** para cache e offline
- ✅ **Web App Manifest** para instalação
- ✅ **Responsivo** para todos os dispositivos
- ✅ **Ícones otimizados** para diferentes tamanhos
- ✅ **Tema personalizado** do SintrafGV

## 🔒 Segurança

### Login Seguro
- **CPF** com máscara de formatação
- **Data de nascimento** com validação
- **Matrícula bancária** com campo protegido
- **Token JWT** para sessões autenticadas

### Integridade de Votos
- **Hash SHA-256** para cada voto
- **Comprovante único** com número sequencial
- **Auditoria completa** sem comprometer sigilo
- **Separação** entre identidade e escolhas

## 📋 Fluxo de Uso

1. **Login**: Associado insere CPF + Data nascimento + Matrícula
2. **Eleições**: Lista eleições disponíveis para votação
3. **Votação**: Wizard passo a passo com cada pergunta
4. **Confirmação**: Revisão das escolhas antes de confirmar
5. **Comprovante**: Geração automática com hash de integridade

## 🛠️ Desenvolvimento

### Pré-requisitos
- Node.js 18+
- npm ou yarn

### Instalação
```bash
cd src/frontend/voting
npm install
```

### Desenvolvimento
```bash
npm run dev
```

### Build para Produção
```bash
npm run build
```

### Preview da Build
```bash
npm run preview
```

## 📦 Estrutura do Projeto

```
src/
├── components/          # Componentes reutilizáveis
│   └── PrivateRoute.tsx
├── contexts/           # Contextos React
│   └── AuthContext.tsx
├── pages/             # Páginas principais
│   ├── LoginPage.tsx
│   ├── EleicoesPage.tsx
│   ├── VotacaoPage.tsx
│   └── ComprovantePage.tsx
├── services/          # Serviços e APIs
│   └── api.ts
├── App.tsx           # Componente principal
├── App.css          # Estilos globais
└── main.tsx        # Entry point
```

## 🔧 Configuração

### Variáveis de Ambiente
Criar arquivo `.env` na raiz:

```env
VITE_API_URL=https://api.sintrafgv.com.br/api
```

### Configuração da API
O app se conecta automaticamente com o backend .NET através do `api.ts`.

## 📱 Instalação como PWA

### Android
1. Abrir no Chrome/Edge
2. Tocar no menu "⋮"
3. Selecionar "Adicionar à tela inicial"

### iOS
1. Abrir no Safari
2. Tocar no botão "Compartilhar"
3. Selecionar "Adicionar à Tela de Início"

### Desktop
1. Abrir no Chrome/Edge
2. Clicar no ícone "Instalar" na barra de endereços
3. Confirmar instalação

## 🎨 Personalização

### Cores do Tema
Definidas em `App.tsx`:
- **Primary**: #1976d2 (azul SintrafGV)
- **Secondary**: #dc004e (vermelho destaque)

### Ícones PWA
Localizados em `/public/`:
- `pwa-192x192.png` - Ícone pequeno
- `pwa-512x512.png` - Ícone grande

## 🧪 Testes

### Validações Implementadas
- **Autenticação** com tripla validação
- **Navegação** protegida por rotas privadas
- **Votação** com validação de elegibilidade
- **Integridade** com hash de segurança

### Cenários Testados
- ✅ Login com dados corretos/incorretos
- ✅ Navegação entre páginas
- ✅ Fluxo completo de votação
- ✅ Geração de comprovante
- ✅ Logout e sessão expirada

## 📊 Performance

### Otimizações Implementadas
- **Code splitting** automático por rotas
- **Lazy loading** de componentes
- **Service Worker** para cache inteligente
- **Minificação** automática na build
- **Tree shaking** para reduzir bundle

### Métricas Esperadas
- **First Paint**: < 1.5s
- **Interactive**: < 2.5s
- **Bundle size**: < 500KB gzipped

## 🔄 Deploy

### Build de Produção
```bash
npm run build
```

### Arquivos Gerados
- `/dist/` - Arquivos estáticos prontos
- `manifest.json` - Configuração PWA
- `sw.js` - Service Worker

### Servidor Web
Qualquer servidor que suporte SPA:
- Nginx
- Apache
- IIS
- Vercel/Netlify

## 📞 Suporte

Para dúvidas técnicas:
- **Email**: suporte@sintrafgv.com.br
- **Telefone**: (11) 1234-5678

---

*SintrafGV - Sistema de Votação Digital*  
*Versão 1.0 - Fevereiro 2026*