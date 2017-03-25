

IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17032501
))
BEGIN


CREATE TABLE [dbo].[PersonAttribute](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[PersonAttributeTypeEnum] [int] NOT NULL,
	[Name] [nvarchar](256) NULL,
	[Description] [nvarchar](max) NULL,
	[InitialValue] [int] NOT NULL,
	[CurrentValue] [int] NOT NULL,
	[XpSpent] [int] NOT NULL,
	[CampaignId] [int] NOT NULL,
 CONSTRAINT [PK_PersonAttribute] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]



ALTER TABLE [dbo].[PersonAttribute]  WITH CHECK ADD  CONSTRAINT [FK_PersonAttribute_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([Id])


ALTER TABLE [dbo].[PersonAttribute] CHECK CONSTRAINT [FK_PersonAttribute_Person]



ALTER TABLE Person ADD TotalAdvancesTaken int NOT NULL CONSTRAINT DefaulttotalAdvances_Person DEFAULT 0

  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17032501,GetDate(),'Add PersonAttribute Table');

END