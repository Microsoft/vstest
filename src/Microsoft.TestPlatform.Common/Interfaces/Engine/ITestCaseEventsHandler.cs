// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;

    /// <summary>
    /// The Test Case level events.
    /// </summary>
    public interface ITestCaseEventsHandler
    {
        /// <summary>
        ///  Report start of executing a test case.
        /// </summary>
        /// <param name="testCase">Details of the test case whose execution is just started.</param>
        void SendTestCaseStart(TestCase testCase);

        /// <summary>
        /// Report end of executing a test case.
        /// </summary>
        /// <param name="testCase">Details of the test case.</param>
        /// <param name="outcome">Result of the test case executed.</param>
        void SendTestCaseEnd(TestCase testCase, TestOutcome outcome);
        
        /// <summary>
        /// Sends the test result
        /// </summary>
        /// <param name="result"> The result. </param>
        /// <returns>True, if result can be flushed</returns>
        bool SendTestResult(TestResult result);
    }
}
