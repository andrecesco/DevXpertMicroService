-- ============================================================================
-- Script de Seed para SQL Server - EduOnline Platform
-- ============================================================================
-- Este script cria a estrutura inicial de schemas e dados para desenvolvimento/teste
-- Execução: Docker entrypoint para SQL Server

USE [master];
GO

-- Verificar se o banco de dados existe, caso contrário criar
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EduOnlineDB')
BEGIN
	CREATE DATABASE [EduOnlineDB];
	ALTER DATABASE [EduOnlineDB] SET RECOVERY SIMPLE;
END;
GO

USE [EduOnlineDB];
GO

-- ============================================================================
-- SCHEMA: AUTH - Autenticação e Usuários
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Auth')
	EXEC sp_executesql N'CREATE SCHEMA Auth';
GO

-- Tabela de Usuários
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auth].[Users]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Auth].[Users] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[Email] [nvarchar](256) NOT NULL UNIQUE,
		[NormalizedEmail] [nvarchar](256) NOT NULL UNIQUE,
		[UserName] [nvarchar](256) NOT NULL UNIQUE,
		[NormalizedUserName] [nvarchar](256) NOT NULL UNIQUE,
		[FullName] [nvarchar](256) NOT NULL,
		[PasswordHash] [nvarchar](max) NULL,
		[SecurityStamp] [nvarchar](max) NULL,
		[ConcurrencyStamp] [nvarchar](max) NULL,
		[PhoneNumber] [nvarchar](20) NULL,
		[PhoneNumberConfirmed] [bit] NOT NULL DEFAULT 0,
		[EmailConfirmed] [bit] NOT NULL DEFAULT 0,
		[TwoFactorEnabled] [bit] NOT NULL DEFAULT 0,
		[LockoutEnabled] [bit] NOT NULL DEFAULT 1,
		[LockoutEnd] [datetimeoffset] NULL,
		[AccessFailedCount] [int] NOT NULL DEFAULT 0,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[IsActive] [bit] NOT NULL DEFAULT 1
	);
	CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [Auth].[Users]([Email]);
	CREATE NONCLUSTERED INDEX [IX_Users_NormalizedUserName] ON [Auth].[Users]([NormalizedUserName]);
END;
GO

-- Tabela de Roles (Papéis)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auth].[Roles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Auth].[Roles] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[Name] [nvarchar](256) NOT NULL UNIQUE,
		[NormalizedName] [nvarchar](256) NOT NULL UNIQUE,
		[ConcurrencyStamp] [nvarchar](max) NULL,
		[Description] [nvarchar](500) NULL,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE()
	);
END;
GO

-- Tabela de User Roles (Mapeamento User-Role)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Auth].[UserRoles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Auth].[UserRoles] (
		[UserId] [uniqueidentifier] NOT NULL,
		[RoleId] [uniqueidentifier] NOT NULL,
		PRIMARY KEY ([UserId], [RoleId]),
		FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([RoleId]) REFERENCES [Auth].[Roles]([Id]) ON DELETE CASCADE
	);
END;
GO

-- Dados iniciais: Roles
IF NOT EXISTS (SELECT 1 FROM [Auth].[Roles] WHERE [Name] = 'Administrador')
BEGIN
	INSERT INTO [Auth].[Roles] ([Id], [Name], [NormalizedName], [Description]) VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', 'Administrador do sistema');
	INSERT INTO [Auth].[Roles] ([Id], [Name], [NormalizedName], [Description]) VALUES (NEWID(), 'Aluno', 'ALUNO', 'Estudante da plataforma');
END;
GO

-- ============================================================================
-- SCHEMA: CONTEUDOS - Cursos e Aulas
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Conteudos')
	EXEC sp_executesql N'CREATE SCHEMA Conteudos';
GO

-- Tabela de Cursos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Conteudos].[Cursos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Conteudos].[Cursos] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[Nome] [nvarchar](256) NOT NULL,
		[Descricao] [nvarchar](max) NOT NULL,
		[Instrutor] [nvarchar](256) NOT NULL,
		[CargaHoraria] [int] NOT NULL,
		[Preco] [decimal](10, 2) NOT NULL,
		[Ativo] [bit] NOT NULL DEFAULT 1,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE()
	);
	CREATE NONCLUSTERED INDEX [IX_Cursos_Nome] ON [Conteudos].[Cursos]([Nome]);
	CREATE NONCLUSTERED INDEX [IX_Cursos_Ativo] ON [Conteudos].[Cursos]([Ativo]);
END;
GO

-- Tabela de Aulas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Conteudos].[Aulas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Conteudos].[Aulas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[CursoId] [uniqueidentifier] NOT NULL,
		[Titulo] [nvarchar](256) NOT NULL,
		[Descricao] [nvarchar](max) NOT NULL,
		[UrlVideo] [nvarchar](max) NULL,
		[Ordem] [int] NOT NULL,
		[DuracaoMinutos] [int] NOT NULL,
		[Ativo] [bit] NOT NULL DEFAULT 1,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([CursoId]) REFERENCES [Conteudos].[Cursos]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Aulas_CursoId] ON [Conteudos].[Aulas]([CursoId]);
	CREATE NONCLUSTERED INDEX [IX_Aulas_Ordem] ON [Conteudos].[Aulas]([CursoId], [Ordem]);
