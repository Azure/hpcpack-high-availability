// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailablityModule.Storage.Sample.Client
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using HighAvailabilityModule.Storage.Client;
    class Program
    {
        static async Task Main(string[] args)
        {
            string conStr = "server=.;database=HighAvailabilityStorage;Trusted_Connection=SSPI;Connect Timeout=30";
            if (args.Length>1)
            {
                conStr = args[1];
            }

            var timeout = TimeSpan.FromSeconds(0.2);
            var interval = TimeSpan.FromSeconds(0.1);

            SQLStorageMembershipClient client = new SQLStorageMembershipClient(conStr, timeout);

            //Monitor
            string path = "local\\hpc";
            string keyA = "A";
            string keyB = "B";
            string value = "111";
            client.Monitor(path, keyA, interval, client.Callback);

            //SetMethod
            try
            {
                await client.SetStringAsync(path, keyA, value).ConfigureAwait(false);
                await client.SetStringAsync(path, keyB, value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured when setting data entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            //GetMethod
            try
            {
                var result = await client.TryGetStringAsync(path, keyA).ConfigureAwait(false);
                Console.WriteLine($"Get value: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured when getting data entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            //EnumerateMethod
            try
            {
                List<string> getKey = new List<string>();
                getKey = await client.EnumerateDataEntryAsync(path).ConfigureAwait(false);
                foreach (string k in getKey)
                {
                    Console.WriteLine($"Get key: {k}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured when enumerating data entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }


            //DeleteMethod
            try
            {
                await client.DeleteDataEntryAsync(path, keyA).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured when deleting data entry: {ex.ToString()}");
                throw;
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
        }
    }
}
