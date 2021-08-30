
SET IDENTITY_INSERT hmsDashboardPreferenceCodeLkup ON;

IF NOT EXISTS(SELECT * FROM hmsDashboardPreferenceCodeLkup WHERE hmsDashboardPreferenceCodeLkupId=10)
BEGIN
	INSERT INTO hmsDashboardPreferenceCodeLkup(hmsDashboardPreferenceCodeLkupId,PreferenceName) VALUES(10,'Admins')
END

SET IDENTITY_INSERT hmsDashboardPreferenceCodeLkup OFF;