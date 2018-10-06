IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18042101
))
BEGIN

DELETE FROM [dbo].[SiteFeature]
      WHERE name = 'AutoPopulatePeopleInNewSessions'
		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18042101,GetDate(),'Remove AutoPopulatePeopleInNewSessions SiteFeature');

END		


