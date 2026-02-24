# Sistema de Votações e Enquetes - SintrafGV

## 1. Visão Geral

Sistema de votações eletrônicas para o sindicato SintrafGV, permitindo realizar enquetes simples e eleições complexas com múltiplas perguntas/cargos.

### 1.1 Objetivos
- Modernizar o sistema legado de enquetes (atualmente limitado a 2 opções por pergunta)
- Suportar eleições com múltiplas perguntas/cargos
- Executar em Web, Android e iOS via WebView moderno (PWA)
- Garantir segurança e auditabilidade do processo eleitoral

### 1.2 Plataformas Alvo
- **Web**: React + Vite + MUI (responsivo)
- **Mobile**: WebView nativo (Android/iOS) com acesso a recursos do dispositivo
  - Câmera para validação facial (futuro)
  - Biometria para autenticação
  - Push notifications para avisos de votação
  - Geolocalização (opcional, para auditoria)

---

## 2. Análise do Sistema Legado

### 2.1 Estrutura Atual (CEnquetes)
```
CEnquetes
├── DATA (DateTime) - Início da votação
├── HORAINICIO (string) - Hora início
├── DATARESULTADO (DateTime) - Fim da votação
├── HORAFINAL (string) - Hora fim
├── TITULO (string) - Título da enquete
├── DESCRICAO (string) - Descrição
├── ARQUIVOANEXO (string) - PDF/documento anexo
├── BANCO (CBancos) - Filtro por banco (ou null = todos)
├── ATIVO (bool) - Enquete ativa
├── ASSOCIADO (bool) - Apenas associados podem votar
├── PERGUNTA (string) - A pergunta
├── RESPOSTA01 (string) - Opção 1 (padrão: "SIM")
├── RESPOSTA02 (string) - Opção 2 (padrão: "NÃO")
```

### 2.2 Limitações do Sistema Legado
1. **Apenas 2 opções de resposta** (SIM/NÃO)
2. **Uma pergunta por enquete** - não suporta eleições com múltiplos cargos
3. **Sem candidatos** - não há estrutura para chapa/candidato
4. **Sem foto/perfil** - candidatos não têm apresentação visual
5. **Filtro básico** - apenas por banco ou todos
6. **Sem auditoria robusta** - log simples de login

### 2.3 Estrutura de Votos (CPessoasEnquetes)
```
CPessoasEnquetes
├── PESSOA (CPessoas) - Quem votou
├── ENQUETE (CEnquetes) - Em qual enquete
├── PERGUNTA (string) - Texto da pergunta
├── RESPOSTA01 (int) - 1 se votou opção 1
├── RESPOSTA02 (int) - 1 se votou opção 2
├── DATA (DateTime) - Quando votou
```

---

## 3. Nova Arquitetura

### 3.1 Entidades Principais

#### Eleicao (substitui CEnquetes)
```csharp
public class Eleicao
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string? ArquivoAnexo { get; set; }  // PDF com regulamento
    
    // Período de votação
    public DateTime InicioVotacao { get; set; }
    public DateTime FimVotacao { get; set; }
    
    // Configurações
    public TipoEleicao Tipo { get; set; }  // Enquete, Eleicao
    public bool ApenasAssociados { get; set; }
    public bool ApenasAtivos { get; set; }
    public Guid? BancoId { get; set; }  // Filtro opcional
    
    // Status
    public StatusEleicao Status { get; set; }  // Rascunho, Aberta, Encerrada, Apurada
    
    // Relacionamentos
    public ICollection<Pergunta> Perguntas { get; set; }
    public ICollection<Voto> Votos { get; set; }
    
    // Auditoria
    public DateTime CriadoEm { get; set; }
    public Guid CriadoPor { get; set; }
}

public enum TipoEleicao { Enquete, Eleicao }
public enum StatusEleicao { Rascunho, Aberta, Encerrada, Apurada, Cancelada }
```

#### Pergunta
```csharp
public class Pergunta
{
    public Guid Id { get; set; }
    public Guid EleicaoId { get; set; }
    public int Ordem { get; set; }
    public string Texto { get; set; }  // Ex: "Presidente", "Conselho Fiscal"
    public string? Descricao { get; set; }
    public TipoPergunta Tipo { get; set; }  // UnicoVoto, MultiploVoto
    public int? MaxVotos { get; set; }  // Para múltipla escolha
    public bool PermiteBranco { get; set; }
    
    public ICollection<Opcao> Opcoes { get; set; }
}

public enum TipoPergunta { UnicoVoto, MultiploVoto }
```

