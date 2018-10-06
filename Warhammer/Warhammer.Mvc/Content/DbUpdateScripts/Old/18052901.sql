IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18052901
))
BEGIN

	ALTER TABLE dbo.CampaignDetails ADD [CustomCss] [nvarchar](max) NULL
	
	ALTER TABLE dbo.CampaignDetails ADD [Url] [nvarchar](200) NULL
	
	ALTER TABLE dbo.CampaignDetails ADD [BackgroundImage] [varbinary](max) NULL
	
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18052901,GetDate(),'Add Campaign URL, CSS, and Image');

END		