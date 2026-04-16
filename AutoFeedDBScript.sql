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
    [transferDate] DATE NOT NULL,
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
-- 3. DATA INSERTION (Dependency Order Fix)
-- ======================================================

-- Roles
INSERT INTO [Role] (description) VALUES ('Manager'), ('Technician'), ('Farmer'), ('Veterinarian'), ('Admin');

-- Foods
INSERT INTO [Food] (name, type, note) VALUES 
('Growth Pellet', 'Processed', 'N/A'), ('Organic Corn', 'Grain', 'N/A'), 
('Soy Protein', 'Protein', 'N/A'), ('Mineral Block', 'Supplement', 'N/A'), 
('Vitamin Mix', 'Additive', 'N/A');

-- 15 Barns (10 Large, 4 Flock, 1 Sick)
INSERT INTO [Barn] (temperature, humidity, type, area, waterAmount, foodAmount, createDate) VALUES 
-- BarnID 1 to 10
(24, 55, 'Large Chicken Barn', 100, 80, 40.0, '2026-04-01'), (24, 55, 'Large Chicken Barn', 100, 75, 40.0, '2026-04-01'),
(23, 58, 'Large Chicken Barn', 100, 85, 40.0, '2026-04-02'), (24, 55, 'Large Chicken Barn', 100, 70, 40.0, '2026-04-02'),
(25, 54, 'Large Chicken Barn', 100, 90, 40.0, '2026-04-03'), (24, 57, 'Large Chicken Barn', 100, 88, 40.0, '2026-04-03'),
(24, 55, 'Large Chicken Barn', 100, 82, 40.0, '2026-04-04'), (23, 59, 'Large Chicken Barn', 100, 79, 40.0, '2026-04-04'),
(24, 56, 'Large Chicken Barn', 100, 81, 40.0, '2026-04-05'), (24, 55, 'Large Chicken Barn', 100, 83, 40.0, '2026-04-05'),
-- BarnID 11 to 14
(25, 60, 'Flock Barn', 300, 85, 150.5, '2026-04-01'), (25, 61, 'Flock Barn', 300, 90, 150.5, '2026-04-01'), 
(25, 60, 'Flock Barn', 300, 75, 150.5, '2026-04-02'), (25, 62, 'Flock Barn', 300, 80, 150.5, '2026-04-02'),
-- BarnID 15
(22, 70, 'Flock Sick Barn', 150, 20, 75.25, '2026-04-03');

-- IoT Devices
INSERT INTO [IoT_Device] (name, description, status) VALUES 
('DHT11', 'Temp/Humid Sensor', 1), ('HX711', 'Weight Sensor', 1), ('V2 sensor', 'Water Sensor', 1);

-- FlockChicken (Quantity Control Logic)
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus) VALUES 
('Flock_Alpha', 1, 0.5, '2026-03-01', '2026-04-01', 'healthy'), -- 2 moved to LargeChicken
('Flock_Beta', 2, 0.45, '2026-03-05', '2026-04-02', 'healthy'),  -- 1 moved
('Flock_Gamma', 3, 0.4, '2026-03-10', '2026-04-05', 'sick'), 
('Flock_Delta', 2, 0.55, '2026-03-15', '2026-04-08', 'healthy'), 
('Flock_Epsilon', 2, 0.52, '2026-03-20', '2026-04-10', 'healthy');

-- Tasks
INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('Feeding','Cycle 1','07:00','08:00'), ('Cleaning','Area scan','09:00','10:30'), 
('Vet Check','Monthly','13:00','15:00'), ('Hardware','IoT Calibration','16:00','17:00'), 
('Watering','Refill tanks','10:00','11:00');

