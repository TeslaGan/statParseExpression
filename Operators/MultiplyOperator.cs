using System.Linq.Expressions;

namespace StatExpressionParser
{
    internal sealed class MultiplyOperator: IBinaryOperator 
    {
        public int Precedence => 3;

        public string Name => "*";

        public Expression Get(Expression firstOperand, Expression secondOperand)
        {
            return Expression.Multiply(firstOperand, secondOperand);
        }
    }
}