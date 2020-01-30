IF NOT EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='hmsSystemPreferences')
BEGIN
	CREATE TABLE [dbo].[hmsSystemPreferences]
	(
		[hmsSystemPreferenceId] BIGINT NOT NULL IDENTITY(1,1),
		[SystemKey] VARCHAR(50) NOT NULL,
		[SystemValue] VARCHAR(3000) NOT NULL,
		CONSTRAINT hmsSystemPreferences_PK PRIMARY KEY NONCLUSTERED ([hmsSystemPreferenceId])
	);
	
	INSERT INTO [hmsSystemPreferences](SystemKey,SystemValue) VALUES ('AllowedAgreementTypesForAccounts','CDA,Custody,DDA,Deemed ISDA,Enhanced Custody,FCM,FXPB,GMRA,ISDA,Listed Options,MRA,MSFTA,Non-US Listed Options,PB,Synthetic Prime Brokerage')

END
GO