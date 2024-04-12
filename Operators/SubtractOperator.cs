using System.Linq.Expressions;

namespace StatExpressionParser
{
    internal sealed class SubtractOperator : IBinaryOperator
    {
        public int Precedence => 4;

        public string Name => "-";

        public Expression Get(Expression firstOperand, Expression secondOperand)
        {
            return Expression.Subtract(firstOperand, secondOperand);
        }
    }
}
