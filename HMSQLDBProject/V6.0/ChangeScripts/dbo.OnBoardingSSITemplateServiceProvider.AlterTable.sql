
IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='OnBoardingSSITemplateServiceProvider')
BEGIN
	EXEC sp_rename 'OnBoardingSSITemplateServiceProvider', 'OnBoardingSSITemplateServiceProvider_backup';
END

