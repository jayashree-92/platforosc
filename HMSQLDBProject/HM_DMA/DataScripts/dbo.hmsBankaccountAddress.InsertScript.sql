USE HM_WIRES
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingSSITemplate_bkup_v13_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
     
	SELECT * INTO dmabackup.onBoardingSSITemplate_bkup_allData_v13_0 FROM onBoardingSSITemplate
	SELECT  ROW_NUMBER() OVER (ORDER BY ssi.onBoardingSSITemplateId) AS RowId,SSI.*  FROM onBoardingSSITemplate ssi WHERE UltimateBeneficiaryType='Account Name' and UltimateBeneficiaryAccountName is not null
	DECLARE @count INT,@i INT = 1

	SELECT @count = (SELECT COUNT(*) FROM DMABackup.onBoardingSSITemplate_bkup_v13_0)
	WHILE (@i <= @count)
	BEGIN
		DECLARE @AccountName VARCHAR(MAX) 
		SELECT @AccountName = UltimateBeneficiaryAccountName FROM DMABackup.onBoardingSSITemplate_bkup_v13_0 WHERE RowId=@i
		IF NOT EXISTS (SELECT * FROM hmsBankAccountAddress WHERE UltimateBeneficiaryAccountName=@AccountName)
		BEGIN
			INSERT INTO [dbo].[hmsBankAccountAddress]
			   ([AccountName]
			   ,[AccountAddress]
			   ,[CreatedBy]
			   ,[CreatedAt]
			   ,[UpdatedBy]
			   ,[UpdatedAt]
			   ,[IsDeleted])
		 VALUES
			   (@AccountName
			   ,''
			   ,'Data Migration'
			   ,getdate()
			   ,'Data Migration'
			   ,getdate()
			   ,0)
		END
		   SET @i = @i + 1;
	END
END

