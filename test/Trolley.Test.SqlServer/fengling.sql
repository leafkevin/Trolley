USE [fengling]
GO
DROP TABLE IF EXISTS [dbo].[sys_brand];
CREATE TABLE [dbo].[sys_brand](
	[Id] [int] NOT NULL,
	[BrandNo] [nvarchar](50) NULL DEFAULT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[CompanyId] [int] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_brand] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_company];
CREATE TABLE [dbo].[sys_company](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[Nature] [nvarchar](50) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_company] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_menu];
CREATE TABLE [dbo].[sys_menu](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[ParentId] [int] NULL DEFAULT NULL,
	[PageId] [int] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_menu] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order];
CREATE TABLE [dbo].[sys_order](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL DEFAULT NULL,	
	[ProductCount] [int] NULL DEFAULT NULL,
	[TotalAmount] [float] NULL DEFAULT NULL,
	[BuyerId] [int] NULL DEFAULT NULL,
	[BuyerSource] [nvarchar](50) NULL DEFAULT NULL,
	[SellerId] [int] NULL DEFAULT NULL,
	[Products] [ntext] NULL DEFAULT NULL,
	[Disputes] [ntext] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_order] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order_104_202405];
CREATE TABLE [dbo].[sys_order_104_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL DEFAULT NULL,	
	[ProductCount] [int] NULL DEFAULT NULL,
	[TotalAmount] [float] NULL DEFAULT NULL,
	[BuyerId] [int] NULL DEFAULT NULL,
	[BuyerSource] [nvarchar](50) NULL DEFAULT NULL,
	[SellerId] [int] NULL DEFAULT NULL,
	[Products] [ntext] NULL DEFAULT NULL,
	[Disputes] [ntext] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_order_104_202405] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order_105_202405];
CREATE TABLE [dbo].[sys_order_105_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderNo] [nvarchar](50) NULL DEFAULT NULL,	
	[ProductCount] [int] NULL DEFAULT NULL,
	[TotalAmount] [float] NULL DEFAULT NULL,
	[BuyerId] [int] NULL DEFAULT NULL,
	[BuyerSource] [nvarchar](50) NULL DEFAULT NULL,
	[SellerId] [int] NULL DEFAULT NULL,
	[Products] [ntext] NULL DEFAULT NULL,
	[Disputes] [ntext] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_order_105_202405] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order_detail];
CREATE TABLE [dbo].[sys_order_detail](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL DEFAULT NULL,
	[ProductId] [int] NULL DEFAULT NULL,
	[Price] [float] NULL DEFAULT NULL,
	[Quantity] [int] NULL DEFAULT NULL,
	[Amount] [float] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_order_detail] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order_detail_104_202405];
CREATE TABLE [dbo].[sys_order_detail_104_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL DEFAULT NULL,
	[ProductId] [int] NULL DEFAULT NULL,
	[Price] [float] NULL DEFAULT NULL,
	[Quantity] [int] NULL DEFAULT NULL,
	[Amount] [float] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 CONSTRAINT [pk_sys_order_detail_104_202405] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_order_detail_105_202405];
CREATE TABLE [dbo].[sys_order_detail_105_202405](
	[Id] [nvarchar](50) NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[OrderId] [nvarchar](50) NULL DEFAULT NULL,
	[ProductId] [int] NULL DEFAULT NULL,
	[Price] [float] NULL DEFAULT NULL,
	[Quantity] [int] NULL DEFAULT NULL,
	[Amount] [float] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_order_detail_105_202405] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_page];
CREATE TABLE [dbo].[sys_page](
	[Id] [int] NOT NULL,
	[Url] [nvarchar](200) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_page] PRIMARY KEY CLUSTERED([Id] ASC));
GO
DROP TABLE IF EXISTS [dbo].[sys_product];
CREATE TABLE [dbo].[sys_product](
	[Id] [int] NOT NULL,
	[ProductNo] [nvarchar](50) NULL DEFAULT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[BrandId] [int] NULL DEFAULT NULL,
	[CategoryId] [int] NULL DEFAULT NULL,
	[Price] [float] NULL DEFAULT NULL,
	[CompanyId] [int] NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_product] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_user];
CREATE TABLE [dbo].[sys_user](
	[Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[Gender] [nvarchar](50) NULL DEFAULT NULL,
	[Age] [int] NULL DEFAULT NULL,
	[CompanyId] [int] NULL DEFAULT NULL, 
	[GuidField] [uniqueidentifier] NULL DEFAULT NULL,
	[SomeTimes] [time](7) NULL DEFAULT NULL,
	[SourceType] [nvarchar](50) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_user] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_user_104];
CREATE TABLE [sys_user_104]  (
  	[Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[Gender] [nvarchar](50) NULL DEFAULT NULL,
	[Age] [int] NULL DEFAULT NULL,
	[CompanyId] [int] NULL DEFAULT NULL,
	[GuidField] [uniqueidentifier] NULL DEFAULT NULL,
	[SomeTimes] [time](7) NULL DEFAULT NULL,
	[SourceType] [nvarchar](50) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_user_104] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_user_105];
CREATE TABLE [sys_user_105]  (
  	[Id] [int] NOT NULL,
	[TenantId] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL DEFAULT NULL,
	[Gender] [nvarchar](50) NULL DEFAULT NULL,
	[Age] [int] NULL DEFAULT NULL,
	[CompanyId] [int] NULL DEFAULT NULL,
	[GuidField] [uniqueidentifier] NULL DEFAULT NULL,
	[SomeTimes] [time](7) NULL DEFAULT NULL,
	[SourceType] [nvarchar](50) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_user_105] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_function];
CREATE TABLE [dbo].[sys_function](
	[MenuId] [int] NOT NULL,
	[PageId] [int] NOT NULL,
	[FunctionName] [nvarchar](50) NULL DEFAULT NULL,
	[Description] [nvarchar](500) NULL DEFAULT NULL,
	[IsEnabled] [bit] NULL DEFAULT NULL,
	[CreatedAt] [datetime] NULL DEFAULT NULL,
	[CreatedBy] [int] NULL DEFAULT NULL,
	[UpdatedAt] [datetime] NULL DEFAULT NULL,
	[UpdatedBy] [int] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_function] PRIMARY KEY CLUSTERED([MenuId] ASC,[PageId] ASC)
);
GO
DROP TABLE IF EXISTS [dbo].[sys_update_entity];
CREATE TABLE [dbo].[sys_update_entity](
	[Id] [int] NOT NULL,
	[BooleanField] [bit] NULL DEFAULT NULL,
	[EnumField] [tinyint] NULL DEFAULT NULL,
	[GuidField] [nvarchar](50) NULL DEFAULT NULL,
	[DateTimeField] [datetime] NULL DEFAULT NULL,
	[DateOnlyField] [date] NULL DEFAULT NULL,
	[DateTimeOffsetField] [datetimeoffset] NULL DEFAULT NULL,
	[TimeSpanField] [time] NULL DEFAULT NULL,
	[TimeOnlyField] [time] NULL DEFAULT NULL,
 	CONSTRAINT [pk_sys_update_entity] PRIMARY KEY CLUSTERED([Id] ASC)
);
GO