#### Opcao (Candidato/Resposta)
```csharp
public class Opcao
{
    public Guid Id { get; set; }
    public Guid PerguntaId { get; set; }
    public int Ordem { get; set; }
    public string Texto { get; set; }  // Nome ou "SIM"/"NÃO"
    public string? Descricao { get; set; }  // Proposta, biografia
    public string? Foto { get; set; }  // URL da foto
    public string? Chapa { get; set; }  // Número/nome da chapa
    
    // Contagem (preenchido na apuração)
    public int TotalVotos { get; set; }
}
```

#### Voto
```csharp
public class Voto
{
    public Guid Id { get; set; }
    public Guid EleicaoId { get; set; }
    public Guid AssociadoId { get; set; }
    public DateTime DataHora { get; set; }
    
    // Auditoria (sem identificar o voto)
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Dispositivo { get; set; }  // Web, Android, iOS
    
    // Hash para garantir integridade
    public string HashVoto { get; set; }
}
```

#### VotoDetalhe (separado para sigilo)
```csharp
public class VotoDetalhe
{
    public Guid Id { get; set; }
    public Guid EleicaoId { get; set; }
    public Guid PerguntaId { get; set; }
    public Guid OpcaoId { get; set; }  // ou null para branco
    public DateTime DataHora { get; set; }
    
    // SEM AssociadoId - garante sigilo do voto
}
```

### 3.2 Fluxo de Votação

```
1. Associado faz login (CPF + Data Nascimento ou biometria)
2. Sistema lista eleições disponíveis para ele
3. Associado seleciona uma eleição
4. Para cada pergunta:
   a. Exibe a pergunta e opções
   b. Associado seleciona sua resposta
   c. Próxima pergunta
5. Tela de confirmação com resumo
6. Confirma voto (com biometria no mobile)
7. Registro do voto + comprovante
```

---

## 4. Painel Administrativo

### 4.1 Funcionalidades

#### Listagem de Eleições
- Tabela com: Título, Tipo, Período, Status, Participação, Ações
- Filtros: Por status, tipo, período
- Ações: Ver, Editar, Duplicar, Encerrar, Apurar

#### Criar/Editar Eleição
```
Dados Básicos
├── Título
├── Descrição
├── Tipo (Enquete / Eleição)
├── Arquivo anexo (PDF)
└── Período de votação (data/hora início e fim)

Configurações
├── Apenas associados ativos
├── Filtro por banco
└── Permitir voto em branco

Perguntas
├── [+ Adicionar Pergunta]
├── Pergunta 1
│   ├── Texto: "Presidente"
│   ├── Tipo: Único voto
│   └── Opções
│       ├── [+ Adicionar Opção]
│       ├── Opção 1: "João Silva" [foto] [remover]
│       └── Opção 2: "Maria Santos" [foto] [remover]
└── Pergunta 2
    ├── Texto: "Conselho Fiscal (escolha até 3)"
    ├── Tipo: Múltiplo voto (max: 3)
    └── Opções
        ├── Opção 1: "Carlos Lima"
        ├── Opção 2: "Ana Paula"
        └── Opção 3: "Roberto Costa"
```

#### Dashboard de Acompanhamento
- Total de eleitores aptos
- Total de votos registrados
- Percentual de participação
- Gráfico de votos por hora (sem revelar opções)
- Lista de quem votou (sem revelar em quem)

#### Apuração
- Só disponível após encerramento
- Gera relatório com totais por opção
- Gráficos de resultado
- Exportar para PDF

---

## 5. Frontend de Votação (PWA)

### 5.1 Tecnologias
- **React + Vite + MUI** (mesmo stack do admin)
- **PWA** com Service Worker para offline parcial
- **WebView** nativo para Android/iOS

### 5.2 Recursos Nativos via WebView
```javascript
// Interface JavaScript <-> Nativo
window.SintrafGV = {
  // Biometria
  requestBiometric: () => Promise<boolean>,
  
  // Push Notifications
  registerPush: (token: string) => void,
  
  // Dispositivo
  getDeviceInfo: () => { platform, version, model },
  
  // Câmera (futuro - validação facial)
  requestCamera: () => Promise<string>, // base64 da foto
}
```

### 5.3 Telas

#### Login
- CPF (com máscara)
- Data de Nascimento
- Botão "Entrar com Biometria" (se disponível)

#### Lista de Eleições
- Cards com: Título, Período, Status (Votar/Já votou/Encerrada)
- Pull-to-refresh

#### Votação
- Wizard passo a passo (uma pergunta por tela)
- Opções com foto do candidato (se eleição)
- Botão "Próximo" / "Anterior"
- Barra de progresso

#### Confirmação
- Resumo de todas as escolhas
- Checkbox "Confirmo meu voto"
- Botão "Confirmar Voto" (solicita biometria)

#### Comprovante
- Hash único do voto
- Data/hora
- Opção de capturar screenshot

#### Resultados
- Apenas para eleições apuradas
- Gráficos de barras horizontais
- Números absolutos e percentuais

---

## 6. Segurança

