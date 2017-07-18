IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17042401
))
BEGIN

ALTER TABLE [Person] ADD LastScoreCalculation datetime NULL 
ALTER TABLE [Person] ADD XpSpendAvailable bit NOT NULL CONSTRAINT XpSpendAvailable_Person DEFAULT 0
ALTER TABLE [Person] ADD WishingWell int NOT NULL CONSTRAINT WishingWell_Person DEFAULT 0
	
  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17042401,GetDate(),'Add LastScoreCalculation, XpSpendAvailable and WishingWell to Person');

END