	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20060601
))
BEGIN

	CREATE TABLE dbo.Term
		(
		Id int NOT NULL IDENTITY (1, 1),
		PlayerId int NOT NULL,
		PersonId int NOT NULL,
		TermNumber int NOT NULL,
		RolledStatId int NULL,
		RollResultEnum int NULL,
		Question NVARCHAR(MAX) NULL,
		Answer NVARCHAR(MAX) NULL,
		Outcome NVARCHAR(MAX) NULL,
		)  ON [PRIMARY]
		
	
	ALTER TABLE dbo.Term ADD CONSTRAINT
		PK_Term PRIMARY KEY CLUSTERED 
		(
		Id
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	
	ALTER TABLE dbo.Term ADD CONSTRAINT
		FK_Term_Player FOREIGN KEY
		(
		PlayerId
		) REFERENCES dbo.Player
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
	
	ALTER TABLE dbo.Term ADD CONSTRAINT
		FK_Term_Person FOREIGN KEY
		(
		PersonId
		) REFERENCES dbo.Person
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
				ALTER TABLE dbo.Term ADD CONSTRAINT
		FK_Term_RolledStatId FOREIGN KEY
		(
		RolledStatId
		) REFERENCES dbo.PersonAttribute
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20060601,GetDate(),'Add Term table');
END		