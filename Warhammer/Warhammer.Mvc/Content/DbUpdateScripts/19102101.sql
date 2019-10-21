	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19102101
))
BEGIN

	ALTER TABLE dbo.PageImage ADD FileIdentifier nvarchar(50) NULL 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19102101,GetDate(),'Add FileIdentifier for Pageimage');

END		