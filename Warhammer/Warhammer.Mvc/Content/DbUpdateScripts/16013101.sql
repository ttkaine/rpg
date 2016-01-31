

IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16013101
))
BEGIN


CREATE TABLE [dbo].[Setting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NOT NULL,
	[Name] [nvarchar](250) NULL,
	[DisplayName] [nvarchar](250) NULL,
	[Description] [nvarchar](max) NULL,
	[TrueText] [nvarchar](50) NULL,
	[FalseText] [nvarchar](50) NULL,
 CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


SET IDENTITY_INSERT [dbo].[Setting] ON 


INSERT [dbo].[Setting] ([Id], [SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (1, 1, N'SendEmailOnMyTurn', N'Notify me when it is my turn', N'When it becomes your turn in a TEXT session, an email will be sent to let you know to check the site', N'Yes', N'No')

INSERT [dbo].[Setting] ([Id], [SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (2, 1, N'SendEmailOnNewPage', N'Notify me when someone creates a new page', N'When somebody else creates a new page the site will let you know', N'Yes', N'No')

INSERT [dbo].[Setting] ([Id], [SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (3, 1, N'SendEmailOnNewComment', N'Notify me when somebody comments', N'If someone posts a comment on the site then you''ll get it in an email', N'Yes', N'No')

INSERT [dbo].[Setting] ([Id], [SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (4, 1, N'SendEmailOnUpdatePage', N'Notify me when sombody updates a page', N'If someone updates a page you''ll get an email to let you know', N'Yes', N'No')

INSERT [dbo].[Setting] ([Id], [SectionId], [Name], [DisplayName], [Description], [TrueText], [FalseText]) VALUES (5, 1, N'SendDailySummaryEmail', N'Send me a daily summary of updates', N'Once per day you''ll get send turn reminders and a quick summary of updates so you can see what''s going on', N'Yes', N'No')

SET IDENTITY_INSERT [dbo].[Setting] OFF


		    
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16013101,GetDate(),'Add Settings Table to store the settings definitions');

END
