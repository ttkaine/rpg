	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19102601
))
BEGIN

	ALTER TABLE [PageImage] DROP COLUMN [Data]

	ALTER TABLE [Trophy] DROP COLUMN [ImageData]


	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19102601,GetDate(),'Drop old varbinary image columns');

END		