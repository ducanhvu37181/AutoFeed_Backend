-- ======================================================
-- 1. DATABASE RE-CREATION
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
    [description] NVARCHAR(255) NOT NULL
);

CREATE TABLE [dbo].[Food] (
    [foodID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [price] DECIMAL(18, 2) NOT NULL,
    [quantity] INT NOT NULL,
    [note] NVARCHAR(MAX) NULL
);

CREATE TABLE [dbo].[Barn] (
    [barnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [temperature] DECIMAL(5, 2) NOT NULL,
    [humidity] DECIMAL(5, 2) NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [area] DECIMAL(18, 2) NOT NULL,
    [createDate] DATETIME DEFAULT GETDATE()
);

CREATE TABLE [dbo].[IoT_Device] (
    [deviceID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] BIT DEFAULT 1 -- 1=Active
);

CREATE TABLE [dbo].[FlockChicken] (
    [flockID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [quantity] INT NOT NULL,
    [weight] DECIMAL(18, 2) NOT NULL,
    [DoB] DATE NOT NULL,                 -- Date of Birth
    [transferDate] DATE NOT NULL,        -- Ngày chuyển chuồng
    [healthStatus] NVARCHAR(100) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1         
);

CREATE TABLE [dbo].[Task] (
    [taskID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [startTime] TIME NOT NULL,           -- Time only
    [endTime] TIME NOT NULL,             -- Time only
    [status] BIT DEFAULT 1 
);

-- ======================================================
-- 3. DEPENDENT TABLES (Level 1)
-- ======================================================

CREATE TABLE [dbo].[User] (
    [userID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [roleID] INT NOT NULL,
    [email] NVARCHAR(100) NOT NULL UNIQUE,
    [password] NVARCHAR(255) NOT NULL,
    [fullName] NVARCHAR(100) NOT NULL,
    [phone] NVARCHAR(20) NOT NULL,
    [username] NVARCHAR(50) NOT NULL UNIQUE,
    [lastLogin] DATETIME NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_User_Role] FOREIGN KEY([roleID]) REFERENCES [dbo].[Role] ([roleID])
);

CREATE TABLE [dbo].[BarnIoT_Device] (
    [bDeviceID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [deviceID] INT NOT NULL,
    [installationDate] DATE NOT NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_BarnIoT_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_BarnIoT_Device] FOREIGN KEY([deviceID]) REFERENCES [dbo].[IoT_Device] ([deviceID])
);

CREATE TABLE [dbo].[Data_IoT] (
    [dataID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [deviceID] INT NOT NULL,
    [value] DECIMAL(18, 4) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [recordDate] DATETIME DEFAULT GETDATE(),
    [sequenceNumber] INT NOT NULL, -- "Lan thu may"
    CONSTRAINT [FK_DataIoT_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_DataIoT_Device] FOREIGN KEY([deviceID]) REFERENCES [dbo].[IoT_Device] ([deviceID])
);

CREATE TABLE [dbo].[LargeChicken] (
    [chickenLID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [flockID] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [weight] DECIMAL(18, 2) NOT NULL,
    [healthStatus] NVARCHAR(100) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1, 
    CONSTRAINT [FK_LargeChicken_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID])
);

CREATE TABLE [dbo].[Inventory] (
    [inventID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [foodID] INT NOT NULL,
    [quantity] INT NOT NULL,
    [weightPerBag] DECIMAL(18, 2) NOT NULL,
    [expiredDate] DATE NOT NULL,
    CONSTRAINT [FK_Inventory_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID])
);

CREATE TABLE [dbo].[FoodStorage] (
    [storageID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [foodID] INT NOT NULL,
    [barnID] INT NOT NULL,
    [food_weight] DECIMAL(18, 2) NOT NULL,
    [leftover_food] DECIMAL(18, 2) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_Storage_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_Storage_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID])
);

-- ======================================================
-- 4. RELATIONSHIP TABLES (Level 2)
-- ======================================================

CREATE TABLE [dbo].[ChickenBarn] (
    [CBarnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [startDate] DATE NOT NULL,
    [exportDate] DATE NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_CBarn_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_CBarn_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_CBarn_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

-- Filtered Indexes to handle unique constraints with multiple NULLs
CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Flock ON ChickenBarn(flockID) WHERE flockID IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Chicken ON ChickenBarn(chickenLID) WHERE chickenLID IS NOT NULL;

CREATE TABLE [dbo].[FeedingRule] (
    [ruleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [times] INT NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_Rule_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_Rule_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID])
);

CREATE UNIQUE NONCLUSTERED INDEX UIX_FRule_Flock ON FeedingRule(flockID) WHERE flockID IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UIX_FRule_Chicken ON FeedingRule(chickenLID) WHERE chickenLID IS NOT NULL;

-- ======================================================
-- 5. OPERATION & LOGGING TABLES (Level 3)
-- ======================================================

CREATE TABLE [dbo].[FeedingRuleDetail] (
    [feedRuleDetailID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ruleID] INT NOT NULL,
    [foodID] INT NOT NULL,
    [startDate] DATE NOT NULL,
    [endDate] DATE NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_Detail_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID]),
    CONSTRAINT [FK_Detail_Rule] FOREIGN KEY([ruleID]) REFERENCES [dbo].[FeedingRule] ([ruleID])
);

CREATE TABLE [dbo].[Report] (
    [reportID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] BIT DEFAULT 1,
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Report_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

CREATE TABLE [dbo].[Request] (
    [requestID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] NVARCHAR(50) DEFAULT 'pending', -- Default 'pending'
    [createdAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Request_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

-- UPDATED: Schedule using DATE only for date columns
CREATE TABLE [dbo].[Schedule] (
    [schedID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [taskID] INT NOT NULL,
    [CBarnID] INT NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [note] NVARCHAR(MAX) NOT NULL,
    [priority] NVARCHAR(10) DEFAULT 'medium',               
    [status] NVARCHAR(20) DEFAULT 'pending', -- 'pending', 'in progress', 'completed'
    [startDate] DATE NOT NULL,               -- Updated from DATETIME to DATE
    [endDate] DATE NULL,                     -- Updated from DATETIME to DATE
    [createdDate] DATE DEFAULT CAST(GETDATE() AS DATE), -- Updated to DATE
    CONSTRAINT [FK_Sched_CBarn] FOREIGN KEY([CBarnID]) REFERENCES [dbo].[ChickenBarn] ([CBarnID]),
    CONSTRAINT [FK_Sched_Task] FOREIGN KEY([taskID]) REFERENCES [dbo].[Task] ([taskID]),
    CONSTRAINT [FK_Sched_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([status] IN ('pending', 'in progress', 'completed')),
    CONSTRAINT [CK_Schedule_Priority] CHECK ([priority] IN ('low', 'medium', 'high'))
);
GO

-- ======================================================
-- 6. FULL SAMPLE DATA (3 ROWS PER ENTITY)
-- ======================================================

INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');

INSERT INTO [User] (roleID, email, password, fullName, phone, username) VALUES 
(1, 'mgr@farm.com', 'p1', 'Alice Johnson', '0123456789', 'alice_mgr'),
(2, 'tech@farm.com', 'p2', 'Bob Smith', '0987654321', 'bob_tech'),
(3, 'farmer@farm.com', 'p3', 'Charlie Brown', '0555666777', 'charlie_f');

INSERT INTO [Food] (name, type, price, quantity) VALUES 
('Corn Mix', 'Grain', 12.5, 1000), ('Soy Protein', 'Supplement', 25.0, 500), ('Vitamin B', 'Liquid', 15.0, 200);

INSERT INTO [Barn] (temperature, humidity, type, area) VALUES 
(25.5, 60.0, 'Nursery', 250), (22.0, 55.0, 'Layer', 600), (24.0, 50.0, 'Broiler', 400);

INSERT INTO [IoT_Device] (name, description, status) VALUES 
('T-Sensor-01', 'Air Temp', 1), ('H-Sensor-01', 'Air Humid', 1), ('W-Level-01', 'Water tank', 1);

INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus) VALUES 
('Flock Alpha', 200, 0.5, '2026-01-01', '2026-03-01', 'Healthy'),
('Flock Beta', 300, 0.4, '2026-02-01', '2026-03-05', 'Stable'),
('Flock Gamma', 150, 0.6, '2026-02-15', '2026-03-10', 'Healthy');

INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('Feeding A', 'Distribute grain', '07:00', '08:00'), 
('Sensor Check', 'Calibrate tech', '10:00', '11:00'), 
('Clean Barn', 'Sanitize floor', '13:00', '15:00');

INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate) VALUES (1,1,'2026-03-01'), (2,2,'2026-03-02'), (3,3,'2026-03-03');

INSERT INTO [Data_IoT] (barnID, deviceID, value, description, sequenceNumber) VALUES 
(1,1,25.4,'Routine',1), (2,2,55.1,'Routine',1), (3,3,88.2,'Routine',1);

INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus) VALUES 
(1, 'Hen-01', 2.5, 'Active'), (2, 'Hen-02', 2.3, 'Good'), (3, 'Hen-03', 2.8, 'Healthy');

INSERT INTO [Inventory] (foodID, quantity, weightPerBag, expiredDate) VALUES 
(1, 20, 50.0, '2027-01-01'), (2, 10, 25.0, '2026-12-01'), (3, 5, 10.0, '2026-11-01');

INSERT INTO [FoodStorage] (foodID, barnID, food_weight, leftover_food) VALUES 
(1, 1, 100.0, 10.0), (2, 2, 250.0, 20.0), (3, 3, 150.0, 5.0);

-- Assignment Rule: 1 entity per barn
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate) VALUES 
(1, 1, NULL, '2026-03-01'), 
(2, NULL, 1, '2026-03-01'), 
(3, 2, NULL, '2026-03-01');

INSERT INTO [FeedingRule] (flockID, chickenLID, times, description) VALUES 
(1, NULL, 3, 'Flock Growth'), (NULL, 1, 2, 'Individual Boost'), (2, NULL, 3, 'Standard');

INSERT INTO [FeedingRuleDetail] (ruleID, foodID, startDate, endDate, description) VALUES 
(1,1,'2026-03-01','2026-04-01','Morning Grain'), 
(2,2,'2026-03-01','2026-04-01','Protein Boost'), 
(3,3,'2026-03-01','2026-04-01','Vitamins');

INSERT INTO [Report] (userID, type, description) VALUES 
(1, 'Admin', 'Weekly Review'), (2, 'Tech', 'Sensor Repair'), (3, 'Field', 'Barn cleaning complete');

INSERT INTO [Request] (userID, type, description) VALUES 
(2, 'Repair', 'Fix sensor 01'), (3, 'Supply', 'Need more Grain'), (1, 'Access', 'Add new farmer');

-- UPDATED Schedule Sample: Using only DATE format
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate) VALUES 
(3, 1, 1, 'Morning feed', 'Check water too', 'high', 'pending', '2026-03-27'),
(2, 2, 2, 'Tech check', 'Calibrated', 'medium', 'in progress', '2026-03-27'),
(3, 3, 3, 'Afternoon clean', 'Used sanitizer', 'low', 'completed', '2026-03-26');
GO