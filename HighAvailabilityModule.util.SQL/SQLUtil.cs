// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.Util.SQL
{
    using System;

    public class SQLUtil
    {
        public string GetConStr(string conStr, TimeSpan operationTimeout)
        {
            return (conStr.IndexOf("Connect Timeout") == -1 ? conStr : conStr.Substring(0, conStr.IndexOf("Connect Timeout")))
                + "Connect Timeout=" + Convert.ToInt32(Math.Ceiling(operationTimeout.TotalSeconds)).ToString();
        }
    }
}
