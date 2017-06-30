IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17042702
))
BEGIN

ALTER TABLE [Page] ADD GmNotes nvarchar(MAX) NULL
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17042702,GetDate(),'Add GmNotes To Page');

END