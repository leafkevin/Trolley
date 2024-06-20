USE [fengling]
GO

CREATE TABLE [dbo].[sys_brand](
	[Id] [int] NOT NULL,
	[BrandNo] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[CompanyId] [int] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_brand] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
INSERT INTO [dbo].[sys_brand] ([Id], [BrandNo], [Name], [CompanyId], [IsEnabled], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (1, 'BN-001', '波司登', 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO [dbo].[sys_brand] ([Id], [BrandNo], [Name], [CompanyId], [IsEnabled], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (2, 'BN-002', '雪中飞', 2, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO [dbo].[sys_brand] ([Id], [BrandNo], [Name], [CompanyId], [IsEnabled], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (3, 'BN-003', '优衣库', 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
CREATE TABLE [dbo].[sys_company](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Nature] [nvarchar](50) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_company] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

INSERT INTO [dbo].[sys_company] ([Id], [Name], [Nature], [IsEnabled], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (1, '微软', 'Internet', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO [dbo].[sys_company] ([Id], [Name], [Nature], [IsEnabled], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (2, '谷歌', 'Internet', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

CREATE TABLE [dbo].[sys_menu](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ParentId] [int] NULL,
	[PageId] [int] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_menu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
CREATE TABLE [dbo].[sys_order](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL,	
	[ProductCount] [int] NULL,
	[TotalAmount] [float] NULL,
	[BuyerId] [int] NULL,
	[BuyerSource] [nvarchar](50) NULL,
	[SellerId] [int] NULL,
	[Products] [ntext] NULL,
	[Disputes] [ntext] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [dbo].[sys_order_104_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL,	
	[ProductCount] [int] NULL,
	[TotalAmount] [float] NULL,
	[BuyerId] [int] NULL,
	[BuyerSource] [nvarchar](50) NULL,
	[SellerId] [int] NULL,
	[Products] [ntext] NULL,
	[Disputes] [ntext] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order_104_202405] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [dbo].[sys_order_105_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL,	
	[ProductCount] [int] NULL,
	[TotalAmount] [float] NULL,
	[BuyerId] [int] NULL,
	[BuyerSource] [nvarchar](50) NULL,
	[SellerId] [int] NULL,
	[Products] [ntext] NULL,
	[Disputes] [ntext] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order_105_202405] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
CREATE TABLE [dbo].[sys_order_detail](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL,
	[ProductId] [int] NULL,
	[Price] [float] NULL,
	[Quantity] [int] NULL,
	[Amount] [float] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order_detail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [dbo].[sys_order_detail_104_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL,
	[ProductId] [int] NULL,
	[Price] [float] NULL,
	[Quantity] [int] NULL,
	[Amount] [float] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order_detail_104_202405] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
CREATE TABLE [dbo].[sys_order_detail_105_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL,
	[ProductId] [int] NULL,
	[Price] [float] NULL,
	[Quantity] [int] NULL,
	[Amount] [float] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_order_detail_105_202405] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [dbo].[sys_page](
	[Id] [int] NOT NULL,
	[Url] [nvarchar](200) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_page] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
CREATE TABLE [dbo].[sys_product](
	[Id] [int] NOT NULL,
	[ProductNo] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[BrandId] [int] NULL,
	[CategoryId] [int] NULL,
	[Price] [float] NULL,
	[CompanyId] [int] NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_product] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO
CREATE TABLE [dbo].[sys_user](
	[Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Gender] [nvarchar](50) NULL,
	[Age] [int] NULL,
	[CompanyId] [int] NULL, 
	[GuidField] [uniqueidentifier] NULL,
	[SomeTimes] [time](7) NULL,
	[SourceType] [nvarchar](50) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_user] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [sys_user_104]  (
  [Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Gender] [nvarchar](50) NULL,
	[Age] [int] NULL,
	[CompanyId] [int] NULL,
	[GuidField] [uniqueidentifier] NULL,
	[SomeTimes] [time](7) NULL,
	[SourceType] [nvarchar](50) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_user_104] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO

CREATE TABLE [sys_user_105]  (
  [Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Gender] [nvarchar](50) NULL,
	[Age] [int] NULL,
	[CompanyId] [int] NULL,
	[GuidField] [uniqueidentifier] NULL,
	[SomeTimes] [time](7) NULL,
	[SourceType] [nvarchar](50) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_user_105] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO


CREATE TABLE [dbo].[sys_function](
	[MenuId] [int] NOT NULL,
	[PageId] [int] NOT NULL,
	[FunctionName] [nvarchar](50) NULL,
	[Description] [nvarchar](500) NULL,
	[IsEnabled] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
 CONSTRAINT [pk_sys_function] PRIMARY KEY CLUSTERED 
(
	[MenuId] ASC,
	[PageId] ASC
)
);
GO

CREATE TABLE [dbo].[sys_update_entity](
	[Id] [int] NOT NULL,
	[BooleanField] [bit] NULL,
	[EnumField] [nvarchar](50) NULL,
	[GuidField] [uniqueidentifier] NULL,
	[DateTimeField] [datetime] NULL,
	[DateOnlyField] [date] NULL,
	[DateTimeOffsetField] [datetimeoffset] NULL,
	[TimeSpanField] [time] NULL,
	[TimeOnlyField] [time] NULL,
 CONSTRAINT [pk_sys_update_entity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
);
GO