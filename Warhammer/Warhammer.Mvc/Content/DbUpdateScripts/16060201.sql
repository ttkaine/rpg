
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16060201
))
BEGIN


CREATE TABLE dbo.SimpleHitPoints
	(
	Id int NOT NULL IDENTITY (1, 1),
	PersonId int NOT NULL,
	HitPointTypeId int NOT NULL,
	HitPointLevelId int NOT NULL,
	Purchased datetime NULL,
	XpCost int NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.SimpleHitPoints ADD CONSTRAINT
	PK_SimpleHitPoints PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.SimpleHitPoints ADD CONSTRAINT
	FK_SimpleHitPoints_Person FOREIGN KEY
	(
	PersonId
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16060201,GetDate(),'Add hit point Table');

END