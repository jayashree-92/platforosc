USE [HM_WIRES]
GO


IF NOT EXISTS(SELECT * FROM hmsAccountCallback WHERE RecCreatedby ='Retrofit')
BEGIN
    
	INSERT INTO [dbo].[hmsAccountCallback]
           ([onBoardingAccountId]
           ,[ContactName]
           ,[ContactNumber]
           ,[Title]
           ,[IsCallbackConfirmed]
           ,[RecCreatedBy]
           ,[RecCreatedDt]
           ,[ConfirmedBy]
           ,[ConfirmedAt])
     

			select onBoardingAccountId           
           ,'Retrofit'
           ,'Retrofit'
           ,'Retrofit'
           ,0
           ,'Retrofit'
           ,getdate()
           ,'Retrofit'
           ,getdate()	
				FROM onBoardingAccount a WHERE onBoardingAccountId not in 
				(SELECT onBoardingAccountId  FROM hmsAccountCallback) and isdeleted=0 and hmfundid!=0 and a.createdat<='2021-02-16'	  
  
 END


