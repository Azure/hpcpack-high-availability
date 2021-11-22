// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Storage.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;
    using Microsoft.Hpc.HighAvailabilityModule.Util.SQL;

    public class SQLStorageMembershipClient : IMembershipStorageClient
    {
        public string ConStr { get; set; }

        public TimeSpan OperationTimeout { get; set; }

        private SQLUtil sqlUtil = new SQLUtil();

        private readonly string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private static string DefaultTime = "1753-01-01 12:00:00.000";

        private const string GetDataEntrySpName = "dbo.GetDataEntry";

        private const string SetDataEntrySpName = "dbo.SetDataEntry";

        private const string DeleteDataEntrySpName = "dbo.DeleteDataEntry";

        private const string EnumerateDataEntrySpName = "dbo.EnumerateDataEntry";

        private const string GetDataTimeSpName = "dbo.GetDataTime";

        public static readonly TraceSource ts = new TraceSource("Microsoft.Hpc.HighAvailablity.SQLStorageMembershipClient");

        private string value;

        private string type;

        public SQLStorageMembershipClient(string conStr, TimeSpan operationTimeout)
        {
            this.OperationTimeout = operationTimeout;
            this.ConStr = this.sqlUtil.GetConStr(conStr, this.OperationTimeout);
        }

        public async Task <(string value, string type)> GetDataEntryAsync(string path, string key)
        {
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = GetDataEntrySpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@dpath", SqlDbType.NVarChar).Value = path;
            comStr.Parameters.Add("@dkey", SqlDbType.NVarChar).Value = key;

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                SqlDataReader ReturnedValue = await comStr.ExecuteReaderAsync().ConfigureAwait(false);
                if (ReturnedValue.HasRows)
                {
                    ReturnedValue.Read();
                    value = ReturnedValue[0].ToString();
                    type = ReturnedValue[1].ToString();
                    ReturnedValue.Close();
                }
                else
                {
                    value = string.Empty;
                    type = string.Empty;
                }
                return (value, type);
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, $"Error occured when getting data entry: {ex.ToString()}");
                throw new InvalidOperationException($"Error occured when getting data entry: {ex.ToString()}", ex);
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public object TryParseDataEntry(string value, string type)
        {
            if (type == "System.Guid")
            {
                return Guid.Parse(value);
            }
            else if (type == "System.String")
            {
                return value;
            }
            else if (type == "System.Int32")
            {
                return int.Parse(value);
            }
            else if (type == "System.Int64")
            {
                return long.Parse(value);
            }
            else if (type == "System.Double")
            {
                return double.Parse(value);
            }
            else if (type == "System.String[]")
            {
                return value.Split(",".ToCharArray());
            }
            else if (type == "System.Byte[]")
            {
                return System.Text.Encoding.UTF8.GetBytes(value);
            }
            else
            {
                ts.TraceEvent(TraceEventType.Error, 0, $"Input type is not valid: {type}.");
                throw new InvalidOperationException("Input type is not valid.");
            }
        }

        public async Task<Guid> TryGetGuidAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.Guid")
            {
                return (Guid)TryParseDataEntry(value, type);
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not Guid.");
            }
        }

        public async Task<string> TryGetStringAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.String")
            {
                return (string)TryParseDataEntry(value, type);
            }
            else if(type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not string.");
            }
        }

        public async Task<int> TryGetIntAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.Int32")
            {
                return (int)TryParseDataEntry(value, type); 
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not int.");
            }
        }

        public async Task<long> TryGetLongAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.Int64")
            {
                return (long)TryParseDataEntry(value, type);
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not long.");
            }
        }

        public async Task<double> TryGetDoubleAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.Double")
            {
                return (double)TryParseDataEntry(value, type);
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not double.");
            }
        }

        public async Task<string[]> TryGetStringArrayAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.String[]")
            {
                return (string[])TryParseDataEntry(value, type);
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not string[].");
            }
        }

        public async Task<byte[]> TryGetByteArrayAsync(string path, string key)
        {
            var getDataEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
            value = getDataEntry.value;
            type = getDataEntry.type;

            if (type == "System.Byte[]")
            {
                return (byte[])TryParseDataEntry(value, type);
            }
            else if (type == string.Empty)
            {
                throw new EmptyValueException("Get empty value.");
            }
            else
            {
                throw new InvalidOperationException("Input value is not byte[].");
            }
        }

        private async Task<DateTime> GetDataTimeAsync(string path, string key)
        {
            string lastOperationTime;
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = GetDataTimeSpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@dpath", SqlDbType.NVarChar).Value = path;
            comStr.Parameters.Add("@dkey", SqlDbType.NVarChar).Value = key;

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                SqlDataReader ReturnedValue = await comStr.ExecuteReaderAsync().ConfigureAwait(false);
                if (ReturnedValue.HasRows)
                {
                    ReturnedValue.Read();
                    lastOperationTime = ReturnedValue[0].ToString();
                    ReturnedValue.Close();
                }
                else
                {
                    lastOperationTime = DefaultTime;
                }
                return Convert.ToDateTime(Convert.ToDateTime(lastOperationTime).ToString(this.timeFormat));
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error occured when getting last operation time: {ex.ToString()}");
                throw new InvalidOperationException($"Error occured when getting last operation time: {ex.ToString()}", ex);
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task SetDataEntryAsync(string path, string key, string value, string type, bool forceWrite = false)
        {
            DateTime lastOperationTime = Convert.ToDateTime(Convert.ToDateTime(DefaultTime).ToString(this.timeFormat));
            if (forceWrite == false)
            {
                lastOperationTime = await GetDataTimeAsync(path, key).ConfigureAwait(false);
            }

            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = SetDataEntrySpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@dpath", SqlDbType.NVarChar).Value = path;
            comStr.Parameters.Add("@dkey", SqlDbType.NVarChar).Value = key;
            comStr.Parameters.Add("@dvalue", SqlDbType.NVarChar).Value = value;
            comStr.Parameters.Add("@dtype", SqlDbType.NVarChar).Value = type;
            comStr.Parameters.Add("@lastOperationTime", SqlDbType.DateTime).Value = lastOperationTime;

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                await comStr.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, $"Error occured when setting data entry: {ex.ToString()}");
                throw new InvalidOperationException($"Error occured when setting data entry: {ex.ToString()}", ex);
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task SetGuidAsync(string path, string key, Guid value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, value.ToString(), "System.Guid", forceWrite).ConfigureAwait(false);
        }

        public async Task SetStringAsync(string path, string key, string value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, value, "System.String", forceWrite).ConfigureAwait(false);
        }

        public async Task SetIntAsync(string path, string key, int value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, value.ToString(), "System.Int32", forceWrite).ConfigureAwait(false);
        }

        public async Task SetLongAsync(string path, string key, long value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, value.ToString(), "System.Int64", forceWrite).ConfigureAwait(false);
        }

        public async Task SetDoubleAsync(string path, string key, double value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, value.ToString(), "System.Double", forceWrite).ConfigureAwait(false);
        }

        public async Task SetStringArrayAsync(string path, string key, string[] value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, string.Join(",", value), "System.String[]", forceWrite).ConfigureAwait(false);
        }

        public async Task SetByteArrayAsync(string path, string key, byte[] value, bool forceWrite = false)
        {
            await SetDataEntryAsync(path, key, System.Text.Encoding.UTF8.GetString(value), "System.Byte[]", forceWrite).ConfigureAwait(false);
        }

        public async Task DeleteDataEntryAsync(string path, string key)
        {
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = DeleteDataEntrySpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@dpath", SqlDbType.NVarChar).Value = path;
            comStr.Parameters.Add("@dkey", SqlDbType.NVarChar).Value = key;

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                await comStr.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, $"Error occured when deleting data entry: {ex.ToString()}");
                throw new InvalidOperationException($"Error occured when deleting data entry: {ex.ToString()}", ex);
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task<List<string>> EnumerateDataEntryAsync(string path)
        {
            List<string> keyList = new List<string>();

            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = EnumerateDataEntrySpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@dpath", SqlDbType.NVarChar).Value = path;

            try
            {
                await con.OpenAsync().ConfigureAwait(false);
                SqlDataReader ReturnedValue = await comStr.ExecuteReaderAsync().ConfigureAwait(false);
                if (ReturnedValue.HasRows)
                {
                    while (ReturnedValue.Read())
                    {
                        keyList.Add(ReturnedValue[0].ToString());
                    }
                    ReturnedValue.Close();
                }
                return keyList;
            }
            catch (Exception ex)
            {
                ts.TraceEvent(TraceEventType.Error, 0, $"Error occured when enumerating data entry: {ex.ToString()}");
                throw new InvalidOperationException($"Error occured when enumerating data entry: {ex.ToString()}", ex);
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task Monitor(string path, string key, TimeSpan interval, Action<string, string> callback)
        {
            string lastSeenValue = string.Empty;
            string lastSeenType = string.Empty;
            while (true)
            {
                try
                {
                    var getEntry = await this.GetDataEntryAsync(path, key).ConfigureAwait(false);
                    value = getEntry.value;
                    type = getEntry.type;

                    if (DataChanged(value, type, lastSeenValue, lastSeenType))
                    {
                        callback(value, type);
                        lastSeenValue = value;
                        lastSeenType = type;
                    }
                }
                catch (Exception ex)
                {
                    ts.TraceEvent(TraceEventType.Error, 0, $"Error occured {ex.ToString()}");
                    throw;
                }
                finally
                {
                    await Task.Delay(interval).ConfigureAwait(false);
                }
            }
        }

        public void Callback(string value, string type)
        {
            Console.WriteLine($"[Monitor] Value: {value}    Type: {type}");
        }

        private static bool DataChanged(string value, string type, string lastSeenValue, string lastSeenType)
        {
            return lastSeenValue != value
                || lastSeenType != type
                || (lastSeenValue == string.Empty && value != string.Empty)
                || (lastSeenType == string.Empty && type != string.Empty);
        }

        public class EmptyValueException : ApplicationException
        {
            public EmptyValueException(string message) : base(message) { }

            public override string Message
            {
                get
                {
                    return base.Message;
                }
            }
        }
    }
}