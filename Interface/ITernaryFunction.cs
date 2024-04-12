using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface ITernaryFunction : IFunction
    {
        Expression Get(Expression firstOperand, Expression secondOperand, Expression thirdOperand);
    }
}
