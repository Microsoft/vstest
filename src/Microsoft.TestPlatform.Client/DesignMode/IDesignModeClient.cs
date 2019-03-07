// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Client.DesignMode
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestPlatform.Client.RequestHelper;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    /// <summary>
    /// The interface for design mode client.
    /// </summary>
    public interface IDesignModeClient : IDisposable
    {
        /// <summary>
        /// Setups client based on port
        /// </summary>
        /// <param name="port">port number to connect</param>
        void ConnectToClientAndProcessRequests(int port, ITestRequestManager testRequestManager);

        /// <summary>
        /// Send a custom host launch message to IDE
        /// </summary>
        /// <param name="defaultTestHostStartInfo">Default TestHost Start Info</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>Process id of the launched test host.</returns>
        int LaunchCustomHost(TestProcessStartInfo defaultTestHostStartInfo, CancellationToken cancellationToken);

        /// <summary>
        /// Handles parent process exit
        /// </summary>
        void HandleParentProcessExit();

        /// <summary>
        /// Send the raw messages to IDE
        /// </summary>
        /// <param name="rawMessage"></param>
        void SendRawMessage(string rawMessage);

        /// <summary>
        /// Send the test session messages to IDE
        /// </summary>
        /// /// <param name="level"></param>
        /// <param name="message"></param>
        void SendTestMessage(TestMessageLevel level, string message);
    }
}
