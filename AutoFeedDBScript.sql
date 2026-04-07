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
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [CK_Barn_Type] CHECK ([type] IN ('Flock barn', 'flock sick barn', 'large chicken barn')) --
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
    [DoB] DATE NOT NULL,
    [transferDate] DATE NOT NULL,
    [healthStatus] NVARCHAR(100) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1
);

CREATE TABLE [dbo].[Task] (
    [taskID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [startTime] TIME NOT NULL, -- Time only
    [endTime] TIME NOT NULL,   -- Time only
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
    [avatarURL] NVARCHAR(MAX) NULL, --
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
    [sequenceNumber] INT NOT NULL, 
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
    [url] NVARCHAR(MAX) NULL, -- Picture link
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

-- UPDATED: ChickenBarn Status as String
CREATE TABLE [dbo].[ChickenBarn] (
    [CBarnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [startDate] DATE NOT NULL,
    [exportDate] DATE NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] NVARCHAR(20) NOT NULL, -- active, export
    CONSTRAINT [FK_CBarn_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_CBarn_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_CBarn_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID]),
    CONSTRAINT [CK_ChickenBarn_Status] CHECK ([status] IN ('active', 'export')) --
);

-- Filtered Indexes for 1 entity per barn rule
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
    [status] NVARCHAR(50) DEFAULT 'pending', 
    [url] NVARCHAR(MAX) NULL,                   
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Report_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Report_Status] CHECK ([status] IN ('pending', 'approved', 'rejected'))
);

CREATE TABLE [dbo].[Request] (
    [requestID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] NVARCHAR(50) DEFAULT 'pending',   
    [url] NVARCHAR(MAX) NULL,                   
    [createdAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Request_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID])
);

