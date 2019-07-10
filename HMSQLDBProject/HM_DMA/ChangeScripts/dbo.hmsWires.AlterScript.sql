--IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE ='F' AND NAME = 'FK_hmsWires_onBoardingAccount_onBoardingAccountId')
--BEGIN
--	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_onBoardingAccount_onBoardingAccountId FOREIGN KEY ([OnBoardAccountId]) REFERENCES onBoardingAccount(onBoardingAccountId);
--END


--IF NOT EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE ='F' AND NAME = 'FK_hmsWires_onBoardingSSITemplate_onBoardingSSITemplateId')
--BEGIN
--	ALTER TABLE hmsWires ADD CONSTRAINT FK_hmsWires_onBoardingSSITemplate_onBoardingSSITemplateId FOREIGN KEY ([OnBoardSSITemplateId]) REFERENCES onBoardingSSITemplate(onBoardingSSITemplateId);
--END
