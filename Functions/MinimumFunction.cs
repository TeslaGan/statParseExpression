using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace StatExpressionParser
{
    internal sealed class MinimumFunction : IBinaryFunction
    {
        public int OperandCount => 2;

        public string Name => "Min";

        private static MethodInfo method = typeof(MinimumFunction).GetMethod("Min");

        public Expression Get(Expression firstOperand, Expression secondOperand)
        {
            return Expression.Call(method, firstOperand, secondOperand);
        }

        public static float Min(float arg1,float arg2)
        {
            return Mathf.Min( arg1, arg2);
        }
    }
}

