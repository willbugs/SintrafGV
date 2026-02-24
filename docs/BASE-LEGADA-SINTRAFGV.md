# Descritivo da Base Legada – SintrafGV

Documento de análise da base de conhecimento legada do sistema do sindicato **SintrafGV**, para suporte à criação de uma nova plataforma.  
**Data:** Fevereiro/2025.

---

## 1. Visão geral do sistema

- **Solução:** `Sintrafgv.sln` (Visual Studio, .NET Framework 4.7.1)
- **Localização:** pasta `base conhecimento legado`
- **Tipo:** Aplicação web ASP.NET MVC com múltiplos projetos (Domain, Persistência, Negócios, Web, WebEnquete, Reports, Teste)
- **Banco de dados:** Suporte multi-SGBD (SQL Server, PostgreSQL, MySQL, Firebird) via NHibernate
- **Arquitetura:** Camadas (Domain → Persistência → Negócios → Web), injeção de dependência (Ninject), sessão por requisição/cliente

---

## 2. Estrutura da solução (projetos)

| Projeto      | Função principal |
|-------------|-------------------|
| **Domain**  | Entidades de domínio (modelos), atributos de validação, enums, utilitários |
| **Persistencia** | Mapeamentos Fluent NHibernate (M*), `FluentSessionFactory`, configuração de sessão e cache |
| **Negocios** | Regras de negócio (N*), acesso a dados via NHibernate, e-mail (MailKit) |
| **Web**     | Aplicação principal: MVC, controllers, views, autenticação, relatórios, SignalR |
| **WebEnquete** | Aplicação separada para enquetes (votação de associados) |
| **Reports** | Geração de relatórios (R*), integração com ReportViewer e PDF (iTextSharp, PDFsharp) |
| **Teste**    | Projeto de testes |

**Dependências entre projetos:**  
`Web` → Reports, Negocios, Persistencia, Domain  
`WebEnquete` → Negocios, Persistencia, Domain  
`Negocios` → Persistencia, Domain  
`Persistencia` → Domain  
`Reports` → Domain (e NHibernate para dados)

---

## 3. Stack tecnológica

- **Runtime:** .NET Framework 4.7.1  
- **Web:** ASP.NET MVC 5.2.7, Razor, Owin, IIS  
- **ORM:** NHibernate 5.2.7, Fluent NHibernate 2.1.2  
- **Cache:** NHibernate.Caches.SysCache2 (second-level cache)  
- **DI:** Ninject 4 (beta)  
- **Autenticação:** Microsoft.AspNet.Identity.Core, Owin (Cookies, OAuth)  
- **Relatórios:** Microsoft.ReportingServices.ReportViewerControl.WebForms 150.1404, iTextSharp 5.5.13, PDFsharp 1.51  
- **E-mail:** MailKit 2.6, MimeKit 2.6  
- **Tempo real:** Microsoft.AspNet.SignalR 2.4.1  
- **Front-end:** Bootstrap, jQuery 3.5, CoreUI (template), jQuery Validate, Moment.js  
- **Outros:** Newtonsoft.Json, BouncyCastle (criptografia), Microsoft.SqlServer.Types (geografia)

---

## 4. Banco de dados e persistência

### 4.1 Multi-tenant e conexão

- A conexão é escolhida por **nome do cliente** (ex.: `nCliente`), usado como chave em `ConnectionStrings[nCliente]` no `Web.config`/`app.config`.
- Valor padrão quando vazio: `"PCMAX"`.
- **Enum de SGBD** (`Domain.Util.Banco`): `SQLServer`, `PostgreSQL`, `FirebirdSql`, `MySQL`, `Memory`.

### 4.2 FluentSessionFactory (Persistencia)

