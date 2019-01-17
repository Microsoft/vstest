﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestWindow.FullyQualifiedNameUtilities;

namespace TestWindow.Tests.FullyQualifiedNameUtilities
{
    [TestClass]
    public class FullyQualifiedNameParserTests
    {
        [TestMethod]
        public void ParseTypeName()
        {
            (string, string) Parse(string fullTypeName)
            {
                FullyQualifiedNameParser.ParseTypeName(fullTypeName, out var namespaceName, out var typeName);
                return (namespaceName, typeName);
            };

            Assert.AreEqual(("NS", "Class"), Parse("NS.Class"));
            Assert.AreEqual(("NS.NS", "Class"), Parse("NS.NS.Class"));
            Assert.AreEqual(("NS.NS", "Class`2"), Parse("NS.NS.Class`2"));
            Assert.AreEqual(("NS.NS", "ClassA`2+ClassInner"), Parse("NS.NS.ClassA`2+ClassInner"));
            Assert.AreEqual(("NS.NS", "ClassA`2+ClassInner`1"), Parse("NS.NS.ClassA`2+ClassInner`1"));
            Assert.AreEqual(("", "ClassA`2+ClassInner`1"), Parse("ClassA`2+ClassInner`1"));
        }

        [TestMethod]
        public void ParseMethodName()
        {
            (string, int, string[]) Parse(string methodName)
            {
                FullyQualifiedNameParser.ParseMethodName(methodName, out var method, out var arity, out var parameterTypes);
                return (method, arity, parameterTypes);
            }

            void AssertParse(string expectedMethod, int expectedArity, string[] expectedParams, string expression)
            {
                var (method, arity, parameters) = Parse(expression);
                Assert.AreEqual(expectedMethod, method);
                Assert.AreEqual(expectedArity, arity);
                CollectionAssert.AreEqual(expectedParams, parameters, "parameter comparison");
            }

            Assert.AreEqual(("Method", 0, null), Parse("Method"));
            Assert.AreEqual(("Method", 0, null), Parse("Method()"));
            Assert.AreEqual(("Method<A,B>", 2, null), Parse("Method<A,B>`2()"));
            AssertParse("Method", 0, new string[] { "System.Int32" }, "Method(System.Int32)");
            AssertParse("Method", 0, new string[] { "TypeA", "List<B>" }, "Method(TypeA,List<B>)");
            AssertParse("Method", 1, new string[] { "B", "List<B>" }, "Method`1(B,List<B>)");
            AssertParse("Method", 0, new string[] { "B[]" }, "Method(B[])");
            AssertParse("Method", 0, new string[] { "A[,]", "B[,,][]" }, "Method(A[,],B[,,][])");
        }

        [TestMethod]
        public void ParseInvalidMethodName()
        {
            (string, int, string[]) Parse(string methodName)
            {
                FullyQualifiedNameParser.ParseMethodName(methodName, out var method, out var arity, out var parameterTypes);
                return (method, arity, parameterTypes);
            }

            Assert.ThrowsException<InvalidQualifiedNameException>(() => Parse(" Method"), "Whitespace is not valid in a FullyQualifiedName (pos: 0)");
            Assert.ThrowsException<InvalidQualifiedNameException>(() => Parse("Method( List)"), "Whitespace is not valid in a FullyQualifiedName (pos: 7)");

            Assert.ThrowsException<InvalidQualifiedNameException>(() => Parse("Method(List)xa"), "Unexpected characters after the end of the FullyQualifiedName (pos: 7)");

            Assert.ThrowsException<InvalidQualifiedNameException>(() => Parse("Method("), "FullyQualifiedName is incomplete");
            Assert.ThrowsException<InvalidQualifiedNameException>(() => Parse("Method`4a"), "Method arity must be numeric");
        }

    }
}
