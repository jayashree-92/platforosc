use HM_WIRES

IF NOT EXISTS(SELECT * FROM hmsWireStatusLkup WHERE hmsWireStatusId = 7 AND Status = 'System Failure')
BEGIN
	SET  IDENTITY_INSERT  hmsWireStatusLkup ON
	Insert into hmsWireStatusLkup(hmsWireStatusId,Status) values (7, 'System Failure')
	SET IDENTITY_INSERT hmsWireStatusLkup OFF
END
GO


select * from hmsWireStatusLkup