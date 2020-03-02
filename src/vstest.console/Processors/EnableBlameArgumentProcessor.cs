﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommandLine.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using Microsoft.VisualStudio.TestPlatform.CommandLine.Processors.Utilities;
    using Microsoft.VisualStudio.TestPlatform.Common;
    using Microsoft.VisualStudio.TestPlatform.Common.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
    using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
    using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
    using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers.Interfaces;
    using CommandLineResources = Microsoft.VisualStudio.TestPlatform.CommandLine.Resources.Resources;

    internal class EnableBlameArgumentProcessor : IArgumentProcessor
    {
        /// <summary>
        /// The name of the command line argument that the ListTestsArgumentExecutor handles.
        /// </summary>
        public const string CommandName = "/Blame";

        private Lazy<IArgumentProcessorCapabilities> metadata;

        private Lazy<IArgumentExecutor> executor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableBlameArgumentProcessor"/> class.
        /// </summary>
        public EnableBlameArgumentProcessor()
        {
        }

        public Lazy<IArgumentProcessorCapabilities> Metadata
        {
            get
            {
                if (this.metadata == null)
                {
                    this.metadata = new Lazy<IArgumentProcessorCapabilities>(() => new EnableBlameArgumentProcessorCapabilities());
                }

                return this.metadata;
            }
        }

        /// <summary>
        /// Gets or sets the executor.
        /// </summary>
        public Lazy<IArgumentExecutor> Executor
        {
            get
            {
                if (this.executor == null)
                {
                    this.executor = new Lazy<IArgumentExecutor>(() => new EnableBlameArgumentExecutor(RunSettingsManager.Instance, new PlatformEnvironment(), new FileHelper()));
                }

                return this.executor;
            }
            set
            {
                this.executor = value;
            }
        }
    }

    /// <summary>
    /// The argument capabilities.
    /// </summary>
    internal class EnableBlameArgumentProcessorCapabilities : BaseArgumentProcessorCapabilities
    {
        public override string CommandName => EnableBlameArgumentProcessor.CommandName;

        public override bool AllowMultiple => false;

        public override bool IsAction => false;

        public override ArgumentProcessorPriority Priority => ArgumentProcessorPriority.Logging;

        public override string HelpContentResourceName => CommandLineResources.EnableBlameUsage;

        public override HelpContentPriority HelpPriority => HelpContentPriority.EnableDiagArgumentProcessorHelpPriority;
    }

    /// <summary>
    /// The argument executor.
    /// </summary>
    internal class EnableBlameArgumentExecutor : IArgumentExecutor
    {
        /// <summary>
        /// Blame logger and data collector friendly name
        /// </summary>
        private static string BlameFriendlyName = "blame";

        /// <summary>
        /// Run settings manager
        /// </summary>
        private IRunSettingsProvider runSettingsManager;

        /// <summary>
        /// Platform environment
        /// </summary>
        private IEnvironment environment;

        /// <summary>
        /// For file related operation
        /// </summary>
        private readonly IFileHelper fileHelper;

        #region Constructor

        internal EnableBlameArgumentExecutor(IRunSettingsProvider runSettingsManager, IEnvironment environment, IFileHelper fileHelper)
        {
            this.runSettingsManager = runSettingsManager;
            this.environment = environment;
            this.Output = ConsoleOutput.Instance;
            this.fileHelper = fileHelper;
        }

        #endregion

        #region Properties

        internal IOutput Output { get; set; }

        #endregion

        #region IArgumentExecutor

        /// <summary>
        /// Initializes with the argument that was provided with the command.
        /// </summary>
        /// <param name="argument">Argument that was provided with the command.</param>
        public void Initialize(string argument)
        {
            var enableDump = false;
            var exceptionMessage = string.Format(CultureInfo.CurrentUICulture, CommandLineResources.InvalidBlameArgument, argument);
            Dictionary<string, string> collectDumpParameters = null;

            if (!string.IsNullOrWhiteSpace(argument))
            {
                // Get blame argument list.
                var blameArgumentList = ArgumentProcessorUtilities.GetArgumentList(argument, ArgumentProcessorUtilities.SemiColonArgumentSeparator, exceptionMessage);

                // Get collect dump key.
                var collectDumpKey = blameArgumentList[0];
                bool isCollectDumpKeyValid = ValidateCollectDumpKey(collectDumpKey);

                // Check if dump should be enabled or not.
                enableDump = isCollectDumpKeyValid && IsDumpCollectionSupported();

                // Get collect dump parameters.
                var collectDumpParameterArgs = blameArgumentList.Skip(1);
                collectDumpParameters = ArgumentProcessorUtilities.GetArgumentParameters(collectDumpParameterArgs, ArgumentProcessorUtilities.EqualNameValueSeparator, exceptionMessage);
            }

            // Initialize blame.
            InitializeBlame(enableDump, collectDumpParameters);
        }

        /// <summary>
        /// Executes the argument processor.
        /// </summary>
        /// <returns>The <see cref="ArgumentProcessorResult"/>.</returns>
        public ArgumentProcessorResult Execute()
        {
            // Nothing to do since we updated the logger and data collector list in initialize
            return ArgumentProcessorResult.Success;
        }

        /// <summary>
        /// Initialize blame.
        /// </summary>
        /// <param name="enableDump">Enable dump.</param>
        /// <param name="blameParameters">Blame parameters.</param>
        private void InitializeBlame(bool enableDump, Dictionary<string, string> collectDumpParameters)
        {
            // Add Blame Logger
            LoggerUtilities.AddLoggerToRunSettings(BlameFriendlyName, null, this.runSettingsManager);

            // Add Blame Data Collector
            CollectArgumentExecutor.AddDataCollectorToRunSettings(BlameFriendlyName, this.runSettingsManager, this.fileHelper);


            // Add default run settings if required.
            if (this.runSettingsManager.ActiveRunSettings?.SettingsXml == null)
            {
                this.runSettingsManager.AddDefaultRunSettings();
            }
            var settings = this.runSettingsManager.ActiveRunSettings?.SettingsXml;

            // Get results directory from RunSettingsManager
            var resultsDirectory = GetResultsDirectory(settings);

            // Get data collection run settings. Create if not present.
            var dataCollectionRunSettings = XmlRunSettingsUtilities.GetDataCollectionRunSettings(settings);
            if (dataCollectionRunSettings == null)
            {
                dataCollectionRunSettings = new DataCollectionRunSettings();
            }

            // Create blame configuration element.
            var XmlDocument = new XmlDocument();
            var outernode = XmlDocument.CreateElement("Configuration");
            var node = XmlDocument.CreateElement("ResultsDirectory");
            outernode.AppendChild(node);
            node.InnerText = resultsDirectory;

            // Add collect dump node in configuration element.
            if (enableDump)
            {
                AddCollectDumpNode(collectDumpParameters, XmlDocument, outernode);
            }

            // Add blame configuration element to blame collector.
            foreach (var item in dataCollectionRunSettings.DataCollectorSettingsList)
            {
                if (item.FriendlyName.Equals(BlameFriendlyName))
                {
                    item.Configuration = outernode;
                }
            }

            // Update run settings.
            runSettingsManager.UpdateRunSettingsNodeInnerXml(Constants.DataCollectionRunSettingsName, dataCollectionRunSettings.ToXml().InnerXml);
        }

        /// <summary>
        /// Get results directory.
        /// </summary>
        /// <param name="settings">Settings xml.</param>
        /// <returns>Results directory.</returns>
        private string GetResultsDirectory(string settings)
        {
            string resultsDirectory = null;
            if (settings != null)
            {
                try
                {
                    RunConfiguration runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(settings);
                    resultsDirectory = RunSettingsUtilities.GetTestResultsDirectory(runConfiguration);
                }
                catch (SettingsException se)
                {
                    if (EqtTrace.IsErrorEnabled)
                    {
                        EqtTrace.Error("EnableBlameArgumentProcessor: Unable to get the test results directory: Error {0}", se);
                    }
                }
            }

            return resultsDirectory;
        }

        /// <summary>
        /// Checks if dump collection is supported.
        /// </summary>
        /// <returns>Dump collection supported flag.</returns>
        private bool IsDumpCollectionSupported()
        {
            var dumpCollectionSupported = this.environment.OperatingSystem == PlatformOperatingSystem.Windows &&
                    this.environment.Architecture != PlatformArchitecture.ARM64 &&
                    this.environment.Architecture != PlatformArchitecture.ARM;

            if (!dumpCollectionSupported)
            {
                Output.Warning(false, CommandLineResources.BlameCollectDumpNotSupportedForPlatform);
            }

            return dumpCollectionSupported;
        }

        /// <summary>
        /// Check if collect dump key is valid.
        /// </summary>
        /// <param name="collectDumpKey">Collect dump key.</param>
        /// <returns>Flag for collect dump key valid or not.</returns>
        private bool ValidateCollectDumpKey(string collectDumpKey)
        {
            var isCollectDumpKeyValid = collectDumpKey != null && collectDumpKey.Equals(Constants.BlameCollectDumpKey, StringComparison.OrdinalIgnoreCase);

            if (!isCollectDumpKeyValid)
            {
                Output.Warning(false, string.Format(CultureInfo.CurrentUICulture, CommandLineResources.BlameIncorrectOption, collectDumpKey));
            }

            return isCollectDumpKeyValid;
        }

        /// <summary>
        /// Adds collect dump node in outer node.
        /// </summary>
        /// <param name="parameters">Parameters.</param>
        /// <param name="XmlDocument">Xml document.</param>
        /// <param name="outernode">Outer node.</param>
        private void AddCollectDumpNode(Dictionary<string, string> parameters, XmlDocument XmlDocument, XmlElement outernode)
        {
            var dumpNode = XmlDocument.CreateElement(Constants.BlameCollectDumpKey);
            if (parameters != null && parameters.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in parameters)
                {
                    var attribute = XmlDocument.CreateAttribute(entry.Key);
                    attribute.Value = entry.Value;
                    dumpNode.Attributes.Append(attribute);
                }
            }
            outernode.AppendChild(dumpNode);
        }

        #endregion
    }
}
