// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommandLine.UnitTests.Processors
{
    using System;

    using Microsoft.VisualStudio.TestPlatform.CommandLine.Processors;
    using Microsoft.VisualStudio.TestPlatform.Common.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using vstest.console.UnitTests.TestDoubles;

    [TestClass]
    public class EnableLoggersArgumentProcessorTests
    {
        [TestInitialize]
        public void Initialize()
        {
            RunTestsArgumentProcessorTests.SetupMockExtensions();
        }

        [TestMethod]
        public void GetMetadataShouldReturnEnableLoggerArgumentProcessorCapabilities()
        {
            EnableLoggerArgumentProcessor processor = new EnableLoggerArgumentProcessor();
            Assert.IsTrue(processor.Metadata.Value is EnableLoggerArgumentProcessorCapabilities);
        }

        [TestMethod]
        public void GetExecuterShouldReturnEnableLoggerArgumentExecutor()
        {
            EnableLoggerArgumentProcessor processor = new EnableLoggerArgumentProcessor();
            Assert.IsTrue(processor.Executor.Value is EnableLoggerArgumentExecutor);
        }

        [TestMethod]
        public void CapabilitiesShouldAppropriateProperties()
        {
            EnableLoggerArgumentProcessorCapabilities capabilities = new EnableLoggerArgumentProcessorCapabilities();
            Assert.AreEqual("/Logger", capabilities.CommandName);
            Assert.AreEqual(("--logger|/logger:<Logger Uri/FriendlyName>" + Environment.NewLine + "      Specify a logger for test results.  For example, to log results into a " + Environment.NewLine + "      Visual Studio Test Results File (TRX) use  /logger:trx [;LogFileName=<Defaults to unique file name>]" + Environment.NewLine + "      Creates file in TestResults directory with given LogFileName." + Environment.NewLine + "" + Environment.NewLine + "      To publish test results to Team Foundation Server, use TfsPublisher as shown below" + Environment.NewLine + "      Example: /logger:TfsPublisher;" + Environment.NewLine + "                Collection=<team project collection url>;" + Environment.NewLine + "                BuildName=<build name>;" + Environment.NewLine + "                TeamProject=<team project name>" + Environment.NewLine + "                [;Platform=<Defaults to \"Any CPU\">]" + Environment.NewLine + "                [;Flavor=<Defaults to \"Debug\">]" + Environment.NewLine + "                [;RunTitle=<title>]").Replace("\r", string.Empty), capabilities.HelpContentResourceName.Replace("\r", string.Empty));

            Assert.AreEqual(HelpContentPriority.EnableLoggerArgumentProcessorHelpPriority, capabilities.HelpPriority);
            Assert.AreEqual(false, capabilities.IsAction);
            Assert.AreEqual(ArgumentProcessorPriority.Logging, capabilities.Priority);

            Assert.AreEqual(true, capabilities.AllowMultiple);
            Assert.AreEqual(false, capabilities.AlwaysExecute);
            Assert.AreEqual(false, capabilities.IsSpecialCommand);
        }

        [TestMethod]
        public void ExecutorInitializeWithNullOrEmptyArgumentsShouldThrowException()
        {
            var executor = new EnableLoggerArgumentExecutor(TestLoggerManager.Instance);
            Assert.ThrowsException<CommandLineException>(() =>
            {
                executor.Initialize(null);
            });
        }

        [TestMethod]
        public void ExecutorInitializeWithValidArgumentsShouldAddTestLoggerToTestLoggerManager()
        {
            RunTestsArgumentProcessorTests.SetupMockExtensions();
            var testloggerManager = new DummyTestLoggerManager();
            var executor = new EnableLoggerArgumentExecutor(testloggerManager);

            var countBefore = testloggerManager.GetInitializedLoggers.Count;

            executor.Initialize("TestLoggerExtension;Collection=http://localhost:8080/tfs/DefaultCollection;TeamProject=MyProject;BuildName=DailyBuild_20121130.1");
            var countAfter = testloggerManager.GetInitializedLoggers.Count;
            Assert.IsTrue(countBefore == 0);
            Assert.IsTrue(countAfter == 1);
        }

        [TestMethod]
        public void ExecutorInitializeWithValidArgumentsShouldAddConsoleloggerToTestLoggerManager()
        {
            RunTestsArgumentProcessorTests.SetupMockExtensions();
            var testloggerManager = new DummyTestLoggerManager();
            var executor = new EnableLoggerArgumentExecutor(testloggerManager);

            executor.Initialize("console;verbosity=minimal");
            Assert.IsTrue(testloggerManager.GetInitializedLoggers.Contains("logger://Microsoft/TestPlatform/ConsoleLogger/v2"));
        }

        [TestMethod]
        public void ExectorInitializeShouldThrowExceptionIfInvalidArgumentIsPassed()
        {
            var executor = new EnableLoggerArgumentExecutor(TestLoggerManager.Instance);
            Assert.ThrowsException<CommandLineException>(() =>
            {
                executor.Initialize("TestLoggerExtension;==;;;Collection=http://localhost:8080/tfs/DefaultCollection;TeamProject=MyProject;BuildName=DailyBuild_20121130.1");
            });
        }

        [TestMethod]
        public void ExecutorExecuteShouldReturnArgumentProcessorResultSuccess()
        {
            var executor = new EnableLoggerArgumentExecutor(null);
            var result = executor.Execute();
            Assert.AreEqual(ArgumentProcessorResult.Success, result);
        }
    }
}
