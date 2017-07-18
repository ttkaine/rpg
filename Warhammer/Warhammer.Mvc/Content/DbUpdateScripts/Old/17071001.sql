IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17071001
))
BEGIN


ALTER TABLE dbo.Person ADD
	GenderEnum int NULL

ALTER TABLE dbo.Page ADD
	WordCount int NOT NULL CONSTRAINT DF_Page_WordCount DEFAULT 0
	
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17071001,GetDate(),'add gender to person and wordcount to page');

END