-- ============================================================================
-- Script de Seed para SQL Server - EduOnline Platform
-- ============================================================================
-- Cada API possui seu próprio banco de dados (Database per Service), refletindo
-- o isolamento real usado pelas connection strings do docker-compose.yml.
-- Não são usados schemas para "separar" domínios dentro de um único banco.
-- Tabelas e colunas seguem exatamente o modelo gerado pelas migrations do EF Core
-- de cada serviço (ver src/*/Migrations/*_Initial.cs), para evitar divergência
-- entre o schema aplicado manualmente e o schema esperado pela aplicação.
-- Execução: Docker entrypoint para SQL Server
-- ============================================================================

-- ============================================================================
-- BANCO: EduOnlineAuthDb - Autenticação e Usuários (ASP.NET Core Identity)
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

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetRoles] (
		[Id] [nvarchar](450) NOT NULL PRIMARY KEY,
		[Name] [nvarchar](256) NULL,
		[NormalizedName] [nvarchar](256) NULL,
		[ConcurrencyStamp] [nvarchar](max) NULL
	);
	CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetUsers] (
		[Id] [nvarchar](450) NOT NULL PRIMARY KEY,
		[StatusId] [int] NOT NULL,
		[StatusNome] [nvarchar](max) NOT NULL,
		[UserName] [nvarchar](256) NULL,
		[NormalizedUserName] [nvarchar](256) NULL,
		[Email] [nvarchar](256) NULL,
		[NormalizedEmail] [nvarchar](256) NULL,
		[EmailConfirmed] [bit] NOT NULL,
		[PasswordHash] [nvarchar](max) NULL,
		[SecurityStamp] [nvarchar](max) NULL,
		[ConcurrencyStamp] [nvarchar](max) NULL,
		[PhoneNumber] [nvarchar](max) NULL,
		[PhoneNumberConfirmed] [bit] NOT NULL,
		[TwoFactorEnabled] [bit] NOT NULL,
		[LockoutEnd] [datetimeoffset] NULL,
		[LockoutEnabled] [bit] NOT NULL,
		[AccessFailedCount] [int] NOT NULL
	);
	CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers]([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
	CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers]([NormalizedEmail]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RefreshTokens] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[Username] [nvarchar](max) NOT NULL,
		[Token] [uniqueidentifier] NOT NULL,
		[ExpirationDate] [datetime2] NOT NULL
	);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetRoleClaims] (
		[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[RoleId] [nvarchar](450) NOT NULL,
		[ClaimType] [nvarchar](max) NULL,
		[ClaimValue] [nvarchar](max) NULL,
		FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetUserClaims] (
		[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[UserId] [nvarchar](450) NOT NULL,
		[ClaimType] [nvarchar](max) NULL,
		[ClaimValue] [nvarchar](max) NULL,
		FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetUserLogins] (
		[LoginProvider] [nvarchar](128) NOT NULL,
		[ProviderKey] [nvarchar](128) NOT NULL,
		[ProviderDisplayName] [nvarchar](max) NULL,
		[UserId] [nvarchar](450) NOT NULL,
		PRIMARY KEY ([LoginProvider], [ProviderKey]),
		FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetUserRoles] (
		[UserId] [nvarchar](450) NOT NULL,
		[RoleId] [nvarchar](450) NOT NULL,
		PRIMARY KEY ([UserId], [RoleId]),
		FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE,
		FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[AspNetUserTokens] (
		[UserId] [nvarchar](450) NOT NULL,
		[LoginProvider] [nvarchar](128) NOT NULL,
		[Name] [nvarchar](128) NOT NULL,
		[Value] [nvarchar](max) NULL,
		PRIMARY KEY ([UserId], [LoginProvider], [Name]),
		FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
	);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'ADMINISTRADOR')
BEGIN
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', NEWID());
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (NEWID(), 'Aluno', 'ALUNO', NEWID());
END;
GO

-- Observação: os Ids abaixo são fixos (em vez de NEWID()) para poderem ser reutilizados
-- como chave do aluno/admin nos demais bancos (EduOnlineAlunosDb) sem depender de SELECT
-- entre bancos no momento da criação. O PasswordHash foi gerado offline com
-- Microsoft.AspNetCore.Identity.PasswordHasher<T> (PBKDF2-HMACSHA256), senha "Teste@123",
-- exatamente o mesmo algoritmo usado em runtime pelo UserManager<T>.
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUsers] WHERE [Id] = '3AFD838B-B52B-42F0-90FA-BD94E7EDD19C')
BEGIN
	DECLARE @AdminId NVARCHAR(450) = '3AFD838B-B52B-42F0-90FA-BD94E7EDD19C';
	DECLARE @AdminPasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEMKNdS+y3bH6sMmwfRDWIY1lnRF9U0Js0mc3AJ9k+rPURoiVTc/BpoqrUV5+oNuWww==';

	INSERT INTO [dbo].[AspNetUsers] ([Id], [StatusId], [StatusNome], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount])
	VALUES (@AdminId, 2, 'Cadastrado', 'admin@eduonline.com', 'ADMIN@EDUONLINE.COM', 'admin@eduonline.com', 'ADMIN@EDUONLINE.COM', 1, @AdminPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), 0, 0, 1, 0);

	INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
	SELECT @AdminId, [Id] FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'ADMINISTRADOR';
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUsers] WHERE [Id] = 'D9475E09-793F-4BD8-8F63-93E5038C0D16')
BEGIN
	DECLARE @AlunoId NVARCHAR(450) = 'D9475E09-793F-4BD8-8F63-93E5038C0D16';
	DECLARE @AlunoPasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEIfqlKvZovd0L4bUAtA5BANJZjwmHirB4IQDz6HfGkyp59e62FVPb8t/ML6UxQBhjA==';

	INSERT INTO [dbo].[AspNetUsers] ([Id], [StatusId], [StatusNome], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount])
	VALUES (@AlunoId, 2, 'Cadastrado', 'aluno@eduonline.com', 'ALUNO@EDUONLINE.COM', 'aluno@eduonline.com', 'ALUNO@EDUONLINE.COM', 1, @AlunoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), 0, 0, 1, 0);

	INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
	SELECT @AlunoId, [Id] FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'ALUNO';
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
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[Nome] [varchar](100) NOT NULL,
		[Autor] [varchar](100) NOT NULL,
		[Validade] [date] NOT NULL,
		[Ativo] [bit] NOT NULL,
		[Valor] [decimal](18, 2) NOT NULL,
		[Tema] [varchar](2048) NOT NULL,
		[NivelId] [int] NOT NULL,
		[CargaHoraria] [int] NOT NULL,
		[DataCriacao] [datetime2] NOT NULL
	);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Aulas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Aulas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[CursoId] [uniqueidentifier] NOT NULL,
		[Titulo] [varchar](100) NOT NULL,
		[Descricao] [varchar](100) NULL,
		[LinkMaterial] [varchar](2048) NOT NULL,
		[DuracaoEmMinutos] [int] NOT NULL,
		[DataCriacao] [datetime2] NOT NULL,
		FOREIGN KEY ([CursoId]) REFERENCES [dbo].[Cursos]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_Aulas_CursoId] ON [dbo].[Aulas]([CursoId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Cursos])
BEGIN
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Autor], [Validade], [Ativo], [Valor], [Tema], [NivelId], [CargaHoraria], [DataCriacao]) VALUES (NEWID(), 'Introdução a C#', 'Prof. João Silva', '2030-12-31', 1, 149.99, 'Programação', 1, 40, GETUTCDATE());
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Autor], [Validade], [Ativo], [Valor], [Tema], [NivelId], [CargaHoraria], [DataCriacao]) VALUES (NEWID(), 'ASP.NET Core Avançado', 'Prof. Maria Santos', '2030-12-31', 1, 199.99, 'Programação', 3, 60, GETUTCDATE());
	INSERT INTO [dbo].[Cursos] ([Id], [Nome], [Autor], [Validade], [Ativo], [Valor], [Tema], [NivelId], [CargaHoraria], [DataCriacao]) VALUES (NEWID(), 'Banco de Dados SQL', 'Prof. Carlos Oliveira', '2030-12-31', 1, 179.99, 'Banco de Dados', 2, 50, GETUTCDATE());
END;
GO

-- ============================================================================
-- BANCO: EduOnlineAlunosDb - Alunos e Matrículas
-- ============================================================================
-- Observação: [Id] em Alunos referencia o Id do usuário em EduOnlineAuthDb, e
-- [CursoId] em Matriculas referencia EduOnlineConteudosDb. Como bancos separados
-- não suportam FOREIGN KEY entre si, a integridade referencial é responsabilidade
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

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alunos]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Alunos] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[Nome] [varchar](100) NOT NULL,
		[Email] [varchar](100) NOT NULL,
		[DataNascimento] [date] NULL,
		[Ativo] [bit] NOT NULL,
		[DataCriacao] [datetime2] NOT NULL
	);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Matriculas]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Matriculas] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[AlunoId] [uniqueidentifier] NOT NULL,
		[CursoId] [uniqueidentifier] NOT NULL,
		[CursoNome] [varchar](100) NOT NULL,
		[Validade] [date] NOT NULL,
		[StatusId] [int] NOT NULL,
		[PagamentoStatusId] [int] NOT NULL,
		[TotalAulas] [int] NULL,
		[AulasConcluidas] [nvarchar](max) NULL,
		[Ativo] [bit] NOT NULL,
		[DataCriacao] [datetime2] NOT NULL,
		FOREIGN KEY ([AlunoId]) REFERENCES [dbo].[Alunos]([Id]) ON DELETE CASCADE
	);
	CREATE INDEX [IX_Matriculas_AlunoId] ON [dbo].[Matriculas]([AlunoId]);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Certificados]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Certificados] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[MatriculaId] [uniqueidentifier] NOT NULL,
		[Link] [varchar](100) NOT NULL,
		[DataCriacao] [datetime2] NOT NULL,
		FOREIGN KEY ([MatriculaId]) REFERENCES [dbo].[Matriculas]([Id]) ON DELETE CASCADE
	);
	CREATE UNIQUE INDEX [IX_Certificados_MatriculaId] ON [dbo].[Certificados]([MatriculaId]);
END;
GO

-- Seed: perfil e matrícula do aluno de teste (mesmo Id usado em AspNetUsers, em
-- EduOnlineAuthDb, para o usuário aluno@eduonline.com).
IF NOT EXISTS (SELECT 1 FROM [dbo].[Alunos] WHERE [Id] = 'D9475E09-793F-4BD8-8F63-93E5038C0D16')
BEGIN
	DECLARE @AlunoId UNIQUEIDENTIFIER = 'D9475E09-793F-4BD8-8F63-93E5038C0D16';
	DECLARE @MatriculaCursoId UNIQUEIDENTIFIER;
	DECLARE @MatriculaCursoNome VARCHAR(100);

	INSERT INTO [dbo].[Alunos] ([Id], [Nome], [Email], [DataNascimento], [Ativo], [DataCriacao])
	VALUES (@AlunoId, 'João da Silva', 'aluno@eduonline.com', '2000-05-15', 1, GETUTCDATE());

	SELECT TOP 1 @MatriculaCursoId = [Id], @MatriculaCursoNome = [Nome] FROM [EduOnlineConteudosDb].[dbo].[Cursos];

	IF @MatriculaCursoId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Matriculas] ([Id], [AlunoId], [CursoId], [CursoNome], [Validade], [StatusId], [PagamentoStatusId], [TotalAulas], [Ativo], [DataCriacao])
		VALUES (NEWID(), @AlunoId, @MatriculaCursoId, @MatriculaCursoNome, '2030-12-31', 1, 2, NULL, 1, GETUTCDATE());
	END;
END;
GO

-- ============================================================================
-- BANCO: EduOnlinePagamentosDb - Transações e Histórico
-- ============================================================================
-- Observação: [AlunoId] e [CursoId] referenciam EduOnlineAlunosDb e
-- EduOnlineConteudosDb; sem FK entre bancos pelo mesmo motivo descrito acima.
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
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[AlunoId] [uniqueidentifier] NOT NULL,
		[CursoId] [uniqueidentifier] NOT NULL,
		[Total] [decimal](18, 2) NOT NULL,
		[NomeCartao] [varchar](100) NOT NULL,
		[NumeroCartao] [varchar](100) NOT NULL,
		[ExpiracaoCartao] [varchar](100) NOT NULL,
		[CvvCartao] [varchar](100) NOT NULL,
		[DataCriacao] [datetime2] NOT NULL
	);
END;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transacoes]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Transacoes] (
		[Id] [uniqueidentifier] NOT NULL PRIMARY KEY,
		[PagamentoId] [uniqueidentifier] NOT NULL,
		[Total] [decimal](18, 2) NOT NULL,
		[StatusTransacaoId] [int] NULL,
		[DataCriacao] [datetime2] NOT NULL,
		FOREIGN KEY ([PagamentoId]) REFERENCES [dbo].[Pagamentos]([Id]) ON DELETE CASCADE
	);
	CREATE UNIQUE INDEX [IX_Transacoes_PagamentoId] ON [dbo].[Transacoes]([PagamentoId]);
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
