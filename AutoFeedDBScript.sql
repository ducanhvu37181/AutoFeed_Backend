-- ======================================================
-- 1. DATABASE SETUP
-- ======================================================
USE [master]
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AutoFeedDB')
BEGIN
    CREATE DATABASE [AutoFeedDB];
END
GO

USE [AutoFeedDB]
GO

-- ======================================================
-- 2. BASE TABLES (Level 0)
-- ======================================================

CREATE TABLE [dbo].[Role] (
    [roleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [description] NVARCHAR(255) NULL
);

CREATE TABLE [dbo].[Food] (
    [foodID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NULL,
    [type] NVARCHAR(50) NULL,
    [price] DECIMAL(18, 2) NULL,
    [quantity] INT NULL,
    [note] NVARCHAR(MAX) NULL
);

CREATE TABLE [dbo].[Barn] (
    [barnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [temperature] DECIMAL(5, 2) NULL,
    [humidity] DECIMAL(5, 2) NULL,
    [type] NVARCHAR(50) NULL,
    [area] DECIMAL(18, 2) NULL,
    [createDate] DATETIME DEFAULT GETDATE()
);

CREATE TABLE [dbo].[IoT_Device] (
    [deviceID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1 -- Boolean: 1=Active, 0=Inactive
);

CREATE TABLE [dbo].[FlockChicken] (
    [flockID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NULL,
    [quantity] INT NULL,
    [weight] DECIMAL(18, 2) NULL,
    [DoB] DATE NULL,                 -- Replaced Age with Date of Birth
    [transferDate] DATE NULL,        -- Track migration between barns
    [healthStatus] NVARCHAR(100) NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1         -- Track if flock is currently on farm
);

CREATE TABLE [dbo].[Task] (
    [taskID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] NVARCHAR(100) NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 0 
);

-- ======================================================
-- 3. DEPENDENT TABLES (Level 1)
-- ======================================================

CREATE TABLE [dbo].[User] (
    [userID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [roleID] INT NULL,
    [email] NVARCHAR(100) NOT NULL UNIQUE,
    [password] NVARCHAR(255) NOT NULL,
    [fullName] NVARCHAR(100) NULL,
    [phone] NVARCHAR(20) NULL,
    [username] NVARCHAR(50) NULL UNIQUE,
    [lastLogin] DATETIME NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_User_Role] FOREIGN KEY([roleID]) REFERENCES [dbo].[Role] ([roleID])
);

CREATE TABLE [dbo].[BarnIoT_Device] (
    [bDeviceID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NULL,
    [deviceID] INT NULL,
    [installationDate] DATE NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_BarnIoT_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_BarnIoT_Device] FOREIGN KEY([deviceID]) REFERENCES [dbo].[IoT_Device] ([deviceID])
);

CREATE TABLE [dbo].[Data_IoT] (
    [dataID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [deviceID] INT NOT NULL,
    [value] DECIMAL(18, 4) NULL,
    [description] NVARCHAR(MAX) NULL,
    [recordDate] DATETIME DEFAULT GETDATE(), -- Ngay + Gio
    [sequenceNumber] INT NULL,                -- Lan thu may
    CONSTRAINT [FK_DataIoT_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_DataIoT_Device] FOREIGN KEY([deviceID]) REFERENCES [dbo].[IoT_Device] ([deviceID])
);

CREATE TABLE [dbo].[LargeChicken] (
    [chickenLID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [flockID] INT NULL,
    [name] NVARCHAR(100) NULL,
    [weight] DECIMAL(18, 2) NULL,
    [healthStatus] NVARCHAR(100) NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1, -- Removed Age, added isActive Boolean
    CONSTRAINT [FK_LargeChicken_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID])
);

CREATE TABLE [dbo].[Inventory] (
    [inventID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [foodID] INT NULL,
    [quantity] INT NULL,
    [weightPerBag] DECIMAL(18, 2) NULL,
    [expiredDate] DATE NULL,
    CONSTRAINT [FK_Inventory_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID])
);

CREATE TABLE [dbo].[FoodStorage] (
    [storageID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [foodID] INT NULL,
    [barnID] INT NULL,
    [food_weight] DECIMAL(18, 2) NULL,
    [leftover_food] DECIMAL(18, 2) NULL,
    [note] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_Storage_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_Storage_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID])
);

-- ======================================================
-- 4. RELATIONSHIP TABLES (Level 2)
-- ======================================================

CREATE TABLE [dbo].[ChickenBarn] (
    [CBarnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NULL,
    [chickenLID] INT NULL UNIQUE,
    [flockID] INT NULL UNIQUE,
    [startDate] DATE NULL,
    [exportDate] DATE NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_CBarn_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_CBarn_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_CBarn_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

CREATE TABLE [dbo].[FeedingRule] (
    [ruleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [chickenLID] INT NULL UNIQUE,
    [flockID] INT NULL UNIQUE,
    [times] INT NULL,
    [description] NVARCHAR(MAX) NULL,
    [note] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_Rule_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_Rule_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

-- ======================================================
-- 5. OPERATION & LOGGING TABLES (Level 3)
-- ======================================================

CREATE TABLE [dbo].[FeedingRuleDetail] (
    [feedRuleDetailID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ruleID] INT NULL,
    [foodID] INT NULL,
    [startDate] DATE NULL,
    [endDate] DATE NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_Detail_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID]),
    CONSTRAINT [FK_Detail_Rule] FOREIGN KEY([ruleID]) REFERENCES [dbo].[FeedingRule] ([ruleID])
);

CREATE TABLE [dbo].[Report] (
    [reportID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NULL,
    [type] NVARCHAR(50) NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1,
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Report_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

CREATE TABLE [dbo].[Request] (
    [requestID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NULL,
    [type] NVARCHAR(50) NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 0,
    [createdAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Request_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

CREATE TABLE [dbo].[Schedule] (
    [schedID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NULL,
    [taskID] INT NULL,
    [CBarnID] INT NULL,
    [description] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 0,
    [startDate] DATETIME NULL,
    [endDate] DATETIME NULL,
    [createdDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Sched_CBarn] FOREIGN KEY([CBarnID]) REFERENCES [dbo].[ChickenBarn] ([CBarnID]),
    CONSTRAINT [FK_Sched_Task] FOREIGN KEY([taskID]) REFERENCES [dbo].[Task] ([taskID]),
    CONSTRAINT [FK_Sched_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);
GO

-- ======================================================
-- 6. SAMPLE DATA INSERTION (NO ID TYPING)
-- ======================================================

-- Roles
INSERT INTO [dbo].[Role] (description) VALUES ('Administrator'), ('Farm Worker');

-- Food
INSERT INTO [dbo].[Food] (name, type, price, quantity) VALUES ('Organic Corn', 'Grain', 15.50, 500), ('Vitamin Pellets', 'Supplement', 25.00, 200);

-- Barns
INSERT INTO [dbo].[Barn] (temperature, humidity, type, area) VALUES (24.5, 60.0, 'Broiler', 500.00), (22.0, 55.0, 'Layer', 300.00);

-- IoT Devices
INSERT INTO [dbo].[IoT_Device] (name, description, status) VALUES ('Sensor-T1', 'Temperature Sensor', 1), ('Sensor-H1', 'Humidity Sensor', 1);

-- Flock
INSERT INTO [dbo].[FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus, isActive) 
VALUES ('White Leghorn Flock A', 1000, 1.2, '2026-01-10', '2026-03-01', 'Healthy', 1);

-- Users
INSERT INTO [dbo].[User] (roleID, email, password, fullName, username, status) 
VALUES (1, 'admin@autofeed.com', 'hashed_pass_123', 'John Doe', 'admin_john', 1),
       (2, 'worker1@autofeed.com', 'hashed_pass_456', 'Jane Smith', 'jane_worker', 1);

-- Data IoT (Barn 1, Device 1)
INSERT INTO [dbo].[Data_IoT] (barnID, deviceID, value, description, sequenceNumber) 
VALUES (1, 1, 24.8, 'Standard reading', 1), (1, 1, 25.1, 'Standard reading', 2);

-- Large Chicken
INSERT INTO [dbo].[LargeChicken] (flockID, name, weight, healthStatus, isActive) 
VALUES (1, 'Queen Hen 01', 2.5, 'Excellent', 1);

-- Assign Chicken to Barn
INSERT INTO [dbo].[ChickenBarn] (barnID, flockID, startDate, status) VALUES (1, 1, '2026-03-01', 1);

-- Tasks
INSERT INTO [dbo].[Task] (title, description, status) VALUES ('Morning Feeding', 'Distribute 50kg of corn', 0);

-- Schedule
INSERT INTO [dbo].[Schedule] (userID, taskID, CBarnID, startDate, status) VALUES (2, 1, 1, GETDATE(), 0);
GO