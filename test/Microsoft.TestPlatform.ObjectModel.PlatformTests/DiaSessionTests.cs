﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.ObjectModel.PlatformTests
{
    using System;
    using System.Diagnostics;

    using Microsoft.TestPlatform.TestUtilities;
    using Microsoft.TestPlatform.TestUtilities.PerfInstrumentation;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DiaSessionTests : IntegrationTestBase
    {
        private const string NET46 = "net46";
        private const string NETCOREAPP10 = "netcoreapp1.0";

        public static string GetAndSetTargetFrameWork(IntegrationTestEnvironment testEnvironment)
        {
            var currentTargetFrameWork = testEnvironment.TargetFramework;
#if NET46
            testEnvironment.TargetFramework = NET46;
#else
            testEnvironment.TargetFramework = NETCOREAPP10;
#endif
            return currentTargetFrameWork;
        }

        [TestMethod]
        public void GetNavigationDataShouldReturnCorrectFileNameAndLineNumber()
        {
            var currentTargetFrameWork = GetAndSetTargetFrameWork(this.testEnvironment);
            var assemblyPath = this.GetSampleTestAssembly();

            DiaSession diaSession = new DiaSession(assemblyPath);
            DiaNavigationData diaNavigationData = diaSession.GetNavigationData("SampleUnitTestProject.UnitTest1", "PassingTest");

            Assert.IsNotNull(diaNavigationData, "Failed to get navigation data");
            StringAssert.EndsWith(diaNavigationData.FileName, @"\SimpleTestProject\UnitTest1.cs");

            Assert.AreEqual(23, diaNavigationData.MinLineNumber, "Incorrect min line number");
            Assert.AreEqual(25, diaNavigationData.MaxLineNumber, "Incorrect max line number");

            this.testEnvironment.TargetFramework = currentTargetFrameWork;
        }

        [TestMethod]
        public void GetNavigationDataShouldReturnCorrectDataForAsyncMethod()
        {
            var currentTargetFrameWork = GetAndSetTargetFrameWork(this.testEnvironment);
            var assemblyPath = this.GetAssetFullPath("SimpleTestProject3.dll");

            DiaSession diaSession = new DiaSession(assemblyPath);
            DiaNavigationData diaNavigationData = diaSession.GetNavigationData("SampleUnitTestProject3.UnitTest1+<AsyncTestMethod>d__1", "MoveNext");

            Assert.IsNotNull(diaNavigationData, "Failed to get navigation data");
            StringAssert.EndsWith(diaNavigationData.FileName, @"\SimpleTestProject3\UnitTest1.cs");

            Assert.AreEqual(20, diaNavigationData.MinLineNumber, "Incorrect min line number");
            Assert.AreEqual(22, diaNavigationData.MaxLineNumber, "Incorrect max line number");

            this.testEnvironment.TargetFramework = currentTargetFrameWork;
        }

        [TestMethod]
        public void GetNavigationDataShouldReturnCorrectDataForOverLoadedMethod()
        {
            var currentTargetFrameWork = GetAndSetTargetFrameWork(this.testEnvironment);
            var assemblyPath = this.GetAssetFullPath("SimpleTestProject3.dll");

            DiaSession diaSession = new DiaSession(assemblyPath);
            DiaNavigationData diaNavigationData = diaSession.GetNavigationData("SampleUnitTestProject3.Class1", "OverLoadededMethod");

            Assert.IsNotNull(diaNavigationData, "Failed to get navigation data");
            StringAssert.EndsWith(diaNavigationData.FileName, @"\SimpleTestProject3\UnitTest1.cs");

            Assert.AreEqual(32, diaNavigationData.MinLineNumber, "Incorrect min line number");
            Assert.AreEqual(33, diaNavigationData.MaxLineNumber, "Incorrect max line number");

            this.testEnvironment.TargetFramework = currentTargetFrameWork;
        }

        [TestMethod]
        public void GetNavigationDataShouldReturnNullForNotExistMethodNameOrNotExistTypeName()
        {
            var currentTargetFrameWork = GetAndSetTargetFrameWork(this.testEnvironment);
            var assemblyPath = this.GetSampleTestAssembly();

            DiaSession diaSession = new DiaSession(assemblyPath);

            // Not exist method name
            DiaNavigationData diaNavigationData = diaSession.GetNavigationData("SampleUnitTestProject.UnitTest1", "NotExistMethod");
            Assert.IsNull(diaNavigationData);

            // Not Exist Type name
            diaNavigationData = diaSession.GetNavigationData("SampleUnitTestProject.NotExistType", "PassingTest");
            Assert.IsNull(diaNavigationData);

            this.testEnvironment.TargetFramework = currentTargetFrameWork;
        }

        [TestMethod]
        public void DiaSessionPerfTest()
        {
            var currentTargetFrameWork = GetAndSetTargetFrameWork(this.testEnvironment);
            var assemblyPath = this.GetAssetFullPath("PerfTestProject.dll");

            var watch = Stopwatch.StartNew();
            DiaSession diaSession = new DiaSession(assemblyPath);
            DiaNavigationData diaNavigationData = diaSession.GetNavigationData("PerfTestProject.UnitTest1", "MSTest_D1_01");
            watch.Stop();

            Assert.IsNotNull(diaNavigationData, "Failed to get navigation data");
            StringAssert.EndsWith(diaNavigationData.FileName, @"\PerfTestProject\UnitTest1.cs");
            Assert.AreEqual(diaNavigationData.MinLineNumber, 17);
            Assert.AreEqual(diaNavigationData.MaxLineNumber, 20);
            var expectedTime = 150;
            Assert.IsTrue(watch.Elapsed.Milliseconds < expectedTime, string.Format("DiaSession Perf test Actual time:{0} ms Expected time:{1} ms", watch.Elapsed.Milliseconds, expectedTime));

            this.testEnvironment.TargetFramework = currentTargetFrameWork;
        }
    }
}