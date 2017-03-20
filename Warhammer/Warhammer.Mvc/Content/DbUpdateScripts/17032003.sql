IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 17032003
))
BEGIN

INSERT [dbo].[Setting] ([SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (3, N'ShadowMode', N'Shadow Mode', N'Live in the shadows, only getting glimpses of the world. You only see things linked to your characters.', N'Shadow', N'Light')



  INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (17032003,GetDate(),'Add Shadow Mode User Setting');

END