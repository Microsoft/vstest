// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.TestHost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Tracing;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        private const string TestSourceArgumentString = "--testsourcepath";

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            try
            {
                TestPlatformEventSource.Instance.TestHostStart();
                WaitForDebuggerIfEnabled();
                Run(args);
            }
            catch (Exception ex)
            {
                EqtTrace.Error("TestHost: Error occured during initialization of TestHost : {0}", ex);
            }
            finally
            {
                TestPlatformEventSource.Instance.TestHostStop();
            }
        }

        private static void Run(string[] args)
        {
            var argsDictionary = GetArguments(args);
            // Invoke the engine with arguments
            GetEngineInvoker(argsDictionary).Invoke(argsDictionary);
        }

        private static IEngineInvoker GetEngineInvoker(IDictionary<string, string> argsDictionary)
        {
            IEngineInvoker invoker = null;
#if NET46
            // If Args contains test source argument, invoker Engine in new appdomain 
            string testSourcePath;
            if (argsDictionary.TryGetValue(TestSourceArgumentString, out testSourcePath) && !string.IsNullOrWhiteSpace(testSourcePath))
            {
                // remove the test source arg from dictionary
                argsDictionary.Remove(TestSourceArgumentString);

                // Only DLLs and EXEs can have app.configs or ".exe.config" or ".dll.config"
                if (System.IO.File.Exists(testSourcePath) &&
                        (testSourcePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                        || testSourcePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
                {
                    invoker = new AppDomainEngineInvoker<DefaultEngineInvoker>(testSourcePath);
                }
            }
#endif
            return invoker ?? new DefaultEngineInvoker();
        }

        /// <summary>
        /// Parse command line arguments to a dictionary.
        /// </summary>
        /// <param name="args">Command line arguments. Ex: <c>{ "--port", "12312", "--parentprocessid", "2312", "--testsourcepath", "C:\temp\1.dll" }</c></param>
        /// <returns>Dictionary of arguments keys and values.</returns>
        private static IDictionary<string, string> GetArguments(string[] args)
        {
            IDictionary<string, string> argsDictionary = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    if (i < args.Length - 1 && !args[i + 1].StartsWith("-"))
                    {
                        argsDictionary.Add(args[i], args[i + 1]);
                        i++;
                    }
                    else
                    {
                        argsDictionary.Add(args[i], null);
                    }
                }
            }

            return argsDictionary;
        }

        private static void WaitForDebuggerIfEnabled()
        {
            var debugEnabled = Environment.GetEnvironmentVariable("VSTEST_HOST_DEBUG");
            if (!string.IsNullOrEmpty(debugEnabled) && debugEnabled.Equals("1", StringComparison.Ordinal))
            {
                ConsoleOutput.Instance.WriteLine("Waiting for debugger attach...", OutputLevel.Information);

                var currentProcess = Process.GetCurrentProcess();
                ConsoleOutput.Instance.WriteLine(
                    string.Format("Process Id: {0}, Name: {1}", currentProcess.Id, currentProcess.ProcessName),
                    OutputLevel.Information);

                while (!Debugger.IsAttached)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                Debugger.Break();
            }
        }
    }
}
