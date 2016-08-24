
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16072902
))
BEGIN

UPDATE [dbo].[Page]
   SET [XpAwarded] = 1
 WHERE [Created] < getdate()-21

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16072902,GetDate(),'Mark All Old Pages as XP Earned');

END