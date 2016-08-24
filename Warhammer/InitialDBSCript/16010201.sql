
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

	
INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('UserSettings'
           ,'User Settings area for users to set things the way they like them'
           ,0)

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('CharacterLeague'
           ,'Show the character league page'
           ,1)		   

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('Graveyard'
           ,'include a graveyard page'
           ,1)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('CharacterSheet'
           ,'show the warhammer character sheet'
           ,1)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('WarhammerMap'
           ,'show the warhammer map'
           ,1)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('TrophyCabinet'
           ,'include the trophy cabinet page'
           ,1)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('SessionPage'
           ,'include a list of sessions page'
           ,1)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('PublicLeague'
           ,'make the league public all over'
           ,0)	

INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('ImmediateEmailer'
           ,'Send Email Immediately when somethings happens'
           ,0)	
		   
INSERT INTO [dbo].[SiteFeature] ([Name] ,[Description] ,[IsEnabled])
     VALUES
           ('NightlyEmailer'
           ,'Send email overnight on a schedule'
           ,0)	
		   
		   		   		   
		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16010201,GetDate(),'Adding User SEttings tables and additional Feature settings');

END