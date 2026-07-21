-- ============================================================================
-- Script de Seed para SQL Server - EduOnline Platform
-- ============================================================================
-- Cada API possui seu próprio banco de dados (Database per Service), refletindo
-- o isolamento real usado pelas connection strings do docker-compose.yml.
-- Não são usados schemas para "separar" domínios dentro de um único banco.
-- Execução: Docker entrypoint para SQL Server
-- ============================================================================

-- ============================================================================
-- BANCO: EduOnlineAuthDb - Autenticação e Usuários
-- ============================================================================
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EduOnlineAuthDb')
BEGIN
	CREATE DATABASE [EduOnlineAuthDb];
	ALTER DATABASE [EduOnlineAuthDb] SET RECOVERY SIMPLE;
END;
GO

USE [EduOnlineAuthDb];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Users] (
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
	CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
	CREATE NONCLUSTERED INDEX [IX_Users_NormalizedUserName] ON [dbo].[Users]([NormalizedUserName]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Roles] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[Name] [nvarchar](256) NOT NULL UNIQUE,
		[NormalizedName] [nvarchar](256) NOT NULL UNIQUE,
		[ConcurrencyStamp] [nvarchar](max) NULL,
		[Description] [nvarchar](500) NULL,
		[CreatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE()
	);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[UserRoles] (
		[UserId] [uniqueidentifier] NOT NULL,
		[RoleId] [uniqueidentifier] NOT NULL,
		PRIMARY KEY ([UserId], [RoleId]),
		FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
	);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Name] = 'Administrador')
BEGIN
	INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [Description]) VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', 'Administrador do sistema');
	INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [Description]) VALUES (NEWID(), 'Aluno', 'ALUNO', 'Estudante da plataforma');
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@eduonline.com')
BEGIN
	DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
	DECLARE @AdminRoleId UNIQUEIDENTIFIER;

	INSERT INTO [dbo].[Users] ([Id], [Email], [NormalizedEmail], [UserName], [NormalizedUserName], [FullName])
	VALUES (@AdminId, 'admin@eduonline.com', 'ADMIN@EDUONLINE.COM', 'admin', 'ADMIN', 'Administrador do Sistema');

	SELECT @AdminRoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrador';
	INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (@AdminId, @AdminRoleId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'aluno@eduonline.com')
BEGIN
	DECLARE @AlunoUserId UNIQUEIDENTIFIER = NEWID();
	DECLARE @AlunoRoleId UNIQUEIDENTIFIER;

	INSERT INTO [dbo].[Users] ([Id], [Email], [NormalizedEmail], [UserName], [NormalizedUserName], [FullName])
	VALUES (@AlunoUserId, 'aluno@eduonline.com', 'ALUNO@EDUONLINE.COM', 'aluno', 'ALUNO', 'João da Silva');

	SELECT @AlunoRoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Aluno';
	INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (@AlunoUserId, @AlunoRoleId);
END;
GO

-- ============================================================================
-- BANCO: EduOnlineConteudosDb - Cursos e Aulas
-- ============================================================================
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EduOnlineConteudosDb')
BEGIN
	CREATE DATABASE [EduOnlineConteudosDb];
	ALTER DATABASE [EduOnlineConteudosDb] SET RECOVERY SIMPLE;
END;
GO

USE [EduOnlineConteudosDb];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cursos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Cursos] (
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
	CREATE NONCLUSTERED INDEX [IX_Cursos_Nome] ON [dbo].[Cursos]([Nome]);
	CREATE NONCLUSTERED INDEX [IX_Cursos_Ativo] ON [dbo].[Cursos]([Ativo]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Aulas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Aulas] (
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
		FOREIGN KEY ([CursoId]) REFERENCES [dbo].[Cursos]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Aulas_CursoId] ON [dbo].[Aulas]([CursoId]);
	CREATE NONCLUSTERED INDEX [IX_Aulas_Ordem] ON [dbo].[Aulas]([CursoId], [Ordem]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Cursos])
BEGIN
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'Introdução a C#', 'Aprenda os fundamentos da linguagem C# e programação orientada a objetos', 'Prof. João Silva', 40, 149.99);
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'ASP.NET Core Avançado', 'Desenvolvimento de aplicações web profissionais com ASP.NET Core', 'Prof. Maria Santos', 60, 199.99);
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Descricao], [Instrutor], [CargaHoraria], [Preco]) VALUES (NEWID(), 'Banco de Dados SQL', 'Modelagem e manipulação de dados com SQL Server', 'Prof. Carlos Oliveira', 50, 179.99);
END;
GO

-- ============================================================================
-- BANCO: EduOnlineAlunosDb - Alunos e Matrículas
-- ============================================================================
-- Observação: [UserId] e [CursoId] referenciam registros em EduOnlineAuthDb e
-- EduOnlineConteudosDb, respectivamente. Como bancos separados não suportam
-- FOREIGN KEY entre si, a integridade referencial passa a ser responsabilidade
-- da aplicação (nada diferente do que já acontece entre microsserviços).
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EduOnlineAlunosDb')
BEGIN
	CREATE DATABASE [EduOnlineAlunosDb];
	ALTER DATABASE [EduOnlineAlunosDb] SET RECOVERY SIMPLE;
END;
GO

