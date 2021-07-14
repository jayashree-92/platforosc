IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsBankAccountAddress')
BEGIN
	CREATE TABLE [dbo].[hmsBankAccountAddress](
	[hmsBankAccountAddressId] [bigint] IDENTITY(1,1) NOT NULL,	
	[AccountName] [varchar](500) NULL,	
	[AccountAddress] [varchar](3000) NULL,
	[CreatedBy] [varchar](100) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedBy] [varchar](100) NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[hmsBankAccountAddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


END
GO

IF NOT EXISTS(SELECT * FROM SYS.objects WHERE NAME = 'UK_hmsBankAccountAddress_AccountName')
BEGIN
ALTER TABLE hmsBankAccountAddress ADD CONSTRAINT UK_hmsBankAccountAddress_AccountName UNIQUE (AccountName)
END
GO

