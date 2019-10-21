	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19102102
))
BEGIN

	ALTER TABLE dbo.Post ADD RollModifier int NOT NULL CONSTRAINT DF_PageImage_RollModifier DEFAULT 0 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19102102,GetDate(),'Add RollModifier for Post');

END		