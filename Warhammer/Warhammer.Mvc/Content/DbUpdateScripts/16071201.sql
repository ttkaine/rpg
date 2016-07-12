IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16071201
))
BEGIN

CREATE TABLE [dbo].[ExceptionLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Severity] [int] NOT NULL,
	[Message] [nvarchar](max) NULL,
	[DateTime] [datetime] NOT NULL,
	[Timestamp] [timestamp] NOT NULL,
	[UserId] [int] NULL,
	[StackTrace] [nvarchar](max) NULL,
	[ExceptionType] [nvarchar](250) NULL,
	[ExecutingAssembly] [nvarchar](250) NULL,
	[Sequence] [int] NOT NULL,
	[Identifier] [nvarchar](50) NOT NULL,
	[EntityId] [int] NULL,
 CONSTRAINT [PK_ExceptionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16071201,GetDate(),'Add exception log table');

END