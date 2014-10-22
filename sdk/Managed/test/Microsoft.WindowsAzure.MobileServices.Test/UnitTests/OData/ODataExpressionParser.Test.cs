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

            BinaryOperatorNode comparisonNode = queryNode as BinaryOperatorNode;
            Assert.IsNotNull(comparisonNode);

            MemberAccessNode left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.IsNotNull(left);

            ConstantNode right = comparisonNode.RightOperand as ConstantNode;
            Assert.IsNotNull(right);

            Assert.AreEqual("Field", left.MemberName);
            Assert.AreEqual(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.AreEqual(filterGuid, right.Value);
        }

        [TestMethod]
        public void ParseFilter_Guid_InvalidGuidString()
        {
            var ex = AssertEx.Throws<MobileServiceODataException>(() => ODataExpressionParser.ParseFilter(string.Format("Field eq guid'this is not a guid'")));

            Assert.AreEqual(ex.Message, "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
        }
    }
}
