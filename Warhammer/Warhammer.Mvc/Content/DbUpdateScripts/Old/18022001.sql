IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18022001
))
BEGIN

	ALTER TABLE dbo.Person ADD
		XpGroup int NULL
		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18022001,GetDate(),'Add XP group to person');

END		