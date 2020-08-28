
IF EXISTS(SELECT * FROM sys.objects WHERE type='U' AND name='OnBoardingSSITemplateServiceProvider_backup')
BEGIN
	DROP TABLE OnBoardingSSITemplateServiceProvider_backup;
END