USE HighAvailabilityWitness
GO
EXEC tSQLt.NewTestClass 'MembershipServerSQLUnitTest';
GO

IF OBJECT_ID('MembershipServerSQLUnitTest_Excepted') IS NOT NULL
	DROP TABLE MembershipServerSQLUnitTest_Excepted;
GO
CREATE TABLE MembershipServerSQLUnitTest_Excepted
(uuid nvarchar(50),
utype nvarchar(50) NOT NULL PRIMARY KEY,
uname nvarchar(50),
timeStamp datetime
)
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat1]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @uuid nvarchar(50);
	DECLARE @utype nvarchar(50);
	DECLARE @uname nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;

	SET @uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @utype = 'A';
	SET @uname = '1';
	SET @lastSeenTimeStamp = '';
	SET @lastSeenUtype = '';
	SET @lastSeenTimeStamp = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);

	EXEC dbo.HeartBeat @uuid, @utype, @uname, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @utype;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@uuid, @utype, @uname, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat2]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SELECT @lastSeenTimeStamp = lastSeenTimeStamp FROM @TempTable;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client2Uuid, @ClientUtypeA, @ClientUtname2, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat3]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut + 200, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SELECT @lastSeenTimeStamp = lastSeenTimeStamp FROM @TempTable;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname,timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat4]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, '', '', @DefaultTime, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat5]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SET @lastSeenTimeStamp = @now;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat6]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -200, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SELECT @lastSeenTimeStamp = lastSeenTimeStamp FROM @TempTable;
	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;
	
	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat7]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @now;
	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @now;

	SELECT uuid, utype, uname,timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat8]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -200, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SELECT @lastSeenTimeStamp = lastSeenTimeStamp FROM @TempTable;
	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat9]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut, @now);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @Client1Time;
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	SELECT @lastSeenUuid = lastSeenUuid FROM @TempTable;
	SELECT @lastSeenUtype = lastSeenUtype FROM @TempTable;
	SELECT @lastSeenTimeStamp = lastSeenTimeStamp FROM @TempTable;
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;
	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, @lastSeenUuid, @lastSeenUtype, @lastSeenTimeStamp, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client2Uuid, @ClientUtypeA, @ClientUtname2, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat10]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client3Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtypeB nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(lastSeenUuid nvarchar(50),
	lastSeenUtype nvarchar(50),
	lastSeenUname nvarchar(50),
	lastSeenTimeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client3Uuid = '33253ab7-27b6-478c-a359-4eca7df83b80';
	SET @ClientUtypeA = 'A';
	SET @ClientUtypeB = 'B';
	SET @ClientUtname1 = '1';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.HeartBeat @Client1Uuid, @ClientUtypeA, @ClientUtname1, '', '', @DefaultTime, @now;
	EXEC dbo.HeartBeat @Client3Uuid, @ClientUtypeb, @ClientUtname1, '', '', @DefaultTime, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client3Uuid, @ClientUtypeB, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test HeartBeat11]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @Client2Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @ClientUtname2 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @Client2Uuid = '39a78df0-e101-49b9-8c56-ec2fea2e47df';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @ClientUtname2 = '2';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	INSERT dbo.HeartBeatTable(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now)
	EXEC dbo.HeartBeat @Client2Uuid, @ClientUtypeA, @ClientUtname2, '', '', @DefaultTime, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM dbo.HeartBeatTable WHERE utype = @ClientUtypeA;
	INSERT INTO MembershipServerSQLUnitTest_Excepted (uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test GetHeartBeat1]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(uuid nvarchar(50),
	utype nvarchar(50),
	uname nvarchar(50),
	timeStamp datetime)

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipServerSQLUnitTest_Excepted(uuid, utype, uname, timeStamp) VALUES('', '', '', @DefaultTime);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test GetHeartBeat2]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(uuid nvarchar(50),
	utype nvarchar(50),
	uname nvarchar(50),
	timeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	INSERT INTO dbo.HeartBeatTable(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;

	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipServerSQLUnitTest_Excepted(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test GetHeartBeat3]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(uuid nvarchar(50),
	utype nvarchar(50),
	uname nvarchar(50),
	timeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut, @now);

	INSERT INTO dbo.HeartBeatTable(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;
	
	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipServerSQLUnitTest_Excepted(uuid, utype, uname, timeStamp) VALUES('', '', '', @Client1Time);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityWitness
GO
CREATE OR ALTER PROCEDURE MembershipServerSQLUnitTest.[test GetHeartBeat4]
AS
BEGIN
	DELETE dbo.HeartBeatTable;
	DELETE MembershipServerSQLUnitTest_Excepted;
	DECLARE @Client1Uuid nvarchar(50);
	DECLARE @ClientUtypeA nvarchar(50);
	DECLARE @ClientUtname1 nvarchar(50);
	DECLARE @lastSeenUuid nvarchar(50);
	DECLARE @lastSeenUtype nvarchar(50);
	DECLARE @lastSeenTimeStamp datetime;
	DECLARE @TimeOut int;
	DECLARE @now datetime;
	DECLARE @Client1Time datetime;
	DECLARE @TempTable TABLE
	(uuid nvarchar(50),
	utype nvarchar(50),
	uname nvarchar(50),
	timeStamp datetime);

	SET @Client1Uuid = 'cdca5b45-6ea1-4d91-81f6-d39f4821e791';
	SET @ClientUtypeA = 'A';
	SET @ClientUtname1 = '1';
	SET @TimeOut = 5000;
	SET @now = CONVERT(DATETIME,'2019-07-11 12:00:00.000',21);
	SET @Client1Time = DATEADD(MILLISECOND, -@TimeOut + 200, @now);

	INSERT INTO dbo.HeartBeatTable(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);
	INSERT INTO @TempTable EXEC dbo.GetHeartBeat @ClientUtypeA, @now;


	SELECT uuid, utype, uname, timeStamp INTO MembershipServerSQLUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipServerSQLUnitTest_Excepted(uuid, utype, uname, timeStamp) VALUES(@Client1Uuid, @ClientUtypeA, @ClientUtname1, @Client1Time);

	EXEC tSQLt.AssertEqualsTable MembershipServerSQLUnitTest_Excepted, MembershipServerSQLUnitTest_Actual;
END
GO 