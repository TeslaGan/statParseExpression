using System.Linq.Expressions;

namespace StatExpressionParser
{
    internal sealed class AddOperator : IBinaryOperator
    {
        public int Precedence => 4;

        public string Name => "+";

        public Expression Get(Expression firstOperand, Expression secondOperand)
        {
            return Expression.Add(firstOperand,secondOperand);
        }
    }
}