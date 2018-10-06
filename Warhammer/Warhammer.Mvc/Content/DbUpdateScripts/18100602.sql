IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18100602
))
BEGIN

INSERT INTO [dbo].[PlayerCampaign]
           ([PlayerId]
           ,[CampaginId]
           ,[PlayerModeEnum]
           ,[ShowInGlobal])
SELECT  p.id, c.CampaignId, 1, 0 from
[dbo].[Player] p, [dbo].[CampaignDetails] c

		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18100602,GetDate(),'Set up Players to Campaigns');

END	