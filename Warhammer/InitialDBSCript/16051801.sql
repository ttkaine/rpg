
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16051801
))
BEGIN


CREATE TABLE [dbo].[FateAspect](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[AspectType] [int] NOT NULL,
	[AspectName] [nvarchar](500) NOT NULL,
	[IsVisible] [bit] NOT NULL,
 CONSTRAINT [PK_FateAspect] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[FateStat](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[StatType] [int] NOT NULL,
	[StatValue] [int] NOT NULL,
	[IsVisible] [bit] NOT NULL,
 CONSTRAINT [PK_FateStat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[FateStunt](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[Title] [nvarchar](500) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsVisible] [bit] NOT NULL,
 CONSTRAINT [PK_FateStunt] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


ALTER TABLE [dbo].[FateAspect] ADD  CONSTRAINT [DF_FateAspect_IsVisible]  DEFAULT ((0)) FOR [IsVisible]

ALTER TABLE [dbo].[FateStat] ADD  CONSTRAINT [DF_FateStat_IsVisible]  DEFAULT ((0)) FOR [IsVisible]

ALTER TABLE [dbo].[FateStunt] ADD  CONSTRAINT [DF_FateStunt_IsVisible]  DEFAULT ((0)) FOR [IsVisible]

ALTER TABLE [dbo].[FateAspect]  WITH CHECK ADD  CONSTRAINT [FK_FateAspect_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[FateAspect] CHECK CONSTRAINT [FK_FateAspect_Person]

ALTER TABLE [dbo].[FateStat]  WITH CHECK ADD  CONSTRAINT [FK_FateStat_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[FateStat] CHECK CONSTRAINT [FK_FateStat_Person]

ALTER TABLE [dbo].[FateStunt]  WITH CHECK ADD  CONSTRAINT [FK_FateStunt_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[FateStunt] CHECK CONSTRAINT [FK_FateStunt_Person]

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16051801,GetDate(),'Add Fate Stat Tables');

END