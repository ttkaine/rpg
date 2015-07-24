IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 7
))
BEGIN

CREATE TABLE [dbo].[Award](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[TrophyId] [int] NOT NULL,
	[Reason] [nvarchar](500) NULL,
	[AwardedOn] [datetime] NOT NULL,
	[NominatedById] [int] NULL,
	[NominatedDate] [datetime] NULL,
 CONSTRAINT [PK_Award] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Trophy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ImageData] [varbinary](max) NULL,
	[MimeType] [nvarchar](150) NULL,
	[PointsValue] [int] NOT NULL CONSTRAINT DF_Person_PointsValue DEFAULT 0,
 CONSTRAINT [PK_Trophy] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Award]  WITH CHECK ADD  CONSTRAINT [FK_Award_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
 
ALTER TABLE [dbo].[Award] CHECK CONSTRAINT [FK_Award_Person]
 
ALTER TABLE [dbo].[Award]  WITH CHECK ADD  CONSTRAINT [FK_Award_Player] FOREIGN KEY([NominatedById])
REFERENCES [dbo].[Player] ([Id])
 
ALTER TABLE [dbo].[Award] CHECK CONSTRAINT [FK_Award_Player]
 
ALTER TABLE [dbo].[Award]  WITH CHECK ADD  CONSTRAINT [FK_Award_Trophy] FOREIGN KEY([TrophyId])
REFERENCES [dbo].[Trophy] ([Id])
 
ALTER TABLE [dbo].[Award] CHECK CONSTRAINT [FK_Award_Trophy]


INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (7,GetDate(),'Adding trophy tables');

END
