IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16082402
))
BEGIN

ALTER TABLE Session ADD XpAwarded datetime null
ALTER TABLE SessionLog ADD XpAwarded datetime null

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16082402,GetDate(),'Add XpAwarded only to Session and SessionLog');

END
