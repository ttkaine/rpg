IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 5
))
BEGIN

delete from [dbo].[PageView]
INSERT INTO [dbo].[PageView]
           ([PageId]
           ,[PlayerId]
           ,[Viewed])
select p.Id,pl.Id,GEtDate() from [Page] p, [Player] pl 

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (5,GetDate(),'Mark all current pages as SEEN');

END
