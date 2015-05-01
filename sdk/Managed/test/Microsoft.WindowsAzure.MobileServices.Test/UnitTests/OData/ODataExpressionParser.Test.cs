using System;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.OData
{
    [Tag("unit")]
    [Tag("odata")]
    public class ODataExpressionParserTest : TestBase
    {
        [TestMethod]
        public void ParseFilter_Guid()
        {
            Guid filterGuid = Guid.NewGuid();

            QueryNode queryNode = ODataExpressionParser.ParseFilter(string.Format("Field eq guid'{0}'", filterGuid));

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(right);

            Assert.AreEqual("Field", left.MemberName);
            Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.AreEqual(filterGuid, right.Value);
        }

        [TestMethod]
        public void ParseFilter_TrueToken()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("(true eq null) and false");

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as BinaryOperatorNode;
            Assert.IsNotNull(left);

            var trueNode = left.LeftOperand as ConstantNode;
            Assert.IsNotNull(trueNode);
            Assert.AreEqual(true, trueNode.Value);

            var nullNode = left.RightOperand as ConstantNode;
            Assert.IsNotNull(nullNode);
            Assert.AreEqual(null, nullNode.Value);

            var falseNode = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(falseNode);

            Assert.AreEqual(BinaryOperatorKind.And, comparisonNode.OperatorKind);
            Assert.AreEqual(false, falseNode.Value);
        }

        [TestMethod]
        public void ParseFilter_DateTimeMember()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("datetime eq 1");

            Assert.IsNotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(right);

            Assert.AreEqual("datetime", left.MemberName);
            Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.AreEqual(1L, right.Value);
        }

        [TestMethod]
        public void ParseFilter_Guid_InvalidGuidString()
        {
            var ex = AssertEx.Throws<MobileServiceODataException>(() => ODataExpressionParser.ParseFilter(string.Format("Field eq guid'this is not a guid'")));

            Assert.IsTrue(ex.Message.Equals("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).") || ex.Message.Equals("Invalid Guid format: this is not a guid"));
        }
    }
}
