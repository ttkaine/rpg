IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17111101
))
BEGIN

ALTER TABLE dbo.Post ADD
	ImageId int NULL

ALTER TABLE dbo.Post ADD CONSTRAINT
	FK_Post_PageImage FOREIGN KEY
	(
	ImageId
	) REFERENCES dbo.PageImage
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	

		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17111101,GetDate(),'Add an image to posts');

END