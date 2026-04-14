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
    [note] NVARCHAR(MAX) NULL
);

CREATE TABLE [dbo].[Barn] (
    [barnID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [temperature] DECIMAL(5, 2) NOT NULL,
    [humidity] DECIMAL(5, 2) NOT NULL,
    [type] NVARCHAR(50) NOT NULL,
    [area] DECIMAL(18, 2) NOT NULL,
    [createDate] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [CK_Barn_Type] CHECK ([type] IN ('Flock barn', 'flock sick barn', 'large chicken barn'))
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

CREATE TABLE [dbo].[FeedingSession] (
    [sessionID] INT IDENTITY(1,1) PRIMARY KEY,

    [foodID] INT NOT NULL,
    [userID] INT NULL,

    [plannedQuantity] DECIMAL(10,2) NOT NULL,
    [actualQuantity] DECIMAL(10,2) NULL,

    [status] NVARCHAR(20) DEFAULT 'pending',

    [createdAt] DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_FS_Food FOREIGN KEY(foodID) REFERENCES Food(foodID),
    CONSTRAINT FK_FS_User FOREIGN KEY(userID) REFERENCES [User](userID),

    CONSTRAINT CK_FS_Status CHECK (status IN ('pending', 'completed'))
);

CREATE TABLE [dbo].[FeedingSessionDetail] (
    [detailID] INT IDENTITY(1,1) PRIMARY KEY,

    [sessionID] INT NOT NULL,
    [inventID] INT NOT NULL,

    [quantity] DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_FSD_Session FOREIGN KEY(sessionID) REFERENCES FeedingSession(sessionID),
    CONSTRAINT FK_FSD_Inventory FOREIGN KEY(inventID) REFERENCES Inventory(inventID)
);

CREATE TABLE [dbo].[InventoryLog] (
    [logID] INT IDENTITY(1,1) PRIMARY KEY,

    [foodID] INT,
    [inventID] INT NULL,

    [type] NVARCHAR(20),

    [quantity] DECIMAL(10,2),

    [note] NVARCHAR(MAX),

    [createdAt] DATETIME DEFAULT GETDATE()
);
GO

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
    [status] NVARCHAR(20) NOT NULL, -- active, export
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
    [status] NVARCHAR(20) DEFAULT 'active', -- active, disabled
    CONSTRAINT [FK_Rule_Flock] FOREIGN KEY([flockID]) REFERENCES [dbo].[FlockChicken] ([flockID]),
    CONSTRAINT [FK_Rule_LargeChicken] FOREIGN KEY([chickenLID]) REFERENCES [dbo].[LargeChicken] ([chickenLID]),
    CONSTRAINT [CK_FeedingRule_Status] CHECK ([status] IN ('active', 'disabled'))
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

-- UPDATED: Status now includes 'disabled'
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
    CONSTRAINT [CK_Schedule_Status] CHECK ([status] IN ('pending', 'in progress', 'completed', 'disabled')), -- Added disabled
    CONSTRAINT [CK_Schedule_Priority] CHECK ([priority] IN ('low', 'medium', 'high'))
);
GO

-- ======================================================
-- 6. FULL SAMPLE DATA
-- ======================================================

-- Roles
INSERT INTO [Role] (description) VALUES ('Manager'), ('TechFarmer'), ('Farmer');

-- Users (15)
INSERT INTO [User] (roleID, email, password, fullName, phone, username, avatarURL) VALUES 
(1, 'alice@farm.com', 'p1', 'Alice Johnson', '0123456781', 'alice_mgr', 'https://tse1.mm.bing.net/th/id/OIP.p9bNjr0mX-spDgSi1S5hrgHaLH?rs=1&pid=ImgDetMain&o=7&rm=3'),
(2, 'bob@farm.com', 'p2', 'Bob Smith', '0123456782', 'bob_tech', 'https://haycafe.vn/wp-content/uploads/2022/03/Hinh-anh-chan-dung-nam-dep.jpg'),
(3, 'charlie@farm.com', 'p3', 'Charlie Brown', '0123456783', 'charlie_f', 'https://tse1.mm.bing.net/th/id/OIP.s_OjVf-2_ScyG9UniAlm6wHaLH?w=1024&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3'),
(3, 'david@farm.com', 'p4', 'David Miller', '0123456784', 'david_f', 'https://www.microsoft.com/en-us/research/wp-content/uploads/2023/03/Hoang-photo-1024x1024.jpg'),
(3, 'eve@farm.com', 'p5', 'Eve Adams', '0123456785', 'eve_f', 'https://leasy.github.io/images/busi.png'),
(3, 'frank@farm.com', 'p6', 'Frank White', '0123456786', 'frank_f', 'https://s7d1.scene7.com/is/image/dmqualcommprod/jiadi-zhu-800-800px?$QC_Responsive$&fmt=png-alpha'),
(3, 'grace@farm.com', 'p7', 'Grace Hopper', '0123456787', 'grace_f', 'https://i1.rgstatic.net/ii/profile.image/11431281089589168-1665624682603_Q512/Cunjun-Li.jpg'),
(3, 'hank@farm.com', 'p8', 'Hank Hill', '0123456788', 'hank_f', 'https://i1.rgstatic.net/ii/profile.image/539946005917696-1505744567688_Q512/Shuai-Han-13.jpg'),
(3, 'ivy@farm.com', 'p9', 'Ivy League', '0123456789', 'ivy_f', 'https://tse1.mm.bing.net/th/id/OIP.oAel1TzRXgP0-G7OahplEAHaHa?rs=1&pid=ImgDetMain&o=7&rm=3'),
(3, 'jack@farm.com', 'p10', 'Jack Reacher', '0123456710', 'jack_f', 'https://i1.rgstatic.net/ii/profile.image/1069199820607491-1631928510570_Q512/Hainan-Zhang-8.jpg'),
(3, 'kelly@farm.com', 'p11', 'Kelly Green', '0123456711', 'kelly_f', 'https://tse1.mm.bing.net/th/id/OIP.qegvQOMGxNYOvF8qVrOO3QHaHa?w=1536&h=1536&rs=1&pid=ImgDetMain&o=7&rm=3'),
(3, 'leo@farm.com', 'p12', 'Leo Messi', '0123456712', 'leo_f', 'https://tse4.mm.bing.net/th/id/OIP.8T8Oc2frNnW1dZwsWsN3qAHaH7?w=536&h=574&rs=1&pid=ImgDetMain&o=7&rm=3'),
(3, 'mia@farm.com', 'p13', 'Mia Wallace', '0123456713', 'mia_f', 'https://www.newtimeshair.com/wp-content/uploads/2023/05/Robin-2-1.jpg'),
(3, 'noah@farm.com', 'p14', 'Noah Ark', '0123456714', 'noah_f', 'https://i1.rgstatic.net/ii/profile.image/11431281264074724-1722426969202_Q512/Mingyang-Ren-4.jpg'),
(3, 'olivia@farm.com', 'p15', 'Olivia Pope', '0123456715', 'olivia_f', 'https://i1.rgstatic.net/ii/profile.image/1060523051347968-1629859807394_Q512/Q-Sun-3.jpg');

-- Food (15)
INSERT INTO [Food] (name, type, note) VALUES 
('Corn Mix', 'Grain', 'N'), ('Soy Mix', 'Protein', 'N'), ('Wheat', 'Grain', 'N'),
('Rice', 'Grain', 'N'), ('Calcium', 'Supp', 'N'), ('Fish Meal', 'Protein', 'N'),
('Barley', 'Grain', 'N'), ('Vitamin A', 'Supp', 'N'), ('Vitamin B', 'Supp', 'N'),
('Mineral Block', 'Supp', 'N'), ('Pellets', 'Processed', 'N'), ('Oats', 'Grain', 'N'),
('Seeds', 'Organic', 'N'), ('Mix C', 'Processed', 'N'), ('Mix D', 'Processed', 'N');

-- Barns (15)
INSERT INTO [Barn] (temperature, humidity, type, area) VALUES 
(25, 60, 'Flock barn', 300), (25, 60, 'Flock barn', 300), (25, 60, 'Flock barn', 300),
(25, 60, 'Flock barn', 300), (25, 60, 'Flock barn', 300), (22, 70, 'flock sick barn', 150),
(22, 70, 'flock sick barn', 150), (22, 70, 'flock sick barn', 150), (22, 70, 'flock sick barn', 150),
(22, 70, 'flock sick barn', 150), (24, 55, 'large chicken barn', 100), (24, 55, 'large chicken barn', 100),
(24, 55, 'large chicken barn', 100), (24, 55, 'large chicken barn', 100), (24, 55, 'large chicken barn', 100);

-- FlockChicken (15)
INSERT INTO [FlockChicken] (name, quantity, weight, DoB, transferDate, healthStatus) VALUES 
('F1', 5, 0.5, '2026-01-01', '2026-03-01', 'healthy'), ('F2', 5, 0.5, '2026-01-01', '2026-03-01', 'healthy'),
('F3', 5, 0.5, '2026-01-01', '2026-03-01', 'healthy'), ('F4', 5, 0.5, '2026-01-01', '2026-03-01', 'healthy'),
('F5', 5, 0.5, '2026-01-01', '2026-03-01', 'healthy'), ('F6', 2, 0.4, '2026-02-01', '2026-03-05', 'sick'),
('F7', 3, 0.4, '2026-02-01', '2026-03-05', 'sick'), ('F8', 3, 0.4, '2026-02-01', '2026-03-05', 'sick'),
('F9', 2, 0.4, '2026-02-01', '2026-03-05', 'sick'), ('F10', 3, 0.4, '2026-02-01', '2026-03-05', 'sick'),
('F11', 5, 0.8, '2025-12-01', '2026-02-01', 'healthy'), ('F12', 5, 0.8, '2025-12-01', '2026-02-01', 'healthy'),
('F13', 5, 0.8, '2025-12-01', '2026-02-01', 'healthy'), ('F14', 5, 0.8, '2025-12-01', '2026-02-01', 'healthy'),
('F15', 5, 0.8, '2025-12-01', '2026-02-01', 'healthy');

-- LargeChicken (15)
INSERT INTO [LargeChicken] (flockID, name, weight, healthStatus, url) VALUES 
(11, 'L1', 3.5, 'healthy', 'https://tse2.mm.bing.net/th/id/OIP.uMHvXzIat56m-af5peVqGgHaJq?rs=1&pid=ImgDetMain&o=7&rm=3'), (11, 'L2', 3.4, 'healthy', 'https://anhdephd.vn/wp-content/uploads/2022/05/hinh-nen-ga-choi-dep.jpg'),
(12, 'L3', 3.6, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-duoc-chai-long-ti-mi.jpg'), (12, 'L4', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-so-huu-bo-long-mau-do-nau.jpg'),
(13, 'L5', 3.7, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-co-cua.jpg'), (13, 'L6', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-xong-tran.jpg'),
(14, 'L7', 3.8, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-chuyen-nghiep.jpg'), (14, 'L8', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-dung-manh.jpg'),
(15, 'L9', 3.9, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-hien-ngang.jpg'), (15, 'L10', 3.5, 'healthy', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-khi-dung-duoi-anh-nang-gat.jpg'),
(1, 'L11', 2.0, 'sick', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-long-trang.jpg'), (2, 'L12', 2.0, 'sick', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-sieu-chien.jpg'),
(3, 'L13', 2.0, 'sick', 'https://haycafe.vn/wp-content/uploads/2022/03/Anh-ga-choi-vung-hoang-da.jpg'), (4, 'L14', 2.0, 'sick', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-ga-troi-tren-nen-co-xanh_011040616.jpg'),
(5, 'L15', 2.0, 'sick', 'https://img1.kienthucvui.vn/uploads/2021/01/15/anh-giong-ga-choi-noi-tieng-tai-viet-nam_011040771.jpg');

-- ChickenBarn (15)
INSERT INTO [ChickenBarn] (barnID, flockID, chickenLID, startDate, status) VALUES 
(1, 1, NULL, '2026-03-01', 'active'), (2, 2, NULL, '2026-03-01', 'active'), (3, 3, NULL, '2026-03-01', 'active'),
(4, 4, NULL, '2026-03-01', 'active'), (5, 5, NULL, '2026-03-01', 'active'), (6, 6, NULL, '2026-03-01', 'active'),
(7, 7, NULL, '2026-03-01', 'active'), (8, 8, NULL, '2026-03-01', 'active'), (9, 9, NULL, '2026-03-01', 'active'),
(10, 10, NULL, '2026-03-01', 'active'), (11, NULL, 1, '2026-03-01', 'active'), (12, NULL, 2, '2026-03-01', 'active'),
(13, NULL, 3, '2026-03-01', 'active'), (14, NULL, 4, '2026-03-01', 'active'), (15, NULL, 5, '2026-03-01', 'active');

-- Inventory (15)
INSERT INTO [Inventory] (foodID, quantity, weightPerBag, expiredDate) VALUES 
(1,50,20,'2027-01-01'), (2,50,20,'2027-01-01'), (3,50,20,'2027-01-01'), (4,50,20,'2027-01-01'), (5,50,20,'2027-01-01'),
(6,50,20,'2027-01-01'), (7,50,20,'2027-01-01'), (8,50,20,'2027-01-01'), (9,50,20,'2027-01-01'), (10,50,20,'2027-01-01'),
(11,50,20,'2027-01-01'), (12,50,20,'2027-01-01'), (13,50,20,'2027-01-01'), (14,50,20,'2027-01-01'), (15,50,20,'2027-01-01');

-- Tasks (15)
INSERT INTO [Task] (title, description, startTime, endTime) VALUES 
('T1','Feed','07:00','08:00'), ('T2','Feed','07:00','08:00'), ('T3','Feed','07:00','08:00'), ('T4','Clean','09:00','10:00'), ('T5','Clean','09:00','10:00'),
('T6','Clean','09:00','10:00'), ('T7','Check','11:00','12:00'), ('T8','Check','11:00','12:00'), ('T9','Check','11:00','12:00'), ('T10','Vet','13:00','14:00'),
('T11','Vet','13:00','14:00'), ('T12','Vet','13:00','14:00'), ('T13','Light','18:00','19:00'), ('T14','Light','18:00','19:00'), ('T15','Light','18:00','19:00');

-- Schedule (15) - Includes 'disabled' status
INSERT INTO [Schedule] (userID, taskID, CBarnID, description, note, priority, status, startDate, endDate) VALUES 
(3, 1, 1, 'D', 'N', 'high', 'pending', '2026-04-07', '2026-04-10'), (4, 2, 2, 'D', 'N', 'high', 'in progress', '2026-04-07', '2026-04-10'),
(5, 3, 3, 'D', 'N', 'high', 'completed', '2026-04-07', '2026-04-10'), (6, 4, 4, 'D', 'N', 'medium', 'disabled', '2026-04-07', '2026-04-10'),
(7, 5, 5, 'D', 'N', 'medium', 'pending', '2026-04-07', '2026-04-10'), (8, 6, 6, 'D', 'N', 'medium', 'in progress', '2026-04-07', '2026-04-10'),
(9, 7, 7, 'D', 'N', 'low', 'completed', '2026-04-07', '2026-04-10'), (10, 8, 8, 'D', 'N', 'low', 'disabled', '2026-04-07', '2026-04-10'),
(11, 9, 9, 'D', 'N', 'low', 'pending', '2026-04-07', '2026-04-10'), (12, 10, 10, 'D', 'N', 'high', 'in progress', '2026-04-07', '2026-04-10'),
(13, 11, 11, 'D', 'N', 'high', 'completed', '2026-04-07', '2026-04-10'), (14, 12, 12, 'D', 'N', 'medium', 'disabled', '2026-04-07', '2026-04-10'),
(15, 13, 13, 'D', 'N', 'medium', 'pending', '2026-04-07', '2026-04-10'), (3, 14, 14, 'D', 'N', 'low', 'in progress', '2026-04-07', '2026-04-10'),
(4, 15, 15, 'D', 'N', 'low', 'completed', '2026-04-07', '2026-04-10');

-- FeedingRule (3)
INSERT INTO [FeedingRule] (flockID, chickenLID, startDate, endDate, times, description, status) VALUES 
(1, NULL, '2026-03-01', '2026-04-01', 3, 'Morning Heavy', 'active'), 
(2, NULL, '2026-03-01', '2026-04-01', 2, 'Noon Lite', 'active'), 
(NULL, 1, '2026-03-01', '2026-04-01', 3, 'Old Strategy', 'disabled');

-- FeedingRuleDetail (3)
INSERT INTO [FeedingRuleDetail] (ruleID, foodID, feedHour, feedMinute, amount, description) VALUES 
(1, 1, 7, 30, 25.50, 'Breakfast cycle'),
(2, 2, 12, 0, 15.00, 'Lunch cycle'),
(3, 3, 18, 15, 10.25, 'Dinner cycle');

-- Reports (15)
INSERT INTO [Report] (userID, type, description, status, url) VALUES 
(1, 'A', 'D1', 'pending', 'h1'), (2, 'A', 'D2', 'reviewed', 'h2'), (3, 'A', 'D3', 'rejected', 'h3'),
(4, 'A', 'D4', 'pending', 'h4'), (5, 'A', 'D5', 'reviewed', 'h5'), (6, 'A', 'D6', 'rejected', 'h6'),
(7, 'A', 'D7', 'pending', 'h7'), (8, 'A', 'D8', 'reviewed', 'h8'), (9, 'A', 'D9', 'rejected', 'h9'),
(10, 'A', 'D10', 'pending', 'h10'), (11, 'A', 'D11', 'reviewed', 'h11'), (12, 'A', 'D12', 'rejected', 'h12'),
(13, 'A', 'D13', 'pending', 'h13'), (14, 'A', 'D14', 'reviewed', 'h14'), (15, 'A', 'D15', 'rejected', 'h15');

-- Requests (15)
INSERT INTO [Request] (userID, type, description, url) VALUES 
(3,'S','D1','u1'), (4,'S','D2','u2'), (5,'S','D3','u3'), (6,'S','D4','u4'), (7,'S','D5','u5'),
(8,'S','D6','u6'), (9,'S','D7','u7'), (10,'S','D8','u8'), (11,'S','D9','u9'), (12,'S','D10','u10'),
(13,'S','D11','u11'), (14,'S','D12','u12'), (15,'S','D13','u13'), (3,'S','D14','u14'), (4,'S','D15','u15');

-- FoodStorage (15)
INSERT INTO [FoodStorage] (foodID, barnID, food_weight, leftover_food) VALUES 
(1,1,100,10), (2,2,100,10), (3,3,100,10), (4,4,100,10), (5,5,100,10),
(6,6,100,10), (7,7,100,10), (8,8,100,10), (9,9,100,10), (10,10,100,10),
(11,11,100,10), (12,12,100,10), (13,13,100,10), (14,14,100,10), (15,15,100,10);

-- FeedingSession (5)
INSERT INTO [FeedingSession] (foodID, userID, plannedQuantity, actualQuantity, status)
VALUES 
(1, 3, 50, 48, 'completed'),
(2, 4, 40, NULL, 'pending'),
(3, 5, 30, 29, 'completed'),
(4, 6, 60, 58, 'completed'),
(5, 7, 45, NULL, 'pending');

--FeedingSessionDetail (5)
INSERT INTO [FeedingSessionDetail] (sessionID, inventID, quantity)
VALUES
(1, 1, 20),
(1, 2, 28),
(2, 3, 40),
(3, 4, 29),
(4, 5, 58);

--InventoryLog (5)
INSERT INTO [InventoryLog] (foodID, inventID, type, quantity, note)
VALUES
(1, 1, 'consume', 20, 'Feeding session 1'),
(2, 2, 'consume', 28, 'Feeding session 1'),
(3, 3, 'consume', 40, 'Feeding session 2'),
(4, 4, 'consume', 29, 'Feeding session 3'),
(5, 5, 'consume', 58, 'Feeding session 4');
GO