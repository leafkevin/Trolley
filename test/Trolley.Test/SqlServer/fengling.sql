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
	[Id] [int] NOT NULL,
	[OrderNo] [nvarchar](50) NULL,
	[ProductCount] [int] NULL,
	[TotalAmount] [float] NULL,
	[BuyerId] [int] NULL,
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
CREATE TABLE [dbo].[sys_order_detail](
	[Id] [int] NOT NULL,
	[OrderId] [int] NULL,
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
	[Name] [nvarchar](50) NULL,
	[Gender] [tinyint] NULL,
	[Age] [int] NULL,
	[CompanyId] [int] NULL,
	[GuidField] [uniqueidentifier] NULL,
	[SomeTimes] [time](7) NULL,
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