-- Users
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL, lastLogin) VALUES 
(1, 'alice@farm.com', 'p1', 'Alice Johnson', '0123456781', 'alice_mgr', 'https://tse1.mm.bing.net/th/id/OIP.p9bNjr0mX-spDgSi1S5hrgHaLH?rs=1&pid=ImgDetMain&o=7&rm=3', '2026-04-10'),
(2, 'bob@farm.com', 'p2', 'Bob Smith', '0123456782', 'bob_tech', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg', '2026-04-12'),
(3, 'charlie@farm.com', 'p3', 'Charlie Brown', '0123456783', 'charlie_f', 'https://tse1.mm.bing.net/th/id/OIP.s_OjVf-2_ScyG9UniAlm6wHaLH?w=1024&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3', '2026-04-14'),
(3, 'david@farm.com', 'p4', 'David Miller', '0123456784', 'david_f', 'https://www.microsoft.com/en-us/research/wp-content/uploads/2023/03/Hoang-photo-1024x1024.jpg', '2026-04-15'),
(4, 'eve@farm.com', 'p5', 'Eve Adams', '0123456785', 'eve_vet', 'https://leasy.github.io/images/busi.png', '2026-04-13');

-- IoT Assignments to Barn 1
INSERT INTO [BarnIoT_Device] (barnID, deviceID, installationDate) VALUES 
(1, 1, '2026-04-02'), (1, 2, '2026-04-02'), (1, 3, '2026-04-03');

-- LargeChicken
INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
(1, 'Champion_01', 3.5, 'healthy', 'https://tse2.mm.bing.net/th/id/OIP.uMHvXzIat56m-af5peVqGgHaJq?rs=1&pid=ImgDetMain&o=7&rm=3'),
(2, 'Elite_02', 3.4, 'healthy', 'https://anhdephd.vn/wp-content/uploads/2022/05/hinh-nen-ga-choi-dep.jpg'),
(1, 'Champion_03', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-duoc-chai-long-ti-mi.jpg'),
(4, 'Delta_01', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-so-huu-bo-long-mau-do-nau.jpg'),
(5, 'Epsilon_01', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-co-cua.jpg');

-- Inventory
INSERT INTO [Inventory] (foodID, quantity, weightPerBag, expiredDate) VALUES 
(1,100,20,'2027-01-01'), (2,50,25,'2027-02-15'), (3,200,50,'2026-12-30'), (4,80,20,'2026-11-20'), (5,30,5,'2027-06-10');

-- Data_IoT
INSERT INTO [Data_IoT] (barnID, deviceID, value, description, recordDate, sequenceNumber) VALUES 
(1, 1, 28.5, 'Temp Check', '2026-04-15 08:00', 101), (1, 2, 42.1, 'Weight Check', '2026-04-15 08:10', 103);

-- **ChickenBarn: Generates CBarnID 1, 2, 3, 4, 5**
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, status) VALUES 
(11, 1, NULL, '2026-04-01', 'active'), -- CBarnID: 1
(1, NULL, 1, '2026-04-05', 'active'),  -- CBarnID: 2
(3, NULL, 3, '2026-04-06', 'active'),  -- CBarnID: 3
(12, 2, NULL, '2026-04-07', 'active'), -- CBarnID: 4
(15, 3, NULL, '2026-04-08', 'active'); -- CBarnID: 5

-- FeedingRule
INSERT INTO [FeedingRule] (flockID, chickenLID, startDate, endDate, times, description) VALUES 
(1, NULL, '2026-04-01', '2026-05-01', 3, 'Chick diet'), 
(NULL, 1, '2026-04-05', '2026-05-05', 2, 'Individual champion diet');

INSERT INTO [FeedingRuleDetail] (ruleID, foodID, feedHour, feedMinute, amount, description) VALUES 
(1, 1, 7, 0, 2.5, 'Morning'), (1, 1, 18, 0, 2.5, 'Evening'), (2, 3, 8, 30, 15.0, 'Rich Breakfast');

-- Reports (Shared PDF URL)
INSERT INTO [Report] (userID, type, description, status, url, createDate) VALUES 
(3, 'Log', 'Maintenance scan', 'reviewed', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-05'),
(4, 'Prod', 'Weight summary', 'pending', 'https://firebasestorage.googleapis.com/v0/b/autofeeddata-6bcd2.firebasestorage.app/o/avatars%2Ff0439f90-8361-492d-b366-b21c5f6d8581.pdf?alt=media&token=e6f7c2b6-1687-413c-b50e-e4fcb9818ae0', '2026-04-14');

-- Requests (Status: pending, approved, rejected)
INSERT INTO [Request] (userID, type, description, status, createdAt) VALUES 
(4, 'Supply', 'Order 20 bags Soy', 'pending', '2026-04-08'),
(3, 'Hard', 'Replace faulty V2', 'approved', '2026-04-09');

-- **Schedule: Must use CBarnID (1-5) as Foreign Key**
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate, createdDate) VALUES 
(3, 1, 1, 'Conduct feed for Flock Alpha', 'N/A', 'high', 'completed', '2026-04-10', '2026-04-10', '2026-04-08'),
(4, 2, 2, 'Sweep Barn 1 floor', 'Clean sector', 'medium', 'pending', '2026-04-12', '2026-04-12', '2026-04-11'),
(5, 3, 5, 'Vet check Sick Barn', 'Urgent', 'high', 'in progress', '2026-04-13', '2026-04-13', '2026-04-12'),
(3, 4, 2, 'Calibrate HX711 in Barn 1', 'IoT task', 'medium', 'pending', '2026-04-14', '2026-04-14', '2026-04-13'),
(2, 5, 4, 'Refill tanks for Flock Beta', 'Sector B', 'low', 'pending', '2026-04-15', '2026-04-15', '2026-04-14');
GO