﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.Extensions.BlameDataCollector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

    /// <summary>
    /// The blame collector.
    /// </summary>
    [DataCollectorFriendlyName("Blame")]
    [DataCollectorTypeUri("datacollector://Microsoft/TestPlatform/Extensions/Blame/v1")]
    public class BlameCollector : DataCollector, ITestExecutionEnvironmentSpecifier
    {
        private const int DefaultInactivityTimeInMinutes = 60;

        private DataCollectionSink dataCollectionSink;
        private DataCollectionEnvironmentContext context;
        private DataCollectionEvents events;
        private DataCollectionLogger logger;
        private IProcessDumpUtility processDumpUtility;
        private List<Guid> testSequence;
        private Dictionary<Guid, BlameTestObject> testObjectDictionary;
        private IBlameReaderWriter blameReaderWriter;
        private XmlElement configurationElement;
        private int testStartCount;
        private int testEndCount;
        private bool collectProcessDumpCollectionBasedOnTrigger;
        private bool collectProcessDumpOnTestHostHang;
        private bool collectDumpAlways;
        private bool processFullDumpEnabled;
        private bool inactivityTimerAlreadyFired;
        private string attachmentGuid;
        private Timer inactivityTimer;
        private TimeSpan inactivityTimespan = TimeSpan.FromMinutes(DefaultInactivityTimeInMinutes);
        private int testHostProcessId;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlameCollector"/> class.
        /// Using XmlReaderWriter by default
        /// </summary>
        public BlameCollector()
            : this(new XmlReaderWriter(), new ProcessDumpUtility())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlameCollector"/> class.
        /// </summary>
        /// <param name="blameReaderWriter">
        /// BlameReaderWriter instance.
        /// </param>
        /// <param name="processDumpUtility">
        /// ProcessDumpUtility instance.
        /// </param>
        internal BlameCollector(IBlameReaderWriter blameReaderWriter, IProcessDumpUtility processDumpUtility)
        {
            this.blameReaderWriter = blameReaderWriter;
            this.processDumpUtility = processDumpUtility;
        }

        /// <summary>
        /// Gets environment variables that should be set in the test execution environment
        /// </summary>
        /// <returns>Environment variables that should be set in the test execution environment</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTestExecutionEnvironmentVariables()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Initializes parameters for the new instance of the class <see cref="BlameDataCollector"/>
        /// </summary>
        /// <param name="configurationElement">The Xml Element to save to</param>
        /// <param name="events">Data collection events to which methods subscribe</param>
        /// <param name="dataSink">A data collection sink for data transfer</param>
        /// <param name="logger">Data Collection Logger to send messages to the client </param>
        /// <param name="environmentContext">Context of data collector environment</param>
        public override void Initialize(
            XmlElement configurationElement,
            DataCollectionEvents events,
            DataCollectionSink dataSink,
            DataCollectionLogger logger,
            DataCollectionEnvironmentContext environmentContext)
        {
            if (string.Equals(Environment.GetEnvironmentVariable("DEBUG_BLAMEDATACOLLECTOR"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Debugger.Launch();
            }

            ValidateArg.NotNull(logger, nameof(logger));

            this.events = events;
            this.dataCollectionSink = dataSink;
            this.context = environmentContext;
            this.configurationElement = configurationElement;
            this.testSequence = new List<Guid>();
            this.testObjectDictionary = new Dictionary<Guid, BlameTestObject>();
            this.logger = logger;

            // Subscribing to events
            this.events.TestHostLaunched += this.TestHostLaunchedHandler;
            this.events.SessionEnd += this.SessionEndedHandler;
            this.events.TestCaseStart += this.EventsTestCaseStart;
            this.events.TestCaseEnd += this.EventsTestCaseEnd;

            if (this.configurationElement != null)
            {
                var collectDumpNode = this.configurationElement[Constants.DumpModeKey];
                this.collectProcessDumpCollectionBasedOnTrigger = collectDumpNode != null;
                if (this.collectProcessDumpCollectionBasedOnTrigger)
                {
                    this.ValidateAndAddProcessDumpParameters(collectDumpNode);
                }
            }

            this.attachmentGuid = Guid.NewGuid().ToString().Replace("-", string.Empty);

            if (this.collectProcessDumpOnTestHostHang)
            {
                this.inactivityTimer = new Timer((object state) => { this.CollectDumpAndAbortTesthost(); }, null, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
            }
        }

        /// <summary>
        /// Disposes of the timer when called to prevent further calls.
        /// Kills the other instance of procdump if launched for collecting trigger based dumps.
        /// Starts and waits for a new proc dump process to collect a single dump and then
        /// kills the testhost process.
        /// </summary>
        private void CollectDumpAndAbortTesthost()
        {
            EqtTrace.Info(string.Format(CultureInfo.CurrentUICulture, Resources.Resources.InactivityTimeout, (int)this.inactivityTimespan.TotalMinutes));
            this.inactivityTimerAlreadyFired = true;

            try
            {
                EqtTrace.Verbose("Calling dispose on Inactivity timer.");
                this.inactivityTimer.Dispose();
            }
            catch
            {
                EqtTrace.Verbose("Inactivity timer is already disposed.");
            }

            if (this.collectProcessDumpCollectionBasedOnTrigger)
            {
                // Detach procdump from the testhost process to prevent testhost process from crashing
                // if/when we try to kill the existing proc dump process.
                new Win32NamedEvent($"Procdump-{this.testHostProcessId}");
            }

            try
            {
                this.processDumpUtility.StartHangBasedProcessDump(this.testHostProcessId, this.attachmentGuid, this.GetResultsDirectory(), this.processFullDumpEnabled);
            }
            catch (Exception e)
            {
                EqtTrace.Error($"BlameCollector.CollectDumpAndAbortTesthost: Failed with error {e}");
            }

            try
            {
                var dumpFile = this.processDumpUtility.GetDumpFile();
                if (!string.IsNullOrEmpty(dumpFile))
                {
                    var fileTransferInformation = new FileTransferInformation(this.context.SessionDataCollectionContext, dumpFile, true);
                    this.dataCollectionSink.SendFileAsync(fileTransferInformation);
                }
                else
                {
                    EqtTrace.Error("BlameCollector.CollectDumpAndAbortTesthost: blame:CollectDumpOnHang was enabled but dump file was not generated.");
                }
            }
            catch (FileNotFoundException ex)
            {
                EqtTrace.Error(ex.Message);
            }

            Process.GetProcessById(this.testHostProcessId).Kill();
        }

        private void ValidateAndAddProcessDumpParameters(XmlElement collectDumpNode)
        {
            foreach (XmlAttribute attribute in collectDumpNode.Attributes)
            {
                if (string.Equals(attribute.Name, Constants.CollectDumpAlwaysKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(attribute.Value, Constants.TrueConfigurationValue, StringComparison.OrdinalIgnoreCase) || string.Equals(attribute.Value, Constants.FalseConfigurationValue, StringComparison.OrdinalIgnoreCase))
                    {
                        bool.TryParse(attribute.Value, out this.collectDumpAlways);
                    }
                    else
                    {
                        this.logger.LogWarning(this.context.SessionDataCollectionContext, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.BlameParameterValueIncorrect, attribute.Name, Constants.TrueConfigurationValue, Constants.FalseConfigurationValue));
                    }
                }
                else if (string.Equals(attribute.Name, Constants.DumpTypeKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(attribute.Value, Constants.FullConfigurationValue, StringComparison.OrdinalIgnoreCase) || string.Equals(attribute.Value, Constants.MiniConfigurationValue, StringComparison.OrdinalIgnoreCase))
                    {
                        this.processFullDumpEnabled = string.Equals(attribute.Value, Constants.FullConfigurationValue, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        this.logger.LogWarning(this.context.SessionDataCollectionContext, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.BlameParameterValueIncorrect, attribute.Name, Constants.FullConfigurationValue, Constants.MiniConfigurationValue));
                    }
                }
                else if (string.Equals(attribute.Name, Constants.CollectDumpOnTestSessionHang, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(attribute.Value, Constants.TrueConfigurationValue, StringComparison.OrdinalIgnoreCase) || string.Equals(attribute.Value, Constants.FalseConfigurationValue, StringComparison.OrdinalIgnoreCase))
                    {
                        bool.TryParse(attribute.Value, out this.collectProcessDumpOnTestHostHang);
                    }
                    else
                    {
                        this.logger.LogWarning(this.context.SessionDataCollectionContext, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.BlameParameterValueIncorrect, attribute.Name, Constants.TrueConfigurationValue, Constants.FalseConfigurationValue));
                    }
                }
                else if (string.Equals(attribute.Name, Constants.ExpectedExecutionTimeOfLongestRunningTestInMinutes, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(attribute.Value) && int.TryParse(attribute.Value, out int inactivityTimespanInMinutes))
                    {
                        this.inactivityTimespan = TimeSpan.FromMinutes(inactivityTimespanInMinutes);
                    }
                    else
                    {
                        this.logger.LogWarning(this.context.SessionDataCollectionContext, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.UnexpectedValueForInactivityTimespanValue, attribute.Value));
                    }
                }
                else
                {
                    this.logger.LogWarning(this.context.SessionDataCollectionContext, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.BlameParameterKeyIncorrect, attribute.Name));
                }
            }
        }

        /// <summary>
        /// Called when Test Case Start event is invoked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">TestCaseStartEventArgs</param>
        private void EventsTestCaseStart(object sender, TestCaseStartEventArgs e)
        {
            this.ResetInactivityTimer();

            if (EqtTrace.IsInfoEnabled)
            {
                EqtTrace.Info("Blame Collector : Test Case Start");
            }

            var blameTestObject = new BlameTestObject(e.TestElement);

            // Add guid to list of test sequence to maintain the order.
            this.testSequence.Add(blameTestObject.Id);

            // Add the test object to the dictionary.
            this.testObjectDictionary.Add(blameTestObject.Id, blameTestObject);

            // Increment test start count.
            this.testStartCount++;
        }

        /// <summary>
        /// Called when Test Case End event is invoked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">TestCaseEndEventArgs</param>
        private void EventsTestCaseEnd(object sender, TestCaseEndEventArgs e)
        {
            this.ResetInactivityTimer();

            if (EqtTrace.IsInfoEnabled)
            {
                EqtTrace.Info("Blame Collector : Test Case End");
            }

            this.testEndCount++;

            // Update the test object in the dictionary as the test has completed.
            if (this.testObjectDictionary.ContainsKey(e.TestElement.Id))
            {
                this.testObjectDictionary[e.TestElement.Id].IsCompleted = true;
            }
        }

        /// <summary>
        /// Called when Session End event is invoked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">SessionEndEventArgs</param>
        private void SessionEndedHandler(object sender, SessionEndEventArgs args)
        {
            this.ResetInactivityTimer();

            if (EqtTrace.IsInfoEnabled)
            {
                EqtTrace.Info("Blame Collector : Session End");
            }

            try
            {
                // If the last test crashes, it will not invoke a test case end and therefore
                // In case of crash testStartCount will be greater than testEndCount and we need to write the sequence
                // And send the attachment
                if (this.testStartCount > this.testEndCount)
                {
                    var filepath = Path.Combine(this.GetResultsDirectory(), Constants.AttachmentFileName + "_" + this.attachmentGuid);
                    filepath = this.blameReaderWriter.WriteTestSequence(this.testSequence, this.testObjectDictionary, filepath);
                    var fileTranferInformation = new FileTransferInformation(this.context.SessionDataCollectionContext, filepath, true);
                    this.dataCollectionSink.SendFileAsync(fileTranferInformation);
                }

                if (this.collectProcessDumpCollectionBasedOnTrigger)
                {
                    // If there was a test case crash or if we need to collect dump on process exit.
                    if (this.testStartCount > this.testEndCount || this.collectDumpAlways)
                    {
                        try
                        {
                            var dumpFile = this.processDumpUtility.GetDumpFile();
                            if (!string.IsNullOrEmpty(dumpFile))
                            {
                                var fileTranferInformation = new FileTransferInformation(this.context.SessionDataCollectionContext, dumpFile, true);
                                this.dataCollectionSink.SendFileAsync(fileTranferInformation);
                            }
                            else
                            {
                                EqtTrace.Warning("BlameCollector.SessionEndedHandler: blame:CollectDump was enabled but dump file was not generated.");
                                this.logger.LogWarning(args.Context, Resources.Resources.ProcDumpNotGenerated);
                            }
                        }
                        catch (FileNotFoundException ex)
                        {
                            EqtTrace.Warning(ex.Message);
                            this.logger.LogWarning(args.Context, ex.Message);
                        }
                    }
                }
            }
            finally
            {
                // Attempt to terminate the proc dump process if proc dump was enabled
                if (this.collectProcessDumpCollectionBasedOnTrigger)
                {
                    this.processDumpUtility.TerminateProcess();
                }

                this.DeregisterEvents();
            }
        }

        /// <summary>
        /// Called when Test Host Initialized is invoked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">TestHostLaunchedEventArgs</param>
        private void TestHostLaunchedHandler(object sender, TestHostLaunchedEventArgs args)
        {
            this.ResetInactivityTimer();
            this.testHostProcessId = args.TestHostProcessId;

            if (!this.collectProcessDumpCollectionBasedOnTrigger)
            {
                return;
            }

            try
            {
                this.processDumpUtility.StartTriggerBasedProcessDump(args.TestHostProcessId, this.attachmentGuid, this.GetResultsDirectory(), this.processFullDumpEnabled);
            }
            catch (TestPlatformException e)
            {
                if (EqtTrace.IsWarningEnabled)
                {
                    EqtTrace.Warning("BlameCollector.TestHostLaunchedHandler: Could not start process dump. {0}", e);
                }

                this.logger.LogWarning(args.Context, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.ProcDumpCouldNotStart, e.Message));
            }
            catch (Exception e)
            {
                if (EqtTrace.IsWarningEnabled)
                {
                    EqtTrace.Warning("BlameCollector.TestHostLaunchedHandler: Could not start process dump. {0}", e);
                }

                this.logger.LogWarning(args.Context, string.Format(CultureInfo.CurrentUICulture, Resources.Resources.ProcDumpCouldNotStart, e.ToString()));
            }
        }

        /// <summary>
        /// Resets the inactivity timer
        /// </summary>
        private void ResetInactivityTimer()
        {
            if (!this.collectProcessDumpOnTestHostHang || this.inactivityTimerAlreadyFired)
            {
                return;
            }

            EqtTrace.Verbose("Reset the inactivity timer since an event was received.");
            try
            {
                this.inactivityTimer.Change(this.inactivityTimespan, TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception e)
            {
                EqtTrace.Warning($"Failed to reset the inactivity timer with error {e}");
            }
        }

        /// <summary>
        /// Method to deregister handlers and cleanup
        /// </summary>
        private void DeregisterEvents()
        {
            this.events.SessionEnd -= this.SessionEndedHandler;
            this.events.TestCaseStart -= this.EventsTestCaseStart;
            this.events.TestCaseEnd -= this.EventsTestCaseEnd;
        }

        private string GetResultsDirectory()
        {
            try
            {
                XmlElement resultsDirectoryElement = this.configurationElement["ResultsDirectory"];
                string resultsDirectory = resultsDirectoryElement != null ? resultsDirectoryElement.InnerText : string.Empty;
                return resultsDirectory;
            }
            catch (NullReferenceException exception)
            {
                if (EqtTrace.IsErrorEnabled)
                {
                    EqtTrace.Error("Blame Collector : " + exception);
                }

                return string.Empty;
            }
        }
    }
}
