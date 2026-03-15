CREATE DATABASE IF NOT EXISTS `RestaurantSystem_Database`
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE `RestaurantSystem_Database`;

-- =============================================
-- TABLE 1: Admins
-- =============================================
CREATE TABLE IF NOT EXISTS `Admins` (
    `AdminId` INT NOT NULL AUTO_INCREMENT,
    `Username` VARCHAR(50) NOT NULL,
    `Password` VARCHAR(255) NOT NULL,
    `FullName` VARCHAR(100) NULL,
    `Email` VARCHAR(100) NULL,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`AdminId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `Admins` (`Username`, `Password`, `FullName`, `Email`, `IsActive`) VALUES
('admin', 'admin123', 'System Administrator', 'admin@tastygmailbites.com', 1);