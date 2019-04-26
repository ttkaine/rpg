	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19042601
))
BEGIN

CREATE TABLE dbo.SiteIcon
	(
	Id int NOT NULL IDENTITY (1, 1),
	CampaignId int NOT NULL,
	Size int NOT NULL,
	Data varbinary(MAX) NOT NULL

	)  ON [PRIMARY]

ALTER TABLE dbo.SiteIcon ADD CONSTRAINT
	PK_SiteIcon PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19042601,GetDate(),'Add Site Icon table');

END		