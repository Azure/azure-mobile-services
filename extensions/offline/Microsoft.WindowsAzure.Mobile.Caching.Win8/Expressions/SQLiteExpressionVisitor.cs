using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class SQLiteExpressionVisitor : ODataExpressionVisitor
    {
        private StringBuilder sqlBuilder;
        public string SqlStatement 
        {
            get { return sqlBuilder.ToString(); }
        }

        public SQLiteExpressionVisitor()
        {
            sqlBuilder = new StringBuilder();
        }

        public override IQueryOptions VisitQueryOptions(IQueryOptions query)
        {
            this.sqlBuilder.Append("SELECT * FROM {0}");
            if (query.Filter != null && !string.IsNullOrEmpty(query.Filter.RawValue))
            {
                this.sqlBuilder.Append(" WHERE ");
                this.Visit(query.Filter.Expression);
            }
            if (query.OrderBy != null && !string.IsNullOrEmpty(query.OrderBy.RawValue))
            {
                this.sqlBuilder.Append(" ORDER BY ");
                this.Visit(query.OrderBy.Expression);
            }
            if (query.Top != null && !string.IsNullOrEmpty(query.Top.RawValue))
            {
                this.sqlBuilder.Append(" LIMIT ");
                this.Visit(query.Top.Expression);
            }
            if (query.Skip != null && !string.IsNullOrEmpty(query.Skip.RawValue))
            {
                this.sqlBuilder.Append(" OFFSET ");
                this.Visit(query.Skip.Expression);
            }

            return query;
        }

        public override ODataExpression VisitSkip(ODataSkipExpression expr)
        {
            this.sqlBuilder.Append(expr.Value);
            return expr;
        }

        public override ODataExpression VisitTop(ODataTopExpression expr)
        {
            this.sqlBuilder.Append(expr.Value);
            return expr;
        }

        public override ODataExpression VisitOrderBy(ODataOrderByExpression expr)
        {
            foreach (ODataOrderByExpression.Selector s in expr.Selectors)
            {
                this.Visit(s.Expression);
                this.sqlBuilder.Append(" " + s.Order.ToString().ToUpperInvariant() + " ");
            }
            // remove last space
            this.sqlBuilder.Remove(this.sqlBuilder.Length - 1, 1);
            return expr;
        }

        public override ODataExpression VisitBinary(ODataBinaryExpression expr)
        {
            this.sqlBuilder.Append("(");
            this.Visit(expr.Left);
            switch (expr.ExpressionType)
            {
                case ExpressionType.AndAlso:
                    this.sqlBuilder.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    this.sqlBuilder.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    this.sqlBuilder.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    this.sqlBuilder.Append(" != ");
                    break;
                case ExpressionType.LessThan:
                    this.sqlBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.sqlBuilder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    this.sqlBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.sqlBuilder.Append(" >= ");
                    break;
                case ExpressionType.Add:
                    this.sqlBuilder.Append(" + ");
                    break;
                case ExpressionType.Subtract:
                    this.sqlBuilder.Append(" - ");
                    break;
                case ExpressionType.Multiply:
                    this.sqlBuilder.Append(" * ");
                    break;
                case ExpressionType.Divide:
                    this.sqlBuilder.Append(" / ");
                    break;
                case ExpressionType.Modulo:
                    this.sqlBuilder.Append(" % ");
                    break;
                default:
                    throw new NotSupportedException();
            }
            this.Visit(expr.Right);
            this.sqlBuilder.Append(")");

            return expr;
        }

        public override ODataExpression VisitUnary(ODataUnaryExpression expr)
        {
            switch (expr.ExpressionType)
            {
                case ExpressionType.Not:
                    this.sqlBuilder.Append("NOT ");
                    this.Visit(expr.Operand);
                    break;
                case ExpressionType.Negate:
                    this.sqlBuilder.Append("-");
                    this.Visit(expr.Operand);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return expr;
        }

        public override ODataExpression VisitConstant(ODataConstantExpression expr)
        {
            if(expr.Value == null)
            {
                sqlBuilder.Append("NULL");
                return expr;
            }

            Type type = expr.Value.GetType();
            if (type == typeof(bool))
            {
                this.sqlBuilder.Append((bool)expr.Value ? "'True'" : "'False'");
            }
            else if (type == typeof(long) || type == typeof(int) || type == typeof(short) || type == typeof(sbyte)
                || type == typeof(ulong) || type == typeof(uint) || type == typeof(ushort) || type == typeof(double)
                || type == typeof(float) || type == typeof(decimal))
            {
                this.sqlBuilder.Append((int)expr.Value);
            }
            else
            {
                this.sqlBuilder.AppendFormat("'{0}'", expr.Value);
            }

            return expr;
        }

        public override ODataExpression VisitFunctionCall(ODataFunctionCallExpression expr)
        {
            switch(expr.Name)
            {
                case "substringof":
                    this.sqlBuilder.Append("(");
                    this.Visit(expr.Arguments[1]);
                    this.sqlBuilder.Append(" LIKE ");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(")");
                    break;
                case "endswith":
                case "startswith":
                    this.sqlBuilder.Append("(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(" LIKE ");
                    this.Visit(expr.Arguments[1]);
                    this.sqlBuilder.Append(")");
                    break;
                case "length":
                    this.sqlBuilder.Append("(length(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "indexof":
                    this.sqlBuilder.Append("(");
                    this.sqlBuilder.Append("insrt(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(", ");
                    this.Visit(expr.Arguments[1]);
                    this.sqlBuilder.Append("))");
                    break;
                case "replace":
                    this.sqlBuilder.Append("(");
                    this.sqlBuilder.Append("replace(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(", ");
                    this.Visit(expr.Arguments[1]);
                    this.sqlBuilder.Append(", ");
                    this.Visit(expr.Arguments[2]);
                    this.sqlBuilder.Append("))");
                    break;
                case "substring":
                    this.sqlBuilder.Append("(");
                    this.sqlBuilder.Append("substr(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(", ");
                    this.Visit(expr.Arguments[1]);
                    if (expr.Arguments.Length > 2)
                    {
                        this.sqlBuilder.Append(", ");
                        this.Visit(expr.Arguments[2]);
                    }
                    this.sqlBuilder.Append("))");
                    break;
                case "tolower":
                    this.sqlBuilder.Append("(lower(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "toupper":
                    this.sqlBuilder.Append("(upper(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "trim":
                    this.sqlBuilder.Append("(trim(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "concat":
                    this.sqlBuilder.Append("(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append(" || ");
                    this.Visit(expr.Arguments[1]);
                    this.sqlBuilder.Append(")");
                    break;
                    //DATE
                case "day":
                    this.sqlBuilder.Append("(datetime('%d',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "hour":
                    this.sqlBuilder.Append("(datetime('%H',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "minute":
                    this.sqlBuilder.Append("(datetime('%M',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "month":
                    this.sqlBuilder.Append("(datetime('%m',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "second":
                    this.sqlBuilder.Append("(datetime('%S',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "year":
                    this.sqlBuilder.Append("(datetime('%Y',");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                //Math
                case "round":
                    this.sqlBuilder.Append("(round(");
                    this.Visit(expr.Arguments[0]);
                    this.sqlBuilder.Append("))");
                    break;
                case "floor":
                case "ceiling":
                    throw new NotSupportedException("SQLite does not support floor or ceiling");
            }

            return expr;
        }

        public override ODataExpression VisitMember(ODataMemberExpression expr)
        {
            this.sqlBuilder.Append(expr.Member);
            return expr;
        }

        public override ODataExpression VisitParameter(ODataParameterExpression expr)
        {
            throw new NotSupportedException();
        }
    }
}
