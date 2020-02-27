IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'hmFundId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD hmFundId BIGINT NOT NULL DEFAULT 0

	DECLARE @command0 varchar(Max);

	SELECT @command0 ='UPDATE tgt SET  tgt.hmFundId  =src.intFundId
	FROM onBoardingAccount  tgt
	INNER JOIN DMA_HM..vw_hFundOps src on tgt.dmaFundOnBoardId = src.OnBoardFundId
	WHERE src.OnBoardFundId is not null'	
	EXEC(@command0);

END

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBICorABAId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD BeneficiaryBICorABAId BIGINT NULL ;

	DECLARE @command1 varchar(Max);

	SELECT @Command1 ='UPDATE TGT SET TGT.BeneficiaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingAccount TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.BeneficiaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command1);

	ALTER TABLE [dbo].[onBoardingAccount]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingAccount_BeneficiaryBICorABAId] FOREIGN KEY([BeneficiaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBICorABAId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD IntermediaryBICorABAId BIGINT NULL;

	DECLARE @command2 varchar(Max);

	SELECT @Command2 ='UPDATE TGT SET TGT.IntermediaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingAccount TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.IntermediaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command2);

	ALTER TABLE [dbo].[onBoardingAccount]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingAccount_IntermediaryBICorABAId] FOREIGN KEY([IntermediaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END


IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBICorABAId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD UltimateBeneficiaryBICorABAId BIGINT NULL;
	
	DECLARE @command3 varchar(Max);

	SELECT @Command3 ='UPDATE TGT SET TGT.UltimateBeneficiaryBICorABAId = SRC.onBoardingAccountBICorABAId
	FROM onBoardingAccount TGT INNER JOIN onBoardingAccountBICorABA SRC ON SRC.BICorABA = TGT.UltimateBeneficiaryBICorABA
	WHERE  (TGT.BeneficiaryType =''BIC'' AND SRC.IsABA =0 ) OR  (TGT.BeneficiaryType =''ABA'' AND SRC.IsABA =1 )'

	EXEC(@command3);

	ALTER TABLE [dbo].[onBoardingAccount]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingAccount_UltimateBeneficiaryBICorABAId] FOREIGN KEY([UltimateBeneficiaryBICorABAId])
    REFERENCES [dbo].[onBoardingAccountBICorABA] ([onBoardingAccountBICorABAId]);

END


IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'WirePortalCutoffId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD WirePortalCutoffId BIGINT NULL;

		DECLARE @command4 varchar(Max);

		SELECT @Command4 ='UPDATE TGT SET TGT.WirePortalCutoffId =SRC.onBoardingWirePortalCutoffId FROM onBoardingAccount TGT INNER JOIN onBoardingWirePortalCutoff SRC ON SRC.CutOffTimeZone = TGT.CutOffTimeZone 
						WHERE SRC.DaystoWire = TGT.DaystoWire AND SRC.CutoffTime = TGT.CutoffTime AND SRC.CutoffTime is not null AND SRC.Currency = TGT.Currency AND SRC.CashInstruction =TGT.CashInstruction'

		EXEC(@command4);
	
	ALTER TABLE [dbo].[onBoardingAccount]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingAccount_WirePortalCutoffId] FOREIGN KEY([WirePortalCutoffId])
    REFERENCES [dbo].[onBoardingWirePortalCutoff] ([onBoardingWirePortalCutoffId]);

END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SwiftGroupId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD SwiftGroupId BIGINT NULL;

		DECLARE @command5 varchar(Max);

		SELECT @Command5 ='UPDATE TGT SET TGT.SwiftGroupId = SRC.hmsSwiftGroupId FROM onBoardingAccount TGT INNER JOIN hmsSwiftGroup SRC ON SRC.SwiftGroup = TGT.SwiftGroup 
						WHERE SRC.SendersBIC = TGT.SendersBIC'

		EXEC(@command5);
	
	ALTER TABLE [dbo].[onBoardingAccount]  WITH CHECK ADD  CONSTRAINT [FK_onBoardingAccount_SwiftGroupId] FOREIGN KEY([SwiftGroupId])
    REFERENCES [dbo].[hmsSwiftGroup] ([hmsSwiftGroupId]);

END
GO

IF NOT EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'AssociatedCustodyAcctNumber' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
	ALTER TABLE onBoardingAccount ADD AssociatedCustodyAcctNumber VARCHAR(100) NULL;
END
GO

--- Following are the columns to DROP - V 

--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'dmaFundOnBoardId' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN dmaFundOnBoardId; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBICorABA' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN BeneficiaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBankName' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN BeneficiaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'BeneficiaryBankAddress' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN BeneficiaryBankAddress; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBICorABA' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN IntermediaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBankName' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN IntermediaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IntermediaryBankAddress' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN IntermediaryBankAddress; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBICorABA' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN UltimateBeneficiaryBICorABA; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBankName' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN UltimateBeneficiaryBankName; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'UltimateBeneficiaryBankAddress' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN UltimateBeneficiaryBankAddress; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'CutoffTime' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN CutoffTime; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'DaystoWire' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN DaystoWire; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SwiftGroup' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN SwiftGroup; END
--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'SendersBIC' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN SendersBIC; END



--IF EXISTS(SELECT * FROM sys.default_constraints WHERE name LIKE 'DF__onBoardin__CutOf__%')
--BEGIN
--DECLARE @command1 varchar(Max);

--	SELECT @Command1 = 'ALTER TABLE onBoardingAccount drop constraint  ' + const.NAME
--	FROM sys.default_constraints const   
--	INNER JOIN sys.tables tab on const .parent_object_id = tab.object_id
--	WHERE  tab.name='onBoardingAccount' AND const.name LIKE 'DF__onBoardin__CutOf__%'
	
--EXEC(@command1);
	
--END


--IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'CutOffTimeZone' AND TABLE_NAME = 'onBoardingAccount') BEGIN ALTER TABLE onBoardingAccount DROP COLUMN CutOffTimeZone; END


--select * from onBoardingAccount



--select * from onBoardingWirePortalCutoff

