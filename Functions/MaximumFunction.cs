using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace StatExpressionParser
{
    internal sealed class MaximumFunction : IBinaryFunction
    {
        public int OperandCount => 2;

        public string Name => "Max";

        private static MethodInfo method = typeof(MaximumFunction).GetMethod("Max");

        public Expression Get(Expression firstOperand, Expression secondOperand)
        {
            return Expression.Call(method, firstOperand, secondOperand);
        }

        public static float Max(float arg1, float arg2)
        {
            return Mathf.Max(arg1, arg2);
        }
    }
}
