	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 20031501
))
BEGIN

  	ALTER TABLE dbo.Award ADD
		SessionId int NULL

	ALTER TABLE dbo.AwardNomination ADD
		SessionId int NULL
		

ALTER TABLE dbo.Award ADD CONSTRAINT
	FK_Award_Session FOREIGN KEY
	(
	SessionId
	) REFERENCES dbo.Session
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 


	 ALTER TABLE dbo.AwardNomination ADD CONSTRAINT
	FK_AwardNomination_Session FOREIGN KEY
	(
	SessionId
	) REFERENCES dbo.Session
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (20031501,GetDate(),'Link Awards to Sessions');

END		