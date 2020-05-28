IF OBJECT_ID('HeartBeatTable') IS NOT NULL
	DROP TABLE HeartBeatTable;
GO
CREATE TABLE HeartBeatTable
(uuid nvarchar(50),
utype nvarchar(50) NOT NULL PRIMARY KEY,
uname nvarchar(50),
timeStamp datetime);
GO

IF OBJECT_ID('ParameterTable') IS NOT NULL
	DROP TABLE ParameterTable;
GO
CREATE TABLE ParameterTable
(heartbeatTimeOut int);
INSERT INTO dbo.ParameterTable (heartbeatTimeOut)
VALUES(5000);
GO

IF OBJECT_ID('HeartBeatInvalid') IS NOT NULL
	DROP FUNCTION HeartBeatInvalid;
GO
CREATE FUNCTION HeartBeatInvalid
(@utype nvarchar(50),
@now datetime)
RETURNS bit
AS
	BEGIN
		DECLARE @InValid bit;
		DECLARE @TimeOut int;
		DECLARE @TimeOutMin int;
		DECLARE @OrgTime datetime;
		SELECT @TimeOut = heartbeatTimeOut FROM dbo.ParameterTable;
		SELECT @orgTime = timeStamp FROM dbo.HeartBeatTable WHERE utype = @utype;
		SET @TimeOutMin = @TimeOut/60000 + 2
		IF (NOT EXISTS(SELECT * FROM dbo.HeartBeatTable WHERE utype = @utype))
			OR (DATEDIFF(MINUTE, @orgTime, @now) >= @TimeOutMin)
			OR (DATEDIFF(MILLISECOND, @orgTime, @now) >= @TimeOut)
			SET @InValid = 1;
		ELSE
			SET @InValid = 0;
		RETURN @InValid
	END
GO

IF OBJECT_ID('LastSeenEntryValid') IS NOT NULL
	DROP FUNCTION LastSeenEntryValid;
GO
CREATE FUNCTION LastSeenEntryValid
(@utype nvarchar(50), 
@lastSeenUuid nvarchar(50), 
@lastSeenUtype nvarchar(50), 
@lastSeenTimeStamp datetime)
RETURNS bit
AS
	BEGIN
		 DECLARE @IsValid bit;
		 IF (EXISTS(SELECT * FROM dbo.HeartBeatTable WHERE utype = @utype)) 
			AND ((SELECT uuid FROM dbo.HeartBeatTable WHERE utype = @utype) = @lastSeenUuid)
			AND ((SELECT utype FROM dbo.HeartBeatTable WHERE utype = @utype) = @lastSeenUtype) 
			AND ((SELECT timeStamp FROM dbo.HeartBeatTable WHERE utype = @utype) = @lastSeenTimeStamp)
				SET @IsValid = 1;
		ELSE
			SET @IsValid = 0;
	RETURN @IsValid
	END
GO

IF OBJECT_ID('ValidInput') IS NOT NULL
	DROP FUNCTION ValidInput;
GO
CREATE FUNCTION ValidInput
(@uuid nvarchar(50), 
@utype nvarchar(50), 
@lastSeenUuid nvarchar(50), 
@lastSeenUtype nvarchar(50), 
@lastSeenTimeStamp datetime,
@now datetime)
RETURNS bit
AS
	BEGIN
		DECLARE @IsValid bit;
		IF (NOT EXISTS(SELECT * FROM dbo.HeartBeatTable WHERE utype = @utype)) 
			OR ((dbo.HeartBeatInvalid(@utype, @now) = 1) AND (@lastSeenUuid = '') AND (@lastSeenUtype = ''))
			OR ((dbo.LastSeenEntryValid(@utype, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp) = 1) 
			AND ((SELECT uuid FROM dbo.HeartBeatTable WHERE utype = @utype) = @uuid) AND ((SELECT utype FROM dbo.HeartBeatTable WHERE utype=@utype)=@utype))
			SET @IsValid = 1;
		ELSE
			SET @IsValid = 0;
		RETURN @IsValid
	END
GO

IF OBJECT_ID('HeartBeat') IS NOT NULL
	DROP PROCEDURE HeartBeat;
GO
CREATE PROCEDURE HeartBeat
	@uuid nvarchar(50),
	@utype nvarchar(50),
	@uname nvarchar(50),
	@lastSeenUuid nvarchar(50),
	@lastSeenUtype nvarchar(50),
	@lastSeenTimeStamp datetime,
	@now datetime = NULL
AS
	SET NOCOUNT ON;
	IF @now IS NULL
		SET @now = GETDATE();
	IF dbo.ValidInput(@uuid, @utype, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now) = 1
		BEGIN
			IF NOT EXISTS (SELECT * FROM dbo.HeartBeatTable WHERE utype = @utype)
				INSERT INTO dbo.HeartBeatTable (uuid, utype, uname, timeStamp)
				VALUES(@uuid, @utype, @uname, @now);
			ELSE
				UPDATE dbo.HeartBeatTable
				SET uuid = @uuid, utype = @utype, uname = @uname, timeStamp = @now
				WHERE utype = @utype AND timeStamp = @lastSeenTimeStamp;
		END
GO

IF OBJECT_ID('GetHeartBeat') IS NOT NULL
	DROP PROCEDURE GetHeartBeat;
GO
CREATE PROCEDURE GetHeartBeat
	@utype nvarchar(50),
	@now datetime = NULL
AS
	SET NOCOUNT ON
	IF @now IS NULL
		SET @now = GETDATE();
	IF dbo.HeartBeatInvalid(@utype, @now) = 1
		BEGIN
			DECLARE @OldTime datetime;
			IF NOT EXISTS (SELECT timeStamp FROM dbo.HeartBeatTable WHERE utype = @utype)
				BEGIN
					SET @OldTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
					SELECT '' AS uuid, '' AS utype, '' AS uname, @OldTime AS timeStamp
				END
			ELSE
				BEGIN
					SELECT @OLDTIME = timeStamp FROM dbo.HeartBeatTable WHERE utype = @utype;
					SELECT '' AS uuid, '' AS utype, '' AS uname, @OldTime AS timeStamp;
				END
		END
	ELSE
		SELECT TOP 1 uuid, utype, uname, timeStamp FROM dbo.HeartBeatTable WHERE utype=@utype;
GO

IF OBJECT_ID('GetParameter') IS NOT NULL
	DROP PROCEDURE GetParameter;
GO
CREATE PROCEDURE GetParameter
	@parameterName nvarchar(50)
AS
	SET NOCOUNT ON
	EXEC ('SELECT TOP 1 ' +@parameterName +' FROM dbo.ParameterTable')
GO