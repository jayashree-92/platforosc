IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'EST' AND TimeZoneStandardName = 'Eastern Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('EST', 'Eastern Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'EDT' AND TimeZoneStandardName = 'Eastern Daylight Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('EDT', 'Eastern Daylight Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'CST' AND TimeZoneStandardName = 'Central Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('CST', 'Central Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'CDT' AND TimeZoneStandardName = 'Central Daylight Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('CDT', 'Central Daylight Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'MST' AND TimeZoneStandardName = 'Mountain Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('MST', 'Mountain Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'MDT' AND TimeZoneStandardName = 'Mountain Daylight Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('MDT', 'Mountain Daylight Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'PST' AND TimeZoneStandardName = 'Pacific Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('PST', 'Pacific Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'PDT' AND TimeZoneStandardName = 'Pacific Daylight Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('PDT', 'Pacific Daylight Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'CET' AND TimeZoneStandardName = 'Central European Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('CET', 'Central European Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'GMT' AND TimeZoneStandardName = 'GMT Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('GMT', 'GMT Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'IST' AND TimeZoneStandardName = 'India Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('IST', 'India Standard Time')
END 
GO

IF NOT EXISTS(SELECT 8 FROM hmsWireCutoffTimeZones WHERE TimeZone = 'JST' AND TimeZoneStandardName = 'Tokyo Standard Time')
BEGIN
INSERT INTO hmsWireCutoffTimeZones ([TimeZone], [TimeZoneStandardName]) VALUES('JST', 'Tokyo Standard Time')
END 
GO

UPDATE hmsWireCutoffTimeZones SET TimeZoneStandardName = 'Tokyo Standard Time' WHERE TimeZone = 'JST'
GO

