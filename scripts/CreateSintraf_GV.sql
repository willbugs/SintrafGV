-- =============================================================================
-- Script de criação do banco Sintraf_GV - Nova plataforma SintrafGV
-- =============================================================================
-- Banco LEGADO (produção): Sintrafgv
-- Banco NOVO (plataforma): Sintraf_GV
-- =============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Sintraf_GV')
BEGIN
    CREATE DATABASE [Sintraf_GV];
END
GO

USE [Sintraf_GV];
GO

-- =============================================================================
-- Tabela: Associados
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Associados')
BEGIN
    CREATE TABLE [Associados] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(200) NOT NULL,
        [Cpf] nvarchar(14) NOT NULL,
        [MatriculaSindicato] nvarchar(50) NULL,
        [MatriculaBancaria] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [Celular] nvarchar(20) NULL,
        [Ativo] bit NOT NULL,
        [DataUltimaAtualizacao] datetime2 NULL,
        [CriadoEm] datetime2 NOT NULL,
        [Agencia] nvarchar(max) NULL,
        [Aposentado] bit NOT NULL DEFAULT 0,
        [Banco] nvarchar(max) NULL,
        [Cep] nvarchar(max) NULL,
        [Cidade] nvarchar(max) NULL,
        [CodAgencia] nvarchar(max) NULL,
        [Conta] nvarchar(max) NULL,
        [Ctps] nvarchar(max) NULL,
        [DataAdmissao] datetime2 NULL,
        [DataDesligamento] datetime2 NULL,
        [DataFiliacao] datetime2 NULL,
        [DataNascimento] datetime2 NULL,
        [Endereco] nvarchar(max) NULL,
        [Estado] nvarchar(max) NULL,
        [EstadoCivil] nvarchar(max) NULL,
        [Funcao] nvarchar(max) NULL,
        [Naturalidade] nvarchar(max) NULL,
        [Serie] nvarchar(max) NULL,
        [Sexo] nvarchar(max) NULL,
        [Telefone] nvarchar(max) NULL,
        [Bairro] nvarchar(max) NULL,
        [Complemento] nvarchar(max) NULL,
        [Carteirinha] nvarchar(max) NULL,
        [Base] nvarchar(max) NULL,
        [Motivo] nvarchar(max) NULL,
        CONSTRAINT [PK_Associados] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Associados_Cpf] ON [Associados] ([Cpf]);
END
GO

-- =============================================================================
-- Tabela: Usuarios
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [SenhaHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [CriadoEm] datetime2 NOT NULL,
        [Ativo] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END
GO

-- =============================================================================
-- Tabela: Eleicoes
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Eleicoes')
BEGIN
    CREATE TABLE [Eleicoes] (
        [Id] uniqueidentifier NOT NULL,
        [Titulo] nvarchar(300) NOT NULL,
        [Descricao] nvarchar(2000) NULL,
        [ArquivoAnexo] nvarchar(500) NULL,
        [InicioVotacao] datetime2 NOT NULL,
        [FimVotacao] datetime2 NOT NULL,
        [Tipo] int NOT NULL,
        [Status] int NOT NULL,
        [ApenasAssociados] bit NOT NULL,
        [ApenasAtivos] bit NOT NULL,
        [BancoId] uniqueidentifier NULL,
        [CriadoEm] datetime2 NOT NULL,
        [CriadoPorId] uniqueidentifier NULL,
        [AtualizadoEm] datetime2 NULL,
        CONSTRAINT [PK_Eleicoes] PRIMARY KEY ([Id])
    );
END
GO

-- =============================================================================
-- Tabela: Perguntas
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Perguntas')
BEGIN
    CREATE TABLE [Perguntas] (
        [Id] uniqueidentifier NOT NULL,
        [EleicaoId] uniqueidentifier NOT NULL,
        [Ordem] int NOT NULL,
        [Texto] nvarchar(500) NOT NULL,
        [Descricao] nvarchar(1000) NULL,
        [Tipo] int NOT NULL,
        [MaxVotos] int NULL,
        [PermiteBranco] bit NOT NULL,
        CONSTRAINT [PK_Perguntas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Perguntas_Eleicoes_EleicaoId] FOREIGN KEY ([EleicaoId]) REFERENCES [Eleicoes] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Perguntas_EleicaoId] ON [Perguntas] ([EleicaoId]);
END
GO

-- =============================================================================
-- Tabela: Opcoes
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Opcoes')
BEGIN
    CREATE TABLE [Opcoes] (
        [Id] uniqueidentifier NOT NULL,
        [PerguntaId] uniqueidentifier NOT NULL,
        [Ordem] int NOT NULL,
        [Texto] nvarchar(300) NOT NULL,
        [Descricao] nvarchar(1000) NULL,
        [Foto] nvarchar(500) NULL,
        [AssociadoId] uniqueidentifier NULL,
        CONSTRAINT [PK_Opcoes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Opcoes_Perguntas_PerguntaId] FOREIGN KEY ([PerguntaId]) REFERENCES [Perguntas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Opcoes_Associados_AssociadoId] FOREIGN KEY ([AssociadoId]) REFERENCES [Associados] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Opcoes_PerguntaId] ON [Opcoes] ([PerguntaId]);
    CREATE INDEX [IX_Opcoes_AssociadoId] ON [Opcoes] ([AssociadoId]);
END
GO

-- =============================================================================
-- Tabela: Votos
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Votos')
BEGIN
    CREATE TABLE [Votos] (
        [Id] uniqueidentifier NOT NULL,
        [EleicaoId] uniqueidentifier NOT NULL,
        [AssociadoId] uniqueidentifier NOT NULL,
        [DataHoraVoto] datetime2 NOT NULL,
        [IpOrigem] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [CodigoComprovante] nvarchar(100) NULL,
        [AssinaturaDigital] nvarchar(500) NULL,
        [ChaveCriptografia] nvarchar(100) NOT NULL,
        [DadosDispositivo] nvarchar(200) NULL,
        [DataValidacaoCartorio] datetime2 NULL,
        [HashAnterior] nvarchar(100) NULL,
        [HashVoto] nvarchar(100) NOT NULL,
        [Latitude] nvarchar(50) NULL,
        [Longitude] nvarchar(50) NULL,
        [NumeroProtocoloCartorio] nvarchar(100) NULL,
        [NumeroSequencial] nvarchar(50) NULL,
        [ObservacoesValidacao] nvarchar(500) NULL,
        [RespostaCriptografada] nvarchar(200) NOT NULL,
        [TimestampPreciso] nvarchar(50) NOT NULL,
        [ValidadoCartorio] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Votos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Votos_Associados_AssociadoId] FOREIGN KEY ([AssociadoId]) REFERENCES [Associados] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Votos_Eleicoes_EleicaoId] FOREIGN KEY ([EleicaoId]) REFERENCES [Eleicoes] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_Votos_AssociadoId] ON [Votos] ([AssociadoId]);
    CREATE UNIQUE INDEX [IX_Votos_EleicaoId_AssociadoId] ON [Votos] ([EleicaoId], [AssociadoId]);
    CREATE INDEX [IX_Votos_DataHoraVoto] ON [Votos] ([DataHoraVoto]);
    CREATE UNIQUE INDEX [IX_Votos_HashVoto] ON [Votos] ([HashVoto]);
