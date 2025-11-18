-- ===========================================
-- Script de Inicialização Completa do Banco de Dados
-- Database: dr_ocupacional
-- ===========================================

-- Criar banco de dados se não existir
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'dr_ocupacional')
BEGIN
    CREATE DATABASE [dr_ocupacional];
    PRINT 'Database dr_ocupacional criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Database dr_ocupacional já existe.';
END
GO

-- Usar o banco de dados
USE dr_ocupacional;
GO

-- ===========================================
-- Tabela: tb_menu
-- Descrição: Armazena os itens do menu do sistema
-- ===========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tb_menu]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tb_menu] (
        [cod_menu] INT IDENTITY(1,1) NOT NULL,
        [nome] NVARCHAR(255) NOT NULL,
        [ordem] INT NOT NULL DEFAULT 0,
        [icon] NVARCHAR(100) NULL,
        CONSTRAINT [PK_tb_menu] PRIMARY KEY CLUSTERED ([cod_menu] ASC)
    );
    
    PRINT 'Tabela tb_menu criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela tb_menu já existe.';
END
GO

-- ===========================================
-- Índices para melhorar performance
-- ===========================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tb_menu_nome' AND object_id = OBJECT_ID('dbo.tb_menu'))
BEGIN
    CREATE INDEX [IX_tb_menu_nome] ON [dbo].[tb_menu] ([nome]);
    PRINT 'Índice IX_tb_menu_nome criado com sucesso!';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tb_menu_ordem' AND object_id = OBJECT_ID('dbo.tb_menu'))
BEGIN
    CREATE INDEX [IX_tb_menu_ordem] ON [dbo].[tb_menu] ([ordem]);
    PRINT 'Índice IX_tb_menu_ordem criado com sucesso!';
END
GO

PRINT 'Script de inicialização executado com sucesso!';
GO

