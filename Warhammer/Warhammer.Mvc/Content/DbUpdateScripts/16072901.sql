
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16072901
))
BEGIN

ALTER TABLE dbo.Page ADD
	XpAwarded int NULL

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16072901,GetDate(),'Add Xp Awarded column to Page');

END