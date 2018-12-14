IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 18120901
))
BEGIN
CREATE TABLE [dbo].[ScoreBreakdown](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[PersonId] [int] NOT NULL,
	[ScoreTypeId] [int] NOT NULL,
	[PointsValue] [decimal](16, 2) NOT NULL,
	[CampaignId] [int] NOT NULL,
 CONSTRAINT [PK_ScoreBreakdown] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE dbo.ScoreBreakdown ADD CONSTRAINT
	FK_ScoreBreakdown_Person FOREIGN KEY
	(
	PersonId
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 


		
		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (18120901,GetDate(),'Add score breakdown table');

END				