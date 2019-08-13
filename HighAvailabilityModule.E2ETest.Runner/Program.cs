// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.E2ETest.Runner
{
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;

    using HighAvailabilityModule.Client.Rest;
    using HighAvailabilityModule.Client.SQL;
    using HighAvailabilityModule.E2ETest.TestCases;
    using HighAvailabilityModule.Interface;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            string clientType;
            string testType;
            string conStr;

            int typeCount = 10;

            IMembershipClient judge;
            Func<string, string, TimeSpan, IMembershipClient> clientFactory;

            if (args.Length<2 || args[1] == "help")
            {
                Console.WriteLine("HA-module E2ETest for basic & chaos test.");
                Console.WriteLine("Args: ");
                Console.WriteLine("Client Type:      rest/sql");
                Console.WriteLine("Test Type:        basic/chaos");
                Console.WriteLine("Utype:            string");
                Console.WriteLine("Connected String (required only for sql client)");
                return;
            }

            if (args.Length < 3)
            {
                Console.WriteLine("Please give the test client type(rest/sql) and test type(basic/chaos).");
                Console.WriteLine("Use \"help\" for usage help.");
                return;
            }
            else
            {
                clientType = args[1];
                testType = args[2];

                string logFileName = "LogFile_" + clientType + "_" + testType + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                Trace.Listeners.Add(new TextWriterTraceListener(System.IO.File.CreateText(logFileName)));
                Trace.WriteLine($"Test client type: {clientType}");
                Trace.WriteLine($"Test type: {testType}");
            }

            if (clientType == "rest")
            {
                judge = new RestMembershipClient();
                clientFactory = RestMembershipClient.CreateNew;
            }
            else if (clientType == "sql")
            {
                if (args.Length < 4)
                {
                    Console.WriteLine("Please give the connected string for sql client.");
                    Console.WriteLine("Use \"help\" for usage help.");
                    return;
                }
                else
                {
                    conStr = args[3];

                    judge = new SQLMembershipClient(conStr);
                    clientFactory = (utype, uname, timeout) => SQLMembershipClient.CreateNew(utype, uname, timeout, conStr);
                }
            }
            else
            {
                Console.WriteLine("Please give the supported test client type.(rest/sql)");
                Console.WriteLine("Use \"help\" for usage help.");
                return;
            }

            if (testType == "basic")
            {
                Task[] tasks = new Task[typeCount];
                for (int i = 0; i < typeCount; i++)
                {
                    string type = ((char)('A' + i)).ToString();
                    var basictest = new BasicTest(clientFactory, judge);
                    tasks[i] = basictest.Start(type);
                }
                await Task.WhenAny(tasks);
            }
            else if (testType == "chaos")
            {
                Task[] tasks = new Task[typeCount];
                for (int i = 0; i < typeCount; i++)
                {
                    string type = ((char)('A' + i)).ToString();
                    var basictest = new ChaosTest(clientFactory, judge);
                    tasks[i] = basictest.Start(type);
                }
                await Task.WhenAny(tasks);
            }
            else
            {
                Console.WriteLine("Please give the supported test type.(basic/chaos)");
                Console.WriteLine("Use \"help\" for usage help.");
                return;
            }
        }
    }
}