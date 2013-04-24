using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Caching;

namespace Microsoft.WindowsAzure.MobileServices.Caching.Test
{
    [TestClass]
    public class ODataFilterParserTest
    {
        [TestMethod]
        public void ParenthesisExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("(true)");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataConstantExpression));
            Assert.IsTrue((bool)((ODataConstantExpression)exp).Value);
        }

        [TestMethod]
        public void BooleanExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("false");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataConstantExpression));
            Assert.IsFalse((bool)((ODataConstantExpression)exp).Value);
        }

        [TestMethod]
        public void AndExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("true and false");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.AndAlso, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.IsTrue((bool)((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.IsFalse((bool)((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void OrExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("true or false");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.OrElse, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.IsTrue((bool)((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.IsFalse((bool)((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void BoolPrimitiveMemberExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("Product/IsAvailable");

            Exception ex = null;
            try
            {
                ODataExpression exp = parser.Parse();
            }
            catch (NotSupportedException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);

            //Assert.IsNotNull(exp);
            //Assert.IsTrue(exp.GetType() == typeof(ODataMemberExpression));

            //ODataMemberExpression memExp = (ODataMemberExpression)exp;
            //Assert.AreEqual(ExpressionType.MemberAccess, memExp.ExpressionType);
            //Assert.AreEqual("Product", memExp.Member);

            //Assert.AreEqual(ExpressionType.MemberAccess, memExp.Instance.ExpressionType);
            //Assert.AreEqual("IsAvailable", ((ODataMemberExpression)memExp.Instance).Member);
        }

        [TestMethod]
        public void EqualExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("4 eq 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Equal, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(4, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void NotEqualExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3 ne 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.NotEqual, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void LessThanExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("34.0f lt 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.LessThan, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(34.0f, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void LessThanOrEqualExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3 le 5L");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.LessThanOrEqual, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5L, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void GreaterThanExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.99m gt 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.GreaterThan, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.99m, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void GreaterThanOrEqualExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 ge 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.GreaterThanOrEqual, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void AddExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 add 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Add, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void SubtractExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 sub 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Subtract, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void MultiplyExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 mul 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Multiply, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void DivideExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 div 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Divide, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void ModuloExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("3.0 mod 5");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataBinaryExpression));

            ODataBinaryExpression binExp = (ODataBinaryExpression)exp;
            Assert.AreEqual(ExpressionType.Modulo, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void NegateExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("- (3.0 mod 5)");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataUnaryExpression));

            ODataUnaryExpression unExp = (ODataUnaryExpression)exp;
            Assert.AreEqual(ExpressionType.Negate, unExp.ExpressionType);

            ODataBinaryExpression binExp = unExp.Operand as ODataBinaryExpression;
            Assert.IsNotNull(binExp);
            Assert.AreEqual(ExpressionType.Modulo, binExp.ExpressionType);

            Assert.IsTrue(binExp.Left.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(3.0, ((ODataConstantExpression)binExp.Left).Value);
            Assert.IsTrue(binExp.Right.GetType() == typeof(ODataConstantExpression));
            Assert.AreEqual(5, ((ODataConstantExpression)binExp.Right).Value);
        }

        [TestMethod]
        public void NotExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("not IsAvailable");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataUnaryExpression));

            ODataUnaryExpression unExp = (ODataUnaryExpression)exp;
            Assert.AreEqual(ExpressionType.Not, unExp.ExpressionType);

            ODataMemberExpression binExp = unExp.Operand as ODataMemberExpression;
            Assert.IsNotNull(binExp);
            Assert.AreEqual(ExpressionType.MemberAccess, binExp.ExpressionType);

            Assert.IsNull(binExp.Instance);
            Assert.AreEqual("IsAvailable", binExp.Member);
        }

        [TestMethod]
        public void IsOfExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("isof ( IsAvailable, 'Edm.Boolean' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("isof", unExp.Name);
            Assert.AreEqual("IsAvailable", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("Edm.Boolean", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void CastExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("cast ( IsAvailable, 'Edm.String' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("cast", unExp.Name);
            Assert.AreEqual("IsAvailable", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("Edm.String", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void BoolCastExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("cast ( IsAvailable, 'Edm.Boolean' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("cast", unExp.Name);
            Assert.AreEqual("IsAvailable", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("Edm.Boolean", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void EndswithMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("endswith ( Exp, 'test' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("endswith", unExp.Name);
            Assert.AreEqual("Exp", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("test", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void IndexOfMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("indexof ( Exp, 'test' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("indexof", unExp.Name);
            Assert.AreEqual("Exp", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("test", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void SubstringOfMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("substringof ( IsAvailable, 'Edm.Boolean' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("substringof", unExp.Name);
            Assert.AreEqual("IsAvailable", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("Edm.Boolean", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void IntersectsMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("geo.intersects ( IsAvailable, 'Edm.Boolean' )");

            ODataExpression exp = parser.Parse();

            Assert.IsNotNull(exp);
            Assert.IsTrue(exp.GetType() == typeof(ODataFunctionCallExpression));

            ODataFunctionCallExpression unExp = (ODataFunctionCallExpression)exp;
            Assert.AreEqual(ExpressionType.Call, unExp.ExpressionType);

            Assert.AreEqual("geo.intersects", unExp.Name);
            Assert.AreEqual("IsAvailable", unExp.Arguments.Cast<ODataMemberExpression>().First().Member);
            Assert.AreEqual("Edm.Boolean", unExp.Arguments.Skip(1).Cast<ODataConstantExpression>().First().Value);
        }

        [TestMethod]
        public void AnyMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("any(Actors, it/Name eq 'John Belushi')");

            Exception ex = null;
            try
            {
                ODataExpression exp = parser.Parse();
            }
            catch (NotSupportedException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
        }

        [TestMethod]
        public void AllMethodCallExpressionTest()
        {
            ODataFilterParser parser = new ODataFilterParser("all(Actors, it/Name eq 'John Belushi')");

            Exception ex = null;
            try
            {
                ODataExpression exp = parser.Parse();
            }
            catch (NotSupportedException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
        }
    }
}
