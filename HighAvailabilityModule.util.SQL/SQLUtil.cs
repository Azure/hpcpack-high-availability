// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Util.SQL
{
    using System;
    using System.Linq;

    public class SQLUtil
    {
        public string GetConStr(string conStr, TimeSpan operationTimeout)
        {
            if (operationTimeout == default(TimeSpan))
            {
                return conStr;
            }
            else
            {
                return string.Join(";", conStr.Split(';').Where(s => !s.Contains("Connect Timeout")).Concat(new[] { "Connect Timeout=" + Convert.ToInt32(Math.Ceiling(operationTimeout.TotalSeconds)).ToString() }));
            }
        }
    }
}
