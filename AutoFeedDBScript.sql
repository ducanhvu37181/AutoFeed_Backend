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
    CONSTRAINT [CK_Barn_Type] CHECK ([type] IN ('Flock barn', 'flock sick barn', 'large chicken barn'))
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
    [transferDate] DATE NOT NULL,
    [healthStatus] NVARCHAR(50) NOT NULL,
    [note] NVARCHAR(MAX) NULL,
    [isActive] BIT DEFAULT 1,
    CONSTRAINT [CK_Flock_Health] CHECK ([healthStatus] IN ('healthy', 'sick'))
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

CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Flock ON ChickenBarn(flockID) WHERE flockID IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX UIX_CBarn_Chicken ON ChickenBarn(chickenLID) WHERE chickenLID IS NOT NULL;

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
-- 3. UPDATED SAMPLE DATA (Logical Dates from April 1, 2026)
-- ======================================================

-- 5 Roles
INSERT INTO [Role] (description) VALUES ('Manager'), ('Technician'), ('Farmer'), ('Veterinarian'), ('Admin');

-- 5 Foods
INSERT INTO [Food] (name, type, note) VALUES 
('Corn Mix', 'Grain', 'Daily nutritional base'), ('Soy Mix', 'Protein', 'Protein supplement'), 
('Growth Pellet', 'Processed', 'For large chicken development'), ('Mineral Block', 'Supplement', 'Bone health'), 
('Vitamin Premix', 'Additive', 'Immunity support');

-- 15 Barns (10 Large, 3 Small, 2 Sick)
INSERT INTO [Barn] (temperature, humidity, type, area, waterAmount, foodAmount, createDate) VALUES 
-- 10 Large Chicken Barns
(24, 55, 'large chicken barn', 100, 80, 40.0, '2026-04-01'), (24, 55, 'large chicken barn', 100, 75, 40.0, '2026-04-01'),
(23, 58, 'large chicken barn', 100, 85, 40.0, '2026-04-02'), (24, 55, 'large chicken barn', 100, 70, 40.0, '2026-04-02'),
(25, 54, 'large chicken barn', 100, 90, 40.0, '2026-04-03'), (24, 57, 'large chicken barn', 100, 88, 40.0, '2026-04-03'),
(24, 55, 'large chicken barn', 100, 82, 40.0, '2026-04-04'), (23, 59, 'large chicken barn', 100, 79, 40.0, '2026-04-04'),
(24, 56, 'large chicken barn', 100, 81, 40.0, '2026-04-05'), (24, 55, 'large chicken barn', 100, 83, 40.0, '2026-04-05'),
-- 3 Flock Barns (Small)
(25, 60, 'Flock barn', 300, 85, 150.5, '2026-04-01'), (25, 61, 'Flock barn', 300, 90, 150.5, '2026-04-01'), (25, 60, 'Flock barn', 300, 75, 150.5, '2026-04-02'),
-- 2 Sick Barns
(22, 70, 'flock sick barn', 150, 20, 75.25, '2026-04-02'), (22, 71, 'flock sick barn', 150, 35, 75.25, '2026-04-03');

-- 3 IoT Devices on Barn 1
INSERT INTO [IoT_Device] (name, description, status) VALUES 
('DHT11', 'Temperature and Humidity Sensor', 1),
('HX711', 'Load Cell Weight Sensor', 1),
('V2 sensor', 'Ultrasonic Water Level Sensor', 1);

INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate) VALUES 
(1, 1, '2026-04-02'), (1, 2, '2026-04-02'), (1, 3, '2026-04-03');

-- 5 Users (Real Avatar URLs)
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL, lastLogin) VALUES 
(1, 'alice@farm.com', 'p1', 'Alice Johnson', '0123456781', 'alice_mgr', 'https://tse1.mm.bing.net/th/id/OIP.p9bNjr0mX-spDgSi1S5hrgHaLH?rs=1&pid=ImgDetMain&o=7&rm=3', '2026-04-10'),
(2, 'bob@farm.com', 'p2', 'Bob Smith', '0123456782', 'bob_tech', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg', '2026-04-12'),
(3, 'charlie@farm.com', 'p3', 'Charlie Brown', '0123456783', 'charlie_f', 'https://tse1.mm.bing.net/th/id/OIP.s_OjVf-2_ScyG9UniAlm6wHaLH?w=1024&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3', '2026-04-14'),
(3, 'david@farm.com', 'p4', 'David Miller', '0123456784', 'david_f', 'https://www.microsoft.com/en-us/research/wp-content/uploads/2023/03/Hoang-photo-1024x1024.jpg', '2026-04-15'),
(4, 'eve@farm.com', 'p5', 'Eve Adams', '0123456785', 'eve_vet', 'https://leasy.github.io/images/busi.png', '2026-04-13');

-- 5 FlockChicken (Quantity = 3)
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus) VALUES 
('Flock_Alpha', 3, 0.5, '2026-03-01', '2026-04-01', 'healthy'),
('Flock_Beta', 3, 0.45, '2026-03-05', '2026-04-02', 'healthy'),
('Flock_Gamma_Sick', 3, 0.4, '2026-03-10', '2026-04-05', 'sick'),
('Flock_Delta', 3, 0.55, '2026-03-15', '2026-04-08', 'healthy'),
('Flock_Epsilon', 3, 0.52, '2026-03-20', '2026-04-10', 'healthy');

