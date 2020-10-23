﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.VsTestConsole.TranslationLayer.Interfaces
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces;

    /// <summary>
    /// 
    /// </summary>
    public interface ITestSession
    {
        /// <summary>
        /// Starts a test run given a list of sources.
        /// </summary>
        /// <param name="sources">Sources to Run tests on</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        void RunTests(
            IEnumerable<string> sources,
            string runSettings,
            ITestRunEventsHandler testRunEventsHandler);

        /// <summary>
        /// Starts a test run given a list of sources.
        /// </summary>
        /// <param name="sources">Sources to Run tests on</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="options">Options to be passed into the platform.</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        void RunTests(
            IEnumerable<string> sources,
            string runSettings,
            TestPlatformOptions options,
            ITestRunEventsHandler testRunEventsHandler);

        /// <summary>
        /// Starts a test run given a list of test cases
        /// </summary>
        /// <param name="testCases">TestCases to run</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        void RunTests(
            IEnumerable<TestCase> testCases,
            string runSettings,
            ITestRunEventsHandler testRunEventsHandler);

        /// <summary>
        /// Starts a test run given a list of test cases
        /// </summary>
        /// <param name="testCases">TestCases to run</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="options">Options to be passed into the platform.</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        void RunTests(
            IEnumerable<TestCase> testCases,
            string runSettings,
            TestPlatformOptions options,
            ITestRunEventsHandler testRunEventsHandler);

        /// <summary>
        /// Starts a test run given a list of sources by giving caller an option to start their own test host.
        /// </summary>
        /// <param name="sources">Sources to Run tests on</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        /// <param name="customTestHostLauncher">Custom test host launcher for the run.</param>
        void RunTestsWithCustomTestHost(
            IEnumerable<string> sources,
            string runSettings,
            ITestRunEventsHandler testRunEventsHandler,
            ITestHostLauncher customTestHostLauncher);

        /// <summary>
        /// Starts a test run given a list of sources by giving caller an option to start their own test host.
        /// </summary>
        /// <param name="sources">Sources to Run tests on</param>
        /// <param name="runSettings">RunSettings XML to run the tests</param>
        /// <param name="options">Options to be passed into the platform.</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events</param>
        /// <param name="customTestHostLauncher">Custom test host launcher for the run.</param>
        void RunTestsWithCustomTestHost(
            IEnumerable<string> sources,
            string runSettings,
            TestPlatformOptions options,
            ITestRunEventsHandler testRunEventsHandler,
            ITestHostLauncher customTestHostLauncher);

        /// <summary>
        /// Starts a test run given a list of test cases by giving caller an option to start their own test host
        /// </summary>
        /// <param name="testCases">TestCases to run.</param>
        /// <param name="runSettings">RunSettings XML to run the tests.</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events.</param>
        /// <param name="customTestHostLauncher">Custom test host launcher for the run.</param>
        void RunTestsWithCustomTestHost(
            IEnumerable<TestCase> testCases,
            string runSettings,
            ITestRunEventsHandler testRunEventsHandler,
            ITestHostLauncher customTestHostLauncher);

        /// <summary>
        /// Starts a test run given a list of test cases by giving caller an option to start their own test host
        /// </summary>
        /// <param name="testCases">TestCases to run.</param>
        /// <param name="runSettings">RunSettings XML to run the tests.</param>
        /// <param name="options">Options to be passed into the platform.</param>
        /// <param name="testRunEventsHandler">EventHandler to receive test run events.</param>
        /// <param name="customTestHostLauncher">Custom test host launcher for the run.</param>
        void RunTestsWithCustomTestHost(
            IEnumerable<TestCase> testCases,
            string runSettings,
            TestPlatformOptions options,
            ITestRunEventsHandler testRunEventsHandler,
            ITestHostLauncher customTestHostLauncher);
    }
}
