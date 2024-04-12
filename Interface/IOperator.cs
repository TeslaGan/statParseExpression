namespace StatExpressionParser
{
    internal interface IOperator
    {
        public int Precedence { get; }
        public string Name { get; }
    }
}