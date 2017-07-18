IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17071801
))
BEGIN

INSERT [dbo].[CampaignDetails] ([CurrentGameDate], [CampaignId]) VALUES (CAST(N'1853-11-10 00:00:00.000' AS DateTime), 6)

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17071801,GetDate(),'add a campaign for the mouse');

END