
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16082401
))
BEGIN

ALTER TABLE Page DROP COLUMN XpAwarded

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16082401,GetDate(),'Drop XpAwarded');

END