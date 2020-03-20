﻿﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.ObjectModel.UnitTests
{
    using System.Globalization;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CustomStringArrayConverterTests
    {
        private readonly CustomStringArrayConverter customStringArrayConverter;

        public CustomStringArrayConverterTests()
        {
            this.customStringArrayConverter = new CustomStringArrayConverter();
        }

        [TestMethod]
        public void CustomStringArrayConverterShouldDeserializeWellformedJson()
        {
            var json = "[ \"val2\", \"val1\" ]";

            var data = this.customStringArrayConverter.ConvertFrom(null, CultureInfo.InvariantCulture, json) as string[];

            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Length);
            CollectionAssert.AreEqual(new[] { "val2", "val1" }, data);
        }

        [TestMethod]
        public void CustomStringArrayConverterShouldDeserializeEmptyArray()
        {
            var json = "[]";

            var data = this.customStringArrayConverter.ConvertFrom(null, CultureInfo.InvariantCulture, json) as string[];

            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Length);
        }

        [TestMethod]
        public void CustomStringArrayConverterShouldDeserializeNullKeyOrValue()
        {
            var json = "[null, \"val\"]";

            var data = this.customStringArrayConverter.ConvertFrom(null, CultureInfo.InvariantCulture, json) as string[];

            Assert.AreEqual(2, data.Length);
            Assert.IsNull(data[0]);
            Assert.AreEqual("val", data[1]);
        }

        [TestMethod]
        public void CustomStringArrayConverterShouldDeserializeEmptyKeyOrValue()
        {
            var json = "[\"\", \"\"]";

            var data = this.customStringArrayConverter.ConvertFrom(null, CultureInfo.InvariantCulture, json) as string[];

            Assert.AreEqual(2, data.Length);
            Assert.AreEqual(string.Empty, data[0]);
            Assert.AreEqual(string.Empty, data[1]);
        }

        [TestMethod]
        public void CustomStringArrayConverterShouldDeserializeNullValue()
        {
            var data = this.customStringArrayConverter.ConvertFrom(null, CultureInfo.InvariantCulture, null) as string[];

            Assert.IsNull(data);
        }
    }
}