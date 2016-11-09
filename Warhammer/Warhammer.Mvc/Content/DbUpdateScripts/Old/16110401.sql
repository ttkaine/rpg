IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16110401
))
BEGIN

ALTER TABLE PERSON 
ADD [XPAwarded] [decimal](16, 2) NOT NULL 
CONSTRAINT [DF_Person_ExperiencePointsAwarded]  DEFAULT ((0.0))


INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16110401,GetDate(),'Add ourselves a new decimal XP column');

END