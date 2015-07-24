IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 6
))
BEGIN

ALTER TABLE dbo.Person ADD
	CauseOfDeath nvarchar(500) NULL

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (6,GetDate(),'Adding Cause of death to person');

END