- **Método principal:** `AbrirSession(string nBanco = "DATABASE", Banco bancoEscolhido = Banco.SQLServer, bool criarBanco = false)`.
- Lê a connection string da seção com nome `nCliente` (após normalização: trim, uppercase, remoção de `/`).
- Configuração: isolation level `ReadCommitted`, batch size 2000, query cache e second-level cache (SysCache).
- Mapeamentos: carregados da assembly onde está `MUsuarios` (todos os *FluentMappings*).
- Opção `criarBanco = true`: executa `SchemaExport` para criar/atualizar schema no banco.

### 4.3 Injeção de sessão (Web)

- **NinjectDependencyResolver:** faz bind de `ISession` para `FluentSessionFactory.AbrirSession(nBanco)` em escopo transiente, nome `"Default"`.
- O `nBanco` padrão no resolver é `"DATABASE"`; na prática o nome do cliente pode vir do host/virtual path para multi-tenant.

---

## 5. Modelo de domínio (entidades)

### 5.1 Classes base

- **CBase** (Domain): entidade raiz com `Sguid` (string, GUID), `Usuario` (CUsuarios). Usado por entidades principais (Empresa, Usuarios, Pessoas, Enquetes, etc.).
- **CBase1** (Domain): outra base com apenas `Sguid` (sem usuário). Usado por Menus, SubMenus, Telas, Campos, Relatorios, etc.

### 5.2 Entidades principais (tabelas / agregados)

| Entidade | Descrição resumida |
|----------|---------------------|
| **CEmpresa** | Empresa do sindicato: NOMEREDUZIDO, EMITENTE_CNPJ, EMITENTE_IE, EMITENTE_RAZAOSOCIAL, EMITENTE_FANTASIA, EMITENTE_UF |
| **CUsuarios** | Usuário do sistema: LOGINUSUARIO, SENHA, NOMEUSUARIO, TIPOUSUARIO (A/E/P), foto (byte[]), Ultimadataconexao, Ultimoipconecao, Sessao, GrupoUsuarios, listas USUARIOSEMPRESAS e USUARIOSPERMISSOES |
| **CUsuariosEmpresas** | Vínculo usuário–empresa (multi-empresa): PUSUARIO, EMPRESA |
| **CUsuariosPermissoes** | Permissão por tela: Proprietario (usuário), Submenus (CSubMenus), Atela (descrição), Ativo |
| **CGrupoUsuarios** | Grupo de usuários: Nomegrupo, GRUPOUSUARIOSPERMISSOES |
| **CGrupoUsuariosPermissoes** | Permissão do grupo: Proprietario (grupo), Submenus, Atela, Ativo |
| **CPessoas** | Associado/pessoa: NOME, CPF, MATRICULASINDICATO, MATRICULABANCARIA, dados pessoais (sexo, estado civil, endereço, cidade, estado, naturalidade, nascimento), dados bancários (CODAGENCIA, AGENCIA, CONTA, BANCO), FUNCAO, CTPS, SERIE, datas (admissão, filiação, desligamento), CELULAR, TELEFONE, EMAIL, flags ASSOCIADO, ATIVO, APOSENTADO, DEACORDO, NOVO, DTULTIMAATUALIZACAO, listas PESSOASENQUETES e LOGENQUETELOGIN |
| **CBancos** | Banco para contas: NOME, NUMERO |
| **CEnquetes** | Enquete/votação: DATA, HORAINICIO, DATARESULTADO, HORAFINAL, TITULO, DESCRICAO, ARQUIVOANEXO, BANCO (CBancos), ATIVO, ASSOCIADO, PERGUNTA, RESPOSTA01/RESPOSTA02 (textos), arquivo anexo (upload) |
| **CPessoasEnquetes** | Voto de uma pessoa em uma enquete: PESSOA, ENQUETE, PERGUNTA, RESPOSTA01/RESPOSTA02 (contadores int), DATA |
| **CResultadoEnquetes** | Resultado agregado da enquete: ENQUETE, PERGUNTA, RESPOSTA01, RESPOSTA02 (totais) |
| **CLogEnqueteLogin** | Log de acesso na enquete: DATA, PESSOA, CPF, MATRICULA, DATANASCIMENTO |
| **CParametros** | Parâmetros do sistema (e-mail): SMTP, LOGIN, SENHA, PORTA, EMAILATIVACOES |
| **CErros** | Log de erros: IpServidor, Data, Mensagem, Source, StackTrace |

