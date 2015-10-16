IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 13
))
BEGIN

CREATE TABLE [dbo].[Post](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [int] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[CharacterId] [int] NULL,
	[PostType] [int] NOT NULL,
	[OriginalContent] [nvarchar](max) NOT NULL,
	[RevisedContent] [nvarchar](max) NOT NULL CONSTRAINT [DF_Post_RevisedContent]  DEFAULT (' '),
	[TargetPlayerIds] [nvarchar](250) NULL,
	[DatePosted] [datetime] NOT NULL,
	[DieSize] [int] NOT NULL,
	[DieCount] [int] NOT NULL,
	[RollType] [int] NOT NULL CONSTRAINT [DF_Post_RollType]  DEFAULT ((0)),
	[RollTarget] [int] NOT NULL CONSTRAINT [DF_Post_RollTarget]  DEFAULT ((0)),
	[RollValues] [nvarchar](250) NOT NULL,
	[ReRollMaximums] [bit] NOT NULL CONSTRAINT [DF_Post_RerollMaximums]  DEFAULT ((0)),
	[IsRevised] [bit] NOT NULL CONSTRAINT [DF_Post_IsRevised]  DEFAULT ((0)),
	[LastEdited] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL CONSTRAINT [DF_Post_IsDeleted]  DEFAULT ((0)),
	[DeletedDate] [datetime] NULL,
 CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [SESSION] ADD
[IsClosed] [bit] NOT NULL CONSTRAINT [DF_Session_IsClosed]  DEFAULT ((0))

ALTER TABLE [SESSION] ADD
[IsTextSession] [bit] NOT NULL CONSTRAINT [DF_Session_IsTextSession]  DEFAULT ((0))

ALTER TABLE [PLAYER] ADD
[IsGm] [bit] NOT NULL CONSTRAINT [DF_Player_IsGm]  DEFAULT ((0))


ALTER TABLE dbo.Post ADD CONSTRAINT
	FK_Post_Player FOREIGN KEY
	(
	PlayerId
	) REFERENCES dbo.Player
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
ALTER TABLE dbo.Post ADD CONSTRAINT
	FK_Post_Session FOREIGN KEY
	(
	SessionId
	) REFERENCES dbo.Session
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (13, GetDate(),'Additions for Forum');
	
END
