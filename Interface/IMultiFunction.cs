using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface IMultiFunction : IFunction
    {
        Expression Get(Expression[] operands);
    }
}