### 5.3 Modelo de menus e telas (sistema dinâmico)

- **CMenus:** Menu (Menu, Acesso, Sequencia, Icone), lista SubMenu, Usuario.
- **CSubMenus:** Submenu (Descricao, Acesso, Tipo: 0 submenu / 1 tela / 2 divisão, NomeTela, Sequencia, Atela), lista Tela e UsuariosParmissoes.
- **CTelas:** Tela (Help, NomeTela, Acesso, Parent, Sequencia, Lista, Novo, Apagar, Seleciona, Importar), listas Campos e Relatorios.
- **CCampos:** Campo da tela: Sequencia, Tela, Descricao, Campo, Visivel, ReadOnly, Tipo (campo, drop, data, valor, memo, drop outra tabela, etc.), Lista, Listar, Source, Mascara, CampoSource, IdCampo, Grid, Chave, FuncaoBusca, Adicionar, Collapse, Unico.
- **CRelatorios:** Relatório da tela: Tela, ReportName, Titulo, Extensao, listas DataSets e Filtros.
- **CRelatoriosDataSet:** DataSet do relatório: Relatorio, DataSetName, MetodoName.
- **CRelatoriosFiltros:** Filtro do relatório: Relatorio, TituloCampo, NomeCampo, TipoCampo, Source, SourceField, Requerido.

Tipos de campo usados no PadraoController (constantes): Drop=1, Data=2, Valor=3, DropBox=5, CheckBox=6, Foto=7, Chave=9, DropBoxMulti=B, Time=C, File=D.

### 5.4 Validações (Domain.Attribute)

- **ValidarCpfCnpjAttribute:** valida CPF (11 dígitos) e CNPJ (14 dígitos).
- **LowerCaseAttribute / UpperCaseAttribute:** normalizam string.
- **MinimoItensListaAttribute:** exige mínimo de itens em lista.
- **RequiredTipo:** required condicional por tipo.
- **TotalPagamentosAttribute:** valida soma de lista vs valor.

---

## 6. Camada de persistência (Persistencia)

- **Mapeamentos:** Um arquivo por entidade (ex.: MUsuarios, MEmpresa, MPessoas, MEnquetes, MPessoasEnquetes, MResultadoEnquetes, MParametros, MMenus, MBancos, MErros, MUsuariosEmpresas, MUsuariosPermissoes, MGrupoUsuarios, MGrupoUsuariosPermissoes, MLogEnqueteLogin, etc.).
- **FluentSessionFactory:** único ponto de criação de `ISessionFactory` e `ISession`, com suporte a múltiplos bancos e criação de schema.

---

## 7. Camada de negócios (Negocios)

- **Padrão:** Uma classe N* por entidade (NUsuarios, NEmpresa, NPessoas, NEnquetes, NPessoasEnquetes, NResultadoEnquetes, NParametros, NMenus, NSubMenus, NCampos, NRelatorios, NBancos, NErros, NUsuariosEmpresas, NUsuariosPermissoes, NGrupoUsuarios, NGrupoUsuariosPermissoes, NLogEnqueteLogin, etc.).
- **NBase / NBase1:** provavelmente classes base genéricas para CRUD.
- **Email:** uso de MailKit/MimeKit para envio de e-mail (configuração via CParametros).
- **Global:** possíveis constantes ou helpers globais.
- Recebem `ISession` (geralmente por construtor) e realizam consultas e persistência via NHibernate.

---

## 8. Aplicação Web (projeto Web)

### 8.1 Inicialização

- **Global.asax:** Define `NinjectDependencyResolver` como `DependencyResolver`, registra `FiltroProviderCustom`, áreas, filtros globais, rotas e bundles.

