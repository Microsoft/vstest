﻿﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.ObjectModel.UnitTests.Client
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    [TestClass]
    public class DiscoveryCriteriaTests
    {
        private readonly DiscoveryCriteria discoveryCriteria;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                      {
                                                                          TypeNameHandling = TypeNameHandling.None
                                                                      };

        public DiscoveryCriteriaTests()
        {
            this.discoveryCriteria = new DiscoveryCriteria(
                                         new[] { "sampleTest.dll" },
                                         100,
                                         "<RunConfiguration></RunConfiguration>");
        }

        [TestMethod]
        public void DiscoveryCriteriaSerializesToExpectedJson()
        {
            var expectedJson = "{\"Sources\":[\"sampleTest.dll\"],\"AdapterSourceMap\":{\"_none_\":[\"sampleTest.dll\"]},\"FrequencyOfDiscoveredTestsEvent\":100,\"DiscoveredTestEventTimeout\":\"10675199.02:48:05.4775807\",\"RunSettings\":\"<RunConfiguration></RunConfiguration>\"}";

            var json = JsonConvert.SerializeObject(this.discoveryCriteria, Settings);

            Assert.AreEqual(expectedJson, json);
        }

        [TestMethod]
        public void DiscoveryCriteriaShouldBeDeserializable()
        {
            var json = "{\"Sources\":[\"sampleTest.dll\"],\"AdapterSourceMap\":{\"_none_\":[\"sampleTest.dll\"]},\"FrequencyOfDiscoveredTestsEvent\":100,\"DiscoveredTestEventTimeout\":\"10675199.02:48:05.4775807\",\"RunSettings\":\"<RunConfiguration></RunConfiguration>\"}";

            var criteria = JsonConvert.DeserializeObject<DiscoveryCriteria>(json, Settings);

            Assert.AreEqual(TimeSpan.MaxValue, criteria.DiscoveredTestEventTimeout);
            Assert.AreEqual(100, criteria.FrequencyOfDiscoveredTestsEvent);
            Assert.AreEqual("<RunConfiguration></RunConfiguration>", criteria.RunSettings);
            Assert.AreEqual("sampleTest.dll", criteria.AdapterSourceMap["_none_"].Single());
            CollectionAssert.AreEqual(new[] { "sampleTest.dll" }, criteria.Sources.ToArray());
        }
    }
}
