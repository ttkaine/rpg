IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17042701
))
BEGIN

ALTER TABLE [PersonAttribute] ADD IsPrivate bit NOT NULL CONSTRAINT IsPrivate_PersonAttribute DEFAULT 1
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17042701,GetDate(),'Add IsPrivate bit to PersonAttribute');

END