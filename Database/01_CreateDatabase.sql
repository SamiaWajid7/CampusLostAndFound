-- Campus Lost and Found Portal - Database Creation Script
-- SQL Server 2019+

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CampusLostAndFound')
BEGIN
    CREATE DATABASE CampusLostAndFound;
END
GO

USE CampusLostAndFound;
GO

-- =============================================
-- Table: Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IconClass] NVARCHAR(50) NULL, -- For UI icon display
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- =============================================
-- Table: Locations (Campus Buildings/Areas)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Locations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Locations] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Building] NVARCHAR(100) NULL,
        [Floor] NVARCHAR(50) NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- =============================================
-- Table: AspNetUsers (Extended with custom fields)
-- This integrates with ASP.NET Core Identity
-- =============================================
-- Note: ASP.NET Core Identity will create AspNetUsers table
-- We add custom columns via ApplicationUser model

-- =============================================
-- Table: Items (Lost and Found Items)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Items]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Items] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL,
        [CategoryId] INT NOT NULL,
        [LocationId] INT NOT NULL,
        [LocationDetails] NVARCHAR(500) NULL, -- Specific details like "near the water fountain"
        [ItemType] INT NOT NULL, -- 0 = Lost, 1 = Found
        [Status] INT NOT NULL DEFAULT 0, -- 0 = Open, 1 = Claimed, 2 = Returned, 3 = Expired, 4 = Cancelled
        [DateOccurred] DATE NOT NULL, -- Date item was lost/found
        [ReportedByUserId] NVARCHAR(450) NOT NULL,
        [ClaimedByUserId] NVARCHAR(450) NULL,
        [VerifiedByUserId] NVARCHAR(450) NULL,
        [VerificationNotes] NVARCHAR(1000) NULL,
        [DateClaimed] DATETIME2 NULL,
        [DateVerified] DATETIME2 NULL,
        [DateReturned] DATETIME2 NULL,
        [IsVerified] BIT NOT NULL DEFAULT 0,
        [ViewCount] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_Items_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([Id]),
        CONSTRAINT [FK_Items_Locations] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations]([Id])
    );
END
GO

-- =============================================
-- Table: ItemImages
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItemImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ItemImages] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ItemId] INT NOT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [IsPrimary] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_ItemImages_Items] FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items]([Id]) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- Table: Claims (Claim requests for items)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Claims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Claims] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ItemId] INT NOT NULL,
        [ClaimantUserId] NVARCHAR(450) NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL, -- Why they believe it's theirs
        [ProofDescription] NVARCHAR(2000) NULL, -- Identifying details only owner would know
        [Status] INT NOT NULL DEFAULT 0, -- 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Cancelled
        [AdminNotes] NVARCHAR(1000) NULL,
        [ReviewedByUserId] NVARCHAR(450) NULL,
        [DateReviewed] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_Claims_Items] FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Items]([Id]) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- Table: Notifications
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Message] NVARCHAR(1000) NOT NULL,
        [Type] INT NOT NULL, -- 0 = Info, 1 = Success, 2 = Warning, 3 = ClaimUpdate, 4 = ItemMatch
        [RelatedItemId] INT NULL,
        [RelatedClaimId] INT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [ReadAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_Notifications_Items] FOREIGN KEY ([RelatedItemId]) REFERENCES [dbo].[Items]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Notifications_Claims] FOREIGN KEY ([RelatedClaimId]) REFERENCES [dbo].[Claims]([Id]) ON DELETE NO ACTION
    );
END
GO

-- =============================================
-- Table: AuditLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] NVARCHAR(450) NULL,
        [Action] NVARCHAR(100) NOT NULL,
        [EntityType] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(50) NULL,
        [OldValues] NVARCHAR(MAX) NULL,
        [NewValues] NVARCHAR(MAX) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- =============================================
-- Indexes for Performance
-- =============================================
CREATE NONCLUSTERED INDEX [IX_Items_CategoryId] ON [dbo].[Items]([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_Items_LocationId] ON [dbo].[Items]([LocationId]);
CREATE NONCLUSTERED INDEX [IX_Items_Status] ON [dbo].[Items]([Status]);
CREATE NONCLUSTERED INDEX [IX_Items_ItemType] ON [dbo].[Items]([ItemType]);
CREATE NONCLUSTERED INDEX [IX_Items_ReportedByUserId] ON [dbo].[Items]([ReportedByUserId]);
CREATE NONCLUSTERED INDEX [IX_Items_DateOccurred] ON [dbo].[Items]([DateOccurred] DESC);
CREATE NONCLUSTERED INDEX [IX_Claims_ItemId] ON [dbo].[Claims]([ItemId]);
CREATE NONCLUSTERED INDEX [IX_Claims_ClaimantUserId] ON [dbo].[Claims]([ClaimantUserId]);
CREATE NONCLUSTERED INDEX [IX_Claims_Status] ON [dbo].[Claims]([Status]);
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId] ON [dbo].[Notifications]([UserId]);
CREATE NONCLUSTERED INDEX [IX_Notifications_IsRead] ON [dbo].[Notifications]([IsRead]);
CREATE NONCLUSTERED INDEX [IX_ItemImages_ItemId] ON [dbo].[ItemImages]([ItemId]);
GO

PRINT 'Database schema created successfully!';
GO
