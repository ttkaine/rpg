	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18121801
))
BEGIN

	ALTER TABLE dbo.PageImage ADD [Public] BIT NOT NULL CONSTRAINT	DF_PageImage_Public DEFAULT 0 
		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18121801,GetDate(),'Add public flag for Pageimage');

END		