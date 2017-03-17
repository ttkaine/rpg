
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17031701
))
BEGIN


ALTER TABLE PostOrder ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_PostOrder DEFAULT 1
ALTER TABLE SiteFeature ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_SiteFeature DEFAULT 1
ALTER TABLE PersonStat ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_PersonStat DEFAULT 1
ALTER TABLE Page ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Page DEFAULT 1
ALTER TABLE PageImage ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_PageImage DEFAULT 1
ALTER TABLE CreatureAbility ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_CreatureAbility DEFAULT 1
ALTER TABLE ScoreHistory ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_ScoreHistory DEFAULT 1
ALTER TABLE FateAspect ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_FateAspect DEFAULT 1
ALTER TABLE FateStat ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_FateStat DEFAULT 1
ALTER TABLE Reputation ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Reputation DEFAULT 1
ALTER TABLE FateStunt ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_FateStunt DEFAULT 1
ALTER TABLE SimpleHitPoints ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_SimpleHitPoints DEFAULT 1
ALTER TABLE PriceListItem ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_PriceListItem DEFAULT 1
ALTER TABLE PageView ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_PageView DEFAULT 1
ALTER TABLE AdminSetting ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_AdminSetting DEFAULT 1
ALTER TABLE CampaignDetails ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_CampaignDetails DEFAULT 1
ALTER TABLE Award ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Award DEFAULT 1
ALTER TABLE Rumour ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Rumour DEFAULT 1
ALTER TABLE Comment ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Comment DEFAULT 1
ALTER TABLE Post ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Post DEFAULT 1
ALTER TABLE Asset ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_Asset DEFAULT 1
ALTER TABLE CampaignDetails ADD GmId int NOT NULL CONSTRAINT DefaultGm_CampaignDetails DEFAULT 1
ALTER TABLE Player DROP [DF_Player_IsGm]
ALTER TABLE Player DROP Column IsGM;

CREATE TABLE [dbo].[PlayerCampaign](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PlayerId] [int] NOT NULL,
	[CampaginId] [int] NOT NULL,
	[PlayerModeEnum] [int] NOT NULL,
 CONSTRAINT [PK_PlayerCampaign] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PlayerCampaign]  WITH CHECK ADD  CONSTRAINT [FK_PlayerCampaign_CampaignDetails] FOREIGN KEY([CampaginId])
REFERENCES [dbo].[CampaignDetails] ([Id])

ALTER TABLE [dbo].[PlayerCampaign] CHECK CONSTRAINT [FK_PlayerCampaign_CampaignDetails]

ALTER TABLE [dbo].[PlayerCampaign]  WITH CHECK ADD  CONSTRAINT [FK_PlayerCampaign_Player] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Player] ([Id])

ALTER TABLE [dbo].[PlayerCampaign] CHECK CONSTRAINT [FK_PlayerCampaign_Player]

   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17031701,GetDate(),'MultiCampaign Setup');

END
