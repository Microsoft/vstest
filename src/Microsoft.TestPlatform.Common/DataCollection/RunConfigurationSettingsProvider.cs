﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.Common.DataCollection
{
    using System.Xml;

    using Microsoft.VisualStudio.TestPlatform.Common.DataCollection.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    /// <summary>
    /// Provides settings for data collectors.    
    /// </summary>
    [SettingsName(RunConfigurationSettingsName)]
    public class RunConfigurationSettingsProvider : IRunConfigurationSettingsProvider
    {
        /// <summary>
        /// Name of the data collection settings node in RunSettings.
        /// </summary>
        public const string RunConfigurationSettingsName = "RunConfiguration";

        /// <summary>
        /// Gets run configuration settings.
        /// </summary>
        public RunConfiguration Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads the Run specific data collection settings from RunSettings.
        /// </summary>
        /// <param name="reader">Xml reader</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Load(XmlReader reader)
        {
            ValidateArg.NotNull<XmlReader>(reader, "reader");
            reader.Read();
            this.Settings = RunConfiguration.FromXml(reader);
        }
    }
}