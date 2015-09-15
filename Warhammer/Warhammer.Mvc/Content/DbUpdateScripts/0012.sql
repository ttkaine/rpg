IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 12
))
BEGIN

UPDATE [dbo].[Trophy]
	set TypeId = 1 where Name = 'The Dead Award'
	
UPDATE [dbo].[Trophy]
	set TypeId = 2 where Name = 'Main Party Banner'
	
	
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (12,GetDate(),'Setting typeid on Trophy dead and Banner');
	
END
