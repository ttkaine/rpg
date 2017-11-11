
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17081801
))
BEGIN

ALTER TABLE dbo.AwardNomination ADD
	AwardId int NULL

ALTER TABLE dbo.AwardNomination ADD CONSTRAINT
	FK_AwardNomination_Award FOREIGN KEY
	(
	AwardId
	) REFERENCES dbo.Award
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

INSERT INTO [dbo].[Setting]
           ([SectionId]
           ,[Name]
           ,[DisplayName]
           ,[Description]
           ,[TrueText]
           ,[FalseText])
     VALUES
           (4
           ,'PrivateAwardNominations'
           ,'Keep my award monimations private'
           ,'Private award nominations will not be shown on the site'
           ,'Private'
           ,'Public')

ALTER TABLE dbo.AwardNomination ADD
	IsPrivate bit NOT NULL CONSTRAINT DF_AwardNomination_IsPrivate DEFAULT 0

		INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17081801,GetDate(),'Add a private flag for nominations');

END