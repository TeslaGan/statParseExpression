using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface ITernaryOperator: IOperator
    {
        Expression Get(Expression firstOperand, Expression secondOperand, Expression thirdOperand);
    }
}