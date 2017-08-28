// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;

    using Newtonsoft.Json;

    /// <summary>
    /// The test run criteria with tests.
    /// </summary>
    public class TestRunCriteriaWithTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunCriteriaWithTests"/> class.
        /// Ensure that names of constructor parameters match the public property names of the same for JSON serialization
        /// </summary>
        /// <param name="tests"> The tests. </param>
        /// <param name="packages"> The packages which actually contain sources(UWP).</param>
        /// <param name="runSettings"> The test run settings. </param>
        /// <param name="testExecutionContext"> The test Execution Context. </param>
        [JsonConstructor]
        public TestRunCriteriaWithTests(IEnumerable<TestCase> tests, IEnumerable<string> packages, string runSettings, TestExecutionContext testExecutionContext)
        {
            this.Tests = tests;
            this.Packages = packages;
            this.RunSettings = runSettings;
            this.TestExecutionContext = testExecutionContext;
        }

        /// <summary>
        /// Gets the tests.
        /// </summary>
        public IEnumerable<TestCase> Tests { get; private set; }

        /// <summary>
        /// Gets the test run settings.
        /// </summary>
        public string RunSettings { get; private set; }

        /// <summary>
        /// Gets or sets the test execution context.
        /// </summary>
        public TestExecutionContext TestExecutionContext { get; set; }

        /// <summary>
        /// Gets the test Containers (e.g. DLL/EXE/artifacts to scan)
        /// </summary>
        public IEnumerable<string> Packages { get; private set; }
    }
}