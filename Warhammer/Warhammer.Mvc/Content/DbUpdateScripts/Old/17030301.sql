
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17030301
))
BEGIN


CREATE TABLE [dbo].[Asset](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NULL,
	[Title] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Upkeep] [int] NOT NULL,
 CONSTRAINT [PK_Asset] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


ALTER TABLE [dbo].[Asset]  WITH CHECK ADD  CONSTRAINT [FK_Asset_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE

ALTER TABLE  [dbo].[Person]
  ADD 	[Upkeep] [int] NULL

   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17030301,GetDate(),'Add asset table');

END