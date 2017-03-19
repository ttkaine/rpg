IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17020401
))
BEGIN

	ALTER TABLE [dbo].[Person] ADD [Crowns] [int] NULL
	ALTER TABLE [dbo].[Person] ADD [Shillings] [int] NULL
	ALTER TABLE [dbo].[Person] ADD [Pennies] [int] NULL
	ALTER TABLE [dbo].[Person] ADD [DateOfBirth] [datetime] NULL
	ALTER TABLE [dbo].[Person] ADD [Height] [nvarchar](max) NULL
 
   INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17020401,GetDate(),'Add money and DoB to Person');

END