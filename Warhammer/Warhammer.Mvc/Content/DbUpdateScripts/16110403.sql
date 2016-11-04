IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16110403
))
BEGIN


ALTER TABLE [dbo].[Person]
DROP CONSTRAINT  DF_Person_CurrentXp

ALTER TABLE [dbo].[Person]
DROP COLUMN [CurrentXp]

   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16110403,GetDate(),'get rid of the old xp column to avoid confusions');

END