// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommunicationUtilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
    using Microsoft.VisualStudio.TestPlatform.Utilities;

    /// <summary>
    /// Communication client implementation over sockets.
    /// </summary>
    public class SocketClient : ICommunicationEndPoint
    {
        /// <summary>
        /// Time for which the client wait for executor/runner process to start, and host server
        /// </summary>
        private const int CONNECTIONRETRYTIMEOUT = 50 * 1000;

        private readonly CancellationTokenSource cancellation;
        private readonly TcpClient tcpClient;
        private readonly Func<Stream, ICommunicationChannel> channelFactory;
        private ICommunicationChannel channel;
        private bool stopped;

        public SocketClient()
            : this(stream => new LengthPrefixCommunicationChannel(stream))
        {
        }

        protected SocketClient(Func<Stream, ICommunicationChannel> channelFactory)
        {
            // Used to cancel the message loop
            this.cancellation = new CancellationTokenSource();
            this.stopped = false;

            this.tcpClient = new TcpClient { NoDelay = true };
            this.channelFactory = channelFactory;
        }

        /// <inheritdoc />
        public event EventHandler<ConnectedEventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <inheritdoc />
        public string Start(string endPoint)
        {
            var ipEndPoint = endPoint.GetIPEndPoint();

            if (EqtTrace.IsVerboseEnabled)
            {
                EqtTrace.Verbose("Waiting for connecting to server");
            }

            Task connectionTask = null;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // It is possible that first ConnectAsync call could not connect to server, so do a repoll to connect to server for 50secs
            do
            {
                try
                {
                    connectionTask = this.tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
                    connectionTask.Wait();
                }
                catch (Exception ex)
                {
                    EqtTrace.Verbose("Connection Failed with error {0}, retrying", ex.Message);
                }
            }
            while ((this.tcpClient != null) && !this.tcpClient.Connected && watch.ElapsedMilliseconds < CONNECTIONRETRYTIMEOUT);

            this.OnServerConnected(connectionTask);

            return ipEndPoint.ToString();
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (!this.stopped)
            {
                EqtTrace.Info("SocketClient: Stop: Cancellation requested. Stopping message loop.");
                this.cancellation.Cancel();
            }
        }

        private void OnServerConnected(Task connectAsyncTask)
        {
            if (this.Connected != null)
            {
                if (connectAsyncTask.IsFaulted || !this.tcpClient.Connected)
                {
                    this.Disconnected.SafeInvoke(this, new DisconnectedEventArgs { Error = connectAsyncTask.Exception }, "SocketClient: ServerDisconnected");
                    if (EqtTrace.IsVerboseEnabled)
                    {
                        EqtTrace.Verbose("Unable to connect to server, Exception occured : {0}", connectAsyncTask.Exception);
                    }
                }
                else
                {
                    this.channel = this.channelFactory(this.tcpClient.GetStream());
                    this.Connected.SafeInvoke(this, new ConnectedEventArgs(this.channel), "SocketClient: ServerConnected");

                    if (EqtTrace.IsVerboseEnabled)
                    {
                        EqtTrace.Verbose("Connected to server, and starting MessageLoopAsync");
                    }

                    // Start the message loop
                    Task.Run(() => this.tcpClient.MessageLoopAsync(
                            this.channel,
                            this.Stop,
                            this.cancellation.Token))
                        .ConfigureAwait(false);
                }
            }
        }

        private void Stop(Exception error)
        {
            if (!this.stopped)
            {
                // Do not allow stop to be called multiple times.
                this.stopped = true;

                // Close the client and dispose the underlying stream
#if NET451
                // tcpClient.Close() calls tcpClient.Dispose().
                this.tcpClient?.Close();
#else
                // tcpClient.Close() not available for netstandard1.5.
                this.tcpClient?.Dispose();
#endif
                this.channel.Dispose();
                this.cancellation.Dispose();

                this.Disconnected?.SafeInvoke(this, new DisconnectedEventArgs(), "SocketClient: ServerDisconnected");
            }
        }
    }
}
