IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16082403
))
BEGIN

Update session set XpAwarded = '2016/01/01'
Update sessionlog set XpAwarded = '2016/01/01'

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16082403,GetDate(),'Set XpAwarded for all existing things');

END
