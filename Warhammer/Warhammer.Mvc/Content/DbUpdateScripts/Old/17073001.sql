IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17073001
))
BEGIN


CREATE TABLE [dbo].[AwardNomination](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[TrophyId] [int] NOT NULL,
	[NominationReason] [nvarchar](500) NULL,
	[RejectedReason] [nvarchar](500) NULL,
	[AcceptedReason] [nvarchar](500) NULL,
	[AwardedOn] [datetime] NULL,
	[RejectedOn] [datetime] null,
	[NominatedById] [int] NULL,
	[NominatedDate] [datetime] NULL,
	[CampaignId] [int] NOT NULL CONSTRAINT [DefaultCampaign_AwardNomination]  DEFAULT ((1)),
 CONSTRAINT [PK_AwardNomination] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]



ALTER TABLE [dbo].[AwardNomination]  WITH CHECK ADD  CONSTRAINT [FK_AwardNomination_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])


ALTER TABLE [dbo].[AwardNomination] CHECK CONSTRAINT [FK_AwardNomination_Person]


ALTER TABLE [dbo].[AwardNomination]  WITH CHECK ADD  CONSTRAINT [FK_AwardNomination_Player] FOREIGN KEY([NominatedById])
REFERENCES [dbo].[Player] ([Id])


ALTER TABLE [dbo].[AwardNomination] CHECK CONSTRAINT [FK_AwardNomination_Player]


ALTER TABLE [dbo].[AwardNomination]  WITH CHECK ADD  CONSTRAINT [FK_AwardNomination_Trophy] FOREIGN KEY([TrophyId])
REFERENCES [dbo].[Trophy] ([Id])


ALTER TABLE [dbo].[AwardNomination] CHECK CONSTRAINT [FK_AwardNomination_Trophy]




	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17073001,GetDate(),'add award nomination table');

END
