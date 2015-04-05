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
            this.Type = Type;
        }

        internal TokenType Type;
    }
    internal class MathTokenNumber<T> : MathToken where T : struct, IConvertible
    {
        internal MathTokenNumber(T Value)
            : base(MathToken.TokenType.Number)
        {
            this.Value = Value;
        }

        internal T Value;
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
            this.Operation = Operation;
        }

        internal OperationType Operation;
    }
    internal class MathTokenizer<T> where T : struct, IConvertible
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

            char decimalPoint = CultureInfo.NumberFormat.NumberDecimalSeparator[0];

            char c = '\0', nc;
            int iPosStart = 0;
            bool bReadingNumber = false;
            bool bReadingAlphabeit = false;
            bool bReadingOperation = false;

            nc = _CurrentReadPosition == _InputString.Length ? '\0' : _InputString[_CurrentReadPosition]; ;
            while (nc != '\0' && null == token)
            {
                c = nc;
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
                        if (typeof(T) == typeof(Double))
                        {
                            token = new MathTokenNumber<T>((T)(object)Double.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                        }
                        else if (typeof(T) == typeof(Single))
                        {
                            token = new MathTokenNumber<T>((T)(object)Single.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                        }
                        else if (typeof(T) == typeof(Decimal))
                        {
                            token = new MathTokenNumber<T>((T)(object)Decimal.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                        }
                        else if (typeof(T) == typeof(Int32))
                        {
                            token = new MathTokenNumber<T>((T)(object)Int32.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                        }
                        else if (typeof(T) == typeof(Int64))
                        {
                            token = new MathTokenNumber<T>((T)(object)Int64.Parse(_InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart), NumberStyles.AllowDecimalPoint, CultureInfo));
                        }
                        else
                        {
                            throw new NotSupportedException("The supplied type cannot be handled");
                        }
                    }
                }
                else if (!bReadingOperation && VarDictionary != null && ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    if (!bReadingAlphabeit)
                    {
                        iPosStart = _CurrentReadPosition - 1;
                        bReadingAlphabeit = true;
                    }
                    if (!((nc >= 'a' && nc <= 'z') || (nc >= 'A' && nc <= 'Z')))
                    {
                        string key = _InputString.Substring(iPosStart, _CurrentReadPosition - iPosStart);
                        T value = default(T);
                        if (VarDictionary.TryGetValue(key, out value) || !FailOnMissingVar)
                        {
                            token = new MathTokenNumber<T>(value);
                        }
                        else
                        {
                            Debug.WriteLine("MathTokenizer: Missing variable \"" + key + "\"");
                            return null;
                        }
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
                        iPosStart = _CurrentReadPosition - 1;
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

        public CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        public Dictionary<string, T> VarDictionary = null;
        public bool FailOnMissingVar = false;
    }

    public static class MathExpressionParser<T> where T : struct, IConvertible
    {
        public static T ParseSimpleExpression(string expression)
        {
            bool success;
            return ParseSimpleExpression(expression, null, null, out success);
        }
        public static T ParseSimpleExpression(string expression, CultureInfo decimalSpecifier)
        {
            bool success;
            return ParseSimpleExpression(expression, decimalSpecifier, null, out success);
        }
        public static T ParseSimpleExpression(string expression, out bool success)
        {
            return ParseSimpleExpression(expression, null, null, out success);
        }
        public static T ParseSimpleExpression(string expression, CultureInfo decimalSpecifier, out bool success)
        {
            return ParseSimpleExpression(expression, decimalSpecifier, null, out success);
        }
        public static T ParseSimpleExpression(string expression, CultureInfo decimalSpecifier, Dictionary<string, T> varDictionary, out bool success)
        {
            return ParseSimpleExpression(expression, decimalSpecifier, varDictionary, false, out success);
        }
        public static T ParseSimpleExpression(string expression, CultureInfo decimalSpecifier, Dictionary<string, T> varDictionary, bool failOnMissingVar, out bool success)
        {
            MathTokenizer<T> tokenizer = new MathTokenizer<T>(expression);
            if (decimalSpecifier != null)
            {
                tokenizer.CultureInfo = decimalSpecifier;
            }
            if (varDictionary != null)
            {
                tokenizer.VarDictionary = varDictionary;
            }
            tokenizer.FailOnMissingVar = failOnMissingVar;

            T value = ParseSimpleExpression_InnerExpr(tokenizer, out success);
            if (success)
            {
                MathToken token = tokenizer.GetNextToken();
                if (null != token && token.Type == MathToken.TokenType.End)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            else
            { // Error inside ParseSimpleExpression_InnerExpr
            }
            return value;
        }
        private static T ParseSimpleExpression_InnerExpr(MathTokenizer<T> tokenizer, out bool success)
        {
            dynamic component1 = ParseSimpleExpression_InnerFactor(tokenizer, out success);

            if (success)
            {
                T component2;
                MathToken token = tokenizer.GetNextToken();
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                while (null != tokenOperation && tokenOperation.Type == MathToken.TokenType.Operation)
                {
                    if (tokenOperation.Operation == MathTokenOperation.OperationType.Add ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Subtract)
                    {
                        component2 = ParseSimpleExpression_InnerFactor(tokenizer, out success);
                        if (!success) break;

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
                    token = tokenizer.GetNextToken();
                    tokenOperation = token as MathTokenOperation;
                }
                if (null == token)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Token expected");
                    success = false;
                }
                else tokenizer.Revert();
            }

            return component1;
        }
        private static T ParseSimpleExpression_InnerFactor(MathTokenizer<T> tokenizer, out bool success)
        {
            dynamic factor1 = ParseSimpleExpression_InnerNumber(tokenizer, out success);

            if (success)
            {
                T factor2;
                MathToken token = tokenizer.GetNextToken();
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                while (null != tokenOperation && tokenOperation.Type == MathToken.TokenType.Operation)
                {
                    if (tokenOperation.Operation == MathTokenOperation.OperationType.Multiply ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Divide)
                    {
                        factor2 = ParseSimpleExpression_InnerNumber(tokenizer, out success);
                        if (!success) break;

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
                    token = tokenizer.GetNextToken();
                    tokenOperation = token as MathTokenOperation;
                }
                if (null == token)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Token expected");
                    success = false;
                }
                else tokenizer.Revert();
            }

            return factor1;
        }
        private static T ParseSimpleExpression_InnerNumber(MathTokenizer<T> tokenizer, out bool success)
        {
            MathToken token = tokenizer.GetNextToken();

            success = false;

            if (null == token)
            {
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: No token available");
                return default(T);
            }

            if (token.Type == MathToken.TokenType.LParen)
            {
                T value = ParseSimpleExpression_InnerExpr(tokenizer, out success);
                if (success)
                {
                    MathToken lParenExpected = tokenizer.GetNextToken();
                    if (null == lParenExpected || lParenExpected.Type != MathToken.TokenType.RParen)
                    {
                        Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Unbalanced parenthesis");
                    }
                    else success = true;
                }
                else
                {// Error inside ParseSimpleExpression_InnerExpr
                }
                return value;
            }
            else if (token.Type == MathToken.TokenType.Number)
            {
                MathTokenNumber<T> numberToken = token as MathTokenNumber<T>;
                if (null == numberToken)
                {
                    Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                    return default(T);
                }
                else
                {
                    success = true;
                }
                return numberToken.Value;
            }
            else if (token.Type == MathToken.TokenType.Operation)
            {
                MathTokenOperation tokenOperation = token as MathTokenOperation;
                if (null != tokenOperation && (tokenOperation.Operation == MathTokenOperation.OperationType.Add ||
                        tokenOperation.Operation == MathTokenOperation.OperationType.Subtract))
                {
                    tokenizer.Revert();
                    return default(T);
                }
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                return default(T);
            }
            else
            {
                Debug.WriteLine("MathExpressionParser:ParseSimpleExpression: Not a number");
                return default(T);
            }
        }
    };
}
