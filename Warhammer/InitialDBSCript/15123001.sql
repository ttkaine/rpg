IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15123001
))
BEGIN

INSERT INTO [dbo].[SiteFeature]
           ([Name]
           ,[Description]
           ,[IsEnabled])
     VALUES
           ('Public Leauge'
           ,'Make it so all the PCs are on the leauge for everyone.'
           ,0)
		   
		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15123001,GetDate(),'Adding Public League Feature');

END