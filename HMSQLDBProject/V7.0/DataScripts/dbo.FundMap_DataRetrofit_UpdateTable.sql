IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'hmsWires' AND COLUMN_NAME = 'DummyClientFundId')
BEGIN
	IF NOT EXISTS(SELECT * from hmsWires WHERE DummyClientFundId IS NOT NULL)
	BEGIN
		UPDATE hmsWires SET DummyClientFundId = hmFundId WHERE hmFundId IS NOT NULL;
		UPDATE TGT SET TGT.hmFundId = SRC.FundId FROM hmsWires TGT JOIN HM..ClientFund SRC ON TGT.hmFundId = SRC.ClientFundID WHERE TGT.hmFundId !=SRC.FundID;
	END
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= 'onBoardingAccount' AND COLUMN_NAME = 'DummyClientFundId')
BEGIN
	IF NOT EXISTS(SELECT * from onBoardingAccount WHERE DummyClientFundId IS NOT NULL)
	BEGIN
		UPDATE onBoardingAccount SET DummyClientFundId = hmFundId WHERE hmFundId IS NOT NULL;
		UPDATE TGT SET TGT.hmFundId = SRC.FundId FROM onBoardingAccount TGT JOIN HM..ClientFund SRC ON TGT.hmFundId = SRC.ClientFundID WHERE TGT.hmFundId !=SRC.FundID;
	END
END




--SELECT distinct src.hmFundId,bkup.hmFundId as ClientFundId,F.ShortFundName,LF.LegalFundName,CF.ClientFundName 
--FROM hmsWires src,DMABackup.hmsWires_bkup_V36_0 bkup, HM..Fund F,HM..ClientFund CF,HM..LegalFund LF  
--WHERE src.hmsWireId = bkup.hmsWireId 
--AND F.FundID = src.hmFundId AND CF.ClientFundID =bkup.hmFundId AND src.hmFundId !=bkup.hmFundId AND F.LegalFundID =LF.LegalFundID

--SELECT DISTINCT src.hmFundId,src.DummyClientFundId,F.ShortFundName,LF.LegalFundName,CF.ClientFundName FROM hmsWires src, HM..Fund F,HM..ClientFund CF,HM..LegalFund LF  WHERE F.FundID = src.hmFundId AND CF.ClientFundID =src.DummyClientFundId AND hmFundId !=DummyClientFundId AND F.LegalFundID =LF.LegalFundID
--SELECT DISTINCT src.hmFundId,src.DummyClientFundId,F.ShortFundName,LF.LegalFundName,CF.ClientFundName FROM onBoardingAccount src, HM..Fund F,HM..ClientFund CF,HM..LegalFund LF  WHERE F.FundID = src.hmFundId AND CF.ClientFundID =src.DummyClientFundId AND hmFundId !=DummyClientFundId AND F.LegalFundID =LF.LegalFundID