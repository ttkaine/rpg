IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18100701
))
BEGIN

ALTER TABLE dbo.Session ADD
	ArcId int NULL

ALTER TABLE dbo.Session ADD CONSTRAINT
	FK_Session_Arc FOREIGN KEY
	(
	ArcId
	) REFERENCES dbo.Arc
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18100701,GetDate(),'Link Artc to Session');

END		