CREATE TABLE [dbo].[Schedule] (
    [schedID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [taskID] INT NOT NULL,
    [CBarnID] INT NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [note] NVARCHAR(MAX) NOT NULL,
    [priority] NVARCHAR(10) DEFAULT 'medium',               
    [status] NVARCHAR(20) DEFAULT 'pending', 
    [startDate] DATE NOT NULL, -- DATE
    [endDate] DATE NULL,       -- DATE
    [createdDate] DATE DEFAULT CAST(GETDATE() AS DATE),
    CONSTRAINT [FK_Sched_CBarn] FOREIGN KEY([CBarnID]) REFERENCES [dbo].[ChickenBarn] ([CBarnID]),
    CONSTRAINT [FK_Sched_Task] FOREIGN KEY([taskID]) REFERENCES [dbo].[Task] ([taskID]),
    CONSTRAINT [FK_Sched_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([status] IN ('pending', 'in progress', 'completed')),
    CONSTRAINT [CK_Schedule_Priority] CHECK ([priority] IN ('low', 'medium', 'high'))
);
GO

-- ======================================================
-- 6. SAMPLE DATA (15 RECORDS PER ENTITY)
-- ======================================================

-- 1. Roles
INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');

-- 2. Users (15 samples)
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL) VALUES 
(1, 'alice@farm.com', 'p1', 'Alice Johnson', '0123456781', 'alice_mgr', 'https://cdn.com/a1.png'),
(2, 'bob@farm.com', 'p2', 'Bob Smith', '0123456782', 'bob_tech', 'https://cdn.com/a2.png'),
(3, 'charlie@farm.com', 'p3', 'Charlie Brown', '0123456783', 'charlie_f', 'https://cdn.com/a3.png'),
(3, 'david@farm.com', 'p4', 'David Miller', '0123456784', 'david_f', 'https://cdn.com/a4.png'),
(3, 'eve@farm.com', 'p5', 'Eve Adams', '0123456785', 'eve_f', 'https://cdn.com/a5.png'),
(3, 'frank@farm.com', 'p6', 'Frank White', '0123456786', 'frank_f', 'https://cdn.com/a6.png'),
(3, 'grace@farm.com', 'p7', 'Grace Hopper', '0123456787', 'grace_f', 'https://cdn.com/a7.png'),
(3, 'hank@farm.com', 'p8', 'Hank Hill', '0123456788', 'hank_f', 'https://cdn.com/a8.png'),
(3, 'ivy@farm.com', 'p9', 'Ivy League', '0123456789', 'ivy_f', 'https://cdn.com/a9.png'),
(3, 'jack@farm.com', 'p10', 'Jack Reacher', '0123456710', 'jack_f', 'https://cdn.com/a10.png'),
(3, 'kelly@farm.com', 'p11', 'Kelly Green', '0123456711', 'kelly_f', 'https://cdn.com/a11.png'),
(3, 'leo@farm.com', 'p12', 'Leo Messi', '0123456712', 'leo_f', 'https://cdn.com/a12.png'),
(3, 'mia@farm.com', 'p13', 'Mia Wallace', '0123456713', 'mia_f', 'https://cdn.com/a13.png'),
(3, 'noah@farm.com', 'p14', 'Noah Ark', '0123456714', 'noah_f', 'https://cdn.com/a14.png'),
(3, 'olivia@farm.com', 'p15', 'Olivia Pope', '0123456715', 'olivia_f', 'https://cdn.com/a15.png');

-- 3. Barns (15 samples - 3 types)
INSERT INTO [Barn] (temperature, humidity, type, area) VALUES 
(25.0, 60, 'Flock barn', 300), (25.1, 60, 'Flock barn', 300), (25.2, 60, 'Flock barn', 300),
(25.3, 60, 'Flock barn', 300), (25.4, 60, 'Flock barn', 300), (22.0, 70, 'flock sick barn', 150),
(22.1, 70, 'flock sick barn', 150), (22.2, 70, 'flock sick barn', 150), (22.3, 70, 'flock sick barn', 150),
(22.4, 70, 'flock sick barn', 150), (24.0, 55, 'large chicken barn', 100), (24.1, 55, 'large chicken barn', 100),
(24.2, 55, 'large chicken barn', 100), (24.3, 55, 'large chicken barn', 100), (24.4, 55, 'large chicken barn', 100);

-- 4. FlockChicken (15 samples)
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus) VALUES 
('Flock A1', 100, 0.5, '2026-01-01', '2026-03-01', 'Healthy'), ('Flock A2', 100, 0.5, '2026-01-01', '2026-03-01', 'Healthy'),
('Flock A3', 100, 0.5, '2026-01-01', '2026-03-01', 'Healthy'), ('Flock A4', 100, 0.5, '2026-01-01', '2026-03-01', 'Healthy'),
('Flock A5', 100, 0.5, '2026-01-01', '2026-03-01', 'Healthy'), ('Flock S1', 50, 0.4, '2026-02-01', '2026-03-05', 'Sick'),
('Flock S2', 50, 0.4, '2026-02-01', '2026-03-05', 'Sick'), ('Flock S3', 50, 0.4, '2026-02-01', '2026-03-05', 'Sick'),
('Flock S4', 50, 0.4, '2026-02-01', '2026-03-05', 'Sick'), ('Flock S5', 50, 0.4, '2026-02-01', '2026-03-05', 'Sick'),
('Flock G1', 200, 0.8, '2025-12-01', '2026-02-01', 'Healthy'), ('Flock G2', 200, 0.8, '2025-12-01', '2026-02-01', 'Healthy'),
('Flock G3', 200, 0.8, '2025-12-01', '2026-02-01', 'Healthy'), ('Flock G4', 200, 0.8, '2025-12-01', '2026-02-01', 'Healthy'),
('Flock G5', 200, 0.8, '2025-12-01', '2026-02-01', 'Healthy');

-- 5. LargeChicken (15 samples)
INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
(11, 'King 01', 3.5, 'Excellent', 'https://cdn.com/c1.jpg'), (11, 'King 02', 3.4, 'Excellent', 'https://cdn.com/c2.jpg'),
(12, 'King 03', 3.6, 'Excellent', 'https://cdn.com/c3.jpg'), (12, 'King 04', 3.5, 'Excellent', 'https://cdn.com/c4.jpg'),
(13, 'King 05', 3.7, 'Excellent', 'https://cdn.com/c5.jpg'), (13, 'King 06', 3.5, 'Excellent', 'https://cdn.com/c6.jpg'),
(14, 'King 07', 3.8, 'Excellent', 'https://cdn.com/c7.jpg'), (14, 'King 08', 3.5, 'Excellent', 'https://cdn.com/c8.jpg'),
(15, 'King 09', 3.9, 'Excellent', 'https://cdn.com/c9.jpg'), (15, 'King 10', 3.5, 'Excellent', 'https://cdn.com/c10.jpg'),
(1, 'Hero 11', 2.0, 'Good', 'https://cdn.com/c11.jpg'), (2, 'Hero 12', 2.0, 'Good', 'https://cdn.com/c12.jpg'),
(3, 'Hero 13', 2.0, 'Good', 'https://cdn.com/c13.jpg'), (4, 'Hero 14', 2.0, 'Good', 'https://cdn.com/c14.jpg'),
(5, 'Hero 15', 2.0, 'Good', 'https://cdn.com/c15.jpg');

