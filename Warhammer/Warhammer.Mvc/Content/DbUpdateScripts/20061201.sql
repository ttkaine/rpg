	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20061201
))
BEGIN

	ALTER TABLE dbo.Person ADD GenerationComplete bit NOT NULL CONSTRAINT DF_Person_GenerationComplete DEFAULT 0 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20061201,GetDate(),'Add GenerationComplete to Person');

END	