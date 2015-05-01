// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal class ODataExpressionParser
    {
        private ODataExpressionLexer lexer;
        private static Dictionary<string, QueryNode> keywords;

        static ODataExpressionParser()
        {
            keywords = new Dictionary<string, QueryNode>()
			{
				{ "true", new ConstantNode(true) },
				{ "false", new ConstantNode(false) },
				{ "null", new ConstantNode(null) },
				{ "datetime", null }, // type constructed dynamically,
                { "datetimeoffset", null }, // type constructed dynamically
                { "guid", null } // type constructed dynamically
                 
			};
        }

        private ODataExpressionParser(string expression)
        {
            this.lexer = new ODataExpressionLexer(expression);
        }

        public QueryNode ParseFilter()
        {
            QueryNode expr = this.ParseExpression();

            this.ValidateToken(QueryTokenKind.End, () => "The specified odata query has syntax errors.");

            return expr;
        }

        public IList<OrderByNode> ParseOrderBy()
        {
            var orderings = new List<OrderByNode>();
            while (true)
            {
                QueryNode expr = this.ParseExpression();
                OrderByDirection direction = OrderByDirection.Ascending;
                if (this.TokenIdentifierIs("asc"))
                {
                    this.lexer.NextToken();
                }
                else if (this.TokenIdentifierIs("desc"))
                {
                    this.lexer.NextToken();
                    direction = OrderByDirection.Descending;
                }
                orderings.Add(new OrderByNode(expr, direction));
                if (this.lexer.Token.Kind != QueryTokenKind.Comma)
                {
                    break;
                }
                this.lexer.NextToken();
            }
            this.ValidateToken(QueryTokenKind.End, () => "The specified odata query has syntax errors.");
            return orderings;
        }

        private QueryNode ParseExpression()
        {
            return this.ParseLogicalOr();
        }

        private QueryNode ParseLogicalOr()
        {
            QueryNode left = this.ParseLogicalAnd();
            while (this.lexer.Token.Kind == QueryTokenKind.Or)
            {
                this.lexer.NextToken();
                QueryNode right = this.ParseLogicalAnd();
                left = new BinaryOperatorNode(BinaryOperatorKind.Or, left, right);
            }
            return left;
        }

        private QueryNode ParseLogicalAnd()
        {
            QueryNode left = this.ParseComparison();
            while (this.lexer.Token.Kind == QueryTokenKind.And)
            {
                this.lexer.NextToken();
                QueryNode right = this.ParseComparison();
                left = new BinaryOperatorNode(BinaryOperatorKind.And, left, right);
            }
            return left;
        }

        private QueryNode ParseComparison()
        {
            QueryNode left = this.ParseAdditive();

            while (this.lexer.Token.Kind == QueryTokenKind.Equal ||
                   this.lexer.Token.Kind == QueryTokenKind.NotEqual ||
                   this.lexer.Token.Kind == QueryTokenKind.GreaterThan ||
                   this.lexer.Token.Kind == QueryTokenKind.GreaterThanEqual ||
                   this.lexer.Token.Kind == QueryTokenKind.LessThan ||
                   this.lexer.Token.Kind == QueryTokenKind.LessThanEqual)
            {

                QueryTokenKind opKind = this.lexer.Token.Kind;
                this.lexer.NextToken();
                QueryNode right = this.ParseAdditive();

                switch (opKind)
                {
                    case QueryTokenKind.Equal:
                        left = new BinaryOperatorNode(BinaryOperatorKind.Equal, left, right);
                        break;
                    case QueryTokenKind.NotEqual:
                        left = new BinaryOperatorNode(BinaryOperatorKind.NotEqual, left, right);
                        break;
                    case QueryTokenKind.GreaterThan:
                        left = new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, left, right);
                        break;
                    case QueryTokenKind.GreaterThanEqual:
                        left = new BinaryOperatorNode(BinaryOperatorKind.GreaterThanOrEqual, left, right);
                        break;
                    case QueryTokenKind.LessThan:
                        left = new BinaryOperatorNode(BinaryOperatorKind.LessThan, left, right);
                        break;
                    case QueryTokenKind.LessThanEqual:
                        left = new BinaryOperatorNode(BinaryOperatorKind.LessThanOrEqual, left, right);
                        break;
                }
            }
            return left;
        }

        private QueryNode ParseAdditive()
        {
            QueryNode left = this.ParseMultiplicative();

            while (this.lexer.Token.Kind == QueryTokenKind.Add || this.lexer.Token.Kind == QueryTokenKind.Sub)
            {
                QueryTokenKind opKind = this.lexer.Token.Kind;
                this.lexer.NextToken();
                QueryNode right = this.ParseMultiplicative();
                switch (opKind)
                {
                    case QueryTokenKind.Add:
                        left = new BinaryOperatorNode(BinaryOperatorKind.And, left, right);
                        break;
                    case QueryTokenKind.Sub:
                        left = new BinaryOperatorNode(BinaryOperatorKind.Subtract, left, right);
                        break;
                }
            }
            return left;
        }

        private QueryNode ParseMultiplicative()
        {
            QueryNode left = this.ParseUnary();

            while (this.lexer.Token.Kind == QueryTokenKind.Multiply ||
                   this.lexer.Token.Kind == QueryTokenKind.Divide ||
                   this.lexer.Token.Kind == QueryTokenKind.Modulo)
            {
                QueryTokenKind opKind = this.lexer.Token.Kind;
                this.lexer.NextToken();
                var right = this.ParseUnary();
                switch (opKind)
                {
                    case QueryTokenKind.Multiply:
                        left = new BinaryOperatorNode(BinaryOperatorKind.Multiply, left, right);
                        break;
                    case QueryTokenKind.Divide:
                        left = new BinaryOperatorNode(BinaryOperatorKind.Divide, left, right);
                        break;
                    case QueryTokenKind.Modulo:
                        left = new BinaryOperatorNode(BinaryOperatorKind.Modulo, left, right);
                        break;
                }
            }
            return left;
        }

        private QueryNode ParseUnary()
        {
            if (this.lexer.Token.Kind == QueryTokenKind.Minus ||
                this.lexer.Token.Kind == QueryTokenKind.Not)
            {
                QueryTokenKind opKind = this.lexer.Token.Kind;
                int opPos = this.lexer.Token.Position;
                this.lexer.NextToken();
                if (opKind == QueryTokenKind.Minus &&
                    (this.lexer.Token.Kind == QueryTokenKind.IntegerLiteral ||
                    this.lexer.Token.Kind == QueryTokenKind.RealLiteral))
                {
                    this.lexer.Token.Text = "-" + this.lexer.Token.Text;
                    this.lexer.Token.Position = opPos;
                    return this.ParsePrimary();
                }
                QueryNode expr = this.ParseUnary();
                if (opKind == QueryTokenKind.Minus)
                {
                    expr = new UnaryOperatorNode(UnaryOperatorKind.Negate, expr);
                }
                else
                {
                    expr = new UnaryOperatorNode(UnaryOperatorKind.Not, expr);
                }
                return expr;
            }
            return this.ParsePrimary();
        }

        private QueryNode ParsePrimary()
        {
            QueryNode expr = this.ParsePrimaryStart();
            while (true)
            {
                if (this.lexer.Token.Kind == QueryTokenKind.Dot)
                {
                    this.lexer.NextToken();
                    expr = this.ParseMemberAccess(expr);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private QueryNode ParseMemberAccess(QueryNode instance)
        {
            var errorPos = this.lexer.Token.Position;
            string id = this.GetIdentifier();
            this.lexer.NextToken();
            if (this.lexer.Token.Kind == QueryTokenKind.OpenParen)
            {
                return this.ParseFunction(id, errorPos);
            }
            else
            {
                return new MemberAccessNode(instance, id);
            }
        }

        private QueryNode ParseFunction(string functionName, int errorPos)
        {

            IList<QueryNode> args = null;

            if (this.lexer.Token.Kind == QueryTokenKind.OpenParen)
            {
                args = this.ParseArgumentList();

                this.ValidateFunction(functionName, args, errorPos);
            }
            else
            {
                // if it is a function it should begin with a '('
                this.ParseError("'(' expected.".FormatInvariant(errorPos), errorPos);
            }

            return new FunctionCallNode(functionName, args);
        }

        private void ValidateFunction(string functionName, IList<QueryNode> functionArgs, int errorPos)
        {
            // validate parameters
            switch (functionName)
            {
                case "day":
                case "month":
                case "year":
                case "hour":
                case "minute":
                case "second":
                case "floor":
                case "ceiling":
                case "round":
                case "tolower":
                case "toupper":
                case "length":
                case "trim":
                    this.ValidateFunctionParameters(functionName, functionArgs, 1);
                    break;
                case "substringof":
                case "startswith":
                case "endswith":
                case "concat":
                case "indexof":
                    this.ValidateFunctionParameters(functionName, functionArgs, 2);
                    break;
                case "replace":
                    this.ValidateFunctionParameters(functionName, functionArgs, 3);
                    break;
                case "substring":
                    if (functionArgs.Count != 2 && functionArgs.Count != 3)
                    {
                        this.ParseError("Function 'substring' requires 2 or 3 parameters.", errorPos);
                    }
                    break;
            }
        }

        private void ValidateFunctionParameters(string functionName, IList<QueryNode> args, int expectedArgCount)
        {
            if (args.Count != expectedArgCount)
            {
                var error = "Function '{0}' requires {1} parameter(s).".FormatInvariant(functionName, expectedArgCount);
                throw new MobileServiceODataException(error, this.lexer.Token.Position);
            }
        }

        private IList<QueryNode> ParseArgumentList()
        {
            this.ValidateToken(QueryTokenKind.OpenParen, () => "'(' expected.");
            this.lexer.NextToken();

            IList<QueryNode> args = this.lexer.Token.Kind != QueryTokenKind.CloseParen ? this.ParseArguments() : new List<QueryNode>();

            this.ValidateToken(QueryTokenKind.CloseParen, () => "')' or ',' expected.");
            this.lexer.NextToken();
            return args;
        }

        private IList<QueryNode> ParseArguments()
        {
            var args = new List<QueryNode>();
            while (true)
            {
                args.Add(this.ParseExpression());
                if (this.lexer.Token.Kind != QueryTokenKind.Comma)
                {
                    break;
                }
                this.lexer.NextToken();
            }
            return args;
        }

        private string GetIdentifier()
        {
            this.ValidateToken(QueryTokenKind.Identifier, () => "Expected identifier.");
            return this.lexer.Token.Text;
        }

        private QueryNode ParsePrimaryStart()
        {
            switch (this.lexer.Token.Kind)
            {
                case QueryTokenKind.Identifier:
                    return this.ParseIdentifier();
                case QueryTokenKind.StringLiteral:
                    return this.ParseStringLiteral();
                case QueryTokenKind.IntegerLiteral:
                    return this.ParseIntegerLiteral();
                case QueryTokenKind.RealLiteral:
                    return this.ParseRealLiteral();
                case QueryTokenKind.OpenParen:
                    return this.ParseParenExpression();
                default:
                    this.ParseError("Expression expected.", this.lexer.Token.Position);
                    return null;
            }
        }

        private QueryNode ParseIntegerLiteral()
        {
            this.ValidateToken(QueryTokenKind.IntegerLiteral, () => "Expected integer literal.");
            var text = this.lexer.Token.Text;

            long value;
            if (!Int64.TryParse(text, out value))
            {
                this.ParseError("The specified odata query has invalid real literal '{0}'.".FormatInvariant(text), this.lexer.Token.Position);
            }
            this.lexer.NextToken();
            if (this.lexer.Token.Text.ToUpper() == "L")
            {
                // to parse the OData 'L/l' correctly
                this.lexer.NextToken();
                return new ConstantNode(value);
            }
            return new ConstantNode(value);
        }

        private QueryNode ParseRealLiteral()
        {
            this.ValidateToken(QueryTokenKind.RealLiteral, () => "Expected real literal.");
            string text = this.lexer.Token.Text;

            char last = Char.ToUpper(text[text.Length - 1]);
            if (last == 'F' || last == 'M' || last == 'D')
            {
                // so terminating F/f, M/m, D/d have no effect.
                text = text.Substring(0, text.Length - 1);
            }

            object value = null;
            switch (last)
            {
                case 'M':
                    decimal mVal;
                    if (Decimal.TryParse(text, out mVal))
                    {
                        value = mVal;
                    }
                    break;
                case 'F':
                    float fVal;
                    if (Single.TryParse(text, out fVal))
                    {
                        value = fVal;
                    }
                    break;
                case 'D':
                default:
                    double dVal;
                    if (Double.TryParse(text, out dVal))
                    {
                        value = dVal;
                    }
                    break;
            }
            if (value == null)
            {
                this.ParseError("The specified odata query has invalid real literal '{0}'.".FormatInvariant(text), this.lexer.Token.Position);
            }

            this.lexer.NextToken();
            return new ConstantNode(value);
        }

        private QueryNode ParseParenExpression()
        {
            this.ValidateToken(QueryTokenKind.OpenParen, () => "'(' expected.");
            this.lexer.NextToken();
            QueryNode e = this.ParseExpression();
            this.ValidateToken(QueryTokenKind.CloseParen, () => "')' or operator expected");
            this.lexer.NextToken();
            return e;
        }

        private QueryNode ParseIdentifier()
        {
            this.ValidateToken(QueryTokenKind.Identifier, () => "Expected identifier.");

            QueryNode value;
            if (keywords.TryGetValue(this.lexer.Token.Text, out value))
            {
                // type construction has the format of type'value' e.g. datetime'2001-04-01T00:00:00Z'
                // therefore if the next character is a single quote then we try to 
                // interpret this as type construction else its a normal member access
                if (value == null && this.lexer.CurrentChar == '\'')
                {
                    return this.ParseTypeConstruction();
                }
                else if (value != null) // this is a constant
                {
                    this.lexer.NextToken();
                    return value;
                }
            }

            return this.ParseMemberAccess(null);
        }

        private ConstantNode ParseTypeConstruction()
        {
            var typeIdentifier = this.lexer.Token.Text;
            var errorPos = this.lexer.Token.Position;
            this.lexer.NextToken();
            ConstantNode typeExpression = null;

            if (this.lexer.Token.Kind == QueryTokenKind.StringLiteral)
            {
                errorPos = this.lexer.Token.Position;
                ConstantNode stringExpr = this.ParseStringLiteral();
                string literalValue = stringExpr.Value.ToString();

                try
                {
                    if (typeIdentifier == "datetime")
                    {
                        var date = DateTime.Parse(literalValue);
                        typeExpression = new ConstantNode(date);
                    }
                    else if (typeIdentifier == "datetimeoffset")
                    {
                        var date = DateTimeOffset.Parse(literalValue);
                        typeExpression = new ConstantNode(date);
                    }
                    else if (typeIdentifier == "guid")
                    {
                        var guid = Guid.Parse(literalValue);
                        typeExpression = new ConstantNode(guid);
                    }
                }
                catch (Exception ex)
                {
                    this.ParseError(ex.Message, errorPos);
                }
            }

            if (typeExpression == null)
            {
                this.ParseError("The specified odata query has invalid '{0}' type creation expression.".FormatInvariant(typeIdentifier), errorPos);
            }

            return typeExpression;
        }

        private ConstantNode ParseStringLiteral()
        {
            this.ValidateToken(QueryTokenKind.StringLiteral, () => "Expected string literal.");

            char quote = this.lexer.Token.Text[0];
            // Unwrap string (remove surrounding quotes) 
            string value = this.lexer.Token.Text.Substring(1, this.lexer.Token.Text.Length - 2);
            // unwrap escaped quotes.
            value = value.Replace("''", "'");

            this.lexer.NextToken();
            return new ConstantNode(value);
        }

        public static QueryNode ParseFilter(string filter)
        {
            var parser = new ODataExpressionParser(filter);
            return parser.ParseFilter();
        }

        public static IList<OrderByNode> ParseOrderBy(string orderBy)
        {
            var parser = new ODataExpressionParser(orderBy);
            return parser.ParseOrderBy();
        }

        private void ParseError(string message, int errorPos)
        {
            throw new MobileServiceODataException(message, errorPos);
        }

        private bool TokenIdentifierIs(string id)
        {
            return this.lexer.Token.Kind == QueryTokenKind.Identifier && this.lexer.Token.Text == id;
        }

        private void ValidateToken(QueryTokenKind tokenKind, Func<string> errorString)
        {
            if (this.lexer.Token.Kind != tokenKind)
            {
                this.ParseError(errorString(), this.lexer.Token.Position);
            }
        }
    }
}