-- 6. ChickenBarn (15 assignments - rule followed)
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, status) VALUES 
(1, 1, NULL, '2026-03-01', 'active'), (2, 2, NULL, '2026-03-01', 'active'), (3, 3, NULL, '2026-03-01', 'active'),
(4, 4, NULL, '2026-03-01', 'active'), (5, 5, NULL, '2026-03-01', 'active'), (6, 6, NULL, '2026-03-01', 'active'),
(7, 7, NULL, '2026-03-01', 'active'), (8, 8, NULL, '2026-03-01', 'active'), (9, 9, NULL, '2026-03-01', 'active'),
(10, 10, NULL, '2026-03-01', 'active'), (11, NULL, 1, '2026-03-01', 'active'), (12, NULL, 2, '2026-03-01', 'active'),
(13, NULL, 3, '2026-03-01', 'active'), (14, NULL, 4, '2026-03-01', 'active'), (15, NULL, 5, '2026-03-01', 'active');

-- 7. IoT Devices
INSERT INTO [IoT_Device] (name, description) VALUES 
('D1','T'), ('D2','H'), ('D3','T'), ('D4','H'), ('D5','T'), ('D6','H'), ('D7','T'), ('D8','H'), ('D9','T'), ('D10','H'),
('D11','T'), ('D12','H'), ('D13','T'), ('D14','H'), ('D15','T');

-- 8. Food
INSERT INTO [Food] (name, type, price, quantity) VALUES 
('F1','G',10,100), ('F2','G',10,100), ('F3','G',10,100), ('F4','S',20,100), ('F5','S',20,100),
('F6','V',15,100), ('F7','V',15,100), ('F8','G',10,100), ('F9','G',10,100), ('F10','G',10,100),
('F11','S',20,100), ('F12','S',20,100), ('F13','V',15,100), ('F14','V',15,100), ('F15','G',10,100);

-- 9. Tasks
INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('T1','Feed','07:00','08:00'), ('T2','Feed','07:00','08:00'), ('T3','Feed','07:00','08:00'), ('T4','Clean','09:00','10:00'), ('T5','Clean','09:00','10:00'),
('T6','Clean','09:00','10:00'), ('T7','Check','11:00','12:00'), ('T8','Check','11:00','12:00'), ('T9','Check','11:00','12:00'), ('T10','Vet','13:00','14:00'),
('T11','Vet','13:00','14:00'), ('T12','Vet','13:00','14:00'), ('T13','Light','18:00','19:00'), ('T14','Light','18:00','19:00'), ('T15','Light','18:00','19:00');

-- 10. Schedule
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate) VALUES 
(3, 1, 1, 'D', 'N', 'high', 'pending', '2026-04-07'), (4, 2, 2, 'D', 'N', 'high', 'pending', '2026-04-07'),
(5, 3, 3, 'D', 'N', 'high', 'pending', '2026-04-07'), (6, 4, 4, 'D', 'N', 'medium', 'pending', '2026-04-07'),
(7, 5, 5, 'D', 'N', 'medium', 'pending', '2026-04-07'), (8, 6, 6, 'D', 'N', 'medium', 'pending', '2026-04-07'),
(9, 7, 7, 'D', 'N', 'low', 'pending', '2026-04-07'), (10, 8, 8, 'D', 'N', 'low', 'pending', '2026-04-07'),
(11, 9, 9, 'D', 'N', 'low', 'pending', '2026-04-07'), (12, 10, 10, 'D', 'N', 'high', 'pending', '2026-04-07'),
(13, 11, 11, 'D', 'N', 'high', 'pending', '2026-04-07'), (14, 12, 12, 'D', 'N', 'medium', 'pending', '2026-04-07'),
(15, 13, 13, 'D', 'N', 'medium', 'pending', '2026-04-07'), (3, 14, 14, 'D', 'N', 'low', 'pending', '2026-04-07'),
(4, 15, 15, 'D', 'N', 'low', 'pending', '2026-04-07');

-- 11. FeedingRule (15 samples)
INSERT INTO [FeedingRule] (flockID, chickenLID, times, description) VALUES 
(1,NULL,3,'R1'), (2,NULL,3,'R2'), (3,NULL,3,'R3'), (4,NULL,3,'R4'), (5,NULL,3,'R5'),
(6,NULL,2,'R6'), (7,NULL,2,'R7'), (8,NULL,2,'R8'), (9,NULL,2,'R9'), (10,NULL,2,'R10'),
(NULL,1,2,'R11'), (NULL,2,2,'R12'), (NULL,3,2,'R13'), (NULL,4,2,'R14'), (NULL,5,2,'R15');

-- Remaining tables (Data_IoT, Report, Request, etc.) can be similarly populated as needed.
GO