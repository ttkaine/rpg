
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16051601
))
BEGIN

CREATE TABLE dbo.ScoreHistory
	(
	Id int NOT NULL IDENTITY (1, 1),
	DateTime datetime NOT NULL,
	PersonId int NOT NULL,
	ScoreTypeId int NOT NULL,
	PointsValue decimal(16, 2) NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.ScoreHistory ADD CONSTRAINT
	PK_ScoreHistory PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.ScoreHistory ADD CONSTRAINT
	FK_ScoreHistory_Person FOREIGN KEY
	(
	PersonId
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16051601,GetDate(),'Add ScoreHistory Table');

END
