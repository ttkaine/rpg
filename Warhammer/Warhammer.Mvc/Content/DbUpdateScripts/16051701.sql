
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16051701
))
BEGIN

ALTER TABLE dbo.Person ADD

                CurrentScore decimal(16, 2) NOT NULL CONSTRAINT
DF_Person_CurrentScore DEFAULT 0

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16051701,GetDate(),'Add CurrentScore to Person');

END