-- 5 LargeChicken (Real Chicken URLs)
INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
(1, 'Hen_A1', 3.5, 'healthy', 'https://tse2.mm.bing.net/th/id/OIP.uMHvXzIat56m-af5peVqGgHaJq?rs=1&pid=ImgDetMain&o=7&rm=3'),
(2, 'Rooster_B2', 3.4, 'healthy', 'https://anhdephd.vn/wp-content/uploads/2022/05/hinh-nen-ga-choi-dep.jpg'),
(4, 'Hen_C3', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-duoc-chai-long-ti-mi.jpg'),
(5, 'Rooster_D4', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-so-huu-bo-long-mau-do-nau.jpg'),
(1, 'Hen_E5', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-co-cua.jpg');

-- ChickenBarn
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, status) VALUES 
(11, 1, NULL, '2026-04-01', 'active'), (1, NULL, 1, '2026-04-05', 'active'),
(12, 2, NULL, '2026-04-06', 'active'), (14, 3, NULL, '2026-04-07', 'active'),
(2, NULL, 2, '2026-04-08', 'active');

-- Data_IoT
INSERT INTO [Data_IoT] (barnID, deviceID, value, description, recordDate, sequenceNumber) VALUES 
(1, 1, 28.5, 'Temp reading', '2026-04-15 08:00', 101), (1, 1, 65.0, 'Humidity reading', '2026-04-15 08:05', 102),
(1, 2, 42.1, 'Feed weight reading', '2026-04-15 08:10', 103), (1, 3, 85.0, 'Water level reading', '2026-04-15 08:15', 104),
(1, 1, 28.7, 'Periodic log', '2026-04-15 09:00', 105);

-- Inventory
INSERT INTO [Inventory] (foodID, quantity, weightPerBag, expiredDate) VALUES 
(1, 100, 20.0, '2027-01-01'), (2, 50, 25.0, '2027-02-15'),
(3, 200, 50.0, '2026-12-30'), (4, 80, 20.0, '2026-11-20'),
(5, 30, 5.0, '2027-06-10');

-- Tasks
INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('Feeding', 'Dispense feed to barns', '07:00', '08:00'),
('Cleaning', 'Sweep and disinfect', '09:00', '10:30'),
('Checkup', 'Health inspection by vet', '13:00', '15:00'),
('Maintenance', 'Check IoT sensors', '16:00', '17:00'),
('Watering', 'Refill water tanks', '10:00', '11:00');

-- 5 Schedules (Updated: endDate = startDate)
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate, createdDate) VALUES 
(3, 1, 1, 'Conduct morning feed', 'Normal', 'high', 'completed', '2026-04-10', '2026-04-10', '2026-04-08'),
(4, 2, 2, 'Sweep large barn sector A', 'N/A', 'medium', 'pending', '2026-04-11', '2026-04-11', '2026-04-10'),
(5, 3, 4, 'Isolation flock check', 'Urgent', 'high', 'in progress', '2026-04-12', '2026-04-12', '2026-04-11'),
(3, 4, 1, 'Recalibrate HX711', 'Important', 'medium', 'pending', '2026-04-13', '2026-04-13', '2026-04-12'),
(2, 5, 3, 'Refill sector B tanks', 'Sector B', 'low', 'pending', '2026-04-14', '2026-04-14', '2026-04-13');

-- FeedingRules & Details
INSERT INTO [FeedingRule] (flockID, chickenLID, startDate, endDate, times, description) VALUES 
(1, NULL, '2026-04-01', '2026-05-01', 3, 'Growth diet'),
(NULL, 1, '2026-04-05', '2026-05-05', 2, 'Mature diet');

INSERT INTO [FeedingRuleDetail] (ruleID, foodID, feedHour, feedMinute, amount, description) VALUES 
(1, 1, 7, 30, 2.5, 'Morning'), (1, 1, 12, 0, 2.0, 'Noon'), (1, 1, 18, 0, 2.5, 'Evening'),
(2, 3, 8, 30, 15.0, 'Morning Heavy'), (2, 3, 17, 0, 12.0, 'Evening Lite');

-- 5 Reports (Using Shared PDF URL)
INSERT INTO [Report] (userID, type, description, status, url, createDate) VALUES 
(3, 'Maintenance', 'Sensor battery replacement log', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-05'),
(2, 'Technical', 'IoT Gateway connectivity report', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-08'),
(5, 'Medical', 'Sick flock recovery status', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-12'),
(4, 'Production', 'Yield analysis Barn 11', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-14'),
(1, 'Inventory', 'Stock usage summary week 2', 'rejected', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-15');

-- 5 Requests
INSERT INTO [Request] (userID, type, description, status, createdAt) VALUES 
(4, 'Supplies', 'Need 10 more bags of Soy Mix', 'pending', '2026-04-08'),
(3, 'Equipment', 'Replacement for faulty V2 sensor', 'approved', '2026-04-09'),
(5, 'Medicine', 'Order bird flu vaccines', 'pending', '2026-04-11'),
(4, 'Personal', 'Request day off for personal errands', 'approved', '2026-04-12'),
(2, 'Utility', 'Check electrical wiring sector C', 'pending', '2026-04-14');
GO