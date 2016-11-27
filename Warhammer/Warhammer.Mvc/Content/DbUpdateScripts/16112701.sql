IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16112701
))
BEGIN



CREATE TABLE [dbo].[Creature](
	[Id] [int] NOT NULL,
	[ParentType] [int] NULL,
	[ThreatLevelId] [int] NOT NULL,
 CONSTRAINT [PK_Creature] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[CreatureAbility](
	[Id] [int] NOT NULL,
	[CreatureId] [int] NOT NULL,
	[Name] [nvarchar](500) NULL,
	[Description] [nvarchar](max) NULL,
	[AbilityThreatLevelId] [int] NOT NULL,
 CONSTRAINT [PK_CreatureAbility] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


CREATE TABLE [dbo].[Organisation](
	[Id] [int] NOT NULL,
	[OrganisationType] [int] NOT NULL,
 CONSTRAINT [PK_Organisation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Reputation](
	[Id] [int] NOT NULL,
	[WearLevelId] [int] NOT NULL,
	[HarmLevelId] [int] NOT NULL,
	[Xp] [decimal](18, 2) NOT NULL,
	[Title] [nvarchar](500) NULL,
	[Description] [nvarchar](max) NULL,
	[PersonId] [int] NULL,
	[OrganisationId] [int] NULL,
	[CreatureId] [int] NULL,
 CONSTRAINT [PK_Reputation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


ALTER TABLE [dbo].[Creature]  WITH CHECK ADD  CONSTRAINT [FK_Creature_Creature] FOREIGN KEY([ParentType])
REFERENCES [dbo].[Creature] ([Id])

ALTER TABLE [dbo].[Creature] CHECK CONSTRAINT [FK_Creature_Creature]

ALTER TABLE [dbo].[Creature]  WITH CHECK ADD  CONSTRAINT [FK_Creature_Page] FOREIGN KEY([Id])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[Creature] CHECK CONSTRAINT [FK_Creature_Page]

ALTER TABLE [dbo].[CreatureAbility]  WITH CHECK ADD  CONSTRAINT [FK_CreatureAbility_Creature] FOREIGN KEY([CreatureId])
REFERENCES [dbo].[Creature] ([Id])

ALTER TABLE [dbo].[CreatureAbility] CHECK CONSTRAINT [FK_CreatureAbility_Creature]

ALTER TABLE [dbo].[Organisation]  WITH CHECK ADD  CONSTRAINT [FK_Organisation_Page] FOREIGN KEY([Id])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[Organisation] CHECK CONSTRAINT [FK_Organisation_Page]

ALTER TABLE [dbo].[Reputation]  WITH CHECK ADD  CONSTRAINT [FK_Reputation_Creature] FOREIGN KEY([CreatureId])
REFERENCES [dbo].[Creature] ([Id])

ALTER TABLE [dbo].[Reputation] CHECK CONSTRAINT [FK_Reputation_Creature]

ALTER TABLE [dbo].[Reputation]  WITH CHECK ADD  CONSTRAINT [FK_Reputation_Organisation] FOREIGN KEY([OrganisationId])
REFERENCES [dbo].[Organisation] ([Id])

ALTER TABLE [dbo].[Reputation] CHECK CONSTRAINT [FK_Reputation_Organisation]

ALTER TABLE [dbo].[Reputation]  WITH CHECK ADD  CONSTRAINT [FK_Reputation_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[Reputation] CHECK CONSTRAINT [FK_Reputation_Person]


   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16112701,GetDate(),'Add Reputations and Organisations and Creatures');

END

