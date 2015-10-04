IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 13
))
BEGIN

/****** Object:  Table [dbo].[Scene]    Script Date: 04/10/2015 20:09:47 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Scene](
	[Id] [int] NOT NULL,
	[LeadCharacterId] [int] NULL,
	[LocationId] [int] NULL,
	[DateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Scene] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


/****** Object:  Table [dbo].[ScenePost]    Script Date: 04/10/2015 20:09:47 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[ScenePost](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SceneId] [int] NOT NULL,
	[CharacterId] [int] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[TextContent] [nvarchar](max) NULL,
	[DateTime] [datetime] NOT NULL,
	[EditedDateTime] [datetime] NULL,
 CONSTRAINT [PK_ScenePost] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


ALTER TABLE [dbo].[Scene]  WITH CHECK ADD  CONSTRAINT [FK_Scene_Page] FOREIGN KEY([Id])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[Scene] CHECK CONSTRAINT [FK_Scene_Page]

ALTER TABLE [dbo].[Scene]  WITH CHECK ADD  CONSTRAINT [FK_Scene_Person] FOREIGN KEY([LeadCharacterId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[Scene] CHECK CONSTRAINT [FK_Scene_Person]

ALTER TABLE [dbo].[Scene]  WITH CHECK ADD  CONSTRAINT [FK_Scene_Place] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Place] ([Id])

ALTER TABLE [dbo].[Scene] CHECK CONSTRAINT [FK_Scene_Place]

ALTER TABLE [dbo].[ScenePost]  WITH CHECK ADD  CONSTRAINT [FK_ScenePost_Person] FOREIGN KEY([CharacterId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[ScenePost] CHECK CONSTRAINT [FK_ScenePost_Person]

ALTER TABLE [dbo].[ScenePost]  WITH CHECK ADD  CONSTRAINT [FK_ScenePost_Player] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Player] ([Id])

ALTER TABLE [dbo].[ScenePost] CHECK CONSTRAINT [FK_ScenePost_Player]

ALTER TABLE [dbo].[ScenePost]  WITH CHECK ADD  CONSTRAINT [FK_ScenePost_Scene] FOREIGN KEY([SceneId])
REFERENCES [dbo].[Scene] ([Id])

ALTER TABLE [dbo].[ScenePost] CHECK CONSTRAINT [FK_ScenePost_Scene]



	
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (13,GetDate(),'Adding Scene Tables');
	
END

