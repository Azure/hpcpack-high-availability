// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.Sample.RestClient
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading.Tasks;

    using Microsoft.Hpc.HighAvailabilityModule.Algorithm;
    using Microsoft.Hpc.HighAvailabilityModule.Client.Rest;

    class Program
    {
        static async Task Main(string[] args)
        {
            // Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            string utype;
            string uname;

            ArrayList AllType = new ArrayList();

            if (args.Length != 0)
            {
                utype = args[1];
                if (utype == "query")
                {
                    uname = "-1";
                    for (int i = 2; i < args.Length; i++)
                    {
                        AllType.Add(args[i]);
                    }
                } 
                else
                {
                    uname = args[2];
                } 
            }
            else
            {
                Console.WriteLine("Please give the client's type and machine name!");
                return;
            }

            var interval = TimeSpan.FromSeconds(1);
            var timeout = TimeSpan.FromSeconds(5);

            RestMembershipClient client = new RestMembershipClient(utype, uname, interval);

            MembershipWithWitness algo = new MembershipWithWitness(client, interval, timeout, string.Empty);

            Console.WriteLine("Uuid:{0}",client.Uuid);
            Console.WriteLine("Type:{0}",client.Utype);
            Console.WriteLine("Machine Num:{0}", client.Uname);

            if (client.Utype == "query")
            {
                while (true)
                {
                    foreach (string qtype in AllType)
                    {
                        var primary = await client.GetHeartBeatEntryAsync(qtype).ConfigureAwait(false);
                        if (!primary.IsEmpty)
                        {
                            Console.WriteLine($"[Query Result] Type:{primary.Utype}. Machine Num:{primary.Uname}. Running as primary. [{primary.TimeStamp}]");
                            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
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
                                    Console.WriteLine($"Type:{client.Utype}. Machine Num:{client.Uname}. Running as primary. [{DateTime.UtcNow}]");
                                    await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                                }
                            }),
                    null).ConfigureAwait(false);
            }
        }
    }
}