// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Microsoft.Hpc.HighAvailabilityModule.UnitTest
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.Hpc.HighAvailabilityModule.Algorithm;
    using Microsoft.Hpc.HighAvailabilityModule.Client.InMemory;
    using Microsoft.Hpc.HighAvailabilityModule.Interface;
    using Microsoft.Hpc.HighAvailabilityModule.Server.InMemory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AlgorithmTest
    {
        private static TimeSpan Timeout => TimeSpan.FromSeconds(1.5);

        private static TimeSpan Interval => TimeSpan.FromSeconds(0.5);

        private static string Client1Uuid => "cdca5b45-6ea1-4d91-81f6-d39f4821e791";
        private static string Client2Uuid => "39a78df0-e101-49b9-8c56-ec2fea2e47df";
        private static string Client3Uuid => "33253ab7-27b6-478c-a359-4eca7df83b80";

        private static string ClientUtypeA => "A";
        private static string ClientUtypeB => "B";

        private static string ClientUname1 => "1";
        private static string ClientUname2 => "2";

        private static string AffinityType => "A";

        private InMemoryMembershipServer server;

        private InMemoryMembershipClient client;
        private InMemoryMembershipClient client2;
        private InMemoryMembershipClient client3;

        private MembershipWithWitness algo;
        private MembershipWithWitness algo2;
        private MembershipWithWitness algo3;

        [TestInitialize]
        public void Initialize()
        {
            this.server = new InMemoryMembershipServer(Timeout);
            this.client = new InMemoryMembershipClient(this.server, Client1Uuid, ClientUtypeA, ClientUname1);
            this.algo = new MembershipWithWitness(this.client, Interval, Timeout, string.Empty);

            this.client2 = new InMemoryMembershipClient(this.server, Client2Uuid, ClientUtypeA, ClientUname2);
            this.algo2 = new MembershipWithWitness(this.client2, Interval, Timeout, string.Empty);

            this.client3 = new InMemoryMembershipClient(this.server, Client3Uuid, ClientUtypeB, ClientUname1);
            this.algo3 = new MembershipWithWitness(this.client3, Interval, Timeout, AffinityType);
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest1()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest2()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            await this.algo.CheckPrimaryAsync(now).ConfigureAwait(false);
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest3()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo.CheckPrimaryAsync(now).ConfigureAwait(false);
            Assert.IsTrue(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest4()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now - Timeout));
            await this.algo.CheckPrimaryAsync(now).ConfigureAwait(false);
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest5()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo.CheckPrimaryAsync(now - Timeout + Interval).ConfigureAwait(false);
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest6()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo.CheckPrimaryAsync(now).ConfigureAwait(false);
            await this.algo.CheckPrimaryAsync(now - Timeout + Interval).ConfigureAwait(false);
            Assert.IsTrue(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest7()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest8()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo3.CheckPrimaryAsync(now).ConfigureAwait(false);
            Assert.IsFalse(this.algo3.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task AffinityAsPrimaryTest1()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            await this.algo.CheckAffinityAsync(now).ConfigureAwait(false);
            Assert.IsTrue(this.algo.AffinityAsPrimary(now));
        }

        [TestMethod]
        public async Task AffinityAsPrimaryTest2()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            await this.algo3.CheckAffinityAsync(now).ConfigureAwait(false);
            Assert.IsFalse(this.algo3.AffinityAsPrimary(now));
        }

        [TestMethod]
        public async Task AffinityAsPrimaryTest3()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo3.CheckAffinityAsync(now).ConfigureAwait(false);
            Assert.IsTrue(this.algo3.AffinityAsPrimary(now));
        }

        [TestMethod]
        public async Task AffinityAsPrimaryTest4()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client2Uuid, ClientUtypeA, ClientUname2, now));
            await this.algo3.CheckAffinityAsync(now).ConfigureAwait(false);
            Assert.IsFalse(this.algo3.AffinityAsPrimary(now));
        }

        [TestMethod]
        public async Task AffinityAsPrimaryTest5()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now));
            await this.algo3.CheckAffinityAsync(now - Timeout).ConfigureAwait(false);
            Assert.IsFalse(this.algo3.AffinityAsPrimary(now));
        }

        [TestMethod]
        public async Task HeartBeatAsPrimaryTest1()
        {
            DateTime now = DateTime.UtcNow;
            await this.algo.CheckPrimaryAsync(now).ConfigureAwait(false);
            await this.algo.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
            TestAssistantPackage.AssertCurrentEntry(this.server.CurrentTable, Client1Uuid, ClientUtypeA, ClientUname1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task HeartBeatAsPrimaryTest2()
        {
            await this.algo.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task HeartBeatAndCheckTest1()
        {
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            await this.algo.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        public async Task HeartBeatAndCheckTest2()
        {
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            await this.algo.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            await this.algo3.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            await this.algo3.HeartBeatAsPrimaryAsync().ConfigureAwait(false);
            await this.algo3.CheckPrimaryAsync(DateTime.UtcNow).ConfigureAwait(false);
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(2000)]
        public async Task GetPrimaryTest1()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(2000)]
        public async Task GetPrimaryTest2()
        {
            var getPrimaryTask = this.algo.GetPrimaryAsync();
            this.algo.Stop();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => getPrimaryTask).ConfigureAwait(false);
            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest3()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            this.algo.KeepPrimaryAsync();
            this.algo2.GetPrimaryAsync();

            this.algo.Stop();
            this.algo2.Stop();

            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsFalse(this.algo2.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest4()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            await this.algo2.GetPrimaryAsync().ConfigureAwait(false);

            this.algo.Stop();
            this.algo2.Stop();

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo2.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest5()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            this.server.RemoveCurrent();
            await this.algo.KeepPrimaryAsync().ConfigureAwait(false);

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest6()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            var task = this.algo.KeepPrimaryAsync();
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            this.server.RemoveCurrent();
            await task.ConfigureAwait(false);

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTestWithAffinityTest1()
        {
            this.algo3.GetPrimaryAsync();

            this.algo3.Stop();

            Assert.IsFalse(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTestWithAffinityTest2()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            this.algo.KeepPrimaryAsync();
            await this.algo3.GetPrimaryAsync().ConfigureAwait(false);

            this.algo.Stop();

            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTestWithAffinityTest3()
        {
            await this.algo2.GetPrimaryAsync().ConfigureAwait(false);
            this.algo2.KeepPrimaryAsync();
            this.algo3.GetPrimaryAsync();

            this.algo2.Stop();
            this.algo3.Stop();

            Assert.IsTrue(this.algo2.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsFalse(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(6000)]
        public async Task GetPrimaryTestWithAffinityTest4()
        {
            await this.algo2.GetPrimaryAsync().ConfigureAwait(false);
            var task = this.algo3.GetPrimaryAsync();
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            await task.ConfigureAwait(false);

            this.algo.Stop();
            this.algo3.Stop();

            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsFalse(this.algo2.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        /// <summary>
        /// Simulate network latency is at Interval * 0.9
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Timeout(10000)]
        public async Task KeepPrimaryTest1()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            this.server.ReplyDelay = Interval * 0.9;
            this.algo.KeepPrimaryAsync();
            await Task.Delay(Timeout * 3).ConfigureAwait(false);
            this.algo.Stop();
            this.server.ReplyDelay = TimeSpan.Zero;
           
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(10000)]
        public async Task KeepPrimaryWithaffinityTest1()
        {
            await this.algo.GetPrimaryAsync().ConfigureAwait(false);
            this.algo.KeepPrimaryAsync();
            await this.algo3.GetPrimaryAsync().ConfigureAwait(false);
            this.algo3.KeepPrimaryAsync();

            this.algo.Stop();
            await Task.Delay(Timeout * 2).ConfigureAwait(false);
            this.algo3.Stop();

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsFalse(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }
    }
}