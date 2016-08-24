IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 8
))
BEGIN

ALTER TABLE dbo.Person ADD
	IsInMainParty bit NOT NULL CONSTRAINT DF_Person_IsInMainParty DEFAULT 0

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (8,GetDate(),'Adding IsInMainParty to Person');

END
