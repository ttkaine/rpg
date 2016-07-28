
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16072501
))
BEGIN

ALTER TABLE dbo.Page ADD
	PlainText nvarchar(MAX) NULL

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16072501,GetDate(),'Add plain text column to Page');

END