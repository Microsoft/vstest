// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.Utilities.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers.Interfaces;

    /// <summary>
    /// The file helper.
    /// </summary>
    public class FileHelper : IFileHelper
    {
        /// <inheritdoc/>
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        /// <inheritdoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc/>
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <inheritdoc/>
        public Stream GetStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        /// <inheritdoc/>
        public IEnumerable<string> EnumerateFiles(string directory, string pattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(directory, pattern, searchOption);
        }

        /// <inheritdoc/>
        public FileAttributes GetFileAttributes(string path)
        {
            return new FileInfo(path).Attributes;
        }
    }
}
