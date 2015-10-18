IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15101801
))
BEGIN
UPDATE [dbo].[Player]
   SET [IsGm] = 1
 WHERE Id = 1
 
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15101801,GetDate(),'Setting self as Is GM');

END

