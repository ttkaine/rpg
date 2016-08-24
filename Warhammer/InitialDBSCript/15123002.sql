IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15123002
))
BEGIN

ALTER TABLE dbo.Person ADD
	XpSpent int NOT NULL CONSTRAINT DF_Person_XpSpent DEFAULT 0
		   
		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15123002,GetDate(),'XpSpent column on Person');

END