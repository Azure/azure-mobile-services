// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal sealed class ODataExpressionLexer
    {
        private string text;
        private int textLen;
        private int textPos;
        private char ch;

        public QueryToken Token {get; private set; }

        public ODataExpressionLexer(string expression)
        {
            this.text = expression;
            this.textLen = this.text.Length;
            this.Token = new QueryToken();
            this.SetTextPos(0);
            this.NextToken();
        }

        public QueryToken NextToken()
        {
            while (Char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }

            QueryTokenKind t = QueryTokenKind.Unknown;
            int tokenPos = this.textPos;
            switch (this.ch)
            {
                case '(':
                    this.NextChar();
                    t = QueryTokenKind.OpenParen;
                    break;
                case ')':
                    this.NextChar();
                    t = QueryTokenKind.CloseParen;
                    break;
                case ',':
                    this.NextChar();
                    t = QueryTokenKind.Comma;
                    break;
                case '-':
                    this.NextChar();
                    t = QueryTokenKind.Minus;
                    break;
                case '/':
                    this.NextChar();
                    t = QueryTokenKind.Dot;
                    break;
                case '\'':
                    char quote = this.ch;
                    do
                    {
                        this.AdvanceToNextOccuranceOf(quote);
                        if (this.textPos == this.textLen)
                        {
                            this.ParseError(Resources.ODataExpressionParser_UnterminatedStringLiteral, this.textPos);
                        }
                        this.NextChar();
                    }
                    while (this.ch == quote);
                    t = QueryTokenKind.StringLiteral;
                    break;
                default:
                    if (this.IsIdentifierStart(this.ch) || this.ch == '@' || this.ch == '_')
                    {
                        do
                        {
                            this.NextChar();
                        }
                        while (this.IsIdentifierPart(this.ch) || this.ch == '_');
                        t = QueryTokenKind.Identifier;
                        break;
                    }
                    if (Char.IsDigit(this.ch))
                    {
                        t = QueryTokenKind.IntegerLiteral;
                        do
                        {
                            this.NextChar();
                        }
                        while (Char.IsDigit(this.ch));
                        if (this.ch == '.')
                        {
                            t = QueryTokenKind.RealLiteral;
                            this.NextChar();
                            this.ValidateDigit();
                            do
                            {
                                this.NextChar();
                            }
                            while (Char.IsDigit(this.ch));
                        }
                        if (this.ch == 'E' || this.ch == 'e')
                        {
                            t = QueryTokenKind.RealLiteral;
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
                            while (Char.IsDigit(this.ch));
                        }
                        if (this.ch == 'F' || this.ch == 'f' || this.ch == 'M' || this.ch == 'm' || this.ch == 'D' || this.ch == 'd')
                        {
                            t = QueryTokenKind.RealLiteral;
                            this.NextChar();
                        }
                        break;
                    }
                    if (this.textPos == this.textLen)
                    {
                        t = QueryTokenKind.End;
                        break;
                    }
                    this.ParseError(Resources.ODataExpressionParser_SyntaxError, this.textPos);
                    break;
            }
            this.Token.Kind = t;
            this.Token.Text = this.text.Substring(tokenPos, this.textPos - tokenPos);
            this.Token.Position = tokenPos;

            this.ReClassifyToken();

            return this.Token;
        }

        private void ValidateDigit()
        {
            if (!Char.IsDigit(this.ch)) {
                this.ParseError(Resources.ODataExpressionParser_DigitExpected, this.textPos);
            }
        }

        private void ReClassifyToken() {
            if (Token.Kind == QueryTokenKind.Identifier) {
                if (this.Token.Text == "or") {
                    this.Token.Kind = QueryTokenKind.Or;
                }
                else if (this.Token.Text == "add") {
                    this.Token.Kind = QueryTokenKind.Add;
                }
                else if (this.Token.Text == "and") {
                    this.Token.Kind = QueryTokenKind.And;
                }
                else if (this.Token.Text == "div") {
                    this.Token.Kind = QueryTokenKind.Divide;
                }
                else if (this.Token.Text == "sub") {
                    this.Token.Kind = QueryTokenKind.Sub;
                }
                else if (this.Token.Text == "mul") {
                    this.Token.Kind = QueryTokenKind.Multiply;
                }
                else if (this.Token.Text == "mod") {
                    this.Token.Kind = QueryTokenKind.Modulo;
                }
                else if (this.Token.Text == "ne") {
                    this.Token.Kind = QueryTokenKind.NotEqual;
                }
                else if (this.Token.Text == "not") {
                    this.Token.Kind = QueryTokenKind.Not;
                }
                else if (this.Token.Text == "le") {
                    this.Token.Kind = QueryTokenKind.LessThanEqual;
                }
                else if (this.Token.Text == "lt") {
                    this.Token.Kind = QueryTokenKind.LessThan;
                }
                else if (this.Token.Text == "eq") {
                    this.Token.Kind = QueryTokenKind.Equal;
                }
                else if (this.Token.Text == "ge") {
                    this.Token.Kind = QueryTokenKind.GreaterThanEqual;
                }
                else if (this.Token.Text == "gt") {
                    this.Token.Kind = QueryTokenKind.GreaterThan;
                }
            }
        }

        private bool IsIdentifierStart (char ch) 
        {
            return Char.IsLetter(ch);
        }

        private bool IsIdentifierPart (char ch) 
        {
            bool result = this.IsIdentifierStart(ch) || Char.IsDigit(ch) || (ch == '_' || ch == '-');
            return result;
        }

        private void AdvanceToNextOccuranceOf(char endingValue)
        {
            this.NextChar();
            while (this.textPos < this.textLen && this.ch != endingValue)
            {
                this.NextChar();
            }
        }

        private void NextChar()
        {
            if (this.textPos < this.textLen)
            {
                this.textPos++;
            }
            this.ch = (this.textPos < this.textLen) ? this.text[this.textPos] : '\0';
        }

        private void SetTextPos(int pos)
        {
            this.textPos = pos;
            this.ch = (this.textPos < this.textLen) ? this.text[this.textPos] : '\0';
        }               

        private void ParseError(string message, int errorPos)
        {
            throw new MobileServiceODataException(message, errorPos);
        }
    }
}
