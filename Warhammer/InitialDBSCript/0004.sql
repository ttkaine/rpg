IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 4
))
BEGIN

CREATE TABLE [dbo].[PageView](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PageId] [int] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[Viewed] [datetime] NOT NULL,
 CONSTRAINT [PK_PageView] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PageView]  WITH CHECK ADD  CONSTRAINT [FK_PageView_Page] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[PageView] CHECK CONSTRAINT [FK_PageView_Page]

ALTER TABLE [dbo].[PageView]  WITH CHECK ADD  CONSTRAINT [FK_PageView_Player] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Player] ([Id])

ALTER TABLE [dbo].[PageView] CHECK CONSTRAINT [FK_PageView_Player]



INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (4,GetDate(),'Link Pages to Players with a PageViewTable (for use in detecting new updates a player hasn''t seen');

END
