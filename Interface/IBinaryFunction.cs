using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface IBinaryFunction : IFunction
    {
        Expression Get(Expression firstOperand, Expression secondOperand);
    }
}