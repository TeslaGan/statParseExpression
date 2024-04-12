using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StatExpressionParser
{
    public class ExpressionParser
    {
        internal enum TermType : byte
        {
            None,
            Operator,
            AlphaNumeric,
            Constant,
            BoolConstant,
            Group,
            Separator
        }

        private List<IOperator> _allOperators;
        private char[] _operatorChars;
        private List<IFunction> _allFunctions;
        private Dictionary<string, ParameterExpression> _allParameters;

        public ExpressionParser()
        {
            _allOperators = Assembly.GetAssembly(this.GetType()).GetTypes()
                .Where(p => typeof(IOperator).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IOperator>().ToList();
            _operatorChars = _allOperators.SelectMany(x => x.Name.ToCharArray()).Distinct().ToArray();
            _allFunctions = Assembly.GetAssembly(this.GetType()).GetTypes()
                .Where(p => typeof(IFunction).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .Select(Activator.CreateInstance).Cast<IFunction>().ToList();
            _allParameters = new();
            _allParameters.Add("In", Expression.Parameter(typeof(float)));
        }

        public Func<float, float> Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentNullException("expression");
            Expression exp = ParseInternal(expression, 0, expression.Length);
            return Expression
                .Lambda<Func<float, float>>(exp, _allParameters["In"])
                .Compile();
        }

        private Expression ParseInternal(string parseString, int start, int end)
        {
            var operandStack = new Stack<Expression>();
            var operatorStack = new Stack<IOperator>();
            int termsEnd;
            IOperator oper = null;
            TermType termType;
            TermType prevTermType = TermType.None;
            SkipWhiteSpace(parseString, ref start, end);

            while (start != end)
            {

                
                if (start > end)
                    throw new InvalidOperationException("var start great then end");
                if(TryReadTerm(parseString,start,end,out termsEnd, out termType))
                {
                    switch (termType)
                    {
                        case TermType.Group:
                            operandStack.Push(ParseGroup(parseString,ref start, termsEnd));
                        break;
                        case TermType.Constant:
                            operandStack.Push(ParseConstant(parseString,ref start, termsEnd));
                        break;
                        case TermType.BoolConstant:
                            operandStack.Push(ParseBoolConstant(parseString,ref start, termsEnd));
                        break;
                        case TermType.AlphaNumeric:
                        operandStack.Push(ParseAlphaNumeric(parseString,ref start,termsEnd, FindGroupEnd(parseString, termsEnd, end) + 1));
                        break;
                        case TermType.Operator:
                        oper = ParseOperator(parseString, ref start, termsEnd, prevTermType);
                        break;
                        default:
                        start = termsEnd;
                        break;
                    }
                    if(prevTermType == TermType.Operator)
                    {
                        if (operatorStack.Count > 0 && oper.Precedence >= operatorStack.Peek().Precedence)
                            operandStack.Push(CalculateStacks(operandStack, operatorStack, oper.Precedence));
                        operatorStack.Push(oper);
                    }
                    prevTermType = termType;
                    
                }
                else
                {
                    throw new InvalidOperationException("wrong read string");
                }
                    
            }
            return CalculateStacks(operandStack, operatorStack, int.MaxValue);

        }

        #region ReadTerm

        private void SkipWhiteSpace(string parseString, ref int start, int end)
        {
            for (; start < end && Char.IsWhiteSpace(parseString[start]); start++);
        }
        private bool TryReadTerm(string parseString, int start, int end, out int termsEnd, out TermType operandType)
        {
            operandType = TermType.None;
            termsEnd = start;

            if (IsOperatorChar(parseString[start]))
            {
                operandType = TermType.Operator;
                termsEnd = FindOperatorEnd(parseString, start, end);
                return true;
            }
            if (parseString[start] == '(')
            {
                operandType = TermType.Group;
                termsEnd = FindGroupEnd(parseString, start, end);
                return true;
            }
            if (IsAlphaNumericChar(parseString[start]))
            {
                termsEnd = FindAlphaNumericEnd(parseString, start, end);
                string token = parseString.Substring(start, termsEnd-start);
                if (string.Equals(token, "true", StringComparison.OrdinalIgnoreCase))
                {
                    operandType = TermType.BoolConstant;
                    return true;
                }
                if (string.Equals(token, "false", StringComparison.OrdinalIgnoreCase))
                {
                    operandType = TermType.BoolConstant;
                    return true;
                }
                if (token.Any(x => x == '_' || char.IsLetter(x)))
                {
                    operandType = TermType.AlphaNumeric;
                    return true;
                }
                else
                {
                    operandType = TermType.Constant;
                    return true;
                }
            }
            if(parseString[start] == ';')
            {
                operandType = TermType.Separator;
                termsEnd = start+1;
                return true;
            }
            throw new StatExpressionException(string.Format("Char {0} at position {1} is not a valid", parseString[start], start));
        }
        #endregion

        #region ParseOperator
        private bool IsOperatorChar(char c)
        {
            return (_operatorChars.Contains(c));
        }

        private int FindOperatorEnd(string parseString, int start, int end)
        {
            int length = start;
            for (; length < end && IsOperatorChar(parseString[length]); length++) ;

            length -= start;
            string token;
            while (length > 0)
            {
                token = parseString.Substring(start, length);
                if (_allOperators.Exists(x => x.Name == token))
                {
                    return length + start;
                }
                length--;
            }

            throw new StatExpressionException(string.Format("Unrecognized operator  {0} at position {1}", parseString[start], start));
        }

        private IOperator ParseOperator(string parseString, ref int start, int endTerm, TermType prevTermType)
        {
            var term = parseString.Substring(start, endTerm-start);
            var token = _allOperators.FindAll(x => x.Name == term);
            IOperator oper = null;
            if(token.Count == 1)
            {
                oper = token[0];
            }
            else if(token.Count == 2)
            {
                /*
                if (prevTermType == TermType.None || prevTermType == TermType.Operator)
                    oper = token.Find(x => x is IUnaryOperator);
                else
                    oper = token.Find(x => (x is IUnaryOperator) == false);
                */
                oper = token.Find(x => (x is IUnaryOperator) == (prevTermType == TermType.None || prevTermType == TermType.Operator));
            }
            if(oper != null)
            {
                start = endTerm;
                return oper;
            }
                
            throw new StatExpressionException(string.Format("Operator {0} have problem. Find {1} operators in list. prev term is {2}",term,token.Count,prevTermType));


        }

        private Expression CalculateStacks(Stack<Expression> operandStack, Stack<IOperator> operatorStack, int nextOperatorPrecedence)
        {
            if (operandStack.Count == 0)
                return Expression.Constant(0f);

            while (operatorStack.Count > 0 && operatorStack.Peek().Precedence <= nextOperatorPrecedence)
            {
                
                    if (operatorStack.Peek() is IUnaryOperator)
                    {
                        operandStack.Push(((IUnaryOperator)operatorStack.Pop()).Get(operandStack.Pop()));
                    }
                    else if (operatorStack.Peek() is IBinaryOperator)
                    {
                        operandStack.Push(((IBinaryOperator)operatorStack.Pop()).Get(operandStack.Pop(), operandStack.Pop()));
                    }
                    else
                        throw new StatExpressionException(String.Format("Invalid Calculating stacks"));
            }
            return operandStack.Pop();
        }
        #endregion

        #region ParseGroup

        private int FindGroupEnd(string parseString, int start, int end)
        {
            int depth = 0;
            for (int i = start; i < end; i++)
            {
                if (parseString[i] == '(')
                    ++depth;
                else if (parseString[i] == ')')
                {
                    if (--depth == 0)
                    {
                        return i;
                    }

                }
            }
            throw new StatExpressionException("Count opening '(' and closing ')' brackets is different");
        }
        private Expression ParseGroup(string parseString, ref int start,int end)
        {
            var result = ParseInternal(parseString, ++start, end);
            start = end + 1;
            return result;
        }
        private int FindTermEndInGroup(string parseString, int start, int end)
        {
            int depth = 0;
            for (int i = start; i < end; i++)
            {
                if (parseString[i] == '(')
                    ++depth;
                else if (parseString[i] == ')')
                {
                    if (--depth == -1)
                    {
                        return i;
                    }

                }
                if(depth == 0 && parseString[i] == ';')
                    return i;
            }
            throw new StatExpressionException(string.Format("Term on {0} position have wrong format",start));
        }
        #endregion

        #region AlphaNumericParse
        private bool IsAlphaNumericChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c.ToString() == CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                 || c.ToString() == CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
        }
        private int FindAlphaNumericEnd(string parseString, int start, int end)
        {
            int position = start;
            for (; position < end && IsAlphaNumericChar(parseString[position]); position++) ;
            return position;
        }
        #endregion

        #region Constant
        private Expression ParseConstant(string parseString, ref int start, int endsTerm)
        {
            string term = parseString.Substring(start, endsTerm - start);
            float number;
            if (float.TryParse(term, out number))
            {
                start = endsTerm;
                return Expression.Constant(number);

            }
            throw new StatExpressionException(string.Format("Error parsing number \"{0}\" at position {1} ", term, start));
        }
        private Expression ParseBoolConstant(string parseString, ref int start, int endsTerm)
        {
            string token = parseString.Substring(start, endsTerm);
            if (string.Equals(token, "true", StringComparison.OrdinalIgnoreCase))
            {
                start = endsTerm;
                return Expression.Constant(true);
            }
            if (string.Equals(token, "false", StringComparison.OrdinalIgnoreCase))
            {
                start = endsTerm;
                return Expression.Constant(false);
            }
            throw new StatExpressionException(string.Format("Error with {0} term at position {1}", token, start));
        }
        #endregion

        #region VariableOrFunction
        private Expression ParseAlphaNumeric(string parseString, ref int start, int endsTerm,int end)
        {
            string token = parseString.Substring(start, endsTerm - start);
            if (_allParameters.ContainsKey(token))
            {
                start = endsTerm;
                return _allParameters[token];
            }
            if (_allFunctions.Exists(x => x.Name == token))
            {
                var func = _allFunctions.FindAll(x => x.Name == token);
                var operand = new List<Expression>();
                start = endsTerm + 1;
                while (start < end)
                {
                    endsTerm = FindTermEndInGroup(parseString, start, end);
                    operand.Add(ParseInternal(parseString, start, endsTerm));
                    start = endsTerm + 1;
                }
                if (func[0] is IMultiFunction)
                    return ((IMultiFunction)func[0]).Get(operand.ToArray());
                if (func[0] is IUnaryFunction)
                    return ((IUnaryFunction)func[0]).Get(operand[0]);
                if (func[0] is IBinaryFunction)
                    return ((IBinaryFunction)func[0]).Get(operand[0], operand[1]);
                if (func[0] is ITernaryFunction)
                    return ((ITernaryFunction)func[0]).Get(operand[0], operand[1], operand[2]);

                throw new StatExpressionException(string.Format("Function {0} on position {1} is wrong format", token, start));
            }
            throw new StatExpressionException(string.Format("Function or variable with name \"{0}\" not recognized", start));
        }
        #endregion

    }
}