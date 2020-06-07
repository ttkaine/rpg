	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20060701
))
BEGIN
	CREATE TABLE dbo.DefaultPersonAttribute
		(
		Id int NOT NULL IDENTITY (1, 1),
		CampaignId int not null,
		[PersonAttributeTypeEnum] [int] NOT NULL,
		[Name] [nvarchar](256) NULL,
		[Description] [nvarchar](max) NULL,
		[InitialValue] [int] NOT NULL,
		[IsPrivate] [bit] NOT NULL,
		[IncludeForNpc] [bit] NOT NULL,		
		)  ON [PRIMARY]
		

	ALTER TABLE dbo.DefaultPersonAttribute ADD CONSTRAINT
		PK_DefaultPersonAttribute PRIMARY KEY CLUSTERED 
		(
		Id
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

		
		
	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20060701,GetDate(),'Add Default Attribute table');
END		