END
GO

-- =============================================================================
-- Tabela: VotosDetalhes
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VotosDetalhes')
BEGIN
    CREATE TABLE [VotosDetalhes] (
        [Id] uniqueidentifier NOT NULL,
        [PerguntaId] uniqueidentifier NOT NULL,
        [OpcaoId] uniqueidentifier NULL,
        [DataHora] datetime2 NOT NULL,
        [VotoBranco] bit NOT NULL,
        CONSTRAINT [PK_VotosDetalhes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VotosDetalhes_Perguntas_PerguntaId] FOREIGN KEY ([PerguntaId]) REFERENCES [Perguntas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VotosDetalhes_Opcoes_OpcaoId] FOREIGN KEY ([OpcaoId]) REFERENCES [Opcoes] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_VotosDetalhes_PerguntaId] ON [VotosDetalhes] ([PerguntaId]);
    CREATE INDEX [IX_VotosDetalhes_OpcaoId] ON [VotosDetalhes] ([OpcaoId]);
END
GO

-- =============================================================================
-- Tabela: ConfiguracoesSindicato
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConfiguracoesSindicato')
BEGIN
    CREATE TABLE [ConfiguracoesSindicato] (
        [Id] uniqueidentifier NOT NULL,
        [RazaoSocial] nvarchar(200) NOT NULL,
        [NomeFantasia] nvarchar(200) NOT NULL,
        [CNPJ] nvarchar(18) NOT NULL,
        [InscricaoEstadual] nvarchar(20) NULL,
        [Endereco] nvarchar(300) NOT NULL,
        [Numero] nvarchar(10) NOT NULL,
        [Complemento] nvarchar(100) NULL,
        [Bairro] nvarchar(100) NOT NULL,
        [Cidade] nvarchar(100) NOT NULL,
        [UF] nvarchar(2) NOT NULL,
        [CEP] nvarchar(9) NOT NULL,
        [Telefone] nvarchar(20) NULL,
        [Celular] nvarchar(20) NULL,
        [Email] nvarchar(200) NULL,
        [Website] nvarchar(200) NULL,
        [Presidente] nvarchar(200) NOT NULL,
        [CPFPresidente] nvarchar(14) NOT NULL,
        [Secretario] nvarchar(200) NULL,
        [CPFSecretario] nvarchar(14) NULL,
        [TextoAutenticacao] nvarchar(500) NULL,
        [CartorioResponsavel] nvarchar(200) NULL,
        [EnderecoCartorio] nvarchar(300) NULL,
        [CriadoEm] datetime2 NOT NULL,
        [AtualizadoEm] datetime2 NULL,
        [CriadoPor] nvarchar(100) NULL,
        [AtualizadoPor] nvarchar(100) NULL,
        CONSTRAINT [PK_ConfiguracoesSindicato] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_ConfiguracoesSindicato_CNPJ] ON [ConfiguracoesSindicato] ([CNPJ]);
END
GO

-- =============================================================================
-- Tabela __EFMigrationsHistory (para EF Core)
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );

    -- Registrar todas as migrations já aplicadas
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
    (N'20260223172153_InitialCreate', N'8.0.11'),
    (N'20260223175911_AddUsuario', N'8.0.11'),
    (N'20260224120000_AddUsuarioAtivo', N'8.0.11'),
    (N'20260224131002_AddAssociadoCamposLegado', N'8.0.11'),
    (N'20260224133748_AddAssociadoBairro', N'8.0.11'),
    (N'20260224134040_AddAssociadoComplemento', N'8.0.11'),
    (N'20260224134418_SimplificaStatusAssociado', N'8.0.11'),
    (N'20260224140636_CreateSistemaVotacoes', N'8.0.11'),
    (N'20260225135411_AddConfiguracaoSindicato', N'8.0.11'),
    (N'20260302110132_AddAssociadoCamposLegadoCarteirinhaBaseMotivo', N'8.0.11');
END
GO

PRINT 'Banco Sintraf_GV criado com sucesso.';
GO
