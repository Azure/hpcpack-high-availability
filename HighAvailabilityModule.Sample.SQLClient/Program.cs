// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Sample.SQLClient
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Algorithm;
    using Microsoft.Hpc.HighAvailabilityModule.Client.SQL;
    class Program
    {
        static async Task Main(string[] args)
        {
            TraceSource ts = MembershipWithWitness.ts;
            ts.Switch.Level = SourceLevels.Warning;
            ts.Listeners.Add(new TextWriterTraceListener(Console.Out));

            string utype;
            string uname;
            string conStr;
            string AffinityType = string.Empty;

            ArrayList AllType = new ArrayList();

            if (args.Length >= 4)
            {
                utype = args[1];
                if (utype == "query")
                {
                    uname = "-1";
                    AffinityType = "";
                    for (int i = 2; i < args.Length; i++)
                    {
                        AllType.Add(args[i]);
                    }
                    conStr = args[args.Length - 1];
                }
                else
                {
                    uname = args[2];
                    conStr = args[3];
                    if (args.Length == 5)
                    {
                        AffinityType = args[4];
                    }
                }
            }
            else
            {
                Console.WriteLine("Please give the client's type and machine name!");
                return;
            }

            var interval = TimeSpan.FromSeconds(0.2);
            var timeout = TimeSpan.FromSeconds(5);

            SQLMembershipClient client = new SQLMembershipClient(utype, uname, interval, conStr);
            MembershipWithWitness algo = new MembershipWithWitness(client, interval, timeout, AffinityType);

            Console.WriteLine("Uuid:{0}", client.Uuid);
            Console.WriteLine("Type:{0}", client.Utype);
            Console.WriteLine("Machine Name:{0}", client.Uname);

            if (client.Utype == "query")
            {
                while (true)
                {
                    foreach (string qtype in AllType)
                    {
                        try
                        {
                            var primary = await client.GetHeartBeatEntryAsync(qtype).ConfigureAwait(false);
                            if (!primary.IsEmpty)
                            {
                                Console.WriteLine($"[Query Result] Type:{primary.Utype}. Machine Name:{primary.Uname}. Running as primary. [{primary.TimeStamp}]");
                                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{client.Uuid}] Error occured when querying heartbeat entry: {ex.ToString()}");
                        }
                    }
                }
            }
            else
            {
                await algo.RunAsync(
                    () => Task.Run(
                        async () =>
                            {
                                while (true)
                                {
                                    Console.WriteLine($"Type:{client.Utype}. Machine Name:{client.Uname}. Running as primary. [{DateTime.UtcNow}]");
                                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                                }
                            }),
                    null).ConfigureAwait(false);
            }
        }
    }
}
