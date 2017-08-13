// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.PlatformAbstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

    /// <summary>
    /// Helper class to deal with process related functionality.
    /// </summary>
    public class ProcessHelper : IProcessHelper
    {
        /// <inheritdoc/>
        public object LaunchProcess(
            string processPath,
            string arguments,
            string workingDirectory,
            IDictionary<string, string> environmentVariables,
            Action<object, string> errorCallback,
            Action<object> exitCallBack)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetCurrentProcessFileName()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string GetTestEngineDirectory()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int GetCurrentProcessId()
        {
            return -1;
        }

        /// <inheritdoc/>
        public string GetProcessName(int processId)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        public bool TryGetExitCode(object process, out int exitCode)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetExitCallback(int parentProcessId, Action callbackAction)
        {
        }

        /// <inheritdoc/>
        public void TerminateProcess(object process)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int GetProcessId(object process)
        {
            return -1;
        }
    }
}
