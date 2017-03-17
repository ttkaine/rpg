IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17031704
))
BEGIN


delete from .[ScoreHistory]


   
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17031704,GetDate(),'Clear Out Score History Table');

END

