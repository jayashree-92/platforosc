
USE DBMOD_Data_Backup_DB;
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingAccount_bkup_Aug_21_2023' AND TABLE_SCHEMA ='dbo')
BEGIN
	SELECT * INTO DBMOD_Data_Backup_DB...onBoardingAccount_bkup_Aug_21_2023 FROM HM_WIRES.DBO.onBoardingAccount;
	IF NOT EXISTS (select * from HM_WIRES.DBO.onBoardingAccount where hmFundId =130220) BEGIN 	UPDATE HM_WIRES.DBO.onBoardingAccount SET hmFundId=130220 where hmFundId =130057 END
	IF NOT EXISTS (select * from HM_WIRES.DBO.onBoardingAccount where hmFundId =130221) BEGIN 	UPDATE HM_WIRES.DBO.onBoardingAccount SET hmFundId=130221 where hmFundId =130058 END

END
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'hmsWires_bkup_Aug_21_2023' AND TABLE_SCHEMA ='dbo')
BEGIN
	SELECT * INTO DBMOD_Data_Backup_DB...hmsWires_bkup_Aug_21_2023 FROM HM_WIRES.DBO.hmsWires;
	IF NOT EXISTS (select * from HM_WIRES.DBO.hmsWires where hmFundId =130220) BEGIN 	UPDATE HM_WIRES.DBO.hmsWires SET hmFundId=130220 where hmFundId =130057 END
	IF NOT EXISTS (select * from HM_WIRES.DBO.hmsWires where hmFundId =130221) BEGIN 	UPDATE HM_WIRES.DBO.hmsWires SET hmFundId=130221 where hmFundId =130058 END

END
