IF OBJECT_ID('DataTable') IS NOT NULL
	DROP TABLE DataTable;
GO
CREATE TABLE DataTable
(dpath nvarchar(50) NOT NULL,
dkey nvarchar(50) NOT NULL,
dvalue nvarchar(50),
dtype nvarchar(50),
timestamp datetime
);
GO
ALTER TABLE DataTable ADD PRIMARY KEY(dpath, dkey)
GO

IF OBJECT_ID('GetDataEntry') IS NOT NULL
	DROP PROCEDURE GetDataEntry;
GO
CREATE PROCEDURE GetDataEntry
	@dpath nvarchar(50),
	@dkey nvarchar(50)
AS
	SET NOCOUNT ON
	SELECT dvalue, dtype FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
GO

IF OBJECT_ID('GetDataTime') IS NOT NULL
	DROP PROCEDURE GetDataTime;
GO
CREATE PROCEDURE GetDataTime
	@dpath nvarchar(50),
	@dkey nvarchar(50)
AS
	SET NOCOUNT ON
	SELECT timestamp FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
GO

IF OBJECT_ID('SetDataEntry') IS NOT NULL
	DROP PROCEDURE SetDataEntry;
GO
CREATE PROCEDURE SetDataEntry
	@dpath nvarchar(50),
	@dkey nvarchar(50),
	@dvalue nvarchar(50),
	@dtype nvarchar(50),
	@lastOperationTime datetime,
	@now datetime = NULL
AS
	BEGIN TRY
		BEGIN TRAN
			SET NOCOUNT ON
			IF @now IS NULL
				SET @now = GETDATE();
			IF NOT EXISTS (SELECT * FROM dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey)
				INSERT INTO dbo.DataTable (dpath, dkey, dvalue, dtype, timestamp) 
				VALUES (@dpath, @dkey, @dvalue, @dtype, @now);
			ELSE
				UPDATE dbo.DataTable
				SET dpath = @dpath, dkey = @dkey, dvalue = @dvalue, dtype = @dtype, timestamp = @now
				WHERE dpath = @dpath AND dkey = @dkey AND (@lastOperationTime = CONVERT(DATETIME, '1753-01-01 12:00:00.000', 21) OR timestamp = @lastOperationTime);
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
GO

IF OBJECT_ID('DeleteDataEntry') IS NOT NULL
	DROP PROCEDURE DeleteDataEntry;
GO
CREATE PROCEDURE DeleteDataEntry
	@dpath nvarchar(50),
	@dkey nvarchar(50)
AS
	SET NOCOUNT ON
	DELETE dbo.DataTable WHERE dpath = @dpath AND dkey = @dkey;
GO

IF OBJECT_ID('EnumerateDataEntry') IS NOT NULL
	DROP PROCEDURE EnumerateDataEntry;
GO
CREATE PROCEDURE EnumerateDataEntry
	@dpath nvarchar(50)
AS
	SET NOCOUNT ON
	SELECT dkey FROM dbo.DataTable WHERE dpath = @dpath;
GO