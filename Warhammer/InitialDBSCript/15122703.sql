IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 15122703
))
BEGIN

ALTER TABLE dbo.Person ADD
	Roles nvarchar(MAX) NULL,
	Descriptors nvarchar(MAX) NULL,
	CurrentXp int NOT NULL CONSTRAINT DF_Person_CurrentXp DEFAULT 0
	
CREATE TABLE dbo.PersonStat
	(
	Id int NOT NULL IDENTITY (1, 1),
	PersonId int NOT NULL,
	StatId int NOT NULL,
	InitialValue int NOT NULL,
	CurrentValue int NOT NULL,
	XpSpent int NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	DF_PersonStat_StatId DEFAULT 0 FOR StatId

ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	DF_PersonStat_InitialValue DEFAULT 0 FOR InitialValue

ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	DF_PersonStat_CurrentValue DEFAULT 0 FOR CurrentValue

ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	DF_PersonStat_XpSpent DEFAULT 0 FOR XpSpent

ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	PK_PersonStat PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	
ALTER TABLE dbo.PersonStat ADD CONSTRAINT
	FK_PersonStat_Person FOREIGN KEY
	(
	PersonId
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (15122703,GetDate(),'Adding PersonStats');

END
