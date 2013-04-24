using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class ODataExpression
    {
        public ExpressionType ExpressionType { get; internal set; }

        public virtual ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public static ODataExpression Equal(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Equal;
            return bex;
        }

        public static ODataExpression NotEqual(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.NotEqual;
            return bex;
        }

        public static ODataExpression GreaterThan(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.GreaterThan;
            return bex;
        }

        public static ODataExpression GreaterThanOrEqual(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.GreaterThanOrEqual;
            return bex;
        }

        public static ODataExpression LessThan(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.LessThan;
            return bex;
        }

        public static ODataExpression LessThanOrEqual(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.LessThanOrEqual;
            return bex;
        }

        public static ODataExpression Add(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Add;
            return bex;
        }

        public static ODataExpression Subtract(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Subtract;
            return bex;
        }

        public static ODataExpression OrElse(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.OrElse;
            return bex;
        }

        public static ODataExpression AndAlso(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.AndAlso;
            return bex;
        }

        public static ODataExpression Multiply(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Multiply;
            return bex;
        }

        public static ODataExpression Divide(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Divide;
            return bex;
        }

        public static ODataExpression Modulo(ODataExpression left, ODataExpression right)
        {
            ODataBinaryExpression bex = new ODataBinaryExpression(left, right);
            bex.ExpressionType = ExpressionType.Modulo;
            return bex;
        }

        public static ODataExpression Negate(ODataExpression expr)
        {
            return new ODataUnaryExpression(expr, ExpressionType.Negate);
        }

        public static ODataExpression Not(ODataExpression expr)
        {
            return new ODataUnaryExpression(expr, ExpressionType.Not);
        }
    }

    public class ODataParameterExpression : ODataExpression
    {
        public ODataParameterExpression(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitParameter(this);
        }
    }

    public class ODataUnaryExpression : ODataExpression
    {
        public ODataUnaryExpression(ODataExpression expr, ExpressionType type)
        {
            this.Operand = expr;
            this.ExpressionType = type;
        }

        public ODataExpression Operand { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

    public class ODataBinaryExpression : ODataExpression
    {
        public ODataBinaryExpression(ODataExpression left, ODataExpression right)
        {
            this.Left = left;
            this.Right = right;
        }

        public ODataExpression Left { get; private set; }
        public ODataExpression Right { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public class ODataConstantExpression : ODataExpression
    {
        public ODataConstantExpression(object value)
        {
            this.Value = value;
            this.ExpressionType = ExpressionType.Constant;
        }

        public ODataConstantExpression(object value, Type type)
        {
            this.Value = value;
        }

        public object Value { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }
    }

    public class ODataMemberExpression : ODataExpression
    {
        public ODataMemberExpression(ODataExpression instance, string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException("memberName");
            }

            this.Instance = instance;
            this.Member = memberName;
            this.ExpressionType = ExpressionType.MemberAccess;
        }

        public ODataExpression Instance { get; private set; }
        public string Member { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitMember(this);
        }
    }

    public class ODataFunctionCallExpression : ODataExpression
    {
        public ODataFunctionCallExpression(string name, ODataExpression[] arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.ExpressionType = ExpressionType.Call;
        }

        public string Name { get; private set; }
        public ODataExpression[] Arguments { get; private set; }

        public override ODataExpression Accept(ODataExpressionVisitor visitor)
        {
            return visitor.VisitFunctionCall(this);
        }
    }

    public class ODataOrderByExpression : ODataExpression
    {
        public ODataOrderByExpression(IEnumerable<Selector> selectors)
        {
            if (selectors == null)
            {
                throw new ArgumentNullException("selectors");
            }

            this.Selectors = selectors;
            this.ExpressionType = (ExpressionType)10005;
        }

        public IEnumerable<Selector> Selectors { get; private set; }

        public struct Selector
        {
            public readonly ODataExpression Expression;
            public readonly Order Order;
            public Selector(ODataExpression e, Order order)
            {
                this.Expression = e;
                this.Order = order;
            }
        }
    }

    public class ODataInlineCountExpression : ODataExpression
    {
        public ODataInlineCountExpression(InlineCount count)
        {
            if (Enum.IsDefined(typeof(InlineCount), count))
            {
                throw new ArgumentException(string.Format("Undefined enum value on enum {0}.", count));
            }
            this.Value = count;
            this.ExpressionType = ExpressionType.Constant;
        }

        public InlineCount Value { get; private set; }
    }

    public class ODataSkipExpression : ODataExpression
    {
        public ODataSkipExpression(int skip)
        {
            if (skip < 0)
            {
                throw new ArgumentException("skip must be > 0");
            }
            this.Value = skip;
            this.ExpressionType = ExpressionType.Constant;
        }

        public int Value { get; private set; }
    }

    public class ODataTopExpression : ODataExpression
    {
        public ODataTopExpression(int top)
        {
            if (top < 0)
            {
                throw new ArgumentException("top must be > 0");
            }
            this.Value = top;
            this.ExpressionType = ExpressionType.Constant;
        }

        public int Value { get; private set; }
    }
}
