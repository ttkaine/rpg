IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15122701
))
BEGIN

CREATE TABLE dbo.SiteFeature
	(
	Id int NOT NULL,
	Name nvarchar(250) NOT NULL,
	Description nvarchar(MAX) NULL,
	IsEnabled bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.SiteFeature ADD CONSTRAINT
	PK_SiteFeature PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
 
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15122701,GetDate(),'Adding SiteFeature table');

END

