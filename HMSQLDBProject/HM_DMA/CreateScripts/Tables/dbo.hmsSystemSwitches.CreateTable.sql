IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSystemSwitches')
BEGIN
	CREATE TABLE [dbo].[hmsSystemSwitches](
	[hmsSystemSwitchId] BIGINT IDENTITY(1,1) NOT NULL,
	[Module] VARCHAR(50) NOT NULL,
	[Key] VARCHAR(100) NOT NULL,
	[Value] VARCHAR(500) NOT NULL,
	[LastModifiedBy] VARCHAR(100) NOT NULL,
	[LastModifiedDt] DATETIME NOT NULL DEFAULT(GETDATE())

 PRIMARY KEY  CLUSTERED  
(
	[hmsSystemSwitchId] ASC
)
) ON [PRIMARY]
END
GO 

