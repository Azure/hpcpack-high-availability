USE HighAvailabilityStorage
GO
EXEC tSQLt.NewTestClass 'MembershipSQLStorageUnitTest';
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test GetDataEntry1]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @TempTable TABLE
	(dvalue nvarchar(50),
	dtype nvarchar(50)
	);
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dvalue nvarchar(50),
	dtype nvarchar(50)
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue = '111';
	SET @dtype = 'System.String';

	INSERT INTO dbo.DataTable(dpath, dkey, dvalue, dtype) VALUES(@dpath, @dkey, @dvalue, @dtype);
	INSERT INTO @TempTable EXEC dbo.GetDataEntry @dpath, @dkey;

	SELECT dvalue, dtype INTO MembershipSQLStorageUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dvalue, dtype) VALUES(@dvalue, @dtype);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test SetDataEntry1]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50),
	timestamp datetime
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue = '111';
	SET @dtype = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue, @dtype, @DefaultTime, @now;
	
	SELECT dpath, dkey, dvalue, dtype, timestamp INTO MembershipSQLStorageUnitTest_Actual FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkey, @dvalue, @dtype, @now);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test SetDataEntry2]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue1 nvarchar(50);
	DECLARE @dvalue2 nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @lastOperationTime datetime;
	DECLARE @now datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(lastOperationTime datetime
	)
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50),
	timestamp datetime
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue1 = '111';
	SET @dvalue2 = '222';
	SET @dtype = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue1, @dtype, @DefaultTime;
	INSERT INTO @TempTable  EXEC dbo.GetDataTime @dpath, @dkey;
	SELECT @lastOperationTime = lastOperationTime FROM @TempTable;
	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue2, @dtype, @lastOperationTime, @now;
	
	SELECT dpath, dkey, dvalue, dtype, timestamp INTO MembershipSQLStorageUnitTest_Actual FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkey, @dvalue2, @dtype, @now);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test SetDataEntry3]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue1 nvarchar(50);
	DECLARE @dvalue2 nvarchar(50);
	DECLARE @dtype1 nvarchar(50);
	DECLARE @now datetime;
	DECLARE @lastOperationTime datetime;
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50),
	timestamp datetime
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue1 = '111';
	SET @dvalue2 = '222';
	SET @dtype1 = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);
	SET @lastOperationTime = CONVERT(DATETIME,'2019-08-02 12:00:00.000',21);

	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue1, @dtype1, @lastOperationTime, @now;
	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue2, @dtype1, @lastOperationTime, @now;

	SELECT dpath, dkey, dvalue, dtype, timestamp INTO MembershipSQLStorageUnitTest_Actual FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkey, @dvalue1, @dtype1, @now);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test SetDataEntry4]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue1 nvarchar(50);
	DECLARE @dvalue2 nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @lastOperationTime datetime;
	DECLARE @now datetime;
	DECLARE @newTime datetime;
	DECLARE @DefaultTime datetime;
	DECLARE @TempTable TABLE
	(lastOperationTime datetime
	);
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50),
	timestamp datetime
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue1 = '111';
	SET @dvalue2 = '222';
	SET @dtype = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);
	SET @newTime = CONVERT(DATETIME,'2019-08-01 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue1, @dtype, @DefaultTime;
	INSERT INTO @TempTable EXEC dbo.GetDataTime @dpath, @dkey;
	SELECT @lastOperationTime = lastOperationTime FROM @TempTable;
	UPDATE dbo.DataTable SET timestamp = @newTime WHERE dpath = @dpath AND dkey = @dkey;
	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue2, @dtype, @lastOperationTime, @now;
	
	SELECT dpath, dkey, dvalue, dtype, timestamp INTO MembershipSQLStorageUnitTest_Actual FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkey, @dvalue1, @dtype, @newTime);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test SetDataEntry5]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue1 nvarchar(50);
	DECLARE @dvalue2 nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @now datetime;
	DECLARE @newTime datetime;
	DECLARE @DefaultTime datetime;
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50),
	timestamp datetime
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue1 = '111';
	SET @dvalue2 = '222';
	SET @dtype = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);
	SET @newTime = CONVERT(DATETIME,'2019-08-01 12:00:00.000',21);
	SET @DefaultTime = CONVERT(DATETIME,'1753-01-01 12:00:00.000',21);

	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue1, @dtype, @DefaultTime;
	UPDATE dbo.DataTable SET timestamp = @newTime WHERE dpath = @dpath AND dkey = @dkey;
	EXEC dbo.SetDataEntry @dpath, @dkey, @dvalue2, @dtype, @DefaultTime, @now;
	
	SELECT dpath, dkey, dvalue, dtype, timestamp INTO MembershipSQLStorageUnitTest_Actual FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkey, @dvalue2, @dtype, @now);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test DeleteDataEntry1]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkey nvarchar(50);
	DECLARE @dvalue nvarchar(50);
	DECLARE @dtype nvarchar(50);
	DECLARE @now datetime;
	DECLARE @Actual int;
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dpath nvarchar(50),
	dkey nvarchar(50),
	dvalue nvarchar(50),
	dtype nvarchar(50)
	);

	SET @dpath = 'local\hpc';
	SET @dkey = 'A';
	SET @dvalue = '111';
	SET @dtype = 'System.String';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);

	INSERT INTO dbo.DataTable(dpath, dkey, dvalue, dtype,timestamp) VALUES(@dpath, @dkey, @dvalue, @dtype, @now);

	EXEC dbo.DeleteDataEntry @dpath, @dkey;

	SELECT @Actual = COUNT(*) FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;

	EXEC tSQLt.AssertEquals 0, @Actual;
