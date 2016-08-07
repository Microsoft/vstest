﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.Common.DataCollection.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Microsoft.VisualStudio.TestPlatform.Common;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    using TestCaseStartEventArgs = Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.Events.TestCaseStartEventArgs;

    /// <summary>
    /// Defines the Data Collection Manager for Data Collectors.
    /// </summary>
    internal interface IDataCollectionManager : IDisposable
    {
        /// <summary>
        /// Loads and initializes data collector plugins.
        /// </summary>
        /// <param name="runSettings">Run Settings which has DataCollector configuration.</param>
        /// <returns>Environment variables.</returns>
        IDictionary<string, string> LoadDataCollectors(RunSettings runSettings);

        /// <summary>
        /// Raises TestCaseStart event to all data collectors configured for run.
        /// </summary>
        /// <param name="testCaseStartEventArgs">TestCaseStart event.</param>
        void TestCaseStarted(TestCaseStartEventArgs testCaseStartEventArgs);

        /// <summary>
        /// Raises TestCaseEnd event to all data collectors configured for run.
        /// </summary>
        /// <param name="testCase">Test case which is complete.</param>
        /// <param name="testOutcome">Outcome of the test case.</param>
        /// <returns>Collection of  testCase attachmentSet.</returns>
        Collection<AttachmentSet> TestCaseEnded(TestCase testCase, TestOutcome testOutcome);

        /// <summary>
        /// Raises SessionStart event to all data collectors configured for run.
        /// </summary>
        /// <returns>Are test case level events required.</returns>
        bool SessionStarted();

        /// <summary>
        /// Raises SessionEnd event to all data collectors configured for run.
        /// </summary>
        /// <param name="isCancelled">Specified whether the run is cancelled or not.</param>
        /// <returns>Collection of session attachmentSet.</returns>
        Collection<AttachmentSet> SessionEnded(bool isCancelled);
    }
}