IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15122702
))
BEGIN

CREATE TABLE dbo.Tmp_SiteFeature
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name nvarchar(250) NOT NULL,
	Description nvarchar(MAX) NULL,
	IsEnabled bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_SiteFeature SET (LOCK_ESCALATION = TABLE)

SET IDENTITY_INSERT dbo.Tmp_SiteFeature ON

IF EXISTS(SELECT * FROM dbo.SiteFeature)
	 EXEC('INSERT INTO dbo.Tmp_SiteFeature (Id, Name, Description, IsEnabled)
		SELECT Id, Name, Description, IsEnabled FROM dbo.SiteFeature WITH (HOLDLOCK TABLOCKX)')

SET IDENTITY_INSERT dbo.Tmp_SiteFeature OFF

DROP TABLE dbo.SiteFeature

EXECUTE sp_rename N'dbo.Tmp_SiteFeature', N'SiteFeature', 'OBJECT' 

ALTER TABLE dbo.SiteFeature ADD CONSTRAINT
	PK_SiteFeature PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

INSERT INTO [dbo].[SiteFeature]
           ([Name]
           ,[Description]
           ,[IsEnabled])
     VALUES
           ('SimpleStats'
           ,'Some simple stats for online game use - not the full warhammer stats system, just something simples'
           ,0)	
	
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15122702,GetDate(),'Add Pirate Stats Feature Row');

END

