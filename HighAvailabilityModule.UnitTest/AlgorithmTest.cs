// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.UnitTest
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Diagnostics;

    using HighAvailabilityModule.Algorithm;
    using HighAvailabilityModule.Client.InMemory;
    using HighAvailabilityModule.Interface;
    using HighAvailabilityModule.Server.InMemory;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AlgorithmTest
    {
        private static TimeSpan Timeout => TimeSpan.FromSeconds(1.5);

        private static TimeSpan Interval => TimeSpan.FromSeconds(0.5);

        private static DateTime Now { get; } = DateTime.Parse("2019-09-27T12:00:00.2965246Z");

        private static string Client1Uuid => "cdca5b45-6ea1-4d91-81f6-d39f4821e791";
        private static string Client2Uuid => "39a78df0-e101-49b9-8c56-ec2fea2e47df";
        private static string Client3Uuid => "33253ab7-27b6-478c-a359-4eca7df83b80";

        private static string ClientUtypeA => "A";
        private static string ClientUtypeB => "B";

        private static string ClientUname1 => "1";
        private static string ClientUname2 => "2";

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
            this.algo = new MembershipWithWitness(this.client, Interval, Timeout);

            this.client2 = new InMemoryMembershipClient(this.server, Client2Uuid, ClientUtypeA, ClientUname2);
            this.algo2 = new MembershipWithWitness(this.client2, Interval, Timeout);

            this.client3 = new InMemoryMembershipClient(this.server, Client3Uuid, ClientUtypeB, ClientUname1);
            this.algo3 = new MembershipWithWitness(this.client3, Interval, Timeout);
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest1()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            Assert.IsFalse(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest2()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            await this.algo.CheckPrimaryAsync(Now);
            Assert.IsFalse(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest3()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, Now));
            await this.algo.CheckPrimaryAsync(Now);
            Assert.IsTrue(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest4()
        {
            DateTime now = DateTime.UtcNow;
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, now - Timeout));
            await this.algo.CheckPrimaryAsync(now);
            Assert.IsFalse(this.algo.RunningAsPrimary(now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest5()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, Now));
            await this.algo.CheckPrimaryAsync(Now - Timeout + Interval);
            Assert.IsFalse(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest6()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA, ClientUname1, Now));
            await this.algo.CheckPrimaryAsync(Now);
            await this.algo.CheckPrimaryAsync(Now - Timeout + Interval);
            Assert.IsTrue(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest7()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA,ClientUname1, Now));
            Assert.IsFalse(this.algo.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task RunningAsPrimaryTest8()
        {
            this.server.CurrentTable = new Dictionary<string, HeartBeatEntry>();
            this.server.CurrentTable.Add(ClientUtypeA, new HeartBeatEntry(Client1Uuid, ClientUtypeA,ClientUname1, Now));
            await this.algo3.CheckPrimaryAsync(Now);
            Assert.IsFalse(this.algo3.RunningAsPrimary(Now));
        }

        [TestMethod]
        public async Task HeartBeatAsPrimaryTest1()
        {
            await this.algo.CheckPrimaryAsync(Now);
            await this.algo.HeartBeatAsPrimaryAsync();
            TestAssistantPackage.AssertCurrentEntry(this.server.CurrentTable, Client1Uuid, ClientUtypeA, ClientUname1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task HeartBeatAsPrimaryTest2()
        {
            await this.algo.HeartBeatAsPrimaryAsync();
        }

        [TestMethod]
        public async Task HeartBeatAndCheckTest1()
        {
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow);
            await this.algo.HeartBeatAsPrimaryAsync();
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow);
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        public async Task HeartBeatAndCheckTest2()
        {
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow);
            await this.algo.HeartBeatAsPrimaryAsync();
            await this.algo.CheckPrimaryAsync(DateTime.UtcNow);
            await this.algo3.CheckPrimaryAsync(DateTime.UtcNow);
            await this.algo3.HeartBeatAsPrimaryAsync();
            await this.algo3.CheckPrimaryAsync(DateTime.UtcNow);
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo3.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(2000)]
        public async Task GetPrimaryTest1()
        {
            await this.algo.GetPrimaryAsync();
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(2000)]
        public async Task GetPrimaryTest2()
        {
            var getPrimaryTask = this.algo.GetPrimaryAsync();
            this.algo.Stop();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => getPrimaryTask);
            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest3()
        {
            await this.algo.GetPrimaryAsync();
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
            await this.algo.GetPrimaryAsync();
            await this.algo2.GetPrimaryAsync();

            this.algo.Stop();
            this.algo2.Stop();

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
            Assert.IsTrue(this.algo2.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest5()
        {
            await this.algo.GetPrimaryAsync();
            this.server.RemoveCurrent();
            await this.algo.KeepPrimaryAsync();

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task GetPrimaryTest6()
        {
            await this.algo.GetPrimaryAsync();
            var task = this.algo.KeepPrimaryAsync();
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            this.server.RemoveCurrent();
            await task;

            Assert.IsFalse(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }

        /// <summary>
        /// Simulate network latency is at Interval * 0.9
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Timeout(10000)]
        public async Task KeepPrimaryTest1()
        {
            await this.algo.GetPrimaryAsync();
            this.server.ReplyDelay = Interval * 0.9;
            this.algo.KeepPrimaryAsync();
            await Task.Delay(Timeout * 3).ConfigureAwait(false);
            this.algo.Stop();
            this.server.ReplyDelay = TimeSpan.Zero;
           
            Assert.IsTrue(this.algo.RunningAsPrimary(DateTime.UtcNow));
        }
    }
}