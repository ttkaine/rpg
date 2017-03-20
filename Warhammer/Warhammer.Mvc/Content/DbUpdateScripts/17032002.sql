IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17032002
))
BEGIN

ALTER TABLE [Session] DROP DefaultGmIsSuspended_Session
ALTER TABLE [PostOrder] DROP DefaultIsSuspended_PostOrder
ALTER TABLE [PostOrder] DROP Column [IsSuspended]
ALTER TABLE [Session] DROP Column [GmIsSuspended]

ALTER TABLE PostOrder ADD IsSuspended bit NOT NULL CONSTRAINT DefaultIsSuspended_PostOrder DEFAULT 0
ALTER TABLE Session ADD GmIsSuspended bit NOT NULL CONSTRAINT DefaultGmIsSuspended_Session DEFAULT 0


  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17032002,GetDate(),'Switch IsSuspended for bit fields');

END