IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 11
))
BEGIN

ALTER TABLE [dbo].[Trophy] ADD
	TypeId int NOT NULL CONSTRAINT DF_Trophy_TypeId DEFAULT 0

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (11,GetDate(),'Adding TypeId to Trophy table');
	
END