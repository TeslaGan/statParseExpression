using System.Linq.Expressions;

namespace StatExpressionParser
{
    internal sealed class NegateOperator : IUnaryOperator
    {
        public int Precedence => 2;

        public string Name => "-";

        public Expression Get(Expression operand)
        {
            if(operand is ConstantExpression)
                return Expression.Constant(-(float)((ConstantExpression)operand).Value);
            return Expression.Negate(operand);
        }
    }
}