END;
GO

-- Dados iniciais: Cursos
IF NOT EXISTS (SELECT 1 FROM [Conteudos].[Cursos])
BEGIN
	INSERT INTO [Conteudos].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'Introdução a C#', 'Aprenda os fundamentos da linguagem C# e programação orientada a objetos', 'Prof. João Silva', 40, 149.99);
	INSERT INTO [Conteudos].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'ASP.NET Core Avançado', 'Desenvolvimento de aplicações web profissionais com ASP.NET Core', 'Prof. Maria Santos', 60, 199.99);
	INSERT INTO [Conteudos].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'Banco de Dados SQL', 'Modelagem e manipulação de dados com SQL Server', 'Prof. Carlos Oliveira', 50, 179.99);
END;
GO

-- ============================================================================
-- SCHEMA: ALUNOS - Alunos e Matrículas
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Alunos')
	EXEC sp_executesql N'CREATE SCHEMA Alunos';
GO

-- Tabela de Alunos (estendida do Auth.Users)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Alunos].[AlunosPerfil]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Alunos].[AlunosPerfil] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[UserId] [uniqueidentifier] NOT NULL UNIQUE,
		[Matricula] [nvarchar](50) NOT NULL UNIQUE,
		[DataNascimento] [date] NULL,
		[Cpf] [nvarchar](14) NULL UNIQUE,
		[Telefone] [nvarchar](20) NULL,
		[Endereco] [nvarchar](500) NULL,
		[Cidade] [nvarchar](100) NULL,
		[Estado] [nvarchar](2) NULL,
		[Cep] [nvarchar](10) NULL,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_AlunosPerfil_UserId] ON [Alunos].[AlunosPerfil]([UserId]);
	CREATE NONCLUSTERED INDEX [IX_AlunosPerfil_Matricula] ON [Alunos].[AlunosPerfil]([Matricula]);
END;
GO

-- Tabela de Matrículas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Alunos].[Matriculas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Alunos].[Matriculas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[AlunoId] [uniqueidentifier] NOT NULL,
		[CursoId] [uniqueidentifier] NOT NULL,
		[DataMatricula] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[DataConclusao] [datetime2] NULL,
		[Status] [nvarchar](50) NOT NULL DEFAULT 'Ativa',
		[ProgressoPercentual] [decimal](5, 2) NOT NULL DEFAULT 0,
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([AlunoId]) REFERENCES [Alunos].[AlunosPerfil]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([CursoId]) REFERENCES [Conteudos].[Cursos]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_AlunoId] ON [Alunos].[Matriculas]([AlunoId]);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_CursoId] ON [Alunos].[Matriculas]([CursoId]);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_Status] ON [Alunos].[Matriculas]([Status]);
END;
GO

-- Tabela de Progresso de Aulas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Alunos].[ProgressoAulas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Alunos].[ProgressoAulas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[MatriculaId] [uniqueidentifier] NOT NULL,
		[AulaId] [uniqueidentifier] NOT NULL,
		[DataConclusao] [datetime2] NULL,
		[TempoAssistidoSegundos] [int] NOT NULL DEFAULT 0,
		[Concluida] [bit] NOT NULL DEFAULT 0,
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([MatriculaId]) REFERENCES [Alunos].[Matriculas]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([AulaId]) REFERENCES [Conteudos].[Aulas]([Id]) ON DELETE NO ACTION
	);
	CREATE NONCLUSTERED INDEX [IX_ProgressoAulas_MatriculaId] ON [Alunos].[ProgressoAulas]([MatriculaId]);
END;
GO

-- Tabela de Certificados
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Alunos].[Certificados]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Alunos].[Certificados] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[MatriculaId] [uniqueidentifier] NOT NULL UNIQUE,
		[NumeroCertificado] [nvarchar](100) NOT NULL UNIQUE,
		[DataEmissao] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[DataValidade] [datetime2] NULL,
		[Ativo] [bit] NOT NULL DEFAULT 1,
		FOREIGN KEY ([MatriculaId]) REFERENCES [Alunos].[Matriculas]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Certificados_MatriculaId] ON [Alunos].[Certificados]([MatriculaId]);
END;
GO

-- ============================================================================
-- SCHEMA: PAGAMENTOS - Transações e Histórico
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Pagamentos')
	EXEC sp_executesql N'CREATE SCHEMA Pagamentos';
GO

