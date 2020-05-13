
UPDATE hmsWireTransferTypeLKup SET TransferType ='3rd Party Transfer' where TransferType='Normal Transfer'
UPDATE hmsWireTransferTypeLKup SET TransferType ='Fund Transfer' where TransferType='Book Transfer'


select * from hmsWireTransferTypeLKup
