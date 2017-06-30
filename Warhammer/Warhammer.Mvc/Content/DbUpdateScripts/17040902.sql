IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17040902
))
BEGIN

ALTER TABLE dbo.Person ADD
	HasAttributeMoveAvailable bit NOT NULL CONSTRAINT DF_Person_HasStatSwapAvailable DEFAULT 0

	  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17040902,GetDate(),'Add HasAttributeMoveAvailable to Person');

END