IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17041401
))
BEGIN

ALTER TABLE [Session] ADD GmId int NULL 
	
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17041401,GetDate(),'Add nullable GmId to Session');

END