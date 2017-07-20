﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TestPlatform.CrossPlatEngine.UnitTests.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Client;
    using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Client.Parallel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Host;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ParallelProxyDiscoveryManagerTests
    {
        private const int taskTimeout = 15 * 1000; // In milliseconds.
        private List<Mock<IProxyDiscoveryManager>> createdMockManagers;
        private Func<IProxyDiscoveryManager> proxyManagerFunc;
        private Mock<ITestDiscoveryEventsHandler> mockHandler;
        private List<string> sources = new List<string>() { "1.dll", "2.dll" };
        private DiscoveryCriteria testDiscoveryCriteria;
        private bool proxyManagerFuncCalled;
        private List<string> processedSources;
        private ManualResetEventSlim discoveryCompleted;

        public ParallelProxyDiscoveryManagerTests()
        {
            this.processedSources = new List<string>();
            this.createdMockManagers = new List<Mock<IProxyDiscoveryManager>>();
            this.proxyManagerFunc = () =>
            {
                this.proxyManagerFuncCalled = true;
                var manager = new Mock<IProxyDiscoveryManager>();
                this.createdMockManagers.Add(manager);
                return manager.Object;
            };
            this.mockHandler = new Mock<ITestDiscoveryEventsHandler>();
            this.testDiscoveryCriteria = new DiscoveryCriteria(sources, 100, null);
            this.discoveryCompleted = new ManualResetEventSlim(false);
        }

        [TestMethod]
        public void InitializeShouldCallAllConcurrentManagersOnce()
        {
            var parallelDiscoveryManager = new ParallelProxyDiscoveryManager(this.proxyManagerFunc, 3, false);

            parallelDiscoveryManager.Initialize();

            Assert.AreEqual(3, createdMockManagers.Count, "Number of Concurrent Managers created should be 3");
            createdMockManagers.ForEach(dm => dm.Verify(m => m.Initialize(), Times.Once));
        }

        [TestMethod]
        public void AbortShouldCallAllConcurrentManagersOnce()
        {
            var parallelDiscoveryManager = new ParallelProxyDiscoveryManager(this.proxyManagerFunc, 4, false);

            parallelDiscoveryManager.Abort();

            Assert.AreEqual(4, createdMockManagers.Count, "Number of Concurrent Managers created should be 4");
            createdMockManagers.ForEach(dm => dm.Verify(m => m.Abort(), Times.Once));
        }

        [TestMethod]
        public void DiscoverTestsShouldProcessAllSources()
        {
            var parallelDiscoveryManager = this.SetupDiscoveryManager(this.proxyManagerFunc, 2, false);

            Task.Run(() =>
            {
                parallelDiscoveryManager.DiscoverTests(this.testDiscoveryCriteria, this.mockHandler.Object);
            });

            Assert.IsTrue(this.discoveryCompleted.Wait(ParallelProxyDiscoveryManagerTests.taskTimeout), "Test discovery not completed.");
            Assert.AreEqual(sources.Count, processedSources.Count, "All Sources must be processed.");
            AssertMissingAndDuplicateSources(processedSources);
        }

        /// <summary>
        ///  Create ParallelProxyDiscoveryManager with parallel level 1 and two source,
        ///  Abort in any source should not stop discovery for other sources.
        /// </summary>
        [TestMethod]
        public void DiscoveryTestsShouldProcessAllSourcesOnDiscoveryAbortsForAnySource()
        {
            // Since the hosts are aborted, total aggregated tests sent across will be -1
            var discoveryManagerMock = new Mock<IProxyDiscoveryManager>();
            this.createdMockManagers.Add(discoveryManagerMock);
            var parallelDiscoveryManager = this.SetupDiscoveryManager(() => discoveryManagerMock.Object, 1, true, totalTests: -1);

            Task.Run(() =>
            {
                parallelDiscoveryManager.DiscoverTests(this.testDiscoveryCriteria, this.mockHandler.Object);
            });

            Assert.IsTrue(this.discoveryCompleted.Wait(ParallelProxyDiscoveryManagerTests.taskTimeout), "Test discovery not completed.");
            Assert.AreEqual(2, processedSources.Count, "All Sources must be processed.");
        }

        [TestMethod]
        public void DiscoveryTestsShouldProcessAllSourceIfOneDiscoveryManagerIsStarved()
        {
            // Ensure that second discovery manager never starts. Expect 10 total tests.
            // Override DiscoveryComplete since overall aborted should be true
            var parallelDiscoveryManager = this.SetupDiscoveryManager(this.proxyManagerFunc, 2, false, totalTests: 10);
            this.createdMockManagers[1].Reset();
            this.createdMockManagers[1].Setup(dm => dm.DiscoverTests(It.IsAny<DiscoveryCriteria>(), It.IsAny<ITestDiscoveryEventsHandler>()))
                .Throws<NotImplementedException>();
            this.mockHandler.Setup(mh => mh.HandleDiscoveryComplete(-1, null, true))
                .Callback<long, IEnumerable<TestCase>, bool>((t, l, a) => { this.discoveryCompleted.Set(); });

            Task.Run(() =>
            {
                parallelDiscoveryManager.DiscoverTests(this.testDiscoveryCriteria, this.mockHandler.Object);
            });

            // Processed sources should be 1 since the 2nd source is never discovered
            Assert.IsTrue(this.discoveryCompleted.Wait(ParallelProxyDiscoveryManagerTests.taskTimeout), "Test discovery not completed.");
            Assert.AreEqual(1, processedSources.Count, "All Sources must be processed.");
        }

        [TestMethod]
        public void HandlePartialDiscoveryCompleteShouldCreateANewProxyDiscoveryManagerIfIsAbortedIsTrue()
        {
            this.proxyManagerFuncCalled = false;
            var parallelDiscoveryManager = new ParallelProxyDiscoveryManager(this.proxyManagerFunc, 1, false);
            var proxyDiscovermanager = new ProxyDiscoveryManager(new Mock<ITestRequestSender>().Object, new Mock<ITestRuntimeProvider>().Object);

            parallelDiscoveryManager.HandlePartialDiscoveryComplete(proxyDiscovermanager, 20, new List<TestCase>(), isAborted: true);

            Assert.IsTrue(this.proxyManagerFuncCalled);
        }

        private IParallelProxyDiscoveryManager SetupDiscoveryManager(Func<IProxyDiscoveryManager> getProxyManager, int parallelLevel, bool abortDiscovery, int totalTests = 20)
        {
            var parallelDiscoveryManager = new ParallelProxyDiscoveryManager(getProxyManager, parallelLevel, false);
            this.SetupDiscoveryTests(this.processedSources, abortDiscovery);

            // Setup a complete handler for parallel discovery manager
            this.mockHandler.Setup(mh => mh.HandleDiscoveryComplete(totalTests, null, abortDiscovery))
                .Callback<long, IEnumerable<TestCase>, bool>(
                    (totalTests1, lastChunk, aborted) => { this.discoveryCompleted.Set(); });

            return parallelDiscoveryManager;
        }

        private void SetupDiscoveryTests(List<string> processedSources, bool isAbort)
        {
            var syncObject = new object();
            foreach (var manager in this.createdMockManagers)
            {
                manager.Setup(m => m.DiscoverTests(It.IsAny<DiscoveryCriteria>(), It.IsAny<ITestDiscoveryEventsHandler>())).
                    Callback<DiscoveryCriteria, ITestDiscoveryEventsHandler>(
                        (criteria, handler) =>
                        {
                            lock (syncObject)
                            {
                                processedSources.AddRange(criteria.Sources);
                            }

                            Task.Delay(100).Wait();

                            handler.HandleDiscoveryComplete(isAbort ? -1 : 10, null, isAbort);
                        });
            }
        }

        private void AssertMissingAndDuplicateSources(List<string> processedSources)
        {
            foreach (var source in this.sources)
            {
                bool matchFound = false;

                foreach (var processedSrc in processedSources)
                {
                    if (processedSrc.Equals(source))
                    {
                        if (matchFound)
                        {
                            Assert.Fail("Concurrreny issue detected: Source['{0}'] got processed twice", processedSrc);
                        }

                        matchFound = true;
                    }
                }

                Assert.IsTrue(matchFound, "Concurrency issue detected: Source['{0}'] did NOT get processed at all", source);
            }
        }
    }
}