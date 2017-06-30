IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17042801
))
BEGIN

UPDATE [dbo].[PersonAttribute]
   SET [PersonAttributeTypeEnum] = 6
      ,[Name] = ''
      ,[Description] = ''
      ,[InitialValue] = 1
      ,[CurrentValue] = 1

 WHERE [PersonAttributeTypeEnum] = 4 AND
 [InitialValue] = 4
 
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17042801,GetDate(),'retro-fit existing wear to edge');

END