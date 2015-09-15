IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 10
))
BEGIN
CREATE TABLE [dbo].[Comment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[PersonId] [int] NULL,
	[PageId] [int] NOT NULL,
	[IsAdmin] [bit] NOT NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Page] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Page]

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Person]

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Player] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Player] ([Id])

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Player]

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (10,GetDate(),'Adding table for comments');

END