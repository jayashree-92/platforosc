USE HM_WIRES;

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingAccountDocument_bkup_V7_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
   SELECT * INTO DMABackup.onBoardingAccountDocument_bkup_V7_0 FROM onBoardingAccountDocument;
END
IF  EXISTS(select * from onBoardingAccountDocument where onBoardingAccountId  IN (select onBoardingAccountId FROM onBoardingAccount WHERE hmFundId=0))
BEGIN
    DELETE FROM onBoardingAccountDocument where onBoardingAccountId  IN (select onBoardingAccountId FROM onBoardingAccount WHERE hmFundId=0) 
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingAccountSSITemplateMap_bkup_V7_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
   SELECT * INTO DMABackup.onBoardingAccountSSITemplateMap_bkup_V7_0 FROM onBoardingAccountSSITemplateMap;
END
IF  EXISTS(select * from onBoardingAccountSSITemplateMap where onBoardingAccountId  IN (select onBoardingAccountId FROM onBoardingAccount WHERE hmFundId=0))
BEGIN
    DELETE FROM onBoardingAccountSSITemplateMap where onBoardingAccountId  IN (select onBoardingAccountId FROM onBoardingAccount WHERE hmFundId=0) 
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingAccount_bkup_V7_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
   SELECT * INTO DMABackup.onBoardingAccount_bkup_V7_0 FROM onBoardingAccount;
END
IF  EXISTS(select * from onBoardingAccount where hmFundId =0)
BEGIN
    DELETE FROM onBoardingAccount where hmFundId =0 
END

