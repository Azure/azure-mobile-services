// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Query
{
    [TestClass]
    public class MobileServiceTableQueryDescriptionTests
    {
        private const string EscapedODataString = "$filter=((__updatedat gt datetime'2014-04-04T07:00:00.000Z') and startswith(text,'this%26%27%27%25%25%3D%2C%3F%23'))";

        [TestMethod]
        public void Parse_DoesNotThrow_OnIncompleteQuery()
        {
            var desc = MobileServiceTableQueryDescription.Parse("someTable", "$select&");
        }

        [TestMethod]
        public void Parse_UnescapesThe_Uri()
        {
            var desc = MobileServiceTableQueryDescription.Parse("someTable", EscapedODataString);

            var and = desc.Filter as BinaryOperatorNode;
            Assert.IsNotNull(and);
            Assert.AreEqual(and.OperatorKind, BinaryOperatorKind.And);

            var gt = and.LeftOperand as BinaryOperatorNode;
            Assert.IsNotNull(gt);
            Assert.AreEqual(gt.OperatorKind, BinaryOperatorKind.GreaterThan);

            var updatedAt = gt.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(updatedAt);
            Assert.AreEqual(updatedAt.MemberName, "__updatedat");

            var datetime = gt.RightOperand as ConstantNode;
            Assert.IsNotNull(datetime);
            Assert.AreEqual(datetime.Value, new DateTime(2014, 4, 4, 0, 0, 0, DateTimeKind.Utc));

            var startswith = and.RightOperand as FunctionCallNode;
            Assert.IsNotNull(startswith);
            Assert.AreEqual(startswith.Arguments.Count, 2);

            var text = startswith.Arguments[0] as MemberAccessNode;
            Assert.IsNotNull(text);
            Assert.AreEqual(text.MemberName, "text");

            var value = startswith.Arguments[1] as ConstantNode;
            Assert.IsNotNull(value);
            Assert.AreEqual(value.Value, "this&'%%=,?#");
        }

        [TestMethod]
        public void ToQueryString_EscapesThe_Uri()
        {
            // __updatedat gt datetimeoffset'4-4-2014 0:0:0.000Z'
            var updatedAt = new MemberAccessNode(null, "__updatedat");
            var datetime = new ConstantNode(new DateTime(2014, 4, 4, 0, 0, 0));
            var gt = new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, updatedAt, datetime);

            // startswith(text,'this&''%%=,?#')
            var text = new MemberAccessNode(null, "text");
            var value = new ConstantNode("this&'%%=,?#");
            var startswith = new FunctionCallNode("startswith", new QueryNode[] { text, value });

            //__updatedat gt datetimeoffset'4-4-2014 0:0:0.000Z' and startswith(text,'this&''%%=,?#')
            var and = new BinaryOperatorNode(BinaryOperatorKind.And, gt, startswith);

            var desc = new MobileServiceTableQueryDescription("someTable") { Filter = and };
            Assert.AreEqual(desc.ToQueryString(), EscapedODataString);
        }
    }
}
