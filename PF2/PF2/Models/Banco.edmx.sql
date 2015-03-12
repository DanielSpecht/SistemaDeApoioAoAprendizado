
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/30/2014 18:28:03
-- Generated from EDMX file: C:\Users\Daniel\Documents\Visual Studio 2013\Projects\PF2 - Copy\PF2\Models\Banco.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DefaultConnection];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_dbo_Node_dbo_Node_NodeID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Node] DROP CONSTRAINT [FK_dbo_Node_dbo_Node_NodeID];
GO
IF OBJECT_ID(N'[dbo].[FK_NodeQuestion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Question] DROP CONSTRAINT [FK_NodeQuestion];
GO
IF OBJECT_ID(N'[dbo].[FK_QuestionAnswer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AnswerOption] DROP CONSTRAINT [FK_QuestionAnswer];
GO
IF OBJECT_ID(N'[dbo].[FK_AspNetUsersQuiz]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Quiz] DROP CONSTRAINT [FK_AspNetUsersQuiz];
GO
IF OBJECT_ID(N'[dbo].[FK_QuizNode]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Quiz] DROP CONSTRAINT [FK_QuizNode];
GO
IF OBJECT_ID(N'[dbo].[FK_QuizQuizAnswerQuestion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuizAnswerQuestion] DROP CONSTRAINT [FK_QuizQuizAnswerQuestion];
GO
IF OBJECT_ID(N'[dbo].[FK_QuizAnswerQuestionAnswerOption]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuizAnswerQuestion] DROP CONSTRAINT [FK_QuizAnswerQuestionAnswerOption];
GO
IF OBJECT_ID(N'[dbo].[FK_QuizAnswerQuestionQuestion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[QuizAnswerQuestion] DROP CONSTRAINT [FK_QuizAnswerQuestionQuestion];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[AspNetUsers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AspNetUsers];
GO
IF OBJECT_ID(N'[dbo].[Node]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Node];
GO
IF OBJECT_ID(N'[dbo].[Question]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Question];
GO
IF OBJECT_ID(N'[dbo].[AnswerOption]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AnswerOption];
GO
IF OBJECT_ID(N'[dbo].[Quiz]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Quiz];
GO
IF OBJECT_ID(N'[dbo].[QuizAnswerQuestion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[QuizAnswerQuestion];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AspNetUsers'
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] nvarchar(128)  NOT NULL,
    [Country] nvarchar(max)  NULL,
    [Email] nvarchar(256)  NULL,
    [EmailConfirmed] bit  NOT NULL,
    [PasswordHash] nvarchar(max)  NULL,
    [SecurityStamp] nvarchar(max)  NULL,
    [PhoneNumber] nvarchar(max)  NULL,
    [PhoneNumberConfirmed] bit  NOT NULL,
    [TwoFactorEnabled] bit  NOT NULL,
    [LockoutEndDateUtc] datetime  NULL,
    [LockoutEnabled] bit  NOT NULL,
    [AccessFailedCount] int  NOT NULL,
    [UserName] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'Node'
CREATE TABLE [dbo].[Node] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(50)  NULL,
    [Description] nvarchar(50)  NULL,
    [ParentId] int  NULL
);
GO

-- Creating table 'Question'
CREATE TABLE [dbo].[Question] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [NodeId] int  NOT NULL,
    [Enunciation] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'AnswerOption'
CREATE TABLE [dbo].[AnswerOption] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [QuestionId] int  NOT NULL,
    [Answer] nvarchar(max)  NOT NULL,
    [IsCorrect] bit  NOT NULL
);
GO

-- Creating table 'Quiz'
CREATE TABLE [dbo].[Quiz] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AspNetUsersId] nvarchar(128)  NOT NULL,
    [NodeId] int  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NULL,
    [NumQuestions] int  NOT NULL
);
GO

-- Creating table 'QuizAnswerQuestion'
CREATE TABLE [dbo].[QuizAnswerQuestion] (
    [QuizId] int  NOT NULL,
    [AnswerOptionId] int  NULL,
    [QuestionId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'AspNetUsers'
ALTER TABLE [dbo].[AspNetUsers]
ADD CONSTRAINT [PK_AspNetUsers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Node'
ALTER TABLE [dbo].[Node]
ADD CONSTRAINT [PK_Node]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Question'
ALTER TABLE [dbo].[Question]
ADD CONSTRAINT [PK_Question]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AnswerOption'
ALTER TABLE [dbo].[AnswerOption]
ADD CONSTRAINT [PK_AnswerOption]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Quiz'
ALTER TABLE [dbo].[Quiz]
ADD CONSTRAINT [PK_Quiz]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [QuizId], [QuestionId] in table 'QuizAnswerQuestion'
ALTER TABLE [dbo].[QuizAnswerQuestion]
ADD CONSTRAINT [PK_QuizAnswerQuestion]
    PRIMARY KEY CLUSTERED ([QuizId], [QuestionId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ParentId] in table 'Node'
ALTER TABLE [dbo].[Node]
ADD CONSTRAINT [FK_dbo_Node_dbo_Node_NodeID]
    FOREIGN KEY ([ParentId])
    REFERENCES [dbo].[Node]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dbo_Node_dbo_Node_NodeID'
CREATE INDEX [IX_FK_dbo_Node_dbo_Node_NodeID]
ON [dbo].[Node]
    ([ParentId]);
GO

-- Creating foreign key on [NodeId] in table 'Question'
ALTER TABLE [dbo].[Question]
ADD CONSTRAINT [FK_NodeQuestion]
    FOREIGN KEY ([NodeId])
    REFERENCES [dbo].[Node]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_NodeQuestion'
CREATE INDEX [IX_FK_NodeQuestion]
ON [dbo].[Question]
    ([NodeId]);
GO

-- Creating foreign key on [QuestionId] in table 'AnswerOption'
ALTER TABLE [dbo].[AnswerOption]
ADD CONSTRAINT [FK_QuestionAnswer]
    FOREIGN KEY ([QuestionId])
    REFERENCES [dbo].[Question]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_QuestionAnswer'
CREATE INDEX [IX_FK_QuestionAnswer]
ON [dbo].[AnswerOption]
    ([QuestionId]);
GO

-- Creating foreign key on [AspNetUsersId] in table 'Quiz'
ALTER TABLE [dbo].[Quiz]
ADD CONSTRAINT [FK_AspNetUsersQuiz]
    FOREIGN KEY ([AspNetUsersId])
    REFERENCES [dbo].[AspNetUsers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AspNetUsersQuiz'
CREATE INDEX [IX_FK_AspNetUsersQuiz]
ON [dbo].[Quiz]
    ([AspNetUsersId]);
GO

-- Creating foreign key on [NodeId] in table 'Quiz'
ALTER TABLE [dbo].[Quiz]
ADD CONSTRAINT [FK_QuizNode]
    FOREIGN KEY ([NodeId])
    REFERENCES [dbo].[Node]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_QuizNode'
CREATE INDEX [IX_FK_QuizNode]
ON [dbo].[Quiz]
    ([NodeId]);
GO

-- Creating foreign key on [QuizId] in table 'QuizAnswerQuestion'
ALTER TABLE [dbo].[QuizAnswerQuestion]
ADD CONSTRAINT [FK_QuizQuizAnswerQuestion]
    FOREIGN KEY ([QuizId])
    REFERENCES [dbo].[Quiz]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [AnswerOptionId] in table 'QuizAnswerQuestion'
ALTER TABLE [dbo].[QuizAnswerQuestion]
ADD CONSTRAINT [FK_QuizAnswerQuestionAnswerOption]
    FOREIGN KEY ([AnswerOptionId])
    REFERENCES [dbo].[AnswerOption]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_QuizAnswerQuestionAnswerOption'
CREATE INDEX [IX_FK_QuizAnswerQuestionAnswerOption]
ON [dbo].[QuizAnswerQuestion]
    ([AnswerOptionId]);
GO

-- Creating foreign key on [QuestionId] in table 'QuizAnswerQuestion'
ALTER TABLE [dbo].[QuizAnswerQuestion]
ADD CONSTRAINT [FK_QuizAnswerQuestionQuestion]
    FOREIGN KEY ([QuestionId])
    REFERENCES [dbo].[Question]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_QuizAnswerQuestionQuestion'
CREATE INDEX [IX_FK_QuizAnswerQuestionQuestion]
ON [dbo].[QuizAnswerQuestion]
    ([QuestionId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------