using System.Linq.Expressions;
namespace StatExpressionParser
{
    internal interface IBinaryOperator:IOperator
    {
        Expression Get(Expression leftOperand, Expression rightOperand);
    }
}