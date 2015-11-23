IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15112301
))
BEGIN

ALTER TABLE dbo.Session ADD
	IsPrivate bit NOT NULL CONSTRAINT DF_Session_IsPrivate DEFAULT 0,
	IsGmTurn bit NOT NULL CONSTRAINT DF_Session_IsGmTurn DEFAULT 0

CREATE TABLE [dbo].[PostOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [int] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[LastTurnEnded] [datetime] NOT NULL,
 CONSTRAINT [PK_PostOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[PostOrder]  WITH CHECK ADD  CONSTRAINT [FK_PostOrder_Player] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[Player] ([Id])

ALTER TABLE [dbo].[PostOrder] CHECK CONSTRAINT [FK_PostOrder_Player]

ALTER TABLE [dbo].[PostOrder]  WITH CHECK ADD  CONSTRAINT [FK_PostOrder_Session] FOREIGN KEY([SessionId])
REFERENCES [dbo].[Session] ([Id])

ALTER TABLE [dbo].[PostOrder] CHECK CONSTRAINT [FK_PostOrder_Session]
 
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15112301,GetDate(),'Adding post Order table and bools');

END