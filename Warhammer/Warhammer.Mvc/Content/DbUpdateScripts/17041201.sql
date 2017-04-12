IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17041201
))
BEGIN

ALTER TABLE [Trophy] ADD CampaignId int NULL 
	
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17041201,GetDate(),'Add nullable CampaginId to Trophy');

END