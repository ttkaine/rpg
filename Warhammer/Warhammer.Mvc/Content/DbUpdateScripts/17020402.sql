IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17020402
))
BEGIN

	CREATE TABLE [dbo].[CampaignDetails](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[CurrentGameDate] [datetime] NULL,
	 CONSTRAINT [PK_CampaignDetails] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT [dbo].[CampaignDetails] ([CurrentGameDate]) VALUES (CAST(N'2521-11-10 00:00:00.000' AS DateTime))

 
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17020402,GetDate(),'Add CampaignDetails table');

END