END
GO

--TestMethod
USE HighAvailabilityStorage
GO
CREATE OR ALTER PROCEDURE MembershipSQLStorageUnitTest.[test EnumerateDataEntry1]
AS
BEGIN
	IF OBJECT_ID('MembershipSQLStorageUnitTest_Excepted') IS NOT NULL
		DROP TABLE MembershipSQLStorageUnitTest_Excepted;
	DELETE dbo.DataTable;
	DECLARE @dpath nvarchar(50);
	DECLARE @dkeyA nvarchar(50);
	DECLARE @dkeyB nvarchar(50);
	DECLARE @dvalue1 nvarchar(50);
	DECLARE @dvalue2 nvarchar(50);
	DECLARE @dtype1 nvarchar(50);
	DECLARE @dtype2 nvarchar(50);
	DECLARE @now datetime;
	DECLARE @TempTable TABLE
	(dkey nvarchar(50)
	);
	CREATE TABLE MembershipSQLStorageUnitTest_Excepted
	(dkey nvarchar(50)
	);

	SET @dpath = 'local\hpc';
	SET @dkeyA = 'A';
	SET @dkeyB = 'B';
	SET @dvalue1 = '111';
	SET @dvalue2 = '222';
	SET @dtype1 = 'System.String';
	SET @dtype1 = 'System.Int32';
	SET @now = CONVERT(DATETIME,'2019-07-31 12:00:00.000',21);

	INSERT INTO dbo.DataTable (dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkeyA, @dvalue1, @dtype1, @now);
	INSERT INTO dbo.DataTable (dpath, dkey, dvalue, dtype, timestamp) VALUES(@dpath, @dkeyB, @dvalue2, @dtype2, @now);
	INSERT INTO @TempTable EXEC dbo.EnumerateDataEntry @dpath;

	SELECT dkey INTO MembershipSQLStorageUnitTest_Actual FROM @TempTable;
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dkey) VALUES(@dkeyA);
	INSERT INTO MembershipSQLStorageUnitTest_Excepted(dkey) VALUES(@dkeyB);

	EXEC tSQLt.AssertEqualsTable MembershipSQLStorageUnitTest_Excepted, MembershipSQLStorageUnitTest_Actual;
END
GO