// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Client.SQL
{
    using System;
    using System.Threading.Tasks;
    using System.Data;
    using System.Data.SqlClient;

    using Microsoft.Hpc.HighAvailabilityModule.Interface;
    using Microsoft.Hpc.HighAvailabilityModule.Util.SQL;

    public class SQLMembershipClient: IMembershipClient
    {
        public string Uuid { get; }

        public string Utype { get; }

        public string Uname { get; }

        public TimeSpan OperationTimeout { get; set; }

        public string ConStr { get; set; }

        private SQLUtil sqlUtil = new SQLUtil();

        private string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private const string HeartBeatSpName = "dbo.HeartBeat";

        private const string GetHeartBeatSpName = "dbo.GetHeartBeat";

        private const string GetParameterSpName = "dbo.GetParameter";

        public SQLMembershipClient(string utype, string uname, TimeSpan operationTimeout, string conStr) : this(operationTimeout, conStr)
        {
            this.Uuid = Guid.NewGuid().ToString();
            this.Utype = utype;
            this.Uname = uname;
        }

        public SQLMembershipClient(string conStr)
        {
            this.ConStr = this.sqlUtil.GetConStr(conStr, this.OperationTimeout);
        }

        public SQLMembershipClient(TimeSpan operationTimeout, string conStr)
        {
            this.OperationTimeout = operationTimeout;
            this.ConStr = this.sqlUtil.GetConStr(conStr, operationTimeout);
        }

        public static SQLMembershipClient CreateNew(string utype, string uname, TimeSpan operationTimeout, string conStr) => new SQLMembershipClient(utype, uname, operationTimeout, conStr);

        public async Task HeartBeatAsync(HeartBeatEntryDTO entryDTO)
        {
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = HeartBeatSpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@uuid", SqlDbType.NVarChar).Value = entryDTO.Uuid;
            comStr.Parameters.Add("@utype", SqlDbType.NVarChar).Value = entryDTO.Utype;
            comStr.Parameters.Add("@uname", SqlDbType.NVarChar).Value = entryDTO.Uname;
            comStr.Parameters.Add("@lastSeenUuid", SqlDbType.NVarChar).Value = entryDTO.LastSeenEntry.Uuid;
            comStr.Parameters.Add("@lastSeenUtype", SqlDbType.NVarChar).Value = entryDTO.LastSeenEntry.Utype;
            comStr.Parameters.Add("@lastSeenTimeStamp", SqlDbType.DateTime).Value = entryDTO.LastSeenEntry.TimeStamp.ToString(this.timeFormat);

            try
            {
                await con.OpenAsync();
                await comStr.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.Uuid}] Error occured when sending heartbeat entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task<HeartBeatEntry> GetHeartBeatEntryAsync(string utype)
        {
            HeartBeatEntry heartBeatEntry;
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = GetHeartBeatSpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@utype", SqlDbType.NVarChar).Value = utype;

            try
            {
                await con.OpenAsync();
                SqlDataReader ReturnedEntry = await comStr.ExecuteReaderAsync();
                if (ReturnedEntry.HasRows)
                {
                    ReturnedEntry.Read();
                    heartBeatEntry = new HeartBeatEntry(ReturnedEntry[0].ToString(), ReturnedEntry[1].ToString(),
                        ReturnedEntry[2].ToString(), Convert.ToDateTime(Convert.ToDateTime(ReturnedEntry[3]).ToString(this.timeFormat)));
                    
                    ReturnedEntry.Close();
                }
                else
                {
                    heartBeatEntry = HeartBeatEntry.Empty;
                }
                return heartBeatEntry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.Uuid}] Error occured when getting heartbeat entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public async Task<int> GetParameterAsync(string parameterName)
        {
            int res;
            SqlConnection con = new SqlConnection(this.ConStr);
            string StoredProcedure = GetParameterSpName;
            SqlCommand comStr = new SqlCommand(StoredProcedure, con);
            comStr.CommandType = CommandType.StoredProcedure;
            comStr.CommandTimeout = Convert.ToInt32(Math.Ceiling(this.OperationTimeout.TotalSeconds));

            comStr.Parameters.Add("@parameterName", SqlDbType.NVarChar).Value = parameterName;

            try
            {
                await con.OpenAsync();
                SqlDataReader ReturnedEntry = await comStr.ExecuteReaderAsync();
                if (ReturnedEntry.HasRows)
                {
                    ReturnedEntry.Read();
                    res = (int)ReturnedEntry[0];
                }
                else
                {
                    res = default(int);
                }
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.Uuid}] Error occured when getting parameter: {ex.ToString()}");
                throw;
            }
            finally
            {
                con.Close();
                con.Dispose();
                comStr.Dispose();
            }
        }

        public string GenerateUuid() => this.Uuid;
    }
}
