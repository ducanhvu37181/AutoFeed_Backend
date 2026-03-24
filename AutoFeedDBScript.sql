-- ======================================================
-- 1. DATABASE SETUP
-- ======================================================
USE [master]
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'AutoFeedDB')
BEGIN
    ALTER DATABASE [AutoFeedDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [AutoFeedDB];
END
GO

CREATE DATABASE [AutoFeedDB];
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
    [status] BIT DEFAULT 1 -- 1=Active, 0=Inactive
);

CREATE TABLE [dbo].[FlockChicken] (
    [flockID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NULL,
    [quantity] INT NULL,
    [weight] DECIMAL(18, 2) NULL,
    [DoB] DATE NULL,                 
    [transferDate] DATE NULL,        
    [healthStatus] NVARCHAR(100) NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1         
);

CREATE TABLE [dbo].[Task] (
    [taskID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] NVARCHAR(100) NULL,
    [description] NVARCHAR(MAX) NULL,
    [startTime] TIME NULL,           -- Time only
    [endTime] TIME NULL,             -- Time only
    [status] BIT DEFAULT 1 
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
    [recordDate] DATETIME DEFAULT GETDATE(),
    [sequenceNumber] INT NULL, 
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
    [isActive] BIT DEFAULT 1, 
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
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [startDate] DATE NULL,
    [exportDate] DATE NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_CBarn_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_CBarn_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_CBarn_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

-- Filtered Indexes to allow multiple NULLs but unique real IDs
CREATE UNIQUE NONCLUSTERED INDEX UIX_ChickenBarn_Flock ON ChickenBarn(flockID) WHERE flockID IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UIX_ChickenBarn_Chicken ON ChickenBarn(chickenLID) WHERE chickenLID IS NOT NULL;

CREATE TABLE [dbo].[FeedingRule] (
    [ruleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [times] INT NULL,
    [description] NVARCHAR(MAX) NULL,
    [note] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_Rule_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_Rule_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

CREATE UNIQUE NONCLUSTERED INDEX UIX_FeedingRule_Flock ON FeedingRule(flockID) WHERE flockID IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UIX_FeedingRule_Chicken ON FeedingRule(chickenLID) WHERE chickenLID IS NOT NULL;

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
    [status] NVARCHAR(50) DEFAULT 'pending', -- Updated to String
    [createdAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Request_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

CREATE TABLE [dbo].[Schedule] (
    [schedID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NULL,
    [taskID] INT NULL,
    [CBarnID] INT NULL,
    [description] NVARCHAR(MAX) NULL,
    [note] NVARCHAR(MAX) NULL,
    [priority] NVARCHAR(10) DEFAULT 'medium',               
    [status] NVARCHAR(20) DEFAULT 'pending',                
    [startDate] DATETIME NULL,
    [endDate] DATETIME NULL,
    [createdDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Sched_CBarn] FOREIGN KEY([CBarnID]) REFERENCES [dbo].[ChickenBarn] ([CBarnID]),
    CONSTRAINT [FK_Sched_Task] FOREIGN KEY([taskID]) REFERENCES [dbo].[Task] ([taskID]),
    CONSTRAINT [FK_Sched_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([status] IN ('pending', 'in progress', 'completed')),
    CONSTRAINT [CK_Schedule_Priority] CHECK ([priority] IN ('low', 'medium', 'high'))
);
GO

-- ======================================================
-- 6. SAMPLE DATA (3+ SAMPLES PER ENTITY)
-- ======================================================

-- Roles
INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');

-- Users
INSERT INTO [User] (roleID, email, password, fullName, username) VALUES 
(1, 'mgr@farm.com', 'p1', 'Alice Mgr', 'alice'),
(2, 'tech@farm.com', 'p2', 'Bob Tech', 'bob'),
(3, 'farmer@farm.com', 'p3', 'Charlie Farm', 'charlie');

-- Food
INSERT INTO [Food] (name, type, price, quantity) VALUES 
('Corn', 'Grain', 10.5, 1000), ('Soy', 'Protein', 20.0, 500), ('Vitamin', 'Supp', 5.0, 200);

-- Barns
INSERT INTO [Barn] (temperature, humidity, type, area) VALUES 
(25, 60, 'Type A', 500), (22, 55, 'Type B', 300), (24, 50, 'Type C', 400);

-- IoT
INSERT INTO [IoT_Device] (name, description) VALUES 
('T-01', 'Temp Sensor'), ('H-01', 'Humid Sensor'), ('W-01', 'Water Sensor');

-- Flocks
INSERT INTO [FlockChicken] (name, quantity, DoB, transferDate) VALUES 
('Flock Alpha', 100, '2026-01-01', '2026-03-01'), 
('Flock Beta', 200, '2026-02-01', '2026-03-05'),
('Flock Gamma', 150, '2026-02-15', '2026-03-10');

-- Tasks
INSERT INTO [Task] (title, startTime, endTime) VALUES 
('Feeding', '07:00', '08:00'), ('Cleaning', '09:00', '10:00'), ('Checking', '14:00', '15:00');

-- BarnIoT
INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate) VALUES (1,1,'2026-03-01'), (2,2,'2026-03-01'), (3,3,'2026-03-01');

-- IoT Data
INSERT INTO [Data_IoT] (barnID, deviceID, value, sequenceNumber) VALUES (1,1,25.5,1), (2,2,55.0,1), (3,3,10.0,1);

-- Large Chickens
INSERT INTO [LargeChicken] (flockID, name, weight) VALUES (1, 'Hero 1', 2.5), (2, 'Hero 2', 2.8), (3, 'Hero 3', 3.0);

-- Inventory
INSERT INTO [Inventory] (foodID, quantity, weightPerBag) VALUES (1,10,50), (2,5,25), (3,2,10);

-- Storage
INSERT INTO [FoodStorage] (foodID, barnID, food_weight) VALUES (1,1,500), (2,2,250), (3,3,100);

-- ChickenBarn Assignment (Rule: 1 entity per barn at a time)
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate) VALUES 
(1, 1, NULL, '2026-03-01'), -- Barn 1 has Flock 1
(2, NULL, 2, '2026-03-01'), -- Barn 2 has Large Chicken 2
(3, 3, NULL, '2026-03-01'); -- Barn 3 has Flock 3

-- Feeding Rules
INSERT INTO [FeedingRule] (flockID, chickenLID, times) VALUES 
(1, NULL, 3), (NULL, 2, 2), (3, NULL, 4);

-- Rule Details
INSERT INTO [FeedingRuleDetail] (ruleID, foodID, startDate, endDate) VALUES 
(1,1,'2026-03-01','2026-04-01'), (2,2,'2026-03-01','2026-04-01'), (3,3,'2026-03-01','2026-04-01');

-- Reports
INSERT INTO [Report] (userID, type, description) VALUES (1,'Weekly','OK'), (2,'Daily','OK'), (3,'Health','OK');

-- Requests
INSERT INTO [Request] (userID, type, description) VALUES (1,'Fix','Lights'), (2,'Add','Food'), (3,'Check','Sensor');

-- Schedules
INSERT INTO [Schedule] (userID, taskID, CBarnID, priority, status) VALUES 
(1,1,1,'high','pending'), (2,2,2,'medium','in progress'), (3,3,3,'low','completed');
GO