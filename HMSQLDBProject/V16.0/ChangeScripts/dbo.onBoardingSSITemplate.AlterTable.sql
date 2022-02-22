
IF NOT EXISTS(SELECT * FROM SYS.FOREIGN_KEYS WHERE name='FK_onBoardingSSITemplate_OnBoardingSSITemplateAccountType_TemplateTypeId' and parent_object_id=Object_ID(N'onBoardingSSITemplate'))
BEGIN
	ALTER TABLE onBoardingSSITemplate ALTER COLUMN TemplateTypeId BIGINT NOT NULL;
	ALTER TABLE [dbo].[onBoardingSSITemplate]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingSSITemplate_OnBoardingSSITemplateAccountType_TemplateTypeId] FOREIGN KEY([TemplateTypeId])
	REFERENCES [dbo].[OnBoardingSSITemplateAccountType] ([OnBoardingSSITemplateAccountTypeId])
END



