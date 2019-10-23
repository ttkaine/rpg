	IF(NOT EXISTS(
	select id from dbo.ChangeLog where id = 19102302
))
BEGIN

update [Page] set ImageMime = 'image/png', FileIdentifier = 'default.png'

UPDATE
    [Page]
SET
	ImageMime = 'image/jpeg',
    FileIdentifier = 'default_character.jpg'
FROM
    [Person] AS Table_A
    INNER JOIN [Page] AS Table_B
        ON Table_A.id = Table_B.id

UPDATE
    [Page]
SET
    FileIdentifier = Table_A.FileIdentifier
FROM
    [PageImage] AS Table_A
    INNER JOIN [Page] AS Table_B
        ON Table_A.PageId = Table_B.id
		where Table_A.IsPrimary = 1

	INSERT INTO dbo.ChangeLog (Id, DateTime, Comment) VALUES (19102302,GetDate(),'Set default FileIdentifier for Pages');

END		