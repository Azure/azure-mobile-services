using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class ODataFilterParser
    {
        private static readonly ODataConstantExpression trueLiteral = new ODataConstantExpression(true);
        private static readonly ODataConstantExpression falseLiteral = new ODataConstantExpression(false);
        private static readonly ODataConstantExpression nullLiteral = new ODataConstantExpression(null);

        private static char[] WhitespaceChars = new char[]
        {
	        ' ',
	        '\t',
	        '\n',
	        '\r'
        };

        private List<string> parameters = new List<string>
        {
	        "$it"
        };

        private string text;
        private int textPos;
        private int textLen;
        private char ch;
        private Token token;

        /// <summary>
        /// Use this class to parse an expression in the OData URI format.
        /// </summary>
        /// <remarks>
        /// Literals (non-normative "handy" reference - see spec for correct expression):
        /// Null            null
        /// Boolean         true | false
        /// Int32           (digit+)
        /// Int64           (digit+)(L|l)
        /// Decimal         (digit+ ['.' digit+])(M|m)
        /// Float           (digit+ ['.' digit+][e|E [+|-] digit+)(f|F)
        /// Double          (digit+ ['.' digit+][e|E [+|-] digit+)
        /// String          "'" .* "'"
        /// DateTime        datetime"'"dddd-dd-dd[T|' ']dd:mm[ss[.fffffff]]"'"
        /// DateTimeOffset  datetimeoffset"'"dddd-dd-dd[T|' ']dd:mm[ss[.fffffff]]-dd:mm"'"
        /// Time            time"'"dd:mm[ss[.fffffff]]"'"
        /// Binary          (binary|X)'digit*'
        /// GUID            guid'digit*'
        /// </remarks>
        public ODataFilterParser(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.text = expression;
            this.textLen = this.text.Length;
            this.SetTextPos(0);
            this.NextToken();
        }

        private struct Token
        {
            public TokenId Id;
            public string Text;
            public int Position;
        }

        private enum TokenId
        {
            Unknown,
            End,
            Equal,
            Identifier,
            NullLiteral,
            BooleanLiteral,
            StringLiteral,
            IntegerLiteral,
            Int64Literal,
            SingleLiteral,
            DateTimeLiteral,
            DateTimeOffsetLiteral,
            TimeLiteral,
            DecimalLiteral,
            DoubleLiteral,
            GuidLiteral,
            BinaryLiteral,
            GeographyLiteral,
            GeometryLiteral,
            Exclamation,
            OpenParen,
            CloseParen,
            Comma,
            Colon,
            Minus,
            Slash,
            Question,
            Dot,
            Star,

            Not,
            Modulo,
            Multiply,
            Add,
            Sub,
            Divide,
            LessThan,
            GreaterThan,
            NotEqual,
            And,
            LessThanEqual,
            GreaterThanEqual,
            Or,
            All,
            Any,
        }

        public ODataExpression Parse()
        {
            int exprPos = this.token.Position;
            ODataExpression expr = this.ParseExpression();

            this.ValidateToken(TokenId.End, R.GetString("SyntaxError"));
            return expr;
        }

        // 'add','sub' operators
        private ODataExpression ParseAdditive()
        {
            ODataExpression left = this.ParseMultiplicative();
            while (this.token.Id == TokenId.Add || this.token.Id == TokenId.Sub)
            {
                Token prevToken = this.token;
                this.NextToken();
                ODataExpression right = this.ParseMultiplicative();
                switch (prevToken.Id)
                {
                    case TokenId.Add:
                        left = ODataExpression.Add(left, right);
                        break;
                    case TokenId.Sub:
                        left = ODataExpression.Subtract(left, right);
                        break;
                }
            }
            return left;
        }

        private ODataExpression ParseAll(ODataExpression parent)
        {
            return this.ParseAnyAll(parent, false);
        }

        private ODataExpression ParseAny(ODataExpression parent)
        {
            return this.ParseAnyAll(parent, true);
        }

        private ODataExpression ParseAnyAll(ODataExpression parent, bool isAny)
        {
            //any and all not supported
            throw new NotSupportedException();
        }

        private ODataExpression[] ParseArgumentList(TokenId splitToken)
        {
            this.ValidateToken(TokenId.OpenParen, R.GetString("OpenParenExpected"));
            this.NextToken();
            ODataExpression[] args = this.token.Id != TokenId.CloseParen ? this.ParseArguments(splitToken) : new ODataExpression[0];
            this.ValidateToken(TokenId.CloseParen, R.GetString("CloseParenOrCommaExpected"));
            this.NextToken();
            return args;
        }

        private ODataExpression[] ParseArguments(TokenId splitToken)
        {
            List<ODataExpression> argList = new List<ODataExpression>();
            while (true)
            {
                argList.Add(this.ParseExpression());
                if (this.token.Id != splitToken)
                {
                    break;
                }
                this.NextToken();
            }
            return argList.ToArray();
        }

        private ODataExpression ParseBinaryLiteral()
        {
            this.ValidateToken(TokenId.BinaryLiteral);
            string binaryString = this.token.Text;
            if (!TryRemoveLiteralPrefix("binary", ref binaryString) && !TryRemoveLiteralPrefix("X", ref binaryString))
            {
                throw ParseError("error");
            }
            if (!TryRemoveQuotes(ref binaryString))
            {
                throw ParseError("error");
            }
            if (binaryString.Length % 2 != 0)
            {
                // odd hex strings are not supported
                throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidHexLiteral")));
            }
            byte[] bytes = new byte[binaryString.Length / 2];
            for (int i = 0, j = 0; i < binaryString.Length; i += 2, j++)
            {
                string hexValue = binaryString.Substring(i, 2);
                bytes[j] = byte.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return new ODataConstantExpression(bytes);
        }

        private ODataExpression ParseBooleanLiteral()
        {
            this.ValidateToken(TokenId.BooleanLiteral);
            ODataExpression exp = null;
            if (this.token.Text.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                exp = trueLiteral;
            }
            else if (this.token.Text.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                exp = falseLiteral;
            }
            else
            {
                throw ParseError("Unrecognized literal {0}", this.token.Text);
            }
            this.NextToken();
            return exp;
        }

        // 'eq', 'ne', 'gt', 'gte', 'lt', 'lte' operators
        private ODataExpression ParseComparison()
        {
            ODataExpression left = this.ParseAdditive();
            while (IsComparison(this.token.Id))
            {
                Token op = this.token;
                this.NextToken();
                ODataExpression right = this.ParseAdditive();

                switch (op.Id)
                {
                    case TokenId.Equal:
                        left = ODataExpression.Equal(left, right);
                        break;
                    case TokenId.NotEqual:
                        left = ODataExpression.NotEqual(left, right);
                        break;
                    case TokenId.GreaterThan:
                        left = ODataExpression.GreaterThan(left, right);
                        break;
                    case TokenId.GreaterThanEqual:
                        left = ODataExpression.GreaterThanOrEqual(left, right);
                        break;
                    case TokenId.LessThan:
                        left = ODataExpression.LessThan(left, right);
                        break;
                    case TokenId.LessThanEqual:
                        left = ODataExpression.LessThanOrEqual(left, right);
                        break;
                }
            }
            return left;
        }

        private ODataExpression ParseExpression()
        {
            ODataExpression expr = this.ParseLogicalOr();
            return expr;
        }

        // 'and' operator
        private ODataExpression ParseLogicalAnd()
        {
            ODataExpression left = this.ParseComparison();
            while (this.token.Id == TokenId.And)
            {
                this.NextToken();
                ODataExpression right = this.ParseComparison();
                left = ODataExpression.AndAlso(left, right);
            }
            return left;
        }

        // 'or' operator
        private ODataExpression ParseLogicalOr()
        {
            ODataExpression left = this.ParseLogicalAnd();
            while (this.token.Id == TokenId.Or)
            {
                this.NextToken();
                ODataExpression right = this.ParseLogicalAnd();
                left = ODataExpression.OrElse(left, right);
            }
            return left;
        }        

        // 'mul', 'div', 'mod' operators
        private ODataExpression ParseMultiplicative()
        {
            ODataExpression left = this.ParseUnary();
            while (this.token.Id == TokenId.Multiply || this.token.Id == TokenId.Divide || this.token.Id == TokenId.Modulo)
            {
                Token prevToken = this.token;
                this.NextToken();
                ODataExpression right = this.ParseUnary();
                switch (prevToken.Id)
                {
                    case TokenId.Multiply:
                        left = ODataExpression.Multiply(left, right);
                        break;
                    case TokenId.Divide:
                        left = ODataExpression.Divide(left, right);
                        break;
                    case TokenId.Modulo:
                        left = ODataExpression.Modulo(left, right);
                        break;
                }
            }
            return left;
        }        

        // -, 'not' unary operators
        private ODataExpression ParseUnary()
        {
            if (this.token.Id == TokenId.Minus || this.token.Id == TokenId.Not)
            {
                Token currentToken = this.token;
                this.NextToken();
                if (currentToken.Id == TokenId.Minus && IsNumeric(this.token.Id))
                {
                    this.token.Text = "-" + this.token.Text;
                    this.token.Position = currentToken.Position;
                    return this.ParsePrimary();
                }
                ODataExpression expr = this.ParseUnary();
                if (currentToken.Id == TokenId.Minus)
                {
                    expr = ODataExpression.Negate(expr);
                }
                else
                {
                    expr = ODataExpression.Not(expr);
                }
                return expr;
            }
            return this.ParsePrimary();
        }

        private ODataExpression ParsePrimary()
        {
            Token expressionToken = this.PeekNextToken();
            ODataExpression expression;
            if (expressionToken.Id == TokenId.Dot)
            {
                expression = this.ParseTypeSegment(null);
            }
            else  if (expressionToken.Id == TokenId.Slash)
            {
                expression = this.ParseSegment(null);
            }
            else
            {
                expression = this.ParsePrimaryStart();
            }
            while (this.token.Id == TokenId.Slash)
            {
                this.NextToken();
                if (this.token.Id == TokenId.Any)
                {
                    expression = this.ParseAny(expression);
                }
                else if (this.token.Id == TokenId.All)
                {
                    expression = this.ParseAll(expression);
                }
                else if (this.PeekNextToken().Id == TokenId.Slash)
                {
                    expression = this.ParseSegment(expression);
                }
                else if (this.PeekNextToken().Id == TokenId.Dot)
                {
                    expression = this.ParseTypeSegment(expression);
                }
                else
                {
                    expression = this.ParseMemberAccess(expression);
                }
            }
            return expression;
        }

        private ODataExpression ParsePrimaryStart()
        {
            switch (this.token.Id)
            {
                case TokenId.Identifier:
                    return this.ParseIdentifier();
                case TokenId.OpenParen:
                    return this.ParseParenthesisExpression();
                case TokenId.Star:
                    return this.ParseStarMemberAccess(null);
                case TokenId.NullLiteral:
                    return this.ParseNullLiteral();
                case TokenId.BooleanLiteral:
                    return this.ParseBooleanLiteral();
                case TokenId.StringLiteral:
                    return this.ParseStringLiteral();
                case TokenId.IntegerLiteral:
                    return this.ParseIntegerLiteral();
                case TokenId.Int64Literal:
                    return this.ParseInt64Literal();
                case TokenId.SingleLiteral:
                    return this.ParseSingleLiteral();
                case TokenId.DateTimeLiteral:
                    return this.ParseDateTimeLiteral();
                case TokenId.DateTimeOffsetLiteral:
                    return this.ParseDateTimeOffsetLiteral();
                case TokenId.TimeLiteral:
                    return this.ParseTimeLiteral();
                case TokenId.DecimalLiteral:
                    return this.ParseDecimalLiteral();
                case TokenId.DoubleLiteral:
                    return this.ParseDoubleLiteral();
                case TokenId.GuidLiteral:
                    return this.ParseGuidLiteral();
                case TokenId.BinaryLiteral:
                    return this.ParseBinaryLiteral();
                case TokenId.GeographyLiteral:
                case TokenId.GeometryLiteral:
                    throw new NotSupportedException("Spatial types unsupported");
                case TokenId.Any:
                    throw new NotSupportedException("Any expressions unsupported");
                case TokenId.All:
                    throw new NotSupportedException("Any expressions unsupported");
                default:
                    throw ParseError(R.GetString("ExpressionExpected"));
            }
        }

        private ODataExpression ParseSegment(ODataExpression parent)
        {
            string identifier = this.GetIdentifier();
            this.NextToken();
            if (this.parameters.Contains(identifier) && parent == null)
            {
                return new ODataParameterExpression(identifier);
            }
            throw new NotSupportedException("Segment not supported");
        }

        private ODataExpression ParseNullLiteral()
        {
            this.ValidateToken(TokenId.NullLiteral);
            this.NextToken();
            return nullLiteral;
        }        

        private ODataExpression ParseStringLiteral()
        {
            this.ValidateToken(TokenId.StringLiteral);
            // Unwrap string (remove surrounding quotes)
            string s = this.token.Text;
            if (TryRemoveQuotes(ref s))
            {
                this.NextToken();
                return new ODataConstantExpression(s);
            }
            else
            {
                throw ParseError("error");
            }
        }

        private ODataExpression ParseIntegerLiteral()
        {
            this.ValidateToken(TokenId.IntegerLiteral);

            string text = this.token.Text;
            int value;
            if (!int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            {
                throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidIntegerLiteral"), text));
            }
            this.NextToken();
            return new ODataConstantExpression(value);
        }

        private ODataExpression ParseInt64Literal()
        {
            this.ValidateToken(TokenId.Int64Literal);

            string text = this.token.Text;
            if (TryRemoveLiteralSuffix("L", ref text) || TryRemoveLiteralSuffix("l", ref text))
            {
                long value;
                if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
                {
                    throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidIntegerLiteral"), text));
                }
                this.NextToken();
                return new ODataConstantExpression(value);
            }
            else
            {
                throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidIntegerLiteral"), text));
            }
        }

        private ODataExpression ParseSingleLiteral()
        {
            this.ValidateToken(TokenId.SingleLiteral);

            string text = this.token.Text;
            if (TryRemoveLiteralSuffix("F", ref text) || TryRemoveLiteralSuffix("f", ref text))
            {
                float value;
                if (!float.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value))
                {
                    throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
                }
                this.NextToken();
                return new ODataConstantExpression(value);
            }
            else
            {
                throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
            }
        }

        private ODataExpression ParseStarMemberAccess(ODataExpression instance)
        {
            throw new NotSupportedException("star unsupported");
        }

        private ODataExpression ParseDoubleLiteral()
        {
            this.ValidateToken(TokenId.DoubleLiteral);

            string text = this.token.Text;
            if (TryRemoveLiteralSuffix("D", ref text) || TryRemoveLiteralSuffix("d", ref text))
            {
                double value;
                if (!double.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value))
                {
                    throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
                }
                this.NextToken();
                return new ODataConstantExpression(value);
            }
            else
            {
                double value;
                if (!double.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value))
                {
                    throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
                }
                this.NextToken();
                return new ODataConstantExpression(value);
            }
        }

        private ODataExpression ParseDecimalLiteral()
        {
            this.ValidateToken(TokenId.DecimalLiteral);

            string text = this.token.Text;
            if (TryRemoveLiteralSuffix("M", ref text) || TryRemoveLiteralSuffix("m", ref text))
            {
                decimal value;
                if (!decimal.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value))
                {
                    throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
                }
                this.NextToken();
                return new ODataConstantExpression(value);
            }
            else
            {
                throw ParseError(string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidRealLiteral"), text));
            }
        }

        private ODataExpression ParseDateTimeLiteral()
        {
            this.ValidateToken(TokenId.DateTimeLiteral);
            string datetimeString = this.token.Text;
            if (!TryRemoveLiteralPrefix("datetime", ref datetimeString))
            {
                throw ParseError("error");
            }
            if (!TryRemoveQuotes(ref datetimeString))
            {
                throw ParseError("error");
            }
            DateTime dateTime = DateTime.Parse(datetimeString, CultureInfo.InvariantCulture);
            return new ODataConstantExpression(dateTime);
        }

        private ODataExpression ParseDateTimeOffsetLiteral()
        {
            this.ValidateToken(TokenId.DateTimeOffsetLiteral);
            string datetimeoffsetString = this.token.Text;
            if (!TryRemoveLiteralPrefix("datetimeoffset", ref datetimeoffsetString))
            {
                throw ParseError("error");
            }
            if (!TryRemoveQuotes(ref datetimeoffsetString))
            {
                throw ParseError("error");
            }
            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(datetimeoffsetString, CultureInfo.InvariantCulture);
            this.NextToken();
            return new ODataConstantExpression(dateTimeOffset);
        }

        private ODataExpression ParseTimeLiteral()
        {
            this.ValidateToken(TokenId.TimeLiteral);
            string timeString = this.token.Text;
            if (!TryRemoveLiteralPrefix("time", ref timeString))
            {
                throw ParseError("error");
            }
            if (!TryRemoveQuotes(ref timeString))
            {
                throw ParseError("error");
            }
            TimeSpan timespan = TimeSpan.Parse(timeString, CultureInfo.InvariantCulture);
            this.NextToken();
            return new ODataConstantExpression(timespan);
        }

        private ODataExpression ParseTypeSegment(ODataExpression parent)
        {
            throw new NotSupportedException("Segments unsupported");
        }

        private ODataExpression ParseGuidLiteral()
        {
            this.ValidateToken(TokenId.GuidLiteral);
            string guidString = this.token.Text;
            if (!TryRemoveLiteralPrefix("guid", ref guidString))
            {
                throw ParseError("error");
            }
            if (!TryRemoveQuotes(ref guidString))
            {
                throw ParseError("error");
            }
            Guid guid = Guid.Parse(guidString);
            this.NextToken();
            return new ODataConstantExpression(guid);
        }

        private ODataExpression ParseParenthesisExpression()
        {
            this.ValidateToken(TokenId.OpenParen, R.GetString("OpenParenExpected"));
            this.NextToken();
            ODataExpression e = this.ParseExpression();
            this.ValidateToken(TokenId.CloseParen, R.GetString("CloseParenOrOperatorExpected"));
            this.NextToken();
            return e;
        }

        private ODataExpression ParseIdentifier()
        {
            this.ValidateToken(TokenId.Identifier);
            if (this.PeekNextToken().Id == TokenId.OpenParen)
            {
                return this.ParseIdentifierAsFunction();
            }
            return this.ParseMemberAccess(null);
        }

        private ODataExpression ParseIdentifierAsFunction()
        {
            Token currentToken = this.token;
            this.NextToken();
            ODataExpression[] arguments = this.ParseArgumentList(TokenId.Comma);
            return new ODataFunctionCallExpression(currentToken.Text, arguments);
        }                

        //TODO
        private ODataExpression ParseMemberAccess(ODataExpression instance)
        {
            if (this.token.Text == "*")
            {
                return this.ParseStarMemberAccess(instance);
            }
            string identifier = this.GetIdentifier();
            if (instance == null && this.parameters.Contains(identifier))
            {
                this.NextToken();
                return new ODataParameterExpression(identifier);
            }
            this.NextToken();
            return new ODataMemberExpression(instance, identifier);
        }

        private void SetTextPos(int pos)
        {
            this.textPos = pos;
            this.ch = this.textPos < this.textLen ? this.text[this.textPos] : '\0';
        }

        private void NextChar()
        {
            if (this.textPos < this.textLen)
            {
                this.textPos++;
            }
            this.ch = this.textPos < this.textLen ? this.text[this.textPos] : '\0';
        }

        private Token NextToken()
        {
            Exception ex = null;
            this.token = this.NextTokenImplementation(out ex);
            if (ex != null)
            {
                throw ex;
            }
            return this.token;
        }

        private Token PeekNextToken()
        {
            Exception ex;
            //temp save old values
            int num = this.textPos;
            char c = this.ch;
            Token expressionToken = this.token;
            Token resultToken = this.NextTokenImplementation(out ex);
            //restore old values
            this.textPos = num;
            this.ch = c;
            this.token = expressionToken;
            if (ex != null)
            {
                throw ex;
            }
            return resultToken;
        }

        private Token NextTokenImplementation(out Exception error)
        {
            error = null;
            while (char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }
            TokenId t;
            char c = this.ch;
            int tokenPos = this.textPos;
            switch (this.ch)
            {
                case '\'':
                    char quote = this.ch;
                    do
                    {
                        this.NextChar();
                        while (this.textPos < this.textLen && this.ch != quote)
                        {
                            this.NextChar();
                        }
                        if (this.textPos == this.textLen)
                        {
                            error = ParseError(this.textPos, R.GetString("UnterminatedStringLiteral"));
                        }
                        this.NextChar();
                    }
                    while (this.ch == quote);
                    t = TokenId.StringLiteral;
                    break;
                case '(':
                    this.NextChar();
                    t = TokenId.OpenParen;
                    break;
                case ')':
                    this.NextChar();
                    t = TokenId.CloseParen;
                    break;
                case '*':
                    this.NextChar();
                    t = TokenId.Star;
                    break;
                case ',':
                    this.NextChar();
                    t = TokenId.Comma;
                    break;
                case '-':
                    bool hasNext = this.textPos + 1 < this.textLen;
                    if (hasNext && char.IsDigit(this.text[this.textPos + 1]))
                    {
                        this.NextChar();
                        t = this.ParseFromDigit();
                        if (IsNumeric(t))
                        {
                            break;
                        }
                        this.SetTextPos(tokenPos);
                    }
                    else if (hasNext && this.text[tokenPos + 1] == "INF"[0])
                    {
                        this.NextChar();
                        this.ParseFromIdentifier();
                        string text = this.text.Substring(tokenPos + 1, this.textPos - tokenPos - 1);
                        if (IsInfinityLiteralDouble(text))
                        {
                            t = TokenId.DoubleLiteral;
                            break;
                        }
                        if (IsInfinityLiteralSingle(text))
                        {
                            t = TokenId.SingleLiteral;
                            break;
                        }
                        this.SetTextPos(tokenPos);
                    }
                    this.NextChar();
                    t = TokenId.Minus;
                    break;
                case '.':
                    this.NextChar();
                    t = TokenId.Dot;
                    break;
                case '/':
                    this.NextChar();
                    t = TokenId.Dot;
                    break;
                case ':':
                    this.NextChar();
                    t = TokenId.Colon;
                    break;
                case '=':
                    this.NextChar();
                    t = TokenId.Equal;
                    break;
                case '?':
                    this.NextChar();
                    t = TokenId.Question;
                    break;
                default:
                    if (char.IsLetter(this.ch) || this.ch == '_' || this.ch == '$')
                    {
                        this.ParseFromIdentifier();
                        t = TokenId.Identifier;
                    }
                    else if (Char.IsDigit(this.ch))
                    {
                        t = this.ParseFromDigit();
                    }
                    else if (this.textPos == this.textLen)
                    {
                        t = TokenId.End;
                    }
                    else
                    {
                        error = ParseError(this.textPos, string.Format(CultureInfo.CurrentCulture, R.GetString("InvalidCharacter"), this.ch));
                        t = TokenId.Unknown;
                    }
                    break;
            }
            this.token.Id = t;
            this.token.Text = this.text.Substring(tokenPos, this.textPos - tokenPos);
            this.token.Position = tokenPos;

            this.ReclassifyToken(this.token);

            return this.token;
        }

        private void ReclassifyToken(Token token)
        {
            TokenId tokenId = this.token.Id;
            if (this.token.Id == TokenId.Identifier)
            {
                if (this.ch == '\'')
                {
                    string text = this.token.Text;
                    switch (text)
                    {
                        case "datetime":
                            tokenId = TokenId.DateTimeLiteral;
                            break;
                        case "datetimeoffset":
                            tokenId = TokenId.DateTimeOffsetLiteral;
                            break;
                        case "time":
                            tokenId = TokenId.TimeLiteral;
                            break;
                        case "guid":
                            tokenId = TokenId.GuidLiteral;
                            break;
                        case "binary":
                        case "X":
                            tokenId = TokenId.BinaryLiteral;
                            break;
                        case "geography":
                            tokenId = TokenId.GeographyLiteral;
                            break;
                        case "geometry":
                            tokenId = TokenId.GeometryLiteral;
                            break;
                    }

                    int position = this.token.Position;
                    do
                    {
                        this.NextChar();
                    }
                    while (this.ch != '\0' && this.ch != '\'');
                    if (this.ch == '\0')
                    {
                        throw ParseError(this.textPos, R.GetString("UnterminatedStringLiteral"));
                    }
                    this.NextChar();
                    this.token.Id = tokenId;
                    this.token.Text = this.text.Substring(position, this.textPos - position);
                }
                else
                {
                    switch (this.token.Text)
                    {
                        case "INF":
                        case "NaN":
                            this.token.Id = TokenId.DoubleLiteral;
                            break;
                        case "INFf":
                        case "INFF":
                        case "NaNf":
                        case "NaNF":
                            this.token.Id = TokenId.SingleLiteral;
                            break;
                        case "true":
                        case "false":
                            this.token.Id = TokenId.BooleanLiteral;
                            break;
                        case "null":
                            this.token.Id = TokenId.NullLiteral;
                            break;
                        case "and":
                            this.token.Id = TokenId.And;
                            break;
                        case "or":
                            this.token.Id = TokenId.Or;
                            break;
                        case "add":
                            this.token.Id = TokenId.Add;
                            break;
                        case "div":
                            this.token.Id = TokenId.Divide;
                            break;
                        case "sub":
                            this.token.Id = TokenId.Sub;
                            break;
                        case "mul":
                            this.token.Id = TokenId.Multiply;
                            break;
                        case "mod":
                            this.token.Id = TokenId.Modulo;
                            break;
                        case "ne":
                            this.token.Id = TokenId.NotEqual;
                            break;
                        case "not":
                            this.token.Id = TokenId.Not;
                            break;
                        case "le":
                            this.token.Id = TokenId.LessThanEqual;
                            break;
                        case "lt":
                            this.token.Id = TokenId.LessThan;
                            break;
                        case "eq":
                            this.token.Id = TokenId.Equal;
                            break;
                        case "ge":
                            this.token.Id = TokenId.GreaterThanEqual;
                            break;
                        case "gt":
                            this.token.Id = TokenId.GreaterThan;
                            break;
                        case "any":
                            this.token.Id = TokenId.Any;
                            break;
                        case "all":
                            this.token.Id = TokenId.All;
                            break;
                    }
                }
            }
        }

        private TokenId ParseFromDigit()
        {
            char startChar = this.ch;
            this.NextChar();
            TokenId result;
            if (startChar == '0' && this.ch == 'x' || this.ch == 'X')
            {
                result = TokenId.BinaryLiteral;
                do
                {
                    this.NextChar();
                }
                while (IsCharHexDigit(this.ch));
            }
            else
            {
                result = TokenId.IntegerLiteral;
                while (char.IsDigit(this.ch))
                {
                    this.NextChar();
                }
                if (this.ch == '.')
                {
                    result = TokenId.DoubleLiteral;
                    this.NextChar();
                    this.ValidateDigit();

                    do
                    {
                        this.NextChar();
                    }
                    while (char.IsDigit(this.ch));
                }
                switch (this.ch)
                {
                    case 'E':
                    case 'e':
                        result = TokenId.DoubleLiteral;
                        this.NextChar();
                        if (this.ch == '+' || this.ch == '-')
                        {
                            this.NextChar();
                        }
                        this.ValidateDigit();
                        do
                        {
                            this.NextChar();
                        }
                        while (char.IsDigit(this.ch));
                        break;
                    case 'M':
                    case 'm':
                        result = TokenId.DecimalLiteral;
                        this.NextChar();
                        break;
                    case 'd':
                    case 'D':
                        result = TokenId.DoubleLiteral;
                        this.NextChar();
                        break;
                    case 'L':
                    case 'l':
                        result = TokenId.Int64Literal;
                        this.NextChar();
                        break;
                    case 'f':
                    case 'F':
                        result = TokenId.SingleLiteral;
                        this.NextChar();
                        break;
                }
            }
            return result;
        }

        private void ParseFromIdentifier()
        {            
            do
            {
                this.NextChar();
            }
            while (Char.IsLetterOrDigit(this.ch) || this.ch == '_' || this.ch == '$');
        } 

        private string GetIdentifier()
        {
            this.ValidateToken(TokenId.Identifier, R.GetString("IdentifierExpected"));
            return this.token.Text;
        }

        private void ValidateDigit()
        {
            if (!Char.IsDigit(this.ch))
            {
                throw ParseError(this.textPos, R.GetString("DigitExpected"));
            }
        }

        private void ValidateToken(TokenId t, string errorMessage)
        {
            if (this.token.Id != t)
            {
                throw ParseError(errorMessage);
            }
        }

        private void ValidateToken(TokenId t)
        {
            if (this.token.Id != t)
            {
                throw ParseError(R.GetString("SyntaxError"));
            }
        }

        private Exception ParseError(string format, params object[] args)
        {
            return ParseError(this.token.Position, format, args);
        }

        private static Exception ParseError(int pos, string format, params object[] args)
        {
            return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
        }

        #region Helpers

        private static bool TryRemoveLiteralPrefix(string prefix, ref string text)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text.Remove(0, prefix.Length);
                return true;
            }
            return false;
        }

        private static bool TryRemoveLiteralSuffix(string suffix, ref string text)
        {
            text = text.Trim(WhitespaceChars);
            if (text.Length <= suffix.Length || !text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            text = text.Substring(0, text.Length - suffix.Length);
            return true;
        }

        private static bool TryRemoveQuotes(ref string text)
        {
            if (text.Length < 2)
            {
                return false;
            }
            char c = text[0];
            if (c != '\'' || text[text.Length - 1] != c)
            {
                return false;
            }
            text = text.Substring(1, text.Length - 2).Replace("''", "'");
            return true;
        }

        private static bool IsCharHexDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private static bool IsNumeric(TokenId id)
        {
            return id == TokenId.IntegerLiteral || id == TokenId.DecimalLiteral || id == TokenId.DoubleLiteral || 
                id == TokenId.Int64Literal || id == TokenId.SingleLiteral;
        }

        private static bool IsInfinityLiteralDouble(string text)
        {
            return string.CompareOrdinal(text, 0, "INF", 0, text.Length) == 0;
        }

        private static bool IsInfinityLiteralSingle(string text)
        {
            return text.Length == 4 && (text[3] == 'f' || text[3] == 'F') && string.CompareOrdinal(text, 0, "INF", 0, 3) == 0;
        }

        private static bool IsComparison(TokenId id)
        {
            return id == TokenId.Equal || id == TokenId.NotEqual || id == TokenId.GreaterThan ||
                id == TokenId.GreaterThanEqual || id == TokenId.LessThan ||
                id == TokenId.LessThanEqual;
        }

        #endregion
    }
}
