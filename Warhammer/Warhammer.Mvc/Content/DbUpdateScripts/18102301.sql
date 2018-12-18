IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18102301
))
BEGIN

	ALTER TABLE dbo.CampaignDetails ADD
		ThemeId int NULL

	ALTER TABLE dbo.CampaignDetails ADD CONSTRAINT
		DF_CampaignDetails_ThemeId DEFAULT 0 FOR ThemeId
		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18102301,GetDate(),'Add theme Id for CampaignDetails');

END					