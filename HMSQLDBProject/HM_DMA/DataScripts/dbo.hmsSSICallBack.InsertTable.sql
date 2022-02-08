USE [HM_WIRES]
GO


IF NOT EXISTS(SELECT * FROM hmsSSICallback WHERE RecCreatedby ='Retrofit')
BEGIN
    
	INSERT INTO [dbo].[hmsSSICallback]
           ([onBoardingSSITemplateId]
           ,[ContactName]
           ,[ContactNumber]
           ,[Title]
           ,[IsCallbackConfirmed]
           ,[RecCreatedBy]
           ,[RecCreatedDt]
           ,[ConfirmedBy]
           ,[ConfirmedAt])
     
	 select onBoardingSSITemplateId           
           ,'Retrofit'
           ,'Retrofit'
           ,'Retrofit'
           ,0
           ,'Retrofit'
           ,getdate()
           ,'Retrofit'
           ,getdate()	
				FROM onBoardingSSITemplate a WHERE onBoardingSSITemplateId NOT IN 
				(SELECT onBoardingSSITemplateId  FROM hmsSSICallback) AND isdeleted=0 AND  a.createdat<='2021-02-16'
			 
  
 END


