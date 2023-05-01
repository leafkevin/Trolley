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
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_brand
-- ----------------------------
INSERT INTO `sys_brand` VALUES (1, 'BN-001', '波司登', 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_brand` VALUES (2, 'BN-002', '雪中飞', 2, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_brand` VALUES (3, 'BN-003', '优衣库', 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

-- ----------------------------
-- Table structure for sys_company
-- ----------------------------
DROP TABLE IF EXISTS `sys_company`;
CREATE TABLE `sys_company`  (
  `Id` int NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Nature` varchar(50) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_company
-- ----------------------------
INSERT INTO `sys_company` VALUES (1, '微软', 'Internet', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_company` VALUES (2, '谷歌', 'Internet', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

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
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_menu
-- ----------------------------

-- ----------------------------
-- Table structure for sys_order
-- ----------------------------
DROP TABLE IF EXISTS `sys_order`;
CREATE TABLE `sys_order`  (
  `Id` int NOT NULL,
  `OrderNo` varchar(50) NULL DEFAULT NULL,
  `ProductCount` int NULL DEFAULT NULL,
  `TotalAmount` double NULL DEFAULT NULL,
  `BuyerId` int NULL DEFAULT NULL,
  `SellerId` int NULL DEFAULT NULL,
  `Products` JSON NULL,
  `Disputes` JSON NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_order
-- ----------------------------
INSERT INTO `sys_order` VALUES (1, 'ON-001', 2, 500, 1, 2, '[1,2]', '{\"Id\":1,\"Users\":\"Buyer1,Seller1\",\"Content\":\"\\u65E0\\u826F\\u5546\\u5BB6\\uFF0C\\u6295\\u8BC9\\uFF0C\\u6295\\u8BC9\",\"Result\":\"\\u540C\\u610F\\u66F4\\u6362\",\"CreatedAt\":\"2023-04-29T00:05:28.9494366+08:00\"}', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order` VALUES (2, 'ON-002', NULL, 350, 2, 1, '[1,3]', '{\"Id\":2,\"Users\":\"Buyer2,Seller2\",\"Content\":\"\\u65E0\\u826F\\u5546\\u5BB6\",\"Result\":\"\\u540C\\u610F\\u9000\\u6B3E\",\"CreatedAt\":\"2023-04-29T00:05:28.9495535+08:00\"}', 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order` VALUES (3, 'ON-003', 1, 199, 1, 2, '[2]', NULL, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

-- ----------------------------
-- Table structure for sys_order_detail
-- ----------------------------
DROP TABLE IF EXISTS `sys_order_detail`;
CREATE TABLE `sys_order_detail`  (
  `Id` int NOT NULL,
  `OrderId` int NULL DEFAULT NULL,
  `ProductId` int NULL DEFAULT NULL,
  `Price` double(10, 2) NULL DEFAULT NULL,
  `Quantity` int NULL DEFAULT NULL,
  `Amount` double(10, 2) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_order_detail
-- ----------------------------
INSERT INTO `sys_order_detail` VALUES (1, 1, 1, 299.00, 1, 299.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order_detail` VALUES (2, 1, 2, 159.00, 1, 159.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order_detail` VALUES (3, 1, 3, 69.00, 1, 69.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order_detail` VALUES (4, 2, 1, 299.00, 1, 299.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order_detail` VALUES (5, 2, 3, 69.00, 1, 69.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_order_detail` VALUES (6, 3, 2, 199.00, 1, 199.00, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

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
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_page
-- ----------------------------

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
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_product
-- ----------------------------
INSERT INTO `sys_product` VALUES (1, 'PN-001', '波司登羽绒服', 1, 1, 550, 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_product` VALUES (2, 'PN-002', '雪中飞羽绒裤', 2, 2, 350, 2, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_product` VALUES (3, 'PN-003', '优衣库保暖内衣', 3, 3, 180, 1, 1, '2023-04-29 00:05:28', 1, '2023-04-29 00:05:28', 1);

-- ----------------------------
-- Table structure for sys_user
-- ----------------------------
DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user`  (
  `Id` int NOT NULL,
  `Name` varchar(50) NULL DEFAULT NULL,
  `Gender` tinyint NULL DEFAULT NULL,
  `Age` int NULL DEFAULT NULL,
  `CompanyId` int NULL DEFAULT NULL,
  `GuidField` char(36) NULL DEFAULT NULL,
  `SomeTimes` time(6) NULL DEFAULT NULL,
  `IsEnabled` tinyint(1) NULL DEFAULT NULL,
  `CreatedAt` datetime NULL DEFAULT NULL,
  `CreatedBy` int NULL DEFAULT NULL,
  `UpdatedAt` datetime NULL DEFAULT NULL,
  `UpdatedBy` int NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
);

-- ----------------------------
-- Records of sys_user
-- ----------------------------
INSERT INTO `sys_user` VALUES (1, 'leafkevin', 2, 25, 1, 'e09d9d46-1783-475b-9d57-2721d163c29f', '01:19:29.000000', 1, '2023-03-03 00:00:00', 1, '2023-04-29 00:05:28', 1);
INSERT INTO `sys_user` VALUES (2, 'cindy', 2, 21, 2, '20687f1c-3f27-4741-894e-fa59bcd43dbc', '01:35:30.000000', 1, '2023-03-03 06:06:06', 1, '2023-04-29 00:05:28', 1);

SET FOREIGN_KEY_CHECKS = 1;
