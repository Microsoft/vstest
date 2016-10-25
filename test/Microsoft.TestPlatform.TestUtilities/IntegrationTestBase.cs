// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TestPlatform.TestUtilities
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for integration tests.
    /// </summary>
    public class IntegrationTestBase
    {
        private const string TestSummaryStatusMessageFormat = "Total tests: {0}. Passed: {1}. Failed: {2}. Skipped: {3}";
        private string standardTestOutput = string.Empty;
        private string standardTestError = string.Empty;

        protected readonly IntegrationTestEnvironment testEnvironment;

        private const string TestAdapterRelativePath = @"MSTest.TestAdapter\1.1.3-preview\build\_common";

        public IntegrationTestBase()
        {
            this.testEnvironment = new IntegrationTestEnvironment();
        }

        /// <summary>
        /// Prepare arguments for <c>vstest.console.exe</c>.
        /// </summary>
        /// <param name="testAssembly">Name of the test assembly.</param>
        /// <param name="testAdapterPath">Path to test adapter.</param>
        /// <param name="runSettings">Text of run settings.</param>
        /// <returns>Command line arguments string.</returns>
        public static string PrepareArguments(string testAssembly, string testAdapterPath, string runSettings)
        {
            string arguments = " /platform:x64 ";
            if (string.IsNullOrWhiteSpace(runSettings))
            {
                arguments = string.Concat("\"", testAssembly, "\"", " /testadapterpath:\"", testAdapterPath, "\"");
            }
            else
            {
                arguments = string.Concat(
                    "\"",
                    testAssembly,
                    "\"",
                    " /testadapterpath:\"",
                    testAdapterPath,
                    "\"",
                    " /settings:\"",
                    runSettings,
                    "\"");
            }

            return arguments;
        }

        /// <summary>
        /// Invokes <c>vstest.console</c> with specified arguments.
        /// </summary>
        /// <param name="arguments">Arguments provided to <c>vstest.console</c>.exe</param>
        public void InvokeVsTest(string arguments)
        {
            Execute(arguments, out this.standardTestOutput, out this.standardTestError);
            this.FormatStandardOutCome();
        }

        /// <summary>
        /// Invokes <c>vstest.console</c> to execute tests in a test assembly.
        /// </summary>
        /// <param name="testAssembly">A test assembly.</param>
        /// <param name="testAdapterPath">Path to test adapters.</param>
        /// <param name="runSettings">Run settings for execution.</param>
        public void InvokeVsTestForExecution(string testAssembly, string testAdapterPath, string runSettings = "")
        {
            var arguments = PrepareArguments(testAssembly, testAdapterPath, runSettings);
            this.InvokeVsTest(arguments);
        }

        /// <summary>
        /// Invokes <c>vstest.console</c> to discover tests in a test assembly. "/listTests" is appended to the arguments.
        /// </summary>
        /// <param name="testAssembly">A test assembly.</param>
        /// <param name="testAdapterPath">Path to test adapters.</param>
        /// <param name="runSettings">Run settings for execution.</param>
        public void InvokeVsTestForDiscovery(string testAssembly, string testAdapterPath, string runSettings = "")
        {
            var arguments = PrepareArguments(testAssembly, testAdapterPath, runSettings);
            arguments = string.Concat(arguments, " /listtests");
            this.InvokeVsTest(arguments);
        }
        
        /// <summary>
        /// Validate if the overall test count and results are matching.
        /// </summary>
        /// <param name="passedTestsCount">Passed test count</param>
        /// <param name="failedTestsCount">Failed test count</param>
        /// <param name="skippedTestsCount">Skipped test count</param>
        public void ValidateSummaryStatus(int passedTestsCount, int failedTestsCount, int skippedTestsCount)
        {
            var summaryStatus = string.Format(
                TestSummaryStatusMessageFormat,
                passedTestsCount + failedTestsCount + skippedTestsCount,
                passedTestsCount,
                failedTestsCount,
                skippedTestsCount);

            Assert.IsTrue(this.standardTestOutput.Contains(summaryStatus), "The Test summary does not match. Expected: {0} Test Output: {1}", this.standardTestOutput, summaryStatus);
        }

        /// <summary>
        /// Validates if the test results have the specified set of passed tests.
        /// </summary>
        /// <param name="passedTests">Set of passed tests.</param>
        /// <remarks>Provide the full test name similar to this format SampleTest.TestCode.TestMethodPass.</remarks>
        public void ValidatePassedTests(params string[] passedTests)
        {
            foreach (var test in passedTests)
            {
                var flag = this.standardTestOutput.Contains("Passed " + test)
                           || this.standardTestOutput.Contains("Passed " + GetTestMethodName(test));
                Assert.IsTrue(flag, "Test {0} does not appear in passed tests list.", test);
            }
        }

        /// <summary>
        /// Validates if the test results have the specified set of failed tests.
        /// </summary>
        /// <param name="failedTests">Set of failed tests.</param>
        /// <remarks>
        /// Provide the full test name similar to this format SampleTest.TestCode.TestMethodFailed.
        /// Also validates whether these tests have stack trace info.
        /// </remarks>
        public void ValidateFailedTests(params string[] failedTests)
        {
            foreach (var test in failedTests)
            {
                var flag = this.standardTestOutput.Contains("Failed " + test)
                           || this.standardTestOutput.Contains("Failed " + GetTestMethodName(test));
                Assert.IsTrue(flag, "Test {0} does not appear in failed tests list.", test);
                
                // Verify stack information as well.
                Assert.IsTrue(this.standardTestError.Contains(GetTestMethodName(test)), "No stack trace for failed test: {0}", test);
            }
        }

        /// <summary>
        /// Validates if the test results have the specified set of skipped tests.
        /// </summary>
        /// <param name="skippedTests">The set of skipped tests.</param>
        /// <remarks>Provide the full test name similar to this format SampleTest.TestCode.TestMethodSkipped.</remarks>
        public void ValidateSkippedTests(params string[] skippedTests)
        {
            foreach (var test in skippedTests)
            {
                var flag = this.standardTestOutput.Contains("Skipped " + test)
                           || this.standardTestOutput.Contains("Skipped " + GetTestMethodName(test));
                Assert.IsTrue(flag, "Test {0} does not appear in skipped tests list.", test);
            }
        }

        /// <summary>
        /// Validate if the discovered tests list contains provided tests.
        /// </summary>
        /// <param name="discoveredTestsList">List of tests expected to be discovered.</param>
        public void ValidateDiscoveredTests(params string[] discoveredTestsList)
        {
            foreach (var test in discoveredTestsList)
            {
                var flag = this.standardTestOutput.Contains(test)
                           || this.standardTestOutput.Contains(GetTestMethodName(test));
                Assert.IsTrue(flag, "Test {0} does not appear in discovered tests list.", test);
            }
        }

        protected string GetSampleTestAssembly()
        {
            return this.GetAssetFullPath("SimpleTestProject.dll");
        }

        protected string GetAssetFullPath(string assetName)
        {
            return this.testEnvironment.GetTestAsset(assetName);
        }

        protected string GetTestAdapterPath()
        {
            return this.testEnvironment.GetNugetPackage(TestAdapterRelativePath);
        }

        /// <summary>
        /// Gets the test method name from full name.
        /// </summary>
        /// <param name="testFullName">Fully qualified name of the test.</param>
        /// <returns>Simple name of the test.</returns>
        private static string GetTestMethodName(string testFullName)
        {
            string testMethodName = string.Empty;

            var splits = testFullName.Split('.');
            if (splits.Count() >= 3)
            {
                testMethodName = testFullName.Split('.')[2];
            }

            return testMethodName;
        }

        private static void Execute(string args, out string stdOut, out string stdError)
        {
            var testEnvironment = new IntegrationTestEnvironment();

            using (Process vstestconsole = new Process())
            {
                Console.WriteLine("IntegrationTestBase.Execute: Starting vstest.console.exe");
                vstestconsole.StartInfo.FileName = testEnvironment.GetConsoleRunnerPath();
                vstestconsole.StartInfo.Arguments = args;
                vstestconsole.StartInfo.UseShellExecute = false;
                //vstestconsole.StartInfo.WorkingDirectory = testEnvironment.PublishDirectory;
                vstestconsole.StartInfo.RedirectStandardError = true;
                vstestconsole.StartInfo.RedirectStandardOutput = true;
                vstestconsole.StartInfo.CreateNoWindow = true;

                Console.WriteLine("IntegrationTestBase.Execute: Path = {0}", vstestconsole.StartInfo.FileName);
                Console.WriteLine("IntegrationTestBase.Execute: Arguments = {0}", vstestconsole.StartInfo.Arguments);

                vstestconsole.Start();
                stdError = vstestconsole.StandardError.ReadToEnd();
                stdOut = vstestconsole.StandardOutput.ReadToEnd();

                vstestconsole.WaitForExit(5 * 60 * 1000);
                Console.WriteLine("IntegrationTestBase.Execute: Stopped vstest.console.exe. Exit code = {0}", vstestconsole.ExitCode);
            }
        }

        private void FormatStandardOutCome()
        {
            this.standardTestError = Regex.Replace(this.standardTestError, @"\s+", " ");
            this.standardTestOutput = Regex.Replace(this.standardTestOutput, @"\s+", " ");
        }
    }
}
