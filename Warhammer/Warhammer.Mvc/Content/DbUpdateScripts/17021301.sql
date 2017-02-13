IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17021301
))
BEGIN

CREATE TABLE [dbo].[Rumour](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[PlaceId] [int] NULL,
	[Title] [nvarchar](500) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Rumour] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Rumour]  WITH CHECK ADD  CONSTRAINT [FK_Rumour_Place] FOREIGN KEY([PlaceId])
REFERENCES [dbo].[Place] ([Id])

ALTER TABLE [dbo].[Rumour] CHECK CONSTRAINT [FK_Rumour_Place]
   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17021301,GetDate(),'Add rumour table');

END