-- Tabela de Pagamentos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Pagamentos].[Pagamentos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Pagamentos].[Pagamentos] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[MatriculaId] [uniqueidentifier] NOT NULL,
		[AlunoId] [uniqueidentifier] NOT NULL,
		[Valor] [decimal](10, 2) NOT NULL,
		[Status] [nvarchar](50) NOT NULL DEFAULT 'Pendente',
		[MetodoPagamento] [nvarchar](50) NULL,
		[ReferenciaPagador] [nvarchar](100) NULL,
		[DataPagamento] [datetime2] NULL,
		[DataVencimento] [datetime2] NOT NULL,
		[DataCriacao] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([MatriculaId]) REFERENCES [Alunos].[Matriculas]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([AlunoId]) REFERENCES [Alunos].[AlunosPerfil]([Id]) ON DELETE NO ACTION
	);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_AlunoId] ON [Pagamentos].[Pagamentos]([AlunoId]);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_Status] ON [Pagamentos].[Pagamentos]([Status]);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_DataVencimento] ON [Pagamentos].[Pagamentos]([DataVencimento]);
END;
GO

-- Tabela de Histórico de Transações
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Pagamentos].[HistoricoTransacoes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [Pagamentos].[HistoricoTransacoes] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[PagamentoId] [uniqueidentifier] NOT NULL,
		[StatusAnterior] [nvarchar](50) NOT NULL,
		[StatusNovo] [nvarchar](50) NOT NULL,
		[Descricao] [nvarchar](500) NULL,
		[DataTransacao] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([PagamentoId]) REFERENCES [Pagamentos].[Pagamentos]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_HistoricoTransacoes_PagamentoId] ON [Pagamentos].[HistoricoTransacoes]([PagamentoId]);
END;
GO

-- ============================================================================
-- INSERIR DADOS INICIAIS
-- ============================================================================

-- Inserir Admin User
IF NOT EXISTS (SELECT 1 FROM [Auth].[Users] WHERE [Email] = 'admin@eduonline.com')
BEGIN
	DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
	DECLARE @AdminRoleId UNIQUEIDENTIFIER;

	INSERT INTO [Auth].[Users] ([Id], [Email], [NormalizedEmail], [UserName], [NormalizedUserName], [FullName])
	VALUES (@AdminId, 'admin@eduonline.com', 'ADMIN@EDUONLINE.COM', 'admin', 'ADMIN', 'Administrador do Sistema');

	SELECT @AdminRoleId = [Id] FROM [Auth].[Roles] WHERE [Name] = 'Administrador';
	INSERT INTO [Auth].[UserRoles] ([UserId], [RoleId]) VALUES (@AdminId, @AdminRoleId);
END;
GO

-- Inserir Aluno Teste
IF NOT EXISTS (SELECT 1 FROM [Auth].[Users] WHERE [Email] = 'aluno@eduonline.com')
BEGIN
	DECLARE @AlunoUserId UNIQUEIDENTIFIER = NEWID();
	DECLARE @AlunoId UNIQUEIDENTIFIER = NEWID();
	DECLARE @AlunoRoleId UNIQUEIDENTIFIER;
	DECLARE @CursoId UNIQUEIDENTIFIER;

	-- Criar usuário
	INSERT INTO [Auth].[Users] ([Id], [Email], [NormalizedEmail], [UserName], [NormalizedUserName], [FullName])
	VALUES (@AlunoUserId, 'aluno@eduonline.com', 'ALUNO@EDUONLINE.COM', 'aluno', 'ALUNO', 'João da Silva');

	-- Atribuir role Aluno
	SELECT @AlunoRoleId = [Id] FROM [Auth].[Roles] WHERE [Name] = 'Aluno';
	INSERT INTO [Auth].[UserRoles] ([UserId], [RoleId]) VALUES (@AlunoUserId, @AlunoRoleId);

	-- Criar perfil do aluno
	INSERT INTO [Alunos].[AlunosPerfil] ([Id], [UserId], [Matricula], [DataNascimento], [Cpf], [Telefone])
	VALUES (@AlunoId, @AlunoUserId, 'ALUNO001', DATEFROMPARTS(2000, 5, 15), '123.456.789-00', '(11) 98765-4321');

	-- Matricular no primeiro curso
	SELECT TOP 1 @CursoId = [Id] FROM [Conteudos].[Cursos];
	INSERT INTO [Alunos].[Matriculas] ([AlunoId], [CursoId], [Status])
	VALUES (@AlunoId, @CursoId, 'Ativa');
END;
GO

-- ============================================================================
-- EXIBIR RESULTADO
-- ============================================================================
PRINT '========================================';
PRINT 'Script de Seed executado com sucesso!';
PRINT '========================================';
PRINT '';
PRINT 'Schemas criados:';
SELECT name FROM sys.schemas WHERE name IN ('Auth', 'Conteudos', 'Alunos', 'Pagamentos');
PRINT '';
PRINT 'Tabelas criadas:';
SELECT COUNT(*) as TotalTabelas FROM sys.objects WHERE type = 'U';
PRINT '';
PRINT 'Dados iniciais inseridos:';
PRINT 'Roles: ' + CONVERT(VARCHAR, (SELECT COUNT(*) FROM [Auth].[Roles]));
PRINT 'Usuários: ' + CONVERT(VARCHAR, (SELECT COUNT(*) FROM [Auth].[Users]));
PRINT 'Cursos: ' + CONVERT(VARCHAR, (SELECT COUNT(*) FROM [Conteudos].[Cursos]));
GO
