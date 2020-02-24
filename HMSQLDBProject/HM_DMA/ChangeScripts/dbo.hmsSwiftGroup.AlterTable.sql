IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'onboardingSwiftGroupId' AND TABLE_NAME = 'onboardingSwiftGroup')
BEGIN
EXEC SP_RENAME 'onboardingSwiftGroup.onboardingSwiftGroupId', 'hmsSwiftGroupId';
END
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='onboardingSwiftGroup')
BEGIN
EXEC SP_RENAME 'onboardingSwiftGroup','hmsSwiftGroup'
END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'AcceptedMessages' AND TABLE_NAME = 'hmsSwiftGroup')
BEGIN
ALTER TABLE hmsSwiftGroup ADD AcceptedMessages VARCHAR(50) NULL;
END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Notes' AND TABLE_NAME = 'hmsSwiftGroup')
BEGIN
ALTER TABLE hmsSwiftGroup ADD Notes VARCHAR(500) NULL;
END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SwiftGroupStatusId' AND TABLE_NAME = 'hmsSwiftGroup')
BEGIN
	ALTER TABLE hmsSwiftGroup ADD SwiftGroupStatusId INT NULL;
	
	DECLARE @command varchar(Max);

	SELECT @Command ='UPDATE hmsSwiftGroup SET SwiftGroupStatusId = (SELECT hmsSwiftGroupStatusLkpId FROM hmsSwiftGroupStatusLkp WHERE STATUS = ''Live'')'

	EXEC(@command);

	ALTER TABLE [dbo].[hmsSwiftGroup]  WITH CHECK ADD  CONSTRAINT [FK_hmsSwiftGroup_SwiftGroupStatusId] FOREIGN KEY([SwiftGroupStatusId])
    REFERENCES [dbo].[hmsSwiftGroupStatusLkp] (hmsSwiftGroupStatusLkpId);

END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BrokerLegalEntityId' AND TABLE_NAME = 'hmsSwiftGroup')
BEGIN
ALTER TABLE hmsSwiftGroup ADD BrokerLegalEntityId BIGINT NULL;
END
GO






