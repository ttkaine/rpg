IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17040501
))
BEGIN

INSERT [dbo].[CampaignDetails] ([CurrentGameDate], [CampaignId], [GmId]) VALUES (CAST(N'1865-01-20 00:00:00.000' AS DateTime), 5, 2)

  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17040501,GetDate(),'Add Shadow Mode User Setting');

END