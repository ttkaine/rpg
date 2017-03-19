IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16112501
))
BEGIN

CREATE TABLE [dbo].[PageImage](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PageId] [int] NOT NULL,
	[IsPrimary] [bit] NOT NULL,
	[Data] [varbinary](MAX) NULL,
 CONSTRAINT [PK_PageImage] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[PageImage]  WITH CHECK ADD  CONSTRAINT [FK_PageImage_Page] FOREIGN KEY([PageId])
REFERENCES [dbo].[Page] ([Id])

ALTER TABLE [dbo].[PageImage] CHECK CONSTRAINT [FK_PageImage_Page]



   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16112501,GetDate(),'Add an Image table for Page Images');

END