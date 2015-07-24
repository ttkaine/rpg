IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 2
))
BEGIN

ALTER TABLE dbo.Page ADD
	SignificantUpdateById int NOT NULL CONSTRAINT DF_Page_SignificantUpdateById DEFAULT 0,
	SignificantUpdate datetime NOT NULL CONSTRAINT DF_Page_SignificantUpdate DEFAULT GetDate(),
	Pinned bit NOT NULL CONSTRAINT DF_Page_Pinned DEFAULT 0


INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (2,GetDate(),'Adding Significant Change and Pin Columns to Page');

END
