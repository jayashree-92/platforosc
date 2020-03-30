IF NOT EXISTS(SELECT 8 FROM hmsSystemPreferences WHERE SystemKey = 'ReceivingAgreementTypesForAccount')
BEGIN
 INSERT INTO hmsSystemPreferences (SystemKey, SystemValue) VALUES('ReceivingAgreementTypesForAccount', 'FCM,CDA,ISDA,GMRA,MRA,MSFTA,FXPB')
END
GO