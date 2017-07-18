IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17032001
))
BEGIN

ALTER TABLE [UserSetting] ADD CampaignId int NOT NULL CONSTRAINT DefaultCampaign_UserSetting DEFAULT 1


  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17032001,GetDate(),'Make User Settings Site Specific');

END