// Copyright (c) Microsoft. All rights reserved.

namespace TestPlatform.CrossPlatEngine.UnitTests.Discovery
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    
    using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;
    using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
    using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Discovery;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using TestPlatform.Common.UnitTests.ExtensionFramework;

    [TestClass]
    public class DiscoveryManagerTests
    {
        private DiscoveryManager discoveryManager;

        [TestInitialize]
        public void TestInit()
        {
            this.discoveryManager = new DiscoveryManager();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestDiscoveryExtensionManager.Destroy();
            TestPluginCache.Instance = null;
        }

        #region Initialize tests

        [TestMethod]
        public void InitializeShouldUpdateAdditionalExtenions()
        {
            var testableTestPluginCache = TestPluginCacheTests.SetupMockTestPluginCache();

            // Stub the default extensions folder.
            testableTestPluginCache.DoesDirectoryExistSetter = false;

            this.discoveryManager.Initialize(
                new string[] { typeof(TestPluginCacheTests).GetTypeInfo().Assembly.Location });

            var allDiscoverers = TestDiscoveryExtensionManager.Create().Discoverers;

            Assert.IsNotNull(allDiscoverers);
            Assert.IsTrue(allDiscoverers.Count() > 0);
        }

        #endregion

        #region DiscoverTests tests

        [TestMethod]
        public void DiscoverTestsShouldLogIfTheSourceDoesNotExist()
        {
            var criteria = new DiscoveryCriteria(new List<string> { "imaginary.dll" }, 100, null);
            var mockLogger = new Mock<ITestDiscoveryEventsHandler>();

            this.discoveryManager.DiscoverTests(criteria, mockLogger.Object);

            var errorMessage = string.Format(CultureInfo.CurrentCulture, "Could not find file {0}.", "imaginary.dll");
            mockLogger.Verify(
                l =>
                l.HandleLogMessage(
                    Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Warning,
                    errorMessage),
                Times.Once);
        }

        [TestMethod]
        public void DiscoverTestsShouldLogIfThereAreNoValidSources()
        {
            var sources = new List<string> { "imaginary.dll" };
            var criteria = new DiscoveryCriteria(sources, 100, null);
            var mockLogger = new Mock<ITestDiscoveryEventsHandler>();

            this.discoveryManager.DiscoverTests(criteria, mockLogger.Object);

            var sourcesString = string.Join(",", sources.ToArray());
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Resources.NoValidSourceFoundForDiscovery, sourcesString);
            mockLogger.Verify(
                l =>
                l.HandleLogMessage(
                    Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Warning,
                    errorMessage),
                Times.Once);
        }

        [TestMethod]
        public void DiscoverTestsShouldLogIfTheSameSourceIsSpecifiedTwice()
        {
            TestPluginCacheTests.SetupMockExtensions(
                new string[] { typeof(DiscovererEnumeratorTests).GetTypeInfo().Assembly.Location },
                () => { });

            var sources = new List<string>
                              {
                                  typeof(DiscoveryManagerTests).GetTypeInfo().Assembly.Location,
                                  typeof(DiscoveryManagerTests).GetTypeInfo().Assembly.Location
                              };

            var criteria = new DiscoveryCriteria(sources, 100, null);
            var mockLogger = new Mock<ITestDiscoveryEventsHandler>();

            this.discoveryManager.DiscoverTests(criteria, mockLogger.Object);

            var errorMessage = string.Format(CultureInfo.CurrentCulture, Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Resources.DuplicateSource, sources[0]);
            mockLogger.Verify(
                l =>
                l.HandleLogMessage(
                    Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Warning,
                    errorMessage),
                Times.Once);
        }

        [TestMethod]
        public void DiscoverTestsShouldDiscoverTestsInTheSpecifiedSource()
        {
            TestPluginCacheTests.SetupMockExtensions(
                new string[] { typeof(DiscovererEnumeratorTests).GetTypeInfo().Assembly.Location },
                () => { });

            var sources = new List<string>
                              {
                                  typeof(DiscoveryManagerTests).GetTypeInfo().Assembly.Location
                              };

            var criteria = new DiscoveryCriteria(sources, 1, null);
            var mockLogger = new Mock<ITestDiscoveryEventsHandler>();

            this.discoveryManager.DiscoverTests(criteria, mockLogger.Object);
            
            // Assert that the tests are passed on via the handletestruncomplete event.
            mockLogger.Verify(l => l.HandleDiscoveryComplete(1, It.IsAny<IEnumerable<TestCase>>(), false), Times.Once);
        }

        #endregion
    }
}
