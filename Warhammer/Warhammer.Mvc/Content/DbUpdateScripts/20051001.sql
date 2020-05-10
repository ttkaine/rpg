	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20051001
))
BEGIN

	CREATE TABLE dbo.PlayerSecrets
		(
		Id int NOT NULL IDENTITY (1, 1),
		PlayerId int NOT NULL,
		PageId int NOT NULL,
		Details nvarchar(MAX) NULL
		)  ON [PRIMARY]
		 TEXTIMAGE_ON [PRIMARY]
	
	ALTER TABLE dbo.PlayerSecrets ADD CONSTRAINT
		PK_PlayerSecrets PRIMARY KEY CLUSTERED 
		(
		Id
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	
	ALTER TABLE dbo.PlayerSecrets ADD CONSTRAINT
		FK_PlayerSecrets_Player FOREIGN KEY
		(
		PlayerId
		) REFERENCES dbo.Player
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
	
	
	ALTER TABLE dbo.PlayerSecrets ADD CONSTRAINT
		FK_PlayerSecrets_Page FOREIGN KEY
		(
		PageId
		) REFERENCES dbo.Page
		(
		Id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 

		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20051001,GetDate(),'Add Player Secrets to Page');
END		