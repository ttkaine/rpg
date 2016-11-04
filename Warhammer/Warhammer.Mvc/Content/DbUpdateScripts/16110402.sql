IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16110402
))
BEGIN

UPDATE [dbo].[Person]
   SET [XPAwarded] = ([XpSpent] + [CurrentXp])
   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16110402,GetDate(),'Se the new decimal XP column value');

END