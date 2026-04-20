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
    CONSTRAINT [CK_Barn_Type] CHECK ([type] IN ('Flock Barn', 'Large Chicken Barn'))
);

CREATE TABLE [dbo].[BarnImage] (
    [imageBarnId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [barnId] INT NOT NULL,
    [url] NVARCHAR(MAX) NOT NULL,
    [description] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_BarnImage_Barn] FOREIGN KEY([barnId]) REFERENCES [dbo].[Barn] ([barnID])
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

CREATE TABLE [dbo].[ErrorIoT] (
    [errorID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [deviceID] INT NOT NULL,
    [barnID] INT NOT NULL,
    [errorMessage] NVARCHAR(MAX) NOT NULL,
    [severity] NVARCHAR(20) NOT NULL, 
    [recordDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_ErrorIoT_Device] FOREIGN KEY([deviceID]) REFERENCES [dbo].[IoT_Device] ([deviceID]),
    CONSTRAINT [FK_ErrorIoT_Barn] FOREIGN KEY([barnID]) REFERENCES [dbo].[Barn] ([barnID])
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
    [importDate] DATE NOT NULL,
    [expiredDate] DATE NOT NULL,
    [DaysExpired] AS IIF(DATEDIFF(DAY, GETDATE(), [expiredDate]) >= 0, 0, DATEDIFF(DAY, GETDATE(), [expiredDate])),
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

CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Flock ON ChickenBarn(flockID) WHERE flockID IS NOT NULL AND status = 'active';
CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Chicken ON ChickenBarn(chickenLID) WHERE chickenLID IS NOT NULL AND status = 'active';

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
    CONSTRAINT [CK_Report_Status] CHECK ([status] IN ('pending', 'reviewed')),
    CONSTRAINT [CK_Report_Type] CHECK ([type] IN ('Feed', 'Maintenance', 'Medical', 'Inventory', 'Schedule', 'Flock', 'Others'))
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
    CONSTRAINT [CK_Request_Status] CHECK ([status] IN ('pending', 'approved', 'rejected')),
    CONSTRAINT [CK_Request_Type] CHECK ([type] IN ('Feed', 'Maintenance', 'Medical', 'Inventory', 'Schedule', 'Flock', 'Others'))
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

-- 3.1 Roles & Foods
INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');
INSERT INTO [Food] (name, type, note) VALUES 
('Organic Corn', 'Grain', 'High fiber'), ('Soy Protein', 'Protein', 'Mass gain'), 
('Growth Pellet', 'Processed', 'Standard'), ('Mineral Block', 'Supplement', 'Bone health'), 
('Vitamin Mix', 'Additive', 'Immunity');

-- 3.2 Barns (30 Barns)
INSERT INTO [Barn] (temperature, humidity, type, area, waterAmount, foodAmount) VALUES 
(24.5, 55.0, 'Large Chicken Barn', 100, 80, 40.0), (24.0, 56.0, 'Large Chicken Barn', 100, 75, 40.0), (23.5, 58.0, 'Large Chicken Barn', 100, 85, 40.0), (24.2, 55.0, 'Large Chicken Barn', 100, 70, 40.0), (25.1, 54.0, 'Large Chicken Barn', 100, 90, 40.0), 
(24.8, 57.0, 'Large Chicken Barn', 100, 88, 40.0), (24.0, 55.0, 'Large Chicken Barn', 100, 82, 40.0), (23.9, 59.0, 'Large Chicken Barn', 100, 79, 40.0), (24.3, 56.0, 'Large Chicken Barn', 100, 81, 40.0), (24.5, 55.0, 'Large Chicken Barn', 100, 83, 40.0),
(25.0, 54.0, 'Large Chicken Barn', 100, 90, 40.0), (24.2, 57.0, 'Large Chicken Barn', 100, 88, 40.0), (24.1, 55.0, 'Large Chicken Barn', 100, 82, 40.0), (23.8, 59.0, 'Large Chicken Barn', 100, 79, 40.0), (24.4, 56.0, 'Large Chicken Barn', 100, 81, 40.0),
(24.0, 55.0, 'Large Chicken Barn', 100, 80, 40.0), (24.0, 55.0, 'Large Chicken Barn', 100, 80, 40.0), (24.0, 55.0, 'Large Chicken Barn', 100, 80, 40.0), (24.0, 55.0, 'Large Chicken Barn', 100, 80, 40.0), (24.0, 55.0, 'Large Chicken Barn', 100, 80, 40.0),
(26.5, 60.0, 'Flock Barn', 300, 85, 150.0), (26.2, 61.0, 'Flock Barn', 300, 90, 150.0), (25.8, 60.0, 'Flock Barn', 300, 75, 150.0), (27.0, 62.0, 'Flock Barn', 300, 80, 150.0), (26.4, 61.0, 'Flock Barn', 300, 82, 150.0),
(26.0, 60.0, 'Flock Barn', 300, 85, 150.0), (26.0, 60.0, 'Flock Barn', 300, 85, 150.0), (26.0, 60.0, 'Flock Barn', 300, 85, 150.0), (26.0, 60.0, 'Flock Barn', 300, 85, 150.0), (26.0, 60.0, 'Flock Barn', 300, 85, 150.0);

-- 3.3 Users
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL) VALUES 
(1, 'manager@farm.com', 'p1', 'Alice Manager', '0912345671', 'alice_mgr', 'https://tse1.mm.bing.net/th/id/OIP.p9bNjr0mX-spDgSi1S5hrgHaLH?rs=1&pid=ImgDetMain&o=7&rm=3'),
(2, 'tech@farm.com', 'p2', 'Bob Smith', '0912345672', 'bob_tech', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg'),
(3, 'farmer1@farm.com', 'p3', 'Charlie Brown', '0912345673', 'charlie_f', 'https://tse1.mm.bing.net/th/id/OIP.s_OjVf-2_ScyG9UniAlm6wHaLH?w=1024&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3'),
(3, 'farmer2@farm.com', 'p4', 'Frank Green', '0912345674', 'frank_f', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg'),
(3, 'farmer3@farm.com', 'p5', 'Visger Green', '0912345674', 'vis_gr', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg');


-- 3.4 Flock & Large Chicken
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus, isActive) VALUES 
('Flock_1', 0, 0.45, '2025-12-01', '2026-04-01', 'healthy', 0), ('Flock_2', 0, 0.50, '2025-12-05', '2026-04-01', 'healthy', 0), ('Flock_3', 0, 0.48, '2025-12-10', '2026-04-02', 'healthy', 0), ('Flock_4', 0, 0.52, '2025-12-15', '2026-04-02', 'healthy', 0), ('Flock_5', 0, 0.47, '2025-12-20', '2026-04-03', 'healthy', 0),
('Flock_6', 3, 0.35, '2026-01-10', NULL, 'healthy', 1), ('Flock_7', 3, 0.38, '2026-02-15', NULL, 'healthy', 1), ('Flock_8', 3, 0.30, '2026-03-01', NULL, 'sick', 1), ('Flock_9', 3, 0.32, '2026-01-05', NULL, 'healthy', 1), ('Flock_10', 3, 0.34, '2026-03-15', NULL, 'healthy', 1);

INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
(1, 'LChickenFromFlock1_Barn1', 3.5, 'healthy', 'https://tse2.mm.bing.net/th/id/OIP.uMHvXzIat56m-af5peVqGgHaJq?rs=1&pid=ImgDetMain&o=7&rm=3'), (1, 'LChickenFromFlock1_Barn2', 3.4, 'healthy', 'https://anhdephd.vn/wp-content/uploads/2022/05/hinh-nen-ga-choi-dep.jpg'), (1, 'LChickenFromFlock1_Barn3', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-duoc-chai-long-ti-mi.jpg'),
(2, 'LChickenFromFlock2_Barn4', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-so-huu-bo-long-mau-do-nau.jpg'), (2, 'LChickenFromFlock2_Barn5', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-co-cua.jpg'), (2, 'LChickenFromFlock2_Barn6', 3.8, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-xong-tran.jpg'),
(3, 'LChickenFromFlock3_Barn7', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-chuyen-nghiep.jpg'), (3, 'LChickenFromFlock3_Barn8', 3.9, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-dung-manh.jpg'), (3, 'LChickenFromFlock3_Barn9', 3.4, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-hien-ngang.jpg'),
(4, 'LChickenFromFlock4_Barn10', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-khi-dung-duoi-anh-nang-gat.jpg'), (4, 'LChickenFromFlock4_Barn11', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-long-trang.jpg'), (4, 'LChickenFromFlock4_Barn12', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-sieu-chien.jpg'),
(5, 'LChickenFromFlock5_Barn13', 3.8, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-vung-hoang-da.jpg'), (5, 'LChickenFromFlock5_Barn14', 3.5, 'healthy', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-ga-troi-tren-nen-co-xanh_011040616.jpg'), (5, 'LChickenFromFlock5_Barn15', 3.9, 'healthy', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-giong-ga-choi-noi-tieng-tai-viet-nam_011040771.jpg');

-- 3.5 ChickenBarn
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, exportDate, status) VALUES 
(21, 1, NULL, '2025-12-15', '2026-04-01', 'export'), (22, 2, NULL, '2025-12-20', '2026-04-01', 'export'), (23, 3, NULL, '2025-12-25', '2026-04-02', 'export'), (24, 4, NULL, '2025-12-30', '2026-04-02', 'export'), (25, 5, NULL, '2026-01-05', '2026-04-03', 'export');
INSERT INTO [ChickenBarn] (barnID, chickenLID, startDate, status) VALUES 
(1, 1, '2026-04-01', 'active'), (2, 2, '2026-04-01', 'active'), (3, 3, '2026-04-01', 'active'), (4, 4, '2026-04-01', 'active'), (5, 5, '2026-04-01', 'active'), (6, 6, '2026-04-01', 'active'), (7, 7, '2026-04-02', 'active'), (8, 8, '2026-04-02', 'active'), (9, 9, '2026-04-02', 'active'), (10, 10, '2026-04-02', 'active'), (11, 11, '2026-04-02', 'active'), (12, 12, '2026-04-02', 'active'), (13, 13, '2026-04-03', 'active'), (14, 14, '2026-04-03', 'active'), (15, 15, '2026-04-03', 'active');
INSERT INTO [ChickenBarn] (barnID, flockID, startDate, status) VALUES 
(21, 6, '2026-04-10', 'active'), (22, 7, '2026-04-12', 'active'), (23, 8, '2026-04-14', 'active'), (24, 9, '2026-04-15', 'active'), (25, 10, '2026-04-16', 'active');

-- 3.6 IoT Device Logic: 160 DEVICES (8 TYPES x 20 ACTIVE BARNS)
DECLARE @BarnLoop INT = 1;
WHILE @BarnLoop <= 30
BEGIN
    IF (@BarnLoop BETWEEN 1 AND 15) OR (@BarnLoop BETWEEN 21 AND 25)
    BEGIN
        -- Insert 8 types of devices
        INSERT INTO [IoT_Device] (name, description, status) VALUES 
        ('DHT11', 'Environmental Sensor', 1), ('HX711', 'Weight Converter', 1), ('V2 sensor', 'Water Level Probe', 1),
        ('Loadcell', 'Scale Sensor', 1), ('Servo', 'Gate Motor', 1), ('Green LED', 'Status LED', 1),
        ('Red LED', 'Alert LED', 1), ('Water Pump', 'Pump Motor', 1);

        -- Assign to the current barn (Assumes consecutive deviceIDs)
        INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate)
        SELECT TOP 8 @BarnLoop, deviceID, '2026-04-01'
        FROM [IoT_Device] ORDER BY deviceID DESC;
    END
    SET @BarnLoop = @BarnLoop + 1;
END
GO

-- 3.7 Manual Tasks (NO Feeding)
INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('Sanitation','Floor sweeping','08:00','09:30'), ('Medical Audit','Health check','10:00','11:30'), ('Inventory Count','Count bags','14:00','15:30'), ('Maintenance','Check wires','16:00','17:00'), ('Pressure Check','Test pipes','09:30','10:30');

-- 3.8 FeedingRules & Details (3 samples, each with Breakfast/Lunch/Dinner)
INSERT INTO [FeedingRule] (flockID, chickenLID, startDate, endDate, times, description) VALUES 
(6, NULL, '2026-04-10', '2026-05-10', 3, 'Flock 6 Diet'), (NULL, 1, '2026-04-01', '2026-05-01', 3, 'LChicken 1 Diet'), (7, NULL, '2026-04-12', '2026-05-12', 3, 'Flock 7 Diet');

INSERT INTO [FeedingRuleDetail] (ruleID, foodID, feedHour, feedMinute, amount, description) VALUES 
-- Rule 1
(1, 3, 7, 0, 5.0, 'Morning - Pellet'), (1, 3, 12, 0, 4.0, 'Lunch - Pellet'), (1, 5, 18, 0, 1.0, 'Dinner - Vitamin Mix'),
-- Rule 2
(2, 2, 8, 30, 10.0, 'Morning - Soy'), (2, 2, 13, 0, 8.0, 'Lunch - Soy'), (2, 4, 19, 0, 2.0, 'Dinner - Mineral'),
-- Rule 3
(3, 1, 7, 30, 7.0, 'Morning - Corn'), (3, 1, 12, 30, 6.0, 'Lunch - Corn'), (3, 3, 18, 30, 5.0, 'Dinner - Pellet');

-- 3.9 Reports & Requests (Including 'Flock' type)
INSERT INTO [Report] (userID, type, description, status, url, createDate) VALUES 
(3, 'Medical', 'Flock_8 stable', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-17 08:30:00'),
(4, 'Inventory', 'Weekly audit sector A', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-18 10:15:00'),
(3, 'Flock', 'Flock_6 active behavior', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-19 14:00:00'),
(2, 'Maintenance', 'Sensor check Barn 1', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-20 16:45:00'),
(4, 'Feed', 'Corn supply running low in Barn 3', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-14 09:15:00'),
(3, 'Schedule', 'Weekly schedule conflict detected', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-15 11:00:00'),
(2, 'Others', 'General farm observation notes', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-16 13:30:00'),
(5, 'Medical', 'Flock_9 vaccination completed', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-13 08:45:00'),
(4, 'Inventory', 'Monthly stock summary', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-20 10:30:00'),
(3, 'Maintenance', 'Water pump Barn 5 replaced', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-12 15:00:00'),
(5, 'Flock', 'Flock_10 weight gain on track', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-20 14:15:00');

INSERT INTO [Request] (userID, type, description, status, createdAt) VALUES 
(3, 'Feed', 'Order Soy', 'approved', '2026-04-15 09:00:00'), (3, 'Flock', 'Split Flock_6', 'pending', '2026-04-18 13:30:00'), (2, 'Maintenance', 'Sensor fix', 'pending', '2026-04-19 15:45:00'),
(4, 'Medical', 'Request vet visit for Flock_8', 'approved', '2026-04-13 08:30:00'), (5, 'Inventory', 'Restock Vitamin Mix urgently', 'approved', '2026-04-14 10:00:00'),
(3, 'Schedule', 'Reschedule sanitation task', 'rejected', '2026-04-16 11:30:00'), (2, 'Others', 'Request new staff ID badges', 'pending', '2026-04-17 14:45:00'),
(4, 'Feed', 'Increase corn supply for Barn 21', 'approved', '2026-04-12 09:30:00'), (5, 'Maintenance', 'Replace LED in Barn 10', 'pending', '2026-04-20 16:00:00'),
(3, 'Medical', 'Antibiotics for LChicken 3', 'rejected', '2026-04-19 08:00:00'), (4, 'Flock', 'Merge Flock_9 and Flock_10', 'pending', '2026-04-20 11:15:00');

-- 3.10 Schedule (15 entries - Today is 2026-04-20)
-- Past (completed)
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate, createdDate) VALUES 
(3, 1, 6, 'Sweep area', 'D', 'high', 'completed', '2026-04-10', '2026-04-10', '2026-04-08'), (4, 2, 21, 'Vaccination', 'D', 'medium', 'completed', '2026-04-11', '2026-04-11', '2026-04-09'),
(3, 3, 1, 'Stock Check', 'D', 'low', 'completed', '2026-04-12', '2026-04-12', '2026-04-10'), (2, 4, 7, 'Calibration', 'D', 'medium', 'completed', '2026-04-13', '2026-04-13', '2026-04-11'),
(3, 5, 22, 'Water Test', 'D', 'high', 'completed', '2026-04-14', '2026-04-14', '2026-04-12'), (4, 1, 8, 'Floor Sweep', 'D', 'medium', 'completed', '2026-04-15', '2026-04-15', '2026-04-13'),
(3, 2, 23, 'Vet Audit', 'D', 'high', 'completed', '2026-04-16', '2026-04-16', '2026-04-14'), (4, 3, 9, 'Bag Count', 'D', 'low', 'completed', '2026-04-17', '2026-04-17', '2026-04-15'),
(2, 4, 10, 'IOT Maint', 'D', 'medium', 'completed', '2026-04-18', '2026-04-18', '2026-04-16'), (3, 5, 24, 'Pressure test', 'D', 'high', 'completed', '2026-04-19', '2026-04-19', '2026-04-17');
-- Present & Future (pending)
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate, createdDate) VALUES 
(4, 1, 11, 'Daily sweep', 'N', 'high', 'pending', '2026-04-20', '2026-04-20', '2026-04-18'), (3, 2, 25, 'Growth Audit', 'N', 'medium', 'pending', '2026-04-20', '2026-04-20', '2026-04-18'),
(4, 3, 12, 'Storage Audit', 'N', 'low', 'pending', '2026-04-21', '2026-04-21', '2026-04-19'), (2, 4, 13, 'Sensor Swap', 'N', 'medium', 'pending', '2026-04-22', '2026-04-22', '2026-04-20'),
(3, 5, 14, 'Clean nozzle', 'N', 'high', 'pending', '2026-04-23', '2026-04-23', '2026-04-21');

-- 3.11 Inventory (Today: 2026-04-21)
INSERT INTO [Inventory] (foodID, quantity, weightPerBag, importDate, expiredDate) VALUES 
-- 1. EXPIRED ITEMS (DaysExpired < 0)
(1, 3, 20.0, '2026-01-10', '2026-04-10'),   -- Organic Corn (Expired 11 days ago)
(5, 1, 5.0, '2026-01-15', '2026-04-15'),    -- Vitamin Mix (Expired 6 days ago)
(3, 5, 25.0, '2026-02-01', '2026-04-18'),   -- Growth Pellet (Expired 3 days ago)

-- 2. NEAR EXPIRY (DaysExpired 0~5)
(2, 50, 25.0, '2026-02-20', '2026-04-21'),  -- Soy Protein (Expires today)
(4, 20, 10.0, '2026-03-01', '2026-04-25'),  -- Mineral Block (4 days left)

-- 3. SAFE (DaysExpired > 30)
(1, 200, 20.0, '2026-04-01', '2027-01-01'),  -- Organic Corn (255 days left)
(3, 150, 25.0, '2026-04-05', '2026-12-30'); -- Growth Pellet (253 days left)
GO