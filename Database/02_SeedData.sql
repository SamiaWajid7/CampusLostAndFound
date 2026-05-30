-- Campus Lost and Found Portal - Seed Data Script
-- Run this after creating the database schema

USE CampusLostAndFound;
GO

-- =============================================
-- Seed Categories
-- =============================================
SET IDENTITY_INSERT [dbo].[Categories] ON;

INSERT INTO [dbo].[Categories] ([Id], [Name], [Description], [IconClass], [IsActive])
VALUES 
    (1, 'Electronics', 'Phones, laptops, tablets, chargers, headphones, etc.', 'bi-phone', 1),
    (2, 'Bags & Wallets', 'Backpacks, purses, wallets, laptop bags, etc.', 'bi-bag', 1),
    (3, 'Keys', 'Car keys, room keys, key cards, key chains, etc.', 'bi-key', 1),
    (4, 'ID & Cards', 'Student IDs, credit cards, driver licenses, etc.', 'bi-credit-card', 1),
    (5, 'Clothing', 'Jackets, hats, scarves, gloves, shoes, etc.', 'bi-person-badge', 1),
    (6, 'Books & Notes', 'Textbooks, notebooks, planners, documents, etc.', 'bi-book', 1),
    (7, 'Accessories', 'Jewelry, watches, glasses, umbrellas, etc.', 'bi-gem', 1),
    (8, 'Sports Equipment', 'Balls, rackets, water bottles, gym gear, etc.', 'bi-dribbble', 1),
    (9, 'Food Containers', 'Lunch boxes, water bottles, thermoses, etc.', 'bi-cup-straw', 1),
    (10, 'Other', 'Items that do not fit other categories', 'bi-question-circle', 1);

SET IDENTITY_INSERT [dbo].[Categories] OFF;
GO

-- =============================================
-- Seed Locations (Sample Campus Buildings)
-- =============================================
SET IDENTITY_INSERT [dbo].[Locations] ON;

INSERT INTO [dbo].[Locations] ([Id], [Name], [Building], [Floor], [Description], [IsActive])
VALUES 
    (1, 'Main Library', 'Library Building', NULL, 'Central campus library with study areas and computer labs', 1),
    (2, 'Science Building - Floor 1', 'Science Building', '1st Floor', 'Chemistry and Physics labs', 1),
    (3, 'Science Building - Floor 2', 'Science Building', '2nd Floor', 'Biology labs and lecture halls', 1),
    (4, 'Student Center', 'Student Center', NULL, 'Cafeteria, lounges, and student services', 1),
    (5, 'Engineering Hall', 'Engineering Building', NULL, 'Engineering classrooms and workshops', 1),
    (6, 'Arts Building', 'Arts Building', NULL, 'Art studios, music rooms, and theater', 1),
    (7, 'Business School', 'Business Building', NULL, 'Business and economics classrooms', 1),
    (8, 'Sports Complex', 'Athletic Center', NULL, 'Gym, pool, and sports facilities', 1),
    (9, 'Dormitory A', 'Residence Hall A', NULL, 'Student housing - North Campus', 1),
    (10, 'Dormitory B', 'Residence Hall B', NULL, 'Student housing - South Campus', 1),
    (11, 'Cafeteria', 'Dining Hall', NULL, 'Main campus dining facility', 1),
    (12, 'Parking Lot A', 'Outdoor', NULL, 'Main student parking area', 1),
    (13, 'Parking Lot B', 'Outdoor', NULL, 'Staff and visitor parking', 1),
    (14, 'Campus Bus Stop', 'Outdoor', NULL, 'Main bus stop near entrance', 1),
    (15, 'Admin Building', 'Administration', NULL, 'Registrar, financial aid, and admin offices', 1);

SET IDENTITY_INSERT [dbo].[Locations] OFF;
GO

PRINT 'Seed data inserted successfully!';
GO
