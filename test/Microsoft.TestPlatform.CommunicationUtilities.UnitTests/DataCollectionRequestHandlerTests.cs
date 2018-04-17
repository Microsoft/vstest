// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.CommunicationUtilities.UnitTests
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Net;

    using Microsoft.TestPlatform.CommunicationUtilities.UnitTests.TestDoubles;
    using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;
    using Microsoft.VisualStudio.TestPlatform.Common.DataCollector.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.DataCollection;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Newtonsoft.Json.Linq;
    using VisualStudio.TestPlatform.CoreUtilities.Helpers;

    using CommunicationUtilitiesResources = Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources.Resources;
    using CoreUtilitiesConstants = Microsoft.VisualStudio.TestPlatform.CoreUtilities.Constants;

    [TestClass]
    public class DataCollectionRequestHandlerTests
    {
        private Mock<ICommunicationManager> mockCommunicationManager;
        private Mock<IMessageSink> mockMessageSink;
        private Mock<IDataCollectionManager> mockDataCollectionManager;
        private Mock<IDataCollectionTestCaseEventHandler> mockDataCollectionTestCaseEventHandler;
        private TestableDataCollectionRequestHandler requestHandler;
        private Mock<IDataSerializer> mockDataSerializer;
        private Message afterTestRunEnd = new Message() { MessageType = MessageType.AfterTestRunEnd, Payload = "false" };
        private Message beforeTestRunStart = new Message() { MessageType = MessageType.BeforeTestRunStart, Payload = "settingsXml" };

        public DataCollectionRequestHandlerTests()
        {
            this.mockCommunicationManager = new Mock<ICommunicationManager>();
            this.mockMessageSink = new Mock<IMessageSink>();
            this.mockDataCollectionManager = new Mock<IDataCollectionManager>();
            this.mockDataSerializer = new Mock<IDataSerializer>();
            this.mockDataCollectionTestCaseEventHandler = new Mock<IDataCollectionTestCaseEventHandler>();
            this.mockDataCollectionTestCaseEventHandler.Setup(x => x.WaitForRequestHandlerConnection(It.IsAny<int>())).Returns(true);
            this.requestHandler = new TestableDataCollectionRequestHandler(this.mockCommunicationManager.Object, this.mockMessageSink.Object, this.mockDataCollectionManager.Object, this.mockDataCollectionTestCaseEventHandler.Object, this.mockDataSerializer.Object);

            this.mockCommunicationManager.SetupSequence(x => x.ReceiveMessage()).Returns(this.beforeTestRunStart).Returns(this.afterTestRunEnd);

            this.mockDataCollectionManager.Setup(x => x.SessionStarted()).Returns(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(EnvironmentHelper.VstestConnectionTimeout, string.Empty);
        }

        [TestMethod]
        public void CreateInstanceShouldThrowExceptionIfInstanceCommunicationManagerIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    DataCollectionRequestHandler.Create(null, this.mockMessageSink.Object);
                });
        }

        [TestMethod]
        public void CreateInstanceShouldThrowExceptinIfInstanceMessageSinkIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    DataCollectionRequestHandler.Create(this.mockCommunicationManager.Object, null);
                });
        }

        [TestMethod]
        public void CreateInstanceShouldCreateInstance()
        {
            var result = DataCollectionRequestHandler.Create(this.mockCommunicationManager.Object, this.mockMessageSink.Object);

            Assert.AreEqual(result, DataCollectionRequestHandler.Instance);
        }

        [TestMethod]
        public void InitializeCommunicationShouldInitializeCommunication()
        {
            this.requestHandler.InitializeCommunication(123);

            this.mockCommunicationManager.Verify(x => x.SetupClientAsync(new IPEndPoint(IPAddress.Loopback, 123)), Times.Once);
        }

        [TestMethod]
        public void InitializeCommunicationShouldThrowExceptionIfThrownByCommunicationManager()
        {
            this.mockCommunicationManager.Setup(x => x.SetupClientAsync(It.IsAny<IPEndPoint>())).Throws<Exception>();

            Assert.ThrowsException<Exception>(() =>
                {
                    this.requestHandler.InitializeCommunication(123);
                });
        }

        [TestMethod]
        public void WaitForRequestSenderConnectionShouldInvokeCommunicationManager()
        {
            this.requestHandler.WaitForRequestSenderConnection(0);

            this.mockCommunicationManager.Verify(x => x.WaitForServerConnection(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void WaitForRequestSenderConnectionShouldThrowExceptionIfThrownByCommunicationManager()
        {
            this.mockCommunicationManager.Setup(x => x.WaitForServerConnection(It.IsAny<int>())).Throws<Exception>();

            Assert.ThrowsException<Exception>(() =>
                {
                    this.requestHandler.WaitForRequestSenderConnection(0);
                });
        }

        [TestMethod]
        public void SendDataCollectionMessageShouldSendMessageToCommunicationManager()
        {
            var message = new DataCollectionMessageEventArgs(TestMessageLevel.Error, "message");

            this.requestHandler.SendDataCollectionMessage(message);

            this.mockCommunicationManager.Verify(x => x.SendMessage(MessageType.DataCollectionMessage, message), Times.Once);
        }

        [TestMethod]
        public void SendDataCollectionMessageShouldThrowExceptionIfThrownByCommunicationManager()
        {
            this.mockCommunicationManager.Setup(x => x.SendMessage(MessageType.DataCollectionMessage, It.IsAny<DataCollectionMessageEventArgs>())).Throws<Exception>();
            var message = new DataCollectionMessageEventArgs(TestMessageLevel.Error, "message");

            Assert.ThrowsException<Exception>(() =>
                {
                    this.requestHandler.SendDataCollectionMessage(message);
                });
        }

        [TestMethod]
        public void CloseShouldCloseCommunicationChannel()
        {
            this.requestHandler.Close();

            this.mockCommunicationManager.Verify(x => x.StopClient(), Times.Once);
        }

        [TestMethod]
        public void CloseShouldThrowExceptionIfThrownByCommunicationManager()
        {
            this.mockCommunicationManager.Setup(x => x.StopClient()).Throws<Exception>();

            Assert.ThrowsException<Exception>(() =>
                {
                    this.requestHandler.Close();
                });
        }

        [TestMethod]
        public void DisposeShouldCloseCommunicationChannel()
        {
            this.requestHandler.Dispose();

            this.mockCommunicationManager.Verify(x => x.StopClient(), Times.Once);
        }

        [TestMethod]
        public void ProcessRequestsShouldProcessRequests()
        {
            var testHostLaunchedPayload = new TestHostLaunchedPayload();
            testHostLaunchedPayload.ProcessId = 1234;

            this.mockCommunicationManager.SetupSequence(x => x.ReceiveMessage()).Returns(this.beforeTestRunStart)
                                                                                .Returns(new Message() { MessageType = MessageType.TestHostLaunched, Payload = JToken.FromObject(testHostLaunchedPayload) })
                                                                                .Returns(this.afterTestRunEnd);

            this.mockDataCollectionManager.Setup(x => x.SessionStarted()).Returns(true);
            this.mockDataCollectionManager.Setup(x => x.TestHostLaunched(It.IsAny<int>()));
            this.mockDataSerializer.Setup(x => x.DeserializePayload<TestHostLaunchedPayload>(It.Is<Message>(y => y.MessageType == MessageType.TestHostLaunched)))
                                   .Returns(testHostLaunchedPayload);

            this.requestHandler.ProcessRequests();

            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.InitializeCommunication(), Times.Once);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.WaitForRequestHandlerConnection(It.IsAny<int>()), Times.Once);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.ProcessRequests(), Times.Once);

            // Verify SessionStarted events
            this.mockDataCollectionManager.Verify(x => x.SessionStarted(), Times.Once);
            this.mockCommunicationManager.Verify(x => x.SendMessage(MessageType.BeforeTestRunStartResult, It.IsAny<BeforeTestRunStartResult>()), Times.Once);

            // Verify TestHostLaunched events
            this.mockDataCollectionManager.Verify(x => x.TestHostLaunched(1234), Times.Once);

            // Verify AfterTestRun events.
            this.mockDataCollectionManager.Verify(x => x.SessionEnded(It.IsAny<bool>()), Times.Once);
            this.mockCommunicationManager.Verify(x => x.SendMessage(MessageType.AfterTestRunEndResult, It.IsAny<Collection<AttachmentSet>>()), Times.Once);
        }

        [TestMethod]
        public void ProcessRequestsShouldDisposeDataCollectorsOnAfterTestRunEnd()
        {
            this.mockCommunicationManager.SetupSequence(x => x.ReceiveMessage()).Returns(new Message() { MessageType = MessageType.AfterTestRunEnd, Payload = "false" });

            this.requestHandler.ProcessRequests();

            this.mockDataCollectionManager.Verify(x => x.Dispose());
        }

        [TestMethod]
        public void ProcessRequestsShouldThrowExceptionIfThrownByCommunicationManager()
        {
            this.mockCommunicationManager.Setup(x => x.ReceiveMessage()).Throws<Exception>();

            Assert.ThrowsException<Exception>(() => { this.requestHandler.ProcessRequests(); });
        }

        [TestMethod]
        public void ProcessRequestsShouldInitializeTestCaseEventHandlerIfTestCaseLevelEventsAreEnabled()
        {
            this.requestHandler.ProcessRequests();

            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.InitializeCommunication(), Times.Once);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.ProcessRequests(), Times.Once);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.WaitForRequestHandlerConnection(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void ProcessRequestsShouldSetDefaultTimeoutIfNoEnvVarialbeSet()
        {
            this.requestHandler.ProcessRequests();

            this.mockDataCollectionTestCaseEventHandler.Verify(h => h.WaitForRequestHandlerConnection(EnvironmentHelper.DefaultConnectionTimeout * 1000));
        }

        [TestMethod]
        public void ProcessRequestsShouldSetTimeoutBasedOnEnvVariable()
        {
            var timeout = 10;
            Environment.SetEnvironmentVariable(EnvironmentHelper.VstestConnectionTimeout, timeout.ToString());

            this.requestHandler.ProcessRequests();

            this.mockDataCollectionTestCaseEventHandler.Verify(h => h.WaitForRequestHandlerConnection(timeout * 1000));
        }

        [TestMethod]
        public void ProcessRequestsShouldThrowExceptionIFTestCaseEventHandlerConnectionTimeouts()
        {
            this.mockDataCollectionTestCaseEventHandler.Setup(h => h.WaitForRequestHandlerConnection(It.IsAny<int>()))
                .Returns(false);

            var exMessage = Assert.ThrowsException<AggregateException>(() => this.requestHandler.ProcessRequests()).InnerExceptions[0].Message;

            Assert.AreEqual(
                exMessage,
                string.Format(
                    CultureInfo.CurrentUICulture,
                    CommunicationUtilitiesResources.ConnectionTimeoutErrorMessage,
                    CoreUtilitiesConstants.DatacollectorProcessName,
                    CoreUtilitiesConstants.TesthostProcessName,
                    EnvironmentHelper.DefaultConnectionTimeout,
                    EnvironmentHelper.VstestConnectionTimeout));
        }

        [TestMethod]
        public void ProcessRequestsShouldNotInitializeTestCaseEventHandlerIfTestCaseLevelEventsAreNotEnabled()
        {
            this.mockDataCollectionManager.Setup(x => x.SessionStarted()).Returns(false);

            this.requestHandler.ProcessRequests();

            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.InitializeCommunication(), Times.Never);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.ProcessRequests(), Times.Never);
            this.mockDataCollectionTestCaseEventHandler.Verify(x => x.WaitForRequestHandlerConnection(It.IsAny<int>()), Times.Never);
        }
    }
}