	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20210611
))
BEGIN

	ALTER TABLE dbo.PageView ADD FullView bit NOT NULL CONSTRAINT DF_PageView_FullView DEFAULT 0 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20210611,GetDate(),'Add FullView PageView');

END	