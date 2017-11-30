﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.AcceptanceTests.TranslationLayerTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    /// <inheritdoc />
    public class DiscoveryEventHandler : ITestDiscoveryEventsHandler
    {
        /// <summary>
        /// Gets the discovered test cases.
        /// </summary>
        public List<TestCase> DiscoveredTestCases { get;}

        public DiscoveryEventHandler()
        {
            this.DiscoveredTestCases = new List<TestCase>();
        }

        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            if (discoveredTestCases != null)
            {
                this.DiscoveredTestCases.AddRange(discoveredTestCases);
            }
        }

        public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
        {
            if (lastChunk != null)
            {
                this.DiscoveredTestCases.AddRange(lastChunk);
            }
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            // No Op
        }

        public void HandleRawMessage(string rawMessage)
        {
            // No op
        }
    }

    /// <inheritdoc />
    public class DiscoveryEventHandler2 : ITestDiscoveryEventsHandler2
    {
        /// <summary>
        /// Gets the discovered test cases.
        /// </summary>
        public List<TestCase> DiscoveredTestCases { get; }

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        public IDictionary<string, object> Metrics { get; private set; }

        public DiscoveryEventHandler2()
        {
            this.DiscoveredTestCases = new List<TestCase>();
        }

        public void HandleRawMessage(string rawMessage)
        {
            // No Op
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            // No Op
        }

        public void HandleDiscoveryComplete(DiscoveryCompleteEventArgs discoveryCompleteEventArgs, IEnumerable<TestCase> lastChunk)
        {
            if (lastChunk != null)
            {
                this.DiscoveredTestCases.AddRange(lastChunk);
            }

            this.Metrics = discoveryCompleteEventArgs.Metrics;
        }

        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            if (discoveredTestCases != null)
            {
                this.DiscoveredTestCases.AddRange(discoveredTestCases);
            }
        }
    }

    /// Discovery Event Handler for batch size
    public class DiscoveryEventHandlerForBatchSize : ITestDiscoveryEventsHandler2, ITestDiscoveryEventsHandler
    {
        /// <summary>
        /// Gets the batch size.
        /// </summary>
        public long batchSize { get; private set; }

        /// <summary>
        /// Gets the discovered test cases.
        /// </summary>
        public List<TestCase> DiscoveredTestCases { get; }

        public DiscoveryEventHandlerForBatchSize()
        {
            this.DiscoveredTestCases = new List<TestCase>();
        }

        public void HandleRawMessage(string rawMessage)
        {
            // No Op
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            // No Op
        }

        public void HandleDiscoveryComplete(DiscoveryCompleteEventArgs discoveryCompleteEventArgs, IEnumerable<TestCase> lastChunk)
        {
            if (lastChunk != null)
            {
                this.DiscoveredTestCases.AddRange(lastChunk);
            }
        }

        public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
        {
            if (lastChunk != null)
            {
                this.DiscoveredTestCases.AddRange(lastChunk);
            }
        }

        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            if (discoveredTestCases != null & discoveredTestCases.Count() != 0)
            {
                this.DiscoveredTestCases.AddRange(discoveredTestCases);
                this.batchSize = discoveredTestCases.Count();
            }
        }
    }
}
