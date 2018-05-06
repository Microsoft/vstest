﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.Collector
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Coverage;
    using Coverage.Interfaces;
    using TraceDataCollector.Resources;

    internal class CollectorUtility : ICollectorUtility
    {
        /// <summary>
        /// Vanguard executable name
        /// </summary>
        private const string VanguardExeName = @"CodeCoverage.exe";

        /// <inheritdoc />
        public string GetVanguardPath()
        {
            var vanguardPath = Path.Combine(this.GetVanguardDirectory(), VanguardExeName);
            if (!File.Exists(vanguardPath))
            {
                throw new VanguardException(string.Format(CultureInfo.CurrentUICulture, Resources.VangurdNotFound, vanguardPath));
            }

            return vanguardPath;
        }

        /// <inheritdoc />
        public string GetVanguardDirectory()
        {
            return Path.GetDirectoryName(typeof(CollectorUtility).GetTypeInfo().Assembly.Location);
        }
    }
}