namespace StatExpressionParser
{
    internal interface IFunction
    {
        public int OperandCount { get; }
        public string Name { get; }
    }
}