### 6.1 Autenticação
- JWT com refresh token
- Validação de CPF + Data Nascimento + Matrícula
- Biometria do dispositivo (opcional, mas recomendado)

### 6.2 Integridade do Voto
- Hash SHA-256 do voto (eleicao + perguntas + timestamp)
- Separação entre `Voto` (quem votou) e `VotoDetalhe` (em quem votou)
- Impossível associar pessoa ao voto específico

### 6.3 Auditoria
- Log de todos os acessos
- IP, User-Agent, Dispositivo
- Timestamp de cada ação

### 6.4 Prevenção de Fraude
- Um voto por associado por eleição
- Validação server-side do período de votação
- Rate limiting para prevenir ataques

---

## 7. Migração do Legado

### 7.1 Mapeamento de Dados
```
CEnquetes → Eleicao
├── TITULO → Titulo
├── DESCRICAO → Descricao
├── DATA + HORAINICIO → InicioVotacao
├── DATARESULTADO + HORAFINAL → FimVotacao
├── ATIVO → Status
├── ASSOCIADO → ApenasAssociados
├── BANCO → BancoId
├── PERGUNTA → Perguntas[0].Texto
├── RESPOSTA01 → Perguntas[0].Opcoes[0].Texto
└── RESPOSTA02 → Perguntas[0].Opcoes[1].Texto

CPessoasEnquetes → Voto + VotoDetalhe
├── PESSOA → AssociadoId (em Voto)
├── ENQUETE → EleicaoId
├── DATA → DataHora
├── RESPOSTA01 → VotoDetalhe com OpcaoId da opção 1
└── RESPOSTA02 → VotoDetalhe com OpcaoId da opção 2
```

### 7.2 Script de Migração
- Criar eleições a partir das enquetes existentes
- Preservar histórico de votos
- Manter compatibilidade com relatórios antigos

---

## 8. Roadmap

### Fase 1: MVP Admin ✅ EM PROGRESSO
**Backend:**
- [x] Entidades: Eleicao, Pergunta, Opcao, Voto, VotoDetalhe
- [x] Enums: TipoEleicao, StatusEleicao, TipoPergunta
- [x] DTOs para CRUD e resultados
- [x] Repository e Service de Eleições
- [x] Controller com endpoints CRUD
- [x] Migrations aplicadas (tabelas criadas)
- [ ] Endpoint de apuração/resultados
- [ ] Testes unitários

**Frontend Admin:**
- [x] Tipos TypeScript para eleições
- [x] API client (eleicoesAPI)
- [x] Menu "Eleições" no layout
- [x] Rotas configuradas (/eleicoes, /eleicoes/novo, /eleicoes/:id)
- [x] Página de listagem (EleicoesPage)
- [x] Página de criar/editar com perguntas e opções (EleicaoFormPage)
- [ ] Gerenciamento de status (abrir/encerrar/cancelar)
- [ ] Dashboard com estatísticas de votação
- [ ] Página de visualização de resultados/apuração

### Fase 2: Frontend Votação Web (Associado)
- [ ] Projeto separado para votação (ou rotas públicas)
- [ ] Login do associado (CPF + Data Nascimento)
- [ ] Listagem de eleições disponíveis
- [ ] Fluxo de votação (wizard passo a passo)
- [ ] Confirmação e comprovante
- [ ] Visualização de resultados (após apuração)

### Fase 3: PWA + Mobile
- [ ] Configuração PWA (manifest, service worker)
- [ ] Wrapper Android (WebView)
- [ ] Wrapper iOS (WKWebView)
- [ ] Integração biometria do dispositivo
- [ ] Offline parcial

### Fase 4: Recursos Avançados
- [ ] Push notifications
- [ ] Validação facial (reconhecimento)
- [ ] Relatórios avançados e exportação PDF
- [ ] Migração de dados do sistema legado
- [ ] Auditoria completa com logs detalhados

---

## 9. Considerações Técnicas

### 9.1 Performance
- Lazy loading de imagens de candidatos
- Cache de eleições no dispositivo
- Otimização para conexões lentas

### 9.2 Acessibilidade
- Suporte a leitores de tela
- Alto contraste
- Tamanho de fonte ajustável

### 9.3 Internacionalização
- Preparado para múltiplos idiomas (futuro)
- Formato de data/hora brasileiro

---

## 10. Conclusão

Este documento define a arquitetura do novo sistema de votações do SintrafGV, modernizando o sistema legado e adicionando suporte a:

1. **Múltiplas perguntas** por eleição
2. **Múltiplas opções** de resposta
3. **Candidatos com foto** e descrição
4. **Plataformas móveis** via WebView
5. **Segurança robusta** com separação de sigilo
6. **Auditoria completa** do processo

O sistema será desenvolvido incrementalmente, começando pelo painel administrativo e evoluindo para o frontend de votação com suporte móvel.
