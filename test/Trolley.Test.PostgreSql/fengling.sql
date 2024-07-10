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

 Date: 10/07/2024 09:32:52
*/


-- ----------------------------
-- Table structure for sys_brand
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_brand";
CREATE TABLE "public"."sys_brand" (
  "Id" int4 NOT NULL,
  "BrandNo" varchar(255) COLLATE "pg_catalog"."default",
  "Name" varchar(50) COLLATE "pg_catalog"."default",
  "CompanyId" int4,
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_brand"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_brand
-- ----------------------------
INSERT INTO "public"."sys_brand" VALUES (1, 'BN-001', '波司登', 1, 't', 1, '2024-07-10 09:29:12.026713', 1, '2024-07-10 09:29:12.026714');
INSERT INTO "public"."sys_brand" VALUES (2, 'BN-002', '雪中飞', 2, 't', 1, '2024-07-10 09:29:12.026714', 1, '2024-07-10 09:29:12.026714');
INSERT INTO "public"."sys_brand" VALUES (3, 'BN-003', '优衣库', 1, 't', 1, '2024-07-10 09:29:12.026714', 1, '2024-07-10 09:29:12.026714');

-- ----------------------------
-- Table structure for sys_company
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_company";
CREATE TABLE "public"."sys_company" (
  "Id" int4 NOT NULL,
  "Name" varchar(50) COLLATE "pg_catalog"."default",
  "Nature" varchar(50) COLLATE "pg_catalog"."default",
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;

-- ----------------------------
-- Records of sys_company
-- ----------------------------
INSERT INTO "public"."sys_company" VALUES (1, '微软', 'Internet', 't', 1, '2024-07-10 09:29:12.024123', 1, '2024-07-10 09:29:12.024124');
INSERT INTO "public"."sys_company" VALUES (2, '谷歌', 'Internet', 't', 1, '2024-07-10 09:29:12.024124', 1, '2024-07-10 09:29:12.024124');

-- ----------------------------
-- Table structure for sys_function
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_function";
CREATE TABLE "public"."sys_function" (
  "MenuId" int4 NOT NULL,
  "PageId" int4 NOT NULL,
  "FunctionName" varchar(50) COLLATE "pg_catalog"."default",
  "Description" varchar(200) COLLATE "pg_catalog"."default",
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_function"."MenuId" IS '用户ID';

-- ----------------------------
-- Records of sys_function
-- ----------------------------

-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_menu";
CREATE TABLE "public"."sys_menu" (
  "Id" int4 NOT NULL,
  "Name" varchar(50) COLLATE "pg_catalog"."default",
  "ParentId" int4,
  "PageId" int4,
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_menu"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_menu
-- ----------------------------
INSERT INTO "public"."sys_menu" VALUES (1, '系统管理', 0, 0, 't', 1, '2024-07-10 09:29:12.041273', 1, '2024-07-10 09:29:12.041274');
INSERT INTO "public"."sys_menu" VALUES (2, '用户管理', 1, 1, 't', 1, '2024-07-10 09:29:12.041274', 1, '2024-07-10 09:29:12.041274');
INSERT INTO "public"."sys_menu" VALUES (3, '角色管理', 1, 2, 't', 1, '2024-07-10 09:29:12.041274', 1, '2024-07-10 09:29:12.041274');

-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order";
CREATE TABLE "public"."sys_order" (
  "Id" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "TenantId" varchar(50) COLLATE "pg_catalog"."default",
  "OrderNo" varchar(50) COLLATE "pg_catalog"."default",
  "TotalAmount" float8,
  "BuyerId" int4,
  "BuyerSource" varchar(50) COLLATE "pg_catalog"."default",
  "SellerId" int4,
  "ProductCount" int4,
  "Products" jsonb,
  "Disputes" jsonb,
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;

-- ----------------------------
-- Records of sys_order
-- ----------------------------
INSERT INTO "public"."sys_order" VALUES ('1', '1', 'ON-001', 500, 1, 'Douyin', 2, 2, '[1, 2]', '{"id": 1, "users": "Buyer1,Seller1", "result": "同意更换", "content": "无良商家，投诉，投诉", "createdAt": "2024-07-10T09:29:12.0329058+08:00"}', 't', 1, '2024-07-10 09:29:12.032906', 1, '2024-07-10 09:29:12.032906');
INSERT INTO "public"."sys_order" VALUES ('2', '2', 'ON-002', 350, 2, 'Taobao', 1, NULL, '[1, 3]', '{"id": 2, "users": "Buyer2,Seller2", "result": "同意退款", "content": "无良商家", "createdAt": "2024-07-10T09:29:12.0329067+08:00"}', 't', 1, '2024-07-10 09:29:12.032906', 1, '2024-07-10 09:29:12.032906');
INSERT INTO "public"."sys_order" VALUES ('3', '3', 'ON-003', 199, 1, 'Douyin', 2, 1, '[2]', NULL, 't', 1, '2024-07-10 09:29:12.032907', 1, '2024-07-10 09:29:12.032907');

-- ----------------------------
-- Table structure for sys_order_detail
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_order_detail";
CREATE TABLE "public"."sys_order_detail" (
  "Id" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "TenantId" varchar(50) COLLATE "pg_catalog"."default",
  "OrderId" varchar(255) COLLATE "pg_catalog"."default",
  "ProductId" int4,
  "Price" float8,
  "Quantity" int4,
  "Amount" float8,
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;

-- ----------------------------
-- Records of sys_order_detail
-- ----------------------------
INSERT INTO "public"."sys_order_detail" VALUES ('1', '1', '1', 1, 299, 1, 299, 't', 1, '2024-07-10 09:29:12.037631', 1, '2024-07-10 09:29:12.037632');
INSERT INTO "public"."sys_order_detail" VALUES ('2', '1', '1', 2, 159, 1, 159, 't', 1, '2024-07-10 09:29:12.037633', 1, '2024-07-10 09:29:12.037633');
INSERT INTO "public"."sys_order_detail" VALUES ('3', '2', '1', 3, 69, 1, 69, 't', 1, '2024-07-10 09:29:12.037633', 1, '2024-07-10 09:29:12.037634');
INSERT INTO "public"."sys_order_detail" VALUES ('4', '1', '2', 1, 299, 1, 299, 't', 1, '2024-07-10 09:29:12.037634', 1, '2024-07-10 09:29:12.037634');
INSERT INTO "public"."sys_order_detail" VALUES ('5', '2', '2', 3, 69, 1, 69, 't', 1, '2024-07-10 09:29:12.037634', 1, '2024-07-10 09:29:12.037634');
INSERT INTO "public"."sys_order_detail" VALUES ('6', '3', '3', 2, 199, 1, 199, 't', 1, '2024-07-10 09:29:12.037635', 1, '2024-07-10 09:29:12.037635');

-- ----------------------------
-- Table structure for sys_page
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_page";
CREATE TABLE "public"."sys_page" (
  "Id" int4 NOT NULL,
  "Url" varchar(255) COLLATE "pg_catalog"."default",
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_page"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_page
-- ----------------------------
INSERT INTO "public"."sys_page" VALUES (1, '/user/index', 't', 1, '2024-07-10 09:29:12.044566', 1, '2024-07-10 09:29:12.044567');
INSERT INTO "public"."sys_page" VALUES (2, '/role/index', 't', 1, '2024-07-10 09:29:12.044567', 1, '2024-07-10 09:29:12.044567');

-- ----------------------------
-- Table structure for sys_product
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_product";
CREATE TABLE "public"."sys_product" (
  "Id" int4 NOT NULL,
  "ProductNo" varchar(255) COLLATE "pg_catalog"."default",
  "Name" varchar(50) COLLATE "pg_catalog"."default",
  "Price" float8,
  "BrandId" int4,
  "CategoryId" int4,
  "CompanyId" int4,
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_product"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_product
-- ----------------------------
INSERT INTO "public"."sys_product" VALUES (1, 'PN-001', '波司登羽绒服', 550, 1, 1, 1, 't', 1, '2024-07-10 09:29:12.02949', 1, '2024-07-10 09:29:12.02949');
INSERT INTO "public"."sys_product" VALUES (2, 'PN-002', '雪中飞羽绒裤', 350, 2, 2, 2, 't', 1, '2024-07-10 09:29:12.029491', 1, '2024-07-10 09:29:12.029491');
INSERT INTO "public"."sys_product" VALUES (3, 'PN-003', '优衣库保暖内衣', 180, 3, 3, 1, 't', 1, '2024-07-10 09:29:12.029491', 1, '2024-07-10 09:29:12.029491');

-- ----------------------------
-- Table structure for sys_update_entity
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_update_entity";
CREATE TABLE "public"."sys_update_entity" (
  "Id" int4 NOT NULL,
  "BooleanField" bool,
  "EnumField" varchar(255) COLLATE "pg_catalog"."default",
  "GuidField" uuid,
  "DateTimeField" timestamp(6),
  "DateOnlyField" date,
  "DateTimeOffsetField" timestamptz(6),
  "TimeSpanField" interval(6),
  "TimeOnlyField" time(6)
)
;
COMMENT ON COLUMN "public"."sys_update_entity"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_update_entity
-- ----------------------------

-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS "public"."sys_user";
CREATE TABLE "public"."sys_user" (
  "Id" int4 NOT NULL,
  "TenantId" varchar(50) COLLATE "pg_catalog"."default",
  "Name" varchar(50) COLLATE "pg_catalog"."default",
  "Gender" varchar(50) COLLATE "pg_catalog"."default",
  "Age" int4,
  "CompanyId" int4,
  "SomeTimes" time(6),
  "GuidField" uuid,
  "SourceType" varchar(50) COLLATE "pg_catalog"."default",
  "IsEnabled" bool,
  "CreatedBy" int4,
  "CreatedAt" timestamp(6),
  "UpdatedBy" int4,
  "UpdatedAt" timestamp(6)
)
;
COMMENT ON COLUMN "public"."sys_user"."Id" IS '用户ID';

-- ----------------------------
-- Records of sys_user
-- ----------------------------
INSERT INTO "public"."sys_user" VALUES (1, '1', 'leafkevin', 'Male', 25, 1, '01:19:29', '041a2d29-e522-4b0b-ab96-aab7bb728781', 'Douyin', 't', 1, '2023-03-10 06:07:08', 1, '2023-03-15 16:27:38');
INSERT INTO "public"."sys_user" VALUES (2, '2', 'cindy', 'Male', 21, 2, '01:35:30', '4d661c82-ba5d-4f8d-875d-ee95637a19db', 'Taobao', 't', 1, '2024-07-09 06:07:08', 1, '2024-07-10 09:29:12.019633');

-- ----------------------------
-- Primary Key structure for table sys_brand
-- ----------------------------
ALTER TABLE "public"."sys_brand" ADD CONSTRAINT "sys_brand_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_company
-- ----------------------------
ALTER TABLE "public"."sys_company" ADD CONSTRAINT "sys_company_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_function
-- ----------------------------
ALTER TABLE "public"."sys_function" ADD CONSTRAINT "sys_function_pkey" PRIMARY KEY ("MenuId", "PageId");

-- ----------------------------
-- Primary Key structure for table sys_menu
-- ----------------------------
ALTER TABLE "public"."sys_menu" ADD CONSTRAINT "sys_menu_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_order
-- ----------------------------
ALTER TABLE "public"."sys_order" ADD CONSTRAINT "sys_order_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_order_detail
-- ----------------------------
ALTER TABLE "public"."sys_order_detail" ADD CONSTRAINT "sys_order_detail_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_page
-- ----------------------------
ALTER TABLE "public"."sys_page" ADD CONSTRAINT "sys_page_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_product
-- ----------------------------
ALTER TABLE "public"."sys_product" ADD CONSTRAINT "sys_product_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_update_entity
-- ----------------------------
ALTER TABLE "public"."sys_update_entity" ADD CONSTRAINT "sys_update_entity_pkey" PRIMARY KEY ("Id");

-- ----------------------------
-- Primary Key structure for table sys_user
-- ----------------------------
ALTER TABLE "public"."sys_user" ADD CONSTRAINT "sys_user_pkey" PRIMARY KEY ("Id");
