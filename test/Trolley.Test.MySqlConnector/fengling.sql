/*
 Navicat Premium Data Transfer

 Source Server         : MySql
 Source Server Type    : MySQL
 Source Server Version : 101102 (10.11.2-MariaDB-1:10.11.2+maria~ubu2204)
 Source Host           : localhost:3306
 Source Schema         : fengling

 Target Server Type    : MySQL
 Target Server Version : 101102 (10.11.2-MariaDB-1:10.11.2+maria~ubu2204)
 File Encoding         : 65001

 Date: 29/04/2023 22:53:19
*/

CREATE DATABASE `fengling` CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
CREATE DATABASE `fengling1` CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
CREATE DATABASE `fengling2` CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for sys_brand
-- ----------------------------
DROP TABLE IF EXISTS `sys_brand`;
CREATE TABLE `sys_brand`  (
  `Id` int NOT NULL,
  `BrandNo` varchar(50) NULL DEFAULT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_brand` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_company
-- ----------------------------
DROP TABLE IF EXISTS `sys_company`;
CREATE TABLE `sys_company`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Nature` varchar(50) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_company` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_menu`;
CREATE TABLE `sys_menu`  (
  `Id` int NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `ParentId` int NULL DEFAULT NULL,
  `PageId` int NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
   CONSTRAINT `pk_sys_menu` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS `sys_order`;
CREATE TABLE `sys_order`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderNo` varchar(50) NULL DEFAULT NULL,
  `ProductCount` int NULL DEFAULT NULL,
  `TotalAmount` double NULL DEFAULT NULL,
  `BuyerId` int NULL DEFAULT NULL,
  `BuyerSource` varchar(50) NULL DEFAULT NULL,
  `SellerId` int NULL DEFAULT NULL,
  `Products` JSON NULL DEFAULT NULL,
  `Disputes` JSON NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_104_202405
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_104_202405`;
CREATE TABLE `sys_order_104_202405`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderNo` varchar(50) NULL DEFAULT NULL,
  `ProductCount` int NULL DEFAULT NULL,
  `TotalAmount` double NULL DEFAULT NULL,
  `BuyerId` int NULL DEFAULT NULL,
  `BuyerSource` varchar(50) NULL DEFAULT NULL,
  `SellerId` int NULL DEFAULT NULL,
  `Products` JSON NULL DEFAULT NULL,
  `Disputes` JSON NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_104_202405` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_104_202406
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_104_202406`;
CREATE TABLE `sys_order_104_202406`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderNo` varchar(50) NULL DEFAULT NULL,
  `ProductCount` int NULL DEFAULT NULL,
  `TotalAmount` double NULL DEFAULT NULL,
  `BuyerId` int NULL DEFAULT NULL,
  `BuyerSource` varchar(50) NULL DEFAULT NULL,
  `SellerId` int NULL DEFAULT NULL,
  `Products` JSON NULL DEFAULT NULL,
  `Disputes` JSON NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_104_202406` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_105_202405
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_105_202405`;
CREATE TABLE `sys_order_105_202405`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderNo` varchar(50) NULL DEFAULT NULL,
  `ProductCount` int NULL DEFAULT NULL,
  `TotalAmount` double NULL DEFAULT NULL,
  `BuyerId` int NULL DEFAULT NULL,
  `BuyerSource` varchar(50) NULL DEFAULT NULL,
  `SellerId` int NULL DEFAULT NULL,
  `Products` JSON NULL DEFAULT NULL,
  `Disputes` JSON NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_105_202405` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_detail
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_detail`;
CREATE TABLE `sys_order_detail`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderId` varchar(50) NULL DEFAULT NULL,
  `ProductId` int NULL DEFAULT NULL,
  `Price` double(10, 2) NULL DEFAULT NULL,
  `Quantity` int NULL DEFAULT NULL,
  `Amount` double(10, 2) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_detail` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_detail_104_202405
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_detail_104_202405`;
CREATE TABLE `sys_order_detail_104_202405`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderId` varchar(50) NULL DEFAULT NULL,
  `ProductId` int NULL DEFAULT NULL,
  `Price` double(10, 2) NULL DEFAULT NULL,
  `Quantity` int NULL DEFAULT NULL,
  `Amount` double(10, 2) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_detail_104_202405` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_order_detail_105_202405
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_detail_105_202405`;
CREATE TABLE `sys_order_detail_105_202405`  (
  `Id` varchar(50) NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `OrderId` varchar(50) NULL DEFAULT NULL,
  `ProductId` int NULL DEFAULT NULL,
  `Price` double(10, 2) NULL DEFAULT NULL,
  `Quantity` int NULL DEFAULT NULL,
  `Amount` double(10, 2) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_order_detail_105_202405` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_page
-- ----------------------------
DROP TABLE IF EXISTS `sys_page`;
CREATE TABLE `sys_page`  (
  `Id` int NOT NULL,
  `Url` varchar(200) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_page` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_product
-- ----------------------------
DROP TABLE IF EXISTS `sys_product`;
CREATE TABLE `sys_product`  (
  `Id` int NOT NULL,
  `ProductNo` varchar(50) NULL DEFAULT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `BrandId` int NULL DEFAULT NULL,
  `CategoryId` int NULL DEFAULT NULL,
  `Price` double NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_product` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user`  (
  `Id` int NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Gender` enum('Unknown','Male','Female') NULL DEFAULT NULL,
  `Age` int NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `GuidField` char(36) NULL DEFAULT NULL,
  `SomeTimes` time(6) NULL DEFAULT NULL,
  `SourceType` enum('Website','Wechat','Douyin','Taobao') NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_user` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_user_104
-- ----------------------------
DROP TABLE IF EXISTS `sys_user_104`;
CREATE TABLE `sys_user_104`  (
  `Id` int NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Gender` enum('Unknown','Male','Female') NULL DEFAULT NULL,
  `Age` int NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `GuidField` char(36) NULL DEFAULT NULL,
  `SomeTimes` time(6) NULL DEFAULT NULL,
  `SourceType` enum('Website','Wechat','Douyin','Taobao') NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_user_104` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_user_105
-- ----------------------------
DROP TABLE IF EXISTS `sys_user_105`;
CREATE TABLE `sys_user_105`  (
  `Id` int NOT NULL,
  `TenantId` varchar(50) NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Gender` enum('Unknown','Male','Female') NULL DEFAULT NULL,
  `Age` int NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `GuidField` char(36) NULL DEFAULT NULL,
  `SomeTimes` time(6) NULL DEFAULT NULL,
  `SourceType` enum('Website','Wechat','Douyin','Taobao') NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_user_105` PRIMARY KEY (`Id`)
);
-- ----------------------------
-- Table structure for sys_function
-- ----------------------------
DROP TABLE IF EXISTS `sys_function`;
CREATE TABLE `sys_function`  (
  `MenuId` int NOT NULL,
  `PageId` int NOT NULL,
  `FunctionName` varchar(50) NULL DEFAULT NULL,
  `Description` varchar(500) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_function` PRIMARY KEY (`MenuId`,`PageId`)
);
-- ----------------------------
-- Table structure for sys_update_entity
-- ----------------------------
DROP TABLE IF EXISTS `sys_update_entity`;
CREATE TABLE `sys_update_entity`  (
  `Id` int NOT NULL,
  `BooleanField` tinyint(1) NULL DEFAULT NULL,
  `EnumField` tinyint NULL DEFAULT NULL,
  `GuidField` char(36) NULL DEFAULT NULL,
  `DateTimeField` datetime NULL DEFAULT NULL,
  `DateOnlyField` date NULL DEFAULT NULL,
  `DateTimeOffsetField` timestamp NULL DEFAULT NULL,
  `TimeSpanField` time NULL DEFAULT NULL,
  `TimeOnlyField` time NULL DEFAULT NULL,
  `ByteArrayField` varbinary(100) NULL DEFAULT NULL,
  `BitArrayField` bit(8) NULL DEFAULT NULL,
  CONSTRAINT `pk_sys_update_entity` PRIMARY KEY (`Id`)
);
DELIMITER $$
CREATE OR REPLACE PROCEDURE GET_USER(IN pId INTEGER, OUT pOut VARCHAR(50))
BEGIN
	SET pOut = 'OK';
	SELECT * FROM sys_user WHERE Id = pId;
END;
$$
DELIMITER;

DELIMITER $$
CREATE OR REPLACE PROCEDURE `UPDATE_USER`(IN pId INTEGER, OUT pOut VARCHAR(50))
BEGIN
	SET pOut = 'OK';
	UPDATE sys_user SET `Name`='UpdatedName',`Age`=18 WHERE Id = pId;
END;
$$
DELIMITER;



TRUNCATE TABLE `fengling1`.`sys_order_detail`;
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('1', '1', '1', 1, 299.00, 1, 299.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('2', '1', '1', 2, 159.00, 1, 159.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('3', '2', '1', 3, 69.00, 1, 69.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('4', '1', '2', 1, 299.00, 1, 299.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('5', '2', '2', 3, 69.00, 1, 69.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('6', '3', '3', 2, 199.00, 1, 199.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling1`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('7', '1', '3', 1, 550.00, 3, 1650.00, 1, '2025-06-22 10:57:39', 1, '2025-06-22 10:57:39', 1);

TRUNCATE TABLE `fengling1`.`sys_menu`;
INSERT INTO `fengling1`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (1, '系统管理', 0, 0, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling1`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (2, '用户管理', 1, 1, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling1`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (3, '角色管理', 1, 2, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);

TRUNCATE TABLE `fengling1`.`sys_page`;
INSERT INTO `fengling1`.`sys_page` (`Id`, `Url`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (1, '/user/index', 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling1`.`sys_page` (`Id`, `Url`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (2, '/role/index', 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);

TRUNCATE TABLE `fengling2`.`sys_order_detail`;
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('1', '1', '1', 1, 299.00, 1, 299.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('2', '1', '1', 2, 159.00, 1, 159.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('3', '2', '1', 3, 69.00, 1, 69.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('4', '1', '2', 1, 299.00, 1, 299.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('5', '2', '2', 3, 69.00, 1, 69.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('6', '3', '3', 2, 199.00, 1, 199.00, 1, '2025-06-22 10:58:02', 1, '2025-06-22 10:58:02', 1);
INSERT INTO `fengling2`.`sys_order_detail` (`Id`, `TenantId`, `OrderId`, `ProductId`, `Price`, `Quantity`, `Amount`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES ('7', '1', '3', 1, 550.00, 3, 1650.00, 1, '2025-06-22 10:57:39', 1, '2025-06-22 10:57:39', 1);

TRUNCATE TABLE `fengling2`.`sys_menu`;
INSERT INTO `fengling2`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (1, '系统管理', 0, 0, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling2`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (2, '用户管理', 1, 1, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling2`.`sys_menu` (`Id`, `Name`, `ParentId`, `PageId`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (3, '角色管理', 1, 2, 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);

TRUNCATE TABLE `fengling2`.`sys_page`;
INSERT INTO `fengling2`.`sys_page` (`Id`, `Url`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (1, '/user/index', 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
INSERT INTO `fengling2`.`sys_page` (`Id`, `Url`, `IsEnabled`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`) VALUES (2, '/role/index', 1, '2025-06-22 11:27:48', 1, '2025-06-22 11:27:48', 1);
