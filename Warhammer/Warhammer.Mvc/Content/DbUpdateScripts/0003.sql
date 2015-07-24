IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 3
))
BEGIN


	UPDATE [dbo].[Page]
   SET [SignificantUpdateById] = [CreatedById]
      ,[SignificantUpdate] = [Created]


INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (3,GetDate(),'Set Sensible defaults for Significant Change');

END
