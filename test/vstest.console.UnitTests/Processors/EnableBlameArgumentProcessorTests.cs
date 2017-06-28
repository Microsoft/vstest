﻿using Microsoft.VisualStudio.TestPlatform.CommandLine.Processors;
using Microsoft.VisualStudio.TestPlatform.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using vstest.console.UnitTests.TestDoubles;

namespace vstest.console.UnitTests.Processors
{
    [TestClass]
    public class EnableBlameArgumentProcessorTests
    {
        private TestableRunSettingsProvider settingsProvider;
        private EnableBlameArgumentExecutor executor;
        private DummyTestLoggerManager testloggerManager;
        private const string DefaultRunSettings = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<RunSettings>\r\n  <DataCollectionRunSettings>\r\n    <DataCollectors >{0}</DataCollectors>\r\n  </DataCollectionRunSettings>\r\n</RunSettings>";

        public EnableBlameArgumentProcessorTests()
        {
            this.settingsProvider = new TestableRunSettingsProvider();
            this.testloggerManager = new DummyTestLoggerManager();
            this.executor = new EnableBlameArgumentExecutor(this.settingsProvider,this.testloggerManager);
            CollectArgumentExecutor.EnabledDataCollectors.Clear();
        }

        [TestMethod]
        public void GetMetadataShouldReturnEnableBlameArgumentProcessorCapabilities()
        {
            var processor = new EnableBlameArgumentProcessor();
            Assert.IsTrue(processor.Metadata.Value is EnableBlameArgumentProcessorCapabilities);
        }

        [TestMethod]
        public void GetExecuterShouldReturnEnableBlameArgumentProcessorCapabilities()
        {
            var processor = new EnableBlameArgumentProcessor();
            Assert.IsTrue(processor.Executor.Value is EnableBlameArgumentExecutor);
        }

        [TestMethod]
        public void CapabilitiesShouldReturnAppropriateProperties()
        {
            var capabilities = new EnableBlameArgumentProcessorCapabilities();

            Assert.AreEqual("/Blame", capabilities.CommandName);
            Assert.AreEqual(false, capabilities.IsAction);
            Assert.AreEqual(ArgumentProcessorPriority.Diag, capabilities.Priority);

            Assert.AreEqual(false, capabilities.AllowMultiple);
            Assert.AreEqual(false, capabilities.AlwaysExecute);
            Assert.AreEqual(false, capabilities.IsSpecialCommand);
        }

        [TestMethod]
        public void InitializeShouldCreateEntryForBlameInRunSettingsIfNotAlreadyPresent()
        {
            var runsettingsString = string.Format(DefaultRunSettings, "");
            var runsettings = new RunSettings();
            runsettings.LoadSettingsXml(runsettingsString);
            this.settingsProvider.SetActiveRunSettings(runsettings);

            this.executor.Initialize("blame");

            Assert.IsNotNull(this.settingsProvider.ActiveRunSettings);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<RunSettings>\r\n  <DataCollectionRunSettings>\r\n    <DataCollectors>\r\n      <DataCollector friendlyName=\"blame\" enabled=\"True\" />\r\n    </DataCollectors>\r\n  </DataCollectionRunSettings>\r\n</RunSettings>", this.settingsProvider.ActiveRunSettings.SettingsXml);
        }
        [TestMethod]
        public void InitializeShouldNotDisableOtherDataCollectors()
        {
            var runsettingsString = string.Format(DefaultRunSettings, "<DataCollector friendlyName=\"blame\" enabled=\"False\" /><DataCollector friendlyName=\"MyDataCollector1\" enabled=\"True\" />");
            var runsettings = new RunSettings();
            runsettings.LoadSettingsXml(runsettingsString);
            this.settingsProvider.SetActiveRunSettings(runsettings);

            this.executor.Initialize("blame");

            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<RunSettings>\r\n  <DataCollectionRunSettings>\r\n    <DataCollectors>\r\n      <DataCollector friendlyName=\"blame\" enabled=\"True\" />\r\n      <DataCollector friendlyName=\"MyDataCollector1\" enabled=\"True\" />\r\n    </DataCollectors>\r\n  </DataCollectionRunSettings>\r\n</RunSettings>", this.settingsProvider.ActiveRunSettings.SettingsXml);
        }

        [TestMethod]
        public void ExecutorInitializeWithBlameArgumentsShouldAddBlameloggerToTestLoggerManager()
        {
            executor.Initialize("blame;Collection=http://localhost:8080/tfs/DefaultCollection;TeamProject=MyProject;BuildName=DailyBuild_20121130.1");

            Assert.IsTrue(testloggerManager.LoggerExist("blame"));
        }
    }
}
