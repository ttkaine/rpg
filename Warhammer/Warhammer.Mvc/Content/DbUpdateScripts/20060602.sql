	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20060602
))
BEGIN

	CREATE TABLE dbo.ExperiencePoint
		(
		Id int NOT NULL IDENTITY (1, 1),
		PlayerId int NOT NULL,
		PersonId int NOT NULL,
		SessionId int null,
		TermId int null,
		PersonAttributeId int null,
		Awarded DateTime NOT NULL,
		Spent DateTime NULL
		)  ON [PRIMARY]
		
	
	ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		PK_ExperiencePoint PRIMARY KEY CLUSTERED 
		(
		Id
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	
	ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		FK_ExperiencePoint_Player FOREIGN KEY
		(
		PlayerId
		) REFERENCES dbo.Player
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
	
	ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		FK_ExperiencePoint_Person FOREIGN KEY
		(
		PersonId
		) REFERENCES dbo.Person
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 

	ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		FK_ExperiencePoint_Session FOREIGN KEY
		(
		SessionId
		) REFERENCES dbo.Session
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 

	ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		FK_ExperiencePoint_Term FOREIGN KEY
		(
		TermId
		) REFERENCES dbo.Term
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
		
			ALTER TABLE dbo.ExperiencePoint ADD CONSTRAINT
		FK_ExperiencePoint_PersonAttribute FOREIGN KEY
		(
		PersonAttributeId
		) REFERENCES dbo.PersonAttribute
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 

		
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20060602,GetDate(),'Add Experience Point table');
END		