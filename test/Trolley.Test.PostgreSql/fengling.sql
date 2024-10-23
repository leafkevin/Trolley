/*
 Navicat Premium Data Transfer

 Source Server         : postgresql
 Source Server Type    : PostgreSQL
 Source Server Version : 160003 (160003)
 Source Host           : localhost:5432
 Source Catalog        : fengling
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 160003 (160003)
 File Encoding         : 65001

 Date: 13/07/2024 14:22:50
*/


-- ----------------------------
-- Table structure for sys_brand
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_brand";
CREATE TABLE "public"."sys_brand" (
  "Id" int4 NOT NULL,
  "BrandNo" varchar(50) NULL DEFAULT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,  
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_brand" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_company
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_company" CASCADE;
DROP SEQUENCE IF EXISTS "public"."sys_company_Id_seq" CASCADE;
CREATE TABLE "public"."sys_company" (
  "Id" SERIAL NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Nature" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_company" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_menu";
CREATE TABLE "public"."sys_menu" (
  "Id" int4 NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "ParentId" int4 NULL DEFAULT NULL,
  "PageId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_menu" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order";
CREATE TABLE "public"."sys_order" (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_104_202405";
CREATE TABLE "public"."sys_order_104_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_104_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_105_202405";
CREATE TABLE "public"."sys_order_105_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_105_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_detail";
CREATE TABLE "public"."sys_order_detail" (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail_104_202405
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_detail_104_202405";
CREATE TABLE "public"."sys_order_detail_104_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail_104_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail_105_202405
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_detail_105_202405";
CREATE TABLE "public"."sys_order_detail_105_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail_105_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_page
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_page";
CREATE TABLE "public"."sys_page" (
  "Id" int4 NOT NULL,
  "Url" varchar(200) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_page" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_product
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_product";
CREATE TABLE "public"."sys_product" (
  "Id" int4 NOT NULL,
  "ProductNo" varchar(50) NULL DEFAULT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "BrandId" int4 NULL DEFAULT NULL,
  "CategoryId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_product" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_user";
CREATE TABLE "public"."sys_user" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user_104
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_user_104";
CREATE TABLE "public"."sys_user_104" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user_104" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user_105
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_user_105";
CREATE TABLE "public"."sys_user_105" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user_105" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_function
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_function";
CREATE TABLE "public"."sys_function" (
  "MenuId" int4 NOT NULL,
  "PageId" int4 NOT NULL,
  "FunctionName" varchar(50) NULL DEFAULT NULL,
  "Description" varchar(500) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_function" PRIMARY KEY("MenuId","PageId")
);
-- ----------------------------
-- Table structure for sys_update_entity
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_update_entity";
CREATE TABLE "public"."sys_update_entity" (
  "Id" int4 NOT NULL,
  "BooleanField" bool NULL DEFAULT NULL,
  "EnumField" int2 NULL DEFAULT NULL,
  "GuidField" varchar(50) NULL DEFAULT NULL,
  "DateTimeField" timestamp(6) NULL DEFAULT NULL,
  "DateOnlyField" date NULL DEFAULT NULL,
  "DateTimeOffsetField" timestamptz(6) NULL DEFAULT NULL,
  "TimeSpanField" interval(6) NULL DEFAULT NULL,
  "TimeOnlyField" time(6) NULL DEFAULT NULL,
  "ByteArrayField" bytea NULL DEFAULT NULL,
  "BitArrayField" bit(8) NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_update_entity" PRIMARY KEY("Id")
);



CREATE SCHEMA myschema;


-- ----------------------------
-- Table structure for sys_brand
-- ----------------------------


DROP TABLE IF EXISTS "myschema"."sys_brand";
CREATE TABLE "myschema"."sys_brand" (
  "Id" int4 NOT NULL,
  "BrandNo" varchar(50) NULL DEFAULT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,  
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_brand" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_company
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_company" CASCADE;
DROP SEQUENCE IF EXISTS "myschema"."sys_company_Id_seq" CASCADE;
CREATE TABLE "myschema"."sys_company" (
  "Id" SERIAL NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Nature" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_company" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_menu";
CREATE TABLE "myschema"."sys_menu" (
  "Id" int4 NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "ParentId" int4 NULL DEFAULT NULL,
  "PageId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_menu" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order";
CREATE TABLE "myschema"."sys_order" (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order_104_202405";
CREATE TABLE "myschema"."sys_order_104_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_104_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order_105_202405";
CREATE TABLE "myschema"."sys_order_105_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderNo" varchar(50)  NULL DEFAULT NULL,
  "ProductCount" int4 NULL DEFAULT NULL,
  "TotalAmount" float8 NULL DEFAULT NULL,
  "BuyerId" int4 NULL DEFAULT NULL,
  "BuyerSource" varchar(50) NULL DEFAULT NULL,
  "SellerId" int4 NULL DEFAULT NULL,  
  "Products" jsonb NULL DEFAULT NULL,
  "Disputes" jsonb NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_105_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order_detail";
CREATE TABLE "myschema"."sys_order_detail" (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail_104_202405
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order_detail_104_202405";
CREATE TABLE "myschema"."sys_order_detail_104_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail_104_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_order_detail_105_202405
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_order_detail_105_202405";
CREATE TABLE "myschema"."sys_order_detail_105_202405"  (
  "Id" varchar(50) NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "OrderId" varchar(50) NULL DEFAULT NULL,
  "ProductId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "Quantity" int4 NULL DEFAULT NULL,
  "Amount" float8 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_order_detail_105_202405" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_page
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_page";
CREATE TABLE "myschema"."sys_page" (
  "Id" int4 NOT NULL,
  "Url" varchar(200) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_page" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_product
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_product";
CREATE TABLE "myschema"."sys_product" (
  "Id" int4 NOT NULL,
  "ProductNo" varchar(50) NULL DEFAULT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "BrandId" int4 NULL DEFAULT NULL,
  "CategoryId" int4 NULL DEFAULT NULL,
  "Price" float8 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_product" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_user";
CREATE TABLE "myschema"."sys_user" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user_104
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_user_104";
CREATE TABLE "myschema"."sys_user_104" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user_104" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_user_105
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_user_105";
CREATE TABLE "myschema"."sys_user_105" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) NOT NULL,
  "Name" varchar(50) NULL DEFAULT NULL,
  "Gender" varchar(50) NULL DEFAULT NULL,
  "Age" int4 NULL DEFAULT NULL,
  "CompanyId" int4 NULL DEFAULT NULL,
  "GuidField" uuid NULL DEFAULT NULL,
  "SomeTimes" time(6) NULL DEFAULT NULL,
  "SourceType" varchar(50) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_user_105" PRIMARY KEY("Id")
);
-- ----------------------------
-- Table structure for sys_function
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_function";
CREATE TABLE "myschema"."sys_function" (
  "MenuId" int4 NOT NULL,
  "PageId" int4 NOT NULL,
  "FunctionName" varchar(50) NULL DEFAULT NULL,
  "Description" varchar(500) NULL DEFAULT NULL,
  "IsEnabled" bool NULL DEFAULT NULL,
  "CreatedAt" timestamp(6) NULL DEFAULT NULL,
  "CreatedBy" int4 NULL DEFAULT NULL,  
  "UpdatedAt" timestamp(6) NULL DEFAULT NULL,
  "UpdatedBy" int4 NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_function" PRIMARY KEY("MenuId","PageId")
);
-- ----------------------------
-- Table structure for sys_update_entity
-- ----------------------------
DROP TABLE IF EXISTS "myschema"."sys_update_entity";
CREATE TABLE "myschema"."sys_update_entity" (
  "Id" int4 NOT NULL,
  "BooleanField" bool NULL DEFAULT NULL,
  "EnumField" int2 NULL DEFAULT NULL,
  "GuidField" varchar(50) NULL DEFAULT NULL,
  "DateTimeField" timestamp(6) NULL DEFAULT NULL,
  "DateOnlyField" date NULL DEFAULT NULL,
  "DateTimeOffsetField" timestamptz(6) NULL DEFAULT NULL,
  "TimeSpanField" interval(6) NULL DEFAULT NULL,
  "TimeOnlyField" time(6) NULL DEFAULT NULL,
  "ByteArrayField" bytea NULL DEFAULT NULL,
  "BitArrayField" bit(8) NULL DEFAULT NULL,
  CONSTRAINT "pk_sys_update_entity" PRIMARY KEY("Id")
);
