	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20032801
))
BEGIN

	ALTER TABLE dbo.Person ADD CurrentResolve int NOT NULL CONSTRAINT DF_Person_CurrentResolve DEFAULT 0 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20032801,GetDate(),'Add CurrentResolve to Person');

END		