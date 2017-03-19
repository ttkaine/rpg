IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17012001
))
BEGIN

	CREATE TABLE [dbo].[PriceListItem](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ParentId] [int] NULL,
		[Name] [nvarchar](500) NULL,
		[PriceInPence] [decimal](18, 2) NULL,
		[Description] [nvarchar](max) NULL,
	 CONSTRAINT [PK_PriceListItem] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[PriceListItem]  WITH CHECK ADD  CONSTRAINT [FK_PriceListItem_PriceListItem] FOREIGN KEY([ParentId])
	REFERENCES [dbo].[PriceListItem] ([Id])

	ALTER TABLE [dbo].[PriceListItem] CHECK CONSTRAINT [FK_PriceListItem_PriceListItem]



   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17012001,GetDate(),'Add PriceListItem table');

END

