
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16010201
))
BEGIN


CREATE TABLE dbo.UserSetting
	(
	Id int NOT NULL IDENTITY (1, 1),
	PlayerId int NOT NULL,
	SettingId int NOT NULL,
	Enabled bit NOT NULL,
	Data nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.UserSetting ADD CONSTRAINT
	PK_UserSetting PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


	ALTER TABLE dbo.UserSetting ADD CONSTRAINT
	FK_UserSetting_Player FOREIGN KEY
	(
	PlayerId
	) REFERENCES dbo.Player
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

INSERT INTO [dbo].[SiteFeature]
           ([Name]
           ,[Description]
           ,[IsEnabled])
     VALUES
           ('UserSettings'
           ,'Show the User Settings Page.'
           ,0)
		   
		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16010201,GetDate(),'User Settings Feature');

END