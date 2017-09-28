﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestPlatform.Common.UnitTests.Filtering
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

    [TestClass]
    public class FastFilterTests
    {
        [TestMethod]
        public void MultiplePropertyNamesShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName=Test1|Category=Core");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void MultipleOperatorKindsShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("Name=Test1&(Name=Test2|NameTest3)");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void MultipleOperationKindsShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("Name!=TestClass1&Category=Nightly");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void ContainsOperationShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("Name~TestClass1");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void AndOperatorAndEqualsOperationShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("Name=Test1&Name=Test2");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void OrOperatorAndNotEqualsOperationShouldNotCreateFastFilter()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("Name!=Test1|Name!=Test2");
            var fastFilter = filterExpressionWrapper.fastFilter;

            Assert.IsTrue(fastFilter == null);
        }

        [TestMethod]
        public void FastFilterWithSingleEqualsClause()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName=Test1");
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsFalse(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);            

            Assert.IsTrue(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test2"));
        }

        [TestMethod]
        public void FastFilterWithMultipleEqualsClause()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName=Test1|FullyQualifiedName=Test2|FullyQualifiedName=Test3");
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1", "Test2", "Test3" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsFalse(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);            

            Assert.IsTrue(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test2"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test3"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test4"));
        }

        [TestMethod]
        public void FastFilterWithMultipleEqualsClauseAndRegex()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName=Test1|FullyQualifiedName=Test2|FullyQualifiedName=Test3", new FilterOptions() { FilterRegEx = @"^[^\s\(]+" });
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1", "Test2", "Test3" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsFalse(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);

            Assert.IsTrue(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test2"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test3"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test1 (123)"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test2(123)"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test3  (123)"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test4"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test4 ()"));
        }

        [TestMethod]
        public void FastFilterWithSingleNotEqualsClause()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName!=Test1");
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsTrue(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);

            Assert.IsFalse(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test2"));
        }

        [TestMethod]
        public void FastFilterWithMultipleNotEqualsClause()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName!=Test1&FullyQualifiedName!=Test2&FullyQualifiedName!=Test3");
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1", "Test2", "Test3" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsTrue(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);

            Assert.IsFalse(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test2"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test3"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test4"));
        }

        [TestMethod]
        public void FastFilterWithMultipleNotEqualsClauseAndRegex()
        {
            var filterExpressionWrapper = new FilterExpressionWrapper("FullyQualifiedName!=Test1&FullyQualifiedName!=Test2&FullyQualifiedName!=Test3", new FilterOptions() { FilterRegEx = @"^[^\s\(]+" });
            var fastFilter = filterExpressionWrapper.fastFilter;

            var expectedFilterValues = new HashSet<string>() { "Test1", "Test2", "Test3" };

            Assert.IsTrue(fastFilter != null);
            Assert.AreEqual("FullyQualifiedName", fastFilter.FilterPropertyName);
            Assert.IsTrue(fastFilter.IsFilteredOutWhenMatched);
            Assert.IsTrue(expectedFilterValues.SetEquals(fastFilter.FilterPropertyValues));

            filterExpressionWrapper.ValidForProperties(new List<string>() { "FullyQualifiedName" }, null);

            Assert.IsFalse(fastFilter.Evaluate((s) => "Test1"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test2"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test3"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test1 (123)"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test2(123)"));
            Assert.IsFalse(fastFilter.Evaluate((s) => "Test3  (123)"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test4"));
            Assert.IsTrue(fastFilter.Evaluate((s) => "Test4 (123)"));
        }
    }
}