### 8.2 Rotas

- Rota padrão: `{controller}/{action}/{id}` (default: Home, Index, id opcional).
- Ignora `{resource}.axd/{*pathInfo}`.

### 8.3 Controllers

- **HomeController:** Página inicial.
- **AccountController:** Login, seleção de empresa (SelecionaEmpresa).
- **PadraoController:** CRUD genérico dinâmico baseado em CTelas/CCampos: resolve entidade (Domain.C*), negócio (Negocios.N*), relatório (Reports.R*) por convenção de nome; gerencia listas, filtros, importação, relatórios e uso de sessão.
- **RelatoriosController:** Relatórios (ex.: Index).
- **ParametrosController:** Parâmetros do sistema.
- **CepController:** Provável integração com API de CEP.
- **eIntegradController:** Integração externa (eIntegrad).
- **ErrorController:** Tratamento de erro (ex.: Index).

### 8.4 Autenticação e autorização

- **ILoginProvider / CustonLoginProvider:** fornece usuário e empresa atuais (injetado pelo Ninject).
- **ValidacaoUsuarioAttribute:** validação de usuário/autorização em ações.
- Login com Identity/Owin; seleção de empresa após login (multi-empresa).

### 8.5 Infraestrutura Web

- **NinjectDependencyResolver:** bindings de ISession e ILoginProvider.
- **FiltroProviderCustom:** filtros customizados de ação.
- **ProgressHub:** SignalR hub para progresso (ex.: upload/processamento).

### 8.6 Views e front-end

- Layout: `_Layout.cshtml`, `_ViewStart.cshtml`.
- Áreas: Home (Index), Account (Login, SelecionaEmpresa), Padrao (partials: _Adicionar, _Cadastros, _Consultas, _Importar, _ListaItens, _Pesquisar, _Relatorios, _RelatoriosFiltros, _Reload, Relatorio), Relatorios (Index), Parametros (Index), eIntegrad (Index), Error (Index).
- Conteúdo: Bootstrap, CoreUI, jQuery, validação (jQuery Validate, unobtrusive), máscaras (jquery.mask), confirmação (jquery-confirm), SignalR (progresso).
- Temas: style.css, styleDark.css.
- Scripts específicos: funcoes.js, perfil.js; Firebase (init-firebase.js, firebase-messaging-sw.js) para notificações.

### 8.7 Modelos de apresentação (Web.Models)

- LoginModel, Cryptography, Retorno, Functions, DashBoard, UploadFilesResult, LanguageMang, ClasseBase, RelatorioModel, FiltroModel, Basico, ListaModel.

---

## 9. Módulo de enquetes (WebEnquete)

- Projeto web separado (mesmo stack: MVC, NHibernate, Ninject).
- Foco em votações para associados: login do associado (CPF/matrícula/data nascimento), exibição de enquetes ativas, registro de voto (CPessoasEnquetes) e log (CLogEnqueteLogin).
- Reutiliza Domain, Persistencia e Negocios (NEnquetes, NPessoasEnquetes, NResultadoEnquetes, NLogEnqueteLogin).

---

## 10. Módulo de relatórios (Reports)

- Classes R* (ex.: REnquetes, RRelatorios, RPessoas, RProvedor, RBaseReport).
- **RBaseReport:** base para relatórios.
- **CamposRelatorios:** definição de campos.
- Integração com ReportViewer e geração de PDF (iTextSharp, PDFsharp).
- Usado pelo PadraoController e RelatoriosController para exibir e exportar relatórios definidos em CRelatorios/CRelatoriosDataSet/CRelatoriosFiltros.

---

## 11. Funcionalidades de negócio identificadas

