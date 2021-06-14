IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsUsers')
BEGIN
	CREATE TABLE [dbo].[hmsUsers]
	(
		[hmsWireUserId] BIGINT NOT NULL IDENTITY(1,1),
		[hmLoginId] INT NOT NULL,
		[LdapRole] VARCHAR(30) NOT NULL,
		[AccountStatus] VARCHAR(200) NOT NULL,
		CONSTRAINT PK_hmsUsers PRIMARY KEY NONCLUSTERED ([hmsWireUserId]),
		CONSTRAINT UQ_hmsUsers_hmLoginId_LdapRole UNIQUE ([hmLoginId],[LdapRole])
	);
END
GO