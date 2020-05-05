IF NOT EXISTS(SELECT * FROM hmsWireStatusLkup Where [Status] ='On Hold') 
BEGIN
	SET IDENTITY_INSERT hmsWireStatusLkup  ON;
		INSERT INTO hmsWireStatusLkup (hmsWireStatusId,Status) VALUES (6,'On Hold');
	SET IDENTITY_INSERT hmsWireStatusLkup  OFF;
END



	