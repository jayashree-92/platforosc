
USE HM_WIRES
GO
IF OBJECT_ID('dbo.Split') IS NOT NULL
  DROP FUNCTION Split
GO
DECLARE @query NVARCHAR(MAX);

                SET @query  = '
CREATE FUNCTION [dbo].[Split](@data NVARCHAR(MAX), @delimiter NVARCHAR(5))
RETURNS @t TABLE (Val NVARCHAR(max))
AS
BEGIN
   
	DECLARE @tempData varchar(max)
	SET @tempData = REPLACE(@data, @delimiter, ''</d><d>'')
	SET @tempData = REPLACE(@tempData, ''&'', ''&amp;'')
	
    DECLARE @textXML XML;
    SELECT    @textXML = CAST(''<d>'' + @tempData + ''</d>'' AS XML);
 
    INSERT INTO @t(Val)
    SELECT  T.split.value(''.'', ''nvarchar(max)'') AS Val
    FROM    @textXML.nodes(''/d'') T(split)
   
    RETURN
END'
				
	EXEC sp_executesql @query ;

GO


use HM_WIRES
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'hmsDashboardPreferences_bkup_V13_0' AND TABLE_SCHEMA ='DmaBackup')
BEGIN
	
    SELECT  ROW_NUMBER() OVER (ORDER BY sc.hmsDashboardPreferenceId) as RowId,sc.* INTO DMABackup.hmsDashboardPreferences_bkup_V13_0 FROM hmsDashboardPreferences sc where hmsDashboardTemplateId in (select hmsDashboardTemplateId from hmsDashboardTemplates) and preferenceCode=2

	DECLARE @count INT,@i INT = 1,@templateId INT,@RecCreatedById INT

	SELECT @count = (SELECT COUNT(*) FROM DMABackup.hmsDashboardPreferences_bkup_V13_0)
	WHILE (@i <= @count)
		BEGIN
			DECLARE @hmFundIds VARCHAR(MAX),@AdminIds VARCHAR(MAX)
			SELECT @hmFundIds =  Preferences,@templateId=hmsDashboardTemplateId,@RecCreatedById=RecCreatedById FROM DMABackup.hmsDashboardPreferences_bkup_V13_0 WHERE RowId=@i
		

			SELECT @AdminIds =RTRIM(LTRIM(( SELECT DISTINCT SUBSTRING((SELECT DISTINCT ','+ CAST(fund.dmaOnBoardingAdminChoiceId  AS VARCHAR(30)) AS [text()]
									FROM (SELECT @i AS ID,* FROM dbo.Split(@hmFundIds, ',')) ST1
									INNER JOIN vw_FundAccounts fund ON fund.hmfundId = ST1.Val 
									WHERE ST1.ID = ST2.ID
									FOR XML PATH ('')), 2, 1000) FROM (SELECT @i AS ID,* from dbo.Split(@hmFundIds, ',')) ST2)));


			PRINT '-----------HMFUNDIDs---------------------';
			PRINT @hmFundIds;
			
			PRINT '-----------ADMINIDs---------------------';
			PRINT @AdminIds;
			PRINT '--------------------------------';					
			

			-- Prefernce Code 10 = 'Admin'
			INSERT INTO DBO.hmsDashboardPreferences (hmsDashboardTemplateId,PreferenceCode,Preferences,RecCreatedDt,RecCreatedById)
			SELECT @templateId,LC.hmsDashboardPreferenceCodeLkupId,ISNULL(@AdminIds,'-1'),RecCreatedDt,@RecCreatedById FROM DMABackup.hmsDashboardPreferences_bkup_V13_0 SC
			LEFT OUTER JOIN DBO.hmsDashboardPreferenceCodeLkup LC ON LC.PreferenceName ='Admins'
			WHERE RowId =@i 


			SET @i = @i + 1;

		END
  
	

 END
