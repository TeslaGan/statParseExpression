using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace StatExpressionParser
{
    internal sealed class AbsFunction : IUnaryFunction
    {
        public int OperandCount => 1;

        public string Name => "Abs";


        private static MethodInfo method = typeof(AbsFunction).GetMethod("Abs");

        public Expression Get(Expression operand)
        {
            return Expression.Call(method, operand);
        }

        public static float Abs(float arg)
        {
            return Mathf.Abs(arg);
        }
    }
}

