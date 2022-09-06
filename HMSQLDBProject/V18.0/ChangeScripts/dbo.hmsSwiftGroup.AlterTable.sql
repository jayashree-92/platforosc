USE HM_WIRES;
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsAllowWireCancellation' AND TABLE_NAME = 'hmsSwiftGroup')
BEGIN
		DECLARE @Command  nvarchar(1000)
		SELECT @Command = 'ALTER TABLE hmsSwiftGroup DROP ' + d.name
		 FROM SYS.TABLES t
		  JOIN SYS.DEFAULT_CONSTRAINTS d on d.parent_object_id = t.object_id
		  JOIN SYS.COLUMNS c on c.object_id = t.object_id and c.column_id = d.parent_column_id
		 WHERE t.name = 'hmsSwiftGroup'	 and c.name = 'IsAllowWireCancellation'
		EXECUTE (@Command)

	ALTER TABLE hmsSwiftGroup DROP column IsAllowWireCancellation
END
GO


 

 
      