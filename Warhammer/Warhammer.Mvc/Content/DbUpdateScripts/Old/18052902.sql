
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18052902
))
BEGIN

UPDATE [dbo].[CampaignDetails] SET  [Url] = 'warhammer.sendingofeight.co.uk' WHERE CampaignId = 1
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'fatehammer.sendingofeight.co.uk' WHERE CampaignId = 2
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'darknet.sendingofeight.co.uk' WHERE CampaignId = 3
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'playground.sendingofeight.co.uk' WHERE CampaignId = 4
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'cowboy.sendingofeight.co.uk' WHERE CampaignId = 5
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'mouse.sendingofeight.co.uk' WHERE CampaignId = 6
UPDATE [dbo].[CampaignDetails] SET  [Url] = 'writing.sendingofeight.co.uk' WHERE CampaignId = 7

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18052902,GetDate(),'Set up existing Campaign Urls');

END		