USE [EduOnlineAlunosDb];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AlunosPerfil]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AlunosPerfil] (
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
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE()
	);
	CREATE NONCLUSTERED INDEX [IX_AlunosPerfil_UserId] ON [dbo].[AlunosPerfil]([UserId]);
	CREATE NONCLUSTERED INDEX [IX_AlunosPerfil_Matricula] ON [dbo].[AlunosPerfil]([Matricula]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Matriculas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Matriculas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[AlunoId] [uniqueidentifier] NOT NULL,
		[CursoId] [uniqueidentifier] NOT NULL,
		[DataMatricula] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[DataConclusao] [datetime2] NULL,
		[Status] [nvarchar](50) NOT NULL DEFAULT 'Ativa',
		[ProgressoPercentual] [decimal](5, 2) NOT NULL DEFAULT 0,
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([AlunoId]) REFERENCES [dbo].[AlunosPerfil]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_AlunoId] ON [dbo].[Matriculas]([AlunoId]);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_CursoId] ON [dbo].[Matriculas]([CursoId]);
	CREATE NONCLUSTERED INDEX [IX_Matriculas_Status] ON [dbo].[Matriculas]([Status]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProgressoAulas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProgressoAulas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[MatriculaId] [uniqueidentifier] NOT NULL,
		[AulaId] [uniqueidentifier] NOT NULL,
		[DataConclusao] [datetime2] NULL,
		[TempoAssistidoSegundos] [int] NOT NULL DEFAULT 0,
		[Concluida] [bit] NOT NULL DEFAULT 0,
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([MatriculaId]) REFERENCES [dbo].[Matriculas]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_ProgressoAulas_MatriculaId] ON [dbo].[ProgressoAulas]([MatriculaId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Certificados]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Certificados] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[MatriculaId] [uniqueidentifier] NOT NULL UNIQUE,
		[NumeroCertificado] [nvarchar](100) NOT NULL UNIQUE,
		[DataEmissao] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		[DataValidade] [datetime2] NULL,
		[Ativo] [bit] NOT NULL DEFAULT 1,
		FOREIGN KEY ([MatriculaId]) REFERENCES [dbo].[Matriculas]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_Certificados_MatriculaId] ON [dbo].[Certificados]([MatriculaId]);
END;
GO

-- Seed: perfil e matrícula do aluno de teste, lendo Id's de EduOnlineAuthDb e EduOnlineConteudosDb
-- (consulta entre bancos no mesmo servidor via nome de três partes, sem FK entre eles)
IF NOT EXISTS (SELECT 1 FROM [dbo].[AlunosPerfil] WHERE [Matricula] = 'ALUNO001')
BEGIN
	DECLARE @AlunoUserId UNIQUEIDENTIFIER;
	DECLARE @AlunoPerfilId UNIQUEIDENTIFIER = NEWID();
	DECLARE @CursoId UNIQUEIDENTIFIER;

	SELECT @AlunoUserId = [Id] FROM [EduOnlineAuthDb].[dbo].[Users] WHERE [Email] = 'aluno@eduonline.com';
	SELECT TOP 1 @CursoId = [Id] FROM [EduOnlineConteudosDb].[dbo].[Cursos];

	IF @AlunoUserId IS NOT NULL AND @CursoId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[AlunosPerfil] ([Id], [UserId], [Matricula], [DataNascimento], [Cpf], [Telefone])
		VALUES (@AlunoPerfilId, @AlunoUserId, 'ALUNO001', DATEFROMPARTS(2000, 5, 15), '123.456.789-00', '(11) 98765-4321');

		INSERT INTO [dbo].[Matriculas] ([AlunoId], [CursoId], [Status])
		VALUES (@AlunoPerfilId, @CursoId, 'Ativa');
	END;
END;
GO

-- ============================================================================
-- BANCO: EduOnlinePagamentosDb - Transações e Histórico
-- ============================================================================
-- Observação: [MatriculaId] e [AlunoId] referenciam EduOnlineAlunosDb; sem FK
-- entre bancos pelo mesmo motivo descrito acima.
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EduOnlinePagamentosDb')
BEGIN
	CREATE DATABASE [EduOnlinePagamentosDb];
	ALTER DATABASE [EduOnlinePagamentosDb] SET RECOVERY SIMPLE;
END;
GO

USE [EduOnlinePagamentosDb];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pagamentos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Pagamentos] (
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
		[UpdatedAt] [datetime2] NOT NULL DEFAULT GETUTCDATE()
	);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_AlunoId] ON [dbo].[Pagamentos]([AlunoId]);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_Status] ON [dbo].[Pagamentos]([Status]);
	CREATE NONCLUSTERED INDEX [IX_Pagamentos_DataVencimento] ON [dbo].[Pagamentos]([DataVencimento]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoTransacoes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[HistoricoTransacoes] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY DEFAULT NEWID(),
		[PagamentoId] [uniqueidentifier] NOT NULL,
		[StatusAnterior] [nvarchar](50) NOT NULL,
		[StatusNovo] [nvarchar](50) NOT NULL,
		[Descricao] [nvarchar](500) NULL,
		[DataTransacao] [datetime2] NOT NULL DEFAULT GETUTCDATE(),
		FOREIGN KEY ([PagamentoId]) REFERENCES [dbo].[Pagamentos]([Id]) ON DELETE CASCADE
	);
	CREATE NONCLUSTERED INDEX [IX_HistoricoTransacoes_PagamentoId] ON [dbo].[HistoricoTransacoes]([PagamentoId]);
END;
GO

-- ============================================================================
-- EXIBIR RESULTADO
-- ============================================================================
USE [master];
GO
PRINT '========================================';
PRINT 'Script de Seed executado com sucesso!';
PRINT '========================================';
PRINT '';
PRINT 'Bancos criados:';
SELECT name FROM sys.databases WHERE name IN ('EduOnlineAuthDb', 'EduOnlineConteudosDb', 'EduOnlineAlunosDb', 'EduOnlinePagamentosDb');
GO
