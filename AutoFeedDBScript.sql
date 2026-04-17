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
-- 2. TABLE DEFINITIONS
-- ======================================================

CREATE TABLE [dbo].[Role] (
    [roleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [description] NVARCHAR(255) NOT NULL
);

CREATE TABLE [dbo].[Food] (
    [foodID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [note] NVARCHAR(MAX) NULL
);

CREATE TABLE [dbo].[Barn] (
    [barnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [temperature] DECIMAL(5, 2) NOT NULL,
    [humidity] DECIMAL(5, 2) NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [area] DECIMAL(18, 2) NOT NULL,
    [waterAmount] INT NOT NULL,
    [foodAmount] DECIMAL(18, 2) NOT NULL,
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [CK_Barn_Type] CHECK ([type] IN ('Flock Barn', 'Flock Sick Barn', 'Large Chicken Barn'))
);

CREATE TABLE [dbo].[IoT_Device] (
    [deviceID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] BIT DEFAULT 1 
);

CREATE TABLE [dbo].[FlockChicken] (
    [flockID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [quantity] INT NOT NULL, 
    [weight] DECIMAL(18, 2) NOT NULL,
    [DoB] DATE NOT NULL,
    [transferDate] DATE NULL,
    [healthStatus] NVARCHAR(50) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1,
    CONSTRAINT [CK_Flock_Health] CHECK ([healthStatus] IN ('healthy', 'sick')),
    CONSTRAINT [CK_Flock_Qty] CHECK ([quantity] >= 0)
);

CREATE TABLE [dbo].[Task] (
    [taskID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [startTime] TIME NOT NULL, 
    [endTime] TIME NOT NULL,
    [status] BIT DEFAULT 1 
);

CREATE TABLE [dbo].[User] (
    [userID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [roleID] INT NOT NULL,
    [email] NVARCHAR(100) NOT NULL UNIQUE,
    [password] NVARCHAR(255) NOT NULL,
    [fullName] NVARCHAR(100) NOT NULL,
    [phone] NVARCHAR(20) NOT NULL,
    [username] NVARCHAR(50) NOT NULL UNIQUE,
    [avatarURL] NVARCHAR(MAX) NULL,
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
    [healthStatus] NVARCHAR(50) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [url] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1, 
    CONSTRAINT [FK_LargeChicken_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [CK_Chicken_Health] CHECK ([healthStatus] IN ('healthy', 'sick'))
);

CREATE TABLE [dbo].[Inventory] (
    [inventID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [foodID] INT NOT NULL,
    [quantity] INT NOT NULL,
    [weightPerBag] DECIMAL(18, 2) NOT NULL,
    [expiredDate] DATE NOT NULL,
    CONSTRAINT [FK_Inventory_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID])
);

CREATE TABLE [dbo].[ChickenBarn] (
    [CBarnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnID] INT NOT NULL,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [startDate] DATE NOT NULL,
    [exportDate] DATE NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] NVARCHAR(20) NOT NULL,
    CONSTRAINT [FK_CBarn_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID]),
    CONSTRAINT [FK_CBarn_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_CBarn_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID]),
    CONSTRAINT [CK_ChickenBarn_Status] CHECK ([status] IN ('active', 'export'))
);

CREATE TABLE [dbo].[FeedingRule] (
    [ruleID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [chickenLID] INT NULL,
    [flockID] INT NULL,
    [startDate] DATE NOT NULL,
    [endDate] DATE NOT NULL,
    [times] INT NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [status] NVARCHAR(20) DEFAULT 'active',
    CONSTRAINT [FK_Rule_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_Rule_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID]),
    CONSTRAINT [CK_FeedingRule_Status] CHECK ([status] IN ('active', 'disabled'))
);

CREATE TABLE [dbo].[FeedingRuleDetail] (
    [feedRuleDetailID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ruleID] INT NOT NULL,
    [foodID] INT NOT NULL,
    [feedHour] INT NOT NULL,
    [feedMinute] INT NOT NULL,
    [amount] DECIMAL(18, 2) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] BIT DEFAULT 1,
    CONSTRAINT [FK_Detail_Food] FOREIGN KEY([foodID]) REFERENCES [dbo].[Food] ([foodID]),
    CONSTRAINT [FK_Detail_Rule] FOREIGN KEY([ruleID]) REFERENCES [dbo].[FeedingRule] ([ruleID]),
    CONSTRAINT [CK_FeedTime_Hour] CHECK ([feedHour] BETWEEN 0 AND 23),
    CONSTRAINT [CK_FeedTime_Minute] CHECK ([feedMinute] BETWEEN 0 AND 59)
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
    CONSTRAINT [CK_Report_Status] CHECK ([status] IN ('pending', 'reviewed', 'rejected'))
);

CREATE TABLE [dbo].[Request] (
    [requestID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userID] INT NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [status] NVARCHAR(50) DEFAULT 'pending',   
    [url] NVARCHAR(MAX) NULL,                   
    [createdAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Request_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Request_Status] CHECK ([status] IN ('pending', 'approved', 'rejected'))
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
    [startDate] DATE NOT NULL,
    [endDate] DATE NULL,
    [createdDate] DATE DEFAULT CAST(GETDATE() AS DATE),
    CONSTRAINT [FK_Sched_CBarn] FOREIGN KEY([CBarnID]) REFERENCES [dbo].[ChickenBarn] ([CBarnID]),
    CONSTRAINT [FK_Sched_Task] FOREIGN KEY([taskID]) REFERENCES [dbo].[Task] ([taskID]),
    CONSTRAINT [FK_Sched_User] FOREIGN KEY([userID]) REFERENCES [dbo].[User] ([userID]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([status] IN ('pending', 'in progress', 'completed', 'disabled')),
    CONSTRAINT [CK_Schedule_Priority] CHECK ([priority] IN ('low', 'medium', 'high'))
);
GO

-- ======================================================
-- 3. DATA INSERTION
-- ======================================================

-- Phase 1: Base Tables
INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');

INSERT INTO [Food] (name, type, note) VALUES 
('Growth Pellet', 'Processed', 'Standard mix'), ('Organic Corn', 'Grain', 'Energy base'), 
('Soy Protein', 'Protein', 'Mass gain'), ('Mineral Block', 'Supplement', 'Bone health'), 
('Vitamin Mix', 'Additive', 'Immunity');

INSERT INTO [Barn] (temperature, humidity, type, area, waterAmount, foodAmount) VALUES 
-- 15 Large Chicken Barns (Barn 1-15)
(24, 55, 'Large Chicken Barn', 100, 80, 40.0), (24, 55, 'Large Chicken Barn', 100, 75, 40.0),
(23, 58, 'Large Chicken Barn', 100, 85, 40.0), (24, 55, 'Large Chicken Barn', 100, 70, 40.0),
(25, 54, 'Large Chicken Barn', 100, 90, 40.0), (24, 57, 'Large Chicken Barn', 100, 88, 40.0),
(24, 55, 'Large Chicken Barn', 100, 82, 40.0), (23, 59, 'Large Chicken Barn', 100, 79, 40.0),
(24, 56, 'Large Chicken Barn', 100, 81, 40.0), (24, 55, 'Large Chicken Barn', 100, 83, 40.0),
(25, 54, 'Large Chicken Barn', 100, 90, 40.0), (24, 57, 'Large Chicken Barn', 100, 88, 40.0),
(24, 55, 'Large Chicken Barn', 100, 82, 40.0), (23, 59, 'Large Chicken Barn', 100, 79, 40.0),
(24, 56, 'Large Chicken Barn', 100, 81, 40.0),
-- 4 Flock Barns (Barn 16-19)
(26, 60, 'Flock Barn', 300, 85, 150.0), (26, 61, 'Flock Barn', 300, 90, 150.0),
(25, 60, 'Flock Barn', 300, 75, 150.0), (27, 62, 'Flock Barn', 300, 80, 150.0),
-- 1 Sick Barn (Barn 20)
(22, 70, 'Flock Sick Barn', 150, 20, 75.0);

INSERT INTO [IoT_Device] (name, description, status) VALUES 
('DHT11', 'Temp/Humid Sensor', 1), ('HX711', 'Weight Sensor', 1), ('V2 sensor', 'Water Sensor', 1);

-- Phase 2: FlockChicken (5 Source Old, 5 Active New)
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus, isActive) VALUES 
-- Old Flocks (Age >= 3 Months, exported to Large Chicken stage)
('Red Rhode Batch A', 0, 0.45, '2025-12-01', '2026-04-01', 'healthy', 0),
('Leghorn Alpha L3', 0, 0.50, '2025-12-05', '2026-04-01', 'healthy', 0),
('Australorp Prime', 0, 0.48, '2025-12-10', '2026-04-02', 'healthy', 0),
('Sussex Legacy L1', 0, 0.52, '2025-12-15', '2026-04-02', 'healthy', 0),
('Plymouth Rock Mix', 0, 0.47, '2025-12-20', '2026-04-03', 'healthy', 0),
-- New Flocks (Age < 3 Months, currently being raised)
('Junior Reds 2026', 3, 0.35, '2026-02-15', NULL, 'healthy', 1),
('Baby Leghorn B2', 3, 0.38, '2026-02-20', NULL, 'healthy', 1),
('Sussex Isolation', 3, 0.30, '2026-03-01', NULL, 'sick', 1),
('Starter Australorp', 3, 0.32, '2026-03-05', NULL, 'healthy', 1),
('Heritage Junior L5', 3, 0.34, '2026-03-10', NULL, 'healthy', 1);

-- Phase 3: Dependent Level 1
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL) VALUES 
(1, 'manager@farm.com', 'p1', 'Alice Johnson', '0912345671', 'alice_mgr', 'https://tse1.mm.bing.net/th/id/OIP.p9bNjr0mX-spDgSi1S5hrgHaLH?rs=1&pid=ImgDetMain&o=7&rm=3'),
(2, 'tech@farm.com', 'p2', 'Bob Smith', '0912345672', 'bob_tech', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg'),
(3, 'farmer@farm.com', 'p3', 'Charlie Brown', '0912345673', 'charlie_f', 'https://tse1.mm.bing.net/th/id/OIP.s_OjVf-2_ScyG9UniAlm6wHaLH?w=1024&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3');

INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate) VALUES (1, 1, '2026-04-02'), (1, 2, '2026-04-02'), (1, 3, '2026-04-03');

-- LargeChicken (15 birds from Flocks 1-5)
INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
-- From Flock 1 (Red Rhode)
(1, 'Iron Talon', 3.5, 'healthy', 'https://tse2.mm.bing.net/th/id/OIP.uMHvXzIat56m-af5peVqGgHaJq?rs=1&pid=ImgDetMain&o=7&rm=3'),
(1, 'Ruby Crest', 3.4, 'healthy', 'https://anhdephd.vn/wp-content/uploads/2022/05/hinh-nen-ga-choi-dep.jpg'),
(1, 'Midnight Shadow', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-duoc-chai-long-ti-mi.jpg'),
-- From Flock 2 (Leghorn)
(2, 'Golden Spur', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-so-huu-bo-long-mau-do-nau.jpg'),
(2, 'Thunder Wing', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-co-cua.jpg'),
(2, 'Flame Feather', 3.8, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-xong-tran.jpg'),
-- From Flock 3 (Australorp)
(3, 'Stone Beak', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-chuyen-nghiep.jpg'),
(3, 'Silver Claw', 3.9, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-dung-manh.jpg'),
(3, 'Storm Eye', 3.4, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-hien-ngang.jpg'),
-- From Flock 4 (Sussex)
(4, 'Titan', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-khi-dung-duoi-anh-nang-gat.jpg'),
(4, 'Raptor', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-long-trang.jpg'),
(4, 'Phoenix', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-sieu-chien.jpg'),
-- From Flock 5 (Plymouth)
(5, 'Blizzard', 3.8, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-vung-hoang-da.jpg'),
(5, 'Ghost', 3.5, 'healthy', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-ga-troi-tren-nen-co-xanh_011040616.jpg'),
(5, 'Vanguard', 3.9, 'healthy', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-giong-ga-choi-noi-tieng-tai-viet-nam_011040771.jpg');

INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('Feeding','Standard breakfast','07:00','08:00'), ('Sanitation','Floor cleaning','09:00','10:30'), 
('Medical','Vet health check','13:00','14:30'), ('IOT Maint','Sensor calibration','16:00','17:00'), 
('Hydration','Check water pressure','10:00','11:00');

-- Phase 4: ChickenBarn Assignments
-- Exported Flocks (ID 1-5)
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, exportDate, status) VALUES 
(16, 1, NULL, '2025-12-15', '2026-04-01', 'export'),
(17, 2, NULL, '2025-12-20', '2026-04-01', 'export'),
(18, 3, NULL, '2025-12-25', '2026-04-02', 'export'),
(19, 4, NULL, '2025-12-30', '2026-04-02', 'export'),
(16, 5, NULL, '2026-01-05', '2026-04-03', 'export');

-- Active 15 LargeChickens (Barn 1-15)
INSERT INTO [ChickenBarn] (barnID, chickenLID, startDate, status) VALUES 
(1, 1, '2026-04-01', 'active'), (2, 2, '2026-04-01', 'active'), (3, 3, '2026-04-01', 'active'),
(4, 4, '2026-04-01', 'active'), (5, 5, '2026-04-01', 'active'), (6, 6, '2026-04-01', 'active'),
(7, 7, '2026-04-02', 'active'), (8, 8, '2026-04-02', 'active'), (9, 9, '2026-04-02', 'active'),
(10, 10, '2026-04-02', 'active'), (11, 11, '2026-04-02', 'active'), (12, 12, '2026-04-02', 'active'),
(13, 13, '2026-04-03', 'active'), (14, 14, '2026-04-03', 'active'), (15, 15, '2026-04-03', 'active');

-- Active 5 Young Flocks (Barn 16-19, Sick Barn 20)
INSERT INTO [ChickenBarn] (barnID, flockID, startDate, status) VALUES 
(16, 6, '2026-04-10', 'active'), (17, 7, '2026-04-12', 'active'),
(20, 8, '2026-04-14', 'active'), (18, 9, '2026-04-15', 'active'),
(19, 10, '2026-04-16', 'active');

-- Phase 5: Other operational data
INSERT INTO [Inventory] (foodID, quantity, weightPerBag, expiredDate) VALUES (1,50,20,'2027-01-01'), (2,50,20,'2027-01-01'), (3,50,20,'2027-01-01');
INSERT INTO [FeedingRule] (flockID, chickenLID, startDate, endDate, times, description) VALUES (6, NULL, '2026-04-10', '2026-05-10', 3, 'Young Reds growth'), (NULL, 1, '2026-04-01', '2026-05-01', 2, 'Iron Talon heavy diet');
INSERT INTO [FeedingRuleDetail] (ruleID, foodID, feedHour, feedMinute, amount, description) VALUES (1, 1, 7, 0, 5.0, 'Morning'), (2, 3, 8, 30, 10.0, 'Rich Breakfast');
INSERT INTO [Report] (userID, type, description, status, url, createDate) VALUES (3, 'Vet', 'Flock Sussex health audit', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-17');
INSERT INTO [Request] (userID, type, description, status, createdAt) VALUES (3, 'Food', 'Soy protein restock', 'approved', '2026-04-16');

INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate, createdDate) VALUES 
(3, 1, 6, 'Feed Iron Talon', 'Morning cycle', 'high', 'completed', '2026-04-17', '2026-04-17', '2026-04-15'),
(3, 2, 21, 'Sanitize Barn 16', 'Sector A', 'medium', 'pending', '2026-04-17', '2026-04-17', '2026-04-16');
GO

-- NOTE: [Data_IoT] is empty to receive real-time device data.