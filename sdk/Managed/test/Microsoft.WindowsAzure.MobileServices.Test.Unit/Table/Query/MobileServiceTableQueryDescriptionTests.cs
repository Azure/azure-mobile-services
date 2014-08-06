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
        private const string EscapedODataString = "$filter=" +
                                                  "((__updatedat gt datetimeoffset'2014-04-04T07%3A00%3A00.0000000%2B00%3A00') and " +
                                                  "((someDate gt datetime'2014-04-04T07%3A00%3A00.000Z') and " +
                                                  "startswith(text,'this%26%27%27%25%25%3D%2C%3F%23')))";

        [TestMethod]
        public void Parse_DoesNotThrow_OnIncompleteQuery()
        {
            var desc = MobileServiceTableQueryDescription.Parse("someTable", "$select&");
        }

        [TestMethod]
        public void Parse_UnescapesThe_Uri()
        {
            var desc = MobileServiceTableQueryDescription.Parse("someTable", EscapedODataString);

            var and1 = desc.Filter as BinaryOperatorNode;
            Assert.IsNotNull(and1);
            Assert.AreEqual(and1.OperatorKind, BinaryOperatorKind.And);

            var expectedDateTime = new DateTimeOffset(2014, 4, 4, 7, 0, 0, TimeSpan.Zero);

            var gt1 = and1.LeftOperand as BinaryOperatorNode;
            Assert.IsNotNull(gt1);
            Assert.AreEqual(gt1.OperatorKind, BinaryOperatorKind.GreaterThan);
            var updatedAt1 = gt1.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(updatedAt1);
            Assert.AreEqual(updatedAt1.MemberName, "__updatedat");

            var datetime1 = gt1.RightOperand as ConstantNode;
            Assert.IsNotNull(datetime1);
            Assert.AreEqual(datetime1.Value, expectedDateTime);

            var and2 = and1.RightOperand as BinaryOperatorNode;
            Assert.IsNotNull(and2);
            Assert.AreEqual(and2.OperatorKind, BinaryOperatorKind.And);

            var gt2 = and2.LeftOperand as BinaryOperatorNode;
            Assert.IsNotNull(gt2);
            Assert.AreEqual(gt2.OperatorKind, BinaryOperatorKind.GreaterThan);

            var updatedAt2 = gt2.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(updatedAt2);
            Assert.AreEqual(updatedAt2.MemberName, "someDate");

            var datetime2 = gt2.RightOperand as ConstantNode;
            Assert.IsNotNull(datetime2);
            //Note - shouldn't the OData value be parsed as UTC?
            Assert.AreEqual(datetime2.Value, expectedDateTime.LocalDateTime);

            var startswith = and2.RightOperand as FunctionCallNode;
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
        public void ToODataString_EscapesThe_Uri()
        {

            //__updatedat gt datetimeoffset'2014-04-04T07:00:00.0000000+00:00'
            var datetime1 = new ConstantNode(new DateTimeOffset(2014, 4, 4, 7, 0, 0, TimeSpan.FromHours(0)));
            var updatedAt = new MemberAccessNode(null, "__updatedat");
            var gt1 = new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, updatedAt, datetime1);

            // __updatedat gt datetime'2014-04-04T07:0:0.000Z'
            var datetime2 = new ConstantNode(new DateTime(2014, 4, 4, 7, 0, 0, DateTimeKind.Utc));
            var someDate = new MemberAccessNode(null, "someDate");
            var gt2 = new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, someDate, datetime2);

            // startswith(text,'this&''%%=,?#')
            var text = new MemberAccessNode(null, "text");
            var value = new ConstantNode("this&'%%=,?#");
            var startswith = new FunctionCallNode("startswith", new QueryNode[] { text, value });

            //__updatedat gt datetimeoffset'2014-04-04T07:00:00.0000000+00:00' and startswith(text,'this&''%%=,?#')
            var and2 = new BinaryOperatorNode(BinaryOperatorKind.And, gt2, startswith);

            var and1 = new BinaryOperatorNode(BinaryOperatorKind.And, gt1, and2);

            var desc = new MobileServiceTableQueryDescription("someTable") { Filter = and1 };
            Assert.AreEqual(desc.ToODataString(), EscapedODataString);
        }
    }
}
