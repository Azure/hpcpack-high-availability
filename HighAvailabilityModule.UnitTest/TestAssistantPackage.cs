// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace HighAvailabilityModule.UnitTest
{
    using System.Collections.Generic;

    using HighAvailabilityModule.Interface;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    static class TestAssistantPackage
    {
        internal static void AssertCurrentEntry(Dictionary<string, HeartBeatEntry> Current, string Uuid, string Utype, string Uname)
        {
            Assert.IsTrue(Current != null);
            Assert.IsTrue(Current.ContainsKey(Utype));
            Assert.IsTrue(Current[Utype] != null);
            Assert.IsTrue(Current[Utype].Uuid == Uuid);
            Assert.IsTrue(Current[Utype].Utype == Utype);
            Assert.IsTrue(Current[Utype].Uname == Uname);
        }
    }
}
