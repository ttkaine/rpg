
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17031703
))
BEGIN

INSERT [dbo].[CampaignDetails] ([CurrentGameDate], [CampaignId]) VALUES (CAST(N'2521-11-10 00:00:00.000' AS DateTime), 3)

INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'SimpleStats', N'Some simple stats for online game use - not the full warhammer stats system, just something simples', 1, 3)

INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'UserSettings', N'User Settings area for users to set things the way they like them', 1, 3)

INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CharacterLeague', N'Show the character league page', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'Graveyard', N'include a graveyard page', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CharacterSheet', N'show the warhammer character sheet', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'WarhammerMap', N'show the warhammer map', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'TrophyCabinet', N'include the trophy cabinet page', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'SessionPage', N'include a list of sessions page', 1, 3)

INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PublicLeague', N'make the league public all over', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'ImmediateEmailer', N'Send Email Immediately when somethings happens', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'NightlyEmailer', N'Send email overnight on a schedule', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'AdminLeague', N'Admin only', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'AwardHistory', N'AwardHistory', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'AutoPopulatePeopleInNewSessions', N'AutoPopulatePeopleInNewSessions', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'ScoreHistory', N'ScoreHistory', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'FateStats', N'FateStats', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CurrentScorePie', N'CurrentScorePie', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'SimpleAutoXp', N'SimpleAutoXp', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'SimpleHitPoints', N'SimpleHitPoints', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CrowRules', N'CrowRules', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'Bestiary', N'Bestiary', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PriceList', N'PriceList', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PublicPrices', N'PublicPrices', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PersonDetails', N'PersonDetails', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CrowTopStatsPanel', N'CrowTopStatsPanel', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CrowStats', N'CrowStats', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'FuHammerStats', N'FuHammerStats', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PersonDescriptors', N'PersonDescriptors', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PersonRoles', N'PersonRoles', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'PersonAspects', N'PersonAspects', 1, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'RumourMill', N'RumourMill', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'CrowNpcSheet', N'CrowNpcSheet', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'ShowGameDate', N'ShowGameDate', 0, 3)
                                                                                      
INSERT [dbo].[SiteFeature] ([Name], [Description], [IsEnabled], [CampaignId]) VALUES (N'Assets', N'Assets', 1, 3)


   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17031703,GetDate(),'Add DarkNet Meta Table Data');

END