1. **Multi-empresa / multi-tenant:** conexão e empresa selecionada por cliente (connection string + seleção de empresa no login).
2. **Cadastro de associados (Pessoas):** dados cadastrais, bancários, profissionais e de contato; flags associado/ativo/aposentado; validação de CPF.
3. **Enquetes e votações:** criação de enquetes com período, pergunta e duas opções (SIM/NÃO ou custom); voto por pessoa; resultado agregado; log de acesso à enquete.
4. **Usuários e permissões:** usuário com tipo (Administrativo, Ecommerce, Provedor), vínculo a várias empresas, permissões por submenu/tela; grupos de usuários com permissões.
5. **Menus e telas dinâmicas:** menus, submenus, telas e campos configuráveis (CRUD genérico no PadraoController).
6. **Relatórios configuráveis:** relatórios vinculados a telas, com data sets e filtros definidos no modelo (CRelatorios*).
7. **Parâmetros de sistema:** configuração de SMTP e e-mail para ativações.
8. **Log de erros:** persistência de exceções (CErros) para diagnóstico.
9. **Integração:** CEP (CepController), eIntegrad (eIntegradController).
10. **UX:** progresso via SignalR; notificações (Firebase); temas claro/escuro.

---

## 12. Configuração e deploy

- **Connection strings:** em `Web.config` (e possivelmente em `app.config` dos outros projetos), com uma entrada por “cliente” (ex.: PCMAX, DATABASE).
- **Seção por cliente:** `FluentSessionFactory` faz `ConfigurationManager.RefreshSection(nCliente)` e usa `ConnectionStrings[nCliente]`.
- **SQL Server Types:** uso de tipos espaciais (SqlServerTypes, Loader.cs) em Domain/Web; DLLs x86/x64 incluídas no projeto.
- Dependências via NuGet (packages); solução compilável em VS 2019/2022.

---

## 13. Riscos e pontos de atenção para nova plataforma

- **Sessão NHibernate:** escopo transiente por requisição; na nova plataforma definir bem ciclo de vida da sessão/unit of work.
- **PadraoController:** CRUD genérico com reflexão (NewClass, NewNegocio, NewReport); muito acoplado ao modelo atual; sugerido quebrar em APIs/controllers mais explícitos.
- **Multi-tenant:** garantir isolamento por connection string e por empresa (filtrar sempre por empresa onde fizer sentido).
- **Menus/telas em banco:** modelo de metadados rico; na nova plataforma decidir se se mantém modelo dinâmico ou se parte para telas estáticas com permissões.
- **Relatórios:** dependência de ReportViewer e RDLC; avaliar migração para outra stack de relatórios (ex.: relatórios em serviço ou ferramentas modernas).
- **Legado .NET Framework:** nova plataforma provavelmente em .NET 6+; planejar migração de entidades e regras sem reutilizar NHibernate necessariamente (ex.: Entity Core ou Dapper).
- **Validações:** manter regras de CPF/CNPJ e outras validações customizadas na nova base.
- **Enquetes:** fluxo de autenticação do associado (CPF + matrícula + data nascimento) e regras de uma votação por pessoa por enquete devem ser preservados.

---

## 14. Resumo para especificação da nova plataforma

- **Entidades principais a migrar:** Empresa, Usuarios, UsuariosEmpresas, UsuariosPermissoes, GrupoUsuarios, GrupoUsuariosPermissoes, Pessoas, Bancos, Enquetes, PessoasEnquetes, ResultadoEnquetes, LogEnqueteLogin, Parametros, Erros.
- **Conceitos a preservar:** multi-empresa, perfil de usuário e permissões por tela/grupo, cadastro de associados com dados bancários e profissionais, enquetes com período e duas opções, relatórios com filtros e datasets, parâmetros de e-mail, log de erros.
- **Decisões de desenho:** modelo de menus/telas (dinâmico vs estático), stack de relatórios, estratégia de multi-tenant (um banco por cliente vs schema por cliente vs tenant id), e migração gradual ou big-bang.

---

*Documento gerado a partir da análise do código na pasta `base conhecimento legado` (solução Sintrafgv.sln).*
