USE [HM_WIRES]
GO


IF NOT EXISTS(SELECT * FROM hmsAccountCallback WHERE onBoardingAccountId=13 ) 
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
     VALUES
           (13
           ,'Retrofit'
           ,'Retrofit'
           ,'Retrofit'
           ,0
           ,'Retrofit'
           ,getdate()
           ,'Retrofit'
           ,getdate())
		   END
GO


