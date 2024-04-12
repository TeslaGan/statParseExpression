using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface IUnaryOperator: IOperator
    {
        Expression Get(Expression operand);
    }
}
