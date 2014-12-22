using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace dg.Utilities.Maths
{
	internal class MathToken
    {
        internal enum TokenType
        {
            Operation,
            Number,
            LParen,
            RParen,
            End
        }

        internal MathToken(TokenType Type)
        {
            _Type = Type;
        }

        private TokenType _Type;
        virtual internal TokenType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
	}
    internal class MathTokenNumber : MathToken
    {
        internal MathTokenNumber(decimal Value) : base(MathToken.TokenType.Number)
        {
            _Value = Value;
        }

        private decimal _Value;
        virtual internal decimal Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
	}
    internal class MathTokenOperation : MathToken
	{
        internal enum OperationType
        {
		    Add,
		    Subtract,
		    Multiply,
		    Divide
        }

        internal MathTokenOperation(OperationType Operation)
            : base(MathToken.TokenType.Operation)
        {
            _Operation = Operation;
        }

        private OperationType _Operation;
        virtual internal OperationType Operation
        {
            get { return _Operation; }
            set { _Operation = value; }
        }
	}
    internal class MathTokenizer
    {
        private string _InputString;
        private MathToken _PreviousToken;
        private int _CurrentReadPosition;
        private bool _ReturnPreviousToken;

        static char[] lTrimCharSet = new char[] { ' ', '\t', '\r', '\n' };

        internal MathTokenizer(string Input)
        {
            _CurrentReadPosition = 0;
            _ReturnPreviousToken = false;
            _PreviousToken = null;

            _InputString = null;
            if (Input == null)
            {
                _InputString = string.Empty;
            }
            else
            {
                _InputString = Input.TrimStart(lTrimCharSet);
            }
        }
        internal MathToken GetNextToken()
        {
		    if (_ReturnPreviousToken)
		    {
			    _ReturnPreviousToken = false;
			    return _PreviousToken;
		    }

            MathToken token = null;

            char decimalPoint = _CultureInfo.NumberFormat.NumberDecimalSeparator[0];

            char c = '\0', nc;
		    int iPosStart = 0;
            bool bReadingNumber = false;
            bool bReadingAlphabeit = false;
		    bool bReadingOperation = false;

            nc = _CurrentReadPosition == _InputString.Length ? '\0' : _InputString[_CurrentReadPosition]; ;
		    while (nc != '\0' && null==token)
		    {
			    c=nc;
                nc = (++_CurrentReadPosition) == _InputString.Length ? '\0' : _InputString[_CurrentReadPosition];

                if (!bReadingOperation && ((c >= '0' && c <= '9') || c == decimalPoint))
                {
                    if (!bReadingNumber)
                    {
                        iPosStart = _CurrentReadPosition - 1;
                        bReadingNumber = true;
                    }
                    if (!((nc >= '0' && nc <= '9') || nc == decimalPoint))
                    {
                        token = new MathTokenNumber(Decimal.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                    }
                }
                else if (!bReadingOperation && _AlphabeitValueDictionary != null && ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    if (!bReadingAlphabeit)
                    {
                        iPosStart = _CurrentReadPosition - 1;
                        bReadingAlphabeit = true;
                    }
                    if (!((nc >= 'a' && nc <= 'z') || (nc >= 'A' && nc <= 'Z')))
                    {
                        string key = _InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart);
                        decimal value = 0;
                        _AlphabeitValueDictionary.TryGetValue(key, out value);
                        token = new MathTokenNumber(value);
                    }
			    }
			    else if (c == '(')
			    {
				    token = new MathToken(MathToken.TokenType.LParen);
			    }
			    else if (c == ')')
			    {
				    token = new MathToken(MathToken.TokenType.RParen);
			    }
			    else if (c != ' ')
			    {
				    if (!bReadingOperation) 
				    {
					    iPosStart = _CurrentReadPosition-1;
					    bReadingOperation = true;
				    }
				    if (c == '+')
				    {
					    token = new MathTokenOperation(MathTokenOperation.OperationType.Add);
				    }
				    else if (c == '-')
				    {
					    token = new MathTokenOperation(MathTokenOperation.OperationType.Subtract);
				    }
				    else if (c == '*')
				    {
					    token = new MathTokenOperation(MathTokenOperation.OperationType.Multiply);
				    }
				    else if (c == '/')
				    {
					    token = new MathTokenOperation(MathTokenOperation.OperationType.Divide);
				    }
				    else
				    {
                        Debug.WriteLine("MathTokenizer: Unexpected character");
					    break;
                    }
                }
            }
            if (null == token && c == '\0')
            {
                token = new MathToken(MathToken.TokenType.End);
            }

            if (null == token)
            {
                Debug.WriteLine("MathTokenizer: End expected");
                return null;
            }

            _PreviousToken = token;

            return token;
        }
        internal void Revert()
        {
            _ReturnPreviousToken = true;
        }

        private CultureInfo _CultureInfo = CultureInfo.InvariantCulture;
        public System.Globalization.CultureInfo CultureInfo
        {
            get { return _CultureInfo; }
            set { _CultureInfo = value; }
        }

        private Dictionary<string, decimal> _AlphabeitValueDictionary = null;
        public Dictionary<string, decimal> AlphabeitValueDictionary
        {
            get { return _AlphabeitValueDictionary; }
            set { _AlphabeitValueDictionary = value; }
        }
    }

    public static class MathExpressionParser
    {
        public static decimal ParseSimpleExpression(string Expression)
        {
            bool Success;
            return ParseSimpleExpression(Expression, null, null, out Success);
        }
        public static decimal ParseSimpleExpression(string Expression, CultureInfo DecimalSpecifier)
        {
            bool Success;
            return ParseSimpleExpression(Expression, DecimalSpecifier, null, out Success);
        }
        public static decimal ParseSimpleExpression(string Expression, out bool Success)
        {
            return ParseSimpleExpression(Expression, null, null, out Success);
        }
        public static decimal ParseSimpleExpression(string Expression, CultureInfo DecimalSpecifier, out bool Success)
        {
            return ParseSimpleExpression(Expression, DecimalSpecifier, null, out Success);
        }
        public static decimal ParseSimpleExpression(string Expression, CultureInfo DecimalSpecifier, Dictionary<string, decimal> AlphabeitValueDictionary, out bool Success)
        {
            MathTokenizer tokenizer = new MathTokenizer(Expression);
            if (DecimalSpecifier != null)
            {
                tokenizer.CultureInfo = DecimalSpecifier;
            }
            if (AlphabeitValueDictionary != null)
            {
                tokenizer.AlphabeitValueDictionary = AlphabeitValueDictionary;
            }

            decimal value = ParseSimpleExpression_InnerExpr(tokenizer, out Success);
            if (Success)
            {
                MathToken token = tokenizer.GetNextToken();
                if (null != token && token.Type == MathToken.TokenType.End)
                {
                    Success = true;
                }
                else
                {
                    Success = false;
                }
            }
            else
            { // Error inside ParseSimpleExpression_InnerExpr
            }
            return value;
        }
        private static decimal ParseSimpleExpression_InnerExpr(MathTokenizer Tokenizer, out bool Success)
        {
            decimal component1 = ParseSimpleExpression_InnerFactor(Tokenizer, out Success);

            if (Success)
            {
                decimal component2;
                MathToken token = Tokenizer.GetNextToken();
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                while (null != tokenOperation && tokenOperation.Type == MathToken.TokenType.Operation)
                {
                    if (tokenOperation.Operation == MathTokenOperation.OperationType.Add ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Subtract)
                    {
                        component2 = ParseSimpleExpression_InnerFactor(Tokenizer, out Success);
                        if (!Success) break;

                        if (tokenOperation.Operation == MathTokenOperation.OperationType.Add)
                        {
                            component1 += component2;
                        }
                        else
                        {
                            component1 -= component2;
                        }
                    }
                    else break; // Not an Add or Subtract
                    token = Tokenizer.GetNextToken();
                    tokenOperation = token as MathTokenOperation;
                }
                if (null == token)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Token expected");
                    Success = false;
                }
                else Tokenizer.Revert();
            }

            return component1;
        }
        private static decimal ParseSimpleExpression_InnerFactor(MathTokenizer Tokenizer, out bool Success)
        {
            decimal factor1 = ParseSimpleExpression_InnerNumber(Tokenizer, out Success);

            if (Success)
            {
                decimal factor2;
                MathToken token = Tokenizer.GetNextToken();
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                while (null != tokenOperation && tokenOperation.Type == MathToken.TokenType.Operation)
                {
                    if (tokenOperation.Operation == MathTokenOperation.OperationType.Multiply ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Divide)
                    {
                        factor2 = ParseSimpleExpression_InnerNumber(Tokenizer, out Success);
                        if (!Success) break;

                        if (tokenOperation.Operation == MathTokenOperation.OperationType.Multiply)
                        {
                            factor1 *= factor2;
                        }
                        else
                        {
                            factor1 /= factor2;
                        }
                    }
                    else break; // Not an Add or Subtract
                    token = Tokenizer.GetNextToken();
                    tokenOperation = token as MathTokenOperation;
                }
                if (null == token)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Token expected");
                    Success = false;
                }
                else Tokenizer.Revert();
            }

            return factor1;
        }
        private static decimal ParseSimpleExpression_InnerNumber(MathTokenizer Tokenizer, out bool Success)
        {
            MathToken token = Tokenizer.GetNextToken();

            Success = false;

            if (null == token)
            {
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: No token available");
                return 0;
            }

            if (token.Type == MathToken.TokenType.LParen)
            {
                decimal value = ParseSimpleExpression_InnerExpr(Tokenizer, out Success);
                if (Success)
                {
                    MathToken lParenExpected = Tokenizer.GetNextToken();
                    if (null == lParenExpected || lParenExpected.Type != MathToken.TokenType.RParen)
                    {
                        Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Unbalanced parenthesis");
                    }
                    else Success = true;
                }
                else
                {// Error inside ParseSimpleExpression_InnerExpr
                }
                return value;
            }
            else if (token.Type == MathToken.TokenType.Number)
            {
                MathTokenNumber numberToken = token as MathTokenNumber;
                if (null == numberToken)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                    return 0;
                }
                return numberToken.Value;
            }
            else if (token.Type == MathToken.TokenType.Operation)
            {
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                if (null != tokenOperation && (tokenOperation.Operation == MathTokenOperation.OperationType.Add ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Subtract))
                {
                    Tokenizer.Revert();
                    return 0;
                }
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                return 0;
            }
            else
            {
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                return 0;
            }
        }
    };
}
