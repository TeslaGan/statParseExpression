using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface IUnaryFunction : IFunction
    {
        Expression Get(Expression operand);
    }
}
