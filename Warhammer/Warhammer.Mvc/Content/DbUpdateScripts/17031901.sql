IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17031901
))
BEGIN

ALTER TABLE PostOrder ADD IsSuspended int NOT NULL CONSTRAINT DefaultIsSuspended_PostOrder DEFAULT 0
ALTER TABLE Session ADD GmIsSuspended int NOT NULL CONSTRAINT DefaultGmIsSuspended_Session DEFAULT 0

   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17031901,GetDate(),'Add IsSuspended to Players in Text Sessions');

END