IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBICorABAId' AND TABLE_NAME = 'onBoardingSSITemplate')
BEGIN
	ALTER TABLE onBoardingSSITemplate ADD BeneficiaryBICorABAId BIGINT NULL ;

	DECLARE @command1 varchar(Max);

	SELECT @Command1 ='UPDATE TGT SET TGT.BeneficiaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingSSITemplate TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.BeneficiaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command1);

	ALTER TABLE [dbo].[onBoardingSSITemplate]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingSSITemplate_BeneficiaryBICorABAId] FOREIGN KEY([BeneficiaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBICorABAId' AND TABLE_NAME = 'onBoardingSSITemplate')
BEGIN
	ALTER TABLE onBoardingSSITemplate ADD IntermediaryBICorABAId BIGINT NULL;

	DECLARE @command2 varchar(Max);

	SELECT @Command2 ='UPDATE TGT SET TGT.IntermediaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingSSITemplate TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.IntermediaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command2);

	ALTER TABLE [dbo].[onBoardingSSITemplate]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingSSITemplate_IntermediaryBICorABAId] FOREIGN KEY([IntermediaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END


IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBICorABAId' AND TABLE_NAME = 'onBoardingSSITemplate')
BEGIN
	ALTER TABLE onBoardingSSITemplate ADD UltimateBeneficiaryBICorABAId BIGINT NULL;
	
	DECLARE @command3 varchar(Max);

	SELECT @Command3 ='UPDATE TGT SET TGT.UltimateBeneficiaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingSSITemplate TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.UltimateBeneficiaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command3);

	ALTER TABLE [dbo].[onBoardingSSITemplate]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingSSITemplate_UltimateBeneficiaryBICorABAId] FOREIGN KEY([UltimateBeneficiaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END



--- Following are the columns to DROP - V 

--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBICorABA' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN BeneficiaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBankName' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN BeneficiaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBankAddress' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN BeneficiaryBankAddress; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBICorABA' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN IntermediaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBankName' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN IntermediaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBankAddress' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN IntermediaryBankAddress; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBICorABA' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN UltimateBeneficiaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBankName' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN UltimateBeneficiaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBankAddress' AND TABLE_NAME = 'onBoardingSSITemplate') BEGIN ALTER TABLE onBoardingSSITemplate DROP COLUMN UltimateBeneficiaryBankAddress; END
