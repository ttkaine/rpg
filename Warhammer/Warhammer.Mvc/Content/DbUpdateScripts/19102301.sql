	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19102301
))
BEGIN

	ALTER TABLE dbo.[Page] ADD FileIdentifier nvarchar(50) NULL 
	ALTER TABLE dbo.[Trophy] ADD FileIdentifier nvarchar(50) NULL 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19102301,GetDate(),'Add FileIdentifier for Page and Trophy');

END		