
IF EXISTS(SELECT 8 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'hmFundId' AND TABLE_NAME = 'onBoardingAccount')
BEGIN
 UPDATE tgt SET  tgt.hmFundId  =src.intFundId
 FROM onBoardingAccount  tgt
 INNER JOIN DMA_HM..vw_hFundOps src on tgt.dmaFundOnBoardId = src.OnBoardFundId
 WHERE src.OnBoardFundId is not null
END
GO

