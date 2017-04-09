IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17040901
))
BEGIN


ALTER TABLE dbo.CampaignDetails ADD
	AverageStat int NULL
	
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17040901,GetDate(),'Add AverageStat to Campaign Details');

END