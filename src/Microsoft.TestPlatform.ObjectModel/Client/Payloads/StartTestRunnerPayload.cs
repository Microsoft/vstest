﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Payloads
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class used to define the StartTestRunnerPayload sent by the Vstest.console translation layers into design mode.
    /// </summary>
    public class StartTestRunnerPayload
    {
        /// <summary>
        /// RunSettings used for starting the test runner.
        /// </summary>
        [DataMember]
        public IList<string> Sources { get; set; }

        /// <summary>
        /// RunSettings used for starting the test runner.
        /// </summary>
        [DataMember]
        public string RunSettings { get; set; }

        /// <summary>
        /// Should metrics collection be enabled ?
        /// </summary>
        [DataMember]
        public bool CollectMetrics { get; set; }
    }
}
