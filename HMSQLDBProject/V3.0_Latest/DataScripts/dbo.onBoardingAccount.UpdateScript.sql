UPDATE onBoardingAccount SET ApprovedBy = UpdatedBy WHERE onBoardingAccountStatus = 'Approved' AND ApprovedBy IS NULL
GO

UPDATE onBoardingSSITemplate SET ApprovedBy = UpdatedBy WHERE ssiTemplateStatus = 'Approved' AND ApprovedBy IS NULL
GO
