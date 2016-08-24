
IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 16062201
))
BEGIN


CREATE TABLE [dbo].[AdminSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SettingId] [int] NOT NULL,
	[SettingValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AdminSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (16062201,GetDate(),'Add admin settings table');

END